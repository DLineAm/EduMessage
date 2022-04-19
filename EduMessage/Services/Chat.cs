using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public interface IChat
    {
        void Initialize(string address, string token);
        void SetOnMethod<TMessage, TUser>(string methodName, Action<TMessage, TUser> handler);
        Task OpenConnection();
        Task CloseConnection();
        Task SendMessage<TMessage>(string methodName, User recipientUser, TMessage message);
        Task SendMessage<TMessage>(string methodName, int recipientId, TMessage message);

    }
    public class Chat : IChat
    {
        private HubConnection _connection;

        public void Initialize(string address, string token)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(address, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                } )
                .WithAutomaticReconnect()
                .Build();
        }

        public void SetOnMethod<TMessage, TUser>(string methodName, Action<TMessage, TUser> handler)
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

        public async Task SendMessage<TMessage>(string methodName, User recipientUser, TMessage message)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }
            await _connection.SendAsync(methodName, message, recipientUser.Id);
        }

        public async Task SendMessage<TMessage>(string methodName, int recipientId, TMessage message)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }
            await _connection.SendAsync(methodName, message, recipientId);
        }
    }
}