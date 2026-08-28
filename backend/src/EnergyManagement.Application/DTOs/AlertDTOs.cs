using EnergyManagement.Core.Enums;

namespace EnergyManagement.Application.DTOs;

public record AlertDto(
    int Id,
    int BuildingId,
    string BuildingName,
    string Message,
    AlertSeverity Severity,
    AlertStatus Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
