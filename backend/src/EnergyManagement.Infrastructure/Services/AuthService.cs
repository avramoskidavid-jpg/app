using EnergyManagement.Application.DTOs;
using EnergyManagement.Application.Interfaces;
using EnergyManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace EnergyManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return ServiceResult<AuthResponse>.Fail("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var error = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return ServiceResult<AuthResponse>.Fail(error);
        }

        var roleName = request.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        var response = BuildAuthResponse(user, new List<string> { roleName });
        return ServiceResult<AuthResponse>.Ok(response);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ServiceResult<AuthResponse>.Fail("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = BuildAuthResponse(user, roles);
        return ServiceResult<AuthResponse>.Ok(response);
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user, IList<string> roles)
    {
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roles);
        var role = roles.FirstOrDefault() ?? "Viewer";
        var userDto = new UserDto(user.Id, user.FullName, user.Email ?? string.Empty, role);
        return new AuthResponse(token, expiresAt, userDto);
    }
}
