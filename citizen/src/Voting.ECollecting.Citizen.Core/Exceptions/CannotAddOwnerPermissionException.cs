// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class CannotAddOwnerPermissionException() : ValidationException("Cannot add a permission for the owner of a collection");
