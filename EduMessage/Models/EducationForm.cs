using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class EducationForm
    {
        public EducationForm()
        {
            //Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
