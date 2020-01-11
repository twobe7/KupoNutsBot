// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Events;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class EventsAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<IEnumerable<Event>> Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Table<Event> eventsDb = Table<Event>.Create("KupoNuts_Events", 1);
			await eventsDb.Connect();
			List<Event> events = await eventsDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<Event> evt)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Table<Event> eventsDb = Table<Event>.Create("KupoNuts_Events", 1);
			await eventsDb.Connect();

			switch (evt.Action)
			{
				case Actions.Update:
				{
					await eventsDb.Save(evt.Data);
					break;
				}

				case Actions.Delete:
				case Actions.DeleteConfirmed:
				{
					Log.Write("Delete Event: \"" + evt.Data.Name + "\" (" + evt.Data.Id + ")", "Manager");
					await eventsDb.Delete(evt.Data);
					break;
				}
			}
		}
	}
}
