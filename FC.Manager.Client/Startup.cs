// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
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
