using System.Diagnostics;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace Statify;

public static class Compiler
{
    private const string EngineTemplate = @"
        var console = {
            log: function(message) {
                host.Console.WriteLine(message);
            }
        };
    ";

    public static void Compile()
    {
        ConsoleWriter.Title("Compiling project...");

        var compileFileRelativePath =
            ConfigurationManager.GetConfigurationValue<string>(ConfigurationManager.CompileFileKey);

        var compileFileFullPath = PathUtilities.RelativeToProjectDirectory(compileFileRelativePath);
        var compileFileContents = File.ReadAllText(compileFileFullPath);

        if (Path.GetExtension(compileFileFullPath) == ".ts")
            compileFileContents = compileFileFullPath;

        using (var engine = new V8ScriptEngine())
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Statify", typeof(Api));

            try
            {
                engine.Execute(EngineTemplate);
                engine.Execute(compileFileContents);
            }
            catch (Exception e)
            {
                ConsoleWriter.Fatal("Error while executing compile file:");
                Console.WriteLine(e.Message);
            }
        }
    }

    private static string CompileTypeScript(string path)
    {
        var outputPath = Path.Join(PathUtilities.TempDirectory, Path.GetRandomFileName());
        outputPath = Path.GetFullPath(outputPath);

        ConsoleWriter.Title($"\"{path}\" --outFile \"{outputPath}\"");

        File.WriteAllText(outputPath, string.Empty);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "tsc",
                Arguments = $"\"{path}\" --outFile \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var errors = process.StandardError.ReadToEnd();
            throw new Exception($"TypeScript compilation failed: {errors}");
        }

        var result = File.ReadAllText(outputPath);
        File.Delete(outputPath);

        return result;
    }
}