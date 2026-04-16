// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class CertificateService : ICertificateService
{
    private readonly ICaCertificateService _caCertificateService;
    private readonly ICertificateRepository _certificateRepository;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;
    private readonly CertificateFileValidator _validator;
    private readonly IAccessControlListService _accessControlListService;

    public CertificateService(
        ICaCertificateService caCertificateService,
        ICertificateRepository certificateRepository,
        IPermissionService permissionService,
        IDataContext dataContext,
        CertificateFileValidator validator,
        IAccessControlListService accessControlListService)
    {
        _caCertificateService = caCertificateService;
        _certificateRepository = certificateRepository;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _validator = validator;
        _accessControlListService = accessControlListService;
    }

    public async Task<ActiveCertificate> GetActive()
    {
        await _accessControlListService.EnsureIsCtOrChTenant();
        var cert = await _certificateRepository.Query()
                   .Where(x => x.Active)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CertificateEntity), new { Active = true });
        using var caCert = _caCertificateService.GetCertificateAuthorityCertificate();
        return new ActiveCertificate(cert, new CertificateInfo(caCert));
    }

    public async Task<IReadOnlyList<CertificateEntity>> List()
    {
        await _accessControlListService.EnsureIsCtOrChTenant();
        return await _certificateRepository
            .Query()
            .Where(x => !x.Active)
            .OrderByDescending(x => x.AuditInfo.CreatedAt)
            .ToListAsync();
    }

    public async Task<CertificateValidationSummary> ValidateBackupCertificate(
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        await _accessControlListService.EnsureIsCtOrChTenant();
        using var caCert = _caCertificateService.GetCertificateAuthorityCertificate();

        return await _validator.ValidateBackupCertificate(
            caCert,
            stream,
            contentType,
            fileName,
            ct);
    }

    public async Task SetBackupCertificate(
        string? label,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        await _accessControlListService.EnsureIsCtOrChTenant();
        await using var transaction = await _dataContext.BeginTransaction(ct);

        using var caCert = _caCertificateService.GetCertificateAuthorityCertificate();
        var validationResult = await _validator.ValidateBackupCertificate(caCert, stream, contentType, fileName, ct);
        if (validationResult.State == CertificateValidationState.Error)
        {
            throw new ValidationException("Certificate validation error");
        }

        await _certificateRepository.AuditedUpdateRange(
            q => q.Where(x => x.Active),
            x => x.Active = false);

        var entity = new CertificateEntity
        {
            CAInfo = validationResult.CAInfo,
            Info = validationResult.Info,
            Label = label,
            Content = validationResult.Content,
            Active = true,
        };
        _permissionService.SetCreated(entity);
        _permissionService.SetCreated(entity.Content!);
        await _certificateRepository.Create(entity);

        await transaction.CommitAsync(ct);
    }
}
