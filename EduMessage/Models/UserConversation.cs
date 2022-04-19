using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class UserConversation
    {
        public int Id { get; set; }
        public int? IdUser { get; set; }
        public int? IdConversation { get; set; }

        public virtual Conversation IdConversationNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
