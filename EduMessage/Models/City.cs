using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SignalIRServerTest.Models;

#nullable disable

namespace SignalIRServerTest
{
    public partial class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<School> Schools { get; set; }
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; }
    }
}
