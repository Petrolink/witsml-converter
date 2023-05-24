// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Xml.Schema;
using Microsoft.Extensions.FileSystemGlobbing;
using Petrolink.WitsmlConverter;

const string DefaultPattern = "*.xml";
const string SchemaPattern = "*.xsd";

var inputOption = new Option<IList<string>>(new[] { "--input", "-i" }, "An input file or directory path for the transformation. May be specified multiple times. Directory contents will be matched using the --patterns option.") { IsRequired = true };
var outputOption = new Option<string>(new[] { "--output", "-o" }, "The output directory.") { IsRequired = true };
var typeOption = new Option<string>(new[] { "--type", "-t" }, "The output object type.") { IsRequired = true };
var modeOption = new Option<WitsmlTransformType>(new[] { "--transform", "-x" }, "The transformation type.") { IsRequired = true };
var patternsOption = new Option<IList<string>>(new[] { "--patterns", "-p" }, () => new[] { DefaultPattern }, "An input file pattern used when matching against input directories. May be specified multiple times.");
var overwriteOption = new Option<bool>(new[] { "--overwrite", "-f" }, "Whether to overwrite destination files.");
var validationModeOption = new Option<WitsmlValidationMode>(new[] { "--validation-mode", "-a" }, "How to validate WITSML documents against their schemas. Default is equivalent to OnError.");
var schemaDirOption = new Option<IList<string>>(new[] { "--schemas", "-s" }, "A path to a directory containing WITSML schemas. All .xsd files in the directory or a sub-directory will be used. May be specified multiple times.");

// TODO Add verbosity option, -v is reversed for this

var rootCommand = new RootCommand("WITSML Converter");

var transformCommand = new Command("transform", "Transform one or more WITSML documents")
{
    inputOption,
    outputOption,
    typeOption,
    modeOption,
    patternsOption,
    overwriteOption,
    validationModeOption,
    schemaDirOption
};

rootCommand.AddCommand(transformCommand);

transformCommand.SetHandler(ExecuteTransform,
                            inputOption,
                            outputOption,
                            typeOption,
                            modeOption,
                            patternsOption,
                            overwriteOption,
                            validationModeOption,
                            schemaDirOption);

return rootCommand.Invoke(args);

// Execute the transform command
void ExecuteTransform(
    IList<string> input,
    string output,
    string type,
    WitsmlTransformType conversion,
    IList<string> patterns,
    bool overwrite,
    WitsmlValidationMode validationMode,
    IList<string> schemaDirs)
{
    // TODO Detect destination type based on the input
    var matcher = new Matcher();

    matcher.AddIncludePatterns(patterns);

    var inputFilePaths = input.SelectMany(p => ResolveFiles(p, matcher)).ToList();

    if (inputFilePaths.Count == 0)
    {
        WriteError("Error: No valid inputs found");
        return;
    }

    switch (GetPathType(output))
    {
        case PathType.File:
            WriteError("Error: Output path is a file");
            return;
        case PathType.None:
            Directory.CreateDirectory(output);
            break;
    }

    XmlSchemaSet? schemaSet = null;

    if (schemaDirs.Count > 0)
    {
        schemaSet = new XmlSchemaSet();

        // Deduplicate by file name
        var loadedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var schemaDir in schemaDirs)
        {
            foreach (var (path, schema) in LoadAllSchemas(schemaDir))
            {
                var fileName = Path.GetFileName(path);

                if (loadedFiles.Add(fileName))
                {
                    schemaSet.Add(schema);
                }
            }
        }
    }

    foreach (var inputPath in inputFilePaths)
    {
        var outputPath = Path.Combine(output, Path.GetFileName(inputPath));

        Console.WriteLine($"Processing {inputPath}");

        if (!overwrite && File.Exists(outputPath))
        {
            WriteError($"Error: Output file already exists: {outputPath}");
            continue;
        }

        try
        {
            var inputData = File.ReadAllText(inputPath);

            var options = new WitsmlTransformOptions
            {
                ValidationMode = validationMode,
                SchemaSet = schemaSet
            };

            var outputData = WitsmlTransformer.Transform(inputData, conversion, type, options);

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

IEnumerable<(string, XmlSchema)> LoadAllSchemas(string path)
{
    foreach (var p in Directory.EnumerateFiles(path, SchemaPattern, SearchOption.AllDirectories))
    {
        XmlSchema? schema;

        using (var stream = File.OpenRead(p))
        {
            schema = XmlSchema.Read(stream, null);
        }

        if (schema != null)
            yield return (p, schema);
    }
}

enum PathType
{
    None,
    File,
    Directory
}
