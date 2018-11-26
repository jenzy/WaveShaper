using System.Windows;
using System.Windows.Media;

namespace WaveShaper.Wpf
{
    public static class WpfUtil
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);
                if (parentObject == null)
                    return null;

                if (parentObject is T parent)
                    return parent;

                child = parentObject;
            }
        }
    }
}
