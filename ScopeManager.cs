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
    ///     Get a symbol from the store, taking into account current scope and program location
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="ProgramLine"></param>
    /// <returns></returns>
    public static Symbol GetSymbol(string SymbolName, ProgramLine Line)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
            for (int j = 0; j < Scopes[i].Symbols.Count; j++)
                if (Scopes[i].Symbols[j].SymbolName == SymbolName && (Scopes[i].Symbols[j].SymbolType != SymbolKind.Label || Scopes[i].Symbols[j].IsLabelInScope(Line.OriginalCodeLine)))
                {
                    Symbol S = Scopes[i].Symbols[j];
                    S.LineOffset = Line.SectionOffset;
                    return S;
                }
            

        throw new Exception($"Unable to resolve symbol {SymbolName}");
    }

    public static void InstantiateScope(SymbolScope OriginalScope, ProgramSection InSection, int ScopeOffset = 0)
    {
        Scopes.Add(new SymbolScope() { Symbols = OriginalScope.Symbols, ParentScope = OriginalScope.ParentScope, CodeOffset = ScopeOffset, Section = InSection });
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
