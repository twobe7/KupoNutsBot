// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using FC.Manager.Server.RPC;
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

				RPCResult result = new RPCResult();
				result.Data = Serializer.Serialize(val);
				return result;
			}
			catch (Exception ex)
			{
				Log.Write(ex);
				return new RPCResult(ex);
			}
		}
	}
}
