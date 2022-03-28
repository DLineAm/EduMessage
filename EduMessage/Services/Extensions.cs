using System;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http.Connections;

using Newtonsoft.Json;

using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace EduMessage.Services
{
    public static class Extensions
    {
        public static async Task<string> SendRequestAsync<T>(this string address
            , T value
            , HttpRequestType requestType
            , string token = ""
            , bool passCertificateValidation = true)
        {
            using HttpClient client = CreateClient(token, passCertificateValidation);

            HttpResponseMessage response = null;
            StringContent httpContent = null;

            if (value != null)
            {
                var json = JsonConvert.SerializeObject(value);
                httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (requestType != HttpRequestType.Get && requestType != HttpRequestType.Delete && httpContent == null)
            {
                throw new NullReferenceException(nameof(value));
            }

            response = await Send(address, requestType, client, httpContent);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        private static async Task<HttpResponseMessage> Send(string address
            , HttpRequestType requestType
            , HttpClient client
            , StringContent httpContent)
        {
            HttpResponseMessage response;
            switch (requestType)
            {
                case HttpRequestType.Get:
                    response = client.GetAsync(address).Result;
                    break;
                case HttpRequestType.Post:
                    response = await client.PostAsync(address, httpContent);
                    break;
                case HttpRequestType.Put:
                    response = await client.PutAsync(address, httpContent);
                    break;
                case HttpRequestType.Delete:
                    response = await client.DeleteAsync(address);
                    break;
                default:
                    throw new ArgumentException("Unknown HttpRequestType value");
            }

            return response;
        }

        private static HttpClient CreateClient(string token, bool passCertificateValidation)
        {
            HttpClient client;
            if (passCertificateValidation)
            {
                var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
                client = new HttpClient(clientHandler);
            }
            else
            {
                client = new HttpClient();
            }

            if (token != "")
            {
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            }

            return client;
        }

        public static T DeserializeJson<T>(this string value)
        {
            var result = JsonConvert.DeserializeObject<T>(value);
            return result;
        }

        public static async Task<BitmapImage> CreateBitmap(this byte[] data)
        {
            var source = new BitmapImage();
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(data.AsBuffer());
            stream.Seek(0);
            await source.SetSourceAsync(stream);

            return source;
        }
    }

    public enum HttpRequestType
    {
        Get, Post, Put, Delete
    }
}