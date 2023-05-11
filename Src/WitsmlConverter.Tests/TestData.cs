using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petrolink.WitsmlConverter.Tests;

internal static class TestData
{
    public static string Get14SourcePath(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml14Source", $"{type}_no_xsl.xml");
    }
    public static string Get14As20Path(string type)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", "Witsml14As20", $"{type}.xml");
    }

    public static string Read14SourceData(string type)
    {
        return File.ReadAllText(Get14SourcePath(type));
    }

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
}
