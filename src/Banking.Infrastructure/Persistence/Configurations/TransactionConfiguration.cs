using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Currency)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .IsRequired();

        builder.Property(t => t.TransactionDate)
            .IsRequired();

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasOne(t => t.SourceAccount)
            .WithMany()
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TargetAccount)
            .WithMany()
            .HasForeignKey(t => t.TargetAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
