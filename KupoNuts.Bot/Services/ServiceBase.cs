// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System.Threading.Tasks;
	using KupoNuts.Bot.Commands;

	public abstract class ServiceBase
	{
		public ServiceBase()
		{
			CommandsService.BindCommands(this);
		}

		public virtual Task Initialize()
		{
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}
