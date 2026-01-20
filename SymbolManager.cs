using static IC10_Inliner.IC10Program;

namespace IC10_Inliner;

internal class SymbolManager
{
    private List<IList<Symbol>> Scopes = new();
    private List<int> Offsets = new();

    public void AddSymbol(Symbol NewSymbol)
    {
        // TODO Check for collisions

        Scopes[Scopes.Count - 1].Add(NewSymbol);
    }

    /// <summary>
    ///     Get a symbol from the store, taking into account current scope and program location
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="ProgramLine"></param>
    /// <returns></returns>
    public (Symbol Symbol, int Offset) GetSymbol(string SymbolName, ProgramLine Line)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
            for (int j = 0; j < Scopes[i].Count; j++)
                if (Scopes[i][j].IsLabelInScope(Line.OriginalCodeLine))
                    return (Scopes[i][j], Offsets[i]);

        throw new Exception($"Unable to resolve symbol {SymbolName}");
    }

    /// <summary>
    ///     Push a scope onto the stack, for section or macro scopes
    /// </summary>
    /// <param name="SymbolScope"></param>
    public void PushScope(IList<Symbol> SymbolScope, int ScopeOffset = 0)
    {
        Scopes.Add(SymbolScope);
        Offsets.Add(ScopeOffset);
    }

    public void PopScope()
    {
        Scopes.RemoveAt(Scopes.Count - 1);
        Offsets.RemoveAt(Offsets.Count - 1);
    }
}
