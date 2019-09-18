// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;

	public class Program
	{
		private static IWebHost host;

		public static void Main(string[] args)
		{
			host = BuildWebHost(args);
			host.Run();
		}

		public static async Task Run(string[] args)
		{
			host = BuildWebHost(args);
			Authentication.GenerateSecret();
			await DiscordAPI.Start();
			await host.RunAsync();
		}

		public static IWebHost BuildWebHost(string[] args)
		{
			ConfigurationBuilder configBuilder = new ConfigurationBuilder();
			configBuilder.AddCommandLine(args);

			IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args);
			builder.UseConfiguration(configBuilder.Build());
			builder.UseStartup<Startup>();
			return builder.Build();
		}

		public static async Task Exit()
		{
			await host.StopAsync();
			DiscordAPI.Dispose();
		}
	}
}
