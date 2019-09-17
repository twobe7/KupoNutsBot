// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public static class Scheduler
	{
		public static void RunOnSchedule(Func<Task> method, int minutesDelay)
		{
			_ = Task.Factory.StartNew(async () => { await RunAsync(method, minutesDelay); }, TaskCreationOptions.LongRunning);
		}

		private static async Task RunAsync(Func<Task> method, int minutesDelay)
		{
			while (Program.Running)
			{
				int minutes = DateTime.UtcNow.Minute;
				int delay = minutesDelay - minutes;
				while (delay < 0)
					delay += minutesDelay;

				Log.Write("Delaying schedule: " + delay + " minutes", "Bot");
				await Task.Delay(new TimeSpan(0, delay, 0));

				await method.Invoke();

				// Wait 2 minutes before anythign else.
				await Task.Delay(new TimeSpan(0, 2, 0));
			}
		}
	}
}
