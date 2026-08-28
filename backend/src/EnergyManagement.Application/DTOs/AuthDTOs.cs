using EnergyManagement.Core.Enums;

namespace EnergyManagement.Application.DTOs;

public record RegisterRequest(string FullName, string Email, string Password, UserRole Role);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string FullName, string Email, string Role);
