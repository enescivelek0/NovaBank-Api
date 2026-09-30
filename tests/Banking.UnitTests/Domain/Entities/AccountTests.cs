using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Banking.UnitTests.Domain.Entities;

public class AccountTests
{
    [Fact]
    public void Account_Initialization_ShouldGenerateValidTrIbanAndSetBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var account = new Account(customerId, Currency.TRY, 1500m);

        // Assert
        account.CustomerId.Should().Be(customerId);
        account.Balance.Should().Be(1500m);
        account.Currency.Should().Be(Currency.TRY);
        account.IsActive.Should().BeTrue();
        account.Iban.Should().StartWith("TR");
        account.Iban.Length.Should().Be(26);
    }

    [Fact]
    public void Account_Initialization_WithNegativeBalance_ShouldThrowInsufficientBalanceException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var act = () => new Account(customerId, Currency.TRY, -500m);

        // Assert
        act.Should().Throw<InsufficientBalanceException>();
    }

    [Fact]
    public void Withdraw_WhenAmountExceedsBalance_ShouldThrowInsufficientBalanceException()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), Currency.TRY, 100m);

        // Act
        var act = () => account.Withdraw(150m);

        // Assert
        act.Should().Throw<InsufficientBalanceException>();
    }

    [Fact]
    public void Withdraw_WithValidAmount_ShouldDecreaseBalance()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), Currency.TRY, 500m);

        // Act
        account.Withdraw(200m);

        // Assert
        account.Balance.Should().Be(300m);
    }

    [Fact]
    public void Deposit_WithPositiveAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), Currency.TRY, 200m);

        // Act
        account.Deposit(350m);

        // Assert
        account.Balance.Should().Be(550m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Deposit_WithZeroOrNegativeAmount_ShouldThrowInvalidAccountOperationException(decimal amount)
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), Currency.TRY, 100m);

        // Act
        var act = () => account.Deposit(amount);

        // Assert
        act.Should().Throw<InvalidAccountOperationException>();
    }
}
