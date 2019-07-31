// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Discord.WebSocket;
	using KupoNutsBot.Commands;
	using KupoNutsBot.Utils;

	public class ManagerService : ServiceBase
	{
		private const string ManagerLocation = "KupoNutsBot/bin/manager/Manager.dll";

		private CommandLine.CommandProcess managerProcess;

		public override Task Initialize()
		{
			CommandsService.BindCommand("restartManager", this.Restart, Permissions.Administrators, "Restarts the management interface.");

			this.managerProcess = CommandLine.DotNetRun(ManagerLocation);

			return Task.CompletedTask;
		}

		public override async Task Shutdown()
		{
			CommandsService.ClearCommand("restartManager");
			await this.managerProcess.Kill();
		}

		private async Task Restart(string[] args, SocketMessage message)
		{
			await this.Shutdown();
			await this.Initialize();
		}
	}
}
