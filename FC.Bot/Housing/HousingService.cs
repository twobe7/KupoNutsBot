// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Housing
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FC.XIVData;
	using PaissaHouse;

	[Group("housing", "View housing availability")]
	public class HousingService : ServiceBase
	{
		public HousingService(DiscordSocketClient discordClient)
		{
		}

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public override Task Shutdown()
		{
			return base.Shutdown();
		}

		[SlashCommand("open-plots", "Check available housing plots")]
		public async Task GetOpenPlotsForWorld(
			[Autocomplete(typeof(EnumAutoCompleteHandler<XivWorld>))]
			[Summary("serverName", "Name of Character Server")]
			string world)
		{
			await this.DeferAsync();

			// Get user information
			HousingAPI.SearchResponse openPlots = await HousingAPI.Worlds(world);

			// Build embed
			EmbedBuilder builder = GetEmbed(openPlots.Name ?? "Unable to find World");

			if (openPlots.IsError)
			{
				builder.Title = "Open Plots - Error Occurred";
				builder.Description = "Paissa DB says: " + openPlots.ErrorMessage;
			}
			else if (openPlots.NumOpenPlots > 0)
			{
				foreach (HousingAPI.District district in openPlots.Districts.Where(x => x.NumOpenPlots > 0))
				{
					// Start field for district
					EmbedFieldBuilder districtFieldBuilder = new EmbedFieldBuilder()
						.WithName(district.Name);

					StringBuilder stringBuilder = new();

					bool fieldLengthMaxReached = false;

					// Build description
					foreach (HousingAPI.OpenPlot plot in district.OpenPlots)
					{
						// Stop processing plots if field limit has been reached
						if (fieldLengthMaxReached)
							break;

						var plotInfo = $"{plot.GetInfo()}\n";

						if (stringBuilder.Length + plotInfo.Length <= EmbedFieldBuilder.MaxFieldValueLength)
						{
							stringBuilder.Append(plotInfo);
						}
						else if (!fieldLengthMaxReached)
						{
							stringBuilder.Append("And more...");
							fieldLengthMaxReached = true;
						}
					}

					districtFieldBuilder.Value = stringBuilder.ToString(0, Math.Min(stringBuilder.Length, EmbedFieldBuilder.MaxFieldValueLength));

					// Add to embed
					builder.AddField(districtFieldBuilder);
				}

				builder.Description += "\n\n View [Area Maps on Imgur](https://imgur.com/a/n7wzC)";
			}
			else
			{
				builder.Description = "No Open Plots Available";
			}

			// Send Embed
			await this.FollowupAsync(embeds: [builder.Build()]);
		}

		private static EmbedBuilder GetEmbed(string world)
		{
			return new EmbedBuilder()
				.WithTitle($"Open Plots - {world}")
				.WithFooter(new EmbedFooterBuilder()
					.WithText("Data from PaissaHouse Plugin - PaissaDB"));
		}
	}
}
