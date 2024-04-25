// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;
	using Blazored.Modal;
	using Blazored.Modal.Services;
	using Microsoft.AspNetCore.Components;

	public class Modal : ComponentBase
	{
		public static IModalService ModalService;
		public static IModalReference ModalReference;

		private static object[] parameters;
		private static object lastResult;

		public static async Task<TReturn> Show<TReturn, TComponent>(string name, params object[] param)
			where TComponent : Modal
		{
			lastResult = null;
			parameters = param;
			ModalReference = ModalService.Show<TComponent>(name);

			ModalResult result = null;
			////ModalService.OnClose += (ModalResult res) =>
			////{
			////	result = res;
			////};

			while (result == null)
				await Task.Delay(500);

			if (lastResult is TReturn returnData)
			{
				return returnData;
			}
			else
			{
				return default(TReturn);
			}
		}

		public static async Task Show<TComponent>(string name, params object[] param)
			where TComponent : ComponentBase
		{
			lastResult = null;
			parameters = param;

			ModalParameters par = new ModalParameters();
			ModalReference = ModalService.Show<TComponent>(name, par);

			ModalResult result = null;
			////ModalService.OnClose += (ModalResult res) =>
			////{
			////	result = res;
			////};

			while (result == null)
			{
				await Task.Delay(500);
			}
		}

		public void Close(object result = null)
		{
			lastResult = result;
			ModalReference.Close(ModalResult.Cancel());
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			try
			{
				Type t = this.GetType();
				MethodInfo methodInfo = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);

				if (methodInfo == null)
					throw new Exception("Failed to locate Initialize method on modal component: \"" + t + "\"");

				ParameterInfo[] param = methodInfo.GetParameters();

				if (parameters == null && param.Length != 0)
				{
					throw new Exception("Incorrect parameter count, expected 0, got " + param.Length);
				}
				else
				{
					if (param.Length != parameters.Length)
					{
						throw new Exception("Incorrect parameter count, expected " + parameters.Length + ", got " + param.Length);
					}
				}

				Task task = methodInfo.Invoke(this, parameters) as Task;

				if (task != null)
				{
					await task;
				}
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}
