using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using CurrencyExchangeMicroservice.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;


namespace CurrencyExchangeMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly FixerApiClient _fixerApiClient;
        private readonly CurrencyExchangeDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public CurrencyExchangeController(IHttpClientFactory httpClientFactory, CurrencyExchangeDbContext context, IMemoryCache memoryCache)
        {
            _fixerApiClient = new FixerApiClient(httpClientFactory.CreateClient());
            _context = context;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public async Task<ActionResult<CurrencyExchangeResponse>> Post([FromBody] CurrencyExchangeRequest request)
        {
            try
            {
                var clientId = GetClientId();

                // Check if the client has exceeded the rate limit
                if (!IsClientAllowedToTrade(clientId))
                {
                    return StatusCode(429, "You have exceeded the limit of 10 trades per hour.");
                }

                var exchangeRate = await _fixerApiClient.GetExchangeRate(request.FromCurrency, request.ToCurrency, request.Amount);
                var convertedAmount = request.Amount * exchangeRate;
                var response = new CurrencyExchangeResponse
                {
                    FromCurrency = request.FromCurrency,
                    ToCurrency = request.ToCurrency,
                    Amount = request.Amount,
                    ConvertedAmount = convertedAmount,
                    ExchangeRate = exchangeRate
                };

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

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // Add a new endpoint to retrieve trade history
        [HttpGet("trades")]
        public async Task<ActionResult<IEnumerable<CurrencyExchangeTrade>>> GetTrades()
        {
            return Ok(await _context.CurrencyExchangeTrades.ToListAsync());
        }

        private string GetClientId()
        {
            // Use the client's IP address as the identifier (you can use any other unique identifier)
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

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

        private void IncrementTradeCount(string clientId)
        {
            const int cacheExpirationMinutes = 60;

            if (!_memoryCache.TryGetValue(clientId, out int tradeCount))
            {
                // Cache entry not found, add a new entry with a count of 1
                _memoryCache.Set(clientId, 1, TimeSpan.FromMinutes(cacheExpirationMinutes));
            }
            else
            {
                // Increment the trade count and update the cache entry
                _memoryCache.Set(clientId, tradeCount + 1, TimeSpan.FromMinutes(cacheExpirationMinutes));
            }
        }

    }
}
