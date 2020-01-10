// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Manager.Client.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Components;
	using Newtonsoft.Json;

	public static class RPCService
	{
		public static async Task<TResult> Invoke<TResult>(HttpClient client, string method, params object[] param)
		{
			RPCRequest req = new RPCRequest();
			req.Method = method;

			for (int i = 0; i < param.Length; i++)
			{
				req.ParamData.Add(JsonConvert.SerializeObject(param[i]));
			}

			RPCResult result = await client.PostJsonAsync<RPCResult>("RPC", req);

			// TODO: get exception type.
			if (!string.IsNullOrEmpty(result.Exception))
				throw new Exception(result.Exception);

			if (string.IsNullOrEmpty(result.Data))
				return default(TResult);

			return JsonConvert.DeserializeObject<TResult>(result.Data);
		}
	}
}
