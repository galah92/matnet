using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace MatNet
{

    internal class WebSocketServer
    {

        private WebSocketHttpListener listener;
        private MatSet<WebSocket> clients;
        private const int BUFFER_SIZE = 8192;

        public bool IsListening => listener == null || listener.IsListening;
        public int ClientsCount => clients.Count;

        public void Start(string uriString)
        {
            if (listener != null && listener.IsListening) { return; }
            clients = new MatSet<WebSocket>();
            listener = new WebSocketHttpListener();
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
                    HttpListenerContext listenerContext = await listener.GetContextAsync();
                    WebSocketContext webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                    WebSocket webSocket = webSocketContext.WebSocket;
                    clients.Add(webSocket);
                    await HandleClient(webSocket);
                }
            }
            catch (HttpListenerException) { } // Got here probably because StopWSServer() was called
        }

        private async Task HandleClient(WebSocket client)
        {
            try
            {
                ArraySegment<byte> recievedBuffer = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
                while (listener != null && listener.IsListening && client.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult recieveResult;
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
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
                                OnRecieve?.Invoke(client, System.Text.Encoding.UTF8.GetString(ms.ToArray()));
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

        public Action<WebSocket, string> OnRecieve;

        public void Broadcast(string data) => Broadcast(data, clients);

        public void Broadcast(string data, IEnumerable<WebSocket> clients)
        {
            foreach (var client in clients)
            {
                Send(client, data);
            }
        }

        public async void Send(WebSocket client, string data)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.ASCII.GetBytes(data));
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
            clients.Remove(client);
        }

    }

}
