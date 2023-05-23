namespace Petrolink.WitsmlConverter;

/// <summary>
/// Describes how the WITSML document will be validated during transform.
/// </summary>
public enum WitsmlValidationMode
{
    /// <summary>
    /// Use the default WITSML validation behavior: <see cref="OnError"/>.
    /// </summary>
    Default,

    /// <summary>
    /// WITSML validation is disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// WITSML is validated only after an error. This provides more detailed error information when needed.
    /// </summary>
    OnError,

    /// <summary>
    /// WITSML is enabled for every transformation.
    /// </summary>
    Enabled
}
