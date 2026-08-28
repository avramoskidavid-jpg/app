namespace EnergyManagement.Application.Interfaces;

/// Abstraction over an external electricity price feed. The real implementation may call
/// out to a market-price API; callers should not need to know whether the price came from
/// the network or a fallback/simulation.
public interface IEnergyPriceService
{
    Task<decimal> GetCurrentPricePerKwhAsync();
}
