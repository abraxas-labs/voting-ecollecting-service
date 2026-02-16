// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class UserHasAlreadyAPermissionException() : ValidationException("The user has already a permission on the same collection");
