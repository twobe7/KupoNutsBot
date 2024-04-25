// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web
{
	using System;
	using System.Reflection;
	using System.Threading.Tasks;
	using Blazored.Modal;
	using Blazored.Modal.Services;
	using Microsoft.AspNetCore.Components;

	public class Modal : ComponentBase
	{
		////public Modal(IModalService modalService)
		////{
		////	ModalService = modalService;
		////}
		public IModalReference ModalReference;

		private object[]? parameters;
		private object? lastResult;

		[Inject]
		[CascadingParameter]
		public IModalService ModalService { get; set; } = default!;

		[Parameter]
		public object? obj { get; set; }

		public async Task<TReturn?> Show<TReturn, TComponent>(string name, params object[] param)
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

			return lastResult is TReturn returnData
				? returnData
				: default;
		}

		public async Task Show<TComponent>(string name, params object[] param)
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

			//try
			//{
			//	Type? t = this.GetType();
			//	MethodInfo? methodInfo = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance)
			//		?? throw new Exception("Failed to locate Initialize method on modal component: \"" + t + "\"");

			//	ParameterInfo[] param = methodInfo.GetParameters();

			//	if (parameters == null && param.Length != 0)
			//	{
			//		throw new Exception("Incorrect parameter count, expected 0, got " + param.Length);
			//	}
			//	else
			//	{
			//		if (param.Length != parameters?.Length)
			//		{
			//			throw new Exception("Incorrect parameter count, expected " + parameters?.Length + ", got " + param.Length);
			//		}
			//	}

			//	if (methodInfo.Invoke(this, parameters) is Task task)
			//	{
			//		await task;
			//	}
			//}
			//catch (TargetInvocationException ex)
			//{
			//	throw ex.InnerException;
			//}
		}
	}
}
