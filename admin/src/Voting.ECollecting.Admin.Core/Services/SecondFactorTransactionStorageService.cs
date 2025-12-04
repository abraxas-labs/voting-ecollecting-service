// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Iam.SecondFactor.Models;
using Voting.Lib.Iam.SecondFactor.Services;

namespace Voting.ECollecting.Admin.Core.Services;

public class SecondFactorTransactionStorageService : ISecondFactorTransactionRepository
{
    private readonly Abstractions.Adapter.Data.Repositories.ISecondFactorTransactionRepository _repo;
    private readonly TimeProvider _timeProvider;

    public SecondFactorTransactionStorageService(Abstractions.Adapter.Data.Repositories.ISecondFactorTransactionRepository repo, TimeProvider timeProvider)
    {
        _repo = repo;
        _timeProvider = timeProvider;
    }

    public async Task<SecondFactorTransaction> GetById(Guid transactionId)
    {
        var transaction = await _repo.GetByKey(transactionId)
            ?? throw new EntityNotFoundException(nameof(SecondFactorTransactionEntity), transactionId);
        return Mapper.MapToSecondFactorTransaction(transaction);
    }

    public Task Create(SecondFactorTransaction transaction)
    {
        var entity = Mapper.MapToSecondFactorTransaction(transaction);
        return _repo.Create(entity);
    }

    public Task Update(SecondFactorTransaction transaction)
    {
        var entity = Mapper.MapToSecondFactorTransaction(transaction);
        return _repo.Update(entity);
    }

    public async Task DeleteExpired()
    {
        await _repo.Query()
            .Where(x => x.ExpireAt < _timeProvider.GetUtcNowDateTime())
            .ExecuteDeleteAsync();
    }
}
