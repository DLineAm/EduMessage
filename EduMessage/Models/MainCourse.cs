using System.Collections.Generic;
using System.Text.Json.Serialization;
using SignalIRServerTest;

namespace EduMessage.Models
{
    public class MainCourse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MainCourse()
        {
            this.Course = new HashSet<Course>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public int? IdSpeciality { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Course> Course { get; set; }
        public virtual Speciality IdSpecialityNavigation { get; set; }
    }
}