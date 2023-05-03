using System.Reflection;
using System.Text;
using System.Xml;
using Map14To20;
using Map20To14;
using Map20To21;
using Map21To20;

namespace Petrolink.WitsmlConverter;

/// <summary>
/// Performs transformations between WITSML 2.0 and WITSML 1.4
/// </summary>
public static class WitsmlTransformer
{
    private const string WitsmlV2Namespace = "http://www.energistics.org/energyml/data/witsmlv2";
    private const string EnergisticsCommonNamespace = "http://www.energistics.org/energyml/data/commonv2";

    private static readonly Dictionary<string, MappingTypes> s_14to20Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Attachment"] = new(typeof(Map14To20MapToAttachment)),
        ["BhaRun"] = new(typeof(Map14To20MapToBhaRun)),
        ["CementJob"] = new(typeof(Map14To20MapToCementJob)),
        ["DrillReport"] = new(typeof(Map14To20MapToDrillReport)),
        ["FluidsReport"] = new(typeof(Map14To20MapToFluidsReport)),
        ["Log"] = new(typeof(Map14To20MapToLog)),
        ["MudLogReport"] = new(typeof(Map14To20MapToMudLogReport)),
        ["OpsReport"] = new(typeof(Map14To20MapToOpsReport)),
        ["Rig"] = new(typeof(Map14To20MapToRig)),
        ["RigUtilization"] = new(typeof(Map14To20MapToRigUtilization)),
        ["Risk"] = new(typeof(Map14To20MapToRisk)),
        ["StimJob"] = new(typeof(Map14To20MapToStimJob), typeof(Map14To20MapToStimJob2)),
        ["SurveyProgram"] = new(typeof(Map14To20MapToSurveyProgram)),
        ["ToolErrorModel"] = new(typeof(Map14To20MapToToolErrorModel)),
        ["ToolErrorTermSet"] = new(typeof(Map14To20MapToToolErrorTermSet)),
        ["Trajectory"] = new(typeof(Map14To20MapToTrajectory)),
        ["Tubular"] = new(typeof(Map14To20MapToTubular)),
        ["Well"] = new(typeof(Map14To20MapToWell)),
        ["Wellbore"] = new(typeof(Map14To20MapToWellbore)),
        ["WellboreGeology"] = new(typeof(Map14To20MapToWellboreGeology), typeof(Map14To20MapToWellboreGeology2)),
        ["WellboreGeometry"] = new(typeof(Map14To20MapToWellboreGeometry)),
        ["WellboreMarker"] = new(typeof(Map14To20MapToWellboreMarkers)),
    };

    private static readonly Dictionary<string, MappingTypes> s_20to14Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["attachment"] = new(typeof(Map20To14MapToobj_attachment)),
        ["bharun"] = new(typeof(Map20To14MapToobj_bhaRun)),
        ["cementjob"] = new(typeof(Map20To14MapToobj_cementJob)),
        ["drillReport"] = new(typeof(Map20To14MapToobj_drillReport)),
        ["fluidsreport"] = new(typeof(Map20To14MapToobj_fluidsReport)),
        ["formationmarker"] = new(typeof(Map20To14MapToobj_formationMarker)),
        ["log"] = new(typeof(Map20To14MapToobj_log)),
        ["mudlog"] = new(typeof(Map20To14MapToobj_mudLog), typeof(Map20To14MapToobj_mudLog2)),
        ["opsreport"] = new(typeof(Map20To14MapToobj_opsReport)),
        ["rig"] = new(typeof(Map20To14MapToobj_rig)),
        ["rigUtilization"] = new(typeof(Map20To14MapToobj_rigUtilization)),
        ["risk"] = new(typeof(Map20To14MapToobj_risk)),
        ["stimjob"] = new(typeof(Map20To14MapToobj_stimJob)),
        ["surveyprogram"] = new(typeof(Map20To14MapToobj_surveyProgram)),
        ["toolErrorModel"] = new(typeof(Map20To14MapToobj_toolErrorModel)),
        ["toolErrorTermSet"] = new(typeof(Map20To14MapToobj_toolErrorTermSet)),
        ["trajectory"] = new(typeof(Map20To14MapToobj_trajectory)),
        ["tubular"] = new(typeof(Map20To14MapToobj_tubular)),
        ["wbgeometry"] = new(typeof(Map20To14MapToobj_wbGeometry)),
        ["well"] = new(typeof(Map20To14MapToobj_well)),
        ["wellbore"] = new(typeof(Map20To14MapToobj_wellbore)),
        ["wellboreGeology"] = new(typeof(Map20To14MapToobj_wellboreGeology), typeof(Map20To14MapToobj_wellboreGeology2)),
    };

    private static readonly Dictionary<string, MappingTypes> s_20To21Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Channel"] = new MappingTypes(Map1: typeof(Map20To21MapToLog),
                                       Before1: typeof(Map20To21MapToChannelSetFromChannel),
                                       Before2: typeof(Map20To21MapToLogFromChannelSet),
                                       After1: typeof(Map20To21MapToChannelSetFromLog),
                                       After2: typeof(Map20To21MapToChannelFromChannelSet)),
        ["ChannelSet"] = new MappingTypes(Map1: typeof(Map20To21MapToLog),
                                          Before1: typeof(Map20To21MapToLogFromChannelSet),
                                          After1: typeof(Map20To21MapToChannelSetFromLog)),
        ["Log"] = new(typeof(Map20To21MapToLog)),
        ["Rig"] = new(typeof(Map20To21MapToRig)),
        ["RigUtilization"] = new(typeof(Map20To21MapToRigUtilization)),
        ["Trajectory"] = new(typeof(Map20To21MapToTrajectory)),
        ["Well"] = new(typeof(Map20To21MapToWell)),
        ["Wellbore"] = new(typeof(Map20To21MapToWellbore)),
    };

    private static readonly Dictionary<string, MappingTypes> s_21To20Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Channel"] = new MappingTypes(Map1: typeof(Map21To20MapToLog),
                                       Before1: typeof(Map21To20MapToChannelSetFromChannel),
                                       Before2: typeof(Map21To20MapToLogFromChannelSet),
                                       After1: typeof(Map21To20MapToChannelSetFromLog),
                                       After2: typeof(Map21To20MapToChannelFromChannelSet)),
        ["ChannelSet"] = new MappingTypes(Map1: typeof(Map21To20MapToLog),
                                          Before1: typeof(Map21To20MapToLogFromChannelSet),
                                          After1: typeof(Map21To20MapToChannelSetFromLog)),
        ["Log"] = new(typeof(Map21To20MapToLog)),
        ["Rig"] = new(typeof(Map21To20MapToRig)),
        ["Well"] = new(typeof(Map21To20MapToWell)),
        ["Wellbore"] = new(typeof(Map21To20MapToWellbore)),
    };

    private delegate bool UomConverter(string uom, out string newUom);

    /// <summary>
    /// Transforms a document from WITSML 1.4 to 2.0, or the reverse.
    /// </summary>
    /// <param name="input">Input XML document string</param>
    /// <param name="version">The mapping version</param>
    /// <param name="typeName">The destination type name</param>
    /// <returns>A transformed XML document string</returns>
    public static string Transform(string input, WitsmlConversionType version, string typeName)
    {
        MappingTypes types = GetMapperTypes(version, typeName);

        XmlDocument inputDoc = ToXmlDocument(input);

        if (types.Before1 != null)
            inputDoc = RunMapper(inputDoc, types.Before1);

        if (types.Before2 != null)
            inputDoc = RunMapper(inputDoc, types.Before2);

        XmlDocument outputDoc = RunMapper(inputDoc, types.Map1);

        var nsm = new XmlNamespaceManager(outputDoc.NameTable);
        nsm.AddNamespace(string.Empty, WitsmlV2Namespace);
        nsm.AddNamespace("eml", EnergisticsCommonNamespace);

        // Need to add creation times before running the second mapper to ensure that the document is valid,
        // otherwise the second mapper will throw an exception when eml:Creation is missing.
        if (version == WitsmlConversionType.Witsml14To20)
        {
            AddCreationTimes(outputDoc, nsm);
        }

        if (types.Map2 != null)
        {
            outputDoc = RunMapper(outputDoc, types.Map2);
        }

        if (version == WitsmlConversionType.Witsml14To20)
        {
            ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert14To20);
        }
        else if (version == WitsmlConversionType.Witsml20To14)
        {
            ConvertUnits(outputDoc, nsm, Witsml20UnitConverter.TryConvert20To14);
        }

        if (types.After1 != null)
            outputDoc = RunMapper(outputDoc, types.After1);

        if (types.After2 != null)
            outputDoc = RunMapper(outputDoc, types.After2);

        return ToString(outputDoc);
    }

    /// <summary>
    /// Searches for all "uom" attributes in the document and attempts conversion using a converter delegate.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="nsm"></param>
    /// <param name="converter"></param>
    private static void ConvertUnits(XmlDocument doc, XmlNamespaceManager nsm, UomConverter converter)
    {
        foreach (XmlElement el in doc.SelectNodes("//*[@uom]", nsm).Cast<XmlElement>())
        {
            string uomVal = el.GetAttribute("uom");

            if (converter(uomVal, out string newUom))
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
    private static void AddCreationTimes(XmlDocument doc, XmlNamespaceManager nsm)
    {
        // Use the same timestamp value for everything
        string now = DateTime.UtcNow.ToString("O");

        foreach (XmlElement el in doc.SelectNodes("//eml:Citation", nsm).Cast<XmlElement>())
        {
            if (el.ChildNodes.OfType<XmlElement>().Any(e => e.Name == "eml:Creation"))
                continue;

            XmlElement creation = doc.CreateElement("eml:Creation", EnergisticsCommonNamespace);

            creation.InnerText = now;

            // Should be inserted in the correct order, which is before eml:Format
            XmlElement refNode = el.ChildNodes.OfType<XmlElement>().First(e => e.Name == "eml:Format");

            el.InsertBefore(creation, refNode);
        }
    }

    private static string ToString(XmlDocument doc)
    {
        var sb = new StringBuilder();

        using var writer = XmlWriter.Create(sb);

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

        object mapper1 = Activator.CreateInstance(type);

        mapper1.GetType().InvokeMember(
            "Run",
            BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
            null,
            mapper1,
            new object[] { alInput, alOutput });

        return outputDoc;
    }

    private static MappingTypes GetMapperTypes(WitsmlConversionType version, string type)
    {
        try
        {
            return version switch
            {
                WitsmlConversionType.Witsml14To20 => s_14to20Types[type],
                WitsmlConversionType.Witsml20To14 => s_20to14Types[type],
                WitsmlConversionType.Witsml20To21 => s_20To21Types[type],
                WitsmlConversionType.Witsml21To20 => s_21To20Types[type],
                _ => throw new ArgumentOutOfRangeException(nameof(version)),
            };
        }
        catch (KeyNotFoundException ex)
        {
            throw new ArgumentOutOfRangeException("Unknown mapping type: " + type, ex);
        }
    }

    private record MappingTypes(
        Type Map1,
        Type? Map2 = null,
        Type? Before1 = null,
        Type? Before2 = null,
        Type? After1 = null,
        Type? After2 = null);
}
