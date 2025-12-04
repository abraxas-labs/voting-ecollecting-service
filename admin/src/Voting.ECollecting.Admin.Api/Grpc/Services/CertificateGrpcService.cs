// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Grpc.Mappings;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;

namespace Voting.ECollecting.Admin.Api.Grpc.Services;

[Zertifikatsverwalter]
public class CertificateGrpcService : CertificateService.CertificateServiceBase
{
    private readonly ICertificateService _certificateService;

    public CertificateGrpcService(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    public override async Task<GetActiveCertificateResponse> GetActive(
        GetActiveCertificateRequest request,
        ServerCallContext context)
    {
        var cert = await _certificateService.GetActive();
        return Mapper.MapToActiveCertificateResponse(cert);
    }

    public override async Task<ListCertificatesResponse> List(
        ListCertificatesRequest request,
        ServerCallContext context)
    {
        var certs = await _certificateService.List();
        return Mapper.MapToListCertificatesResponse(certs);
    }
}
