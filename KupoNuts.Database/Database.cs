// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using KupoNuts.Events;
	using Newtonsoft.Json;

	[Serializable]
	public class Database
	{
		public const string Location = "data.json";

		public string Token;
		public int Karma = 0;

		public ulong LogChannel;
		public ulong StatusMessage;

		// Used to notify when the update process is finished
		public ulong StatusChannel;

		public List<Event> Events = new List<Event>();

		private static Database instance;

		public static Database Instance
		{
			get
			{
				////if (instance == null)
				Load();

				return instance;
			}
		}

		public static void Save()
		{
			if (instance == null)
				throw new Exception("Database not loaded");

			string json = JsonConvert.SerializeObject(instance, Formatting.Indented);
			File.WriteAllText(Location, json);
		}

		public static Event GetEvent(string id)
		{
			if (Instance == null)
				throw new Exception("Database not loaded");

			foreach (Event evt in Instance.Events)
			{
				if (evt.Id == id)
				{
					return evt;
				}
			}

			return null;
		}

		public static void UpdateOrInsert(Event evt)
		{
			if (Instance == null)
				throw new Exception("Database not loaded");

			Event evt2 = GetEvent(evt.Id);

			if (evt2 != null)
				Instance.Events.Remove(evt2);

			Instance.Events.Add(evt);
		}

		private static void Load()
		{
			if (!File.Exists(Location))
			{
				instance = new Database();
				Save();
			}
			else
			{
				string json = File.ReadAllText(Location);
				instance = JsonConvert.DeserializeObject<Database>(json);
				Save();
			}
		}
	}
}
