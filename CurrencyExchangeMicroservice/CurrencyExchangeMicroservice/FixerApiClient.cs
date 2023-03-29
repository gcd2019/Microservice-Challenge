using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;

namespace CurrencyExchangeMicroservice
{
    public class FixerApiClient
    {
        private const string ApiKey = "YnhaO8i9JfkyfVMEKWAUUsL6cpfaMDjp";
        private const string BaseUrl = "https://api.apilayer.com/fixer/";

        private readonly HttpClient _httpClient;

        public FixerApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency, decimal amount)
        {
            var client = new RestClient($"{BaseUrl}convert?to={toCurrency}&from={fromCurrency}&amount={amount}");
            client.Timeout = -1;

            var request = new RestRequest(Method.GET);
            request.AddHeader("apikey", ApiKey);

            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error getting exchange rate: {response.ErrorMessage}");
            }

            var result = JsonConvert.DeserializeObject<FixerConversionResponse>(response.Content);
            return result.Result;
        }
    }
}
