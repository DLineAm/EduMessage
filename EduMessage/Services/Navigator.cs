using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Pages;

namespace EduMessage.Services
{
    public class Navigator
    {
        private readonly Frame _contentFrame = ((MainPage)((Frame)Window.Current.Content).Content).ContentFrame;
        public void Navigate(Type pageType, object parameter, NavigationTransitionInfo navigationTransitionInfo, FrameType type = FrameType.ContentFrame)
        {
            if (type == FrameType.ContentFrame)
            {
                _contentFrame.Navigate(pageType, parameter,
                    navigationTransitionInfo);
            }

            if (type == FrameType.LoginFrame)
            {
                ((((Frame)Window.Current.Content).Content as MainPage).ContentFrame.Content as LoginPage).ContentFrame
                    ?.Navigate(pageType, parameter, navigationTransitionInfo);
            }
        }

        public void Navigate(Type pageType, FrameType type = FrameType.ContentFrame)
        {
            if (type == FrameType.ContentFrame)
            {
                _contentFrame.Navigate(pageType);
            }
        }

        public void GoBack(FrameType type = FrameType.ContentFrame)
        {
            if (type == FrameType.ContentFrame)
            {
                if (_contentFrame.CanGoBack)
                {
                    _contentFrame.GoBack();
                } 
            }
            else if (type == FrameType.LoginFrame)
            {
                var frame = ((((Frame)Window.Current.Content).Content as MainPage).ContentFrame.Content as LoginPage).ContentFrame;

                if (frame != null && frame.CanGoBack)
                {
                    frame.GoBack();
                }
            }
        }

        public void GoBack( NavigationTransitionInfo info, FrameType type = FrameType.ContentFrame)
        {
            if (type == FrameType.ContentFrame)
            {
                if (_contentFrame.CanGoBack)
                {
                    _contentFrame.GoBack();
                } 
            }
            else if (type == FrameType.LoginFrame)
            {
                var frame = ((((Frame)Window.Current.Content).Content as MainPage).ContentFrame.Content as LoginPage).ContentFrame;

                if (frame != null && frame.CanGoBack)
                {
                    frame.GoBack(info);
                }
            }
        }
    }
}