﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client
{
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using Microsoft.AspNetCore.Components;

	public static class NavigationManagerExtensions
	{
		public static string GetQueryParameter(this NavigationManager self, string parmKey)
		{
			Uri uri = new Uri(self.Uri);

			if (!string.IsNullOrEmpty(uri.Query))
			{
				NameValueCollection queryDictionary = HttpUtility.ParseQueryString(uri.Query);
				string[] values = queryDictionary.GetValues(parmKey);

				if (values.Length > 0)
				{
					return values[0];
				}
			}

			return null;
		}

		public static string GetURL(this NavigationManager self)
		{
			Uri uri = new Uri(self.Uri);
			return string.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.AbsolutePath);
		}
	}
}
