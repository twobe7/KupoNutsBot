// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.IO;
	using System.Text.Json;

	[Serializable]
	public class Settings
	{
		public const string Location = "Settings.json";

		public string? Token { get; set; }

		public string? LogChannel { get; set; }

		public string? StatusChannel { get; set; }

		public string? StatusMessage { get; set; }

		public string? CalendarChannel { get; set; }

		public string? CalendarMessage { get; set; }

		public string? CalendarMessage2 { get; set; }

		public string? SundayFundayChannel { get; set; }

		public string? SundayFundayMessage { get; set; }

		public int SundayFundayWeek { get; set; } = 0;

		public string? FashionReportChannel { get; set; }

		public string? DBKey { get; set; }

		public string? DBSecret { get; set; }

		public string? DiscordKey { get; set; }

		public string? DiscordSecret { get; set; }

		public string? TwitterConsumerKey { get; set; }

		public string? TwitterConsumerSecret { get; set; }

		public string? TwitterToken { get; set; }

		public string? TwitterTokenSecret { get; set; }

		public string? XIVAPIKey { get; set; }

		public string? UserLogChannel { get; set; }

		public string? LodestoneChannel { get; set; }

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
				return JsonSerializer.Deserialize<Settings>(json);
			}
		}

		public void Save()
		{
			string json = JsonSerializer.Serialize(this);
			File.WriteAllText(Location, json);
		}
	}
}
