// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;

public interface IAccessControlListDoiRepository : IDbRepository<DbContext, AccessControlListDoiEntity>;
