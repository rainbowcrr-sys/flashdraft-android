using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;

using Button = Avalonia.Controls.Button;
using CheckBox = Avalonia.Controls.CheckBox;
using Grid = Avalonia.Controls.Grid;
using ListBox = Avalonia.Controls.ListBox;
using ProgressBar = Avalonia.Controls.ProgressBar;
using RowDefinition = Avalonia.Controls.RowDefinition;
using ScrollViewer = Avalonia.Controls.ScrollViewer;
using StackPanel = Avalonia.Controls.StackPanel;
using TabControl = Avalonia.Controls.TabControl;
using TabItem = Avalonia.Controls.TabItem;
using TextBlock = Avalonia.Controls.TextBlock;
using TextBox = Avalonia.Controls.TextBox;
using UserControl = Avalonia.Controls.UserControl;
using Orientation = Avalonia.Layout.Orientation;
using FontWeight = Avalonia.Media.FontWeight;

namespace FlashDraft.App;

/// <summary>
/// The shared application view, constructed in code. Keeping the control tree
/// in C# makes the Android activity independent of XAML resource embedding.
/// </summary>
public class MainView : UserControl
{
    public MainView()
    {
        var root = new Grid { Margin = new Thickness(18) };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Parse("*")));

        var heading = new TextBlock
        {
            Text = "FlashDraft",
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 14),
        };
        Grid.SetRow(heading, 0);
        root.Children.Add(heading);

        var tabs = new TabControl();
        Grid.SetRow(tabs, 1);
        tabs.Items.Add(new TabItem { Header = "Transfer", Content = BuildTransferTab() });
        tabs.Items.Add(new TabItem { Header = "About", Content = BuildAboutTab() });
        root.Children.Add(tabs);

        Content = root;
    }

    private static Control BuildTransferTab()
    {
        var stack = new StackPanel { Spacing = 8 };

        stack.Children.Add(new TextBlock { Text = "My offer code" });
        var localCode = new TextBox { IsReadOnly = true, Watermark = "Generate a code first" };
        localCode.Bind(TextBox.TextProperty, TwoWay(nameof(MainViewModel.LocalCode)));
        stack.Children.Add(localCode);

        stack.Children.Add(new TextBlock { Text = "Peer offer code" });
        var remoteCode = new TextBox { Watermark = "flashdraft://..." };
        remoteCode.Bind(TextBox.TextProperty, TwoWay(nameof(MainViewModel.RemoteCode)));
        stack.Children.Add(remoteCode);

        var generate = new Button { Content = "Generate my code" };
        generate.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.GenerateCodeCommand)));
        var bind = new Button { Content = "Bind peer" };
        bind.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.BindPeerCommand)));
        var wan = new CheckBox { Content = "WAN mode", VerticalAlignment = VerticalAlignment.Center };
        wan.Bind(ToggleButton.IsCheckedProperty, TwoWay(nameof(MainViewModel.IsWanMode)));
        stack.Children.Add(Row(generate, bind, wan));

        stack.Children.Add(new TextBlock { Text = "Source file" });
        var source = new TextBox { Watermark = "Full path of the file to send" };
        source.Bind(TextBox.TextProperty, TwoWay(nameof(MainViewModel.SourcePath)));
        stack.Children.Add(source);

        stack.Children.Add(new TextBlock { Text = "Destination directory" });
        var destination = new TextBox { Watermark = "Leave empty for the default download folder" };
        destination.Bind(TextBox.TextProperty, TwoWay(nameof(MainViewModel.DestinationPath)));
        stack.Children.Add(destination);

        var wait = new Button { Content = "Wait for transfer" };
        wait.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.WaitForTransferCommand)));
        var send = new Button { Content = "Send file" };
        send.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.SendFileCommand)));
        var loopback = new Button { Content = "Loopback self test" };
        loopback.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.RunLoopbackTestCommand)));
        var cancel = new Button { Content = "Cancel" };
        cancel.Bind(Button.CommandProperty, OneWay(nameof(MainViewModel.CancelCommand)));
        stack.Children.Add(Row(wait, send, loopback, cancel));

        var status = new TextBlock();
        status.Bind(TextBlock.TextProperty, OneWay(nameof(MainViewModel.StatusText)));
        var speed = new TextBlock();
        speed.Bind(TextBlock.TextProperty, OneWay(nameof(MainViewModel.SpeedText)));
        stack.Children.Add(Row(status, speed));

        var progress = new ProgressBar { Minimum = 0, Maximum = 100, Height = 14 };
        progress.Bind(RangeBase.ValueProperty, OneWay(nameof(MainViewModel.ProgressPercent)));
        stack.Children.Add(progress);

        var devices = new ListBox { Height = 90 };
        devices.Bind(ItemsControl.ItemsSourceProperty, OneWay(nameof(MainViewModel.Devices)));
        stack.Children.Add(devices);

        return new ScrollViewer { Margin = new Thickness(8), Content = stack };
    }

    private static Control BuildAboutTab()
    {
        var text = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
        };
        text.Bind(TextBlock.TextProperty, OneWay(nameof(MainViewModel.AboutText)));
        return new ScrollViewer { Margin = new Thickness(8), Content = text };
    }

    private static StackPanel Row(params Control[] children)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        foreach (Control child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    private static Binding OneWay(string path) => new(path);

    private static Binding TwoWay(string path) =>
        new(path) { Mode = BindingMode.TwoWay };
}
