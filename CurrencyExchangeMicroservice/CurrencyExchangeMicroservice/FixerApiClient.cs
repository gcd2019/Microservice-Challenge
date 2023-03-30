using Newtonsoft.Json;
using RestSharp;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CurrencyExchangeMicroservice
{
    public class FixerApiClient
    {
        private readonly string ApiKey;
        private const string BaseUrl = "https://api.apilayer.com/fixer/";

        private readonly HttpClient _httpClient;
        private readonly ILogger<FixerApiClient> _logger;

        public FixerApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<FixerApiClient> logger)
        {
            _httpClient = httpClient;
            ApiKey = configuration.GetValue<string>("FixerApi:ApiKey");
            _logger = logger;
        }

        private readonly ConcurrentDictionary<string, (decimal Rate, DateTime Timestamp)> _exchangeRateCache = new();

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency, decimal amount)
        {
            _logger.LogInformation("Getting exchange rate for {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);

            string cacheKey = $"{fromCurrency}-{toCurrency}";
            if (_exchangeRateCache.TryGetValue(cacheKey, out var rateInfo) && !IsRateExpired(rateInfo.Timestamp))
            {
                _logger.LogInformation("Using cached exchange rate: {Rate}", rateInfo.Rate);
                return rateInfo.Rate;
            }
            else
            {
                decimal newRate = await GetExchangeRateFromApi(fromCurrency, toCurrency, amount);
                _exchangeRateCache[cacheKey] = (newRate, DateTime.UtcNow);
                _logger.LogInformation("Fetched new exchange rate from API: {Rate}", newRate);
                return newRate;
            }
        }

        private bool IsRateExpired(DateTime timestamp)
        {
            return (DateTime.UtcNow - timestamp).TotalMinutes > 30;
        }

        public async Task<decimal> GetExchangeRateFromApi(string fromCurrency, string toCurrency, decimal amount)
        {
            var client = new RestClient($"{BaseUrl}convert?to={toCurrency}&from={fromCurrency}&amount={amount}");
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("apikey", ApiKey);

            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError("Error getting exchange rate from API: {ErrorMessage}", response.ErrorMessage);
                throw new Exception($"Error getting exchange rate: {response.ErrorMessage}");
            }

            var result = JsonConvert.DeserializeObject<FixerConversionResponse>(response.Content);
            return result.Result;
        }
    }
}
