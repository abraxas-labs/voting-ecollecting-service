// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Citizen.Adapter.Data.Repositories;

public class UserNotificationRepository(DataContext context) : DbRepository<DataContext, UserNotificationEntity>(context), IUserNotificationRepository;
