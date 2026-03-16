// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Adapter.VotingBasis;

[Mapper]
internal static partial class Mapper
{
    [MapEnum(EnumMappingStrategy.ByName, FallbackValue = BasisDomainOfInfluenceType.Unspecified)]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial BasisDomainOfInfluenceType MapToBasisDomainOfInfluenceType(Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType doiType);

    [MapEnum(EnumMappingStrategy.ByName, FallbackValue = DomainOfInfluenceType.Unspecified)]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainOfInfluenceType MapToDomainOfInfluenceType(Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType doiType);
}
