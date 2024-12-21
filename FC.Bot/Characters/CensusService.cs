// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Characters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FC.Bot.Commands;
using FC.Bot.Services;
using XIVAPI;

public class CensusService(DiscordSocketClient discordClient) : ServiceBase
{
	public readonly DiscordSocketClient DiscordClient = discordClient;

	public override async Task Initialize()
	{
		await base.Initialize();
	}

#if DEBUG
	[Command("Census", Permissions.Administrators, "Perform Census")]
#endif
	public async void GetCharacterCensus(CommandMessage message, ulong freeCompanyId)
	{
		Embed embed = await this.GetFreeCompanyCensus(freeCompanyId);
		await message.Channel.SendMessageAsync(embed: embed);
	}

	private async Task<Embed> GetFreeCompanyCensus(ulong freeCompanyId)
	{
		EmbedBuilder embed = new()
		{
			Title = "Free Company Census",
		};

		FreeCompanyAPI.GetResponse response = await FreeCompanyAPI.GetFreeCompany(freeCompanyId);

		if (response.FreeCompanyMembers == null)
			return embed.WithDescription("No members found").Build();

		// Census variables
		List<CensusData> data = [];
		DateTime startTime = new(1970, 1, 1);
		DateTime activeThreshold = DateTime.Now.Date.AddMonths(-3);

		// Loop members
		foreach (FreeCompanyAPI.Member member in response.FreeCompanyMembers)
		{
			FreeCompanyAPI.GetResponse responseCharacter = await FreeCompanyAPI.GetCharacter(member.ID, FreeCompanyAPI.CharacterData.None, "Character.Gender,Character.Race,Character.Tribe,Character.ParseDate");

			if (responseCharacter.Character == null)
				continue;

			FreeCompanyAPI.Character character = responseCharacter.Character;

			CensusData entry = new()
			{
				// Race
				Race = ((CharacterInfo.Races)character.Race).ToDisplayString(),

				// Tribe
				Tribe = ((CharacterInfo.Tribes)character.Tribe).ToDisplayString(),

				// Male = 1, Female = 2
				Gender = character.Gender,
			};

			// Parse last seen date
			DateTime parsedDate = startTime.AddSeconds(Convert.ToDouble(character.ParseDate));

			// Member is active if seen in last 3 months
			if (parsedDate > activeThreshold)
				entry.Active = true;

			// Insert delay to try space out API request
			////await Task.Delay(250);
		}

		// Total Members
		embed.AddField("Total Members", data.Count, true);

		// Race ranking
		StringBuilder raceRanking = new();
		foreach (IGrouping<string, CensusData> raceGroup in data.GroupBy(x => x.Race))
		{
			// Tribes
			IEnumerable<IGrouping<string, CensusData>> x = raceGroup.GroupBy(x => x.Tribe);
			(string, int) tribeA = (x.First().Key, x.First().Count());
			(string, int) tribeB = (x.Last().Key, x.Last().Count());

			raceRanking.Append($"{raceGroup.Key} - {raceGroup.Count()} ");
			raceRanking.AppendLine($"{tribeA.Item1}: {tribeA.Item2}, {tribeB.Item1}: {tribeB.Item2})");
		}

		embed.AddField("Race", raceRanking);

		// Gender ranking
		StringBuilder genderRanking = new();
		foreach (IGrouping<uint, CensusData> genderGroup in data.GroupBy(x => x.Gender))
		{
			string gender = genderGroup.Key == 1 ? "Male" : "Female";
			genderRanking.AppendLine($"{gender} - {genderGroup.Count()}");
		}

		embed.AddField("Gender", genderRanking);

		// Only active
		List<CensusData> activeData = data.Where(x => x.Active).ToList();

		// Active Members
		embed.AddField("Active Members", activeData.Count, true);

		// Race ranking
		StringBuilder activeRaceRanking = new();
		foreach (IGrouping<string, CensusData> raceGroup in activeData.GroupBy(x => x.Race))
		{
			// Tribes
			IEnumerable<IGrouping<string, CensusData>> x = raceGroup.GroupBy(x => x.Tribe);
			(string, int) tribeA = (x.First().Key, x.First().Count());
			(string, int) tribeB = (x.Last().Key, x.Last().Count());

			activeRaceRanking.Append($"{raceGroup.Key} - {raceGroup.Count()} ");
			activeRaceRanking.AppendLine($"{tribeA.Item1}: {tribeA.Item2}, {tribeB.Item1}: {tribeB.Item2})");
		}

		embed.AddField("Active Race", activeRaceRanking);

		// Gender ranking
		StringBuilder activeGenderRanking = new();
		foreach (IGrouping<uint, CensusData> genderGroup in activeData.GroupBy(x => x.Gender))
		{
			string gender = genderGroup.Key == 1 ? "Male" : "Female";
			activeGenderRanking.AppendLine($"{gender} - {genderGroup.Count()}");
		}

		embed.AddField("Active Gender", activeGenderRanking);

		return embed.Build();
	}

	private class CensusData
	{
		public string Race { get; set; } = string.Empty;
		public string Tribe { get; set; } = string.Empty;
		public uint Gender { get; set; }
		public bool Active { get; set; } = false;
	}
}
