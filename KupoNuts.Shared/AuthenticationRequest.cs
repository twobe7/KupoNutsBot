// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public class AuthenticationRequest
	{
		public string? DiscordCode { get; set; }

		public string? Token { get; set; }
	}
}
