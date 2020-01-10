// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using Newtonsoft.Json;

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

				ParameterInfo[] paramInfos = method.GetParameters();

				if (paramInfos.Length != req.ParamData.Count)
					throw new Exception("Incorrect number of parameters");

				List<object> param = new List<object>();
				for (int i = 0; i < paramInfos.Length; i++)
				{
					param.Add(JsonConvert.DeserializeObject(req.ParamData[i], paramInfos[i].ParameterType));
				}

				object val = method.Invoke(target, param.ToArray());

				if (typeof(Task).IsAssignableFrom(method.ReturnType))
				{
					Task task = (Task)val;
					await task;

					if (method.ReturnType.GenericTypeArguments.Length == 1)
					{
						val = method.ReturnType.GetProperty("Result").GetValue(task);
					}
				}

				RPCResult result = new RPCResult();
				result.Data = JsonConvert.SerializeObject(val);
				return result;
			}
			catch (Exception ex)
			{
				return new RPCResult(ex);
			}
		}
	}
}
