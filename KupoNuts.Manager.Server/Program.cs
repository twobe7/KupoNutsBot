// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server
{
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;

	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseConfiguration(new ConfigurationBuilder()
					.AddCommandLine(args)
					.Build())
				.UseStartup<Startup>()
				.Build();
	}
}
