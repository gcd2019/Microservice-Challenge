namespace CurrencyExchangeMicroservice
{
    // Define the FixerConversionResponse class to represent a currency conversion response from the Fixer API.
    public class FixerConversionResponse
    {
        // Properties for the source and target currencies, the original amount, and the converted amount.
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public decimal Result { get; set; }
    }

}
