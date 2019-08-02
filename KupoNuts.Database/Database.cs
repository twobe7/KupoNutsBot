// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using KupoNuts.Events;
	using Newtonsoft.Json;
	using NodaTime;
	using NodaTime.Serialization.JsonNet;

	// This is a terrible database, it just stores data in a json file, and re-loads it every time
	// the instance is accessed. It has 0 support for concurrency or merging changes.
	// We should strongly consider a real database solution like Amazon Dynamo DB

	// May god have mercy on my soul for using this live.
	[Serializable]
	public class Database
	{
		public static int Version = 2;
		public static string Location = "data_" + Version + ".json";

		public string Token;
		public int Karma = 0;

		public ulong LogChannel;
		public ulong StatusMessage;

		// Used to notify when the update process is finished
		public ulong StatusChannel;

		public List<Event> Events = new List<Event>();

		public static void Init()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
			JsonConvert.DefaultSettings = () => settings;
		}

		public static Database Load()
		{
			Init();

			if (!File.Exists(Location))
			{
				Database instance = new Database();
				instance.Save();
				return instance;
			}
			else
			{
				string json = File.ReadAllText(Location);
				return JsonConvert.DeserializeObject<Database>(json);
			}
		}

		public static void UpdateOrInsert(Event evt)
		{
			Database instance = Load();

			Event evt2 = instance.GetEvent(evt.Id);

			if (evt2 != null)
			{
				if (!instance.Events.Remove(evt2))
					throw new Exception("Failed remove event");
			}

			instance.Events.Add(evt);
			instance.Save();
		}

		public static void Delete(Event evt)
		{
			Database instance = Load();

			Event evt2 = instance.GetEvent(evt.Id);
			if (evt2 != null)
			{
				if (!instance.Events.Remove(evt2))
				{
					throw new Exception("Failed remove event");
				}
			}

			instance.Save();
		}

		public Event GetEvent(string id)
		{
			foreach (Event evt in this.Events)
			{
				if (evt.Id == id)
				{
					return evt;
				}
			}

			return null;
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(Location, json);
		}
	}
}
