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
/// Phone layout. Transfer, About and the first-run License page are three
/// separate, equal-level pages: the host swaps its single child instead of
/// stacking a translucent overlay on top of the main interface. Every page
/// shares the same brand bar, white cards and blue section headers.
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
    private static readonly SolidColorBrush PageBackground =
        new(Color.Parse("#F2F5FB"));

    private readonly Grid _pageHost = new();

    public MainView()
    {
        _pageHost.Background = PageBackground;
        Content = _pageHost;
        SetPage(LicenseGate.IsAccepted() ? BuildMainPage() : BuildLicensePage());
    }

    /// <summary>Replaces the current page. Nothing is ever stacked on top.</summary>
    private void SetPage(Control page)
    {
        _pageHost.Children.Clear();
        _pageHost.Children.Add(page);
    }

    private Control PageShell(Control body, bool showTabs, bool aboutActive)
    {
        var root = new Grid { Background = PageBackground };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Parse("*")));
        if (showTabs)
        {
            root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        Control bar = BuildBrandBar();
        Grid.SetRow(bar, 0);
        root.Children.Add(bar);

        Grid.SetRow(body, 1);
        root.Children.Add(body);

        if (showTabs)
        {
            Control tabs = BuildTabStrip(aboutActive);
            Grid.SetRow(tabs, 2);
            root.Children.Add(tabs);
        }

        return root;
    }

    private static Control BuildBrandBar()
    {
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
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
        };
        row.Children.Add(BrandIcon(34));
        var titles = new StackPanel { Spacing = 0, VerticalAlignment = VerticalAlignment.Center };
        titles.Children.Add(new TextBlock
        {
            Text = "FlashDraft",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
        });
        titles.Children.Add(new TextBlock
        {
            Text = "LAN and Internet file transfer",
            FontSize = 10,
            Foreground = Brushes.White,
        });
        row.Children.Add(titles);
        bar.Child = row;
        return bar;
    }

    private Control BuildTabStrip(bool aboutActive)
    {
        var tabs = new Border
        {
            Height = 56,
            Background = Brushes.White,
            BorderBrush = Line,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));

        var transfer = new Button
        {
            Content = "Transfer",
            FontSize = 14,
            Foreground = aboutActive ? Muted : Blue,
            FontWeight = aboutActive ? FontWeight.Normal : FontWeight.SemiBold,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        transfer.Click += (_, _) => SetPage(BuildMainPage());

        var about = new Button
        {
            Content = "About",
            FontSize = 14,
            Foreground = aboutActive ? Blue : Muted,
            FontWeight = aboutActive ? FontWeight.SemiBold : FontWeight.Normal,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        about.Click += (_, _) => SetPage(BuildAboutPage());

        Grid.SetColumn(transfer, 0);
        Grid.SetColumn(about, 1);
        grid.Children.Add(transfer);
        grid.Children.Add(about);
        tabs.Child = grid;
        return tabs;
    }

    /// <summary>Main interface: single column of cards plus the bottom tab strip.</summary>
    private Control BuildMainPage()
    {
        var column = new StackPanel { Spacing = 12, Margin = new Thickness(12, 12, 12, 8) };
        column.Children.Add(PairingCard());
        column.Children.Add(TransferCard());
        column.Children.Add(ProgressCard());
        return PageShell(new ScrollViewer { Content = column }, showTabs: true, aboutActive: false);
    }

    /// <summary>
    /// Standalone About page. Same brand bar, same cards, same tab strip as the
    /// main interface; only the content column changes.
    /// </summary>
    private Control BuildAboutPage()
    {
        var column = new StackPanel { Spacing = 12, Margin = new Thickness(12, 12, 12, 12) };

        var aboutPanel = new StackPanel { Spacing = 10 };
        aboutPanel.Children.Add(SectionHeader("About / 关于"));
        aboutPanel.Children.Add(new TextBlock
        {
            Text = LicenseContent.Notice,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = Dark,
        });
        aboutPanel.Children.Add(new TextBlock
        {
            Text = "Version / 版本: 0.1.2",
            FontSize = 11,
            Foreground = Muted,
        });
        column.Children.Add(Card(aboutPanel));

        var licensePanel = new StackPanel { Spacing = 10 };
        licensePanel.Children.Add(SectionHeader("License / 许可全文"));
        licensePanel.Children.Add(LicenseReader(300));
        column.Children.Add(Card(licensePanel));

        var back = BigButton("返回传输 / Back to Transfer", true);
        back.HorizontalAlignment = HorizontalAlignment.Stretch;
        back.Click += (_, _) => SetPage(BuildMainPage());
        column.Children.Add(back);

        return PageShell(new ScrollViewer { Content = column }, showTabs: true, aboutActive: true);
    }

    /// <summary>
    /// Standalone first-run license page. No tab strip, so it cannot be skipped.
    /// Accepting swaps to the main page; declining means closing the app.
    /// </summary>
    private Control BuildLicensePage()
    {
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
            SetPage(BuildMainPage());
        };

        var column = new StackPanel { Margin = new Thickness(18), Spacing = 10 };
        column.Children.Add(new TextBlock
        {
            Text = "License Agreement / 许可协议",
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Foreground = Blue,
        });
        column.Children.Add(LicenseReader(300));
        column.Children.Add(check);
        column.Children.Add(proceed);
        column.Children.Add(hint);

        return PageShell(new ScrollViewer { Content = column }, showTabs: false, aboutActive: false);
    }

    private static Border LicenseReader(double height) => new()
    {
        Height = height,
        Background = new SolidColorBrush(Color.Parse("#F7F9FD")),
        BorderBrush = Line,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(8),
        Padding = new Thickness(10),
        Child = new ScrollViewer
        {
            Content = new TextBlock
            {
                Text = LicenseContent.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Foreground = Dark,
            },
        },
    };

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
