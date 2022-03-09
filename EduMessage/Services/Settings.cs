using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

namespace EduMessage.Services
{
    public class Settings
    {
        public static ApplicationDataContainer Get()
        {
            return Windows.Storage.ApplicationData.Current.LocalSettings;
        }
    }

}
