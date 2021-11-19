using System;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QuoteTree;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace OnlinePriceSystem.Controllers
{
    public class NodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ANode));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JsonReader reader2 = CreateDeepCopy<JsonReader>(reader);
            string type;
            do
            {
                reader2.Read();
            }
            while (reader2.Value != null && reader2.Value.ToString() != "TypeStr");

            reader2.Read();
            type = reader2.Value.ToString();

            object obj = null;
            switch (type)
            {
                case "Math":
                    obj = serializer.Deserialize(reader, typeof(MathNode));
                    break;
                case "Decision":
                    obj = serializer.Deserialize(reader, typeof(DecisionNode));
                    break;
                case "Conditional":
                    obj = serializer.Deserialize(reader, typeof(ConditionalNode));
                    break;
                case "Range":
                    obj = serializer.Deserialize(reader, typeof(RangeNode));
                    break;
                default:
                    // code block
                    break;
            }

            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch(((ANode)value).Type)
            {
                case NodeType.Math:
                    // code block
                    serializer.Serialize(writer, value, typeof(MathNode));
                    break;
                case NodeType.Decision:
                    // code block
                    serializer.Serialize(writer, value, typeof(DecisionNode));
                    break;
                case NodeType.Conditional:
                    // code block
                    serializer.Serialize(writer, value, typeof(ConditionalNode));
                    break;
                case NodeType.Range:
                    // code block
                    serializer.Serialize(writer, value, typeof(RangeNode));
                    break;
                default:
                    // code block
                    break;
            }

        }

        public static T CreateDeepCopy<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}

