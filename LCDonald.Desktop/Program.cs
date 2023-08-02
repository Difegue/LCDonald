using System;
using System.Reflection.PortableExecutable;
using Avalonia;
using CommunityToolkit.Mvvm.DependencyInjection;
using LCDonald.Core.Controller;
using Microsoft.Extensions.DependencyInjection;

namespace LCDonald.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

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
