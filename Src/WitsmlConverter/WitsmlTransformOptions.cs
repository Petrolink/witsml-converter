using System.Xml;
using System.Xml.Schema;

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
    public XmlWriterSettings? XmlWriterSettings { get; set; }

    /// <summary>
    /// Gets whether to add creation timestamps when converting from WITSML 1.4 to 2.0. If null, defaults to true.
    /// </summary>
    public bool? AddCreationTimes { get; set; }

    /// <summary>
    /// Gets the creation time to be used when <see cref="AddCreationTimes"/> is true.
    /// If null, then <see cref="DateTime.UtcNow"/> will be used when the transformation is executed.
    /// </summary>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// Gets whether to convert units when converting from WITSML 1.4 to 2.0 or from 2.0 to 1.4.
    /// If null, defaults to true.
    /// </summary>
    public bool? ConvertUnits { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="WitsmlValidationMode"/> to be used for input documents.
    /// </summary>
    public WitsmlValidationMode ValidationMode { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="XmlSchemaSet"/> to use when validating input documents. Required when
    /// <see cref="ValidationMode"/> is <see cref="WitsmlValidationMode.Enabled"/>.
    /// </summary>
    public XmlSchemaSet? SchemaSet { get; set; }
}
