using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class EducationType
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<School> Schools { get; set; }
    }
}
