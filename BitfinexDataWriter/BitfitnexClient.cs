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
        private ClientWebSocket _webSocket;
        private Uri _uri;
        private const int receiveChunkSize = 1000; //можно вообще вычислить, исходя из формата заявки

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
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //        do
                //        {
                //            result = await client.ReceiveAsync(message, token);
                //            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //            resultMessage.Append(receivedMessage);
                //            if (result.MessageType != WebSocketMessageType.Text)
                //                break;

                //        } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    Console.WriteLine($"{DateTime.UtcNow} : {receivedMessage}");
                }
            }
        }

    public async Task Send(string message, CancellationToken cancellationToken)
        {
            //BfxValidations.ValidateInput(message, nameof(message));

            //Log.Debug(L($"Sending:  {message}"));
            var buffer = Encoding.UTF8.GetBytes(message);
            var messageSegment = new ArraySegment<byte>(buffer);
            await _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
