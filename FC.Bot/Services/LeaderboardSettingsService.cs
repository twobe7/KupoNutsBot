// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System.Threading.Tasks;
	using FC.Data;

	public class LeaderboardSettingsService : ServiceBase
	{
		private static readonly Table LeaderboardSettingsDb = new Table("LeaderboardSettings", 0);

		public static async Task<T> GetSettings<T>(ulong guildId)
			where T : SettingsEntry, new()
		{
			string key = guildId + typeof(T).FullName;
			T settings = await LeaderboardSettingsDb.LoadOrCreate<T>(key);
			settings.Guild = guildId;
			return settings;
		}

		public static async Task SaveSettings<T>(T settings)
			where T : SettingsEntry, new()
		{
			await LeaderboardSettingsDb.Save<T>(settings);
		}

		public override async Task Initialize()
		{
			await base.Initialize();
			await LeaderboardSettingsDb.Connect();
		}
	}
}
