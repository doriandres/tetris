using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using SharpHook;
using Windows.Graphics;

namespace Tetris;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(wndLifeCycleBuilder =>
            {
                wndLifeCycleBuilder.OnWindowCreated(window =>
                {
                    var nativeWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var win32WindowsId = Win32Interop.GetWindowIdFromWindow(nativeWindowHandle);
                    var winuiAppWindow = AppWindow.GetFromWindowId(win32WindowsId);
                    if (winuiAppWindow.Presenter is OverlappedPresenter p)
                    {
                        p.Maximize();
                    }
                    else
                    {
                        const int width = 1200;
                        const int height = 800;
                        winuiAppWindow.MoveAndResize(new RectInt32(1920 / 2 - width / 2, 1080 / 2 - height / 2, width, height));
                    }
                });
            });
        });
#endif

        var hook = new TaskPoolGlobalHook();
        hook.KeyPressed += TetrisController.OnKeyPressed;

#if DEBUG
        builder.Logging.AddDebug();
#endif
        _ = hook.RunAsync();

        return builder.Build();
    }


}
