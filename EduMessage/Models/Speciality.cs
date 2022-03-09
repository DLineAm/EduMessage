using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Speciality
    {
        public Speciality()
        {
            //Groups = new HashSet<Group>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
    }
}
