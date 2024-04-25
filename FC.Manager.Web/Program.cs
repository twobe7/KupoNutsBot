using Blazored.Modal;
using Blazored.Toast;
using FC.Manager.Web;
using FC.Manager.Web.Components;
using FC.Manager.Web.Components.Pages;
using FC.Manager.Web.Services;

// Run the bot
Task botTask = FC.Bot.Program.Run(args);

// Build Web application
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddBlazoredToast();

builder.Services.AddBlazoredModal();

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
	.AddSingleton<ShopService>()
	.AddScoped<Modal>()
	.AddScoped<Inspector>();

builder.Services.AddHttpClient()
	.AddScoped<HttpClient>();

builder.Services
	.AddServerSideBlazor()
		.AddCircuitOptions(options => { options.DetailedErrors = true; })
		.AddInteractiveServerComponents();

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

app.Run();
