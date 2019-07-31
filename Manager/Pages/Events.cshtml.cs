// This document is intended for use by Kupo Nut Brigade developers.

namespace Manager.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.RazorPages;
	using Microsoft.AspNetCore.Mvc.Rendering;

	public class EventsModel : PageModel
	{
		public SelectList ChannelList { get; set; }

		public void OnGet()
		{
			List<string> values = new List<string>();
			values.Add("Test");

			this.ChannelList = new SelectList(values);
		}

		public void OnPostEvent(int id)
		{
			Console.WriteLine("Post used");
		}
	}
}
