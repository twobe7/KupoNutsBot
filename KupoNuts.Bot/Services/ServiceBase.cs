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

		public bool Alive
		{
			get;
			private set;
		}

		public virtual Task Initialize()
		{
			this.Alive = true;
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			this.Alive = false;
			return Task.CompletedTask;
		}
	}
}
