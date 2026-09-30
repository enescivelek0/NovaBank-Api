using Banking.Domain.Entities;

namespace Banking.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(Customer customer);
}
