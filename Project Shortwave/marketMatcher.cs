using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Project_Shortwave
{
    public class marketMatcher
    {
        public static HashSet<string> targetMarkets = new HashSet<string>();

        static marketMatcher()
        {
            var leaderMarkets = new List<Market>();
            var laggerMarkets = new List<Market>();


            var httpClient = new HttpClient();
            var binanceTickers = JsonDocument.ParseAsync(httpClient.GetStreamAsync("https://api.binance.com/api/v3/ticker/24hr").Result).Result.RootElement.EnumerateArray();
            foreach (var item in binanceTickers)
            {
                if (item.GetProperty("symbol").ToString().EndsWith("BTC")) //we are only interested in BTC quoted markets (for now)
                {
                    var market = new Market();
                    market.symbol = item.GetProperty("symbol").ToString();
                    market.dailyVolume = decimal.Parse(item.GetProperty("quoteVolume").ToString());
                    market.exchange = "binance";
                    leaderMarkets.Add(market);
                }
            }

            var bitrueTickers = JsonDocument.ParseAsync(httpClient.GetStreamAsync("https://www.bitrue.com/api/v1/ticker/24hr").Result).Result.RootElement.EnumerateArray();
            foreach (var item in bitrueTickers)
            {
                if (item.GetProperty("symbol").ToString().EndsWith("BTC")) //we are only interested in BTC quoted markets (for now)
                {
                    var market = new Market();
                    market.symbol = item.GetProperty("symbol").ToString();
                    market.dailyVolume = decimal.Parse(item.GetProperty("quoteVolume").ToString()); //bitrue only expresses volume in BTC
                    market.exchange = "bitrue";
                    laggerMarkets.Add(market);

                }
            }
            httpClient.Dispose();

            List<KeyValuePair<Market, decimal>> matchedMarkets = new List<KeyValuePair<Market, decimal>>();
            foreach (var leaderMarket in leaderMarkets)
            {
                foreach (var laggerMarket in laggerMarkets)
                {
                    if (laggerMarket.dailyVolume > 1)
                    {
                        if (leaderMarket.symbol == laggerMarket.symbol)
                        {
                            var volumeDisparity = Math.Round((leaderMarket.dailyVolume / laggerMarket.dailyVolume), 3);
                            matchedMarkets.Add(new KeyValuePair<Market, decimal>(leaderMarket, volumeDisparity));
                        }
                    }
                }
            }
            var matchedMarketsSorted = matchedMarkets.OrderByDescending(o => o.Value).Take(5);
            Console.WriteLine("the top five most disproportionately traded markets are:");
            foreach (var market in matchedMarketsSorted)
            {
                Console.WriteLine($"market: {market.Key.symbol} -- volume is {market.Value} times higher on Binance compared to Bitrue");
                targetMarkets.Add(market.Key.symbol);
            }
        }
    }
}
