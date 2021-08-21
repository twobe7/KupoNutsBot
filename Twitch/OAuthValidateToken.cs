// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitch
{
	using System;

	[Serializable]
	public class OAuthValidateToken
	{
		public string? ClientId { get; set; }
		public string? Login { get; set; }
		public string[]? Scope { get; set; }
		public string? UserId { get; set; }
		public int? ExpiresIn { get; set; }
	}
}
