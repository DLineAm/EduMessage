using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Data;
using EduMessage.ViewModels;
using SignalIRServerTest.Models;

namespace EduMessage.Utils
{
    public class MessageToAttachmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var convertedValue = value as ObservableCollection<MessageList>;
            HashSet<Attachment> result = new HashSet<Attachment>();
            foreach (var messages in convertedValue)
            {
                var attachments = messages.Select(m => m.Attachments);
                foreach (var attachment in attachments)
                {
                    var imageAttachments = attachment.Where(a => a is {IdType: 3});
                    foreach (var imageAttachment in imageAttachments)
                    {
                        result.Add(imageAttachment);
                    }
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}