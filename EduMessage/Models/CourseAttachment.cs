using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class CourseAttachment
    {
        public int Id { get; set; }
        public int? IdCourse { get; set; }
        public int? IdAttachmanent { get; set; }

        public virtual Attachment IdAttachmanentNavigation { get; set; }
        public virtual Course IdCourseNavigation { get; set; }
    }
}
