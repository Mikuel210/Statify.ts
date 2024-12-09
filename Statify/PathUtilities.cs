namespace Statify;

public static class PathUtilities
{
    public static string ProjectDirectory => Program.ProjectDirectory;

    public static string TempDirectory
    {
        get
        {
            var statifyRelativePath =
                ConfigurationManager.GetConfigurationValue<string>(ConfigurationManager.StatifyDirectoryKey);
            var statifyAbsolutePath = From(ProjectDirectory, statifyRelativePath);

            return Path.Join(statifyAbsolutePath, "./temp/");
        }
    }

    public static string From(string directory, string relativePath)
    {
        var path = Path.Join(directory, relativePath);
        return Path.GetFullPath(path);
    }
}