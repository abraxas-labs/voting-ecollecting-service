// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Constants;

namespace Voting.ECollecting.Admin.Domain.Exceptions;

public class DomainException(string message, ProcessStatusCode statusCode) : Exception(message)
{
    public ProcessStatusCode StatusCode { get; } = statusCode;
}
