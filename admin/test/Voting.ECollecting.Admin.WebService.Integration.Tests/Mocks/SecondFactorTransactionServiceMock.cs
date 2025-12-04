// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Iam.SecondFactor.Exceptions;
using Voting.Lib.Iam.SecondFactor.Models;
using Voting.Lib.Iam.SecondFactor.Services;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

public class SecondFactorTransactionServiceMock : ISecondFactorTransactionService
{
    private readonly Dictionary<Guid, string> _actionIdHashesByTransactionId = new();
    private readonly List<(Guid TransactionId, string ActionIdHash, string Message)> _createdTransactions = new();

    public IReadOnlyList<(Guid TransactionId, string ActionIdHash, string Message)> CreatedTransactions =>
        _createdTransactions;

    public Guid AddVerifiedActionId(ISecondFactorTransactionActionId actionId)
    {
        var id = Guid.NewGuid();
        _actionIdHashesByTransactionId[id] = actionId.ComputeHash();
        return id;
    }

    public Task<SecondFactorTransactionInfo> Create(ISecondFactorTransactionActionId actionId, string message)
    {
        var id = Guid.NewGuid();
        _createdTransactions.Add((id, actionId.ComputeHash(), message));
        return Task.FromResult(new SecondFactorTransactionInfo(
            new SecondFactorTransaction { Id = id },
            "mocked code",
            "mocked message",
            "mocked qr code"));
    }

    public async Task EnsureVerified(
        Guid transactionId,
        Func<Task<ISecondFactorTransactionActionId>> actionProvider,
        CancellationToken cancellationToken)
    {
        if (!_actionIdHashesByTransactionId.TryGetValue(transactionId, out var expectedHash))
        {
            throw new SecondFactorTransactionNotVerifiedException();
        }

        var actionId = await actionProvider();
        if (!actionId.ComputeHash().Equals(expectedHash, StringComparison.Ordinal))
        {
            throw new SecondFactorTransactionDataChangedException();
        }
    }

    public void Reset()
    {
        _actionIdHashesByTransactionId.Clear();
        _createdTransactions.Clear();
    }
}
