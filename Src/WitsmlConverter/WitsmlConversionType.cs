namespace Petrolink.WitsmlConverter;

/// <summary>
/// Describes the type of WITSML conversion
/// </summary>
public enum WitsmlConversionType
{
    /// <summary>
    /// A conversion from WITSML 1.4 to WITSML 2.0
    /// </summary>
    Witsml14To20,

    /// <summary>
    /// A conversion from WITSML 2.0 to WITSML 1.4
    /// </summary>
    Witsml20To14,

    /// <summary>
    /// A conversion from WITSML 2.0 to WITSML 2.1
    /// </summary>
    Witsml20To21,

    /// <summary>
    /// A conversion form WITSML 2.1 to WITSML 2.0
    /// </summary>
    Witsml21To20,
}
