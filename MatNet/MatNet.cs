using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Linq;
using Newtonsoft.Json;


namespace MatNet
{

    class MatNet
    {
        public static string Version => "v0.0.1";
        public static MatQueue<GenericMessage> CommandsQueue;
        public static MatQueue<GenericMessage> UpdatesQueue;
        public static int ClientsCount => server.ClientsCount;

        private static ConcurrentDictionary<string, Tuple<string, ConcurrentBag<WebSocket>>> Store;
        private static readonly WebSocketServer server = new WebSocketServer() { OnRecieveAction = HandleServerOnRecieve };

        public static void Start(string uriString = "http://+:1234/")
        {
            Stop();
            CommandsQueue = new MatQueue<GenericMessage>();
            UpdatesQueue = new MatQueue<GenericMessage>() { EnqueueAction = HandleGUIQueue };
            Store = new ConcurrentDictionary<string, Tuple<string, ConcurrentBag<WebSocket>>>();
            server.Start(uriString);
        }

        public static void Stop() => server.Stop();

        private static void HandleGUIQueue()
        {
            GenericMessage msg;
            if (server.IsListening && UpdatesQueue.TryDequeue(out msg))
            {
                string msgJSON = JsonConvert.SerializeObject(msg);
                switch (msg.Type)
                {
                    case "PUSH":
                        server.Broadcast(msgJSON);
                        break;
                    case "STORE":
                        Tuple<string, ConcurrentBag<WebSocket>> storeItem;
                        if (Store.TryGetValue(msg.ID, out storeItem))
                        {  // ID exists in Store
                            server.Broadcast(msgJSON, storeItem.Item2.AsEnumerable());
                        }
                        Store[msg.ID] = new Tuple<string, ConcurrentBag<WebSocket>>(msgJSON, new ConcurrentBag<WebSocket>());
                        break;
                }
            }
        }

        private static void HandleServerOnRecieve(WebSocket client, string data)
        {
            GenericMessage msg;
            try
            {
                msg = JsonConvert.DeserializeObject<GenericMessage>(data);
            }
            catch { return; }  // Error in parsing, data is probably corrupt
            switch (msg.Type)
            {
                case "COMMAND":
                    CommandsQueue.Enqueue(msg);
                    break;
                case "QUERY":
                    Tuple<string, ConcurrentBag<WebSocket>> storeItem;
                    if (Store.TryGetValue(msg.ID, out storeItem))
                    {  // data already exist in Store, so add the client to the bag
                        Store[msg.ID].Item2.Add(client);
                    }
                    else
                    {  // data doesn't exist in Store, so init new bag with the client
                        Store[msg.ID] = new Tuple<string, ConcurrentBag<WebSocket>>(string.Empty, new ConcurrentBag<WebSocket>() { client });
                    }
                    break;
            }
        }

    }

}