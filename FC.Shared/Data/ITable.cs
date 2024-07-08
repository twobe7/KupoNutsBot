// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public interface ITable
	{
		Task Connect();

		Task<T> CreateEntry<T>(string? id = null)
			where T : EntryBase, new();

		Task<string> GetNewID<T>()
			where T : EntryBase, new();

		Task<T?> Load<T>(string key)
			where T : EntryBase, new();

		Task<T> LoadOrCreate<T>(string key)
			where T : EntryBase, new();

		Task<List<T>> LoadAll<T>(Dictionary<string, object>? conditions = null, int skip = 0, int? take = null)
			where T : EntryBase, new();

		Task Delete<T>(T entry)
			where T : EntryBase, new();

		Task Delete<T>(string key)
			where T : EntryBase, new();

		Task Save<T>(T document)
			where T : EntryBase, new();

		Task<bool> Exists();
	}
}
