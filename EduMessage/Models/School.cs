using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;

#nullable disable

namespace SignalIRServerTest
{
    public partial class School
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdCity { get; set; }
        public string Address { get; set; }
        public int IdEducationType { get; set; }

        public virtual City IdCityNavigation { get; set; }
        public virtual EducationType IdEducationTypeNavigation { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
