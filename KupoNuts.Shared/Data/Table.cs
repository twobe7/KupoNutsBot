// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Data
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public abstract class Table<T>
		where T : EntryBase, new()
	{
		public readonly string Name;
		public readonly int Version;

		protected Table(string databaseName, int version)
		{
			this.Name = databaseName;
			this.Version = version;
		}

		public string InternalName
		{
			get
			{
				return Settings.Load().DatabasePrefix + "_" + this.Name + "_" + this.Version;
			}
		}

		public static Table<T> Create(string databaseName, int version)
		{
			if (Settings.Load().UseDynamoDb)
			{
				return new DynamoDBTable<T>(databaseName, version);
			}
			else
			{
				return new JsonTable<T>(databaseName, version);
			}
		}

		public abstract Task Connect();

		public abstract Task<T> CreateEntry(string? id = null);

		public abstract Task<string> GetNewID();

		public abstract Task<T?> Load(string key);

		public abstract Task<T> LoadOrCreate(string key);

		public abstract Task<List<T>> LoadAll(Dictionary<string, object>? conditions = null);

		public abstract Task Delete(T entry);

		public abstract Task Delete(string key);

		public abstract Task Save(T document);
	}
}
