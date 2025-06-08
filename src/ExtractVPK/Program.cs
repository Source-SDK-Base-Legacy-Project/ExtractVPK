using CommandLine;
using System.IO.Abstractions;

namespace ExtractVPK;

internal class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "VPK file to extract.")]
        public string InputVPKPath { get; set; } = string.Empty;

        [Option('o', "output", Required = true, HelpText = "Output path.")]
        public string OutputPath { get; set; } = string.Empty;

        [Option('e', "extract", Required = false, HelpText = "File(s) to extract.")]
        public IEnumerable<string> FilesToExtract { get; set; } = [];

        [Option('x', "exclude", Required = false, HelpText = "Files(s) to exclude.")]
        public IEnumerable<string> FilesToExclude { get; set; } = [];

        [Option('n', "no-path", Required = false, HelpText = "Don't append VPK file root path.")]
        public bool NoFileRootPath { get; set; }
    }

    public class Logger : ILogger
    {
        public void LogInformation(string message)
        {
            Console.WriteLine(message);
        }
        public void LogError(string message)
        {
            Console.Error.WriteLine(message);
        }
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                var fileSystem = new FileSystem();
                var logger = new Logger();
                var vpkExtractor = new VPKExtractor(fileSystem)
                {
                    NoFileRootPath = o.NoFileRootPath
                };
                var extractVPK = new ExtractVPKLogic(fileSystem, vpkExtractor)
                {
                    FilesToExclude = o.FilesToExclude.ToList(),
                    FilesToExtract = o.FilesToExtract.ToList(),
                };
                extractVPK.ExtractVPK(o.InputVPKPath, o.OutputPath, logger);
            });
    }
}
