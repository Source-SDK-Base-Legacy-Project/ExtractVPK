using SteamDatabase.ValvePak;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace ExtractVPK;

public interface IVPKFileFilter
{
    bool PassesFilter(string vpkFile);
}

public class VPKFileFilter(List<Regex> filesToExcludeRegex, List<Regex> filesToExtractRegex) : IVPKFileFilter
{
    readonly List<Regex> _filesToExcludeRegex = filesToExcludeRegex;
    readonly List<Regex> _filesToExtractRegex = filesToExtractRegex;

    public bool PassesFilter(string vpkFile)
    {
        // If input matches one of the file exclusion pattern, ignore.
        if (_filesToExcludeRegex.Count > 0 && _filesToExcludeRegex.Any(r => r.IsMatch(vpkFile)))
            return false;

        // If input doesn't match any of the files to extract pattern, ignore.
        if (_filesToExtractRegex.Count > 0 && !_filesToExtractRegex.Any(r => r.IsMatch(vpkFile)))
            return false;

        return true;
    }
}

public enum VPKExtractionResult
{
    Complete = 0,
    CompleteWithErrors,
    Failed
}

public interface IVPKExtractor
{
    VPKExtractionResult Extract(string vpkPath, string outputDir, IVPKFileFilter fileFilter, ILogger? logger);
}

public class VPKExtractor : IVPKExtractor
{
    readonly IFileSystem _fileSystem;
    public bool NoFileRootPath { get; set; } = false;

    public VPKExtractor(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public VPKExtractionResult Extract(string vpkPath, string outputDir, IVPKFileFilter fileFilter, ILogger? logger = null)
    {
        VPKExtractionResult result = VPKExtractionResult.Complete;

        try
        {
            if (!_fileSystem.Directory.Exists(outputDir))
                _fileSystem.Directory.CreateDirectory(outputDir);

            using var package = new Package();

            var fs = _fileSystem.FileStream.New(vpkPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            package.SetFileName(vpkPath);
            package.Read(fs);

            const bool VALIDATE_CRC = false;
            if (VALIDATE_CRC)
                package.VerifyHashes();

            int numExpectedFilesToExtract = 0;
            foreach (var extension in package.Entries.Values)
            {
                foreach (var entry in extension)
                {
                    // Only allow entries that pass the filter.
                    if (fileFilter.PassesFilter(entry.GetFullPath()))
                        ++numExpectedFilesToExtract;
                }
            }

            int numExtractedFiles = 0;
            foreach (var extension in package.Entries.Values)
            {
                foreach (var entry in extension)
                {
                    // Only allow entries that pass the filter.
                    if (!fileFilter.PassesFilter(entry.GetFullPath()))
                        continue;

                    string? entryDir = _fileSystem.Path.GetDirectoryName(entry.GetFullPath());
                    if (null == entryDir)
                    {
                        logger?.LogError($"Error: {entry.GetFullPath()}");
                        result = VPKExtractionResult.CompleteWithErrors;
                        continue;
                    }
                    entryDir = PathUtils.ConvertToUnixSeparator(_fileSystem.Path.Join(outputDir, entryDir));

                    if (!NoFileRootPath && !_fileSystem.Directory.Exists(entryDir))
                        _fileSystem.Directory.CreateDirectory(entryDir);

                    logger?.LogInformation($"Extracting {entry.GetFullPath()}");
                    var fullPath = PathUtils.ConvertToUnixSeparator(_fileSystem.Path.Join(outputDir, NoFileRootPath ? entry.GetFileName() : entry.GetFullPath()));
                    package.ReadEntry(entry, out byte[] fileContents, VALIDATE_CRC);
                    _fileSystem.File.WriteAllBytes(fullPath, fileContents);
                    ++numExtractedFiles;
                }
            }

            fs?.Close();

            // If at least one file was to be extracted and none were extracted, mark it as failed.
            if (numExpectedFilesToExtract > 0 && numExtractedFiles == 0)
                result = VPKExtractionResult.Failed;
        }
        catch (Exception e)
        {
            logger?.LogError(e.Message);
            result = VPKExtractionResult.Failed;
        }

        return result;
    }
}