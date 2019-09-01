// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;

	[Serializable]
	public class Channel
	{
		public Channel()
		{
		}

		public Channel(ulong id, string name, Types type)
		{
			this.Id = id.ToString();
			this.Name = name;
			this.Type = type;
		}

		public enum Types
		{
			Unknown,
			Text,
			Voice,
		}

		public string? Id { get; set; }

		public string? Name { get; set; }

		public Types Type { get; set; }
	}
}
