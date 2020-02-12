// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Waiter! check please.
	/// </summary>
	public static class TableService
	{
		public static ITable Create(string tableName, int version)
		{
			if (Settings.Load().UseDynamoDb)
			{
				return new DynamoDBTable(tableName, version);
			}
			else
			{
				return new JsonTable(tableName, version);
			}
		}
	}
}
