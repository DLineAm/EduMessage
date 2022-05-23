﻿using System;
using System.Collections.Generic;
using SignalIRServerTest.Models;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Device
    {
        //public int Id { get; set; }
        public int IdUser { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public byte[] SerialNumber { get; set; }

        public virtual User IdUserNavigation { get; set; }
    }
}
