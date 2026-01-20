using static IC10_Inliner.IC10Program;

namespace IC10_Inliner
{
    public record SymbolScope
    {
        public IList<Symbol> Symbols { get; init; } = [];

        required public SymbolScope? ParentScope { get; init; }

        required public ProgramSection? Section { get; init; }
        required public int CodeOffset { get; set; } // This can be modified later, as assembly is when section offsets are calculated

        public int GetLabelOffset() => Section?.Offset ?? 0;
    }
}
