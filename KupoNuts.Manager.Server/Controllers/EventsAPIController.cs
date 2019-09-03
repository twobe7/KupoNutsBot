// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using KupoNuts.Bot;
	using KupoNuts.Events;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class EventsAPIController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<Event> Get()
		{
			return Data.Load().Events;
		}

		[HttpPost]
		public void Post(EventAction evt)
		{
			switch (evt.Action)
			{
				case EventAction.Actions.Update:
				{
					Database db = Data.Load();
					db.UpdateOrInsertEvent(evt.Event);
					db.Save();
					break;
				}

				case EventAction.Actions.Delete:
				case EventAction.Actions.DeleteConfirmed:
				{
					Log.Write("Delete Event: \"" + evt.Event.Name + "\" (" + evt.Event.Id + ")");
					Database db = Data.Load();
					db.DeleteEvent(evt.Event.Id);
					db.Save();
					break;
				}
			}
		}
	}
}
