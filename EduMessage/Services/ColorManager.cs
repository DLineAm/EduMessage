using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace EduMessage.Services
{
    public class ColorManager
    {
        public Color GetAccentColor()
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var rgba = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
            return rgba;
        }
    }
}
