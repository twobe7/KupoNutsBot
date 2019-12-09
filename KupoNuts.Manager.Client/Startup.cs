// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Client
{
	using Blazored.Modal;
	using Microsoft.AspNetCore.Components.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using Toolbelt.Blazor.Extensions.DependencyInjection;

	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddBlazoredModal();
		}

		public void Configure(IComponentsApplicationBuilder app)
		{
			app.AddComponent<App>(@"app");
			app.UseLocalTimeZone();
		}
	}
}
