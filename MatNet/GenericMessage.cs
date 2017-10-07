using System.Linq;
using Newtonsoft.Json.Linq;

namespace MatNet
{

    class GenericMessage
    {

        public string Type;
        public string ID;
        public JObject Payload = new JObject();

        public GenericMessage(string type = "", string id = "")
        {
            Type = type;
            ID = id;
        }

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

        public string[] Keys => Payload.Properties().Select(p => p.Name).ToArray();

    }

}