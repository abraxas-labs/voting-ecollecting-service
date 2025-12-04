// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IPermissionService
{
    string UserId { get; }

    string? UserEmail { get; }
}
