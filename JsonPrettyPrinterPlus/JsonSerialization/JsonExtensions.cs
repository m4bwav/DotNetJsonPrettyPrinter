using System;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace JsonPrettyPrinterPlus.JsonSerialization
{
    /// <summary>
    /// Serialisation helpers over System.Text.Json (since 2.0.0; JavaScriptSerializer does not exist outside
    /// .NET Framework). Dates come out as ISO 8601 and property names keep their C# casing. The overloads that
    /// take a <see cref="JsonTypeInfo{T}"/> are safe for trimming and native AOT; the others use reflection.
    /// </summary>
    public static class JsonExtensions
    {
        private const string ReflectionWarning =
            "Uses reflection-based System.Text.Json serialisation. Pass a JsonTypeInfo<T> from a JsonSerializerContext for trimmed or AOT applications.";

        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
        };

        /// <summary>Serialises <paramref name="graph"/> as compact JSON.</summary>
        /// <returns>The JSON text; <c>null</c> serialises as <c>null</c>.</returns>
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReflectionWarning)]
        [RequiresDynamicCode(ReflectionWarning)]
#endif
        public static string ToJson(this object? graph)
        {
            return graph.ToJson(DefaultOptions, prettyPrint: false);
        }

        /// <summary>Serialises <paramref name="graph"/> and optionally pretty prints the result.</summary>
        /// <param name="graph">The object to serialise; <c>null</c> serialises as <c>null</c>.</param>
        /// <param name="prettyPrint">When true, the JSON is passed through <see cref="PrettyPrinterExtensions.PrettyPrintJson(string)"/>.</param>
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReflectionWarning)]
        [RequiresDynamicCode(ReflectionWarning)]
#endif
        public static string ToJson(this object? graph, bool prettyPrint)
        {
            return graph.ToJson(DefaultOptions, prettyPrint);
        }

        /// <summary>Serialises <paramref name="graph"/> with the given serializer options and optionally pretty prints the result.</summary>
        /// <param name="graph">The object to serialise; <c>null</c> serialises as <c>null</c>.</param>
        /// <param name="options">System.Text.Json options; <c>null</c> means the library defaults.</param>
        /// <param name="prettyPrint">When true, the JSON is passed through <see cref="PrettyPrinterExtensions.PrettyPrintJson(string)"/>.</param>
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReflectionWarning)]
        [RequiresDynamicCode(ReflectionWarning)]
#endif
        public static string ToJson(this object? graph, JsonSerializerOptions? options, bool prettyPrint = false)
        {
            var json = JsonSerializer.Serialize(graph, graph == null ? typeof(object) : graph.GetType(), options ?? DefaultOptions);

            return prettyPrint ? json.PrettyPrintJson() : json;
        }

        /// <summary>Serialises <paramref name="graph"/> with source-generated metadata (trim and AOT safe) and optionally pretty prints the result.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="typeInfo"/> is null.</exception>
        public static string ToJson<T>(this T graph, JsonTypeInfo<T> typeInfo, bool prettyPrint = false)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            var json = JsonSerializer.Serialize(graph, typeInfo);

            return prettyPrint ? json.PrettyPrintJson() : json;
        }

        /// <summary>Deserialises <paramref name="json"/> as a <typeparamref name="T"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReflectionWarning)]
        [RequiresDynamicCode(ReflectionWarning)]
#endif
        public static T? DeserializeFromJson<T>(this string json)
        {
            return json.DeserializeFromJson<T>(DefaultOptions);
        }

        /// <summary>Deserialises <paramref name="json"/> as a <typeparamref name="T"/> with the given serializer options.</summary>
        /// <param name="json">The JSON text.</param>
        /// <param name="options">System.Text.Json options; <c>null</c> means the library defaults.</param>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(ReflectionWarning)]
        [RequiresDynamicCode(ReflectionWarning)]
#endif
        public static T? DeserializeFromJson<T>(this string json, JsonSerializerOptions? options)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }

        /// <summary>Deserialises <paramref name="json"/> with source-generated metadata (trim and AOT safe).</summary>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> or <paramref name="typeInfo"/> is null.</exception>
        public static T? DeserializeFromJson<T>(this string json, JsonTypeInfo<T> typeInfo)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            return JsonSerializer.Deserialize(json, typeInfo);
        }
    }
}
