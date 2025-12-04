// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

public record ECollectingProtocolDataContainer(
    string DomainOfInfluenceType,
    string CollectionType,
    string CollectionDescription,
    int TotalValidElectronicSignatureCount,
    int TotalValidPhysicalSignatureCount,
    int TotalInvalidPhysicalSignatureCount,
    int TotalSignatureCount,
    List<DomainOfInfluenceDataContainer> DomainOfInfluences);
