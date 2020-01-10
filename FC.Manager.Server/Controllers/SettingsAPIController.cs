// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Controllers
{
	using System.Net.Http;
	using Microsoft.AspNetCore.Mvc;

	[ApiController]
	[Route("[controller]")]
	public class SettingsAPIController : ControllerBase
	{
		[HttpGet]
		public Settings Get()
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return null;

			Settings settings = Settings.Load();
			return settings;
		}

		[HttpPost]
		public void Post(Settings settings)
		{
			if (!Authentication.IsAuthenticated(this.Request))
				return;

			settings.Save();
		}
	}
}
