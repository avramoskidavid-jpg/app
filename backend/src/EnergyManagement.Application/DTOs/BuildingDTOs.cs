using EnergyManagement.Core.Enums;

namespace EnergyManagement.Application.DTOs;

public record BuildingDto(
    int Id,
    string Name,
    string Address,
    string Type,
    double AreaSqm,
    double WarningThreshold,
    double HighThreshold,
    double EnergyTarget,
    int MeterCount,
    double CurrentMonthConsumption,
    decimal CurrentMonthCost,
    ConsumptionStatus Status);

public record BuildingCreateDto(
    string Name,
    string Address,
    string Type,
    double AreaSqm,
    double WarningThreshold,
    double HighThreshold,
    double EnergyTarget);

public record BuildingUpdateDto(
    string Name,
    string Address,
    string Type,
    double AreaSqm,
    double WarningThreshold,
    double HighThreshold,
    double EnergyTarget);
