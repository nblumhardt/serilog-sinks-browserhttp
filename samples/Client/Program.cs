using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SerilogBlazorDemo.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var levelSwitch = new LoggingLevelSwitch();
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.ControlledBy(levelSwitch)
				.Enrich.WithProperty("InstanceId", Guid.NewGuid().ToString("n"))
				.WriteTo.BrowserConsole()
				.WriteTo.BrowserHttp(controlLevelSwitch: levelSwitch)
				.CreateLogger();

			Log.Information("Hello, browser!");

			try
			{
				var builder = WebAssemblyHostBuilder.CreateDefault(args);
				builder.RootComponents.Add<App>("app");

				builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

				await builder.Build().RunAsync();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "An exception occurred while creating the WASM host");
				throw;
			}
		}
	}
}
