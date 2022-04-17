using MvvmGen;
using SignalIRServerTest;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class ChatPageViewModel
    {
        [Property] private User _user;
        public void Initialize(User user)
        {
            _user = user;
        }
    }
}