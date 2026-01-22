using static IC10_Inliner.IC10Program;
using static IC10_Inliner.Symbol;

namespace IC10_Inliner;

internal class ScopeManager
{
    private static readonly List<SymbolScope> Scopes = [ new SymbolScope() { ParentScope = null, CodeOffset = 0, Section = null } ];

    public static void AddSymbol(Symbol NewSymbol)
    {
        if (Scopes[^1].Symbols.Any(x => x.SymbolName == NewSymbol.SymbolName))
            throw new Exception($"Duplicate symbol definition {NewSymbol.SymbolName}");

        // TODO this needs to be a warning, not an error.  That will have to come a bit later.
        if (Scopes[0..^1].Any(x => x.Symbols.Any(y => y.SymbolName == NewSymbol.SymbolName)))
            throw new Exception($"Symbol definition {NewSymbol.SymbolName} shadows existing symbol");

        Scopes[^1].Symbols.Add(NewSymbol);
    }

    /// <summary>
    ///     Get a symbol from the specified scope
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="ProgramLine"></param>
    /// <returns></returns>
    public static Symbol GetSymbol(string SymbolName, SymbolScope InScope)
    {
        SymbolScope? Scope = InScope;
        while (Scope != null)
        {
            for (int j = 0; j < Scope.Symbols.Count; j++)
                if (Scope.Symbols[j].SymbolName == SymbolName)
                    return Scope.Symbols[j];

            Scope = Scope.ParentScope;
        }

        throw new Exception($"Unable to resolve symbol {SymbolName}");
    }

    /// <summary>
    ///     Get a symbol from the store, taking into account current scope and program location
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="ProgramLine"></param>
    /// <returns></returns>
    public static Symbol GetSymbol(string SymbolName, ProgramLine Line)
    {
        SymbolScope? Scope = Line.Scope;
        while (Scope != null)
        {
            for (int j = 0; j < Scope.Symbols.Count; j++)
                if (Scope.Symbols[j].SymbolName == SymbolName && (Scope.Symbols[j].SymbolType != SymbolKind.Label || Scope.Symbols[j].IsLabelInScope(Line.OriginalCodeLine)))
                    return Scope.Symbols[j];

            Scope = Scope.ParentScope;
        }

        throw new Exception($"Unable to resolve symbol {SymbolName}");
    }

    public static void InstantiateScope(SymbolScope OriginalScope, ProgramSection InSection, int ScopeOffset = 0)
    {
        Scopes.Add(new SymbolScope() { Symbols = [.. OriginalScope.Symbols], ParentScope = OriginalScope.ParentScope, CodeOffset = ScopeOffset, Section = InSection });
    }

    /// <summary>
    ///     Create a new symbol scope, providing an offset to apply to labels within the scope
    /// </summary>
    /// <param name="ScopeOffset"></param>
    /// <returns></returns>
    public static SymbolScope CreateScope(ProgramSection? InSection)
    {
        SymbolScope NewScope = new() { ParentScope = Scopes[^1], CodeOffset = 0, Section = InSection };
        Scopes.Add(NewScope);
        return NewScope;
    
    }

    public static SymbolScope PeekScope() => Scopes[^1];

    public static void PopScope()
    {
        Scopes.RemoveAt(Scopes.Count - 1);
    }
}
