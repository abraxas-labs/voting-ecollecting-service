// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Core.Configuration;

public class AcrConfig
{
    /// <summary>
    /// Gets or sets a list of accepted acr values for the accept permission operation.
    /// </summary>
    public HashSet<string> AcceptPermission { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of accepted acr values for the accept initiative committee membership operation.
    /// </summary>
    public HashSet<string> AcceptInitiativeCommitteeMembership { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of accepted acr values for the sign collection operation.
    /// </summary>
    public HashSet<string> SignCollection { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of accepted acr values for the create collection operation.
    /// </summary>
    public HashSet<string> CreateCollection { get; set; } = [];
}
