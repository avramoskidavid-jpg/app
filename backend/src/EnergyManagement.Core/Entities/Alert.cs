using EnergyManagement.Core.Enums;

namespace EnergyManagement.Core.Entities;

public class Alert
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public Building? Building { get; set; }

    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
