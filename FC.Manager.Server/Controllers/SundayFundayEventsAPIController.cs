// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class SundayFundayEventsAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<IEnumerable<SundayFundayEvent>> Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Table<SundayFundayEvent> itemsDb = Table<SundayFundayEvent>.Create("SundayFunday", 0);
			await itemsDb.Connect();
			List<SundayFundayEvent> events = await itemsDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<SundayFundayEvent> evt)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Table<SundayFundayEvent> itemsDb = Table<SundayFundayEvent>.Create("SundayFunday", 0);
			await itemsDb.Connect();

			switch (evt.Action)
			{
				case Actions.Update:
				{
					await itemsDb.Save(evt.Data);
					break;
				}

				case Actions.Delete:
				case Actions.DeleteConfirmed:
				{
					Log.Write("Delete SundayFundayEvent: \"" + evt.Data.Name + "\" (" + evt.Data.Id + ")", "Manager");
					await itemsDb.Delete(evt.Data);
					break;
				}
			}
		}
	}
}
