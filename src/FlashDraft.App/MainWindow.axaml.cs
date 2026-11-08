using Avalonia.Controls;

namespace FlashDraft.App;

/// <summary>Code-built desktop host. Android uses MainView directly.</summary>
public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "FlashDraft";
        Width = 900;
        Height = 640;
        MinWidth = 720;
        MinHeight = 480;
        Content = new MainView();
    }
}
