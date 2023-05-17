using System.Reflection;
using System.Text;
using System.Xml;

namespace Petrolink.WitsmlConverter;

/// <summary>
/// Performs transformations between WITSML 2.0 and WITSML 1.4
/// </summary>
public static class WitsmlTransformer
{
    private const string WitsmlV2Namespace = "http://www.energistics.org/energyml/data/witsmlv2";
    private const string EnergisticsCommonNamespace = "http://www.energistics.org/energyml/data/commonv2";

    private delegate bool UomConverter(string uom, out string newUom);

    /// <summary>
    /// Transforms a document from WITSML 1.4 to 2.0, or the reverse.
    /// </summary>
    /// <param name="input">Input XML document string</param>
    /// <param name="version">The mapping version</param>
    /// <param name="typeName">The destination type name</param>
    public static string Transform(string input, WitsmlConversionType version, string typeName)
    {
        return Transform(input, new WitsmlTransformOptions(version, typeName));
    }

    /// <summary>
    /// Transforms a document from WITSML 1.4 to 2.0, or the reverse.
    /// </summary>
    /// <param name="input">Input XML document string</param>
    /// <param name="options">Options for the transformation</returns>
    public static string Transform(
        string input,
        WitsmlTransformOptions options)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        ValidateOptions(options);

        var types = GetMapperTypes(options.ConversionType, options.DestinationTypeName);

        var inputDoc = ToXmlDocument(input);

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
        if (options.ConversionType == WitsmlConversionType.Witsml14To20 && (options.AddCreationTimes ?? true))
        {
            AddCreationTimes(outputDoc, nsm, options.CreationTime);
        }

        if (types.Map2 != null)
        {
            outputDoc = RunMapper(outputDoc, types.Map2);
        }

        if (options.ConvertUnits ?? true)
        {
            if (options.ConversionType == WitsmlConversionType.Witsml14To20)
            {
                ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert14To20);
            }
            else if (options.ConversionType == WitsmlConversionType.Witsml20To14)
            {
                ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert20To14);
            }
        }

        if (types.After1 != null)
            outputDoc = RunMapper(outputDoc, types.After1);

        if (types.After2 != null)
            outputDoc = RunMapper(outputDoc, types.After2);

        return ToString(outputDoc, options.XmlWriterSettings);
    }

    private static void ValidateOptions(WitsmlTransformOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        if (options.ConversionType is < WitsmlConversionType.Witsml14To20 or > WitsmlConversionType.Witsml21To20)
            throw new ArgumentOutOfRangeException(nameof(options), "ConversionType is invalid");
        if (string.IsNullOrWhiteSpace(options.DestinationTypeName))
            throw new ArgumentException("DestinationTypeName must be specified", nameof(options));
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

    private static XmlDocument ToXmlDocument(string str)
    {
        using var reader = new StringReader(str);

        var doc = new XmlDocument();
        doc.Load(reader);

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

    private static MappingRegistry.MappingTypes GetMapperTypes(WitsmlConversionType version, string type)
    {
        try
        {
            return version switch
            {
                WitsmlConversionType.Witsml14To20 => MappingRegistry.s_14to20Types[type],
                WitsmlConversionType.Witsml20To14 => MappingRegistry.s_20to14Types[type],
                WitsmlConversionType.Witsml20To21 => MappingRegistry.s_20To21Types[type],
                WitsmlConversionType.Witsml21To20 => MappingRegistry.s_21To20Types[type],
                _ => throw new ArgumentOutOfRangeException(nameof(version)),
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new ArgumentOutOfRangeException("Unknown mapping type: " + type, ex);
        }
    }
}
