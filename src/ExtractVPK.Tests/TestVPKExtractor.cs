using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Abstractions.TestingHelpers;

namespace ExtractVPK.Tests;

public class AllowAllVPKFiles : IVPKFileFilter
{
    public bool PassesFilter(string vpkFile) => true;
}

public class ExcludeAllVPKFiles : IVPKFileFilter
{
    public bool PassesFilter(string vpkFile) => false;
}

public class NoSoundWavFilter : IVPKFileFilter
{
    public bool PassesFilter(string vpkFile)
    {
        return vpkFile != "sound.wav";
    }
}

[TestClass]
public class TestVPKExtractor
{
    [TestMethod]
    public void NoFilesExtractedWithExcludeAllFilter()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "C:/vpks/simple_vpk.vpk", new MockFileData(File.ReadAllBytes("data/vpks/simple_vpk.vpk")) }
        });

        var extractor = new VPKExtractor(fileSystem);
        var vpkExtractionResult = extractor.Extract("C:/vpks/simple_vpk.vpk", "C:/output", new ExcludeAllVPKFiles());
        var expected = new string[] { };
        var actual = fileSystem.Directory.GetFiles("C:/output").Select(a => PathUtils.ConvertToUnixSeparator(a)).ToArray();
        Assert.AreEqual(VPKExtractionResult.Complete, vpkExtractionResult);
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [TestMethod]
    public void AllFilesExtractedWithAllowAllFilter()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "C:/vpks/simple_vpk.vpk", new MockFileData(File.ReadAllBytes("data/vpks/simple_vpk.vpk")) }
        });

        var extractor = new VPKExtractor(fileSystem);
        var vpkExtractionResult = extractor.Extract("C:/vpks/simple_vpk.vpk", "C:/output", new AllowAllVPKFiles());
        var expected = new string[] {
            "C:/output/file1.txt",
            "C:/output/model.mdl",
            "C:/output/sound.wav",
        };
        var actual = fileSystem.Directory.GetFiles("C:/output").Select(a => PathUtils.ConvertToUnixSeparator(a)).ToArray();
        Assert.AreEqual(VPKExtractionResult.Complete, vpkExtractionResult);
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [TestMethod]
    public void SpecificFilesExtractedWithCustomFilter()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "C:/vpks/simple_vpk.vpk", new MockFileData(File.ReadAllBytes("data/vpks/simple_vpk.vpk")) }
        });

        var extractor = new VPKExtractor(fileSystem);
        var vpkExtractionResult = extractor.Extract("C:/vpks/simple_vpk.vpk", "C:/output", new NoSoundWavFilter());
        var expected = new string[] {
            "C:/output/file1.txt",
            "C:/output/model.mdl"
        };
        var actual = fileSystem.Directory.GetFiles("C:/output").Select(a => PathUtils.ConvertToUnixSeparator(a)).ToArray();
        Assert.AreEqual(VPKExtractionResult.Complete, vpkExtractionResult);
        CollectionAssert.AreEquivalent(expected, actual);
    }
}