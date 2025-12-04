// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Test.MockedData;

public static class CitizenAuthMockDefaults
{
    /// <summary>
    /// If any value is set, the user is authenticated, otherwise a 401 throws.
    /// </summary>
    public const string AuthHeader = "authorize";

    /// <summary>
    /// Userid of the mocked authentication handler.
    /// </summary>
    public const string UserIdHeaderName = "x-user";

    /// <summary>
    /// ACR of the mocked authentication handler.
    /// </summary>
    public const string UserAcrHeaderName = "x-user-acr";

    /// <summary>
    /// E-Mail of the mocked authentication handler.
    /// </summary>
    public const string UserEMailHeaderName = "x-user-email";

    /// <summary>
    /// Social security number of the mocked authentication handler.
    /// </summary>
    public const string UserSocialSecurityNumberHeaderName = "x-user-ssn";

    /// <summary>
    /// A userid for a user without any permissions.
    /// </summary>
    public const string NoPermissionUserId = "user-id-without-permission";

    /// <summary>
    /// A userid for a user with deputy permission.
    /// </summary>
    public const string DeputyUserId = "user-id-deputy";

    /// <summary>
    /// A userid for a user with a not yet accepted deputy permission.
    /// </summary>
    public const string DeputyNotAcceptedUserId = "user-id-deputy-not-accepted";

    /// <summary>
    /// A userid for a user with reader permission.
    /// </summary>
    public const string ReaderUserId = "user-id-reader";

    /// <summary>
    /// A userid for a user with not yet accepted reader permission.
    /// </summary>
    public const string ReaderNotAcceptedUserId = "user-id-reader-not-accepted";

    /// <summary>
    /// A userid for a citizen user.
    /// </summary>
    public const string CitizenUserId = "user-id-citizen";

    /// <summary>
    /// User name of the mocked authentication handler.
    /// </summary>
    public const string UserTestName = "Test user";

    /// <summary>
    /// User name of the mocked authentication handler.
    /// </summary>
    public const string UserTestFirstName = "Test";

    /// <summary>
    /// User name of the mocked authentication handler.
    /// </summary>
    public const string UserTestLastName = "user";

    /// <summary>
    /// User e-mail of the mocked authentication handler.
    /// </summary>
    public const string UserTestEMail = "voting+tester@abraxas.ch";

    /// <summary>
    /// User e-mail of the citizen mocked authentication handler.
    /// </summary>
    public const string UserCitizenTestEMail = "rudolph.meier@example.com";

    /// <summary>
    /// Acr value 100.
    /// </summary>
    public const string AcrValue100 = "urn:qa.agov.ch:names:tc:ac:classes:100";

    /// <summary>
    /// Acr value 100.
    /// </summary>
    public const string AcrValue400 = "urn:qa.agov.ch:names:tc:ac:classes:400";
}
