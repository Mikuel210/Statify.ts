using System.Text.Json;

namespace Statify;

public static class ConfigurationManager
{
    public const string CompileFileKey = "compileFile";
    public const string StatifyDirectoryKey = "statifyDirectory";

    private static readonly Dictionary<string, object> DefaultConfiguration;
    private static readonly Dictionary<string, JsonElement> Configuration;

    static ConfigurationManager()
    {
        DefaultConfiguration = new Dictionary<string, object>
        {
            [CompileFileKey] = "compile.js",
            [StatifyDirectoryKey] = "./.statify/"
        };

        var configurationFilePath = PathUtilities.RelativeToProjectDirectory(".statifyconfig");
        var configurationFileContent = string.Empty;

        try
        {
            configurationFileContent = File.ReadAllText(configurationFilePath);
            Configuration = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configurationFileContent);
        }
        catch (FileNotFoundException) { }
        finally
        {
            Configuration ??= new Dictionary<string, JsonElement>();
        }
    }

    public static T GetConfigurationValue<T>(string key)
    {
        if (Configuration.ContainsKey(key))
            return Configuration[key].Deserialize<T>() ?? (T)DefaultConfiguration[key];
        if (DefaultConfiguration.ContainsKey(key))
            return (T)DefaultConfiguration[key];

        throw new KeyNotFoundException("Key not found in configuration");
    }
}