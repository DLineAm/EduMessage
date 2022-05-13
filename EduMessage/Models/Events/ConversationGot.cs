using System.Collections.Generic;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    public record ConversationGot(List<UserConversation> Conversations);
}