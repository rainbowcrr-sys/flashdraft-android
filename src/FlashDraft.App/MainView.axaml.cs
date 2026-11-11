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
/// stacking a translucent overlay on top of the main interface.
///
/// Refined visual system: a taller brand bar with a status pill, cards with a
/// tinted header strip and hairline dividers, rounded filled inputs, rounded
/// primary / secondary / danger buttons, and a bottom tab strip with an active
/// indicator. The control tree is still built in code and no ViewModel is
/// constructed, so the verified Android startup path is untouched.
/// </summary>
public class MainView : UserControl
{
    private static readonly SolidColorBrush Blue =
        new(Color.Parse("#1358DF"));
    private static readonly SolidColorBrush BlueSoft =
        new(Color.Parse("#4A8BFF"));
    private static readonly SolidColorBrush Ink =
        new(Color.Parse("#16202F"));
    private static readonly SolidColorBrush Muted =
        new(Color.Parse("#7A879C"));
    private static readonly SolidColorBrush Line =
        new(Color.Parse("#E7ECF5"));
    private static readonly SolidColorBrush PageBg =
        new(Color.Parse("#F3F6FC"));
    private static readonly SolidColorBrush FieldBg =
        new(Color.Parse("#F8FAFE"));
    private static readonly SolidColorBrush Green =
        new(Color.Parse("#2FBF71"));
    private static readonly SolidColorBrush Accent =
        new(Color.Parse("#FFC93C"));
    private static readonly SolidColorBrush Danger =
        new(Color.Parse("#C0362C"));

    private readonly Grid _pageHost = new();

    public MainView()
    {
        _pageHost.Background = PageBg;
        Content = _pageHost;
        SetPage(LicenseGate.IsAccepted() ? BuildMainPage() : BuildLicensePage());
    }

    /// <summary>Replaces the current page. Nothing is ever stacked on top.</summary>
    private void SetPage(Control page)
    {
        _pageHost.Children.Clear();
        _pageHost.Children.Add(page);
    }

    private Control PageShell(Control body, bool showTabs, string active)
    {
        var root = new Grid { Background = PageBg };
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
            Control tabs = BuildTabStrip(active);
            Grid.SetRow(tabs, 2);
            root.Children.Add(tabs);
        }

        return root;
    }

    /// <summary>Brand bar: icon, two-line title, and a live status pill.</summary>
    private static Control BuildBrandBar()
    {
        var bar = new Border
        {
            Height = 78,
            Padding = new Thickness(18, 0),
            Background = new LinearGradientBrush
            {
                StartPoint = RelativePoint.TopLeft,
                EndPoint = RelativePoint.BottomRight,
                GradientStops =
                {
                    new GradientStop(Color.Parse("#0B47C9"), 0),
                    new GradientStop(Color.Parse("#4A8BFF"), 1),
                },
            },
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };
        left.Children.Add(BrandIcon(42));
        var titles = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        titles.Children.Add(new TextBlock
        {
            Text = Loc.T("AppTitle"),
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
        });
        titles.Children.Add(new TextBlock
        {
            Text = Loc.T("PhoneSubtitle"),
            FontSize = 10.5,
            Foreground = new SolidColorBrush(Color.Parse("#DCE9FF")),
        });
        left.Children.Add(titles);

        var pill = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#2F6BE4")),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(11, 7),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var pillRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            VerticalAlignment = VerticalAlignment.Center,
        };
        pillRow.Children.Add(new Border
        {
            Width = 7,
            Height = 7,
            CornerRadius = new CornerRadius(4),
            Background = Green,
            VerticalAlignment = VerticalAlignment.Center,
        });
        pillRow.Children.Add(new TextBlock
        {
            Text = Loc.T("Ready"),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
        });
        pill.Child = pillRow;

        Grid.SetColumn(left, 0);
        Grid.SetColumn(pill, 1);
        grid.Children.Add(left);
        grid.Children.Add(pill);
        bar.Child = grid;
        return bar;
    }

    /// <summary>Bottom navigation with a rounded active indicator bar.</summary>
    private Control BuildTabStrip(string active)
    {
        var strip = new Border
        {
            Height = 64,
            Background = Brushes.White,
            BorderBrush = Line,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));

        Control transfer = TabItem(Loc.T("TabTransfer"), active == "transfer", () => SetPage(BuildMainPage()));
        Control about = TabItem(Loc.T("TabAbout"), active == "about", () => SetPage(BuildAboutPage()));
        Control settings = TabItem(Loc.T("TabSettings"), active == "settings", () => SetPage(BuildSettingsPage()));
        Grid.SetColumn(transfer, 0);
        Grid.SetColumn(about, 1);
        Grid.SetColumn(settings, 2);
        grid.Children.Add(transfer);
        grid.Children.Add(about);
        grid.Children.Add(settings);
        strip.Child = grid;
        return strip;
    }

    private static Control TabItem(string text, bool active, Action onClick)
    {
        var cell = new Grid();
        cell.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        cell.RowDefinitions.Add(new RowDefinition(GridLength.Parse("*")));

        var indicator = new Border
        {
            Width = 44,
            Height = 3,
            CornerRadius = new CornerRadius(0, 0, 2, 2),
            Background = active ? Blue : Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };
        var button = new Button
        {
            Content = text,
            FontSize = 13,
            FontWeight = active ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = active ? Blue : Muted,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Click += (_, _) => onClick();

        Grid.SetRow(indicator, 0);
        Grid.SetRow(button, 1);
        cell.Children.Add(indicator);
        cell.Children.Add(button);
        return cell;
    }

    /// <summary>Main interface: single column of refined cards plus tab strip.</summary>
    private Control BuildMainPage()
    {
        var column = new StackPanel { Spacing = 14, Margin = new Thickness(14, 14, 14, 10) };
        column.Children.Add(PairingCard());
        column.Children.Add(TransferCard());
        column.Children.Add(ProgressCard());
        return PageShell(new ScrollViewer { Content = column }, showTabs: true, active: "transfer");
    }

    /// <summary>Standalone About page with the same card language as Transfer.</summary>
    private Control BuildAboutPage()
    {
        var column = new StackPanel { Spacing = 14, Margin = new Thickness(14, 14, 14, 14) };

        column.Children.Add(Card(Loc.T("AboutTitle"), Loc.T("Project"), new Control[]
        {
            new TextBlock
            {
                Text = Loc.Notice(),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Foreground = Ink,
            },
            VersionBadge(),
        }));

        column.Children.Add(Card(Loc.T("LicenseTitle"), Loc.T("FullText"), new Control[]
        {
            LicenseReader(280),
        }));

        var back = PrimaryButton(Loc.T("BackToTransfer"));
        back.Click += (_, _) => SetPage(BuildMainPage());
        column.Children.Add(back);

        return PageShell(new ScrollViewer { Content = column }, showTabs: true, active: "about");
    }

    /// <summary>Standalone first-run license page, without a tab strip.</summary>
    private Control BuildLicensePage()
    {
        var hint = new TextBlock
        {
            Text = Loc.T("DeclineApp"),
            FontSize = 10,
            Foreground = Muted,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var check = new CheckBox
        {
            Content = Loc.T("AcceptLicense"),
            Foreground = Ink,
            FontSize = 12,
        };
        var proceed = PrimaryButton(Loc.T("Continue"));
        proceed.Click += (_, _) =>
        {
            if (check.IsChecked != true)
            {
                hint.Text = Loc.T("TickFirst");
                return;
            }

            LicenseGate.MarkAccepted();
            SetPage(BuildMainPage());
        };

        var column = new StackPanel { Margin = new Thickness(14, 14, 14, 14), Spacing = 14 };
        column.Children.Add(Card(Loc.T("LicenseAgreement"), Loc.T("Step1Of1"), new Control[]
        {
            LicenseReader(300),
            check,
            proceed,
            hint,
        }));
        return PageShell(new ScrollViewer { Content = column }, showTabs: false, active: "license");
    }

    /// <summary>Standalone Settings page with a single language switch.</summary>
    private Control BuildSettingsPage()
    {
        var column = new StackPanel { Spacing = 14, Margin = new Thickness(14, 14, 14, 14) };

        var hintEn = new TextBlock
        {
            Text = Loc.T("LanguageHint"),
            FontSize = 11.5,
            Foreground = Muted,
            TextWrapping = TextWrapping.Wrap,
        };
        var hintDefault = new TextBlock
        {
            Text = Loc.T("DefaultFromInstall"),
            FontSize = 11.5,
            Foreground = Muted,
            TextWrapping = TextWrapping.Wrap,
        };

        Button zh = Loc.IsChinese ? PrimaryButton(Loc.T("LangChinese")) : SecondaryButton(Loc.T("LangChinese"));
        Button en = Loc.IsChinese ? SecondaryButton(Loc.T("LangEnglish")) : PrimaryButton(Loc.T("LangEnglish"));
        zh.Click += (_, _) => { Loc.Set(Loc.Chinese); SetPage(BuildSettingsPage()); };
        en.Click += (_, _) => { Loc.Set(Loc.English); SetPage(BuildSettingsPage()); };

        column.Children.Add(Card(Loc.T("SettingsTitle"), Loc.T("SettingsTag"), new Control[]
        {
            new TextBlock { Text = Loc.T("LanguageLabel"), FontSize = 13,
                            FontWeight = FontWeight.SemiBold, Foreground = Ink },
            hintEn,
            hintDefault,
            zh,
            en,
            new TextBlock { Text = Loc.T("Saved"), FontSize = 11,
                            Foreground = Green, TextWrapping = TextWrapping.Wrap },
        }));

        return PageShell(new ScrollViewer { Content = column }, showTabs: true, active: "settings");
    }

    private static Border LicenseReader(double height) => new()
    {
        Height = height,
        Background = FieldBg,
        BorderBrush = Line,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(12),
        Child = new ScrollViewer
        {
            Content = new TextBlock
            {
                Text = LicenseContent.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10.5,
                Foreground = Ink,
            },
        },
    };

    private static Control VersionBadge()
    {
        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#EEF4FF")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 6),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        badge.Child = new TextBlock
        {
            Text = Loc.T("Version"),
            FontSize = 10.5,
            FontWeight = FontWeight.SemiBold,
            Foreground = Blue,
        };
        return badge;
    }

    /// <summary>A card with a tinted header strip, hairline divider and padded body.</summary>
    private static Border Card(string title, string hint, IEnumerable<Control> content)
    {
        var head = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F1F6FF")),
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Padding = new Thickness(16, 13, 16, 13),
        };
        var headGrid = new Grid();
        headGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        headGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        headGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var bar = new Border
        {
            Width = 4,
            Height = 16,
            CornerRadius = new CornerRadius(2),
            Background = Blue,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };
        var heading = new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = Ink,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var tag = new TextBlock
        {
            Text = hint,
            FontSize = 9.5,
            FontWeight = FontWeight.SemiBold,
            Foreground = Muted,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(bar, 0);
        Grid.SetColumn(heading, 1);
        Grid.SetColumn(tag, 2);
        headGrid.Children.Add(bar);
        headGrid.Children.Add(heading);
        headGrid.Children.Add(tag);
        head.Child = headGrid;

        var body = new StackPanel
        {
            Margin = new Thickness(16, 14, 16, 16),
            Spacing = 11,
        };
        foreach (Control item in content) body.Children.Add(item);

        var stack = new StackPanel { Spacing = 0 };
        stack.Children.Add(head);
        stack.Children.Add(new Border { Height = 1, Background = Line });
        stack.Children.Add(body);

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(16),
            BorderBrush = Line,
            BorderThickness = new Thickness(1),
            Child = stack,
        };
    }

    private static Border PairingCard()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(PrimaryButton(Loc.T("GenerateCode")));
        row.Children.Add(SecondaryButton(Loc.T("BindPeer")));

        return Card(Loc.T("Pairing"), Loc.T("Step1"), new Control[]
        {
            Label(Loc.T("MyOfferCode")),
            Field(Loc.T("WatermarkGenerate")),
            Label(Loc.T("PeerOfferCode")),
            Field("flashdraft://..."),
            row,
            new CheckBox { Content = Loc.T("WanMode"), Foreground = Ink, FontSize = 12 },
        });
    }

    private static Border TransferCard()
    {
        var first = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        first.Children.Add(SecondaryButton(Loc.T("WaitTransfer")));
        first.Children.Add(PrimaryButton(Loc.T("SendFile")));
        var second = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        second.Children.Add(SecondaryButton(Loc.T("Loopback")));
        second.Children.Add(DangerButton(Loc.T("Cancel")));

        return Card(Loc.T("TabTransfer"), Loc.T("Step2"), new Control[]
        {
            Label(Loc.T("SourceFile")),
            Field(Loc.T("WatermarkSource")),
            Label(Loc.T("DestDir")),
            Field(Loc.T("WatermarkDest")),
            first,
            second,
        });
    }

    private static Border ProgressCard()
    {
        var status = new Grid();
        status.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        status.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Parse("*")));
        status.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        var dot = new Border
        {
            Width = 7,
            Height = 7,
            CornerRadius = new CornerRadius(4),
            Background = Green,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };
        var state = new TextBlock
        {
            Text = Loc.T("ReadyNoTransfer"),
            FontSize = 11.5,
            Foreground = Muted,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var speed = new TextBlock
        {
            Text = "0 B/s",
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = Blue,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(dot, 0);
        Grid.SetColumn(state, 1);
        Grid.SetColumn(speed, 2);
        status.Children.Add(dot);
        status.Children.Add(state);
        status.Children.Add(speed);

        return Card(Loc.T("Progress"), Loc.T("Live"), new Control[]
        {
            status,
            new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Height = 9,
                Foreground = Blue,
                Background = new SolidColorBrush(Color.Parse("#EDF1F8")),
            },
            new ListBox { Height = 82, Background = FieldBg, BorderBrush = Line,
                          BorderThickness = new Thickness(1) },
        });
    }

    private static TextBlock Label(string text) =>
        new() { Text = text, FontSize = 11.5, FontWeight = FontWeight.SemiBold, Foreground = Muted };

    private static TextBox Field(string watermark) => new()
    {
        Watermark = watermark,
        FontSize = 13,
        Foreground = Ink,
        Background = FieldBg,
        BorderBrush = Line,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(12, 10),
    };

    private static Button PrimaryButton(string text) => new()
    {
        Content = text,
        FontSize = 12.5,
        FontWeight = FontWeight.SemiBold,
        Foreground = Brushes.White,
        Background = Blue,
        BorderThickness = new Thickness(0),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(12, 12),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
    };

    private static Button SecondaryButton(string text) => new()
    {
        Content = text,
        FontSize = 12.5,
        FontWeight = FontWeight.SemiBold,
        Foreground = Blue,
        Background = Brushes.White,
        BorderBrush = new SolidColorBrush(Color.Parse("#C9D8F4")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(12, 12),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
    };

    private static Button DangerButton(string text) => new()
    {
        Content = text,
        FontSize = 12.5,
        FontWeight = FontWeight.SemiBold,
        Foreground = Danger,
        Background = new SolidColorBrush(Color.Parse("#FEF3F2")),
        BorderBrush = new SolidColorBrush(Color.Parse("#F6D2CE")),
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Padding = new Thickness(12, 12),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
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
                    new GradientStop(Color.Parse("#4A8BFF"), 1),
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
            Fill = Accent,
        });

        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(11),
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
}
