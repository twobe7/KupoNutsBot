// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public class Table<T>
		where T : EntryBase, new()
	{
		private readonly ITable table;

		public Table(string tableName, int version)
		{
			this.table = TableService.Create(tableName, version);
		}

		public Task Connect()
		{
			return this.table.Connect();
		}

		public Task<T> CreateEntry(string? id = null)
		{
			return this.table.CreateEntry<T>(id);
		}

		public Task Delete(T entry)
		{
			return this.table.Delete(entry);
		}

		public Task Delete(string key)
		{
			return this.table.Delete<T>(key);
		}

		public Task<string> GetNewID()
		{
			return this.table.GetNewID();
		}

		public Task<T?> Load(string key)
		{
			return this.table.Load<T>(key);
		}

		public Task<List<T>> LoadAll(Dictionary<string, object>? conditions = null)
		{
			return this.table.LoadAll<T>(conditions);
		}

		public Task<T> LoadOrCreate(string key)
		{
			return this.table.LoadOrCreate<T>(key);
		}

		public Task Save(T document)
		{
			return this.table.Save(document);
		}
	}
}
