using CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using static IC10_Inliner.IC10Instruction;
using static IC10_Inliner.IC10Program;

namespace IC10_Inliner
{
    public static partial class IC10Assembler
    {
        const string DefaultSection = "(default)";

        public static ParseResult Parse(string input)
        {
            IC10Program Program = new();
            ParseResult Result = new(Program);
            ProgramSection CurrentSection = new(DefaultSection);

            string[] Lines = input.Split(['\n'], StringSplitOptions.TrimEntries);
            int SourceLine = -1;
            int CodeLineIndex = 0;
            int SectionLineIndex = 0;

            void Warning(string Message)
            {
                Result.Warnings.Add(string.Format("{0} at line {1}", Message, SourceLine));
            }

            void Error(string Message)
            {
                Result.Errors.Add(string.Format("{0} at line {1}", Message, SourceLine));
            }

            void AddSymbol(Symbol NewSymbol)
            {
                if (Program.Symbols.Contains(NewSymbol.SymbolName))
                {
                    Error($"Duplicate Symbol {NewSymbol.SymbolName}");
                    return;
                }

                CurrentSection.Symbols[NewSymbol.SymbolName] = NewSymbol;
                Program.Symbols.Add(NewSymbol.SymbolName);
            }

            void Elide(string Opcode, params string[]? Parameters)
            {
                var NewLine = new ProgramLine()
                {
                    OpCode = Opcode,
                    Params = Parameters?.ToList() ?? [],
                    OriginalCodeLine = SourceLine,
                    SectionOffset = SectionLineIndex
                };

                if (!string.IsNullOrEmpty(Opcode))
                {
                    CurrentSection.Lines.Add(NewLine);
                    CodeLineIndex++;
                    SectionLineIndex++;
                }
            }

            foreach (var Line in Lines)
            {
                SourceLine++;

                if (string.IsNullOrWhiteSpace(Line))
                    continue;

                var Parsed = LineFormat().Match(Line);
                if (!Parsed.Success)
                {
                    Error("Unrecognized formatting or syntax error");
                    continue;
                }

                string Directive = Parsed.Groups["Directive"].Value.ToLower();
                switch (Directive)
                {
                    case "alias":
                    case "define":
                        if (!Parsed.Groups["Params"].Success || Parsed.Groups["Params"].Captures.Count != 2)
                        {
                            Error($"Incorrect parameter count for {Parsed.Groups["Directive"].Value} directive");
                            continue;
                        }

                        var Param1 = Parsed.Groups["Params"].Captures[0].Value;
                        var Param2 = Parsed.Groups["Params"].Captures[1].Value;

                        if (Directive == "define")
                        {
                            AddSymbol(new Symbol(CurrentSection, Param1, Param2, Symbol.SymbolKind.Constant));
                            continue;
                        }

                        // Aliases should point to device pins or registers
                        if (CurrentSection.Aliases.ContainsKey(Param1) || Program.Aliases.Contains(Param1))
                            if (DevicePin().IsMatch(Param2)) // If we're referencing a device pin, it can be just a warning (and may actually be intended behaviour due to how device aliases work)
                                Warning($"Duplicate direct device pin alias {Param1}");
                            else
                                Error($"Duplicate alias {Param2}");
                        else
                            Program.Aliases.Add(Param1);

                        if (DevicePin().IsMatch(Param2))
                            Elide("alias", Param1, Param2);

                        else if (!Register().IsMatch(Param2))
                            Warning($"Possible invalid alias target {Param2}");

                        CurrentSection.Aliases[Param1] = Param2;

                        continue;

                    case "section":
                        if (!Parsed.Groups["Params"].Success)
                        {
                            Error($"Missing section name for section directive");
                        }
                        else
                        {
                            string NewSectionName = Parsed.Groups["Params"].Captures[0].Value;
                            if (!CurrentSection.IsEmpty)
                            {
                                Program.Sections.Add(CurrentSection);
                                if (!Result.SectionNames.Contains(CurrentSection.Name))
                                    Result.SectionNames.Add(CurrentSection.Name);
                            }

                            List<string> RequiredSections = [];

                            if (Parsed.Groups["Params"].Captures.Count > 1)
                            {
                                if (Parsed.Groups["Params"].Captures.Count == 2)
                                {
                                    Error("Invalid parameter count for section directive");
                                }
                                else if (!Parsed.Groups["Params"].Captures[1].Value.Equals("requires", StringComparison.OrdinalIgnoreCase))
                                {
                                    Error("Invalid section definition");
                                }
                                else
                                {
                                    RequiredSections = [.. Parsed.Groups["Params"].Captures.Skip(2).Select(x => x.Value)];
                                    var MissingSection = RequiredSections.FirstOrDefault(x => !Result.SectionNames.Contains(x, StringComparer.OrdinalIgnoreCase));
                                    if (MissingSection is not null)
                                        Error($"Missing section prerequisite {MissingSection}");
                                }
                            }

                            CurrentSection = new(NewSectionName, RequiredSections);
                            SectionLineIndex = 0;
                        }
                        continue;
                }

                if (Parsed.Groups["Label"].Success)
                    AddSymbol(new Symbol(CurrentSection, Parsed.Groups["Label"].Value, SectionLineIndex.ToString(), Symbol.SymbolKind.Label));

                // Otherwise, this is a line of code (label, opcode)
                Elide(Parsed.Groups["Opcode"].Value, [.. Parsed.Groups["Params"].Captures.Select(x => x.Value)]);
            }

            Program.Sections.Add(CurrentSection);
            if (!Result.SectionNames.Contains(CurrentSection.Name))
                Result.SectionNames.Add(CurrentSection.Name);

            return Result;
        }

        public static AssemblyResult Assemble(ParseResult ParseResult, AssemblyOptions Options)
        {
            AssemblyResult Result = new(ParseResult);

            // Refuse to assemble if it failed to parse
            if (!ParseResult.Valid)
            {
                Result.Errors.Add("Unable to assemble due to parse errors");
                return Result;
            }

            var SectionNames = ParseResult.Program.Sections.Select(x => x.Name);
            if (Options.IncludeSections is not null && Options.IncludeSections.Any())
                SectionNames = ParseResult.Program.Sections.Where(x => Options.IncludeSections.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).SelectMany(x => x.RequiredSections).Union(Options.IncludeSections).Distinct();

            List<ProgramSection> Sections = [.. ParseResult.Program.Sections.Where(x => SectionNames.ToList().Contains(x.Name, StringComparer.OrdinalIgnoreCase))];

            int SourceLine = 0;
            int SectionIdx = 0;

            void Warning(string Message)
            {
                Result.Warnings.Add(string.Format("{0} at line {1}", Message, SourceLine));
            }

            void Error(string Message)
            {
                Result.Errors.Add(string.Format("{0} at line {1}", Message, SourceLine));
            }

            string ResolveAlias(string Alias)
            {
                // Aliases can only be used for stuff defined in or before their section (and whose section is included in assembly), for sanity
                for (int i = 0; i < SectionIdx; i++)
                    if (Sections[i].Aliases.TryGetValue(Alias, out string? value))
                        return value;

                return Alias;
            }

            Symbol? ResolveSymbol(string Symbol, bool IgnoreFailedResolve)
            {
                // Symbols also only limited to pre-defined stuff
                for (int i = 0; i < SectionIdx; i++)
                    if (Sections[i].Symbols.TryGetValue(Symbol, out Symbol? value))
                        return value;

                if (ParseResult.Program.Symbols.Contains(Symbol))
                {
                    if (Sections.Any(x => x.Symbols.ContainsKey(Symbol)))
                        Error($"Use before define of symbol {Symbol}");
                    else
                        Error($"{Symbol} not defined in included section");
                }

                if (!IgnoreFailedResolve)
                    Error($"Unable to resolve symbol {Symbol}");

                return null;
            }
            int Offset = 0;
            foreach (var Section in Sections)
            {
                Section.Offset = Offset;
                Offset += Section.Size;

                SectionIdx++;
                foreach (var ProgramLine in Section.Lines)
                {
                    SourceLine = ProgramLine.OriginalCodeLine;

                    if (ProgramLine.OpCode is null)
                        continue;

                    // We'll build up a string representing the output line
                    string Line = ProgramLine.OpCode;

                    // Now, we pull the instruction being entered (if we can), and validate the parameters
                    IC10Instruction Instruction = Instructions.FirstOrDefault(x => x.Mnemonic.Equals(ProgramLine.OpCode, StringComparison.OrdinalIgnoreCase));

                    // Some sanity + error checks
                    if (string.IsNullOrEmpty(Instruction.Mnemonic))
                    {
                        Error($"Unrecognized mnemonic {ProgramLine.OpCode}");
                        continue;
                    }

                    if (Instruction.Parameters.Length != ProgramLine.Params.Count)
                    {
                        Error($"Invalid number of parameters for mnemonic {Instruction.Mnemonic}: expected {Instruction.Parameters.Length}, got {ProgramLine.Params.Count}");
                        continue;
                    }

                    // Now for each parameter we verify that it's a valid parameter (or at least appears to be) for that instruction at that position
                    foreach (var (ParamOrig, ParamMeta) in ProgramLine.Params.Zip(Instruction.Parameters))
                    {
                        var ParamString = ParamOrig;
                        var NoSub = (ParamMeta & ParameterType.NoSubstitution) == ParameterType.NoSubstitution;
                        var AllowUnknown = (ParamMeta & ParameterType.AllowUnknownSymbol) == ParameterType.AllowUnknownSymbol;

                        if (!NoSub)
                            ParamString = ResolveAlias(ParamString);

                        ParameterType ProvidedType = ParameterType.IsUnknownSymbol;

                        // Since I'm not smarter earlier on, instead we try to discern what the parameter is through various examinations
                        if (double.TryParse(ParamString, out _) || Macro().IsMatch(ParamString))
                            ProvidedType = ParameterType.IsConstant;
                        else if (DevicePin().IsMatch(ParamString))
                            ProvidedType = ParameterType.IsDevice;
                        else if (Register().IsMatch(ParamString))
                            ProvidedType = ParameterType.IsRegister;
                        else
                        {
                            if (Symbol.TryParseHex(ParamString, out ulong Value))
                            {
                                ProvidedType = ParameterType.IsConstant;
                                ParamString = Value.ToString();
                            }
                            else
                            {
                                // Symbols are harder to figure out, since we have to resolve them then handle both failed and succeeded resolves.
                                // Also we don't have an exhaustive list of valid constants so I have to be a little sloppy in spots
                                Symbol? symbol = ResolveSymbol(ParamString, AllowUnknown);
                                if (symbol is null)
                                    ProvidedType = ParameterType.IsUnknownSymbol;
                                else if (!NoSub) // If the symbol is valid, only resolve it if this instructions allows (e.g. don't do so for alias lines)
                                {
                                    ParamString = symbol.Resolve(Section);
                                    ProvidedType = symbol.SymbolType switch
                                    {
                                        Symbol.SymbolKind.Constant => ParameterType.IsConstant,
                                        Symbol.SymbolKind.Label => ParameterType.IsConstant,
                                        _ => ParameterType.IsUnknownSymbol
                                    };
                                }
                            }
                        }

                        // If our discerned type doesn't match the expected type (with an abort for enum-friendly types), we lodge an error
                        if (((ParamMeta & ProvidedType) != ProvidedType) && ProvidedType != ParameterType.IsUnknownSymbol && !AllowUnknown)
                            Error("Parameter type mismatch");

                        // If this is a HASH() or STR() macro, then we preprocess it
                        if (!Options.ElideMacros && Macro().IsMatch(ParamString))
                        {
                            if (ParamString.StartsWith("hash", StringComparison.OrdinalIgnoreCase))
                                ParamString = ComputeHash(ParamString[6..^2]).ToString();
                            else if (ParamString.StartsWith("str", StringComparison.OrdinalIgnoreCase))
                                ParamString = ComputeString(ParamString[5..^2]).ToString();
                        }

                        // Lastly, if this is a branch relative instruction, then if we can relativize the value we do so
                        if ((ParamMeta & ParameterType.BranchRelative) != 0)
                        {
                            if (int.TryParse(ParamString, out int LabelAddress))
                                ParamString = (LabelAddress - (ProgramLine.SectionOffset + Section.Offset)).ToString();
                            else if (!Register().IsMatch(ParamString)) // Otherwise if it's not a register symbol something went wrong
                                Error($"Invalid destination {ParamString} for relative branch");
                        }

                        // Append it to the line.
                        // We only do this a few times with short strings, a StringBuilder would be too heavy
                        Line = $"{Line} {ParamString}";
                    }

                    Result.OutputLines.Add(Line);

                    // Throw up warnings for large programs
                    if (Result.OutputLines.Count == 129)
                        Warning("Exceeded vanilla IC10 LoC cap");

                    if (Result.OutputLines.Count == 512)
                        Warning("Exceeded modded More Lines of Code LoC cap");
                }
            }
            Result.FinalSections = Sections;

            return Result;
        }

        internal static uint ComputeHash(string Input)
        {
            var Bytes = Encoding.ASCII.GetBytes(Input);
            return System.IO.Hashing.Crc32.HashToUInt32(Bytes);
        }

        internal static ulong ComputeString(string Input)
        {
            ulong output = 0;
            Input = Input[..Math.Min(6, Input.Length)];
            var stringBytes = Encoding.ASCII.GetBytes(Input);
            for (int i = 0; i < stringBytes.Length; i++)
            {
                output <<= 8;
                output |= stringBytes[i];
            }

            return output;
        }

        [GeneratedRegex(@"^\s*(?:(?:(?<Directive>alias|section|define)|(?:(?<Label>[a-zA-Z_][a-zA-Z0-9_]*):\s*)?(?:(?<Opcode>[a-zA-Z]+))?)(?:[^\S\r\n]+(?<Params>(?:0x|\$)?[a-zA-Z0-9_\-\.:]+|(?:[hH][aA][sS][hH]|[sS][tT][rR])\(\"".*\""\)))*)(?:\s*[#;]\s*(?<Comment>.*))?\\?$")]
        internal static partial Regex LineFormat();

        [GeneratedRegex("^(?:sp|r+(?:[0-9a]|1[0-5]))$")]
        internal static partial Regex Register();


        [GeneratedRegex(@"^(?:[hH][aA][sS][hH]|[sS][tT][rR])\(\"".*\""\)$")]
        internal static partial Regex Macro();

        [GeneratedRegex(@"^d(?:b|[0-5]|r+(?:[0-9a]|1[0-5]))(?::\d)?$")]
        internal static partial Regex DevicePin();

        public struct ParseResult
        {
            public List<string> Warnings = [];
            public List<string> Errors = [];
            public List<string> SectionNames = [];

            public readonly IC10Program Program;

            public readonly bool Valid => Errors.Count == 0;

            internal ParseResult(IC10Program Program)
            {
                this.Program = Program;
            }
        }

        public class AssemblyOptions
        {
            [Value(0, Required = true)]
            public string Filename { get; set; } = "";

            [Option('c', "comments", Default = false)]
            public bool IncludeComments { get; set; } = false;

            [Option('s', "sections")]
            public IEnumerable<string> IncludeSections { get; set; } = [];

            [Option('m', "keep-macros", Default = false)]
            public bool ElideMacros { get; set; } = false;

        }

        public struct AssemblyResult
        {
            public List<string> Warnings = [];
            public List<string> Errors = [];
            public List<string> OutputLines = [];

            public List<ProgramSection> FinalSections = [];

            public readonly string Output => OutputLines.Any() ? OutputLines.Aggregate((x, y) => x + Environment.NewLine + y) : "";
            public readonly bool Valid => Errors.Count == 0;

            internal AssemblyResult(ParseResult ParseFaults)
            {
                Warnings.AddRange(ParseFaults.Warnings);
                Errors.AddRange(ParseFaults.Errors);
            }
        }
    }
}
