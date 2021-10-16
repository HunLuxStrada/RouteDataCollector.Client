using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using Serilog.Events;
using Serilog;

namespace RouteDataCollector
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                // .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                // .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
//                .WriteTo.File(
//                    Path.Combine(Globals.LogsDirectory, @$"{Assembly.GetExecutingAssembly().GetName().Name}-{DateTime.Now:yyyy'.'MM'.'dd}.log"),
//                    outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
//                    fileSizeLimitBytes: 1_000_000,
//#if RELEASE
//                    restrictedToMinimumLevel: LogEventLevel.Information,
//#else
//                    restrictedToMinimumLevel: LogEventLevel.Verbose,
//#endif
//                    rollOnFileSizeLimit: true,
//                    shared: true,
//                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}",
                    theme: ConsoleTheme.None,
#if RELEASE
                    restrictedToMinimumLevel: LogEventLevel.Information
#else
                    restrictedToMinimumLevel: LogEventLevel.Verbose
#endif
                )
                .CreateLogger();
            var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

			return builder.Build();
		}
	}
}