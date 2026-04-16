// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.Signature;

public class CollectionSignService
{
    private readonly ReferendumSignService _referendumSignService;
    private readonly InitiativeSignService _initiativeSignService;
    private readonly PersonInfoResolver _personInfoResolver;

    public CollectionSignService(
        ReferendumSignService referendumSignService,
        InitiativeSignService initiativeSignService,
        PersonInfoResolver personInfoResolver)
    {
        _referendumSignService = referendumSignService;
        _initiativeSignService = initiativeSignService;
        _personInfoResolver = personInfoResolver;
    }

    internal async Task ResolveSigned(
        Dictionary<DomainOfInfluenceType, List<Initiative>> initiatives,
        Dictionary<DomainOfInfluenceType, List<Decree>> referendums)
    {
        var collections = initiatives.Values.SelectMany<List<Initiative>, ICollection>(x => x)
            .Concat(referendums.Values.SelectMany(x => x).SelectMany(x => x.Referendums));
        await LoadIsSigned(collections);

        foreach (var decree in referendums.Values.SelectMany(x => x))
        {
            var isAnySigned = decree.Referendums.Any(r => r.IsSigned == true);
            foreach (var referendum in decree.Referendums.Where(r => r.IsSigned == false))
            {
                referendum.IsDecreeSigned = isAnySigned;
            }
        }
    }

    private async Task LoadIsSigned(IEnumerable<ICollection> collections)
    {
        var seenPersonInfos = new Dictionary<(DomainOfInfluenceType, string), IVotingStimmregisterPersonInfo>();
        foreach (var collection in collections)
        {
            if (!collection.DomainOfInfluenceType.HasValue || string.IsNullOrWhiteSpace(collection.Bfs))
            {
                continue;
            }

            if (!seenPersonInfos.TryGetValue((collection.DomainOfInfluenceType.Value, collection.Bfs), out var personInfo))
            {
                personInfo = await _personInfoResolver.GetPersonInfo(collection.DomainOfInfluenceType.Value, collection.Bfs, true);
                if (personInfo == null)
                {
                    continue;
                }

                seenPersonInfos[(collection.DomainOfInfluenceType.Value, collection.Bfs)] = personInfo;
            }

            (collection.IsSigned, collection.SignatureType) = collection switch
            {
                Referendum referendum => await _referendumSignService.IsCollectionSigned(referendum, personInfo),
                Initiative initiative => await _initiativeSignService.IsCollectionSigned(initiative, personInfo),
                _ => throw new InvalidOperationException($"Unsupported collection type {collection.GetType()}"),
            };
        }
    }
}
