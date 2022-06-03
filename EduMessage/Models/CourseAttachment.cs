using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;
using Attachment = SignalIRServerTest.Models.Attachment;

#nullable disable

namespace SignalIRServerTest
{
    public partial class CourseAttachment
    {
        public int Id { get; set; }
        public int? IdCourse { get; set; }
        public int? IdAttachmanent { get; set; }
        public int? IdStatus { get; set; }

        public virtual Attachment IdAttachmanentNavigation { get; set; }
        public virtual Course IdCourseNavigation { get; set; }
        public virtual TaskStatus IdTaskStatusNavigation { get; set; }
    }
}
