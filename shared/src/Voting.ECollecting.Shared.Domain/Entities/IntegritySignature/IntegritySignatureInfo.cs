// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

public class IntegritySignatureInfo
{
    /// <summary>
    /// Gets or sets the version of the signature.
    /// </summary>
    public byte IntegritySignatureVersion { get; set; }

    /// <summary>
    /// Gets or sets the key id of the signature.
    /// </summary>
    public string IntegritySignatureKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the signature.
    /// </summary>
    public byte[] IntegritySignature { get; set; } = [];
}
