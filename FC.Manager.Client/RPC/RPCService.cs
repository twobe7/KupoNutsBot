// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client.RPC
{
	using System;
	using System.Net.Http;
	using System.Net.Http.Json;
	using System.Threading.Tasks;
	using FC.Serialization;

	public static class RPCService
	{
		public static HttpClient Client;

		public static ulong GuildId;

		public static bool CanManageGuild;

		public static async Task<TResult> Invoke<TResult>(string method, params object[] param)
		{
			if (Client == null)
				throw new Exception("No HttpClient in RPC Service");

			RPCRequest req = new RPCRequest
			{
				Method = method,
				Token = Authentication.Token,
				GuildId = GuildId,
			};

			for (int i = 0; i < param.Length; i++)
			{
				req.ParamData.Add(Serializer.Serialize(param[i]));
			}

			var response = await Client.PostAsJsonAsync("RPC", req);
			var result = await response.Content.ReadFromJsonAsync<RPCResult>();

			// TODO: get exception type.
			if (!string.IsNullOrEmpty(result.Exception))
				throw new Exception(result.Exception);

			if (string.IsNullOrEmpty(result.Data))
				return default;

			return Serializer.Deserialize<TResult>(result.Data);
		}

		public static async Task Invoke(string method, params object[] param)
		{
			if (Client == null)
				throw new Exception("No HttpClient in RPC Service");

			RPCRequest req = new RPCRequest
			{
				Method = method,
				Token = Authentication.Token,
				GuildId = GuildId,
			};

			for (int i = 0; i < param.Length; i++)
			{
				req.ParamData.Add(Serializer.Serialize(param[i]));
			}

			var response = await Client.PostAsJsonAsync("RPC", req);
			var result = await response.Content.ReadFromJsonAsync<RPCResult>();

			// TODO: get exception type.
			if (!string.IsNullOrEmpty(result.Exception))
			{
				throw new Exception(result.Exception);
			}
		}
	}
}
