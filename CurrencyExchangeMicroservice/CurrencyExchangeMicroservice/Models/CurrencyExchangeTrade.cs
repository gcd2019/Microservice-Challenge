using System;

namespace CurrencyExchangeMicroservice.Models
{
    // Define the CurrencyExchangeTrade class to represent a currency exchange trade in the database.
    public class CurrencyExchangeTrade
    {
        // Properties for the trade ID, source and target currencies, original amount,
        // converted amount, exchange rate, and timestamp.
        public Guid Id { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
