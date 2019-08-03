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

	[Serializable]
	public class Database
	{
		public static int Version = 4;
		public static string Location = "data_" + Version + ".json";

		public string Token;
		public int Karma = 0;
		public ulong LogChannel;
		public ulong StatusMessage;
		public ulong StatusChannel;
		public string CalendarChannel;
		public string CalendarMessage;
		public string CalendarMessage2;
		public List<Channel> Channels = new List<Channel>();
		public List<Event> Events = new List<Event>();
		public List<Attendee> Attendees = new List<Attendee>();
		public List<Notification> Notifications = new List<Notification>();

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

		public void Save()
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(Location, json);
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

		public void UpdateOrInsertEvent(Event newEvent)
		{
			for (int i = this.Events.Count - 1; i >= 0; i--)
			{
				if (this.Events[i].Id == newEvent.Id)
				{
					this.Events.RemoveAt(i);
				}
			}

			this.Events.Add(newEvent);
		}

		public void DeleteEvent(string id)
		{
			for (int i = this.Events.Count - 1; i >= 0; i--)
			{
				if (this.Events[i].Id == id)
				{
					this.Events.RemoveAt(i);
				}
			}

			for (int i = this.Notifications.Count - 1; i >= 0; i--)
			{
				if (this.Notifications[i].EventId == id)
				{
					this.Notifications.RemoveAt(i);
				}
			}

			for (int i = this.Attendees.Count - 1; i >= 0; i--)
			{
				if (this.Attendees[i].EventId == id)
				{
					this.Attendees.RemoveAt(i);
				}
			}
		}

		public List<Notification> GetNotifications(string eventId)
		{
			List<Notification> results = new List<Notification>();
			foreach (Notification notify in this.Notifications)
			{
				if (notify.EventId == eventId)
				{
					results.Add(notify);
				}
			}

			return results;
		}

		/// <summary>
		/// Removed any attendees that no longer have a corresponding event.
		/// </summary>
		public void SanatiseAttendees()
		{
			for (int i = this.Attendees.Count - 1; i >= 0; i--)
			{
				if (this.GetEvent(this.Attendees[i].EventId) == null)
				{
					this.Attendees.RemoveAt(i);
				}
			}
		}
	}
}
