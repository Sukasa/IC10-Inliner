namespace IC10_Inliner;

public partial class IC10Program
{
    public readonly struct ProgramLine()
    {
        public string? OpCode { get; init; } = string.Empty;

        public string Directive { get; init; } = string.Empty;

        public List<string> Params { get; init; } = [];

        public string Comment { get; init; } = string.Empty;

        public int OriginalCodeLine { get; init; } = 0;

        public int SectionOffset { get; init; } = 0;
    }
}