using global::Android.App;
using global::Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using FlashDraft.App;

namespace FlashDraft.AndroidHost;

/// <summary>
/// Explicit Name, exported flag, launcher icons and a NoActionBar theme are
/// required for Android 12+ to launch the app reliably. Without the theme the
/// process can terminate immediately after the splash screen.
/// </summary>
[Activity(
    Name = "com.flashdraft.app.MainActivity",
    Label = "FlashDraft",
    MainLauncher = true,
    Exported = true,
    Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher_round",
    Theme = "@style/AppTheme",
    HardwareAccelerated = true,
    LaunchMode = LaunchMode.SingleTop,
    WindowSoftInputMode = global::Android.Views.SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize |
                            ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                            ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : AvaloniaMainActivity<global::FlashDraft.App.App>
{
    // Adjustable: camera-based QR scanning and foreground service wiring belong
    // here when the pairing and background transfer flows are enabled.
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder);
}
