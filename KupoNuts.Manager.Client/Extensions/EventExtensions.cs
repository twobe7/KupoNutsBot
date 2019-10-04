// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using KupoNuts.Manager.Client;
	using KupoNuts.Utils;
	using NodaTime;
	using NodaTime.Text;

	public static class EventExtensions
	{
		public static string GetNextOccuranceString(this Event self)
		{
			Event.Occurance occurance = self.GetNextOccurance();
			if (occurance == null)
				return "Never";

			return occurance.GetInstant().GetDisplayString();
		}

		public static double GetNotifyDurationDouble(this Event self)
		{
			if (string.IsNullOrEmpty(self.NotifyDurationStr))
				return -1;

			Duration? dur = self.NotifyDuration;
			if (dur == null)
				return -1;

			return (dur.Value.Days * 24) + dur.Value.Hours + (dur.Value.Minutes / 60.0);
		}

		public static void SetNotifyDuration(this Event self, double duration)
		{
			if (duration < 0)
			{
				self.NotifyDuration = null;
				return;
			}

			int hours = (int)duration;
			int minutes = (int)((duration - (double)hours) * 60.0);

			self.NotifyDuration = Duration.FromMinutes((hours * 60) + minutes);
		}

		public static string GetChannelName(this Event self, List<Channel> channels)
		{
			if (string.IsNullOrEmpty(self.ChannelId))
				return null;

			foreach (Channel channel in channels)
			{
				if (channel.DiscordId == self.ChannelId)
				{
					return channel.Name;
				}
			}

			return "Unknown";
		}
	}
}
