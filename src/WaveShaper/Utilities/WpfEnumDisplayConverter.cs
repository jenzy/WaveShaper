using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace WaveShaper.Utilities
{
    public class WpfEnumDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Enum)
            {
                return value == null ? DependencyProperty.UnsetValue : GetDescription((Enum) value);
            }
            else if (value is object[])
            {
                
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.ToObject(targetType, value);
        }
        private static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }
    }
}
