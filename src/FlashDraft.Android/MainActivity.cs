using global::Android.App;
using global::Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using FlashDraft.App;

namespace FlashDraft.AndroidHost;

[Activity(
    Label = "FlashDraft",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity<global::FlashDraft.App.App>
{
    // Adjustable: camera-based QR scanning and foreground service wiring belong
    // here when the pairing and background transfer flows are enabled.
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder);
}
