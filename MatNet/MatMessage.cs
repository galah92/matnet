using System.Linq;
using Newtonsoft.Json.Linq;

namespace MatNet
{

    public class Message
    {

        public string Type = string.Empty;
        public string ID;
        public JObject Payload = new JObject();

        public Message(string id = "") => ID = id;

        public string[] GetKeys() => Payload.Properties().Select(p => p.Name).ToArray();

        public void Set(string name, object obj) => Payload[name] = JToken.FromObject(obj);

        public object Get(string name) => ToObject(Payload[name]);

        private object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>().ToDictionary(
                        prop => prop.Name,
                        prop => ToObject(prop.Value)
                    );
                case JTokenType.Array:
                    return token.Select(ToObject).ToArray();
                default:
                    return ((JValue)token).Value;
            }
        }

    }

}
