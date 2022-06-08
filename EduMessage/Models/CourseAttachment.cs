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
        public byte? Mark { get; set; }
        public int? IdUser { get; set; }
        public DateTime? SendTime { get; set; }
        public string Comment { get; set; }

        public virtual Attachment IdAttachmanentNavigation { get; set; }
        public virtual Course IdCourseNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
    }
}
