// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace System.Collections.Generic
{
	public static class DictionaryExtensions
	{
		public static (TKey, TValue) Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self)
		{
			return (self.Key, self.Value);
		}

		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
		{
			key = self.Key;
			value = self.Value;
		}

		/// <summary>
		/// Attempts to update given key in dictionary to given value. Adds key/value if key doesn't exist.
		/// </summary>
		/// <typeparam name="TKey">Type of in Dictionary.</typeparam>
		/// <typeparam name="TValue">Type of Value in Dictionary.</typeparam>
		/// <param name="self">Current Dict object.</param>
		/// <param name="key">Key to find within Dictionary.</param>
		/// <param name="value">Value to update within Dictionary.</param>
		public static void UpdateOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue value)
		{
			if (!self.ContainsKey(key))
				self.Add(key, value);
			else
				self[key] = value;
		}
	}
}