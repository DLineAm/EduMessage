using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;

#nullable disable

namespace SignalIRServerTest
{
    public partial class EducationForm
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
