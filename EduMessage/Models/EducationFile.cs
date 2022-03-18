namespace EduMessage.ViewModels
{
    public class EducationFile
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object ImagePath { get; set; }

        public byte[] Data { get; set; }
        public int Id { get; internal set; }
        public int AttachmentId { get; internal set; }
    }
}
