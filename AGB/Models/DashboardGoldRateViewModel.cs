using System;

namespace AparnaGoldBuyers.Models
{
    public class DashboardGoldRateViewModel
    {
        public bool IsAvailable { get; set; }
        public string ErrorMessage { get; set; }
        public decimal RatePerGram24K { get; set; }
        public decimal RatePer10Grams24K { get; set; }
        public decimal RatePerGram22K { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime DisplayDate { get; set; }
        public DateTime? LastUpdatedIndia { get; set; }
    }
}
