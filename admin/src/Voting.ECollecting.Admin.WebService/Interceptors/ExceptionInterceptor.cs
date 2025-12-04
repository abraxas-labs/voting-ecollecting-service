// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.Admin.WebService.Configuration;
using Voting.ECollecting.Admin.WebService.Exceptions;
using BaseExceptionInterceptor = Voting.Lib.Grpc.Interceptors.ExceptionInterceptor;

namespace Voting.ECollecting.Admin.WebService.Interceptors;

/// <summary>
/// Logs errors and sets mapped status codes.
/// Currently only implemented for async unary and async server streaming calls since no other call types are used (yet).
/// </summary>
public class ExceptionInterceptor : BaseExceptionInterceptor
{
    public ExceptionInterceptor(AppConfig config, ILogger<ExceptionInterceptor> logger)
        : base(logger, config.EnableDetailedErrors)
    {
    }

    protected override StatusCode MapExceptionToStatusCode(Exception ex)
        => ExceptionMapping.MapToGrpcStatusCode(ex);

    protected override bool ExposeExceptionType(Exception ex)
        => ExceptionMapping.ExposeExceptionType(ex);
}
