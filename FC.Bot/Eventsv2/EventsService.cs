// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using FC.Bot.Services;
	using FC.Data;
	using FC.Eventsv2;
	using FC.Utils;
	using NodaTime;

	public class EventsService : ServiceBase
	{
		private static Table<Event> eventsDatabase = new Table<Event>("Events", 2);

		public static Task SaveEvent(Event evt)
		{
			return eventsDatabase.Save(evt);
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			await eventsDatabase.Connect();

			// Test event
			/*Event evt = new Event();
			evt.BaseTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Australia/Sydney");
			evt.Name = "Test event";
			evt.Description = "More testing";
			evt.BeginDate = new LocalDate(2019, 12, 01);
			evt.Channel = 624506315259248671;
			evt.NoticeDuration = Duration.FromDays(1);

			Event.Rule repeatTueSun = new Event.Rule();
			repeatTueSun.Days = Event.Rule.Day.Tuesday | Event.Rule.Day.Sunday;
			repeatTueSun.StartTime = new LocalTime(19, 00);
			repeatTueSun.Duration = Duration.FromMinutes(180);
			repeatTueSun.Units = Event.Rule.TimeUnit.Week;
			repeatTueSun.RepeatEvery = 1;
			evt.Rules.Add(repeatTueSun);

			Event.Rule repeatMon = new Event.Rule();
			repeatMon.Days = Event.Rule.Day.Monday;
			repeatMon.StartTime = new LocalTime(19, 00);
			repeatMon.Duration = Duration.FromMinutes(120);
			repeatMon.Units = Event.Rule.TimeUnit.Week;
			repeatMon.RepeatEvery = 1;
			evt.Rules.Add(repeatMon);

			this.events.Add(evt);*/

			ScheduleService.RunOnSchedule(this.Update);
			await this.Update();
		}

		private async Task Update()
		{
			if (eventsDatabase is null)
				return;

			List<Event> events = await eventsDatabase.LoadAll();
			foreach (Event evt in events)
			{
				await evt.UpdateNotices();
			}
		}
	}
}
