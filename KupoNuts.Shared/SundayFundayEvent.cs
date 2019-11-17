// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class SundayFundayEvent : EntryBase
	{
		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public List<Vote> Votes { get; set; } = new List<Vote>();

		public int CurrentWeek { get; set; } = -1;

		public int CountVotes(int currentWeek)
		{
			int count = 0;

			foreach (Vote vote in this.Votes)
			{
				if (vote.Week != currentWeek)
					continue;

				count++;
			}

			return count;
		}

		public class Vote
		{
			public string UserId { get; set; } = string.Empty;

			public int Week { get; set; } = -1;
		}
	}
}
