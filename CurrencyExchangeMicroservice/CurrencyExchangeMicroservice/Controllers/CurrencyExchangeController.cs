using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeMicroservice.Models;
using CurrencyExchangeMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchangeMicroservice.Controllers
{
    // Defines the CurrencyExchangeController class, which inherits from ControllerBase.
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : ControllerBase
    {
        // Declare private fields for the API client, database context, memory cache, and logger.
        private readonly FixerApiClient _fixerApiClient;
        private readonly CurrencyExchangeDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CurrencyExchangeController> _logger;

        // Constructor to initialize private fields.
        public CurrencyExchangeController(FixerApiClient fixerApiClient, CurrencyExchangeDbContext context, IMemoryCache memoryCache, ILogger<CurrencyExchangeController> logger)
        {
            _fixerApiClient = fixerApiClient;
            _context = context;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        // POST endpoint for currency exchange requests.
        [HttpPost]
        public async Task<ActionResult<CurrencyExchangeResponse>> Post([FromBody] CurrencyExchangeRequest request)
        {
            try
            {
                // Log the received request.
                _logger.LogInformation("Received currency exchange request: {@Request}", request);

                // Get the client ID.
                var clientId = GetClientId();

                // Check if the client has exceeded the rate limit
                if (!IsClientAllowedToTrade(clientId))
                {
                    _logger.LogWarning("Client {ClientId} exceeded rate limit", clientId);
                    return StatusCode(429, "You have exceeded the limit of 10 trades per hour.");
                }

                // Get the exchange rate and converted amount from the API client.
                var (exchangeRate, convertedAmount) = await _fixerApiClient.GetExchangeRate(request.FromCurrency, request.ToCurrency, request.Amount);

                // Create a response object.
                var response = new CurrencyExchangeResponse
                {
                    FromCurrency = request.FromCurrency,
                    ToCurrency = request.ToCurrency,
                    Amount = request.Amount,
                    ConvertedAmount = convertedAmount,
                    ExchangeRate = exchangeRate
                };

                // Add the trade to the database.
                _context.CurrencyExchangeTrades.Add(new CurrencyExchangeTrade
                {
                    Id = Guid.NewGuid(),
                    FromCurrency = request.FromCurrency,
                    ToCurrency = request.ToCurrency,
                    Amount = request.Amount,
                    ConvertedAmount = convertedAmount,
                    ExchangeRate = exchangeRate,
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                IncrementTradeCount(clientId);

                // Log the successful exchange and return the response.
                _logger.LogInformation("Currency exchange completed successfully: {@Response}", response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error and return a bad request response.
                _logger.LogError(ex, "Error occurred during currency exchange: {Message}", ex.Message);
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // GET endpoint to retrieve trade history.
        [HttpGet("trades")]
        public async Task<ActionResult<IEnumerable<CurrencyExchangeTrade>>> GetTrades()
        {
            _logger.LogInformation("Retrieving trade history");
            return Ok(await _context.CurrencyExchangeTrades.ToListAsync());
        }

        // Get the client ID using their IP address.
        private string GetClientId()
        {
            // Use the client's IP address as the identifier (you can use any other unique identifier)
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        // Check if the client is allowed to trade based on their trade count.
        private bool IsClientAllowedToTrade(string clientId)
        {
            const int maxTradesPerHour = 10;

            if (!_memoryCache.TryGetValue(clientId, out int tradeCount))
            {
                // Cache entry not found, client is allowed to trade
                return true;
            }

            return tradeCount < maxTradesPerHour;
        }

        // Increment the trade count for the given client ID.
        private void IncrementTradeCount(string clientId)
        {
            const int cacheExpirationMinutes = 60;

            if (!_memoryCache.TryGetValue(clientId, out int tradeCount))
            {
                // Cache entry not found, add a new entry with a count of 1
                _memoryCache.Set(clientId, 1, TimeSpan.FromMinutes(cacheExpirationMinutes));
                _logger.LogInformation("Client {ClientId} cache entry created with trade count 1", clientId);
            }
            else
            {
                // Increment the trade count and update the cache entry
                _memoryCache.Set(clientId, tradeCount + 1, TimeSpan.FromMinutes(cacheExpirationMinutes));
                _logger.LogInformation("Client {ClientId} trade count incremented to {TradeCount}", clientId, tradeCount + 1);
            }
        }

        // DELETE endpoint to delete all trade history.
        [HttpDelete("trades")]
        public async Task<IActionResult> DeleteAllTrades()
        {
            _logger.LogInformation("Deleting all trade history");

            var trades = _context.CurrencyExchangeTrades;

            if (!trades.Any())
            {
                return NotFound("No trades found to delete.");
            }

            // Remove all trade history from the database.
            _context.CurrencyExchangeTrades.RemoveRange(trades);
            await _context.SaveChangesAsync();

            _logger.LogInformation("All trade history deleted successfully"); 

            return Ok("All trade history deleted successfully");
        }
    }
}
