// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

public record DomainOfInfluenceDataContainer(
    string Name,
    int ValidElectronicSignatureCount,
    int ValidPhysicalSignatureCount,
    int InvalidPhysicalSignatureCount)
{
    public List<DomainOfInfluenceDataContainer> DomainOfInfluences { get; set; } = [];
}
