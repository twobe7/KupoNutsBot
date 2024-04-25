// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Manager.Web.RPC;
	using FC.Serialization;

	public class RPCService : ServiceBase
	{
		public static Dictionary<string, (MethodInfo, object)> Methods = new Dictionary<string, (MethodInfo, object)>();

		public static void BindMethods(object obj)
		{
			Type type = obj.GetType();
			MethodInfo[] methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				RPCAttribute attribute = method.GetCustomAttribute<RPCAttribute>();

				if (attribute == null)
					continue;

				Methods.Add(type.Name + "." + method.Name, (method, obj));
			}
		}

		public static async Task<RPCResult> Invoke(RPCRequest req)
		{
			try
			{
				if (!Methods.ContainsKey(req.Method))
					throw new Exception("No RPC Method: \"" + req.Method + "\"");

				(MethodInfo method, object target) = Methods[req.Method];

				RPCAttribute rpc = method.GetCustomAttribute<RPCAttribute>();
				List<object> parameters = rpc.GetParameters(req, method, req.ParamData);
				object val = await rpc.Invoke(req, method, target, parameters);

				return new RPCResult() { Data = Serializer.Serialize(val) };
			}
			catch (Exception ex)
			{
				Log.Write(ex);
				return new RPCResult(ex);
			}
		}

		public static HttpClient Client;

		public static ulong GuildId;

		public static bool CanManageGuild;

		public static async Task<TResult> Invoke<TResult>(string method, params object[] param)
		{
			if (Client == null)
				throw new Exception("No HttpClient in RPC Service");

			RPCRequest req = new ()
			{
				Method = method,
				Token = Authentication.Token,
				GuildId = GuildId,
			};

			for (int i = 0; i < param.Length; i++)
			{
				req.ParamData.Add(Serializer.Serialize(param[i]));
			}

			var result = await Invoke(req);

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
