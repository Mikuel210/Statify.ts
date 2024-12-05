using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Statify;

public static class Compiler {

	private const string FinishCompilationCode = "STATIFY_END_COMPILATION";

	private static string GetEngineTemplate(string relativeScriptPath) =>
		$$"""
			  require('ts-node').register({ compilerOptions: { module: 'CommonJS' } }); 
			  require('./{{relativeScriptPath}}');
			  console.log('{{FinishCompilationCode}}');
			  """.Replace("\n", "");

	public static void Compile() {
		ConsoleWriter.Title("Compiling project...");

		var compileFileRelativePath =
			ConfigurationManager.GetConfigurationValue<string>(ConfigurationManager.CompileFileKey);

		var process = new Process {
			StartInfo = new() {
				FileName = "node",
				Arguments = $"-e \"{GetEngineTemplate(compileFileRelativePath)}\"",
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		process.Start();

		StreamReader standardOutput = process.StandardOutput;
		StreamWriter standardInput = process.StandardInput;

		var ended = false;

		while (!ended) {
			string line = standardOutput.ReadLine()!;

			try {
				var packet = JsonSerializer.Deserialize<Packet>(line)!;
				packet.Process();

				standardInput.WriteLine(true);
				standardInput.Flush();
			}
			catch (JsonException) {
				if (line == FinishCompilationCode)
					ended = true;
				else
					Console.WriteLine(line);
			}
			catch (Exception e) when
				(e is InvalidOperationException or TargetParameterCountException) {
				standardInput.WriteLine(false);
				standardInput.Flush();
			}
		}

		ConsoleWriter.Success("Compilation was successful.");

		string error = process.StandardError.ReadToEnd();

		if (!string.IsNullOrEmpty(error)) {
			ConsoleWriter.Fatal("An error occured while executing the compile file:");
			Console.WriteLine(error);
		}
	}

	private class Packet {

		public enum EFunction { Write = 0, Compile = 1 }
		[JsonPropertyName("function")] public EFunction Function { get; init; }

		[JsonPropertyName("parameters")] public JsonElement Parameters { get; init; }

		public void Process() {
			MethodInfo method = typeof(Api).GetMethod(Function.ToString()) ??
								throw new ArgumentException(
									"The function enum value does not correspond to a valid method.");

			object[] parametersArray = Parameters.EnumerateArray()
				.Select<JsonElement, object>(e => (e.ValueKind switch {
					JsonValueKind.String => e.GetString(),
					JsonValueKind.Number => e.TryGetInt32(out int intValue) ? intValue : e.GetDouble(),
					JsonValueKind.True => true,
					JsonValueKind.False => false,
					JsonValueKind.Object => e.Deserialize<Dictionary<string, object>>(),
					_ => throw new FormatException("The parameter type is not supported.")
				})!)
				.ToArray();

			method.Invoke(typeof(Api), parametersArray);
		}

	}

}