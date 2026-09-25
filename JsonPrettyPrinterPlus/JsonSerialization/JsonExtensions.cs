using System.Text.Json;

namespace JsonPrettyPrinterPlus.JsonSerialization
{
    /// <summary>
    /// Serialisation helpers. Since 2.0.0 these use System.Text.Json (JavaScriptSerializer does not exist outside
    /// .NET Framework), so dates come out as ISO 8601 and property names keep their C# casing, as before.
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = false,
        };

        public static string ToJSON(this object graph)
        {
            return graph.ToJSON(false);
        }

        public static string ToJSON(this object graph, bool prettyPrint)
        {
            var unprettyJson = JsonSerializer.Serialize(graph, graph == null ? typeof(object) : graph.GetType(), Options);

            if (!prettyPrint)
                return unprettyJson;

            return unprettyJson.PrettyPrintJson();
        }

        public static T DeserializeFromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
