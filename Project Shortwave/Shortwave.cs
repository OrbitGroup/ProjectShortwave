using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Project_Shortwave
{
    public class ProjectShortwave //hypothesis: when identical markets are traded on different exchanges with wide disparity in volume traded, the price-action on the exchange with greater volume will 'lead' the price-action on the other exchange, presenting a trading opportunity.
    {
        public static ConcurrentDictionary<string, Market> binanceMarkets = new ConcurrentDictionary<string, Market>();
        public static ConcurrentDictionary<string, Market> bitrueMarkets = new ConcurrentDictionary<string, Market>();
        public static HashSet<string> targetMarkets; //the 5 most disproportionately traded markets

        static async Task Main(string[] args)
        {
            targetMarkets = marketMatcher.targetMarkets;
            var socket = new binanceSockets();
            var bitrue = new bitrueRESTlistener();
            socket.connectSocketsAsync(targetMarkets); //establish binance websocket streams
            bitrue.listenBitrue(); //hit the REST API for bitrue as many times as the server will let us (the rate limit is about 42 requests per second)
            await monitorMarkets();
        }
        static async Task monitorMarkets() //print the timestamp and correlation of markets every 0.5 seconds. Format is comma delineated to be easily workable in excel/PowerBI
        {
            await Task.Delay(2000);
            while(binanceMarkets.Count > 0 && bitrueMarkets.Count > 0 )
            {
                decimal correlation = 0;
                foreach(var binanceMarket in binanceMarkets)
                {
                    var timeNow = DateTime.UtcNow;

                    if(bitrueMarkets[binanceMarket.Key].askPrice != 0 && binanceMarket.Value.lastPrice != 0)
                    {
                        correlation = Math.Round(binanceMarket.Value.lastPrice / bitrueMarkets[binanceMarket.Key].askPrice, 3);
                        Console.WriteLine($"Time,{timeNow},{binanceMarket.Key},Binance_Price,{binanceMarket.Value.lastPrice},Bitrue_Price,{bitrueMarkets[binanceMarket.Key].askPrice},Correlation,{correlation}");
                    }
                    await Task.Delay(500);
                }
            }
        }
    }
}
