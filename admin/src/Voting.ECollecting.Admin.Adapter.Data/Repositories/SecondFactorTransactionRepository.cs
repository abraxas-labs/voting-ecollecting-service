// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

public class SecondFactorTransactionRepository(DataContext context) : DbRepository<DataContext, SecondFactorTransactionEntity>(context), ISecondFactorTransactionRepository;
