namespace Petrolink.WitsmlConverter.Tests;

/// <summary>
/// Provides useful methods for working with unit test data in the TestData folder.
/// </summary>
internal static class TestData
{
    /// <summary>
    /// The default creation time used in unit test data.
    /// </summary>
    public static readonly DateTime CreationTime = new(2023, 05, 01, 1, 0, 0, DateTimeKind.Utc);

    public static string Get14SourcePath(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml14Source", $"{type}_no_xsl.xml");
    }

    public static string Get14As20Path(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml14As20", $"{type}.xml");
    }

    public static string Get14As21Path(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml14As21", $"{type}.xml");
    }

    public static string Get20SourcePath(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml20Source", $"{type}.xml");
    }

    public static string Get20As14Path(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml20As14", $"{type}.xml");
    }

    public static string Get20As21Path(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml20As21", $"{type}.xml");
    }

    public static string Read14SourceData(string type)
    {
        return File.ReadAllText(Get14SourcePath(type));
    }

    /// <summary>
    /// A list of types used when converting from WITSML 1.4 to 2.0. The second item is
    /// the optional differing WITSML 2.0 type name. If null the 1.4 type name is used.
    /// </summary>
    public static readonly IReadOnlyList<(string, string?)> Witsml14To20Types = new[] {
        ("attachment", null),
        ("bharun", null),
        ("cementjob", null),
        ("drillReport", null),
        ("fluidsreport", null),
        ("formationMarker", "wellboremarker"),
        ("log", null),
        ("mudlog", "mudlogreport"),
        ("opsreport", null),
        ("rig", null),
        ("rig", "rigUtilization"),
        ("risk", null),
        ("stimjob", null),
        ("surveyprogram", null),
        ("toolErrorModel", null),
        ("toolErrorTermSet", null),
        ("trajectory", null),
        ("tubular", null),
        ("wbgeometry", "wellboregeometry"),
        ("well", null),
        ("wellbore", null),
        ("mudlog", "wellboregeology"),
    };

    public static readonly IReadOnlyList<(string, string?)> Witsml20To14Types = new[] {
        ("log", null),
        ("rig", null),
        ("rig", "rigUtilization"),
        ("well", null),
        ("wellbore", null)
    };

    public static readonly IReadOnlyList<string> Witsml20To21Types = new[]
    {
        "Log",
        "Rig",
        "RigUtilization",
        "Trajectory",
        "Well",
        "Wellbore"
    };
}
