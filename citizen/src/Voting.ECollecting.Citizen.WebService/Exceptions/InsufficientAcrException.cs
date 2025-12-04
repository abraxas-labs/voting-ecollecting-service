// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.WebService.Exceptions;

public class InsufficientAcrException(IEnumerable<string> allowedAcr, string? actualAcr)
    : Exception($"acr does not match, actual acr {actualAcr}, expected any of {string.Join(", ", allowedAcr)}");
