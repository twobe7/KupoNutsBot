// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using KupoNuts.Bot;
	using KupoNuts.Events;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class EventsAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<IEnumerable<Event>> Get()
		{
			Database<Event> eventsDb = new Database<Event>("Events", 1);
			await eventsDb.Connect();
			List<Event> events = await eventsDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(EventAction evt)
		{
			Database<Event> eventsDb = new Database<Event>("Events", 1);
			await eventsDb.Connect();

			switch (evt.Action)
			{
				case EventAction.Actions.Update:
				{
					await eventsDb.Save(evt.Event);
					break;
				}

				case EventAction.Actions.Delete:
				case EventAction.Actions.DeleteConfirmed:
				{
					Log.Write("Delete Event: \"" + evt.Event.Name + "\" (" + evt.Event.Id + ")");
					await eventsDb.Delete(evt.Event);
					break;
				}
			}
		}
	}
}
