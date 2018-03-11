using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitfinexDataWriter.Aggregator;
using BitfinexDataWriter.Responses;
using Newtonsoft.Json.Linq;

namespace BitfinexDataWriter
{
    public class BitfitnexClient
    {
        private const int ReceiveChunkSize = 256; // можно вообще вычислить, исходя из формата заявки

        private readonly IAggregatorsStorage _aggregatorsStorage;
        private readonly Uri _uri;
        private readonly ClientWebSocket _webSocket;

        public BitfitnexClient(IAggregatorsStorage aggregatorsStorage, Uri uri)
        {
            _aggregatorsStorage = aggregatorsStorage;
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

            var channelId = (int)parsed[0];
            OnBook(parsed, channelId);
        }

        private void OnObjectMessage(string msg)
        {
            var response = BitfinexJsonSerializer.Deserialize<SubscribedResponse>(msg);
            if (response.Event == "subscribed")
            {
                _aggregatorsStorage.CreateAggregator(response.ChannelId, response.Pair);
            }
        }

        private void OnBook(JToken token, int channelId)
        {
            var data = token[1];

            if (data.Type != JTokenType.Array)
            {
                return; // heartbeat, ignore
            }

            var aggregator = _aggregatorsStorage.GetAggregator(channelId);

            if (data.First.Type == JTokenType.Array)
            {
                var books = data.ToObject<Book[]>();

                aggregator.GetSnapshot(books);
            }
            else
            {
                var book = data.ToObject<Book>();
                aggregator.GetBook(book);
            }
        }
    }
}
