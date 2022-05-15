using System;
using System.Threading.Tasks;

namespace EduMessage.Services
{
    public interface IChat
    {
        void Initialize(string address, string token);
        void SetOnMethod<TMessage, TUser>(string methodName, Action<TMessage, TUser> handler);
        void SetOnMethod<TMessage>(string methodName, Action<TMessage> handler);
        Task OpenConnection();
        Task CloseConnection();
        Task SendMessage<TMessage>(string methodName, TMessage message);
        Task SendMessage<TMessage>(string methodName, int recipientId, TMessage message);

    }
}