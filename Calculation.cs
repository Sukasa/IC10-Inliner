using System.Collections;
using System.Text.RegularExpressions;

namespace IC10_Inliner;

public partial class Calculation : IEnumerable<string>
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
        
        
        
        return 0;
    }

    private static List<Regex> TokenClasses = [IdentifierRegex(), NumericValueRegex(), OperatorRegex()];

    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*")]
    private static partial Regex IdentifierRegex();

    [GeneratedRegex("""(?:(0x|\$)[0-9a-fA-F]*|[\d.]+)""")]
    private static partial Regex NumericValueRegex();

    [GeneratedRegex("""(?:[-+/*%]|[<>!=]=?)""")]
    private static partial Regex OperatorRegex();

    public IEnumerator<string> GetEnumerator()
    {
        var strPos = 0;
        while (strPos < Working.Length)
        {
            yield return "";
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}