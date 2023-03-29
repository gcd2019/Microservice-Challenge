namespace CurrencyExchangeMicroservice.Models
{
    public class CurrencyExchangeResponse
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
