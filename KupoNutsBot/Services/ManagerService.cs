// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public class ManagerService : ServiceBase
	{
		public override Task Initialize()
		{
			return Task.CompletedTask;
		}

		public override Task Shutdown()
		   {
			return Task.CompletedTask;
		}
	}
}
