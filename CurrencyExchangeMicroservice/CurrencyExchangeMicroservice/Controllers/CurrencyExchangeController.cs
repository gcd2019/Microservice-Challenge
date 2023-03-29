using Microsoft.AspNetCore.Mvc;
using CurrencyExchangeMicroservice.Models;
using System;

namespace CurrencyExchangeMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyExchangeController : ControllerBase
    {
        [HttpPost]
        public ActionResult<CurrencyExchangeResponse> Post([FromBody] CurrencyExchangeRequest request)
        {
            try
            {
                var exchangeRate = GetExchangeRate(request.FromCurrency, request.ToCurrency);
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

        private decimal GetExchangeRate(string fromCurrency, string toCurrency)
        {
            // Replace this with actual API call to get the exchange rate
            if (fromCurrency == "USD" && toCurrency == "EUR")
            {
                return 0.85m;
            }
            else if (fromCurrency == "EUR" && toCurrency == "USD")
            {
                return 1.18m;
            }
            else
            {
                throw new Exception("Unsupported currency pair");
            }
        }
    }
}