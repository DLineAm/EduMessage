using System.Collections.ObjectModel;

public class SpecialityTree
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public ObservableCollection<MainCourseTree> MainCourseTrees { get; set; } = new();
}