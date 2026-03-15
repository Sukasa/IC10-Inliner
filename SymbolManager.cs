using System.Diagnostics.CodeAnalysis;
using static IC10_Inliner.IC10Program;

namespace IC10_Inliner;

internal class SymbolManager
{
    private readonly List<IList<Symbol>> Scopes = [];
    private readonly List<int> Offsets = [];

    public void AddSymbol(Symbol NewSymbol)
    {
        // TODO Check for collisions

        Scopes[^1].Add(NewSymbol);
    }

    public double Resolve(string Token, ProgramLine Line, out Symbol.SymbolKind SymbolKind)
    {
        SymbolKind = Symbol.SymbolKind.Constant;
        
        if (double.TryParse(Token, out var Result))
            return Result;
        
        var ResultValueString = Token;
        
        if (TryGetSymbol(Token, Line, out var FoundSymbol))
        {
            ResultValueString = FoundSymbol.Value.Symbol.Resolve();
            SymbolKind = FoundSymbol.Value.Symbol.SymbolType;
        }
        
        if (Symbol.TryParseBinary(ResultValueString, out var Value) || Symbol.TryParseHex(ResultValueString, out Value))
            return Value;
        
        if (ResultValueString.StartsWith("hash", StringComparison.OrdinalIgnoreCase))
            ResultValueString = IC10Assembler.ComputeHash(ResultValueString[6..^2]).ToString();
        
        else if (ResultValueString.StartsWith("str", StringComparison.OrdinalIgnoreCase))
            return IC10Assembler.ComputeString(ResultValueString[5..^2]);
        
        else if (ResultValueString.StartsWith("calc", StringComparison.OrdinalIgnoreCase))
                return Calculation.Calculate(ResultValueString[5..^1], Line.Scope);

        return double.Parse(ResultValueString);
    }
    
    /// <summary>
    ///     Get a symbol from the store, taking into account current scope and program location
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="Line"></param>
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
    ///     try to get a symbol from the store, taking into account current scope and program location
    /// </summary>
    /// <param name="SymbolName"></param>
    /// <param name="Line"></param>
    /// <param name="Result"></param>
    /// <returns></returns>
    public bool TryGetSymbol(string SymbolName, ProgramLine Line, [NotNullWhen(true)] out (Symbol Symbol, int Offset)? Result)
    {
        for (int i = Scopes.Count - 1; i >= 0; i--)
        for (int j = 0; j < Scopes[i].Count; j++)
            if (Scopes[i][j].IsLabelInScope(Line.OriginalCodeLine))
            {
                Result = (Scopes[i][j], Offsets[i]);
                return true;
            }
        
        Result = null;
        return false;
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
