// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class Notification
	{
		public string EventId { get; set; }

		public string ChannelId { get; set; }

		public string MessageId { get; set; }
	}
}
