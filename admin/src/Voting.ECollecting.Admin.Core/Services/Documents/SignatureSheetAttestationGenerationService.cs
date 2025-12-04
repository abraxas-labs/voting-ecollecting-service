// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class SignatureSheetAttestationGenerationService : ISignatureSheetAttestationGenerationService
{
    private const string DefaultInitiativeCollectionTypeName = "Volksinitiative";
    private const string DefaultReferendumCollectionTypeName = "Referendum";

    private readonly ISignatureSheetAttestationGenerator _generator;
    private readonly IInitiativeSubTypeRepository _initiativeSubTypeRepository;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IPermissionService _permissionService;
    private readonly TimeProvider _timeProvider;

    public SignatureSheetAttestationGenerationService(
        ISignatureSheetAttestationGenerator generator,
        IInitiativeSubTypeRepository initiativeSubTypeRepository,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IPermissionService permissionService,
        TimeProvider timeProvider)
    {
        _generator = generator;
        _initiativeSubTypeRepository = initiativeSubTypeRepository;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
    }

    public async Task<Stream> GenerateFile(
        CollectionBaseEntity collection,
        AccessControlListDoiEntity aclDoi,
        ICollection<CollectionSignatureSheetEntity> signatureSheets)
    {
        string collectionTypeName;
        var referendumNumber = string.Empty;

        switch (collection)
        {
            case InitiativeEntity initiative:
                collectionTypeName = initiative.SubTypeId is { } subTypeId
                    ? await _initiativeSubTypeRepository.Query()
                        .Where(x => x.Id == subTypeId)
                        .Select(x => x.Description)
                        .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(nameof(InitiativeSubTypeEntity), subTypeId)
                    : DefaultInitiativeCollectionTypeName;
                break;
            case ReferendumEntity referendum:
                collectionTypeName = DefaultReferendumCollectionTypeName;
                referendumNumber = referendum.Number;
                break;
            default:
                throw new InvalidOperationException($"Unexpected collection type: {collection.Type}");
        }

        var doi = await _domainOfInfluenceRepository.Query()
                      .Include(x => x.Logo!.Content)
                      .WhereCanAccessOwnBfsOrChildren(_permissionService)
                      .FirstOrDefaultAsync(x => x.Bfs == aclDoi.Bfs)
                  ?? throw new AddressMissingForAttestException();
        var signatureSheetGroups = GetSignatureSheetGroups(signatureSheets);
        var signatureSheetCount = signatureSheets.Count;
        var validSignatureCount = signatureSheets.Sum(s => s.Count.Valid);
        var invalidSignatureCount = signatureSheets.Sum(s => s.Count.Invalid);
        var certificationDate = _timeProvider.GetUtcNowDateTime();

        var collectionPublicationDate = DateTime.MinValue;
        if (collection.CollectionStartDate?.Date is { } date)
        {
            // For cantonal collections, the publication date is the day before the collection start date. Otherwise, it is the collection start date.
            collectionPublicationDate = collection.DomainOfInfluenceType == DomainOfInfluenceType.Ct
                ? date.AddDays(-1)
                : date;
        }

        return await _generator.Generate(
            new SignatureSheetAttestationTemplateData(
                collection,
                aclDoi,
                doi,
                signatureSheetCount,
                signatureSheetGroups,
                validSignatureCount,
                invalidSignatureCount,
                certificationDate,
                collectionPublicationDate,
                collectionTypeName,
                referendumNumber));
    }

    // Group signature sheets into unbroken sequences by their Number
    private List<SignatureSheetListDataContainer> GetSignatureSheetGroups(
        IEnumerable<CollectionSignatureSheetEntity> signatureSheets)
    {
        var orderedSheets = signatureSheets
            .OrderBy(s => s.Number)
            .Select(s => s.Number)
            .ToArray();
        var signatureList = new List<SignatureSheetListDataContainer>();

        if (orderedSheets.Length == 0)
        {
            return signatureList;
        }

        var start = orderedSheets[0];
        var end = start;

        for (var i = 1; i < orderedSheets.Length; i++)
        {
            var current = orderedSheets[i];
            if (current != end + 1)
            {
                signatureList.Add(new SignatureSheetListDataContainer(start, end));
                start = current;
            }

            end = current;
        }

        signatureList.Add(new SignatureSheetListDataContainer(start, end));
        return signatureList;
    }
}
