using System;
using System.Collections.Generic;


namespace SignalIRServerTest
{
    public partial class Attachment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int IdType { get; set; }
        public byte[] Data { get; set; }

        public virtual AttachmentType IdTypeNavigation { get; set; }
        public virtual ICollection<CourseAttachment> CourseAttachments { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
