using System.Windows;
using System.Windows.Media;

namespace WaveShaper.Utilities
{
    internal static class WpfUtil
    {
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);
                if (parentObject == null)
                    return null;

                T parent = parentObject as T;
                if (parent != null)
                    return parent;

                child = parentObject;
            }
        }
    }
}
