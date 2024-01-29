// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Threading.Tasks;
	using FC.Serialization;

	public class JsonTable : ITable
	{
		public readonly string Name;
		public readonly int Version;
		private bool connected;

		internal JsonTable(string databaseName, int version)
		{
			this.Name = databaseName;
			this.Version = version;
		}

		public string DirectoryPath
		{
			get
			{
				return "Database/" + this.Name + "_" + this.Version + "/";
			}
		}

		public Task Connect()
		{
			if (!Directory.Exists(this.DirectoryPath))
				Directory.CreateDirectory(this.DirectoryPath);

			this.connected = true;
			return Task.CompletedTask;
		}

		public Task<T> CreateEntry<T>(string? id = null)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			if (id == null)
				id = Guid.NewGuid().ToString();

			T entry = Activator.CreateInstance<T>();
			entry.Id = id;
			this.Save(entry);
			return Task.FromResult(entry);
		}

		public Task Delete<T>(T entry)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			return this.Delete<T>(entry.Id);
		}

		public Task Delete<T>(string key)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			string path = this.GetEntryPath(key);

			if (!File.Exists(path))
				return Task.CompletedTask;

			File.Delete(path);
			return Task.CompletedTask;
		}

		public Task<string> GetNewID<T>()
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			return Task.FromResult(Guid.NewGuid().ToString());
		}

		public Task<T?> Load<T>(string key)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			string path = this.GetEntryPath(key);

			if (!File.Exists(path))
				return Task.FromResult<T?>(null);

			string json = File.ReadAllText(path);
			T? entry = Serializer.Deserialize<T>(json);

			return Task.FromResult((T?)entry);
		}

		public Task<List<T>> LoadAll<T>(Dictionary<string, object>? conditions = null)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			List<T> results = new List<T>();

			string[] files = Directory.GetFiles(this.DirectoryPath, "*.json");

			foreach (string path in files)
			{
				string json = File.ReadAllText(path);
				T? entry = Serializer.Deserialize<T>(json);

				if (entry == null)
					continue;

				bool meetsConditions = true;
				if (conditions != null)
				{
					foreach ((string propertyName, object value) in conditions)
					{
						PropertyInfo? info = entry.GetType().GetProperty(propertyName);
						object? val = info?.GetValue(entry);

						if (val != null && !val.Equals(value))
						{
							meetsConditions = false;
							continue;
						}
					}
				}

				if (!meetsConditions)
					continue;

				results.Add(entry);
			}

			return Task.FromResult(results);
		}

		public async Task<T> LoadOrCreate<T>(string key)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			T? result = await this.Load<T>(key);
			if (result == null)
				result = await this.CreateEntry<T>(key);

			return result;
		}

		public Task Save<T>(T document)
			where T : EntryBase, new()
		{
			if (!this.connected)
				throw new Exception("Database not connected.");

			string path = this.GetEntryPath(document.Id);

			document.Updated = DateTime.UtcNow;

			string json = Serializer.Serialize<T>(document);
			File.WriteAllText(path, json);

			return Task.CompletedTask;
		}

		public Task<bool> Exists()
		{
			return Task.FromResult(Directory.Exists(this.DirectoryPath));
		}

		private string GetEntryPath(string key)
		{
			// replace bad characters with "-".
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				key = key.Replace(c, '-');
			}

			return this.DirectoryPath + key + ".json";
		}
	}
}
