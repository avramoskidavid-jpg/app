namespace EnergyManagement.Application.DTOs;

public record AnomalyResult(
    bool IsAnomaly,
    double CurrentMonthConsumption,
    double PreviousThreeMonthAverage,
    double PercentAboveAverage);

public record EnergyTargetResult(
    bool Exceeded,
    double CurrentMonthConsumption,
    double EnergyTarget,
    double PercentOfTarget);
