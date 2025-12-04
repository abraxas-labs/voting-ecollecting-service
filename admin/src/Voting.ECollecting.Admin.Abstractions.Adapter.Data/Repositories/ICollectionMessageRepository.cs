// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;

public interface ICollectionMessageRepository : IDbRepository<DbContext, CollectionMessageEntity>;
