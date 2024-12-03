namespace Statify;

public static class PathUtilities
{
    public static string TempDirectory
    {
        get
        {
            var statifyRelativePath =
                ConfigurationManager.GetConfigurationValue<string>(ConfigurationManager.StatifyDirectoryKey);
            var statifyAbsolutePath = RelativeToProjectDirectory(statifyRelativePath);

            return Path.Join(statifyAbsolutePath, "./temp/");
        }
    }

    public static string RelativeToProjectDirectory(string relativePath)
    {
        var path = Path.Join(Program.ProjectDirectory, relativePath);
        return Path.GetFullPath(path);
    }
}