using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class User
    {
        public User()
        {
            Devices = new HashSet<Device>();
            MessageIdRecipientNavigations = new HashSet<Message>();
            MessageIdUserNavigations = new HashSet<Message>();
            UserConversations = new HashSet<UserConversation>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public long? Phone { get; set; }
        public string Address { get; set; }
        public int? IdCity { get; set; }
        public int IdSchool { get; set; }
        public int? IdEducationForm { get; set; }
        public int? IdGroup { get; set; }
        public int IdRole { get; set; }
        public byte[] Image { get; set; }

        public virtual City IdCityNavigation { get; set; }
        public virtual EducationForm IdEducationFormNavigation { get; set; }
        public virtual Group IdGroupNavigation { get; set; }
        public virtual Role IdRoleNavigation { get; set; }
        public virtual School IdSchoolNavigation { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> MessageIdRecipientNavigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> MessageIdUserNavigations { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserConversation> UserConversations { get; set; }
    }
}
