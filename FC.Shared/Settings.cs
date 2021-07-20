// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.IO;
	using FC.Serialization;

	[Serializable]
	public class Settings
	{
		public const string Location = "Settings.json";

		public string? Token { get; set; }

		public string? LogChannel { get; set; }

		public string? StatusChannel { get; set; }

		public string? StatusMessage { get; set; }

		public bool UseDynamoDb { get; set; } = false;

		// only needed when running outside of an amazon EC2 instance, and if UseDynamoDB is true.
		public string? DBKey { get; set; }

		// only needed when running outside of an amazon EC2 instance, and if UseDynamoDB is true.
		public string? DBSecret { get; set; }

		public string? DiscordKey { get; set; }

		public string? DiscordSecret { get; set; }

		public string? TenorKey { get; set; }

		public string? TwitterConsumerKey { get; set; }

		public string? TwitterConsumerSecret { get; set; }

		public string? TwitterToken { get; set; }

		public string? TwitterTokenSecret { get; set; }

		public string? XIVAPIKey { get; set; }

		public string? BotDiscordServer { get; set; }

		public string? BotLogExceptionsChannel { get; set; }

		public string? TwitchKey { get; set; }
		public string? TwitchSecret { get; set; }

		public string? YouTubeKey { get; set; }

		public static Settings Load()
		{
			if (!File.Exists(Location))
			{
				Settings settings = new Settings();
				settings.Save();
				return settings;
			}
			else
			{
				string json = File.ReadAllText(Location);
				return Serializer.Deserialize<Settings>(json);
			}
		}

		public void Save()
		{
			string json = Serializer.Serialize(this);
			File.WriteAllText(Location, json);
		}
	}
}
