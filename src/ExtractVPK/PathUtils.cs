namespace ExtractVPK;

public static class PathUtils
{
    public static string ConvertToUnixSeparator(string path)
    {
        return path.Replace('\\', '/')
                .Replace("\\", "/")
                .TrimEnd('\\')
                .TrimEnd('/');
    }
}