using Banking.Application.Common.Interfaces;
using Banking.Application.Features.Accounts.Commands.TransferMoney;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Banking.UnitTests.Features.Accounts.Commands;

public class TransferMoneyCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepoMock;
    private readonly Mock<ITransactionRepository> _transactionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TransferMoneyCommandHandler _handler;

    public TransferMoneyCommandHandlerTests()
    {
        _accountRepoMock = new Mock<IAccountRepository>();
        _transactionRepoMock = new Mock<ITransactionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new TransferMoneyCommandHandler(
            _accountRepoMock.Object,
            _transactionRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidRequestAndSufficientBalance_ShouldExecuteTransferAndCommitTransaction()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sourceAccount = new Account(customerId, Currency.TRY, 10000m);
        var targetAccount = new Account(customerId, Currency.TRY, 5000m);
        var transferAmount = 3000m;

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(sourceAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(targetAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAccount);

        var command = new TransferMoneyCommand(
            sourceAccount.Id,
            targetAccount.Id,
            transferAmount,
            "Kira ödemesi"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(transferAmount);
        result.SourceAccountId.Should().Be(sourceAccount.Id);
        result.TargetAccountId.Should().Be(targetAccount.Id);

        sourceAccount.Balance.Should().Be(7000m);
        targetAccount.Balance.Should().Be(8000m);

        _transactionRepoMock.Verify(t => t.AddAsync(
            It.Is<Transaction>(tx => tx.Amount == transferAmount && tx.TransactionType == TransactionType.Transfer),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInsufficientBalance_ShouldThrowInsufficientBalanceExceptionAndRollbackTransaction()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sourceAccount = new Account(customerId, Currency.TRY, 1000m);
        var targetAccount = new Account(customerId, Currency.TRY, 5000m);
        var transferAmount = 2500m; // Exceeds balance of 1000m

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(sourceAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(targetAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAccount);

        var command = new TransferMoneyCommand(
            sourceAccount.Id,
            targetAccount.Id,
            transferAmount,
            "Yetersiz bakiye denemesi"
        );

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientBalanceException>()
            .Where(e => e.CurrentBalance == 1000m && e.RequestedAmount == 2500m);

        sourceAccount.Balance.Should().Be(1000m);
        targetAccount.Balance.Should().Be(5000m);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSourceAccountNotFound_ShouldThrowEntityNotFoundExceptionAndRollbackTransaction()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(sourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var command = new TransferMoneyCommand(sourceId, targetId, 500m);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCurrenciesDoNotMatch_ShouldThrowInvalidAccountOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sourceAccount = new Account(customerId, Currency.TRY, 5000m);
        var targetAccount = new Account(customerId, Currency.USD, 1000m);

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(sourceAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        _accountRepoMock
            .Setup(r => r.GetByIdAsync(targetAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAccount);

        var command = new TransferMoneyCommand(sourceAccount.Id, targetAccount.Id, 500m);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidAccountOperationException>()
            .WithMessage("*para birimleri uyuşmuyor*");

        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
