using System.Collections.ObjectModel;

public class MainCourseTree
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int SpecialityId { get; set; }
    public ObservableCollection<CourseTree> CourseTrees { get; set; } = new();
}