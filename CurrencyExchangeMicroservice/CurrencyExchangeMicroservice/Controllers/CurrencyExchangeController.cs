using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeMicroservice.Models;
using System;
using System.Net.Http;

namespace CurrencyExchangeMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly FixerApiClient _fixerApiClient;

        public CurrencyExchangeController(HttpClient httpClient)
        {
            _fixerApiClient = new FixerApiClient();
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
