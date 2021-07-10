// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Twitch
{
	using System;

	[Serializable]
	public class OAuthToken
	{
		public string? AccessToken { get; set; }
		public string? RefreshToken { get; set; }
		public int? ExpiresIn { get; set; }
		public string? Scope { get; set; }
		public string? TokenType { get; set; }
	}
}
