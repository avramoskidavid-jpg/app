using System.Net.Http.Json;
using EnergyManagement.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnergyManagement.Infrastructure.Services;

/// <summary>
/// Integration point for a real-time electricity price feed. In production this would call
/// out to a market-price provider (e.g. a grid operator or utility pricing API) using
/// <see cref="_httpClient"/> and the base URL configured under "EnergyPriceApi:BaseUrl".
/// No such provider is wired up here (most require a paid API key), so calls fall back to a
/// realistic simulated price. The fallback still goes through the same cache/error-handling
/// path a real integration would use, so swapping in a real endpoint later only means
/// replacing <see cref="FetchFromExternalApiAsync"/>.
/// </summary>
public class SimulatedEnergyPriceService : IEnergyPriceService
{
    private const string CacheKey = "energy-price:electricity:kwh";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SimulatedEnergyPriceService> _logger;

    public SimulatedEnergyPriceService(
        HttpClient httpClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<SimulatedEnergyPriceService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<decimal> GetCurrentPricePerKwhAsync()
    {
        if (_cache.TryGetValue(CacheKey, out decimal cachedPrice))
        {
            return cachedPrice;
        }

        decimal price;
        try
        {
            price = await FetchFromExternalApiAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Energy price API unavailable, using simulated price.");
            price = SimulateCurrentPrice();
        }

        _cache.Set(CacheKey, price, CacheDuration);
        return price;
    }

    /// <summary>
    /// Placeholder for a real HTTP call to an external price feed. No base URL is configured
    /// by default, so this always falls back to the simulation - see class remarks.
    /// </summary>
    private async Task<decimal> FetchFromExternalApiAsync()
    {
        var baseUrl = _configuration["EnergyPriceApi:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return SimulateCurrentPrice();
        }

        var response = await _httpClient.GetAsync($"{baseUrl}/price/electricity/current");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ExternalPriceResponse>();
        return payload?.PricePerKwh ?? SimulateCurrentPrice();
    }

    /// <summary>
    /// Realistic-looking price simulation: a base rate plus a smooth time-of-day swing and
    /// small deterministic noise, so the price drifts slightly between calls within a
    /// plausible $0.14-$0.34/kWh band without ever spiking wildly between requests.
    /// </summary>
    private static decimal SimulateCurrentPrice()
    {
        const double baseRate = 0.22;
        const double amplitude = 0.06;

        var now = DateTime.UtcNow;
        var hourOfDayFactor = Math.Sin((now.Hour + now.Minute / 60.0) / 24.0 * 2 * Math.PI);
        var minuteNoise = (now.Minute % 10) / 10.0 * 0.02 - 0.01;

        var price = baseRate + amplitude * hourOfDayFactor + minuteNoise;
        price = Math.Clamp(price, 0.12, 0.36);

        return Math.Round((decimal)price, 4);
    }

    private record ExternalPriceResponse(decimal PricePerKwh);
}
