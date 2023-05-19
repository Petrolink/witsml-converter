// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using Petrolink.WitsmlConverter;

const string DefaultPattern = "*.xml";

var inputOption = new Option<IList<string>>(new[] { "--input", "-i" }, "An input file or directory path for the transformation. May be specified multiple times. Directory contents will be matched using the --patterns option.") { IsRequired = true };
var outputOption = new Option<string>(new[] { "--output", "-o" }, "The output directory.") { IsRequired = true };
var typeOption = new Option<string>(new[] { "--type", "-t" }, "The output object type.") { IsRequired = true };
var modeOption = new Option<WitsmlTransformType>(new[] { "--transform", "-x" }, "The transformation type.") { IsRequired = true };
var patternsOption = new Option<IList<string>>(new[] { "--patterns", "-p" }, () => new[] { DefaultPattern }, "An input file pattern used when matching against input directories. May be specified multiple times.");
var overwriteOption = new Option<bool>(new[] { "--overwrite", "-f" }, "Whether to overwrite destination files.");

var rootCommand = new RootCommand("WITSML Converter");

var transformCommand = new Command("transform", "Transform one or more WITSML documents")
{
    inputOption,
    outputOption,
    typeOption,
    modeOption,
    patternsOption,
    overwriteOption
};

rootCommand.AddCommand(transformCommand);

transformCommand.SetHandler(ExecuteTransform,
                            inputOption,
                            outputOption,
                            typeOption,
                            modeOption,
                            patternsOption,
                            overwriteOption);

return rootCommand.Invoke(args);

// Execute the transform command
void ExecuteTransform(
    IList<string> input,
    string output,
    string type,
    WitsmlTransformType conversion,
    IList<string> patterns,
    bool overwrite)
{
    // TODO Detect destination type based on the input
    var matcher = new Matcher();

    matcher.AddIncludePatterns(patterns);

    var inputFilePaths = input.SelectMany(p => ResolveFiles(p, matcher)).ToList();

    if (inputFilePaths.Count == 0)
    {
        WriteError("No valid inputs found");
        return;
    }

    PathType outputType = GetPathType(output);

    if (outputType != PathType.Directory)
    {
        throw DefaultException("Output path is not a directory");
    }

    Directory.CreateDirectory(output);

    foreach (var inputPath in inputFilePaths)
    {
        var outputPath = Path.Combine(output, Path.GetFileName(inputPath));

        Console.WriteLine(inputPath);

        if (!overwrite && File.Exists(outputPath))
        {
            WriteError($"Output file already exists: {outputPath}");
            continue;
        }

        try
        {
            var inputData = File.ReadAllText(inputPath);

            var outputData = WitsmlTransformer.Transform(inputData, conversion, type);

            File.WriteAllText(outputPath, outputData);
        }
        catch (Exception ex)
        {
            WriteError($"Failed to transform file due to exception: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

// Resolves an input file or folder to a list of files
IEnumerable<string> ResolveFiles(string input, Matcher matcher)
{
    return GetPathType(input) switch
    {
        PathType.None => throw DefaultException($"Path does not refer to a file or directory: {input}"),
        PathType.File => new[] { Path.GetFullPath(input) },
        PathType.Directory => matcher.GetResultsInFullPath(input),
        _ => throw new NotImplementedException()
    };
}

// Gets whether a given path represents a file, directory, or nothing
PathType GetPathType(string path)
{
    if (File.Exists(path))
        return PathType.File;
    if (Directory.Exists(path))
        return PathType.Directory;
    return PathType.None;
}

Exception DefaultException(string text) => new Exception(text);

void WriteError(string text)
{
    Console.Error.WriteLine(text);
}

enum PathType
{
    None,
    File,
    Directory
}
