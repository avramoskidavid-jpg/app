namespace EnergyManagement.Core.Enums;

public enum UserRole
{
    Admin,
    Manager,
    Viewer
}

public enum MeterType
{
    Electricity,
    Water,
    Gas
}

public enum ConsumptionStatus
{
    Normal,
    Warning,
    High
}

public enum AlertSeverity
{
    Low,
    Medium,
    High
}

public enum AlertStatus
{
    Open,
    Resolved
}
