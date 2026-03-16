// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IInitiativeCommitteeListService
{
    Task<FileEntity> AddCommitteeList(Guid initiativeId, Stream file, string? contentType, string? fileName, CancellationToken ct);

    Task AcceptCommitteeMembershipWithCommitteeList(Guid initiativeId, UrlToken token, Stream file, string contentType, string fileName, CancellationToken ct);

    Task<FileEntity> GetCommitteeList(Guid initiativeId, Guid fileId);

    Task<IFile> GetCommitteeListTemplate(Guid initiativeId, CancellationToken ct);

    Task<IFile> GetCommitteeListTemplateForMemberByToken(Guid initiativeId, UrlToken token, CancellationToken ct);

    Task DeleteCommitteeList(Guid initiativeId, Guid listId);
}
