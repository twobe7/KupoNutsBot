// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using KupoNuts.Events;
	using KupoNuts.RPG;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class ItemsAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<IEnumerable<Item>> Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Database<Item> itemsDb = new Database<Item>("RPG_Items", 1);
			await itemsDb.Connect();
			List<Item> events = await itemsDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<Item> evt)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Database<Item> itemsDb = new Database<Item>("RPG_Items", 1);
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
					Log.Write("Delete Item: \"" + evt.Data.Name + "\" (" + evt.Data.Id + ")", "Manager");
					await itemsDb.Delete(evt.Data);
					break;
				}
			}
		}
	}
}
