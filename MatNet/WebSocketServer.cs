using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatNet
{

    internal class WebSocketServer
    {

        private WebSocketHTTPListener listener;
        private ConcurrentBag<WebSocket> clients;
        private const int BUFFER_SIZE = 8192;

        public bool IsListening => listener == null || listener.IsListening;
        public int ClientsCount => clients.Count;

        public void Start(string uriString)
        {
            if (listener != null && listener.IsListening) { return; }
            clients = new ConcurrentBag<WebSocket>();
            listener = new WebSocketHTTPListener();
            listener.Prefixes.Add(uriString);
            listener.Start();
            HandleListener();
        }

        public void Stop()
        {
            if (listener == null) { return; }
            listener.Stop();
            listener = null;
            clients = null;
        }

        private async void HandleListener()
        {
            try
            {
                while (listener != null && listener.IsListening)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    WebSocket client = (await context.AcceptWebSocketAsync(null)).WebSocket;
                    clients.Add(client);
                    await HandleClient(client);
                }
            }
            catch (HttpListenerException) { }
        }

        private async Task HandleClient(WebSocket client)
        {
            try
            {
                while (listener != null && listener.IsListening && client.State == WebSocketState.Open)
                {
                    using (var ms = new MemoryStream())
                    {
                        WebSocketReceiveResult recieveResult;
                        do
                        {
                            ArraySegment<byte> recievedBuffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
                            recieveResult = await client.ReceiveAsync(recievedBuffer, CancellationToken.None);
                            ms.Write(recievedBuffer.Array, recievedBuffer.Offset, recieveResult.Count);
                        }
                        while (!recieveResult.EndOfMessage);
                        switch (recieveResult.MessageType)
                        {
                            case WebSocketMessageType.Close:
                                RemoveClient(client, WebSocketCloseStatus.NormalClosure, string.Empty);
                                break;
                            case WebSocketMessageType.Binary:
                                RemoveClient(client, WebSocketCloseStatus.InvalidMessageType, "Cannot accept binary frame");
                                break;
                            case WebSocketMessageType.Text:
                                OnRecieveAction?.Invoke(client, Encoding.ASCII.GetString(ms.ToArray()));
                                break;
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                RemoveClient(client, WebSocketCloseStatus.InternalServerError, ex.Message);
            }
        }

        public Action<WebSocket, string> OnRecieveAction;

        public void Broadcast(string message)
        {
            Broadcast(message, clients.AsEnumerable());
        }

        public void Broadcast(string message, IEnumerable<WebSocket> clients)
        {
            foreach (var client in clients)
            {
                Send(client, message);
            }
        }

        public async void Send(WebSocket client, string message)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(message));
                await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                RemoveClient(client, WebSocketCloseStatus.InternalServerError, ex.Message);
            }
        }

        private void RemoveClient(WebSocket client, WebSocketCloseStatus closeStatus, string message)
        {
            try
            {
                client.CloseAsync(closeStatus, message, CancellationToken.None);
            }
            catch { }
            clients.TryTake(out client);
        }

    }

}