// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public class CollectionCryptoKeyGenerateResult
{
    public Guid Id { get; set; }

    public string EncryptionKeyId { get; set; } = string.Empty;

    public string MacKeyId { get; set; } = string.Empty;

    public bool Success => !string.IsNullOrWhiteSpace(EncryptionKeyId) && !string.IsNullOrWhiteSpace(MacKeyId);
}
