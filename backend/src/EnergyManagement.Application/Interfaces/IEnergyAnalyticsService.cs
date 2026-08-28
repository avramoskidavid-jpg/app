using EnergyManagement.Application.DTOs;

namespace EnergyManagement.Application.Interfaces;

public interface IEnergyAnalyticsService
{
    Task<double> CalculateMonthlyConsumptionAsync(int buildingId, int year, int month);

    decimal CalculateEnergyCost(double consumption, decimal pricePerUnit);

    Task<AnomalyResult> DetectConsumptionAnomalyAsync(int buildingId);

    Task<EnergyTargetResult> CalculateEnergyTargetAsync(int buildingId);

    /// Runs anomaly detection for a single building and creates an Alert if one is newly
    /// detected (and not already open for this month). Returns true if an alert was created.
    Task<bool> CheckBuildingForAnomalyAsync(int buildingId);

    /// Scans every building for a consumption anomaly and creates an Alert for each newly
    /// detected one. Returns the number of alerts created.
    Task<int> ScanAllBuildingsForAnomaliesAsync();
}
