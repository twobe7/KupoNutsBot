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
		[HttpGet]
		public IEnumerable<Event> Get()
		{
			List<Event> events = new List<Event>();

			Event evt = new Event();
			evt.Name = "Admin Meeting";
			evt.Description = "A meeting of the glorious Kupo Nut Brigade overlords!";
			evt.Repeats = Event.Days.Sunday;
			evt.Duration = Duration.FromHours(2);
			evt.DateTime = Instant.FromUtc(2019, 1, 6, 8, 0);

			events.Add(evt);
			events.Add(evt);
			events.Add(evt);
			events.Add(evt);

			return events;
		}
	}
}
