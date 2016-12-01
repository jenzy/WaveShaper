using System.Windows.Input;

namespace WaveShaper
{
    public class CustomCommands
    {
        public static readonly RoutedUICommand Preset = new RoutedUICommand
        (
            "Preset",
            "Preset",
            typeof(CustomCommands)
        );
    }
}
