using Microsoft.AspNetCore.Identity;

namespace EnergyManagement.Core.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
}
