using EnergyManagement.Core.Enums;

namespace EnergyManagement.Application.DTOs;

public record MeterDto(
    int Id,
    int BuildingId,
    string SerialNumber,
    MeterType Type,
    string Unit,
    decimal CostPerUnit,
    double? LatestReadingValue,
    DateTime? LatestReadingTimestamp);

public record MeterCreateDto(
    int BuildingId,
    string SerialNumber,
    MeterType Type,
    string Unit,
    decimal CostPerUnit);

public record MeterUpdateDto(
    string SerialNumber,
    MeterType Type,
    string Unit,
    decimal CostPerUnit);
