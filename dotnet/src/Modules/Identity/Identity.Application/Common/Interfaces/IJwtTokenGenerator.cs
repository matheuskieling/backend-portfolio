using Identity.Domain.Entities;

namespace Identity.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
