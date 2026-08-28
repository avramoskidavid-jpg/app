using EnergyManagement.Core.Enums;

namespace EnergyManagement.Core.Entities;

public class Meter
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public Building? Building { get; set; }

    public string SerialNumber { get; set; } = string.Empty;
    public MeterType Type { get; set; }
    public string Unit { get; set; } = "kWh";
    public decimal CostPerUnit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reading> Readings { get; set; } = new List<Reading>();
}
