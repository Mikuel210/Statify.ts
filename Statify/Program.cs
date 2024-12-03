namespace Statify;

internal class Program
{
    private static readonly Dictionary<string, Action> Commands = new()
    {
        [""] = Compiler.Compile,
        ["compile"] = Compiler.Compile
    };

    public static string ProjectDirectory { get; private set; }

    private static void Main(string[] args)
    {
        ProjectDirectory = Directory.GetCurrentDirectory();

        var command = string.Empty;

        if (args.Length > 0)
            command = args[0].Trim().ToLower();

        if (Commands.ContainsKey(command))
            Commands[command]();
        else
            ConsoleWriter.Fatal("Command not found");
    }
}