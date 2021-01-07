using System;
using System.Collections.Generic;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Project_Shortwave
{
    public class WebSocketAdapter
    {
        private readonly ClientWebSocket _client;

        public WebSocketAdapter(ClientWebSocket client)
        {
            _client = client;
        }

        public WebSocketState State => _client.State;

        public async Task<WebSocketReceiveResult> ReceiveAsync(MemoryStream ms, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            WebSocketReceiveResult result = null;

            do
            {
                try
                {
                    result = await _client.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
            while (!result.EndOfMessage && !cancellationToken.IsCancellationRequested);
            ms.Seek(0, SeekOrigin.Begin);
            return result;
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType type, bool endOfMessage, CancellationToken cancellationToken)
        {
            await _client.SendAsync(buffer, type, endOfMessage, cancellationToken);
        }
    }
}
