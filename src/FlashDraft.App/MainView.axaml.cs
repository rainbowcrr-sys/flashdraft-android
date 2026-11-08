using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace FlashDraft.App;

/// <summary>Minimal visible proof that the Android activity stays alive.</summary>
public class MainView : UserControl
{
    public MainView()
    {
        Background = new SolidColorBrush(Color.Parse("#0B47C9"));
        Content = new TextBlock
        {
            Text = "FlashDraft is running.",
            Foreground = Brushes.White,
            FontSize = 20,
            Margin = new Thickness(24),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
}
