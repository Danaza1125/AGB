using AparnaGoldBuyers.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AparnaGoldBuyers.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var model = await GetTodaysGoldRateAsync();
            return View(model);
        }

        private async Task<DashboardGoldRateViewModel> GetTodaysGoldRateAsync()
        {
            var indiaNow = GetIndiaNow();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // Inferred integration target: gold-api.com provides a free real-time price endpoint,
                    // and we request the quote directly in INR for India display.
                    var response = await client.GetStringAsync("https://api.gold-api.com/price/XAU/INR");
                    var data = JObject.Parse(response);
                    var ouncePriceInInr = data.Value<decimal?>("price");

                    if (!ouncePriceInInr.HasValue || ouncePriceInInr.Value <= 0)
                    {
                        return CreateUnavailableModel(indiaNow, "Today's gold rate in India is not available right now.");
                    }

                    var ratePerGram24K = Math.Round(ouncePriceInInr.Value / 31.1034768m, 2);

                    return new DashboardGoldRateViewModel
                    {
                        IsAvailable = true,
                        CurrencyCode = data.Value<string>("currency") ?? "INR",
                        RatePerGram24K = ratePerGram24K,
                        RatePer10Grams24K = Math.Round(ratePerGram24K * 10m, 2),
                        RatePerGram22K = Math.Round(ratePerGram24K * (22m / 24m), 2),
                        DisplayDate = indiaNow.Date,
                        LastUpdatedIndia = GetLastUpdatedIndia(data) ?? indiaNow
                    };
                }
            }
            catch
            {
                return CreateUnavailableModel(indiaNow, "Unable to fetch today's gold rate in India at the moment.");
            }
        }

        private static DashboardGoldRateViewModel CreateUnavailableModel(DateTime indiaNow, string message)
        {
            return new DashboardGoldRateViewModel
            {
                IsAvailable = false,
                ErrorMessage = message,
                CurrencyCode = "INR",
                DisplayDate = indiaNow.Date
            };
        }

        private static DateTime GetIndiaNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetIndiaTimeZone());
        }

        private static DateTime? GetLastUpdatedIndia(JObject data)
        {
            var updatedAt = data.Value<string>("updatedAt");
            DateTime parsedDate;

            if (!string.IsNullOrWhiteSpace(updatedAt) &&
                DateTime.TryParse(updatedAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(parsedDate.ToUniversalTime(), GetIndiaTimeZone());
            }

            var timestamp = data.Value<long?>("timestamp");
            if (timestamp.HasValue && timestamp.Value > 0)
            {
                var utcDate = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime;
                return TimeZoneInfo.ConvertTimeFromUtc(utcDate, GetIndiaTimeZone());
            }

            return null;
        }

        private static TimeZoneInfo GetIndiaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
