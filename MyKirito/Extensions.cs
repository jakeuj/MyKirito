using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace MyKirito
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(),
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true, IgnoreNullValues = true});
        }

        public static async Task<T> ReadAsJsonAsync<T>(this Stream content)
        {
            return await JsonSerializer.DeserializeAsync<T>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true, IgnoreNullValues = true});
        }

        public static async Task<T> ReadAsJsonAsync<TInput, T>(this TInput content) where TInput : Stream
        {
            return await content.ReadAsJsonAsync<T>();
        }

        public static T ReadAsJsonAsync<T>(this string content,bool ignoreNullValues=true)
        {
            return JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true, IgnoreNullValues = ignoreNullValues });
        }

        public static string ToJsonString<T>(this T content,bool ignoreNullValues=true)
        {
            return JsonSerializer.Serialize(content,
                new JsonSerializerOptions
                {
                    WriteIndented = true, IgnoreNullValues = ignoreNullValues, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
        }

        // 取得 Enum 列舉 Attribute Description 設定值
        public static string GetDescriptionText<T>(this T source) where T : Enum
        {
            var fi = source.GetType().GetField(source.ToString());
            if (fi == null) return source.ToString();
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : source.ToString();
        }
    }
}