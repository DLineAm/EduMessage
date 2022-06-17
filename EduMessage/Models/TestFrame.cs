using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using SignalIRServerTest;

#nullable disable

namespace WebApplication1
{
    public partial class TestFrame
    {
        public TestFrame()
        {
            Courses = new HashSet<Course>();
            TestPages = new HashSet<TestPage>();
        }

        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<TestPage> TestPages { get; set; }
    }
}
