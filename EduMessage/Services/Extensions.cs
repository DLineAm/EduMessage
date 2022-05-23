using Newtonsoft.Json;

using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Attachment = SignalIRServerTest.Models.Attachment;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EduMessage.Services
{
    public static class Extensions
    {

        public static async Task WriteAttachmentImagePath(this IEnumerable<Attachment> attachments)
        {
            var tasks = attachments.Where(a => a != null)
                .Select(attachment => attachment.SplitAndGetImage());

            await Task.WhenAll(tasks);
        }

        public static async Task<string> SendRequestAsync<T>(this string address
            , T value
            , HttpRequestType requestType
            , string token = ""
            , bool passCertificateValidation = true
            , bool isLoopHandleIgnore = false)
        {
            using HttpClient client = CreateClient(token, passCertificateValidation);

            StringContent httpContent = null;

            if (value != null)
            {
                var settings = new JsonSerializerSettings();
                if (isLoopHandleIgnore)
                {
                    settings.MaxDepth = 1;
                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }

                using var stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream,value);
                stream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (requestType != HttpRequestType.Get && requestType != HttpRequestType.Delete && httpContent == null)
            {
                throw new NullReferenceException(nameof(value));
            }

            var response = await Send(address, requestType, client, httpContent);

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

        public static async Task<BitmapImage> CreateBitmap(this byte[] data, int decodedHeightWidth = -1)
        {
            var source = new BitmapImage();
            if (decodedHeightWidth != -1)
            {
                source.DecodePixelHeight = decodedHeightWidth;
            }
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(data.AsBuffer());
            stream.Seek(0);
            await source.SetSourceAsync(stream);

            return source;
        }

        public static void Slide(this UIElement target, Orientation orientation, double? from, double to, int duration = 400, int startTime = 0, EasingFunctionBase easing = null)
        {
            if (easing == null)
            {
                easing = new ExponentialEase();
            }

            var transform = target.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                transform = new CompositeTransform();
                target.RenderTransform = transform;
            }
            target.RenderTransformOrigin = new Point(0.5, 0.5);

            var db = new DoubleAnimation
            {
                To = to,
                From = from,
                EasingFunction = easing,
                Duration = TimeSpan.FromMilliseconds(duration)
            };
            Storyboard.SetTarget(db, target);
            var axis = orientation == Orientation.Horizontal ? "X" : "Y";
            Storyboard.SetTargetProperty(db, $"(UIElement.RenderTransform).(CompositeTransform.Translate{axis})");

            var sb = new Storyboard
            {
                BeginTime = TimeSpan.FromMilliseconds(startTime)
            };

            sb.Children.Add(db);
            sb.Begin();
        }
    }
}