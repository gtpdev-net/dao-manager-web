namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a logged event that occurred during a scan operation.
/// </summary>
/// <remarks>
/// Scan events provide an audit trail of the scanning process, including progress updates,
/// warnings, errors, and informational messages for debugging and monitoring purposes.
/// </remarks>
public class ScanEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the scan event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent <see cref="Entities.Scan"/>.
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the phase of the scan process when the event occurred
    /// (e.g., "Discovery", "Analysis", "Processing").
    /// </summary>
    public string Phase { get; set; } = null!;

    /// <summary>
    /// Gets or sets the descriptive message or details about the event.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the parent scan that generated this event.
    /// </summary>
    public Scan Scan { get; set; } = null!;
}
