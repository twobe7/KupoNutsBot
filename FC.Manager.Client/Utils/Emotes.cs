// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Microsoft.AspNetCore.Components;

	public static class Emotes
	{
		public static async Task<List<Emote>> GetEmotes()
		{
			List<Emote> allEmotes = await RPCService.Invoke<List<Emote>>("EmoteService.GetEmotes");

			////allEmotes.Sort((Emote a, Emote b) =>
			////{
			////	return a.Name.CompareTo(b.Name);
			////});

			return allEmotes;
		}
	}
}
