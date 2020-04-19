// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.RPC
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Manager.Client.RPC;
	using FC.Serialization;

	public class RPCAttribute : Attribute
	{
		public virtual List<object> GetParameters(RPCRequest request, MethodInfo method, List<string> paramData)
		{
			ParameterInfo[] paramInfos = method.GetParameters();

			if (paramInfos.Length != request.ParamData.Count)
				throw new Exception("Incorrect number of parameters, expected " + paramInfos.Length + ", got " + paramData.Count);

			List<object> paramValues = new List<object>();
			for (int i = 0; i < paramInfos.Length; i++)
			{
				paramValues.Add(Serializer.Deserialize(request.ParamData[i], paramInfos[i].ParameterType));
			}

			return paramValues;
		}

		public virtual async Task<object> Invoke(RPCRequest request, MethodInfo method, object target, List<object> param)
		{
			object val = method.Invoke(target, param.ToArray());

			if (typeof(Task).IsAssignableFrom(method.ReturnType))
			{
				Task task = (Task)val;
				await task;

				if (method.ReturnType.GenericTypeArguments.Length == 1)
				{
					val = method.ReturnType.GetProperty("Result").GetValue(task);
					return val;
				}

				return null;
			}

			return val;
		}

		public virtual bool Authenticate(RPCRequest request)
		{
			return true;
		}
	}
}
