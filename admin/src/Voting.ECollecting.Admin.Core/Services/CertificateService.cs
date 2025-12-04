// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class CertificateService : ICertificateService
{
    private readonly CoreAppConfig _config;
    private readonly ILogger<CertificateService> _logger;
    private readonly ICertificateRepository _certificateRepository;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;
    private readonly CertificateValidator _validator;
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;

    public CertificateService(
        CoreAppConfig config,
        ILogger<CertificateService> logger,
        ICertificateRepository certificateRepository,
        IPermissionService permissionService,
        IDataContext dataContext,
        CertificateValidator validator,
        IAccessControlListDoiRepository accessControlListDoiRepository)
    {
        _config = config;
        _logger = logger;
        _certificateRepository = certificateRepository;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _validator = validator;
        _accessControlListDoiRepository = accessControlListDoiRepository;
    }

    public async Task<ActiveCertificate> GetActive()
    {
        await EnsureIsCtOrChTenant();
        var cert = await _certificateRepository.Query()
                   .Where(x => x.Active)
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CertificateEntity), new { Active = true });
        return new ActiveCertificate(cert, new CertificateInfo(GetCertificateAuthorityCertificate()));
    }

    public async Task<IReadOnlyList<CertificateEntity>> List()
    {
        await EnsureIsCtOrChTenant();
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
        await EnsureIsCtOrChTenant();
        return await _validator.ValidateBackupCertificate(
            GetCertificateAuthorityCertificate(),
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
        await EnsureIsCtOrChTenant();
        await using var transaction = await _dataContext.BeginTransaction(ct);

        var validationResult = await _validator.ValidateBackupCertificate(GetCertificateAuthorityCertificate(), stream, contentType, fileName, ct);
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

    private X509Certificate2 GetCertificateAuthorityCertificate()
    {
        var cert = X509Certificate2.CreateFromPem(_config.BackupCertificate.CACertificate);
        if (!cert.HasPrivateKey)
        {
            return cert;
        }

        _logger.LogCritical(SecurityLogging.SecurityEventId, "Private key in backup ca certificate detected");
        throw new InvalidOperationException("Private key in backup ca certificate detected");
    }

    private async Task EnsureIsCtOrChTenant()
    {
        var isChOrCtTenant = await _accessControlListDoiRepository.Query()
            .AnyAsync(x =>
                x.TenantId == _permissionService.TenantId &&
                (x.Type == AclDomainOfInfluenceType.Ct || x.Type == AclDomainOfInfluenceType.Ch));
        if (!isChOrCtTenant)
        {
            throw new ForbiddenException("Only tenants of DOIs of type CT or CH are allowed to perform this operation.");
        }
    }
}
