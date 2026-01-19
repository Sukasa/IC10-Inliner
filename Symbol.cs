using System.Globalization;

namespace IC10_Inliner;

public record Symbol
{
    public SymbolKind SymbolType { get; init; }

    public SymbolScope Scope { get; init; }

    public int OrignalCodeLine { get; init; }

    public string EnumValue { get; init; }

    public double? Value { get; init; }

    public string SymbolName { get; init; }

    public IC10Program.ProgramSection Section { get; init; } // Section this symbol is a part of (for labels)

    public bool IsValidConstant => Value is not null || SymbolType == SymbolKind.Label || IC10Assembler.Macro().IsMatch(EnumValue);

    public bool IsLabelInScope(int FromOriginalCodeLine) => SymbolType == SymbolKind.Label && ((Scope == SymbolScope.Forward && OrignalCodeLine > FromOriginalCodeLine) || (Scope == SymbolScope.Backward && OrignalCodeLine <= FromOriginalCodeLine));

    public string Resolve(IC10Program.ProgramSection FromSection)
    {
        return SymbolType switch
        {
            SymbolKind.Label => ((Value ?? 0.0) + FromSection.Offset).ToString(),
            _ => Value?.ToString() ?? EnumValue,
        };
    }

    public static bool TryParseBinary(string input, out ulong Parsed)
    {
        if (input.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
        {
            return ulong.TryParse(input[2..], NumberStyles.BinaryNumber, CultureInfo.InvariantCulture, out Parsed);
        }
        Parsed = 0;
        return false;
    }

    public static bool TryParseHex(string input, out ulong Parsed)
    {
        if (input.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            return ulong.TryParse(input[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Parsed);
        }
        else if (input.StartsWith('$'))
        {
            return ulong.TryParse(input[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Parsed);
        }
        Parsed = 0;
        return false;
    }

    public Symbol(IC10Program.ProgramSection CurrentSection, string Name, string TextValue, SymbolKind Type)
    {
        Section = CurrentSection;
        SymbolName = Name;
        SymbolType = Type;

        switch (Type)
        {
            case SymbolKind.Constant:
                if (TryParseHex(TextValue, out var ValueInt) || TryParseBinary(TextValue, out ValueInt))
                    Value = ValueInt;
                else if (double.TryParse(TextValue, out var NewValue))
                    Value = NewValue;
                EnumValue = TextValue;

                break;
            case SymbolKind.Label:
                Value = CurrentSection.Size;
                EnumValue = Name;
                break;
            default:
                EnumValue = TextValue;
                Value = null;
                break;
        }
    }

    public enum SymbolScope
    {
        /// <summary>
        ///     Program-level scope, for normal labels
        /// </summary>
        Program,

        /// <summary>
        ///     Macro-level scope, for any labels defined within a macro
        /// </summary>
        Macro,

        /// <summary>
        ///     Forward-jump labels (+, ++, etc)
        /// </summary>
        Forward,

        /// <summary>
        ///     Backward-jump labels (-, --, etc)
        /// </summary>
        Backward
    }

    public enum SymbolKind
    {
        Constant, // Constant symbol, i.e. number or LogicType enum, or STR("")/HASH("") construct
        Label, // Specifically for labels
    }
}