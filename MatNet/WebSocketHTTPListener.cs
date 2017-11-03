using System.Net;
using System.Threading.Tasks;

namespace MatNet
{

    internal class WebSocketHttpListener
    {

        private HttpListener listener = new HttpListener();

        public bool IsListening => listener.IsListening;

        public HttpListenerPrefixCollection Prefixes => listener.Prefixes;

        public void Start() => listener.Start();

        public void Stop() => listener.Stop();

        public async Task<HttpListenerContext> GetContextAsync()
        {
            HttpListenerContext context = await listener.GetContextAsync();
            while (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
                context = await listener.GetContextAsync();
            }
            return context;
        }
    }

}
