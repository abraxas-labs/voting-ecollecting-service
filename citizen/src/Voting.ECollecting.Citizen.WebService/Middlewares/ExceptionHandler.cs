// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.WebService.Configuration;
using Voting.ECollecting.Citizen.WebService.Exceptions;
using BaseExceptionHandler = Voting.Lib.Rest.Middleware.ExceptionHandler;

namespace Voting.ECollecting.Citizen.WebService.Middlewares;

public class ExceptionHandler(AppConfig config, RequestDelegate next, ILogger<ExceptionHandler> logger)
    : BaseExceptionHandler(next, logger, config.EnableDetailedErrors)
{
    protected override int MapExceptionToStatus(Exception ex) => ExceptionMapping.MapToHttpStatusCode(ex);

    protected override bool ExposeExceptionType(Exception ex) => ExceptionMapping.ExposeExceptionType(ex);
}
