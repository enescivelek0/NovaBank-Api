using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Iban)
            .IsRequired()
            .HasMaxLength(26);

        builder.HasIndex(a => a.Iban).IsUnique();

        builder.Property(a => a.Balance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.Currency)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired();
    }
}
