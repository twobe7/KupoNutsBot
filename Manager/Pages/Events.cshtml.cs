// This document is intended for use by Kupo Nut Brigade developers.

namespace Manager.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using KupoNutsBot.Events;
	using KupoNutsBot.Utils;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.RazorPages;
	using Microsoft.AspNetCore.Mvc.Rendering;
	using NodaTime;

	public class EventsModel : PageModel
	{
		private readonly List<Event> events = new List<Event>();

		public SelectList ChannelList
		{
			get;
			set;
		}

		public SelectList ColorList
		{
			get;
			set;
		}

		public DateTime DropOffDate
		{
			get;
			set;
		}

		public bool ShowPopup
		{
			get;
			set;
		}

		public List<Event> Events
		{
			get
			{
				if (this.events.Count <= 0)
				{
					Event evt = new Event();
					evt.Name = "Admin Meeting";
					evt.Description = "A meeting of the glorious Kupo Nut Brigade overlords!";
					evt.Repeats = Event.Days.Sunday;
					evt.Duration = Duration.FromHours(2);
					evt.DateTime = Instant.FromUtc(2019, 1, 6, 8, 0);
					this.events.Add(evt);
				}

				return this.events;
			}
		}

		public void OnGet()
		{
			this.ShowPopup = true;

			List<string> values = new List<string>();
			values.Add("Test");

			this.ChannelList = new SelectList(values);

			this.ColorList = new SelectList(Enum.GetNames(typeof(Event.Colors)));
		}

		public string GetNextOccurance(Event evt)
		{
			DateTimeZone zone = TimeUtils.AEST;
			Instant instant = evt.NextOccurance(zone);
			ZonedDateTime zdt = instant.InZone(zone);

			return zdt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
		}

		public void OnPostEditEvent(object sender, EventArgs e)
		{
			this.ShowPopup = false;
			Console.WriteLine("Post used");
		}

		public void OnPostEvent(
			string eventName,
			string eventDesc,
			string channelSelect,
			Event.Colors colorSelect,
			DateTime eventDate,
			TimeSpan eventTime,
			TimeSpan eventDuration,
			bool repeatMonday,
			bool repeatTuesday,
			bool repeatWednesday,
			bool repeatThursday,
			bool repeatFriday,
			bool repeatSaturday,
			bool repeatSunday)
		{
			Console.WriteLine("Post used");
		}
	}
}
