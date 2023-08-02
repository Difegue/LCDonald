using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using LCDonald.Core.Controller;

namespace LCDonald.Android;

[Activity(
    Label = "LCDonald.Android", 
    Theme = "@style/MyTheme.NoActionBar", 
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Ioc.Default.ConfigureServices(
             new ServiceCollection()
             .AddSingleton<IInteropService, AndroidInteropService>()
             .BuildServiceProvider());

        return base.CustomizeAppBuilder(builder);
    }

}
