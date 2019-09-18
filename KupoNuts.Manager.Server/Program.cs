// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server
{
	using System.Threading.Tasks;
	using KupoNuts.Bot;
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

		public static Task Run(string[] args)
		{
			host = BuildWebHost(args);
			Authentication.GenerateSecret();
			return host.RunAsync();
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

		public static Task Exit()
		{
			return host.StopAsync();
		}
	}
}
