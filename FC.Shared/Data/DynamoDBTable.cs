// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Data
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Amazon;
	using Amazon.DynamoDBv2;
	using Amazon.DynamoDBv2.DataModel;
	using Amazon.DynamoDBv2.DocumentModel;
	using Amazon.DynamoDBv2.Model;
	using Amazon.Runtime.CredentialManagement;

	public class DynamoDBTable : ITable
	{
		public readonly string Name;
		public readonly int Version;

		private AmazonDynamoDBClient? client;
		private DynamoDBContext? context;
		private DynamoDBOperationConfig? operationConfig;

		internal DynamoDBTable(string databaseName, int version)
		{
			this.Name = databaseName;
			this.Version = version;
		}

		private string InternalName
		{
			get
			{
				return this.Name + "_" + this.Version;
			}
		}

		public async Task Connect()
		{
			CredentialProfileOptions options = new CredentialProfileOptions();

			Settings settings = Settings.Load();
			options.AccessKey = settings.DBKey;
			options.SecretKey = settings.DBSecret;

			CredentialProfile profile = new CredentialProfile("Default", options);
			profile.Region = RegionEndpoint.APSoutheast2;

			SharedCredentialsFile sharedFile = new SharedCredentialsFile();
			sharedFile.RegisterProfile(profile);

			AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();

			this.client = new AmazonDynamoDBClient(dbConfig);

			await this.EnsureTable();

			this.context = new DynamoDBContext(this.client);

			try
			{
				await this.client.ListTablesAsync();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to connect to database: " + ex.Message, ex);
			}

			this.operationConfig = new DynamoDBOperationConfig();
			this.operationConfig.OverrideTableName = this.InternalName;
		}

		public async Task<T> CreateEntry<T>(string? id = null)
			where T : EntryBase, new()
		{
			if (id == null)
				id = await this.GetNewID();

			T t = new T();
			t.Id = id;

			return t;
		}

		public async Task<string> GetNewID()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			bool valid = false;
			string guid;

			do
			{
				guid = Guid.NewGuid().ToString();

				valid = await this.context.LoadAsync<EntryBase>(guid, this.operationConfig) == null;
			}
			while (!valid);

			return guid;
		}

		public async Task<T?> Load<T>(string key)
			where T : EntryBase, new()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			return await this.context.LoadAsync<T>(key, this.operationConfig);
		}

		public async Task<T> LoadOrCreate<T>(string key)
			where T : EntryBase, new()
		{
			T? entry = await this.Load<T>(key);

			if (entry is null)
				entry = await this.CreateEntry<T>(key);

			return entry;
		}

		public async Task<List<T>> LoadAll<T>(Dictionary<string, object>? conditions = null)
			where T : EntryBase, new()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			try
			{
				AsyncSearch<T> search = this.context.ScanAsync<T>(ToScanConditions(conditions), this.operationConfig);
				return await search.GetRemainingAsync();
			}
			catch (ResourceNotFoundException)
			{
				throw new Exception("Database table not found. This may be caused by new tables not propagating immediately.");
			}
		}

		public async Task Delete<T>(T entry)
			where T : EntryBase, new()
		{
			if (entry.Id == null)
				return;

			await this.Delete<T>(entry.Id);
		}

		public async Task Delete<T>(string key)
			where T : EntryBase, new()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			await this.context.DeleteAsync<T>(key, this.operationConfig);
		}

		public Task Save<T>(T document)
			where T : EntryBase, new()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			document.Updated = DateTime.UtcNow;
			return this.context.SaveAsync<T>(document, this.operationConfig);
		}

		private static List<ScanCondition> ToScanConditions(Dictionary<string, object>? conditions)
		{
			List<ScanCondition> scanConditions = new List<ScanCondition>();

			if (conditions == null)
				return scanConditions;

			foreach ((string propertyName, object value) in conditions)
			{
				scanConditions.Add(new ScanCondition(propertyName, ScanOperator.Equal, value));
			}

			return scanConditions;
		}

		private async Task EnsureTable()
		{
			if (this.client == null)
				throw new Exception("Database is not connected");

			ListTablesResponse listTablesResponse = await this.client.ListTablesAsync();

			if (listTablesResponse.TableNames.Contains(this.InternalName))
				return;

			CreateTableRequest request = new CreateTableRequest
			{
				TableName = this.InternalName,
				AttributeDefinitions = new List<AttributeDefinition>()
				{
					new AttributeDefinition("Id", ScalarAttributeType.S),
				},
				KeySchema = new List<KeySchemaElement>()
				{
					new KeySchemaElement("Id", KeyType.HASH),
				},
				ProvisionedThroughput = new ProvisionedThroughput(1, 1),
			};

			try
			{
				CreateTableResponse createTableResponse = await this.client.CreateTableAsync(request);
				Log.Write("Created the Table: \"" + this.InternalName + "\"", "Database");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create table: \"" + this.InternalName + "\"", ex);
			}
		}
	}
}
