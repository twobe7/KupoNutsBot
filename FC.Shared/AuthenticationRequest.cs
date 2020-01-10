// This document is intended for use by Kupo Nut Brigade developers.

namespace FC
{
	using System;

	[Serializable]
	public class AuthenticationRequest
	{
		public string? DiscordCode { get; set; }

		public string? Token { get; set; }

		public string? Message { get; set; }

		public string? URL { get; set; }
	}
}
