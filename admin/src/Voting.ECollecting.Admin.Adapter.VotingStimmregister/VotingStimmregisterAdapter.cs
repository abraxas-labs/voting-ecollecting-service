// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.Lib.Database.Models;
using Voting.Stimmregister.Proto.V1.Services;
using Voting.Stimmregister.Proto.V1.Services.Models;
using Voting.Stimmregister.Proto.V1.Services.Requests;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister;

public class VotingStimmregisterAdapter : IVotingStimmregisterAdapter
{
    private static readonly Pageable _singleItemPageable = new() { Page = 1, PageSize = 1 };

    private readonly EcollectingService.EcollectingServiceClient _client;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<VotingStimmregisterAdapter> _logger;

    public VotingStimmregisterAdapter(
        EcollectingService.EcollectingServiceClient client,
        TimeProvider timeProvider,
        ILogger<VotingStimmregisterAdapter> logger)
    {
        _client = client;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Page<IVotingStimmregisterPersonInfo>> ListPersonInfos(
        VotingStimmregisterPersonFilterData filterData,
        Pageable? pageable = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = BuildGetByNameRequest(filterData, pageable);
            var response = await _client.EcollectingServiceGetPeopleByNameAsync(req, cancellationToken: cancellationToken);
            return ResponseMapper.MapToPage(response);
        }
        catch (RpcException e) when (e.StatusCode is StatusCode.Unauthenticated or StatusCode.PermissionDenied)
        {
            throw new NoPermissionStimmregisterException();
        }
    }

    public async Task<IVotingStimmregisterPersonInfo> GetPersonInfo(VotingStimmregisterPersonFilterData filterData, CancellationToken cancellationToken = default)
    {
        try
        {
            var people = await ListPersonInfos(filterData, _singleItemPageable, cancellationToken);
            return people.TotalItemsCount == 1
                ? people.Items[0]
                : throw new PersonNotFoundException();
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Could not fetch persons from Stimmregister, invalid request");
            throw new PersonNotFoundException();
        }
    }

    public async Task<IVotingStimmregisterPersonInfo> GetPersonInfo(
        string bfs,
        Guid registerId,
        DateTime actualityDate,
        CancellationToken cancellationToken = default)
    {
        var ids = new HashSet<Guid> { registerId };
        var people = await GetPersonInfos(bfs, ids, actualityDate, cancellationToken);
        return people[0];
    }

    public async Task<IReadOnlyList<IVotingStimmregisterPersonInfo>> GetPersonInfos(
        string bfs,
        IReadOnlySet<Guid> registerIds,
        DateTime actualityDate,
        CancellationToken cancellationToken = default)
    {
        var req = new EcollectingServiceGetPeopleByIdsRequest
        {
            MunicipalityId = int.Parse(bfs),
            ActualityDate = Timestamp.FromDateTime(actualityDate),
            RegisterIds = { registerIds.Select(id => id.ToString()) },
        };

        try
        {
            var resp = await _client.EcollectingServiceGetPeopleByIdsAsync(req, cancellationToken: cancellationToken);
            if (resp.People.Count != registerIds.Count)
            {
                throw new PersonNotFoundException();
            }

            return ResponseMapper.MapToList(resp);
        }
        catch (RpcException e) when (e.StatusCode is StatusCode.Unauthenticated or StatusCode.PermissionDenied)
        {
            throw new NoPermissionStimmregisterException();
        }
    }

    private EcollectingServiceGetPeopleByNameRequest BuildGetByNameRequest(
        VotingStimmregisterPersonFilterData filter,
        Pageable? pageable = null)
    {
        if (!int.TryParse(filter.Bfs, out var bfsInt))
        {
            throw new ValidationException(
                $"Could not fetch persons from Stimmregister, invalid BFS number {filter.Bfs}");
        }

        return new EcollectingServiceGetPeopleByNameRequest
        {
            MunicipalityId = bfsInt,
            OfficialName = filter.OfficialName ?? string.Empty,
            FirstName = filter.FirstName ?? string.Empty,
            AddressHouseNumber = filter.ResidenceAddressHouseNumber ?? string.Empty,
            AddressStreet = filter.ResidenceAddressStreet ?? string.Empty,
            DateOfBirth = filter.DateOfBirth.HasValue
                ? Timestamp.FromDateTime(filter.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                : null,
            ActualityDate = Timestamp.FromDateTime(filter.ActualityDate ?? _timeProvider.GetUtcNowDateTime()),
            Paging = pageable == null
                ? null
                : new PagingModel { PageIndex = pageable.Page - 1, PageSize = pageable.PageSize, },
        };
    }
}
