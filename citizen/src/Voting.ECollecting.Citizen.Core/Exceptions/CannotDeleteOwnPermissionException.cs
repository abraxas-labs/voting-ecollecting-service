// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class CannotDeleteOwnPermissionException() : ValidationException("Cannot delete own permission");
