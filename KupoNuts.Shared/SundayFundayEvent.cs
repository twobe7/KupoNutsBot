// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class SundayFundayEvent : EntryBase
	{
		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public List<Vote> Votes { get; set; } = new List<Vote>();

		public int CurrentWeek { get; set; } = -1;

		public bool HasVoted(string userId)
		{
			foreach (Vote vote in this.Votes)
			{
				if (vote.UserId == userId)
				{
					return true;
				}
			}

			return false;
		}

		public void AddVote(string userId)
		{
			Vote vote = new Vote();
			vote.UserId = userId;
			this.Votes.Add(vote);
		}

		public bool RemoveVote(string userId)
		{
			for (int i = this.Votes.Count - 1; i >= 0; i--)
			{
				if (this.Votes[i].UserId == userId)
				{
					this.Votes.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		[Serializable]
		public class Vote
		{
			public string UserId { get; set; } = string.Empty;
		}
	}
}
