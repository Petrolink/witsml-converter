using System.Reflection;
using System.Text;
using System.Xml;

namespace Petrolink.WitsmlConverter;

/// <summary>
/// Performs XML transformations on WITSML documents.
/// </summary>
public static class WitsmlTransformer
{
    private const string WitsmlV2Namespace = "http://www.energistics.org/energyml/data/witsmlv2";
    private const string EnergisticsCommonNamespace = "http://www.energistics.org/energyml/data/commonv2";

    // The maximum number of validation error messages to include in a single exception message.
    private const int MaxValidationMessages = 20;

    private delegate bool UomConverter(string uom, out string newUom);

    /// <summary>
    /// Performs an XML transformation on a WITSML document.
    /// </summary>
    /// <param name="input">An input XML document string.</param>
    /// <param name="transformType">The mapping version.</param>
    /// <param name="destinationType">The destination type name. This is not case-sensitive.</param>
    /// <param name="options">Options for the transformation.</param>
    /// <returns>An output XML document string.</returns>
    public static string Transform(
        string input,
        WitsmlTransformType transformType,
        string destinationType,
        WitsmlTransformOptions? options = null)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (transformType is < WitsmlTransformType.Witsml14To20 or > WitsmlTransformType.Witsml21To20)
            throw new ArgumentOutOfRangeException(nameof(transformType));
        if (destinationType == null)
            throw new ArgumentNullException(nameof(destinationType));
        if (options?.ValidationMode == WitsmlValidationMode.Enabled && options?.SchemaSet == null)
            throw new ArgumentException("Must provide a SchemaSet when WitsmlValidationMode is Enabled");

        var types = GetMapperTypes(transformType, destinationType!);

        var inputDoc = ToXmlDocument(input, false, options);

        try
        {
            if (types.Before1 != null)
                inputDoc = RunMapper(inputDoc, types.Before1);

            if (types.Before2 != null)
                inputDoc = RunMapper(inputDoc, types.Before2);

            var outputDoc = RunMapper(inputDoc, types.Map1);

            var nsm = new XmlNamespaceManager(outputDoc.NameTable);
            nsm.AddNamespace(string.Empty, WitsmlV2Namespace);
            nsm.AddNamespace("eml", EnergisticsCommonNamespace);

            // Need to add creation times before running the second mapper to ensure that the document is valid,
            // otherwise the second mapper will throw an exception when eml:Creation is missing.
            if (transformType == WitsmlTransformType.Witsml14To20 && (options?.AddCreationTimes ?? true))
            {
                AddCreationTimes(outputDoc, nsm, options?.CreationTime);
            }

            if (types.Map2 != null)
            {
                outputDoc = RunMapper(outputDoc, types.Map2);
            }

            if (options?.ConvertUnits ?? true)
            {
                if (transformType == WitsmlTransformType.Witsml14To20)
                {
                    ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert14To20);
                }
                else if (transformType == WitsmlTransformType.Witsml20To14)
                {
                    ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert20To14);
                }
            }

            if (types.After1 != null)
                outputDoc = RunMapper(outputDoc, types.After1);

            if (types.After2 != null)
                outputDoc = RunMapper(outputDoc, types.After2);

            return ToString(outputDoc, options?.XmlWriterSettings);
        }
        catch (TargetInvocationException ex) when (IsInvalidInputException(ex.InnerException))
        {
            // If we're here it means an exception was thrown because of a minOccurs or maxOccurs violation
            // To provide a more detailed exception we'll reload the input but indicate there is an error.
            // This will throw an exception if there is an XML schema validation found.
            _ = ToXmlDocument(input, true, options);

            // If no exception was thrown by ToXmlDocument() then that means validation is explicitly disabled or no
            // schema set was provided.
            throw new InvalidOperationException("The input WITSML document was not valid. For more error details, provide a valid XML schema set for the input document.", ex);
        }
    }

    /// <summary>
    /// Searches for all "uom" attributes in the document and attempts conversion using a converter delegate.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="nsm"></param>
    /// <param name="converter"></param>
    private static void ConvertUnits(XmlDocument doc, XmlNamespaceManager nsm, UomConverter converter)
    {
        foreach (var el in doc.SelectNodes("//*[@uom]", nsm).Cast<XmlElement>())
        {
            var uomVal = el.GetAttribute("uom");

            if (converter(uomVal, out var newUom))
            {
                el.SetAttribute("uom", newUom);
            }
        }
    }

    /// <summary>
    /// Searches for all eml:Citation elements and inserts an eml:Creation element if not present.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="nsm"></param>
    /// <param name="creationTime"></param>
    private static void AddCreationTimes(XmlDocument doc, XmlNamespaceManager nsm, DateTime? creationTime)
    {
        creationTime ??= DateTime.UtcNow;

        // Use the same timestamp value for everything
        var now = creationTime.Value.ToString("O");

        foreach (var el in doc.SelectNodes("//eml:Citation", nsm).Cast<XmlElement>())
        {
            if (el.ChildNodes.OfType<XmlElement>().Any(e => e.Name == "eml:Creation"))
                continue;

            var creation = doc.CreateElement("eml:Creation", EnergisticsCommonNamespace);

            creation.InnerText = now;

            // Should be inserted in the correct order, which is before eml:Format
            var refNode = el.ChildNodes.OfType<XmlElement>().First(e => e.Name == "eml:Format");

            el.InsertBefore(creation, refNode);
        }
    }

    private static string ToString(XmlDocument doc, XmlWriterSettings? writerSettings)
    {
        writerSettings ??= new XmlWriterSettings();

        var sb = new StringBuilder();

        using var writer = XmlWriter.Create(sb, writerSettings);

        doc.WriteTo(writer);

        writer.Flush();

        return sb.ToString();
    }

    private static XmlDocument ToXmlDocument(string str, bool isError, WitsmlTransformOptions? options)
    {
        var validationMode = options?.ValidationMode ?? WitsmlValidationMode.Default;

        var validate = validationMode switch
        {
            WitsmlValidationMode.Disabled => false,
            WitsmlValidationMode.Enabled => true,
            _ => isError // For Default or OnError mode
        };

        // Disable external XML resolution for security
        var settings = new XmlReaderSettings { XmlResolver = null };

        var errorCount = 0;
        List<string>? errorMessages = null;

        // If SchemaSet is required then an exception will have already been thrown during the
        // validation of WitsmlTransformOptions
        if (validate && options?.SchemaSet != null)
        {
            settings.Schemas.Add(options.SchemaSet);

            settings.ValidationType = ValidationType.Schema;

            // Since we're defining an event handler an exception will not be thrown during Load().
            // We'll collect all events into a list to combine into a single exception.
            settings.ValidationEventHandler += (s, e) =>
            {
                if (e.Severity == System.Xml.Schema.XmlSeverityType.Warning)
                    return;

                errorCount++;
                errorMessages ??= new List<string>();

                if (errorMessages.Count < MaxValidationMessages)
                {
                    errorMessages.Add($"Ln {e.Exception.LineNumber}, Ch {e.Exception.LinePosition}: {e.Message}");
                }
                else if (errorMessages.Count == MaxValidationMessages)
                {
                    errorMessages.Add("And one or more additional errors...");
                    // No more added after this to keep the exception message from being too long
                }
            };
        }

        using var textReader = new StringReader(str);
        using var xmlReader = XmlReader.Create(textReader, settings);

        var doc = new XmlDocument();
        doc.Load(xmlReader);

        // If there were any errors we combine them into a single exception message
        if (errorCount > 0)
        {
            var allErrors = string.Join(Environment.NewLine, errorMessages);
            var fullMsg = $"{errorCount} schema validation errors(s) occured:{Environment.NewLine}{allErrors}";
            // TODO Use a unique exception type
            throw new InvalidOperationException(fullMsg);
        }

        return doc;
    }

    private static XmlDocument RunMapper(XmlDocument input, Type type)
    {
        var outputDoc = new XmlDocument();

        using var alInput = new Altova.IO.DocumentInput(input);
        using var alOutput = new Altova.IO.DocumentOutput(outputDoc);

        // There is no common mapper type, so need to use reflection

        var mapper1 = Activator.CreateInstance(type);

        mapper1.GetType().InvokeMember(
            "Run",
            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
            null,
            mapper1,
            new object[] { alInput, alOutput });

        return outputDoc;
    }

    private static MappingRegistry.MappingTypes GetMapperTypes(WitsmlTransformType version, string type)
    {
        try
        {
            return version switch
            {
                WitsmlTransformType.Witsml14To20 => MappingRegistry.s_14to20Types[type],
                WitsmlTransformType.Witsml20To14 => MappingRegistry.s_20to14Types[type],
                WitsmlTransformType.Witsml20To21 => MappingRegistry.s_20To21Types[type],
                WitsmlTransformType.Witsml21To20 => MappingRegistry.s_21To20Types[type],
                _ => throw new ArgumentOutOfRangeException(nameof(version)),
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new ArgumentOutOfRangeException("Unknown mapping type: " + type, ex);
        }
    }

    private static bool IsInvalidInputException(Exception ex)
    {
        return ex is InvalidOperationException &&
               ex.Message == "The source node does not exist, which is invalid.\nIn order to process invalid input, disable optimizations based on min/maxOccurs in component settings.";
    }
}
