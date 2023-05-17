using Map14To20;
using Map20To14;
using Map20To21;
using Map21To20;

namespace Petrolink.WitsmlConverter;

internal static class MappingRegistry
{
    internal static readonly Dictionary<string, MappingTypes> s_14to20Types = new(StringComparer.OrdinalIgnoreCase)
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

    internal static readonly Dictionary<string, MappingTypes> s_20to14Types = new(StringComparer.OrdinalIgnoreCase)
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

    internal static readonly Dictionary<string, MappingTypes> s_20To21Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Channel"] = new(Map1: typeof(Map20To21MapToLog),
                          Before1: typeof(Map20To21MapToChannelSetFromChannel),
                          Before2: typeof(Map20To21MapToLogFromChannelSet),
                          After1: typeof(Map20To21MapToChannelSetFromLog),
                          After2: typeof(Map20To21MapToChannelFromChannelSet)),
        ["ChannelSet"] = new(Map1: typeof(Map20To21MapToLog),
                             Before1: typeof(Map20To21MapToLogFromChannelSet),
                             After1: typeof(Map20To21MapToChannelSetFromLog)),
        ["Log"] = new(typeof(Map20To21MapToLog)),
        ["Rig"] = new(typeof(Map20To21MapToRig)),
        ["RigUtilization"] = new(typeof(Map20To21MapToRigUtilization)),
        ["Trajectory"] = new(typeof(Map20To21MapToTrajectory)),
        ["Well"] = new(typeof(Map20To21MapToWell)),
        ["Wellbore"] = new(typeof(Map20To21MapToWellbore)),
    };

    internal static readonly Dictionary<string, MappingTypes> s_21To20Types = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Channel"] = new(Map1: typeof(Map21To20MapToLog),
                          Before1: typeof(Map21To20MapToChannelSetFromChannel),
                          Before2: typeof(Map21To20MapToLogFromChannelSet),
                          After1: typeof(Map21To20MapToChannelSetFromLog),
                          After2: typeof(Map21To20MapToChannelFromChannelSet)),
        ["ChannelSet"] = new(Map1: typeof(Map21To20MapToLog),
                             Before1: typeof(Map21To20MapToLogFromChannelSet),
                             After1: typeof(Map21To20MapToChannelSetFromLog)),
        ["Log"] = new(typeof(Map21To20MapToLog)),
        ["Rig"] = new(typeof(Map21To20MapToRig)),
        ["Well"] = new(typeof(Map21To20MapToWell)),
        ["Wellbore"] = new(typeof(Map21To20MapToWellbore)),
    };

    internal record MappingTypes(
        Type Map1,
        Type? Map2 = null,
        Type? Before1 = null,
        Type? Before2 = null,
        Type? After1 = null,
        Type? After2 = null);
}
