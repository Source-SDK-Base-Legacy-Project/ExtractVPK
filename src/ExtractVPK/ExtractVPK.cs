using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ExtractVPK;

public static class RegexHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<Regex> StringListToRegexList(List<string> strings)
    {
        return 0 == strings.Count ? [] : strings.Select(s => new Regex(s, RegexOptions.Compiled | RegexOptions.IgnoreCase)).ToList();
    }
}

public class FileFilter(List<Regex> filesRegex)
{
    readonly List<Regex> _filesRegex = filesRegex;

    public bool PassesFilter(string file)
    {
        if (_filesRegex.Count == 0)
            return true;

        // If input matches at least one of the patterns, accept.
        if (_filesRegex.Any(r => r.IsMatch(file)))
            return true;

        return false;
    }
}

public class ExtractVPKLogic(IFileSystem fileSystem, IVPKExtractor vpkExtractor)
{
    public List<string>? FilesToExtract { get; set; }
    public List<string>? FilesToExclude { get; set; }

    public bool ExtractVPK(string vpkFilePath, string outputPath, ILogger? logger = null)
    {
        if (!fileSystem.File.Exists(vpkFilePath))
            throw new FileNotFoundException($"The file {fileSystem.Path.GetFileName(vpkFilePath)} does not exist");

        if (!fileSystem.Directory.Exists(outputPath))
            fileSystem.Directory.CreateDirectory(outputPath);

        var filesToExcludeRegex = RegexHelper.StringListToRegexList(FilesToExclude ?? []);
        var filesToExtractRegex = RegexHelper.StringListToRegexList(FilesToExtract ?? []);

        var result = vpkExtractor.Extract(vpkFilePath, outputPath, new VPKFileFilter(filesToExcludeRegex, filesToExtractRegex), logger);
        return result == VPKExtractionResult.Complete;
    }
}