using static IC10_Inliner.IC10Program;
using static IC10_Inliner.Symbol;

namespace IC10_Inliner;

internal class SymbolManager
{
    private readonly List<IList<Symbol>> Scopes = [];
    private readonly List<int> Offsets = [];

    public void AddSymbol(Symbol NewSymbol)
    {
        if (Scopes[^1].Any(x => x.SymbolName == NewSymbol.SymbolName))
            throw new Exception($"Duplicate symbol definition {NewSymbol.SymbolName}");

        // TODO this needs to be a warning, not an error.  That will have to come a bit later.
        if (Scopes[0..^1].Any(x => x.Any(y => y.SymbolName == NewSymbol.SymbolName)))
            throw new Exception($"Symbol definition {NewSymbol.SymbolName} shadows existing symbol");

        Scopes[^1].Add(NewSymbol);
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
                if (Scopes[i][j].SymbolName == SymbolName && (Scopes[i][j].SymbolType != SymbolKind.Label || Scopes[i][j].IsLabelInScope(Line.OriginalCodeLine)))
                    return (Scopes[i][j], Offsets[i]);
            

        throw new Exception($"Unable to resolve symbol {SymbolName}");
    }

    public void InstantiateScope(IList<Symbol> OriginalScope, int ScopeOffset = 0)
    {
        PushScope(OriginalScope, ScopeOffset);
    }

    /// <summary>
    ///     Create a new symbol scope, providing an offset to apply to labels within the scope
    /// </summary>
    /// <param name="ScopeOffset"></param>
    /// <returns></returns>
    public IList<Symbol> CreateScope(int ScopeOffset = 0)
    {
        IList<Symbol> NewScope = [];
        PushScope(NewScope, ScopeOffset);
        return NewScope;
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
