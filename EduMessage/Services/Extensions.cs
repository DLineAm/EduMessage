using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EduMessage.Services
{
    public static class Extensions
    {
        public static async Task<string> GetStringAsync(this string address, bool passCertificateValidation = true)
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

            var response = client.GetAsync(new Uri(address)).Result;
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> GetStringAsync(this string address, string token, bool passCertificateValidation = true)
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

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

            var response = client.GetAsync(new Uri(address)).Result;
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> DeleteFromRequestAsync(this string address, string token, bool passCertificateValidation = true)
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

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

            var response = client.DeleteAsync(new Uri(address)).Result;
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static async Task<string> PostBoolAsync<T>(this string address, T value, bool passCertificateValidation = true)
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
            var json = JsonConvert.SerializeObject(value);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(address, httpContent);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        public static async Task<string> PostBoolAsync<T>(this string address, T value,string token, bool passCertificateValidation = true)
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
            client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            var json = JsonConvert.SerializeObject(value);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(address, httpContent);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();

            return result;
        }

        public static T DeserializeJson<T>(this string value)
        {
            var result = JsonConvert.DeserializeObject<T>(value);
            return result;
        }
    }
}