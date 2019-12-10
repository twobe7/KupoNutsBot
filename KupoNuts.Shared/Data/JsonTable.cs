// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Data
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Text.Json;
	using System.Threading.Tasks;

	public class JsonTable<T> : Table<T>
		where T : EntryBase, new()
	{
		internal JsonTable(string databaseName, int version)
			: base(databaseName, version)
		{
		}

		public string DirectoryPath
		{
			get
			{
				return "Database/" + this.InternalName + "/";
			}
		}

		public override Task Connect()
		{
			if (!Directory.Exists(this.DirectoryPath))
				Directory.CreateDirectory(this.DirectoryPath);

			return Task.CompletedTask;
		}

		public override Task<T> CreateEntry(string? id = null)
		{
			if (id == null)
				id = Guid.NewGuid().ToString();

			T entry = Activator.CreateInstance<T>();
			entry.Id = id;
			this.Save(entry);
			return Task.FromResult(entry);
		}

		public override Task Delete(T entry)
		{
			return this.Delete(entry.Id);
		}

		public override Task Delete(string key)
		{
			string path = this.GetEntryPath(key);

			if (!File.Exists(path))
				return Task.CompletedTask;

			File.Delete(path);
			return Task.CompletedTask;
		}

		public override Task<string> GetNewID()
		{
			return Task.FromResult(Guid.NewGuid().ToString());
		}

		public override Task<T?> Load(string key)
		{
			string path = this.GetEntryPath(key);

			if (!File.Exists(path))
				return Task.FromResult<T?>(null);

			string json = File.ReadAllText(path);
			T? entry = JsonSerializer.Deserialize<T>(json);

			return Task.FromResult((T?)entry);
		}

		public override Task<List<T>> LoadAll(Dictionary<string, object>? conditions = null)
		{
			List<T> results = new List<T>();

			string[] files = Directory.GetFiles(this.DirectoryPath, "*.json");

			foreach (string path in files)
			{
				string json = File.ReadAllText(path);
				T entry = JsonSerializer.Deserialize<T>(json);

				bool meetsConditions = true;
				if (conditions != null)
				{
					foreach ((string propertyName, object value) in conditions)
					{
						PropertyInfo info = entry.GetType().GetProperty(propertyName);
						object val = info.GetValue(entry);

						if (!val.Equals(value))
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

		public override async Task<T> LoadOrCreate(string key)
		{
			T? result = await this.Load(key);
			if (result == null)
				result = await this.CreateEntry(key);

			return result;
		}

		public override Task Save(T entry)
		{
			string path = this.GetEntryPath(entry.Id);

			string json = JsonSerializer.Serialize(entry);
			File.WriteAllText(path, json);

			return Task.CompletedTask;
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
