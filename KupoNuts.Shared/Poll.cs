// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using Amazon.DynamoDBv2.DataModel;
	using NodaTime;
	using NodaTime.Text;

	[Serializable]
	public class Poll : EntryBase
	{
		public ulong MessageId { get; set; } = 0;

		public ulong ChannelId { get; set; } = 0;

		public string Comment { get; set; } = string.Empty;

		public bool Anon { get; set; } = false;

		public string Closes { get; set; } = string.Empty;

		public List<Option> Options { get; set; } = new List<Option>();

		[DynamoDBIgnore]
		public Instant ClosesInstant
		{
			get
			{
				return InstantPattern.General.Parse(this.Closes).Value;
			}

			set
			{
				this.Closes = InstantPattern.General.Format(value);
			}
		}

		public int CountTotalVotes()
		{
			int total = 0;
			foreach (Option op in this.Options)
			{
				total += op.Votes.Count;
			}

			return total;
		}

		[Serializable]
		public class Option
		{
			public Option()
			{
			}

			public Option(string text)
			{
				this.Text = text;
			}

			public string Text { get; set; } = string.Empty;

			public List<ulong> Votes { get; set; } = new List<ulong>();
		}
	}
}
