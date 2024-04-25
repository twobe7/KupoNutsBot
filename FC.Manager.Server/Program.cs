// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FC.Manager.Server.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class Program
{
	private static IWebHost host;
	private static List<ServiceBase> services = new List<ServiceBase>();

	public static void Main(string[] args)
	{
		host = BuildWebHost(args);
		host.Run();
	}

	public static async Task Run(string[] args)
	{
		host = BuildWebHost(args);
		Authentication.GenerateSecret();

		// Add services
		await AddService<Services.RPCService>();
		await AddService<DiscordService>();

		await AddService<AuthenticationService>();
		await AddService<ContentCreatorService>();
		await AddService<EmoteService>();
		await AddService<EventsService>();
		await AddService<EventsV2Service>();
		await AddService<GuildService>();
		await AddService<LeaderboardSettingsService>();
		await AddService<ReactionRoleService>();
		await AddService<SettingsService>();
		await AddService<ShopService>();

		// finally launch the web host.
		await host.RunAsync();
	}

	public static IWebHost BuildWebHost(string[] args)
	{
		ConfigurationBuilder configBuilder = new ();
		configBuilder.AddCommandLine(args);

		IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args);
		builder.UseConfiguration(configBuilder.Build());
		builder.UseStartup<Startup>();
		return builder.Build();
	}

	public static async Task Exit()
	{
		foreach (ServiceBase service in services)
			await service.Shutdown();

		await host.StopAsync();
	}

	public static Task AddService<T>()
		where T : ServiceBase
	{
		Log.Write("Add Service: " + typeof(T), "WebServer");
		T service = Activator.CreateInstance<T>();
		services.Add(service);
		return service.Initialize();
	}
}
