// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using KupoNuts.Events;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Logging;
	using NodaTime;

	[ApiController]
	[Route("[controller]")]
	public class EventsController : ControllerBase
	{
		/*
		Event evt = new Event();
		evt.Id = Guid.NewGuid().ToString();
		evt.Name = "Admin Meeting";
		evt.Description = "A meeting of the glorious Kupo Nut Brigade overlords!";
		evt.Repeats = Event.Days.Sunday;
		evt.Duration = Duration.FromHours(2);
		evt.DateTime = Instant.FromUtc(2019, 1, 6, 8, 0);
		*/

		[HttpGet]
		public IEnumerable<Event> Get()
		{
			return Database.Load().Events;
		}

		[HttpPost]
		public void Post(EventAction evt)
		{
			switch (evt.Action)
			{
				case EventAction.Actions.Update:
				{
					Database.UpdateOrInsert(evt.Event);
					break;
				}

				case EventAction.Actions.Delete:
				case EventAction.Actions.DeleteConfirmed:
				{
					Database.Delete(evt.Event);
					break;
				}
			}
		}
	}
}
