using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class AttachmentType
    {
        public AttachmentType()
        {
            //Attachments = new HashSet<Attachment>();
        }

        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
