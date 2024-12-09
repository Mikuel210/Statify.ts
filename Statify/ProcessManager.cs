using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Statify;

public static class ProcessManager
{
    private static string GetNodeProcessTemplate(string relativeScriptPath, out string finishCode)
    {
        finishCode = "STATIFY_" + new Guid();
        var path = Path.GetFullPath(relativeScriptPath).Replace('\\', '/');

        return $$"""
                 require('ts-node').register({ compilerOptions: { module: 'CommonJS' } }); 
                 require('{{path}}');
                 console.log('{{finishCode}}');
                 """.Replace("\n", "");
    }

    public static Process ExecuteVirtualEnvironment(string relativePath,
        string errorMessage = "An error occured while executing the virtual environment:",
        string successMessage = "Successfully executed the virtual environment.")
    {
        var process = StartVirtualEnvironment(relativePath, out var finishCode);

        HandleCommunication(process, finishCode);

        if (process.HasExited && process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();

            ConsoleWriter.Fatal(errorMessage);
            Console.WriteLine(error);
        }
        else
        {
            ConsoleWriter.Success(successMessage);
        }

        return process;
    }

    private static void HandleCommunication(Process process, string finishCode)
    {
        var standardOutput = process.StandardOutput;
        var standardInput = process.StandardInput;

        while (!standardOutput.EndOfStream)
        {
            var line = standardOutput.ReadLine()!;

            try
            {
                var packet = JsonSerializer.Deserialize<Packet>(line)!;
                packet.Process();

                standardInput.WriteLine(true);
                standardInput.Flush();
            }
            catch (JsonException)
            {
                if (line == finishCode)
                    break;

                Console.WriteLine(line);
            }
            catch (Exception e) when
                (e is InvalidOperationException or TargetParameterCountException)
            {
                standardInput.WriteLine(false);
                standardInput.Flush();
            }
        }
    }

    private static Process StartVirtualEnvironment(string relativePath, out string finishCode)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"-e \"{GetNodeProcessTemplate(relativePath, out finishCode)}\"",
                WorkingDirectory = PathUtilities.ProjectDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        return process;
    }

    public static Process ExecuteVirtualEnvironmentFromCode(string code)
    {
        var path = PathUtilities.From(PathUtilities.TempDirectory, Guid.NewGuid() + ".ts");
        File.CreateText(path).Write(code);

        return ExecuteVirtualEnvironment(path);
    }

    private class Packet
    {
        public enum EFunction
        {
            Write = 0,
            Compile = 1
        }

        [JsonPropertyName("function")] public EFunction Function { get; init; }

        [JsonPropertyName("parameters")] public JsonElement Parameters { get; init; }

        public void Process()
        {
            var method = typeof(Api).GetMethod(Function.ToString()) ??
                         throw new ArgumentException(
                             "The function enum value does not correspond to a valid method.");

            var parametersArray = Parameters.EnumerateArray()
                .Select(ProcessJsonElement)
                .ToArray();


            method.Invoke(typeof(Api), parametersArray);
        }

        private object ProcessJsonElement(JsonElement e)
        {
            return (e.ValueKind switch
            {
                JsonValueKind.String => e.GetString(),
                JsonValueKind.Number => e.TryGetInt32(out var intValue) ? intValue : e.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => e.EnumerateObject()
                    .ToDictionary(
                        prop => prop.Name,
                        prop => ProcessJsonElement(prop.Value)
                    ),
                JsonValueKind.Array => e.EnumerateArray()
                    .Select(ProcessJsonElement)
                    .ToArray(),
                _ => throw new FormatException("The parameter type is not supported.")
            })!;
        }
    }
}