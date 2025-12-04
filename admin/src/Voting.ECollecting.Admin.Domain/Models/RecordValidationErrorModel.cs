// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

/// <summary>
/// Domain model for record validations.
/// </summary>
public class RecordValidationErrorModel
{
    /// <summary>
    /// Gets or sets the number of the record containing the error.
    /// </summary>
    public int RecordNumber { get; set; }

    /// <summary>
    /// Gets or sets the record identifier.
    /// </summary>
    public string RecordIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a list of fields with errors.
    /// </summary>
    public List<FieldValidationErrorModel> Fields { get; set; } = new();
}
