using System.Windows;
using System.Windows.Controls;

namespace LteRefSensTester
{
    internal class SimpleInputDialog : Window
    {
        private readonly TextBox _tb;
        public string Result { get; private set; } = "";

        public SimpleInputDialog(string prompt, string title, string defaultValue = "")
        {
            Title = title;
            Width = 360;
            Height = 140;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 6) });
            _tb = new TextBox { Text = defaultValue, Height = 26 };
            sp.Children.Add(_tb);

            var bp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var ok = new Button
            {
                Content = "OK",
                Width = 72,
                Height = 26,
                Margin = new Thickness(0, 0, 6, 0),
                IsDefault = true
            };
            ok.Click += (_, _) => { Result = _tb.Text.Trim(); DialogResult = true; };

            var cancel = new Button { Content = "Cancel", Width = 72, Height = 26, IsCancel = true };
            bp.Children.Add(ok);
            bp.Children.Add(cancel);
            sp.Children.Add(bp);

            Content = sp;
        }
    }
}
