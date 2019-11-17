// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
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

			Database<SundayFundayEvent> itemsDb = new Database<SundayFundayEvent>("SundayFunday", 0);
			await itemsDb.Connect();
			List<SundayFundayEvent> events = await itemsDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<SundayFundayEvent> evt)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Database<SundayFundayEvent> itemsDb = new Database<SundayFundayEvent>("SundayFunday", 0);
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
