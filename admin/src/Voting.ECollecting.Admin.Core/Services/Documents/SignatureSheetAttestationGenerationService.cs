// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class SignatureSheetAttestationGenerationService : ISignatureSheetAttestationGenerationService
{
    private const string DefaultInitiativeCollectionTypeName = "Volksinitiative";
    private const string DefaultReferendumCollectionTypeName = "Referendum";

    private readonly ISignatureSheetAttestationGenerator _generator;
    private readonly IInitiativeSubTypeRepository _initiativeSubTypeRepository;
    private readonly TimeProvider _timeProvider;

    public SignatureSheetAttestationGenerationService(
        ISignatureSheetAttestationGenerator generator,
        IInitiativeSubTypeRepository initiativeSubTypeRepository,
        TimeProvider timeProvider)
    {
        _generator = generator;
        _initiativeSubTypeRepository = initiativeSubTypeRepository;
        _timeProvider = timeProvider;
    }

    public async Task<IFile> GenerateFile(
        CollectionBaseEntity collection,
        DomainOfInfluenceEntity domainOfInfluence,
        ICollection<CollectionSignatureSheetEntity> signatureSheets)
    {
        string collectionTypeName;
        var referendumSecretIdNumber = string.Empty;

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
                referendumSecretIdNumber = referendum.SecureIdNumber ?? string.Empty;
                break;
            default:
                throw new InvalidOperationException($"Unexpected collection type: {collection.Type}");
        }

        var signatureSheetGroups = GetSignatureSheetGroups(signatureSheets);
        var signatureSheetCount = signatureSheets.Count;
        var validSignatureCount = signatureSheets.Sum(s => s.Count.Valid);
        var invalidSignatureCount = signatureSheets.Sum(s => s.Count.Invalid);
        var certificationDate = _timeProvider.GetUtcNowDateTime();

        var collectionPublicationDate = DateOnly.MinValue;
        if (collection.CollectionStartDate is { } date)
        {
            // For cantonal collections, the publication date is the day before the collection start date. Otherwise, it is the collection start date.
            collectionPublicationDate = collection.DomainOfInfluenceType == DomainOfInfluenceType.Ct
                ? date.AddDays(-1)
                : date;
        }

        return await _generator.GenerateFileModel(
            new SignatureSheetAttestationTemplateData(
                collection,
                domainOfInfluence,
                signatureSheetCount,
                signatureSheetGroups,
                validSignatureCount,
                invalidSignatureCount,
                certificationDate,
                collectionPublicationDate,
                collectionTypeName,
                referendumSecretIdNumber));
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
