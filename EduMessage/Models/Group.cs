using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Group
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? IdFaculty { get; set; }
        public int IdSpeciality { get; set; }

        public virtual Faculty IdFacultyNavigation { get; set; }
        public virtual Speciality IdSpecialityNavigation { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
