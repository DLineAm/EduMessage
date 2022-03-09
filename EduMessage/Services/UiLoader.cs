using Microsoft.UI.Xaml.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;

namespace EduMessage.Services
{
    public class UiLoader
    {
        public AnimatedVisualPlayer Loader { get; private set; }

        public void Initialize(AnimatedVisualPlayer loader)
        {
            Loader = loader;
        }

        public void SetVisibility(Visibility visibility)
        {
            if (Loader == null)
                return;
            
            Loader.Visibility = visibility;
        }
    }
}
