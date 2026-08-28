namespace EnergyManagement.Application.DTOs;

public record DashboardSummaryDto(
    int TotalBuildings,
    double TotalConsumption,
    decimal TotalCost,
    int ActiveAlerts);

public record MonthlyConsumptionDto(string Month, double Consumption, decimal Cost);

public record BuildingDetailsDto(
    BuildingDto Building,
    List<MeterDto> Meters,
    List<MonthlyConsumptionDto> MonthlyConsumption);
