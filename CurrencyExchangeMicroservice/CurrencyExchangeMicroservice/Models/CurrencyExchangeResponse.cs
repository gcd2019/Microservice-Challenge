namespace CurrencyExchangeMicroservice.Models
{
    // Define the CurrencyExchangeResponse class to represent a currency exchange response.
    public class CurrencyExchangeResponse
    {
        // Properties for the source and target currencies, the original amount,
        // the converted amount, and the exchange rate.
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
