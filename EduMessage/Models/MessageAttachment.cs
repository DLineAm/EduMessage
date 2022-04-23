using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class MessageAttachment
    {
        public int Id { get; set; }
        public int? IdMessage { get; set; }
        public int? IdAttachment { get; set; }

        public virtual Attachment IdAttachmentNavigation { get; set; }
        public virtual Message IdMessageNavigation { get; set; }
    }
}
