// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using KupoNutsBot.Events;
	using Newtonsoft.Json;

	[Serializable]
	public class Database
	{
		public const string Location = "data.json";

		public string Token;

		public ulong LogChannel;
		public ulong StatusMessage;

		public List<Event> Events = new List<Event>();

		public static Database Instance
		{
			get;
			private set;
		}

		public static void Load()
		{
			if (!File.Exists(Location))
			{
				Instance = new Database();
				Instance.Save();
			}
			else
			{
				string json = File.ReadAllText(Location);
				Instance = JsonConvert.DeserializeObject<Database>(json);
			}
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(Location, json);
		}
	}
}
