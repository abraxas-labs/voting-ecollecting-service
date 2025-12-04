// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Constants;

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class DomainException(string message, ProcessStatusCode statusCode) : Exception(message)
{
    public ProcessStatusCode StatusCode { get; } = statusCode;
}
