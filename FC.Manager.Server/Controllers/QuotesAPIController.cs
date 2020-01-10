// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Controllers
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.Data;
	using FC.Quotes;
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

			Table<Quote> quotesDb = Table<Quote>.Create("Quotes", Quote.Version);
			await quotesDb.Connect();
			List<Quote> events = await quotesDb.LoadAll();
			return events;
		}

		[HttpPost]
		public async Task Post(DataAction<Quote> action)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			Table<Quote> quotesDb = Table<Quote>.Create("Quotes", Quote.Version);
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
