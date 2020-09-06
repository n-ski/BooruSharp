using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Extensions
{
    internal static class JsonElementExtensions
    {
        public static bool? GetBool(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop)
                ? prop.GetBoolean() : (bool?)null;
        }

        public static DateTime? GetDateTime(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop)
                ? prop.GetDateTime() : (DateTime?)null;
        }

        public static int? GetInt32(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop)
                ? prop.GetInt32() : (int?)null;
        }

        public static string GetString(this in JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop)
                ? prop.GetString() : null;
        }

        public static Uri GetUri(this in JsonElement element, string propertyName)
        {
            var uriString = element.GetString(propertyName);
            return uriString != null ? new Uri(uriString) : null;
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
