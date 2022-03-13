using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Course
    {
        public Course()
        {
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? IdSpeciality { get; set; }

        public virtual Speciality IdSpecialityNavigation { get; set; }
        public virtual ICollection<CourseAttachment> CourseAttachments { get; set; }
    }
}
