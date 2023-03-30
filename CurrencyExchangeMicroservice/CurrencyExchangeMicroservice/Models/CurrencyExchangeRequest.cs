namespace CurrencyExchangeMicroservice.Models
{
    // Define the CurrencyExchangeRequest class to represent a currency exchange request.
    public class CurrencyExchangeRequest
    {
        // Properties for the source and target currencies and the amount to exchange.
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }

}
