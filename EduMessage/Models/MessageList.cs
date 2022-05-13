using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EduMessage.ViewModels
{
    public class MessageList : ObservableCollection<FormattedMessage>
    {
        public MessageList(IEnumerable<FormattedMessage> items) : base(items)
        {

        }

        public object Key { get; set; }
    }
}