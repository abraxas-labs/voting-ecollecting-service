// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record UpdateDomainOfInfluenceRequest(
    string AddressName,
    string Street,
    string ZipCode,
    string Locality,
    string? Phone,
    string? Email,
    string? Webpage,
    IReadOnlyCollection<string> NotificationEmails,
    DomainOfInfluenceSettings? Settings);
