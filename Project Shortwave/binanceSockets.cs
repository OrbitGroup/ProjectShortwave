using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.Json;

namespace Project_Shortwave
{
    public class binanceSockets
    {
        public string URL = "wss://stream.binance.com:9443/ws";
        WebSocketAdapter client;
        public int ID = 1;

        public async Task connectSocketsAsync(HashSet<string> targetMarkets)
        {
            client = await connectClientAsync();
            foreach(var ticker in targetMarkets)
            {
                var market = new Market();
                market.symbol = ticker;
                market.exchange = "binance";
                ProjectShortwave.binanceMarkets.TryAdd(ticker, market);
                var cts = new CancellationToken();
                await client.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes($"{{\"method\": \"SUBSCRIBE\",\"params\": [\"{ticker.ToLower()}@ticker\"], \"id\": {ID} }}")), WebSocketMessageType.Text, true, cts);
                await Task.Delay(500); //wait 500 ms for the connection to be established
                ID++;
            }
            listenAsync();
        }
        public async Task listenAsync()
        {
            while (client.State == WebSocketState.Open)
            {
                var buffer = WebSocket.CreateClientBuffer(1024 * 64, 1024);
                WebSocketReceiveResult result = null;
                using (var ms = new MemoryStream())
                {
                    result = await client.ReceiveAsync(ms, buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var reader = new StreamReader(ms, Encoding.UTF8);
                        var payload = await reader.ReadToEndAsync();
                        var rootelement = JsonDocument.Parse(payload).RootElement;

                        if(rootelement.TryGetProperty("e", out var _)) //the property 'e' is given for trade messages
                        {
                            var symbol = rootelement.GetProperty("s").GetString();   
                            ProjectShortwave.binanceMarkets[symbol].lastPrice = decimal.Parse(rootelement.GetProperty("p").GetString());
                            ProjectShortwave.binanceMarkets[symbol].askPrice = decimal.Parse(rootelement.GetProperty("a").GetString());
                            ProjectShortwave.binanceMarkets[symbol].askSize = decimal.Parse(rootelement.GetProperty("A").GetString());
                            ProjectShortwave.binanceMarkets[symbol].bidPrice = decimal.Parse(rootelement.GetProperty("b").GetString());
                            ProjectShortwave.binanceMarkets[symbol].bidSize = decimal.Parse(rootelement.GetProperty("B").GetString());
                            ProjectShortwave.binanceMarkets[symbol].dailyVolume = decimal.Parse(rootelement.GetProperty("q").GetString());
                            ProjectShortwave.binanceMarkets[symbol].timeStamp = DateTime.UtcNow;
                        }
                    }
                }
            }
        }
        public async Task<WebSocketAdapter> connectClientAsync()
        {
            var client = new ClientWebSocket();
            var adapter = new WebSocketAdapter(client);

            client.Options.KeepAliveInterval = new TimeSpan(0, 0, 5);
            await client.ConnectAsync(new Uri(URL), CancellationToken.None);
            return adapter;
        }
    }
}
