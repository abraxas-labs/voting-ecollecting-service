// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Adapter.ELogin;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.Domain.Exceptions;
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Citizen.WebService.Exceptions;

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
            CollectionAlreadySignedException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict, true),
            DecreeAlreadySignedException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict, true),
            CollectionMaxElectronicSignatureCountReachedException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict, true),
            DecreeMaxElectronicSignatureCountReachedException => new ExceptionMapping(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict, true),
            PersonOrVotingRightNotFoundException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            NoDataException => new ExceptionMapping(StatusCode.Unavailable, StatusCodes.Status204NoContent),
            CannotAddOwnerPermissionException => new ExceptionMapping(StatusCode.AlreadyExists, StatusCodes.Status424FailedDependency, true),
            CannotDeleteOwnPermissionException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            FluentValidation.ValidationException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            InitiativeNotFoundException => new ExceptionMapping(StatusCode.NotFound, StatusCodes.Status404NotFound, true),
            ReferendumNotFoundException => new ExceptionMapping(StatusCode.NotFound, StatusCodes.Status404NotFound, true),
            EntityNotFoundException => new ExceptionMapping(StatusCode.NotFound, StatusCodes.Status404NotFound),
            ValidationException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest),
            InitiativeAlreadyInPreparationException => new ExceptionMapping(StatusCode.AlreadyExists, StatusCodes.Status424FailedDependency, true),
            InitiativeAdmissibilityDecisionRejectedException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            ReferendumAlreadyInPreparationException => new ExceptionMapping(StatusCode.AlreadyExists, StatusCodes.Status424FailedDependency, true),
            SendUserNotificationFailedException => new ExceptionMapping(StatusCode.Internal, StatusCodes.Status500InternalServerError, true),
            UserHasAlreadyAReferendumOnDecreeException => new ExceptionMapping(StatusCode.AlreadyExists, StatusCodes.Status424FailedDependency, true),
            MaxReferendumsOnDecreeReachedException => new ExceptionMapping(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest, true),
            CollectionPermissionAlreadyExistsException => new ExceptionMapping(StatusCode.AlreadyExists, StatusCodes.Status424FailedDependency, true),
            InsufficientAcrException => new ExceptionMapping(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden, true),
            EmailDoesNotMatchException => new ExceptionMapping(StatusCode.PermissionDenied, StatusCodes.Status403Forbidden, true),
            _ => new ExceptionMapping(StatusCode.Internal, StatusCodes.Status500InternalServerError),
        };
}
