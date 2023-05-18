using System.Xml;

namespace Petrolink.WitsmlConverter;

/// <summary>
/// Provides options for WITSML transformation.
/// </summary>
public class WitsmlTransformOptions
{
    /// <summary>
    /// Initializes a new instance with default options.
    /// </summary>
    public WitsmlTransformOptions() { }

    /// <summary>
    /// Gets the XML writer settings. If null, default settings will be used.
    /// </summary>
    public XmlWriterSettings? XmlWriterSettings { get; init; }

    /// <summary>
    /// Gets whether to add creation timestamps when converting from WITSML 1.4 to 2.0. If null, defaults to true.
    /// </summary>
    public bool? AddCreationTimes { get; init; }

    /// <summary>
    /// Gets the creation time to be used when <see cref="AddCreationTimes"/> is true.
    /// If null, then <see cref="DateTime.UtcNow"/> will be used when the transformation is executed.
    /// </summary>
    public DateTime? CreationTime { get; init; }

    /// <summary>
    /// Gets whether to convert units when converting from WITSML 1.4 to 2.0 or from 2.0 to 1.4.
    /// If null, defaults to true.
    /// </summary>
    public bool? ConvertUnits { get; init; }
}
