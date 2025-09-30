using CommandLine;
using static IC10_Inliner.IC10Assembler;


var result = Parser.Default.ParseArguments<AssemblyOptions>(args);

if (result.Errors.Count() == 0 && args.Length > 0)
{
    bool wait = false;
    var Options = result.Value;

    var ParseResult = Parse(File.ReadAllText(Options.Filename));
    wait |= ParseResult.Warnings.Count > 0;

    if (ParseResult.Valid)
    {
        var AssemblyResult = Assemble(ParseResult, Options);

        wait |= AssemblyResult.Warnings.Count > 0;

        if (AssemblyResult.Valid)
        {
            string ShortName = Path.GetFileName(Options.Filename);
            Console.WriteLine($"Assembled {ShortName} => {ShortName[..^4]}.min{ShortName[^4..]}");
            Console.WriteLine($"{AssemblyResult.FinalSections.Count} sections totalling {AssemblyResult.OutputLines.Count} line{(AssemblyResult.OutputLines.Count != 1 ? "s" : "")}");
            File.WriteAllText(Options.Filename[..^4] + ".min" + Options.Filename[^4..], AssemblyResult.Output);
        }
        else
        {
            wait = true;
            Console.WriteLine($"Failed to assemble {Options.Filename}");
            foreach (var warning in ParseResult.Warnings)
                Console.WriteLine($"Warning: {warning}");
            foreach (var warning in AssemblyResult.Warnings)
                Console.WriteLine($"Warning: {warning}");
            foreach (var error in AssemblyResult.Errors)
                Console.WriteLine($"Error: {error}");
        }
    }
    else
    {
        wait = true;
        Console.WriteLine($"Failed to parse file {Options.Filename}");
        foreach (var error in ParseResult.Errors)
            Console.WriteLine($"Error: {error}");
    }
    foreach (var warning in ParseResult.Warnings)
        Console.WriteLine($"Warning: {warning}");

    if (wait)
    {
        Console.Write("Press Enter to continue");
        Console.ReadLine();
    }
}