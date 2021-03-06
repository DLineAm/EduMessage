using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class Attachment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int IdType { get; set; }
        public byte[] Data { get; set; }

        public virtual AttachmentType IdTypeNavigation { get; set; }
    }
}
