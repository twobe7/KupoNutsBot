// This document is intended for use by Kupo Nut Brigade developers.

namespace FC
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class AuthenticationResult
	{
		public string Token { get; set; } = string.Empty;

		public List<Guild>? Guilds { get; set; }

		[Serializable]
		public class Guild
		{
			public string Id { get; set; } = string.Empty;

			public string Name { get; set; } = string.Empty;
		}
	}
}
