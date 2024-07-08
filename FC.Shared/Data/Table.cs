// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public class Table : ITable
	{
		private readonly ITable table;

		public Table(string tableName, int version)
		{
			this.table = TableService.Create(tableName, version);
		}

		public Task<bool> Exists()
		{
			return this.table.Exists();
		}

		public Task Connect()
		{
			return this.table.Connect();
		}

		public Task<T> CreateEntry<T>(string? id = null)
			where T : EntryBase, new()
		{
			return this.table.CreateEntry<T>(id);
		}

		public Task Delete<T>(T entry)
			where T : EntryBase, new()
		{
			return this.table.Delete<T>(entry);
		}

		public Task Delete<T>(string key)
			where T : EntryBase, new()
		{
			return this.table.Delete<T>(key);
		}

		public Task<string> GetNewID<T>()
			where T : EntryBase, new()
		{
			return this.table.GetNewID<T>();
		}

		public Task<T?> Load<T>(string key)
			where T : EntryBase, new()
		{
			return this.table.Load<T>(key);
		}

		public Task<List<T>> LoadAll<T>(Dictionary<string, object>? conditions = null, int skip = 0, int? take = null)
			where T : EntryBase, new()
		{
			return this.table.LoadAll<T>(conditions);
		}

		public Task<T> LoadOrCreate<T>(string key)
			where T : EntryBase, new()
		{
			return this.table.LoadOrCreate<T>(key);
		}

		public Task Save<T>(T document)
			where T : EntryBase, new()
		{
			return this.table.Save<T>(document);
		}
	}
}
