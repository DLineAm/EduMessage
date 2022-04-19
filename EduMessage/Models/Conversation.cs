using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class Conversation
    {
        public Conversation()
        {
            Messages = new HashSet<Message>();
            UserConversations = new HashSet<UserConversation>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? IdAdminUser { get; set; }
        public byte[] Image { get; set; }

        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserConversation> UserConversations { get; set; }
    }
}
