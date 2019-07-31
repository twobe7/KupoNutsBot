// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using KupoNutsBot.Utils;

	public class ManagerService : ServiceBase
	{
		private const string ManagerLocation = "KupoNutsBot/bin/manager/manager.dll";

		private bool shutdown = false;
		private bool hasShutdown = false;

		public override Task Initialize()
		{
			Task task3 = new Task(async () => await this.RunManager(), TaskCreationOptions.LongRunning);
			task3.Start();
			return Task.CompletedTask;
		}

		public override async Task Shutdown()
		{
			this.shutdown = true;

			while (!this.hasShutdown)
			{
				await Task.Yield();
			}
		}

		private async Task RunManager()
		{
			Log.Write("Booting Manager...");
			Process managerProcess = null;
			this.hasShutdown = false;

			try
			{
				managerProcess = BashUtils.RunProc("dotnet " + ManagerLocation);

				while (!managerProcess.HasExited)
				{
					await Task.Yield();
					Thread.Sleep(100);

					if (this.shutdown)
						managerProcess.Kill();

					Log.Write("[Manager] " + managerProcess.StandardOutput.ReadToEnd());
				}

				// sanity check
				managerProcess.WaitForExit();
			}
			catch (Exception ex)
			{
				if (managerProcess != null)
					managerProcess.Kill();

				Log.Write(ex);
			}

			Log.Write("Manager shutdown");
			this.hasShutdown = true;
		}
	}
}
