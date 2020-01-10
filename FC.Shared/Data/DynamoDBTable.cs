// This document is intended for use by Kupo Nut Brigade developers.

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

	public class DynamoDBTable<T> : Table<T>
		where T : EntryBase, new()
	{
		private AmazonDynamoDBClient? client;
		private DynamoDBContext? context;
		private DynamoDBOperationConfig? operationConfig;

		internal DynamoDBTable(string databaseName, int version)
			: base(databaseName, version)
		{
		}

		public override async Task Connect()
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

		public override async Task<T> CreateEntry(string? id = null)
		{
			if (id == null)
				id = await this.GetNewID();

			T t = new T();
			t.Id = id;

			return t;
		}

		public override async Task<string> GetNewID()
		{
			bool valid = false;
			string guid;

			do
			{
				guid = Guid.NewGuid().ToString();
				valid = await this.Load(guid) == null;
			}
			while (!valid);

			return guid;
		}

		public override async Task<T?> Load(string key)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			return await this.context.LoadAsync<T>(key, this.operationConfig);
		}

		public override async Task<T> LoadOrCreate(string key)
		{
			T? entry = await this.Load(key);

			if (entry is null)
				entry = await this.CreateEntry(key);

			return entry;
		}

		public override async Task<List<T>> LoadAll(Dictionary<string, object>? conditions = null)
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

		public override async Task Delete(T entry)
		{
			if (entry.Id == null)
				return;

			await this.Delete(entry.Id);
		}

		public override async Task Delete(string key)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			await this.context.DeleteAsync<T>(key, this.operationConfig);
		}

		public override Task Save(T document)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			document.Updated = DateTime.UtcNow;
			return this.context.SaveAsync(document, this.operationConfig);
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
