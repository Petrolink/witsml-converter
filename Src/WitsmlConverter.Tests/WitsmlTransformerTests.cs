using System.Xml;

namespace Petrolink.WitsmlConverter.Tests;

public class WitsmlTransformerTests
{
    public static IEnumerable<object?[]> Witsml14To20TypesData => TestData.Witsml14To20Types.Select(t => new[] { t.Item1, t.Item2 });

    public static IEnumerable<object?[]> Witsml20To14TypesData => TestData.Witsml20To14Types.Select(t => new[] { t.Item1, t.Item2 });

    [Theory]
    [MemberData(nameof(Witsml14To20TypesData))]
    public void Transform_14Objects_20Objects(string w14type, string? w20type)
    {
        var destType = w20type ?? w14type;

        var srcObj = TestData.Read14SourceData(w14type);
        var dstObj = File.ReadAllText(TestData.Get14As20Path(destType));

        var options = new WitsmlTransformOptions
        {
            DestinationTypeName = destType,
            ConversionType = WitsmlConversionType.Witsml14To20,
            XmlWriterSettings = new XmlWriterSettings { Indent = true },
            // Must use a specific creation time to ensure the resulting XML is identical
            CreationTime = TestData.CreationTime
        };

        var resObj = WitsmlTransformer.Transform(srcObj, options);

        Assert.Equal(dstObj, resObj);
    }

    [Theory]
    [MemberData(nameof(Witsml14To20TypesData))]
    public void Transform_14Objects_21Objects(string w14type, string? w20type)
    {
        var dest20Type = w20type ?? w14type;

        var dstObjPath = TestData.Get14As21Path(dest20Type);

        // Not all types supported yet
        if (!File.Exists(dstObjPath))
            return;

        var srcObj = TestData.Read14SourceData(w14type);
        var dstObj = File.ReadAllText(dstObjPath);

        var options20 = new WitsmlTransformOptions
        {
            DestinationTypeName = dest20Type,
            ConversionType = WitsmlConversionType.Witsml14To20,
            // Must use a specific creation time to ensure the resulting XML is identical
            CreationTime = TestData.CreationTime
        };

        var options21 = new WitsmlTransformOptions
        {
            DestinationTypeName = dest20Type,
            ConversionType = WitsmlConversionType.Witsml20To21,
            XmlWriterSettings = new XmlWriterSettings { Indent = true }
        };

        var resObj20 = WitsmlTransformer.Transform(srcObj, options20);
        var resObj21 = WitsmlTransformer.Transform(resObj20, options21);

        Assert.Equal(dstObj, resObj21);
    }

    [Theory]
    [MemberData(nameof(Witsml20To14TypesData))]
    public void Transform_20Objects_14Objects(string w14type, string? w20type)
    {
        var dstType = w14type;
        var srcType = w20type ?? w14type;

        var resName = $"{srcType}-to-{dstType}";

        var srcObj = File.ReadAllText(TestData.Get20SourcePath(srcType));
        var dstObj = File.ReadAllText(TestData.Get20As14Path(resName));

        var options = new WitsmlTransformOptions
        {
            DestinationTypeName = dstType,
            ConversionType = WitsmlConversionType.Witsml20To14,
            XmlWriterSettings = new XmlWriterSettings { Indent = true }
        };

        var resObj = WitsmlTransformer.Transform(srcObj, options);

        Assert.Equal(dstObj, resObj);
    }
}
