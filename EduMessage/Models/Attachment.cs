using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Attachment
    {
        public Attachment()
        {
            //Messages = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int IdType { get; set; }
        public byte[] Data { get; set; }

        public virtual AttachmentType IdTypeNavigation { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
