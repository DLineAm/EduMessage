using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public class Chat : IChat
    {
        [ThreadStatic]
        private static HubConnection _connection;

        public void Initialize(string address, string token)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(address, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                } )
                .WithAutomaticReconnect()
                //.AddMessagePackProtocol()
                .Build();
        }

        public void SetOnMethod<TMessage, TUser>(string methodName, Action<TMessage, TUser> handler)
        {
            _connection.On(methodName, handler);
        }

        public void SetOnMethod<TMessage>(string methodName, Action<TMessage> handler)
        {
            _connection.On(methodName, handler);
        }

        public async Task OpenConnection()
        {
            await _connection.StartAsync();
        }

        public async Task CloseConnection()
        {
            await _connection.StopAsync();
        }

        public async Task SendMessage<TMessage>(string methodName, TMessage message)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }
            await _connection.SendAsync(methodName, message);
        }

        public async Task SendMessage<TMessage>(string methodName, TMessage message, int recipientId)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }
            await _connection.SendAsync(methodName, message, recipientId);
        }

        public async Task StreamAsync<TMessage>(string methodName, TMessage message, int recipientId)
        {
            var stream = _connection.StreamAsync<TMessage>(methodName, message, recipientId);
        }

        public async Task StreamAsChannelAsync(string methodName, string message, int recipientId)
        {

            //var list = ReadString(message);

            //await _connection.SendAsync(methodName, list);
            //var channel = Channel.CreateUnbounded<char>();
            //var writer = channel.Writer;
            //try
            //{
                
            //    foreach (var ch in message)
            //    {
            //        var result = writer.WriteAsync(ch);
            //    }

            //    writer.Complete();

            //    var reader = channel.Reader;

                
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //    writer.TryComplete(e);
            //}
            //var channel = await _connection.StreamAsChannelAsync<TMessage>(methodName, message, recipientId);
            //while (channel.TryRead(out TMessage data))
            //{
            //    Debug.WriteLine("Received = " + data);
            //}
        }

        //private async IAsyncEnumerable<char> ReadString(string value)
        //{
        //    foreach (var ch in value)
        //    {
        //        yield return ch;
        //    }
        //}
    }
}