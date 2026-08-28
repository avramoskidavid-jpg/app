using EnergyManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyManagement.Tests;

/// Builds an isolated in-memory AppDbContext for each test so tests never share state.
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
