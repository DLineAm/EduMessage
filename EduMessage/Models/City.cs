using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class City
    {
        public City()
        {
            //Schools = new HashSet<School>();
            //Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<School> Schools { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
