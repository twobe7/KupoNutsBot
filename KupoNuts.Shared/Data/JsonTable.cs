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
		private Dictionary<string, T> data = new Dictionary<string, T>();

		internal JsonTable(string databaseName, int version)
			: base(databaseName, version)
		{
		}

		public string Path
		{
			get
			{
				return "Database/" + this.InternalName + ".json";
			}
		}

		public override Task Connect()
		{
			if (File.Exists(this.Path))
			{
				string json = File.ReadAllText(this.Path);
				TableData td = JsonSerializer.Deserialize<TableData>(json);
				this.data = td.Data;
			}

			return Task.CompletedTask;
		}

		public override Task<T> CreateEntry(string? id = null)
		{
			if (id == null)
				id = Guid.NewGuid().ToString();

			T entry = Activator.CreateInstance<T>();
			entry.Id = id;
			this.data.Add(id, entry);
			this.Save();
			return Task.FromResult(entry);
		}

		public override Task Delete(T entry)
		{
			return this.Delete(entry.Id);
		}

		public override Task Delete(string key)
		{
			if (!this.data.ContainsKey(key))
				return Task.CompletedTask;

			this.data.Remove(key);
			this.Save();
			return Task.CompletedTask;
		}

		public override Task<string> GetNewID()
		{
			return Task.FromResult(Guid.NewGuid().ToString());
		}

		public override Task<T?> Load(string key)
		{
			if (!this.data.ContainsKey(key))
				return Task.FromResult<T?>(null);

			return Task.FromResult((T?)this.data[key]);
		}

		public override Task<List<T>> LoadAll(Dictionary<string, object>? conditions = null)
		{
			List<T> results = new List<T>();

			foreach ((string key, T entry) in this.data)
			{
				if (conditions != null)
				{
					foreach ((string propertyName, object value) in conditions)
					{
						PropertyInfo info = entry.GetType().GetProperty(propertyName);
						object val = info.GetValue(entry);

						if (val != value)
						{
							continue;
						}
					}
				}

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

		public override Task Save(T document)
		{
			if (!this.data.ContainsKey(document.Id))
				this.data.Add(document.Id, document);

			this.data[document.Id] = document;

			this.Save();
			return Task.CompletedTask;
		}

		private void Save()
		{
			string dir = System.IO.Path.GetDirectoryName(this.Path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			TableData td = new TableData();
			td.Data = this.data;
			string json = JsonSerializer.Serialize(td);
			File.WriteAllText(this.Path, json);
		}

		[Serializable]
		private class TableData
		{
			public Dictionary<string, T> Data { get; set; } = new Dictionary<string, T>();
		}
	}
}
