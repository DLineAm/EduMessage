using System;
using System.Collections.Generic;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Device
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public int? SerialNumber { get; set; }

        public virtual User IdUserNavigation { get; set; }
    }
}
