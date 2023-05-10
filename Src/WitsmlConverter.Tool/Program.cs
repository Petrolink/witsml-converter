// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Petrolink.WitsmlConverter;

var inputOption = new Option<List<string>>(new[] { "--input", "-i" }, "Input file paths for the transformation") { IsRequired = true };
var outputOption = new Option<string>(new[] { "--output", "-o" }, "Output directory") { IsRequired = true };
var typeOption = new Option<string>(new[] { "--type", "-t" }, "The output object type") { IsRequired = true };
var modeOption = new Option<WitsmlConversionType>(new[] { "--conversion", "-c" }, "The conversion type") { IsRequired = true };
var overwriteOption = new Option<bool>(new[] { "--overwrite", "-r" }, "Whether to overwrite destination files");

var rootCommand = new RootCommand("WITSML Converter");

var transformCommand = new Command("transform", "Transform one or more WITSML documents")
{
    inputOption,
    outputOption,
    typeOption,
    modeOption,
};

rootCommand.AddCommand(transformCommand);

transformCommand.SetHandler(ExecuteTransform, inputOption, outputOption, typeOption, modeOption, overwriteOption);

return rootCommand.Invoke(args);

// Execute the transform command
void ExecuteTransform(List<string> input, string output, string type, WitsmlConversionType conversion, bool overwrite)
{
    // TODO Detect destination type based on the input

    List<string> inputFilePaths = input.SelectMany(ResolveFiles).ToList();

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
IEnumerable<string> ResolveFiles(string input)
{
    return GetPathType(input) switch
    {
        PathType.None => throw DefaultException($"Path does not refer to a file or directory: {input}"),
        PathType.File => new[] { Path.GetFullPath(input) },
        PathType.Directory => Directory.EnumerateFiles(input),
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
