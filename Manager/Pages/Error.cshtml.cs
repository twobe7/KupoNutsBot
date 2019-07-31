// This document is intended for use by Kupo Nut Brigade developers.

namespace Manager.Pages
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.RazorPages;

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public class ErrorModel : PageModel
	{
		public string RequestId { get; set; }

		public bool ShowRequestId
		{
			get
			{
				return !string.IsNullOrEmpty(this.RequestId);
			}
		}

		public void OnGet()
		{
			this.RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
		}
	}
}
