// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Models;

public record InitiativeCommitteeMemberUserPermissions(bool CanEdit, bool CanEditPoliticalDuty, bool CanResend, bool CanReset, bool CanVerify);
