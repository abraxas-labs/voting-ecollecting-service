// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Services.V1;
using Abraxas.Voting.Basis.Services.V1.Models;
using Abraxas.Voting.Basis.Services.V1.Requests;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingBasis;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Import;
using Voting.ECollecting.Admin.WebService.Integration.Tests.DoiTests;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Helpers;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using DomainOfInfluenceCanton = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceCanton;
using DomainOfInfluenceType = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceVotingBasisTests;

public class DoiVotingBasisImportTest : BaseDbTest
{
    public DoiVotingBasisImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldImportStGallenAndThurgau()
    {
        await Import(
            DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportStGallenOnly()
    {
        await Import(
            [Canton.SG],
            [],
            DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportIncrementally()
    {
        // Incremental import 1
        var import1 = DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        import1.Children.Clear();
        await Import(import1);

        // Incremental import 2
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Incremental import 3 (full)
        await Import(
            DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndDeleteStGallenRootTree()
    {
        // Full import
        await Import(
            DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        // Delete StGallenRootTree
        await Import(DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAndListStatistics()
    {
        // Full import SG_Kanton_StGallen_L1_CH
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Prepare import:
        //  > Add TG_Kanton_Thurgau_L1_CH with all children (2 entities)
        //  > Update SG_Kanton_StGallen_L1_CH name attribute (1 entity)
        //  > Delete SG_Kanton_StGallen_L2_CT subtree (4 entities)
        var sgKantonStGallenL1CH = DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        sgKantonStGallenL1CH.Name = $"{sgKantonStGallenL1CH.Name} (updated)";
        sgKantonStGallenL1CH.Children.Remove(sgKantonStGallenL1CH.Children.First(e => e.Id == DoiVotingBasisMockedData.SG_Kanton_StGallen_L2_CT.Id));
        await Import(sgKantonStGallenL1CH, DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IServiceProvider, IEnumerable<ImportStatisticEntity>>(async s =>
        {
            var statisticsRepo = s.GetRequiredService<IImportStatisticRepository>();
            var doiRepo = s.GetRequiredService<IDomainOfInfluenceRepository>();

            var statistics = await statisticsRepo
                .Query()
                .Where(e => e.ImportType == ImportType.DomainOfInfluences && e.SourceSystem == ImportSourceSystem.VotingBasis)
                .ToListAsync();

            var statisticIds = statistics.ConvertAll(stat => stat.Id);

            var acls = await doiRepo
                .Query()
                .OrderBy(a => a.Id)
                .Where(acl => acl.ImportStatisticId.HasValue && statisticIds.Contains(acl.ImportStatisticId!.Value))
                .ToListAsync();

            acls.Should().HaveCount(4);
            return statistics;
        });

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndUpdateInformation()
    {
        // Full import
        await Import(DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        // Update information
        var doi = DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        doi.Name = $"{DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH.Name} (updated)";
        doi.Bfs = "9999";
        doi.TenantName = $"{doi.TenantName} (updated)";
        doi.TenantId = $"{doi.TenantId} (updated)";
        doi.Type = DomainOfInfluenceType.Mu;
        doi.Canton = DomainOfInfluenceCanton.Tg;

        await Import(doi);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndUpdateAddressInformation()
    {
        // Full import
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Update information
        var doi = DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        doi.ReturnAddress.AddressLine1 = "Staatskanzlei St. Gallen (updated)";

        await Import(doi);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());
        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndIgnoreRootsWithoutECollectingEnabled()
    {
        // Full import
        var tg = DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        tg.ECollectingEnabled = false;
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());

        // tg should not be imported as we set ecollecting enabled = false
        result.Any(x => x.Bfs == tg.Bfs)
            .Should()
            .BeFalse();

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndRemoveRootsWithoutECollectingEnabledInUpdate()
    {
        // Full import
        var tg = DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        var tgId = Guid.Parse(tg.Id);
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var tgExists = await RunOnDb(db => db.DomainOfInfluences.AnyAsync(x => x.Id == tgId));
        tgExists.Should().BeTrue();

        // run incremental update
        tg.ECollectingEnabled = false;
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());

        // tg should not be imported as we set ecollecting enabled = false
        result.Any(x => x.Id == tgId)
            .Should()
            .BeFalse();

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndRemoveIgnoredBfsInUpdate()
    {
        // Full import
        var tgAuslandschweizer = DoiVotingBasisMockedData.TG_Auslandschweizer_L2_MU;
        var tgAuslandschweizerId = Guid.Parse(tgAuslandschweizer.Id);
        var sgAuslandschweizer = DoiVotingBasisMockedData.SG_Auslandschweizer_L2_MU;
        var sgAuslandschweizerId = Guid.Parse(sgAuslandschweizer.Id);
        await Import(DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var sgExists = await RunOnDb(db => db.DomainOfInfluences.AnyAsync(x => x.Id == sgAuslandschweizerId));
        sgExists.Should().BeTrue();

        var tgExists = await RunOnDb(db => db.DomainOfInfluences.AnyAsync(x => x.Id == tgAuslandschweizerId));
        tgExists.Should().BeTrue();

        // run incremental update
        await Import([Canton.SG, Canton.TG], [tgAuslandschweizer.Bfs, sgAuslandschweizer.Bfs], DoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, DoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunOnDb(db => db.DomainOfInfluences.OrderBy(a => a.Id).ToListAsync());

        // auslandschweizer should not be imported as we set ecollecting enabled = false
        result.Any(x => x.Id == tgAuslandschweizerId || x.Id == sgAuslandschweizerId)
            .Should()
            .BeFalse();

        await Verify(result);
    }

    private Task Import(params PoliticalDomainOfInfluence[] acls)
        => Import(Enum.GetValues<Canton>().ToHashSet(), [], acls);

    private Task Import(HashSet<Canton> allowedCantons, HashSet<string> ignoredBfs, params PoliticalDomainOfInfluence[] acls)
    {
        var mockedData = CallHelpers.CreateAsyncUnaryCall(new PoliticalDomainOfInfluenceHierarchies()
        {
            PoliticalDomainOfInfluences = { acls },
        });

        return RunScoped<IServiceProvider>(async serviceProvider =>
        {
            serviceProvider.GetRequiredService<IPermissionService>().SetAbraxasAuthIfNotAuthenticated();
            var importer = BuildAclImporterWithMockedGrpcClient(mockedData, serviceProvider);
            await importer.ImportDomainOfInfluences(allowedCantons, ignoredBfs);
        });
    }

    private DomainOfInfluenceImporter BuildAclImporterWithMockedGrpcClient(
        AsyncUnaryCall<PoliticalDomainOfInfluenceHierarchies> mockedGrpcData,
        IServiceProvider serviceProvider)
    {
        var mockedGrpcClient = new Mock<AdminManagementService.AdminManagementServiceClient>();
        mockedGrpcClient
            .Setup(m => m.GetPoliticalDomainOfInfluenceHierarchyAsync(
                It.IsAny<GetPoliticalDomainOfInfluenceHierarchyRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(mockedGrpcData);

        return new DomainOfInfluenceImporter(
            serviceProvider.GetRequiredService<TimeProvider>(),
            serviceProvider.GetRequiredService<IDomainOfInfluenceRepository>(),
            serviceProvider.GetRequiredService<IImportStatisticRepository>(),
            new VotingBasisAdapter(mockedGrpcClient.Object),
            serviceProvider.GetRequiredService<ILogger<DomainOfInfluenceImporter>>(),
            serviceProvider.GetRequiredService<IPermissionService>(),
            serviceProvider.GetRequiredService<IDataContext>(),
            serviceProvider.GetRequiredService<ImportConfig>());
    }
}
