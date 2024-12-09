using System.Text.RegularExpressions;

namespace Statify;

public static class Interpreter
{
    public static string Interpret(Dictionary<string, object> context)
    {
        var template = (string)context["template"];
        var virtualEnvironmentTemplate = GetVirtualEnvironmentTemplate(template);

        ProcessManager.ExecuteVirtualEnvironmentFromCode(virtualEnvironmentTemplate);

        return virtualEnvironmentTemplate;
    }

    private static string GetVirtualEnvironmentTemplate(string template)
    {
        var pattern = """<\s*statify\s*>|<\s*\/\s*statify\s*>""";
        var parts = Regex.Split(template, pattern);

        var virtualEnvironmentTemplate = """
                                         const path = require('path');

                                         // Ensure the parent node_modules directory is included
                                         module.paths.push(path.resolve(__dirname, '../../node_modules'));

                                         const statify = require('statify');
                                         """;
        var writingTypeScript = false;

        foreach (var part in parts)
        {
            if (writingTypeScript)
                virtualEnvironmentTemplate += part;
            else
                virtualEnvironmentTemplate += $"\nstatify.write(`{part.Replace("`", "\\`")}`);";

            writingTypeScript = !writingTypeScript;
        }

        return virtualEnvironmentTemplate;
    }
}