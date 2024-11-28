namespace Sub_App_1.Models;

/// <summary>
/// Represents a model for handling error information in the application.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the request ID associated with the error.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="RequestId"/> should be displayed.
    /// </summary>
    /// <returns>True if <see cref="RequestId"/> is not null or empty; otherwise, false.</returns>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
