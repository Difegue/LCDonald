using Android.App;
using Android.Content;
using Android.OS;
using Application = Android.App.Application;
using Avalonia;
using Avalonia.Android;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using LCDonald.Core.Controller;

namespace LCDonald.Android;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
public class SplashActivity : AvaloniaSplashActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Ioc.Default.ConfigureServices(
             new ServiceCollection()
             .AddSingleton<IInteropService, AndroidInteropService>()
             .BuildServiceProvider());

        return base.CustomizeAppBuilder(builder);
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }

    protected override void OnResume()
    {
        base.OnResume();

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }
}
