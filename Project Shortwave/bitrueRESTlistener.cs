using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project_Shortwave
{
    public class bitrueRESTlistener
    {
        public int rateLimit { get; set; } //number of requests allowed per second

        public bitrueRESTlistener()
        {
            getRateLimit();
        }
        public void getRateLimit()
        {
            var httpClient = new HttpClient();
            var bitrueinfo = JsonDocument.ParseAsync(httpClient.GetStreamAsync("https://www.bitrue.com/api/v1/exchangeInfo").Result).Result.RootElement;
            rateLimit = bitrueinfo.GetProperty("rateLimits")[0].GetProperty("limit").GetInt16();
            var interval = bitrueinfo.GetProperty("rateLimits")[0].GetProperty("interval").GetString();
            if(interval == "MINUTES")
            {
                rateLimit = rateLimit / 60;
            } else
            {
                Console.WriteLine($"interval was {interval}");
            }
        }
        public async Task listenBitrue()
        {
            var httpClient = new HttpClient();
            foreach(var ticker in ProjectShortwave.targetMarkets)
            {
                var market = new Market();
                market.symbol = ticker;
                ProjectShortwave.bitrueMarkets.TryAdd(ticker, market);
            }
            while(true)
            {
                foreach(var ticker in ProjectShortwave.targetMarkets)
                {
                    var orderbookTicker = JsonDocument.ParseAsync(httpClient.GetStreamAsync($"https://www.bitrue.com/api/v1/ticker/bookTicker?symbol={ticker}").Result).Result.RootElement;
                    ProjectShortwave.bitrueMarkets[ticker].timeStamp = DateTime.UtcNow;
                    ProjectShortwave.bitrueMarkets[ticker].bidPrice = decimal.Parse(orderbookTicker.GetProperty("bidPrice").GetString());
                    ProjectShortwave.bitrueMarkets[ticker].bidSize = decimal.Parse(orderbookTicker.GetProperty("bidQty").GetString());
                    ProjectShortwave.bitrueMarkets[ticker].askPrice = decimal.Parse(orderbookTicker.GetProperty("askPrice").GetString());
                    ProjectShortwave.bitrueMarkets[ticker].askSize = decimal.Parse(orderbookTicker.GetProperty("askQty").GetString());
                    await Task.Delay(1000 / rateLimit); //by waiting for this amount of time, it's impossible to exceed the rate limit for Bitrue.
                }
            }
        }
    }
}
