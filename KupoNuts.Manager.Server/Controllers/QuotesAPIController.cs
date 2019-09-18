// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using KupoNuts.Quotes;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class QuotesAPIController : ControllerBase
	{
		[HttpGet]
		public async Task<IEnumerable<Quote>> Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Database<Quote> quotesDb = new Database<Quote>("Quotes", Quote.Version);
			await quotesDb.Connect();
			List<Quote> events = await quotesDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<Quote> action)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Database<Quote> quotesDb = new Database<Quote>("Quotes", Quote.Version);
			await quotesDb.Connect();

			switch (action.Action)
			{
				case Actions.Update:
				{
					await quotesDb.Save(action.Data);
					break;
				}

				case Actions.Delete:
				case Actions.DeleteConfirmed:
				{
					Log.Write("Delete Event: \"" + action.Data.Content + "\" (" + action.Data.Id + ")", "Manager");
					await quotesDb.Delete(action.Data);
					break;
				}
			}
		}
	}
}
