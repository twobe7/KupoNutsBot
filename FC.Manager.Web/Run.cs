namespace FC.Manager.Web;

using Blazored.Modal;
using Blazored.Toast;
using FC.Manager.Web.Components;
using FC.Manager.Web.Services;

public class Run
{
	public static async Task RunApp(string[] args)
	{
		////const string ContentRootPath = Directory.GetCurrentDirectory(); // path to this project
		var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
		{
			Args = args,
			//ContentRootPath = ContentRootPath,
			ApplicationName = typeof(Program).Assembly.FullName,
			//WebRootPath = @$"{ContentRootPath}\wwwroot",
		});

		////var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		Authentication.GenerateSecret();

		builder.Services
			.AddSingleton<RPCService>()
			.AddSingleton<DiscordService>()
			.AddSingleton<AuthenticationService>()
			.AddSingleton<ContentCreatorService>()
			.AddSingleton<EmoteService>()
			.AddSingleton<EventsService>()
			.AddSingleton<EventsV2Service>()
			.AddSingleton<GuildService>()
			.AddSingleton<LeaderboardSettingsService>()
			.AddSingleton<ReactionRoleService>()
			.AddSingleton<SettingsService>()
			.AddSingleton<ShopService>();

		builder.Services.AddHttpClient()
			.AddScoped<HttpClient>();

		builder.Services.AddServerSideBlazor()
			.AddCircuitOptions(options => { options.DetailedErrors = true; })
			.Services
				.AddBlazoredModal()
				.AddBlazoredToast();


		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error", createScopeForErrors: true);
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		////app.UseHttpsRedirection();

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		await app.RunAsync();
	}
}