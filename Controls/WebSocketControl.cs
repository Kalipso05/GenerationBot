using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GenerationBOT.Controls
{
    public class WebSocketControl
    {
        public static async Task<string> ReceiveMessageAsync(ClientWebSocket client)
        {
            var buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await client.ReceiveAsync(buffer, CancellationToken.None);
            return Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        }

        public static async Task SendMessageAsync(ClientWebSocket client, object message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonMessage);
            var buffer = new ArraySegment<byte>(bytes);
            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
