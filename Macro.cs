
namespace IC10_Inliner;

public class Macro
{
    public string Name { get; private set; }
    public int SourceLine { get; private set; }
    public List<string> ParamNames { get; private set; }
    private List<string> BodyLines = new();

    public Macro(string Name, IEnumerable<string> Params, int sourceLine)
    {
        this.Name = Name;
        ParamNames = Params.ToList();
        SourceLine = sourceLine;
    }

    public void Add(string Line)
    {
        BodyLines.Add(Line);
    }

    public IEnumerable<string> Invoke(List<string> Params)
    {
        return BodyLines.Select(ln =>
        {
            for (var i = 0; i < ParamNames.Count; i++)
            {
                //TODO: whitespace-tokenize the line to prevent clbuttic substitution
                ln = ln.Replace(ParamNames[i], Params[i]);
            }
            return ln;
        });
    }
}
