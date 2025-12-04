// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using DomainOfInfluenceCanton = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceCanton;
using DomainOfInfluenceType = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.AclDoiTests;

public class AclDoiImportTest : BaseAclDoiTest
{
    public AclDoiImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldImportAclsStGallenAndThurgau()
    {
        await ImportAcl(
            AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsStGallenOnly()
    {
        await ImportAcl(
            [Canton.SG],
            [],
            AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsIncrementally()
    {
        // Incremental import 1
        var import1 = AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        import1.Children.Clear();
        await ImportAcl(import1);

        // Incremental import 2
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Incremental import 3 (full)
        await ImportAcl(
            AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndDeleteStGallenRootTree()
    {
        // Full import
        await ImportAcl(
            AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH,
            AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        // Delete StGallenRootTree
        await ImportAcl(AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndListStatistics()
    {
        // Full import SG_Kanton_StGallen_L1_CH
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Prepare import:
        //  > Add TG_Kanton_Thurgau_L1_CH with all children (2 entities)
        //  > Update SG_Kanton_StGallen_L1_CH name attribute (1 entity)
        //  > Delete SG_Kanton_StGallen_L2_CT subtree (4 entities)
        var sgKantonStGallenL1CH = AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        sgKantonStGallenL1CH.Name = $"{sgKantonStGallenL1CH.Name} (updated)";
        sgKantonStGallenL1CH.Children.Remove(sgKantonStGallenL1CH.Children.First(e => e.Id == AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L2_CT.Id));
        await ImportAcl(sgKantonStGallenL1CH, AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IServiceProvider, IEnumerable<ImportStatisticEntity>>(async s =>
        {
            var statisticsRepo = s.GetRequiredService<IImportStatisticRepository>();
            var acldoiRepo = s.GetRequiredService<IAccessControlListDoiRepository>();

            var statistics = await statisticsRepo
                .Query()
                .Where(e => e.ImportType == ImportType.Acl && e.SourceSystem == ImportSourceSystem.VotingBasis)
                .ToListAsync();

            var statisticIds = statistics.ConvertAll(stat => stat.Id);

            var acls = await acldoiRepo
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
        await ImportAcl(AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        // Update information
        var doi = AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        doi.Name = $"{AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH.Name} (updated)";
        doi.Bfs = "9999";
        doi.TenantName = $"{doi.TenantName} (updated)";
        doi.TenantId = $"{doi.TenantId} (updated)";
        doi.Type = DomainOfInfluenceType.Mu;
        doi.Canton = DomainOfInfluenceCanton.Tg;

        await ImportAcl(doi);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndUpdateAddressInformation()
    {
        // Full import
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH);

        // Update information
        var doi = AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH;
        doi.ReturnAddress.AddressLine1 = "Staatskanzlei St. Gallen (updated)";

        await ImportAcl(doi);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        await Verify(result);
    }

    [Fact]
    public async Task ShouldImportAclsAndIgnoreRootsWithoutECollectingEnabled()
    {
        // Full import
        var tg = AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        tg.ECollectingEnabled = false;
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

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
        var tg = AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH;
        var tgId = Guid.Parse(tg.Id);
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var tgExists = await RunScoped<IAccessControlListDoiRepository, bool>(async r =>
            await r.Query().AnyAsync(x => x.Id == tgId));
        tgExists.Should().BeTrue();

        // run incremental update
        tg.ECollectingEnabled = false;
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, tg);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

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
        var tgAuslandschweizer = AclDoiVotingBasisMockedData.TG_Auslandschweizer_L2_MU;
        var tgAuslandschweizerId = Guid.Parse(tgAuslandschweizer.Id);
        var sgAuslandschweizer = AclDoiVotingBasisMockedData.SG_Auslandschweizer_L2_MU;
        var sgAuslandschweizerId = Guid.Parse(sgAuslandschweizer.Id);
        await ImportAcl(AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var sgExists = await RunScoped<IAccessControlListDoiRepository, bool>(async r =>
            await r.Query().AnyAsync(x => x.Id == sgAuslandschweizerId));
        sgExists.Should().BeTrue();

        var tgExists = await RunScoped<IAccessControlListDoiRepository, bool>(async r =>
            await r.Query().AnyAsync(x => x.Id == tgAuslandschweizerId));
        tgExists.Should().BeTrue();

        // run incremental update
        await ImportAcl([Canton.SG, Canton.TG], [tgAuslandschweizer.Bfs, sgAuslandschweizer.Bfs], AclDoiVotingBasisMockedData.SG_Kanton_StGallen_L1_CH, AclDoiVotingBasisMockedData.TG_Kanton_Thurgau_L1_CH);

        var result = await RunScoped<IAccessControlListDoiRepository, IEnumerable<AccessControlListDoiEntity>>(async r =>
            await r.Query().OrderBy(a => a.Id).ToListAsync());

        // auslandschweizer should not be imported as we set ecollecting enabled = false
        result.Any(x => x.Id == tgAuslandschweizerId || x.Id == sgAuslandschweizerId)
            .Should()
            .BeFalse();

        await Verify(result);
    }
}
