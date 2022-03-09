using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; }
    }
}
