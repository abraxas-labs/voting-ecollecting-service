// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Storage;

namespace Voting.ECollecting.Citizen.Abstractions.Adapter.Data;

public interface IDataContext
{
    Task SaveChangesAsync();

    Task<IDbContextTransaction> BeginTransaction();
}
