using System;
using System.Reflection.PortableExecutable;
using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using LCDonald.Core.Controller;
using Microsoft.Extensions.DependencyInjection;

namespace LCDonald.Desktop;

class Program
{
    public static string? SelectedGameShortName { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ParseCommandLineArgs(args);
        LCDonald.App.SelectedGameShortName = SelectedGameShortName;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ParseCommandLineArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--game" && i + 1 < args.Length)
            {
                SelectedGameShortName = args[i + 1];
                break;
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                //.With(new Win32PlatformOptions { AllowEglInitialization = true })
                .With(new SkiaOptions { MaxGpuResourceSizeBytes = 256000000 })
                .AfterSetup(CustomizeAppBuilder)
                .LogToTrace();

    private static void CustomizeAppBuilder(AppBuilder builder)
    {
        Ioc.Default.ConfigureServices(
             new ServiceCollection()
             .AddSingleton<IInteropService, DesktopInteropService>()
             .BuildServiceProvider());
    }
}
