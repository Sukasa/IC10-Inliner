namespace IC10_Inliner;

public struct AssemblyResult
{
    public readonly List<string> Warnings = [];
    public readonly List<string> Errors = [];
    public readonly List<string> OutputLines = [];
    public readonly List<Symbol> Symbols = [];

    public List<IC10Program.ProgramSection> FinalSections = [];

    public readonly string Output => OutputLines.Count != 0
        ? OutputLines.Aggregate((x, y) => x + Environment.NewLine + y)
        : "";

    public readonly bool Valid => Errors.Count == 0;

    internal AssemblyResult(ParseResult ParseFaults)
    {
        Warnings.AddRange(ParseFaults.Warnings);
        Errors.AddRange(ParseFaults.Errors);

        Symbols.AddRange(ParseFaults.Program.Sections.SelectMany(x => x.Symbols.Symbols)
            .Where(x => x.SymbolType == Symbol.SymbolKind.Label));
    }
}