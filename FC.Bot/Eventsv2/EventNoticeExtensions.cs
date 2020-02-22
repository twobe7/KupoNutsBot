// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Eventsv2
{
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Eventsv2;

	public static class EventNoticeExtensions
	{
		public static async Task Update(this Event.Notice self, Event owner)
		{
			SocketTextChannel? channel = Program.DiscordClient.GetChannel(owner.Channel) as SocketTextChannel;
			if (channel is null)
				return;

			EmbedBuilder builder = await self.BuildEmbed(owner);

			RestUserMessage? message = null;
			if (self.MessageId != null)
				message = await channel.GetMessageAsync((ulong)self.MessageId) as RestUserMessage;

			if (message is null)
			{
				message = await channel.SendMessageAsync(null, false, builder.Build());
				self.MessageId = message.Id;
				await EventsService.SaveEvent(owner);
			}
			else
			{
				await message.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});
			}
		}

		private static Task<EmbedBuilder> BuildEmbed(this Event.Notice self, Event owner)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = owner.Name;
			builder.Description = owner.Description;
			builder.ImageUrl = owner.ImageUrl;

			return Task.FromResult(builder);
		}
	}
}
