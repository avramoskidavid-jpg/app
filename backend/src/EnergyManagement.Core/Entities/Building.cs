namespace EnergyManagement.Core.Entities;

public class Building
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Type { get; set; } = "Office";
    public double AreaSqm { get; set; }

    // Monthly consumption thresholds (kWh-equivalent units) used to derive status.
    public double WarningThreshold { get; set; } = 5000;
    public double HighThreshold { get; set; } = 8000;

    // Monthly consumption goal assigned to the building; compared against actual usage.
    public double EnergyTarget { get; set; } = 5000;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Meter> Meters { get; set; } = new List<Meter>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
