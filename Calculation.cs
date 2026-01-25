using System.Collections;
using System.Text.RegularExpressions;

namespace IC10_Inliner;

public partial class Calculation : IEnumerable<(string, int)>
{
    // Calculation function.  Given a string, this class tokenizes it, performs symbol substitutions, runs math, and determines a constant result value
    private static readonly Calculation _instance = new Calculation();

    private string Working = "";
    
    private Calculation()
    {
    }

    public static double Calculate(string Input, IC10Program.ProgramLine Context) => _instance.DoCalculate(Input, Context);

    private double DoCalculate(string Input, IC10Program.ProgramLine Context)
    {
        Working = Input;

        var OpCode = "";
        double OpValue1 = 0;
        double? OpValue2 = null;
        
        foreach (var (Token, TokenCode) in this)
        {
            switch (TokenCode)
            {
                case 0:
                    if (OpValue2 is not null)
                        throw new Exception("Invalid Parameter Order");
                    OpValue2 = double.Parse(ScopeManager.GetSymbol(Token, Context).Resolve());
                    break;
                case 1:
                    if (OpValue2 is not null)
                        throw new Exception("Invalid Parameter Order");
                    OpValue2 = double.Parse(Token);
                    break;
                case 2:
                    if (OpCode != "")
                        throw new Exception("Invalid Operator Order");
                    OpCode = Token;
                    continue;
                default:
                    throw new Exception($"Invalid Token {Token}");
            }

            if (OpCode != "")
            {
                var OpValue = OpValue2.Value;
                switch (OpCode)
                {
                    case "+":
                        OpValue1 += OpValue;
                        break;
                    
                    case "-":
                        OpValue1 -= OpValue;
                        break;
                    
                    case "*":
                        OpValue1 *= OpValue;
                        break;
                    
                    case "/":
                        OpValue1 /= OpValue;
                        break;
                    
                    case "%":
                        OpValue1 %= OpValue;
                        break;
                    
                    case "==":
                        OpValue1 = Math.Abs(OpValue1 - OpValue) < 0.001 ? 1 : 0;
                        break;
                    
                    case "<=":
                        OpValue1 = OpValue1 <= OpValue ? 1 : 0;
                        break;
                    
                    case ">=":
                        OpValue1 = OpValue1 >= OpValue ? 1 : 0;
                        break;
                    
                    case "<":
                        OpValue1 = OpValue1 < OpValue ? 1 : 0;
                        break;
                    
                    case ">":
                        OpValue1 = OpValue1 > OpValue ? 1 : 0;
                        break;
                    
                    case "!=": 
                        OpValue1 = Math.Abs(OpValue1 - OpValue) > 0.001 ? 1 : 0;
                        break;
                    
                    default:
                        throw new Exception($"Unknown Operator {OpCode}");
                }

                OpValue2 = null;
                OpCode = "";
            }
            else
            {
                OpValue1 = OpValue2.Value;
                OpValue2 = null;
            }
        }
        
        return OpValue1;
    }

    private static readonly List<Regex> TokenClasses = [IdentifierRegex(), NumericValueRegex(), OperatorRegex()];

    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex IdentifierRegex();

    [GeneratedRegex("""^(?:(0x|\$)[0-9a-fA-F]*|[\d.]+)$""")]
    private static partial Regex NumericValueRegex();

    [GeneratedRegex("""^(?:[-+/*%]|[<>!=]=?)$""")]
    private static partial Regex OperatorRegex();

    public IEnumerator<(string, int)> GetEnumerator()
    {
        var strPos = 0;
        var strLen = 0;
        
        while (strPos < Working.Length)
        {
            strLen = 0;
            
            while (strPos + strLen < Working.Length)
            {
                strLen++;
                if (TokenClasses.Any(x => x.IsMatch(Working[strPos..(strPos + strLen)])))
                    continue;
                
                strLen--;
                break;
            }
            yield return (Working[strPos..(strPos + strLen)], TokenClasses.FindIndex(x => x.IsMatch(Working[strPos..(strPos + strLen)])));

            strPos += strLen;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}