// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;

	public class CalendarService : ServiceBase
	{
		public override async Task Initialize()
		{
			Database db = Database.Load();

			if (string.IsNullOrEmpty(db.CalendarChannel))
				return;

			ulong channelId = ulong.Parse(db.CalendarChannel);
			SocketTextChannel channel = (SocketTextChannel)Program.DiscordClient.GetChannel(channelId);

			// This week
			ulong messageId = 0;

			if (!string.IsNullOrEmpty(db.CalendarMessage))
				messageId = ulong.Parse(db.CalendarMessage);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("**Events in the next week**");
			builder.AppendLine();

			builder.AppendLine("Today");
			builder.AppendLine("- [Mount Farm](https://discordapp.com/channels/391492798353768449/396103387223162881/604606478694875136) - In 1 Hour, 45 Minutes");
			builder.AppendLine();

			builder.AppendLine("Tomorrow");
			builder.AppendLine(" - [Admin Meeting](https://discordapp.com/channels/391492798353768449/604511137299431424/607106732723929088)");
			builder.AppendLine(" - [Sunday Funday](https://discordapp.com/channels/391492798353768449/396103387223162881/604613965150158867)");
			builder.AppendLine();

			////weekBuilder.AppendLine("**Monday**");
			////weekBuilder.AppendLine("None");
			////weekBuilder.AppendLine();

			builder.AppendLine("Tuesday");
			builder.AppendLine(" - [Kuponauts Static Night](https://discordapp.com/channels/391492798353768449/567206290372165644/606418388054704139)");
			builder.AppendLine();

			////weekBuilder.AppendLine("**Wednesday**");
			////weekBuilder.AppendLine("None");
			////weekBuilder.AppendLine();

			builder.AppendLine("Thursday");
			builder.AppendLine(" - [Kuponauts Static Night](https://discordapp.com/channels/391492798353768449/567206290372165644/606418388054704139)");
			builder.AppendLine(" - [Thursday DnD](https://discordapp.com/channels/391492798353768449/540172911269380096/605686026521935872)");
			builder.AppendLine();

			builder.AppendLine("Friday");
			builder.AppendLine(" - [Maps](https://discordapp.com/channels/391492798353768449/396103387223162881/606425996010192896)");
			builder.AppendLine();

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();

			if (messageId == 0)
			{
				RestUserMessage message = await channel.SendMessageAsync(null, false, embedBuilder.Build());

				db = Database.Load();
				db.CalendarMessage = message.Id.ToString();
				db.Save();
			}
			else
			{
				RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(messageId);
				await message.ModifyAsync(x =>
				{
					x.Embed = embedBuilder.Build();
				});
			}

			// Future
			messageId = 0;

			if (!string.IsNullOrEmpty(db.CalendarMessage2))
				messageId = ulong.Parse(db.CalendarMessage2);

			builder = new StringBuilder();

			builder.AppendLine("**Future events**");
			builder.AppendLine();
			builder.AppendLine("Monday 30th September 2019");
			builder.AppendLine(" - [The Eikonic](https://discordapp.com/channels/391492798353768449/396103387223162881/606425996010192896)");
			builder.AppendLine();

			embedBuilder = new EmbedBuilder();
			embedBuilder.Description = builder.ToString();

			if (messageId == 0)
			{
				RestUserMessage message = await channel.SendMessageAsync(null, false, embedBuilder.Build());

				db = Database.Load();
				db.CalendarMessage2 = message.Id.ToString();
				db.Save();
			}
			else
			{
				RestUserMessage message = (RestUserMessage)await channel.GetMessageAsync(messageId);
				await message.ModifyAsync(x =>
				{
					x.Embed = embedBuilder.Build();
				});
			}
		}

		public override Task Shutdown()
		{
			return Task.CompletedTask;
		}
	}
}
