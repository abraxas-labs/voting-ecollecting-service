// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Admin.Api.Http.Responses;
using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Api.Http.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class ResponseMapper
{
    [MapNestedProperties(nameof(CertificateValidationSummary.Info))]
    public static partial CertificateValidationSummaryResponse Map(CertificateValidationSummary result);
}
