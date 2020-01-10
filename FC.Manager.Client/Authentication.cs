// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Net.Http;
	using Microsoft.AspNetCore.Components;

	public static class Authentication
	{
		public static string Token;

		public static bool IsAuthenticated
		{
			get
			{
				return !string.IsNullOrEmpty(Token);
			}
		}

		public static bool Initialize(HttpClient http, NavigationManager navigation)
		{
			if (!IsAuthenticated)
			{
				navigation.NavigateTo("/");
				return false;
			}

			http.DefaultRequestHeaders.Remove("Token");
			http.DefaultRequestHeaders.Add("Token", Authentication.Token);

			return true;
		}
	}
}
