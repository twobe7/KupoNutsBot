// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Manager.Client
{
	using Microsoft.AspNetCore.Blazor.Hosting;

	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args)
		{
			return BlazorWebAssemblyHost.CreateDefaultBuilder().UseBlazorStartup<Startup>();
		}
	}
}
