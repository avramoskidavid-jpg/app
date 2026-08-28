using EnergyManagement.Core.Entities;

namespace EnergyManagement.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user, IList<string> roles);
}
