// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Housing
{
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using FC.Bot.Commands;
	using FC.Bot.Services;
	using PaissaHouse;

	public class HousingService : ServiceBase
	{
		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public override Task Shutdown()
		{
			return base.Shutdown();
		}

		[Command("OpenPlots", Permissions.Everyone, "Shows your level and current xp", CommandCategory.XIVData)]
		public async Task GetOpenPlotsForWorld(CommandMessage message, string world)
		{
			// Get user information
			HousingAPI.SearchResponse openPlots = await HousingAPI.Worlds(world);

			// Build embed
			EmbedBuilder builder = GetEmbed(openPlots.Name ?? "Unable to find World");

			if (openPlots.NumOpenPlots > 0)
			{
				foreach (HousingAPI.District district in openPlots.Districts.Where(x => x.NumOpenPlots > 0))
				{
					// Start field for district
					EmbedFieldBuilder districtFieldBuilder = new EmbedFieldBuilder()
						.WithName(district.Name);

					// Build description
					foreach (HousingAPI.OpenPlot plot in district.OpenPlots)
						districtFieldBuilder.Value += plot.GetInfo() + "\n";

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
			await message.Channel.SendMessageAsync(embed: builder.Build(), messageReference: message.MessageReference);
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
