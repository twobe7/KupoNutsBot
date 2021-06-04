// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.Text.RegularExpressions;

	[Serializable]
	public class Emote
	{
		public Emote()
		{
		}

		public Emote(string name, string url)
		{
			this.Name = name;
			this.Url = url;
			this.IsStandard = true;
		}

		public Emote(ulong id, string name, string url, bool requiresColon, bool isStandard = true)
		{
			this.Id = id.ToString();
			this.Name = Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
			this.Url = url;
			this.RequiresColon = requiresColon;
			this.IsStandard = isStandard;
		}

		public string Id { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public bool RequiresColon { get; set; }
		public bool IsStandard { get; set; }
		public string Value
		{
			get
			{
				return this.IsStandard ? this.Url : $"<:{this.Name}:{this.Id}>";
			}
		}
	}
}
