using System.Collections.Generic;
using SignalIRServerTest.Models;

namespace EduMessage
{
    public record ReplySentEvent(List<MessageAttachment> Message, int RecipientId);
}