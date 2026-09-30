using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.IdentityNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.IdentityNumber).IsUnique();

        builder.HasMany(c => c.Accounts)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
