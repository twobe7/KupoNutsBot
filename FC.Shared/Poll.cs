// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.Collections.Generic;
	using Amazon.DynamoDBv2.DataModel;
	using FC.Utils;
	using NodaTime;
	using NodaTime.Text;

	[Serializable]
	public class Poll : EntryBase
	{
		public ulong MessageId { get; set; } = 0;

		public ulong ChannelId { get; set; } = 0;

		public ulong Author { get; set; } = 0;

		public string Comment { get; set; } = string.Empty;

		public bool Anon { get; set; } = false;

		public string Closes { get; set; } = string.Empty;

		public List<Option> Options { get; set; } = new List<Option>();

		[DynamoDBIgnore] // Ignore when using DynamoDB
		[System.Text.Json.Serialization.JsonIgnore] // Ignore when using Json DB
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

		public bool Closed()
		{
			Duration timeTillClosed = this.ClosesInstant - TimeUtils.Now;
			return timeTillClosed.TotalSeconds <= 0;
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
