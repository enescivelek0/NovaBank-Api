using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(BankingDbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var customer1 = new Customer(
            firstName: "Ahmet",
            lastName: "Yılmaz",
            email: "ahmet.yilmaz@banka.com",
            identityNumber: "12345678901"
        );

        var customer2 = new Customer(
            firstName: "Ayşe",
            lastName: "Demir",
            email: "ayse.demir@banka.com",
            identityNumber: "98765432101"
        );

        await context.Customers.AddRangeAsync(customer1, customer2);
        await context.SaveChangesAsync();

        var account1 = new Account(customer1.Id, Currency.TRY, 25000m);
        var account2 = new Account(customer1.Id, Currency.USD, 1500m);
        var account3 = new Account(customer2.Id, Currency.TRY, 10000m);
        var account4 = new Account(customer2.Id, Currency.EUR, 2000m);

        await context.Accounts.AddRangeAsync(account1, account2, account3, account4);
        await context.SaveChangesAsync();

        var tx1 = Transaction.CreateDeposit(account1.Id, 25000m, Currency.TRY, "Açılış bakiye yatırma işlemi");
        var tx2 = Transaction.CreateDeposit(account2.Id, 1500m, Currency.USD, "Açılış bakiye yatırma işlemi");
        var tx3 = Transaction.CreateDeposit(account3.Id, 10000m, Currency.TRY, "Açılış bakiye yatırma işlemi");
        var tx4 = Transaction.CreateDeposit(account4.Id, 2000m, Currency.EUR, "Açılış bakiye yatırma işlemi");

        await context.Transactions.AddRangeAsync(tx1, tx2, tx3, tx4);
        await context.SaveChangesAsync();
    }
}
