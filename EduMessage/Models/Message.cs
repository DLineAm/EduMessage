using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Message
    {
        public int Id { get; set; }
        public string MessageContent { get; set; }
        public DateTime SendDate { get; set; }
        public int? IdAttachments { get; set; }
        public int IdUser { get; set; }
        public int? IdRecipient { get; set; }

        public virtual Attachment IdAttachmentsNavigation { get; set; }
        public virtual User IdRecipientNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
