using System.Windows.Controls;
using System.Windows.Input;

namespace WaveShaper
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Preset = new RoutedUICommand
        (
            "Preset",
            "Preset",
            typeof(CustomCommands)
        );

        public static class Presets
        {
            public static readonly RoutedUICommand Identity = CreateRoutedCommand("Identity", "Preset.Identity");
            public static readonly RoutedUICommand SoftClip1 = CreateRoutedCommand("Soft clipping 1", "Preset.SoftClipping1");
            public static readonly RoutedUICommand SoftClip2 = CreateRoutedCommand("Soft clipping 2", "Preset.SoftClipping2");
        }


        public static MenuItem ToMenuItem(this RoutedUICommand command, object commandParameter = null)
            => new MenuItem {Header = command.Text, Command = command, CommandParameter = commandParameter};

        private static RoutedUICommand CreateRoutedCommand(string text, string name) => new RoutedUICommand(text, name, typeof(CustomCommands));
    }
}
