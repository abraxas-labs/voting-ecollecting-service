// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Domain.Entities;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Core.Services.Signature;

public class CollectionSignService
{
    private readonly IServiceProvider _serviceProvider;

    public CollectionSignService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal Task LockAndEnsureCanSign(
        CollectionBaseEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] personCollectionMac)
    {
        return collection switch
        {
            InitiativeEntity initiative => LockAndEnsureCanSign(initiative, personInfo, personCollectionMac),
            ReferendumEntity referendum => LockAndEnsureCanSign(referendum, personInfo, personCollectionMac),
            _ => throw new ArgumentException($"Unsupported collection type {collection.Type}", nameof(collection)),
        };
    }

    internal Task LockAndEnsureCanSign(
        CollectionBaseEntity collection,
        IReadOnlySet<Guid> personRegisterIds,
        IReadOnlyList<byte[]> personCollectionMacs)
    {
        return collection switch
        {
            InitiativeEntity initiative => LockAndEnsureCanSign(initiative, personRegisterIds, personCollectionMacs),
            ReferendumEntity referendum => LockAndEnsureCanSign(referendum, personRegisterIds, personCollectionMacs),
            _ => throw new ArgumentException($"Unsupported collection type {collection.Type}", nameof(collection)),
        };
    }

    internal async Task LoadSignatureInfos(
        CollectionBaseEntity collection,
        IEnumerable<CollectionSignatureSheetCandidate> personInfos,
        CancellationToken cancellationToken)
    {
        var tasks = personInfos.Select(pi => LoadSignatureInfosInNewScope(collection, pi, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task LoadSignatureInfosInNewScope(
        CollectionBaseEntity collection,
        CollectionSignatureSheetCandidate candidate,
        CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        candidate.ExistingSignature = collection switch
        {
            InitiativeEntity initiative => await scope.ServiceProvider.GetRequiredService<InitiativeSignService>().TryGetSignature(initiative, candidate.Person, cancellationToken),
            ReferendumEntity referendum => await scope.ServiceProvider.GetRequiredService<ReferendumSignService>().TryGetSignature(referendum, candidate.Person, cancellationToken),
            _ => throw new ArgumentException($"Unsupported collection type {collection.GetType()}", nameof(collection)),
        };
    }

    private Task LockAndEnsureCanSign<T>(
        T collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] personCollectionMac)
        where T : CollectionBaseEntity
    {
        return _serviceProvider.GetRequiredService<ISignService<T>>()
            .LockAndEnsureCanSign(collection, personInfo, personCollectionMac);
    }

    private Task LockAndEnsureCanSign<T>(
        T collection,
        IReadOnlySet<Guid> personRegisterIds,
        IReadOnlyList<byte[]> personCollectionMacs)
        where T : CollectionBaseEntity
    {
        return _serviceProvider.GetRequiredService<ISignService<T>>()
            .LockAndEnsureCanSign(collection, personRegisterIds, personCollectionMacs);
    }
}
