namespace IC10_Inliner;

public partial class IC10Program
{
    public class ProgramSection(string SectionName, List<string>? ReqSections = null)
    {
        public Dictionary<string, Symbol> Symbols { get; } = [];
        public Dictionary<string, string> Aliases { get; } = [];
        public List<ProgramLine> Lines { get; set; } = [];
        public int Offset { get; set; }
        public int Size => Lines.Count;
        public string Name { get; init; } = SectionName;
        public List<string> RequiredSections { get; init; } = ReqSections ?? [];

        public bool IsEmpty => Size == 0 && Aliases.Count == 0 && Symbols.Count == 0;
    }
}