// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

public interface IIntegritySignatureEntity
{
    IntegritySignatureInfo IntegritySignatureInfo { get; set; }
}
