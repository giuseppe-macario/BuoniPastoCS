using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace BuoniPastoGui;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Imposta la cultura italiana (date, giorni, numeri)
        CultureInfo.DefaultThreadCurrentCulture =
            new CultureInfo("it-IT");

        CultureInfo.DefaultThreadCurrentUICulture =
            new CultureInfo("it-IT");

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Configurazione Avalonia
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
