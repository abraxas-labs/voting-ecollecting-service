// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.SecondFactor.Exceptions;

namespace Voting.ECollecting.Admin.WebService.Exceptions;

internal readonly struct ExceptionMapping
{
    private readonly StatusCode _grpcStatusCode;
    private readonly int _httpStatusCode;
    private readonly bool _exposeExceptionType;

    public ExceptionMapping(StatusCode grpcStatusCode, int httpStatusCode, bool exposeExceptionType = false)
    {
        _grpcStatusCode = grpcStatusCode;
        _httpStatusCode = httpStatusCode;
        _exposeExceptionType = exposeExceptionType;
    }

    public static int MapToHttpStatusCode(Exception ex)
        => Map(ex)._httpStatusCode;

    public static StatusCode MapToGrpcStatusCode(Exception ex)
        => Map(ex)._grpcStatusCode;

    public static bool ExposeExceptionType(Exception ex)
        => Map(ex)._exposeExceptionType;

    private static ExceptionMapping Map(Exception ex)
        => ex switch
        {
            NotAuthenticatedException _ => new ExceptionMapping(StatusCode.Unauthenticated, StatusCodes.Status401Unauthorized),
            ForbiddenException _ => new ExceptionMapping(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden),
            FluentValidation.ValidationException _ => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            EntityNotFoundException _ => new ExceptionMapping(StatusCode.NotFound, StatusCodes.Status404NotFound),
            DuplicatedGovernmentDecisionNumberException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status409Conflict, true),
            CollectionAlreadyExistsException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict, true),
            TooManyCollectionSignatureSheetSamplesException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            AddressMissingForAttestException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            ValidationException _ => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            PersonNotFoundException => new ExceptionMapping(StatusCode.NotFound, StatusCodes.Status400BadRequest, true),
            NoPermissionStimmregisterException => new ExceptionMapping(StatusCode.Unauthenticated, StatusCodes.Status401Unauthorized, true),
            SecondFactorTransactionNotVerifiedException => new ExceptionMapping(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden, true),
            VerifySecondFactorTimeoutException => new ExceptionMapping(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden, true),
            SecondFactorTransactionDataChangedException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status400BadRequest, true),
            _ => new ExceptionMapping(StatusCode.Internal, StatusCodes.Status500InternalServerError),
        };
}
