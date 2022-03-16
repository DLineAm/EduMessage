
using SignalIRServerTest;

using System.Collections.Generic;

using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    public class CourseFiles
    {
        public Course Course { get; set; }
        public List<EducationFile> Files { get; set; }

        public Visibility FilesInfoVisibility => Files == null
        || Files.Count == 0
        || Files.Count == 1 && Files[0] == null
        ? Visibility.Collapsed
        : Visibility.Visible;
    }
}
