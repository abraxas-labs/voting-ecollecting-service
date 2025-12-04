// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

/// <summary>
/// Domain model for field validations.
/// </summary>
public class FieldValidationErrorModel
{
    /// <summary>
    /// Gets or sets the number of the record containing the error.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a list of error messages.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
