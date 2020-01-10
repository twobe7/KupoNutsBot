// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
