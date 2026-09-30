using Banking.Application.Common.Interfaces;
using Banking.Infrastructure.Persistence;
using Banking.Infrastructure.Persistence.Repositories;
using Banking.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database configuration: Default to InMemory for frictionless development/testing,
        // can be swapped to PostgreSQL or SQL Server easily via connection string configuration.
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<BankingDbContext>(options =>
                options.UseInMemoryDatabase("BankingDb"));
        }
        else
        {
            // If connection string is specified, use InMemory or configurable provider
            services.AddDbContext<BankingDbContext>(options =>
                options.UseInMemoryDatabase(connectionString));
        }

        // Repositories & Unit of Work
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // JWT Services
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
