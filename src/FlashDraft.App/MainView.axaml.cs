using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Button = Avalonia.Controls.Button;
using Border = Avalonia.Controls.Border;
using Canvas = Avalonia.Controls.Canvas;
using CheckBox = Avalonia.Controls.CheckBox;
using Grid = Avalonia.Controls.Grid;
using ListBox = Avalonia.Controls.ListBox;
using Path = Avalonia.Controls.Shapes.Path;
using ProgressBar = Avalonia.Controls.ProgressBar;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;
using ScrollViewer = Avalonia.Controls.ScrollViewer;
using StackPanel = Avalonia.Controls.StackPanel;
using TextBlock = Avalonia.Controls.TextBlock;
using TextBox = Avalonia.Controls.TextBox;
using UserControl = Avalonia.Controls.UserControl;
using Viewbox = Avalonia.Controls.Viewbox;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
using Orientation = Avalonia.Layout.Orientation;

namespace FlashDraft.App;

/// <summary>
/// Phone layout. Unlike the desktop window this uses a compact brand bar, a
/// single column of cards and a bottom tab strip, with larger touch targets.
/// The control tree is built in code and no ViewModel is constructed, so the
/// activity keeps the verified startup path on Android.
/// </summary>
public class MainView : UserControl
{
    private static readonly SolidColorBrush Blue =
        new(Color.Parse("#0B47C9"));
    private static readonly SolidColorBrush Dark =
        new(Color.Parse("#1B2430"));
    private static readonly SolidColorBrush Muted =
        new(Color.Parse("#667085"));
    private static readonly SolidColorBrush Line =
        new(Color.Parse("#E6EBF4"));

    private Grid? _host;

    public MainView()
    {
        var root = new Grid { Background = new SolidColorBrush(Color.Parse("#F2F5FB")) };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Parse("*")));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Compact brand bar for a narrow portrait viewport.
        var bar = new Border
        {
            Height = 64,
            Padding = new Thickness(16, 0),
            Background = new LinearGradientBrush
            {
                StartPoint = RelativePoint.TopLeft,
                EndPoint = RelativePoint.BottomRight,
                GradientStops =
                {
                    new GradientStop(Color.Parse("#0B47C9"), 0),
                    new GradientStop(Color.Parse("#3A82F0"), 1),
                },
            },
        };
        var barRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12,
                                      VerticalAlignment = VerticalAlignment.Center };
        barRow.Children.Add(BrandIcon(34));
        var titles = new StackPanel { Spacing = 0, VerticalAlignment = VerticalAlignment.Center };
        titles.Children.Add(new TextBlock { Text = "FlashDraft", FontSize = 18,
                                            FontWeight = FontWeight.SemiBold,
                                            Foreground = Brushes.White });
        titles.Children.Add(new TextBlock { Text = "LAN and Internet file transfer",
                                            FontSize = 10, Foreground = Brushes.White });
        barRow.Children.Add(titles);
        bar.Child = barRow;
        Grid.SetRow(bar, 0);
        root.Children.Add(bar);

        // Single column of cards; no left navigation on a phone.
        var column = new StackPanel { Spacing = 12, Margin = new Thickness(12, 12, 12, 8) };
        column.Children.Add(PairingCard());
        column.Children.Add(TransferCard());
        column.Children.Add(ProgressCard());
        var scroll = new ScrollViewer { Content = column };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);

        // Bottom tab strip replaces the desktop side rail.
        var tabs = new Border
        {
            Height = 56,
            Background = Brushes.White,
            BorderBrush = Line,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
        var tabGrid = new Grid();
        tabGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        tabGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        var transfer = new TextBlock { Text = "Transfer", FontSize = 14,
                                       FontWeight = FontWeight.SemiBold, Foreground = Blue,
                                       HorizontalAlignment = HorizontalAlignment.Center,
                                       VerticalAlignment = VerticalAlignment.Center };
        var about = new Button { Content = "About", FontSize = 14, Foreground = Muted,
                                 Background = Brushes.Transparent,
                                 HorizontalAlignment = HorizontalAlignment.Center,
                                 VerticalAlignment = VerticalAlignment.Center };
        about.Click += (_, _) => ShowAboutLicense();
        Grid.SetColumn(transfer, 0);
        Grid.SetColumn(about, 1);
        tabGrid.Children.Add(transfer);
        tabGrid.Children.Add(about);
        tabs.Child = tabGrid;
        Grid.SetRow(tabs, 2);
        root.Children.Add(tabs);

        var host = new Grid();
        host.Children.Add(root);
        _host = host;
        if (!LicenseGate.IsAccepted())
        {
            host.Children.Add(BuildLicenseGate(host));
        }

        Content = host;
    }

    /// <summary>
    /// Full-screen first-run license page. Without ticking the box the user
    /// cannot reach the main view; the app is left running so the user exits
    /// manually, exactly as requested.
    /// </summary>
    private Border BuildLicenseGate(Grid host)
    {
        var veil = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E6F2F5FB")),
        };

        var hint = new TextBlock
        {
            Text = "Decline by closing the app manually / 不接受请手动退出应用",
            FontSize = 10,
            Foreground = Muted,
            TextWrapping = TextWrapping.Wrap,
        };
        var check = new CheckBox
        {
            Content = "我已阅读并接受 / I have read and accept",
            Foreground = Dark,
            FontSize = 12,
        };
        var proceed = new Button
        {
            Content = "继续 / Continue",
            Background = Blue,
            Foreground = Brushes.White,
            Padding = new Thickness(16, 11),
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        proceed.Click += (_, _) =>
        {
            if (check.IsChecked != true)
            {
                hint.Text = "Please tick the box first / 请先勾选";
                return;
            }

            LicenseGate.MarkAccepted();
            host.Children.Remove(veil);
        };

        var stack = new StackPanel { Margin = new Thickness(18), Spacing = 10 };
        stack.Children.Add(new TextBlock { Text = "License Agreement / 许可协议",
                                           FontSize = 17, FontWeight = FontWeight.SemiBold,
                                           Foreground = Blue });
        stack.Children.Add(new Border
        {
            Height = 230,
            Background = Brushes.White,
            BorderBrush = Line,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10),
            Child = new ScrollViewer
            {
                Content = new TextBlock { Text = LicenseContent.Text,
                                          TextWrapping = TextWrapping.Wrap,
                                          FontSize = 10, Foreground = Dark },
            },
        });
        stack.Children.Add(check);
        stack.Children.Add(proceed);
        stack.Children.Add(hint);
        veil.Child = stack;
        return veil;
    }

    /// <summary>About tab re-opens the full license after acceptance.</summary>
    private void ShowAboutLicense()
    {
        Grid? host = _host;
        if (host is null) return;

        var veil = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E6F2F5FB")),
        };
        var close = new Button
        {
            Content = "关闭 / Close",
            Background = Blue,
            Foreground = Brushes.White,
            Padding = new Thickness(16, 11),
            FontSize = 13,
        };
        close.Click += (_, _) => host.Children.Remove(veil);

        var stack = new StackPanel { Margin = new Thickness(18), Spacing = 10 };
        stack.Children.Add(new TextBlock { Text = "License / 许可全文",
                                           FontSize = 17, FontWeight = FontWeight.SemiBold,
                                           Foreground = Blue });
        stack.Children.Add(new Border
        {
            Height = 380,
            Background = Brushes.White,
            BorderBrush = Line,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10),
            Child = new ScrollViewer
            {
                Content = new TextBlock { Text = LicenseContent.Text,
                                          TextWrapping = TextWrapping.Wrap,
                                          FontSize = 10, Foreground = Dark },
            },
        });
        stack.Children.Add(close);
        veil.Child = stack;
        host.Children.Add(veil);
    }

    /// <summary>The real FlashDraft icon, redrawn as vector geometry.</summary>
    private static Control BrandIcon(double size)
    {
        var canvas = new Canvas { Width = 108, Height = 108 };
        var tile = new Rectangle
        {
            Width = 108,
            Height = 108,
            RadiusX = 24,
            RadiusY = 24,
            Fill = new LinearGradientBrush
            {
                StartPoint = RelativePoint.TopLeft,
                EndPoint = RelativePoint.BottomRight,
                GradientStops =
                {
                    new GradientStop(Color.Parse("#0B47C9"), 0),
                    new GradientStop(Color.Parse("#3A82F0"), 1),
                },
            },
        };
        canvas.Children.Add(tile);
        canvas.Children.Add(Arrow("M33 42 H75"));
        canvas.Children.Add(Arrow("M68 35 L77 42 L68 49"));
        canvas.Children.Add(Arrow("M75 66 H33"));
        canvas.Children.Add(Arrow("M40 59 L31 66 L40 73"));
        canvas.Children.Add(new Path
        {
            Data = StreamGeometry.Parse("M57 47 L47 59 H54 L50 70 L64 55 H56 Z"),
            Fill = new SolidColorBrush(Color.Parse("#FFD84D")),
        });

        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(9),
            Background = Brushes.White,
            Child = new Viewbox { Margin = new Thickness(2), Child = canvas },
            VerticalAlignment = VerticalAlignment.Center,
        };
    }

    private static Path Arrow(string geometry) => new()
    {
        Data = StreamGeometry.Parse(geometry),
        Stroke = Brushes.White,
        StrokeThickness = 7,
        StrokeLineCap = PenLineCap.Round,
        StrokeJoin = PenLineJoin.Round,
    };

    private static Border PairingCard()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(SectionHeader("Pairing"));
        panel.Children.Add(Label("My offer code"));
        panel.Children.Add(Field("Generate a code first"));
        panel.Children.Add(Label("Peer offer code"));
        panel.Children.Add(Field("flashdraft://..."));
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        actions.Children.Add(BigButton("Generate my code", true));
        actions.Children.Add(BigButton("Bind peer", false));
        panel.Children.Add(actions);
        panel.Children.Add(new CheckBox { Content = "WAN mode", Foreground = Dark,
                                          FontSize = 13 });
        return Card(panel);
    }

    private static Border TransferCard()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(SectionHeader("Transfer"));
        panel.Children.Add(Label("Source file"));
        panel.Children.Add(Field("Full path of the file to send"));
        panel.Children.Add(Label("Destination directory"));
        panel.Children.Add(Field("Default download folder"));
        var first = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        first.Children.Add(BigButton("Wait for transfer", false));
        first.Children.Add(BigButton("Send file", true));
        panel.Children.Add(first);
        var second = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        second.Children.Add(BigButton("Loopback self test", false));
        second.Children.Add(BigButton("Cancel", false));
        panel.Children.Add(second);
        return Card(panel);
    }

    private static Border ProgressCard()
    {
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(SectionHeader("Progress"));
        panel.Children.Add(new TextBlock { Text = "Ready - no active transfer",
                                           FontSize = 12, Foreground = Muted });
        panel.Children.Add(new ProgressBar { Minimum = 0, Maximum = 100, Value = 0,
                                             Height = 10, Foreground = Blue });
        panel.Children.Add(new ListBox { Height = 84 });
        return Card(panel);
    }

    private static Border Card(Control content) => new()
    {
        Background = Brushes.White,
        CornerRadius = new CornerRadius(14),
        BorderBrush = Line,
        BorderThickness = new Thickness(1),
        Padding = new Thickness(16),
        Child = content,
    };

    private static Control SectionHeader(string title)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(new Border { Width = 3, Height = 15,
                                      CornerRadius = new CornerRadius(2),
                                      Background = Blue,
                                      VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new TextBlock { Text = title, FontSize = 15,
                                         FontWeight = FontWeight.SemiBold,
                                         Foreground = Blue });
        var group = new StackPanel { Spacing = 8 };
        group.Children.Add(row);
        group.Children.Add(new Border { Height = 1, Background = Line });
        return group;
    }

    private static TextBlock Label(string text) =>
        new() { Text = text, FontSize = 12, Foreground = Muted };

    private static TextBox Field(string watermark) =>
        new() { Watermark = watermark, FontSize = 13,
                Padding = new Thickness(10, 9) };

    private static Button BigButton(string text, bool primary) => new()
    {
        Content = text,
        // Larger padding than the desktop buttons for comfortable touch targets.
        Padding = new Thickness(13, 12),
        FontSize = 13,
        Background = primary ? Blue : new SolidColorBrush(Color.Parse("#EEF2F9")),
        Foreground = primary ? Brushes.White : Dark,
    };
}
