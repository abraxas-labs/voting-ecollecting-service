// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.ECollecting.Admin.Core.Exceptions;

public class DuplicatedGovernmentDecisionNumberException(string number)
    : ValidationException($"The government decision number {number} already exists on another collection");
