// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Commands;

	public abstract class ServiceBase : InteractionModuleBase
	{
		public ServiceBase()
		{
		}

		public bool Alive
		{
			get;
			private set;
		}

		public virtual Task Initialize()
		{
			this.Alive = true;
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			this.Alive = false;
			return Task.CompletedTask;
		}
	}
}
