namespace CurrencyExchangeMicroservice
{
    public class FixerConversionResponse
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public decimal Result { get; set; }
    }

}
