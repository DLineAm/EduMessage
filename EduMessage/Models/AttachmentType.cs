using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;

namespace SignalIRServerTest
{
    public partial class AttachmentType
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
