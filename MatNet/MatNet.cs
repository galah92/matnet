using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Reflection;

namespace MatNet
{

    public static class Server
    {

        public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static MatQueue<Message> matQueue;
        private static ConcurrentDictionary<string, MatSet<WebSocket>> store;
        private static WebSocketServer server = new WebSocketServer() { OnRecieve = HandleServerRecieve };

        public static int ClientsCount => server.ClientsCount;

        public static void Start(string uriString = "http://+:1234/")
        {
            Stop();
            matQueue = new MatQueue<Message>();
            store = new ConcurrentDictionary<string, MatSet<WebSocket>>();
            server.Start(uriString);
        }

        public static void Stop()
        {
            if (!server.IsListening) { return; }
            server.Stop();
        }

        public static void Send(Message msg)
        {
            if (!server.IsListening) { return; }
            server.Broadcast(JsonConvert.SerializeObject(msg));
        }

        public static void Store(Message msg)
        {
            if (store.TryGetValue(msg.ID, out MatSet<WebSocket> clients))
            {  // ID exists in store
                server.Broadcast(JsonConvert.SerializeObject(msg), clients);
            }
            store[msg.ID] = new MatSet<WebSocket>();
        }

        public static Message Receive() => matQueue.Dequeue();

        private static void HandleServerRecieve(WebSocket client, string data)
        {
            Message msg;
            try
            {
                msg = JsonConvert.DeserializeObject<Message>(data);
            }
            catch { return; }  // Error in parsing, data is probably corrupt
            switch (msg.Type)
            {
                case "COMMAND":
                    matQueue.Enqueue(msg);
                    break;
                case "QUERY":
                    if (store.TryGetValue(msg.ID, out MatSet<WebSocket> clients))
                    {  // ID exists in store, so add the client to the set
                        store[msg.ID].Add(client);
                    }
                    else
                    {  // ID doesn't exists in store, so init new set with the client
                        store[msg.ID] = new MatSet<WebSocket>() { client };
                    }
                    break;
            }
        }

    }

}
