namespace IC10_Inliner;

public partial class IC10Program
{
    public List<ProgramSection> Sections { get; } = [];
    public Dictionary<string, Macro> Macros { get; } = [];

    public HashSet<string> Symbols { get; } = [];
    public HashSet<string> Aliases { get; } = [];
}