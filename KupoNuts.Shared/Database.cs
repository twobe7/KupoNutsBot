// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Amazon;
	using Amazon.DynamoDBv2;
	using Amazon.DynamoDBv2.DataModel;
	using Amazon.DynamoDBv2.Model;
	using Amazon.Runtime.CredentialManagement;

	public class Database<T>
		where T : EntryBase, new()
	{
		public readonly string Name;
		public readonly int Version;

		private AmazonDynamoDBClient? client;
		private DynamoDBContext? context;
		private DynamoDBOperationConfig? operationConfig;

		public Database(string databaseName, int version)
		{
			this.Name = databaseName;
			this.Version = version;
		}

		private string InternalName
		{
			get
			{
				return "KupoNuts_" + this.Name + "_" + this.Version;
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
			////NetSDKCredentialsFile netDSKFile = new NetSDKCredentialsFile();
			////netDSKFile.RegisterProfile(profile);

			AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig();

			this.client = new AmazonDynamoDBClient(dbConfig);

			////await this.EnsureTable();

			this.context = new DynamoDBContext(this.client);

			try
			{
				await this.client.ListTablesAsync();
			}
			catch (Exception)
			{
				throw new Exception("Unable to connect to database");
			}

			this.operationConfig = new DynamoDBOperationConfig();
			this.operationConfig.OverrideTableName = this.InternalName;
		}

		public async Task<T> CreateEntry(string? id = null)
		{
			if (id == null)
				id = await this.GetNewID();

			T t = new T();
			t.Id = id;

			return t;
		}

		public async Task<string> GetNewID()
		{
			// Probably faster to use sequential id,s and just get the next highest from the db? but
			// guid collisions are super improbable, so this might actually be faster?
			// It might be possible to ignore the check, and just hope the guid is clear, _or_ we could
			// load all the guids into a hashtable in memory and compare there instead.
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

		public async Task<T> Load(string key)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			return await this.context.LoadAsync<T>(key, this.operationConfig);
		}

		public async Task<List<T>> LoadAll()
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			List<ScanCondition> conditions = new List<ScanCondition>();
			AsyncSearch<T> search = this.context.ScanAsync<T>(conditions, this.operationConfig);
			return await search.GetRemainingAsync();
		}

		public async Task Delete(T entry)
		{
			if (entry.Id == null)
				return;

			await this.Delete(entry.Id);
		}

		public async Task Delete(string key)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			await this.context.DeleteAsync<T>(key, this.operationConfig);
		}

		public Task Save(T document)
		{
			if (this.context == null)
				throw new Exception("Database is not connected");

			document.Updated = DateTime.UtcNow;
			return this.context.SaveAsync(document, this.operationConfig);
		}

		private async Task EnsureTable()
		{
			if (this.client == null)
				throw new Exception("Database is not connected");

			ListTablesResponse listTablesResponse = await this.client.ListTablesAsync();

			if (listTablesResponse.TableNames.Contains(this.InternalName))
			{
				return;
				/*Log.Write("A table named: \"" + this.InternalName + "\" already exists. Deleting...");

				DeleteTableRequest deleteRequest = new DeleteTableRequest();
				deleteRequest.TableName = this.InternalName;
				await this.client.DeleteTableAsync(deleteRequest);*/
			}

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
				ProvisionedThroughput = new ProvisionedThroughput(10, 10),
			};

			try
			{
				CreateTableResponse createTableResponse = await this.client.CreateTableAsync(request);
				Console.WriteLine("[Database] Created the Table: \"" + this.InternalName + "\"");
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to create table: \"" + this.InternalName + "\"", ex);
			}
		}
	}
}
