using BitfinexDataWriter.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitfinexDataWriter
{
    public class BitfitnexClient
    {
        private const int receiveChunkSize = 1000; //можно вообще вычислить, исходя из формата заявки

        private readonly Dictionary<int, Aggregator> _aggregators = new Dictionary<int, Aggregator>();

        private ClientWebSocket _webSocket;
        private Uri _uri;

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

        private async Task Receive(CancellationToken cancellationToken)
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[receiveChunkSize];
                var response = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                var responseMessage = Encoding.UTF8.GetString(buffer, 0, response.Count);

                //        do
                //        {
                //            result = await client.ReceiveAsync(message, token);
                //            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //            resultMessage.Append(receivedMessage);
                //            if (result.MessageType != WebSocketMessageType.Text)
                //                break;

                //        } while (!result.EndOfMessage);

                if (response.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    ;// Console.WriteLine($"{DateTime.UtcNow} : {responseMessage}");
                }

                HandleMessage(responseMessage);
            }
        }

        public async Task Send(string message, CancellationToken cancellationToken)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var messageSegment = new ArraySegment<byte>(buffer);
            await _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, cancellationToken);
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
            var parsed = Deserialize<JArray>(msg);
            if (parsed.Count < 2)
            {
                Console.WriteLine("Invalid message format, too low items");
                return;
            }

            var channelId = (int)parsed[0];
            OnBook(parsed, channelId);
        }

        private void OnObjectMessage(string msg)
        {
            var response = Deserialize<SubscribedResponse>(msg);
            if (response.Event == "subscribed")
                _aggregators.Add(response.ChannelId, new Aggregator(response.ChannelId, response.Pair));
        }

        private void OnBook(JToken token, int channelId)
        {
            var data = token[1];

            if (data.Type != JTokenType.Array)
            {
                return; // heartbeat, ignore
            }

            if (data.First.Type == JTokenType.Array)
            {
                var books = data.ToObject<Book[]>();

                // TODO: переписать
                _aggregators[channelId].GetSnapshot(books);
                return;
            }

            var book = data.ToObject<Book>();

            // TODO: переписать
            _aggregators[channelId].GetBook(book);
        }

        private T Deserialize<T>(string msg)
        {
            return JsonConvert.DeserializeObject<T>(msg, BitfinexJsonSerializer.Settings);
        }
    }
}
