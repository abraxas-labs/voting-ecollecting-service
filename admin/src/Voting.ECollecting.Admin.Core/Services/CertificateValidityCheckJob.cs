// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Diagnostics;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class CertificateValidityCheckJob : IScheduledJob
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly IUserNotificationService _userNotificationService;
    private readonly CertificateValidityCheckJobConfig _config;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CertificateValidityCheckJob> _logger;
    private readonly ICaCertificateService _caCertificateService;

    public CertificateValidityCheckJob(
        ICertificateRepository certificateRepository,
        IUserNotificationService userNotificationService,
        CertificateValidityCheckJobConfig config,
        TimeProvider timeProvider,
        ILogger<CertificateValidityCheckJob> logger,
        ICaCertificateService caCertificateService)
    {
        _certificateRepository = certificateRepository;
        _userNotificationService = userNotificationService;
        _config = config;
        _timeProvider = timeProvider;
        _logger = logger;
        _caCertificateService = caCertificateService;
    }

    public async Task Run(CancellationToken ct)
    {
        var activeCertificate = await _certificateRepository.Query().FirstOrDefaultAsync(x => x.Active, ct);
        if (activeCertificate?.Info != null)
        {
            await CheckExpiration(activeCertificate.Info, false, _config.BackupCertificateThreshold, ct);
        }

        var caCertificate = new CertificateInfo(_caCertificateService.GetCertificateAuthorityCertificate());
        await CheckExpiration(caCertificate, true, _config.CACertificateThreshold, ct);
    }

    private async Task CheckExpiration(
        CertificateInfo certificateInfo,
        bool isCaCertificate,
        TimeSpan warningThreshold,
        CancellationToken ct)
    {
        var expirationDate = certificateInfo.NotAfter;
        var now = _timeProvider.GetUtcNowDateTime();
        var remainingDays = (expirationDate - now).TotalDays;
        var certificateType = isCaCertificate ? "CA" : "Backup";
        DiagnosticsConfig.UpdateCertificateExpiryTimestamp(certificateType, expirationDate);

        if (remainingDays <= warningThreshold.TotalDays)
        {
            _logger.LogWarning("{Type} certificate expires on {ExpirationDate}.", certificateType, expirationDate);
            await _userNotificationService.SendUserNotifications(
                _config.NotificationEmails,
                recipientsAreCitizen: false,
                UserNotificationType.CertificateExpirationWarning,
                new UserNotificationContext(
                    CertificateExpirationDate: expirationDate,
                    IsCaCertificate: isCaCertificate),
                cancellationToken: ct);
        }

        if (expirationDate < now)
        {
            _logger.LogCritical("{Type} certificate is expired.", certificateType);
        }
    }
}
