using System.Collections.Generic;

namespace SignalIRServerTest.Models
{
    public class TaskStatus
    {
        public TaskStatus()
        {
            this.CourseAttachment = new HashSet<CourseAttachment>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        
        public virtual ICollection<CourseAttachment> CourseAttachment { get; set; }
    }
}