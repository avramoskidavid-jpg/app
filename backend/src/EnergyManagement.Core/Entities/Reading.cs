namespace EnergyManagement.Core.Entities;

public class Reading
{
    public int Id { get; set; }
    public int MeterId { get; set; }
    public Meter? Meter { get; set; }

    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Notes { get; set; }
}
