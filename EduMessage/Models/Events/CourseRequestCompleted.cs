using System.Collections.Generic;
using SignalIRServerTest;

namespace EduMessage.ViewModels
{
    public record CourseRequestCompleted(IEnumerable<CourseAttachment> CourseAttachments);
}