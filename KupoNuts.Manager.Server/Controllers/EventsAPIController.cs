// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using KupoNuts.Events;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class EventsAPIController : ControllerBase
	{
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
