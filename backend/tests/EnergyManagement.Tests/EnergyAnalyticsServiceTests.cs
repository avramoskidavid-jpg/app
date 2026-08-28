using EnergyManagement.Core.Entities;
using EnergyManagement.Core.Enums;
using EnergyManagement.Infrastructure.Data;
using EnergyManagement.Infrastructure.Services;

namespace EnergyManagement.Tests;

public class EnergyAnalyticsServiceTests
{
    private static Building NewBuilding(AppDbContext db, double energyTarget = 5000, string name = "Test Building")
    {
        var building = new Building { Name = name, Address = "1 Test St", EnergyTarget = energyTarget };
        db.Buildings.Add(building);
        db.SaveChanges();
        return building;
    }

    private static Meter NewMeter(AppDbContext db, int buildingId, MeterType type = MeterType.Electricity)
    {
        var meter = new Meter { BuildingId = buildingId, SerialNumber = Guid.NewGuid().ToString(), Type = type, Unit = "kWh", CostPerUnit = 0.2m };
        db.Meters.Add(meter);
        db.SaveChanges();
        return meter;
    }

    private static void AddReading(AppDbContext db, int meterId, DateTime timestamp, double value)
    {
        db.Readings.Add(new Reading { MeterId = meterId, Timestamp = timestamp, Value = value });
        db.SaveChanges();
    }

    // ----- CalculateMonthlyConsumptionAsync -----

    [Fact]
    public async Task CalculateMonthlyConsumptionAsync_SumsReadingsAcrossMetersForThatBuildingAndMonth()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter1 = NewMeter(db, building.Id);
        var meter2 = NewMeter(db, building.Id, MeterType.Water);

        AddReading(db, meter1.Id, now, 100);
        AddReading(db, meter1.Id, now.AddMonths(-1), 999); // different month, must be excluded
        AddReading(db, meter2.Id, now, 200);

        var result = await service.CalculateMonthlyConsumptionAsync(building.Id, now.Year, now.Month);

        Assert.Equal(300, result);
    }

    [Fact]
    public async Task CalculateMonthlyConsumptionAsync_ReturnsZero_WhenNoReadingsExistForThatMonth()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now.AddMonths(-2), 500); // only a reading in a different month

        var result = await service.CalculateMonthlyConsumptionAsync(building.Id, now.Year, now.Month);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CalculateMonthlyConsumptionAsync_ReturnsZero_WhenBuildingHasNoMeters()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var building = NewBuilding(db);

        var result = await service.CalculateMonthlyConsumptionAsync(building.Id, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CalculateMonthlyConsumptionAsync_IgnoresReadingsFromOtherBuildings()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var buildingA = NewBuilding(db, name: "A");
        var buildingB = NewBuilding(db, name: "B");
        var meterA = NewMeter(db, buildingA.Id);
        var meterB = NewMeter(db, buildingB.Id);

        AddReading(db, meterA.Id, now, 100);
        AddReading(db, meterB.Id, now, 9000);

        var result = await service.CalculateMonthlyConsumptionAsync(buildingA.Id, now.Year, now.Month);

        Assert.Equal(100, result);
    }

    // ----- CalculateEnergyCost -----

    [Theory]
    [InlineData(0, 0.20, 0)]
    [InlineData(100, 0, 0)]
    [InlineData(100, 0.20, 20)]
    [InlineData(150.5, 0.18, 27.09)]
    public void CalculateEnergyCost_MultipliesConsumptionByPrice(double consumption, decimal pricePerUnit, decimal expected)
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);

        var cost = service.CalculateEnergyCost(consumption, pricePerUnit);

        Assert.Equal(expected, cost);
    }

    // ----- DetectConsumptionAnomalyAsync -----

    [Fact]
    public async Task DetectConsumptionAnomalyAsync_NoAnomaly_WhenNoPreviousReadingsExist()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 5000); // current month only, no history at all

        var result = await service.DetectConsumptionAnomalyAsync(building.Id);

        Assert.False(result.IsAnomaly);
        Assert.Equal(0, result.PreviousThreeMonthAverage);
    }

    [Fact]
    public async Task DetectConsumptionAnomalyAsync_NoAnomaly_WhenCurrentEqualsAverage()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 100);
        AddReading(db, meter.Id, now.AddMonths(-1), 100);
        AddReading(db, meter.Id, now.AddMonths(-2), 100);
        AddReading(db, meter.Id, now.AddMonths(-3), 100);

        var result = await service.DetectConsumptionAnomalyAsync(building.Id);

        Assert.False(result.IsAnomaly);
        Assert.Equal(0, result.PercentAboveAverage);
    }

    [Fact]
    public async Task DetectConsumptionAnomalyAsync_BoundaryAtExactlyTwentyPercent_IsNotAnomaly()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        // Previous 3 months average to exactly 100; current month is exactly 120 (+20%).
        AddReading(db, meter.Id, now, 120);
        AddReading(db, meter.Id, now.AddMonths(-1), 100);
        AddReading(db, meter.Id, now.AddMonths(-2), 100);
        AddReading(db, meter.Id, now.AddMonths(-3), 100);

        var result = await service.DetectConsumptionAnomalyAsync(building.Id);

        Assert.Equal(20.0, result.PercentAboveAverage);
        Assert.False(result.IsAnomaly, "Exactly 20% above average should not cross the (strictly greater than) 20% threshold.");
    }

    [Fact]
    public async Task DetectConsumptionAnomalyAsync_JustOverTwentyPercent_IsAnomaly()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        // Average is 100; current is 120.1 (+20.1%), just over the threshold.
        AddReading(db, meter.Id, now, 120.1);
        AddReading(db, meter.Id, now.AddMonths(-1), 100);
        AddReading(db, meter.Id, now.AddMonths(-2), 100);
        AddReading(db, meter.Id, now.AddMonths(-3), 100);

        var result = await service.DetectConsumptionAnomalyAsync(building.Id);

        Assert.True(result.IsAnomaly);
    }

    [Fact]
    public async Task DetectConsumptionAnomalyAsync_TreatsMonthsWithoutReadingsAsZeroInTheAverage()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db);
        var meter = NewMeter(db, building.Id);
        // Only one of the previous 3 months has a reading; the other two are missing entirely.
        AddReading(db, meter.Id, now, 150);
        AddReading(db, meter.Id, now.AddMonths(-1), 300);
        // now.AddMonths(-2) and now.AddMonths(-3): no readings at all.

        var result = await service.DetectConsumptionAnomalyAsync(building.Id);

        // Average = (300 + 0 + 0) / 3 = 100; current 150 is 50% above that.
        Assert.Equal(100, result.PreviousThreeMonthAverage);
        Assert.True(result.IsAnomaly);
        Assert.Equal(50.0, result.PercentAboveAverage);
    }

    // ----- CalculateEnergyTargetAsync -----

    [Fact]
    public async Task CalculateEnergyTargetAsync_ReturnsDefaultResult_WhenBuildingDoesNotExist()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);

        var result = await service.CalculateEnergyTargetAsync(buildingId: 999);

        Assert.False(result.Exceeded);
        Assert.Equal(0, result.CurrentMonthConsumption);
        Assert.Equal(0, result.EnergyTarget);
        Assert.Equal(0, result.PercentOfTarget);
    }

    [Fact]
    public async Task CalculateEnergyTargetAsync_NotExceeded_WhenConsumptionBelowTarget()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db, energyTarget: 1000);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 800);

        var result = await service.CalculateEnergyTargetAsync(building.Id);

        Assert.False(result.Exceeded);
        Assert.Equal(80.0, result.PercentOfTarget);
    }

    [Fact]
    public async Task CalculateEnergyTargetAsync_NotExceeded_WhenConsumptionExactlyEqualsTarget()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db, energyTarget: 1000);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 1000);

        var result = await service.CalculateEnergyTargetAsync(building.Id);

        Assert.False(result.Exceeded, "Consumption equal to (not greater than) the target should not count as exceeded.");
        Assert.Equal(100.0, result.PercentOfTarget);
    }

    [Fact]
    public async Task CalculateEnergyTargetAsync_Exceeded_WhenConsumptionAboveTarget()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db, energyTarget: 1000);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 1250);

        var result = await service.CalculateEnergyTargetAsync(building.Id);

        Assert.True(result.Exceeded);
        Assert.Equal(125.0, result.PercentOfTarget);
    }

    [Fact]
    public async Task CalculateEnergyTargetAsync_ZeroTarget_NeverExceededAndPercentIsZero()
    {
        using var db = TestDbContextFactory.Create();
        var service = new EnergyAnalyticsService(db);
        var now = DateTime.UtcNow;

        var building = NewBuilding(db, energyTarget: 0);
        var meter = NewMeter(db, building.Id);
        AddReading(db, meter.Id, now, 5000);

        var result = await service.CalculateEnergyTargetAsync(building.Id);

        Assert.False(result.Exceeded);
        Assert.Equal(0, result.PercentOfTarget);
    }
}
