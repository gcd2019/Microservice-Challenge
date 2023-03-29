using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using CurrencyExchangeMicroservice.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace CurrencyExchangeMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly FixerApiClient _fixerApiClient;
        private readonly CurrencyExchangeDbContext _context;
        public CurrencyExchangeController(IHttpClientFactory httpClientFactory, CurrencyExchangeDbContext context)
        {
            _fixerApiClient = new FixerApiClient(httpClientFactory.CreateClient());
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<CurrencyExchangeResponse>> Post([FromBody] CurrencyExchangeRequest request)
        {
            try
            {
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

    }
}
