using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Extensions
{
    internal static class JsonElementExtensions
    {
        public static DateTime? GetDateTime(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop)
                // JsonElement throws an exception when trying to parse Lolibooru's
                // format of date/time, so we'll parse the string manually.
                ? DateTime.Parse(prop.GetString())
                : (DateTime?)null;
        }

        public static int? GetInt32(this in JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                switch (prop.ValueKind)
                {
                    // Some boorus return a number wrapped in quotes,
                    // so it needs to be read as a string.
                    case JsonValueKind.String:
                        return int.Parse(prop.GetString());

                    case JsonValueKind.Number:
                        return prop.GetInt32();

                    case JsonValueKind.Null:
                        return null;
                }
            }

            return null;
        }

        public static string GetString(this in JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind == JsonValueKind.String
                    ? prop.GetString() : prop.ToString();
            }

            return null;
        }

        public static Uri GetUri(this in JsonElement element, string propertyName)
        {
            var uriString = element.GetString(propertyName);
            return !string.IsNullOrEmpty(uriString) ? new Uri(uriString) : null;
        }

        public static bool HasProperty(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out _);
        }

        public static IEnumerable<T> Select<T>(this in JsonElement element, Func<JsonElement, T> selector)
        {
            return element.EnumerateArray().Select(selector);
        }
    }
}
