// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Services.V1;
using Abraxas.Voting.Basis.Services.V1.Models;
using Abraxas.Voting.Basis.Services.V1.Requests;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingBasis;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Import;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Helpers;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.AclDoiTests;

public abstract class BaseAclDoiTest : BaseDbTest
{
    protected BaseAclDoiTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected Task ImportAcl(params PoliticalDomainOfInfluence[] acls)
        => ImportAcl(Enum.GetValues<Canton>().ToHashSet(), [], acls);

    protected Task ImportAcl(HashSet<Canton> allowedCantons, HashSet<string> ignoredBfs, params PoliticalDomainOfInfluence[] acls)
    {
        var mockedData = CallHelpers.CreateAsyncUnaryCall(new PoliticalDomainOfInfluenceHierarchies()
        {
            PoliticalDomainOfInfluences = { acls },
        });

        return RunScoped<IServiceProvider>(async serviceProvider =>
        {
            serviceProvider.GetRequiredService<IPermissionService>().SetAbraxasAuthIfNotAuthenticated();
            var importer = BuildAclImporterWithMockedGrpcClient(mockedData, serviceProvider);
            await importer.ImportAcl(allowedCantons, ignoredBfs);
        });
    }

    private AccessControlListImporter BuildAclImporterWithMockedGrpcClient(
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

        return new AccessControlListImporter(
            serviceProvider.GetRequiredService<TimeProvider>(),
            serviceProvider.GetRequiredService<IAccessControlListDoiRepository>(),
            serviceProvider.GetRequiredService<IImportStatisticRepository>(),
            new VotingBasisAdapter(mockedGrpcClient.Object),
            serviceProvider.GetRequiredService<ILogger<AccessControlListImporter>>(),
            serviceProvider.GetRequiredService<IPermissionService>(),
            serviceProvider.GetRequiredService<IDataContext>(),
            serviceProvider.GetRequiredService<ImportConfig>());
    }
}
