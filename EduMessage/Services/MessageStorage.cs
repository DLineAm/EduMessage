using System.Collections.Generic;
using System.Linq;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{

    public class MessageStorage : IStorage<MessageAttachment>
    {
        private HashSet<MessageAttachment> _storage = new();

        public MessageAttachment GetById(int id)
        {
            var messageAttachment = _storage.FirstOrDefault(m => m.Id == id);
            return messageAttachment;
        }

        public IEnumerable<MessageAttachment> GetListById(int id)
        {
            var messageAttachment = _storage.Where(m => m.IdMessage == id);

            return messageAttachment;
        }

        public void Add(MessageAttachment entity)
        {
            _storage.Add(entity);
        }

        public void AddRange(IEnumerable<MessageAttachment> entity)
        {
            foreach (var messageAttachment in entity)
            {
                _storage.Add(messageAttachment);
            }
        }
    }
}