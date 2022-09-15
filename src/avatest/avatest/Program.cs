using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;
using Avalonia.ReactiveUI;
using System;
using System.Collections.Generic;

namespace avatest
{
    internal class Program
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

                .UseSkia()
                .With(new Win32PlatformOptions { AllowEglInitialization = true, UseWgl = true, UseDeferredRendering = true })  // Windows
                .With(new X11PlatformOptions { UseGpu = true, UseEGL = true, UseDeferredRendering = true })  // Linux
                .With(new AvaloniaNativePlatformOptions { UseGpu = true, UseDeferredRendering = true })   // OSX
                //.With(new AngleOptions { AllowedPlatformApis = new List<AngleOptions.PlatformApi> { AngleOptions.PlatformApi.DirectX11 } })  // OpenGL

                .LogToTrace()
                .UseReactiveUI();
    }
}
