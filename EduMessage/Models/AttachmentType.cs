using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SignalIRServerTest.Models;

namespace SignalIRServerTest
{
    public partial class AttachmentType
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
