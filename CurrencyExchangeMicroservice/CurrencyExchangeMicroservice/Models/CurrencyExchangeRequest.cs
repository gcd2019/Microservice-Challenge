namespace CurrencyExchangeMicroservice.Models
{
    public class CurrencyExchangeRequest
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }

}
