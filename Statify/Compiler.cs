namespace Statify;

public static class Compiler
{
    public static void Compile()
    {
        ConsoleWriter.Title("Compiling project...");

        var compileFileRelativePath =
            ConfigurationManager.GetConfigurationValue<string>(ConfigurationManager.CompileFileKey);

        ProcessManager.ExecuteVirtualEnvironment(compileFileRelativePath,
            "An error occured while executing the compile file:", "Project was successfully compiled.");
    }
}