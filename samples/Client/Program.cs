using System;
using Microsoft.AspNetCore.Blazor.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;

namespace SerilogBlazorDemo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SelfLog.Enable(m => Console.Error.WriteLine(m));

            var levelSwitch = new LoggingLevelSwitch();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.BrowserConsole()
                .WriteTo.BrowserHttp(controlLevelSwitch: levelSwitch)
                .CreateLogger();

            Log.Information("Hello, browser!");

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An exception occurred while creating the WASM host");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
