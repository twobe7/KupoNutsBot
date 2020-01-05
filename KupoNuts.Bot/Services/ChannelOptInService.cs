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
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.Utils;

	public class ChannelOptInService : ServiceBase
	{
		private static Dictionary<(ulong, ulong), ChannelData.OptInInfo> watchChannels = new Dictionary<(ulong, ulong), ChannelData.OptInInfo>();

		public override async Task Initialize()
		{
			await base.Initialize();

			List<ChannelData> channels = await ChannelService.GetAllChannelData();
			foreach (ChannelData channelData in channels)
			{
				if (channelData.OptIn == null)
					continue;

				if (string.IsNullOrEmpty(channelData.GuildId))
					continue;

				if (string.IsNullOrEmpty(channelData.ChannelId))
					continue;

				ulong guildId = ulong.Parse(channelData.GuildId);
				ulong channelId = ulong.Parse(channelData.ChannelId);

				watchChannels.Add((guildId, channelId), channelData.OptIn);
			}

			Program.DiscordClient.MessageReceived += this.DiscordClient_MessageReceived;
			Program.DiscordClient.ReactionAdded += this.DiscordClient_ReactionAdded;
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.MessageReceived -= this.DiscordClient_MessageReceived;
			return base.Shutdown();
		}

		[Command("AddOptIn", Permissions.Administrators, "Adds a role to the given channel opt in service")]
		public async Task OptChannel(CommandMessage message, IRole role, IEmote emote)
		{
			ChannelData data = await ChannelService.GetChannelData(message.Guild.Id, message.Channel.Id);
			data.OptIn = new ChannelData.OptInInfo();
			data.OptIn.Emote = emote.Name;
			data.OptIn.Role = role.Id.ToString();
			await ChannelService.SaveChannelData(data);

			if (watchChannels.ContainsKey((message.Guild.Id, message.Channel.Id)))
				watchChannels.Remove((message.Guild.Id, message.Channel.Id));

			watchChannels.Add((message.Guild.Id, message.Channel.Id), data.OptIn);
		}

		[Command("ClearOptIn", Permissions.Administrators, "Removes the opt in service from the current channel")]
		public async Task OptChannel(CommandMessage message)
		{
			ChannelData data = await ChannelService.GetChannelData(message.Guild.Id, message.Channel.Id);
			data.OptIn = null;
			await ChannelService.SaveChannelData(data);

			if (watchChannels.ContainsKey((message.Guild.Id, message.Channel.Id)))
			{
				watchChannels.Remove((message.Guild.Id, message.Channel.Id));
			}
		}

		private async Task DiscordClient_MessageReceived(SocketMessage arg)
		{
			if (!(arg.Channel is SocketGuildChannel guildChannel))
				return;

			ulong guildId = arg.GetGuild().Id;
			ulong channelId = arg.Channel.Id;

			if (!watchChannels.ContainsKey((guildId, channelId)))
				return;

			RestUserMessage? message = await arg.Channel.GetMessageAsync(arg.Id) as RestUserMessage;
			if (message == null)
				return;

			ChannelData.OptInInfo info = watchChannels[(guildId, channelId)];
			IEmote emote = EmoteUtility.Parse(info.Emote);

			await message.AddReactionAsync(emote);
		}

		private async Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (arg3.UserId == Program.DiscordClient.CurrentUser.Id)
				return;

			IUserMessage msg = await arg1.GetOrDownloadAsync();

			ulong guildId = msg.GetGuild().Id;
			ulong channelId = msg.Channel.Id;

			if (!watchChannels.ContainsKey((guildId, channelId)))
				return;

			ChannelData.OptInInfo data = watchChannels[(guildId, channelId)];

			if (arg3.Emote.Name != data.Emote)
				return;

			_ = Task.Run(async () =>
			{
				try
				{
					await msg.RemoveReactionAsync(arg3.Emote, arg3.User.GetValueOrDefault());

					SocketGuild guild = Program.DiscordClient.GetGuild(guildId);
					SocketRole role = guild.GetRole(ulong.Parse(data.Role));
					SocketGuildUser user = guild.GetUser(arg3.UserId);

					bool hasRole = false;
					foreach (SocketRole otherRole in user.Roles)
					{
						if (otherRole.Id == role.Id)
						{
							hasRole = true;
							break;
						}
					}

					IUserMessage msg2;

					if (!hasRole)
					{
						await user.AddRoleAsync(role);
						msg2 = await user.SendMessageAsync(user.Mention + ", you have been granted the " + role.Name + " role on: " + guild.Name);
					}
					else
					{
						await user.RemoveRoleAsync(role);
						msg2 = await user.SendMessageAsync(user.Mention + ", you have lost the " + role.Name + " role on the: " + guild.Name);
					}
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			});
		}
	}
}
