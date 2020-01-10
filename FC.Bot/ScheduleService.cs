// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Bot.Services;

	public class ScheduleService : ServiceBase
	{
		private const int UpdateDelayMinutes = 5;

		private static List<Schedule> schedules = new List<Schedule>();
		private Task? scheduleTask;

		public static void RunOnSchedule(Func<Task> method, int minutesDelay = 15)
		{
			if (minutesDelay % UpdateDelayMinutes != 0)
				throw new Exception("Scheduled task must be in increments of " + UpdateDelayMinutes + " minutes");

			if (minutesDelay > 60)
				throw new Exception("Scheduled task must be on a delay less than 60 minutes");

			Schedule schedule = new Schedule(method, minutesDelay);
			schedules.Add(schedule);
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			this.scheduleTask = await Task.Factory.StartNew(this.Run, TaskCreationOptions.LongRunning);
		}

		private async Task Run()
		{
			while (this.Alive)
			{
				// determine how long to wait to get to the next 15 minute tick
				int minutes = DateTime.UtcNow.Minute;
				int delay = UpdateDelayMinutes - minutes;
				while (delay < 0)
					delay += UpdateDelayMinutes;

				Log.Write("Wait " + delay + " minutes", "Scheduler");
				await Task.Delay(new TimeSpan(0, delay, 0));

				Log.Write("Tick Begin", "Scheduler");
				try
				{
					await this.Tick();
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}

				Log.Write("Tick Complete", "Scheduler");

				// Wait 1 minutes to ensure we don't immediately tick again.
				await Task.Delay(new TimeSpan(0, 1, 0));
			}
		}

		private async Task Tick()
		{
			int minutes = DateTime.UtcNow.Minute;

			if (minutes == 0)
				minutes = 60;

			foreach (Schedule schedule in schedules)
			{
				if (schedule.Delay % minutes == 0)
				{
					Log.Write("Skip " + schedule, "Scheduler");
					continue;
				}

				Log.Write("Run " + schedule, "Scheduler");

				// TODO: Only invoke schedules that should be on this tick.
				// for now we are updating every one, since they almost all want 15 minutes anyway...
				await schedule.Invoke();
			}
		}

		public class Schedule
		{
			public readonly int Delay;
			public readonly Func<Task> Method;

			public Schedule(Func<Task> method, int delay)
			{
				this.Method = method;
				this.Delay = delay;
			}

			public async Task Invoke(int depth = 0)
			{
				try
				{
					await this.Method.Invoke();
				}
				catch (Discord.Net.HttpException httpEx)
				{
					if (httpEx.HttpCode == System.Net.HttpStatusCode.InternalServerError && depth < 5)
					{
						// wait briefly and retry.
						await Task.Delay(100);
						await this.Invoke(depth++);
					}
					else
					{
						Log.Write(httpEx);
					}
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			}

			public override string ToString()
			{
				return this.Method.Target?.GetType() + "." + this.Method.Method.Name;
			}
		}
	}
}
