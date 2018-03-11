using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitfinexDataWriter.Responses;
using Newtonsoft.Json.Linq;

namespace BitfinexDataWriter
{
    public class BitfitnexClient
    {
        private const int ReceiveChunkSize = 256; // можно вообще вычислить, исходя из формата заявки

        private readonly Uri _uri;
        private readonly ClientWebSocket _webSocket;

        public delegate void SubscribeResponse(SubscribedResponse response);
        public delegate void DataReceive(JArray message);

        public event SubscribeResponse OnSubscribeResponse;
        public event DataReceive OnDataReceive;

        public BitfitnexClient(Uri uri)
        {
            _uri = uri;
            _webSocket = new ClientWebSocket();
        }

        public void Listen(CancellationToken cancellationToken)
        {
            _webSocket.ConnectAsync(_uri, cancellationToken).Wait();
            Receive(cancellationToken);
        }

        public async Task Send(string message, CancellationToken cancellationToken)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var messageSegment = new ArraySegment<byte>(buffer);
            await _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, cancellationToken);
        }

        private async Task Receive(CancellationToken cancellationToken)
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[ReceiveChunkSize];
                var responseMessage = new StringBuilder();
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken)
                        .ConfigureAwait(false);
                    var receiveResultString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    responseMessage.Append(receiveResultString);
                }
                while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None)
                        .ConfigureAwait(false);
                }
                else
                {
                    // Console.WriteLine(responseMessage.ToString());
                    HandleMessage(responseMessage.ToString());
                }
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                var formatted = (message ?? string.Empty).Trim();

                if (formatted.StartsWith("{"))
                {
                    OnObjectMessage(formatted);
                    return;
                }

                if (formatted.StartsWith("["))
                {
                    OnArrayMessage(formatted);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while receiving message: {e.Message}");
            }
        }

        private void OnArrayMessage(string msg)
        {
            var parsed = BitfinexJsonSerializer.Deserialize<JArray>(msg);
            if (parsed.Count < 2)
            {
                Console.WriteLine("Invalid message format, too low items");
                return;
            }

            OnDataReceive?.Invoke(parsed);           
        }

        private void OnObjectMessage(string msg)
        {
            var response = BitfinexJsonSerializer.Deserialize<SubscribedResponse>(msg);
            OnSubscribeResponse?.Invoke(response);
        }
    }
}
