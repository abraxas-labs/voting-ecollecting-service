// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text;
using Fennel.CSharp;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.Admin.Domain.Diagnostics;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Scheduler;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Certificates;

public class CertificateValidityCheckJobTest : BaseDbTest
{
    public CertificateValidityCheckJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    private static AuditInfo DefaultAuditInfo => new()
    {
        CreatedById = "test-user-id",
        CreatedByName = "Test User",
        CreatedAt = MockedClock.UtcNowDate,
    };

    [Fact]
    public async Task ShouldSendNotificationWhenBackupCertificateIsBelowThreshold()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();
        var backupExpiration = MockedClock.UtcNowDate.Add(config.BackupCertificateThreshold).AddDays(-1);

        await RunOnDb(async db =>
        {
            db.Certificates.RemoveRange(db.Certificates);
            await db.SaveChangesAsync();

            var file = CreateFileEntity();
            db.Files.Add(file);
            var cert = new CertificateEntity
            {
                Label = "Test Backup Notification",
                Active = true,
                ContentId = file.Id,
                Info = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), backupExpiration, "CN=Test Backup"),
                AuditInfo = DefaultAuditInfo,
            };
            db.Certificates.Add(cert);
            await db.SaveChangesAsync();
        });

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        SentUserNotifications.Should().HaveCount(1);
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.CertificateExpirationWarning)
            .ToListAsync());

        await Verify(new { SentUserNotifications, dbNotifications });
    }

    [Fact]
    public async Task ShouldSendNotificationWhenCaCertificateIsBelowThreshold()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();

        // set a high threshold to test to config CA certificate which is valid for years
        config.CACertificateThreshold = TimeSpan.FromDays(10_000);

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        SentUserNotifications.Should().HaveCount(1);
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.CertificateExpirationWarning)
            .ToListAsync());

        await Verify(new { SentUserNotifications, dbNotifications });

        // reset the threshold to avoid interfering with other tests
        config.CACertificateThreshold = TimeSpan.FromDays(60);
    }

    [Fact]
    public async Task ShouldSendNotificationWhenBothAreBelowThreshold()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();

        // set a high threshold to test to config CA certificate which is valid for years
        config.CACertificateThreshold = TimeSpan.FromDays(10_000);

        var backupExpiration = MockedClock.UtcNowDate.Add(config.BackupCertificateThreshold).AddDays(-1);

        await RunOnDb(async db =>
        {
            db.Certificates.RemoveRange(db.Certificates);
            await db.SaveChangesAsync();

            var file = CreateFileEntity();
            db.Files.Add(file);
            db.Certificates.Add(new CertificateEntity
            {
                Label = "Test Dual Certificate",
                Active = true,
                ContentId = file.Id,
                Info = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), backupExpiration, "CN=Test Backup"),
                AuditInfo = DefaultAuditInfo,
            });
            await db.SaveChangesAsync();
        });

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        SentUserNotifications.Should().HaveCount(2);
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.CertificateExpirationWarning)
            .ToListAsync());

        await Verify(new { SentUserNotifications, dbNotifications });

        // reset the threshold to avoid interfering with other tests
        config.CACertificateThreshold = TimeSpan.FromDays(60);
    }

    [Fact]
    public async Task ShouldNotSendNotificationWhenAboveThreshold()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();
        var backupExpiration = MockedClock.UtcNowDate.Add(config.BackupCertificateThreshold).AddDays(1);
        var caExpiration = MockedClock.UtcNowDate.Add(config.CACertificateThreshold).AddDays(1);

        await RunOnDb(async db =>
        {
            db.Certificates.RemoveRange(db.Certificates);
            await db.SaveChangesAsync();

            var file = CreateFileEntity();
            db.Files.Add(file);
            db.Certificates.Add(new CertificateEntity
            {
                Label = "Test Valid Certificate",
                Active = true,
                ContentId = file.Id,
                Info = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), backupExpiration, "CN=Test Backup"),
                CAInfo = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), caExpiration, "CN=Test CA"),
                AuditInfo = DefaultAuditInfo,
            });
            await db.SaveChangesAsync();
        });

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        SentUserNotifications.Should().BeEmpty();
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.CertificateExpirationWarning)
            .ToListAsync());

        dbNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotSendNotificationWhenInactive()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();
        var expirationDate = MockedClock.UtcNowDate.Add(config.BackupCertificateThreshold).AddDays(-1);

        await RunOnDb(async db =>
        {
            db.Certificates.RemoveRange(db.Certificates);
            await db.SaveChangesAsync();

            var file = CreateFileEntity();
            db.Files.Add(file);
            db.Certificates.Add(new CertificateEntity
            {
                Label = "Test Inactive Certificate",
                Active = false,
                ContentId = file.Id,
                Info = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), expirationDate, "CN=Test Backup"),
                AuditInfo = DefaultAuditInfo,
            });
            await db.SaveChangesAsync();
        });

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        SentUserNotifications.Should().BeEmpty();
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.CertificateExpirationWarning)
            .ToListAsync());

        dbNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldCreateMetrics()
    {
        var config = GetService<CertificateValidityCheckJobConfig>();
        var backupExpiration = MockedClock.UtcNowDate.Add(config.BackupCertificateThreshold).AddDays(-1);

        await RunOnDb(async db =>
        {
            db.Certificates.RemoveRange(db.Certificates);
            await db.SaveChangesAsync();

            var file = CreateFileEntity();
            db.Files.Add(file);
            var cert = new CertificateEntity
            {
                Label = "Test Backup Notification",
                Active = true,
                ContentId = file.Id,
                Info = new CertificateInfo(MockedClock.UtcNowDate.AddDays(-10), backupExpiration, "CN=Test Backup"),
                AuditInfo = DefaultAuditInfo,
            };
            db.Certificates.Add(cert);
            await db.SaveChangesAsync();
        });

        await GetService<JobRunner>().RunJob<CertificateValidityCheckJob>(CancellationToken.None);

        await using var ms = new MemoryStream();
        await Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms, CancellationToken.None);
        await ms.FlushAsync();

        var metrics = Fennel.CSharp.Prometheus.ParseText(Encoding.ASCII.GetString(ms.GetBuffer()))
            .OfType<Metric>()
            .Where(x => x.MetricName.Equals(DiagnosticsConfig.CertificateExpiryTimestampName))
            .ToList();

        metrics.Should().HaveCount(2);
    }

    private static FileEntity CreateFileEntity() => new()
    {
        Id = Guid.NewGuid(),
        Name = "test-cert.pem",
        ContentType = "application/x-pem-file",
        AuditInfo = DefaultAuditInfo,
    };
}
