using System.Text.RegularExpressions;

namespace Statify;

public static class Interpreter {
	
	public static string Interpret(string template, Dictionary<string, object> context) {
		string pattern = """<\s*statify\s*>|<\s*\/\s*statify\s*>""";
		string[] parts = Regex.Split(template, pattern);

		string virtualEnvironmentTemplate = string.Empty;
		bool writingTypeScript = false;
		
		foreach (string part in parts) {
			if (writingTypeScript)
				virtualEnvironmentTemplate += part;
			else
				virtualEnvironmentTemplate += $"\nstatify.write('{part.Replace("'", "\\'")}');";
			
			writingTypeScript = !writingTypeScript;
		}
		
		return virtualEnvironmentTemplate;
	}

}