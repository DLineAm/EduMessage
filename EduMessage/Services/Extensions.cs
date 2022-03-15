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
        //public static async Task<string> GetStringAsync(this string address, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }

        //    var response = client.GetAsync(new Uri(address)).Result;
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();
        //    return result;
        //}

        //public static async Task<string> GetStringAsync(this string address, string token, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
            
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }

        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

        //    var response = client.GetAsync(new Uri(address)).Result;
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();
        //    return result;
        //}

        //public static async Task<string> DeleteFromRequestAsync(this string address, string token, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
            
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }

        //    client.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

        //    var response = client.DeleteAsync(new Uri(address)).Result;
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();
        //    return result;
        //}

        //public static async Task<string> PostBoolAsync<T>(this string address, T value, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }
        //    var json = JsonConvert.SerializeObject(value);
        //    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync(address, httpContent);
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();

        //    return result;
        //}

        //public static async Task<string> PutBoolAsync<T>(this string address, T value, string token, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }

        //    client.DefaultRequestHeaders.Authorization =
        //       new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
        //    var json = JsonConvert.SerializeObject(value);
        //    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = await client.PutAsync(address, httpContent);
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();

        //    return result;
        //}

        //public static async Task<string> PostBoolAsync<T>(this string address, T value,string token, bool passCertificateValidation = true)
        //{
        //    HttpClient client;
        //    if (passCertificateValidation)
        //    {
        //        var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        //        client = new HttpClient(clientHandler);
        //    }
        //    else
        //    {
        //        client = new HttpClient();
        //    }
        //    client.DefaultRequestHeaders.Authorization =
        //       new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
        //    var json = JsonConvert.SerializeObject(value);
        //    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync(address, httpContent);
        //    response.EnsureSuccessStatusCode();
        //    var result = await response.Content.ReadAsStringAsync();

        //    return result;
        //}

        public static async Task<string> SendRequestAsync<T>(this string address, T value, HttpRequestType requestType, string token = "", bool passCertificateValidation = true)
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
                    throw new Exception("Unknown HttpRequestType value");
            }


            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            return result;
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