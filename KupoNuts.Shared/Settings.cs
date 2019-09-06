// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.IO;
	using Newtonsoft.Json;

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

		public string? DBKey { get; set; }

		public string? DBSecret { get; set; }

		public string? UserLogChannel { get; set; }

		public static void Init()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			JsonConvert.DefaultSettings = () => settings;
		}

		public static Settings Load()
		{
			Init();

			if (!File.Exists(Location))
			{
				Settings settings = new Settings();
				settings.Save();
				return settings;
			}
			else
			{
				string json = File.ReadAllText(Location);
				return JsonConvert.DeserializeObject<Settings>(json);
			}
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(Location, json);
		}
	}
}
