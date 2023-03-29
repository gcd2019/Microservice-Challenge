using System;

namespace CurrencyExchangeMicroservice.Models
{
    public class CurrencyExchangeTrade
    {
        public Guid Id { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
