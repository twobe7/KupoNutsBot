// This document is intended for use by Kupo Nut Brigade developers.

namespace Manager.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using KupoNutsBot.Events;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.RazorPages;
	using Microsoft.AspNetCore.Mvc.Rendering;

	public class EventsModel : PageModel
	{
		public SelectList ChannelList { get; set; }

		public SelectList ColorList { get; set; }

		public DateTime DropOffDate { get; set; }

		public void OnGet()
		{
			List<string> values = new List<string>();
			values.Add("Test");

			this.ChannelList = new SelectList(values);

			this.ColorList = new SelectList(Enum.GetNames(typeof(Event.Colors)));
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
