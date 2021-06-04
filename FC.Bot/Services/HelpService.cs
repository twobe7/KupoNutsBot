// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using FC.Utils;
	using NodaTime;

	public class HelpService : ServiceBase
	{
		public static Emoji First = new Emoji("⏮️");
		public static Emoji Previous = new Emoji("⏪");
		public static Emoji Next = new Emoji("⏩");
		public static Emoji Last = new Emoji("⏭️");

		private static readonly int PageSize = 10;

		private static readonly List<Emoji> HelpEmotes = new List<Emoji>()
		{
			First, Previous, Next, Last,
		};

		private static Task? activeHelpWindowTask;

		private static Dictionary<ulong, ActiveHelp> activeHelpEmbeds = new Dictionary<ulong, ActiveHelp>();

		public static string GetTypeName(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean: return "boolean";
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal: return "number";
				case TypeCode.DateTime: return "date and time";
				case TypeCode.String: return "string";
			}

			if (type == typeof(SocketTextChannel))
				return "#channel";

			if (type == typeof(IEmote))
				return ":emote:";

			if (type == typeof(IUser))
				return "@user";

			if (type == typeof(IGuildUser))
				return "@user";

			if (type == typeof(Duration))
				return "duration (1d:1h:1m:1s)";

			return type.Name;
		}

		public static string GetParam(ParameterInfo param, bool requiresQuotes)
		{
			string? name = param.Name;
			Type type = param.ParameterType;

			if (name == null)
				return "unknown";

			name = Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
			name = name.First().ToString().ToUpper() + name.Substring(1);
			name = "[" + name + "]";

			if (type == typeof(string) && requiresQuotes)
				name = "\"" + name + "\"";

			if (type == typeof(IEmote))
				name = ":" + name + ":";

			if (type == typeof(IUser) || type == typeof(IGuildUser))
				name = "@" + name;

			return name;
		}

		public static Task<Embed> GetHelp(CommandMessage message, string? command = null)
		{
			StringBuilder builder = new StringBuilder();
			Permissions permissions = CommandsService.GetPermissions(message.Author);

			if (command == null)
				command = message.Command;

			builder.AppendLine(GetHelp(message.Guild, command, permissions));

			EmbedBuilder embed = new EmbedBuilder();
			embed.Description = builder.ToString();

			if (string.IsNullOrEmpty(embed.Description))
				throw new UserException("I'm sorry, you don't have permission to use that command.");

			return Task.FromResult(embed.Build());
		}

		public static async Task<bool> GetHelp(CommandMessage message, Permissions permissions)
		{
			int page = 0;
			Embed embed = GetHelp(message.Guild, permissions, 0, out page);

			Discord.Rest.RestUserMessage helpWindow = await message.Channel.SendMessageAsync(null, false, embed, messageReference: message.MessageReference);

			// Add window to list
			activeHelpEmbeds.Add(helpWindow.Id, new ActiveHelp(message.Author.Id, 0));

			// Add reactions
			await helpWindow.AddReactionsAsync(HelpEmotes.ToArray());

			// Begin the clean up task if it's not already running
			if (activeHelpWindowTask == null || !activeHelpWindowTask.Status.Equals(TaskStatus.Running))
				activeHelpWindowTask = Task.Run(async () => await ClearReactionsAfterDelay(message.Channel));

			return true;
		}

		public override async Task Initialize()
		{
			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;
			await base.Initialize();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			return base.Shutdown();
		}

		[Command("Help", Permissions.Everyone, "View all commands available to you", CommandCategory.Hidden, requiresQuotes: false, showWait: false)]
		public async Task<bool> Help(CommandMessage message)
		{
			Permissions permissions = CommandsService.GetPermissions(message.Author);
			return await GetHelp(message, permissions);
		}

		[Command("Help", Permissions.Everyone, "View extended information about given command", CommandCategory.Hidden, requiresQuotes: false)]
		public async Task<Embed> Help(CommandMessage message, string command)
		{
			return await GetHelp(message, command);
		}

		private static Embed GetHelp(IGuild guild, Permissions permissions, int startIndex, out int page)
		{
			// Get all help commands
			IEnumerable<HelpCommand> helpCommands = CommandsService.GetGroupedHelpCommands()
															.Where(x => x.Permission <= permissions);

			int numberOfPages = 0;
			int pagesForGroup = 0;
			foreach (IGrouping<CommandCategory, HelpCommand> group in helpCommands.GroupBy(x => x.CommandCategory))
			{
				pagesForGroup = (int)Math.Ceiling((decimal)group.Count() / PageSize);
				numberOfPages += pagesForGroup;

				// Create spacer commands
				int spacerCommandsRequired = (PageSize * pagesForGroup) - group.Count();
				for (int i = 0; i < spacerCommandsRequired; i++)
					helpCommands = helpCommands.Append(new HelpCommand("Spacer", group.Key, string.Empty, Permissions.Everyone));
			}

			helpCommands = helpCommands.OrderBy(x => x.CommandCategory)
										.ThenBy(x => x.CommandName)
										.AsEnumerable();

			// page is 0-indexed
			page = startIndex + 1;

			// If page is greater than last page, wrap to start
			if (page > numberOfPages)
			{
				page = 1;
				startIndex = 0;
			}

			// Moving to last page
			if (startIndex == -1)
			{
				page = numberOfPages;

				helpCommands = helpCommands.TakeLast(PageSize);

				startIndex = 0;
			}

			// Restrict to page size
			helpCommands = helpCommands.Skip(startIndex * PageSize).Take(PageSize);

			// Start Embed
			EmbedBuilder embed = new EmbedBuilder();
			StringBuilder builder = new StringBuilder();

			// Category name
			embed.Title = helpCommands.FirstOrDefault().CommandCategory.ToDisplayString() + " Commands";

			// Iterate commands
			foreach (HelpCommand category in helpCommands)
			{
				if (category.CommandName.ToLower() == "spacer")
					continue;

				builder.Append("**" + category.CommandName + "**");
				builder.Append(" - ");

				if (category.CommandCount > 1)
					builder.Append("*+" + category.CommandCount + "* - ");

				builder.Append(category.Help);
				builder.AppendLine();

				if (category.CommandShortcuts.Count > 0)
				{
					builder.Append("Shortcut: ");
					foreach (string shortcutString in category.CommandShortcuts)
					{
						builder.Append(shortcutString + ", ");
					}

					builder.Remove(builder.Length - 2, 2);

					builder.AppendLine();
				}

				builder.AppendLine();
			}

			builder.AppendLine();

			string prefix = CommandsService.GetPrefix(guild);
			embed.WithThumbnailUrl("https://cdn.discordapp.com/attachments/825936704023691284/828491389838426121/think3.png")
				.WithDescription(builder.ToString())
				.WithAuthor(string.Format("Page {0} of {1}", page, numberOfPages))
				.WithFooter("To get more information on a command, look it up directly, like \"" + prefix + "help time\" or \"" + prefix + "et ?\"");

			// Return page to 0-index
			page -= 1;

			return embed.Build();
		}

		private static string? GetHelp(IGuild guild, string commandStr, Permissions permissions)
		{
			StringBuilder builder = new StringBuilder();
			List<Command> commands = CommandsService.GetCommands(commandStr);

			int count = 0;
			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				count++;
			}

			if (count <= 0)
				return null;

			builder.Append("__");
			////builder.Append(CommandsService.CommandPrefix);
			builder.Append(commandStr);
			builder.AppendLine("__");

			foreach (Command command in commands)
			{
				// Don't show commands users cannot access
				if (command.Permission > permissions)
					continue;

				builder.Append(Utils.Characters.Tab);
				builder.Append(command.Permission);
				builder.Append(" - *");
				builder.Append(command.Help);
				builder.AppendLine("*");

				List<ParameterInfo> parameters = command.GetNeededParams();

				builder.Append("**");
				builder.Append(Utils.Characters.Tab);
				builder.Append(CommandsService.GetPrefix(guild));
				builder.Append(commandStr);
				builder.Append(" ");

				for (int i = 0; i < parameters.Count; i++)
				{
					if (i != 0)
						builder.Append(" ");

					builder.Append(GetParam(parameters[i], command.RequiresQuotes));
				}

				builder.Append("**");
				builder.AppendLine();
				builder.AppendLine();
			}

			return builder.ToString();
		}

		private static async Task<Task> ClearReactionsAfterDelay(ISocketMessageChannel channel)
		{
			while (activeHelpEmbeds.Count > 0)
			{
				Log.Write("Checking For Inactive Help Windows from total of " + activeHelpEmbeds.Count, "Bot");

				DateTime runTime = DateTime.Now;

				foreach (KeyValuePair<ulong, ActiveHelp> help in activeHelpEmbeds)
				{
					if ((runTime - help.Value.LastInteractedWith).TotalSeconds > 20)
					{
						IMessage? cmdMessage = null;
						IMessage message = await channel.GetMessageAsync(help.Key);

						if (message.Reference != null && message.Reference.MessageId.IsSpecified)
						{
							cmdMessage = await channel.GetMessageAsync(message.Reference.MessageId.Value);
						}

						await message.DeleteAsync();

						if (cmdMessage != null)
							await cmdMessage.DeleteAsync();

						activeHelpEmbeds.Remove(help.Key);
					}
				}

				await Task.Delay(15000);
			}

			Log.Write("All Help Windows Inactive", "Bot");
			return Task.CompletedTask;
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> incomingMessage, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				// Don't react to your own reacts!
				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				// Only handle reacts to help embed
				if (!activeHelpEmbeds.ContainsKey(incomingMessage.Id))
					return;

				ActiveHelp helpWindow = activeHelpEmbeds[incomingMessage.Id];
				helpWindow.LastInteractedWith = DateTime.Now;

				// Only handle reacts from the original user, remove the reaction
				if (helpWindow.UserId != reaction.UserId)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				// Only handle relevant reacts
				if (!HelpEmotes.Contains(reaction.Emote))
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					return;
				}

				if (channel is SocketGuildChannel guildChannel)
				{
					IUserMessage message = await incomingMessage.DownloadAsync();
					await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

					// Adjust current page
					if (reaction.Emote.Equals(First))
					{
						helpWindow.CurrentPage = 0;
					}
					else if (reaction.Emote.Equals(Previous))
					{
						helpWindow.CurrentPage -= 1;
					}
					else if (reaction.Emote.Equals(Next))
					{
						helpWindow.CurrentPage += 1;
					}
					else if (reaction.Emote.Equals(Last))
					{
						helpWindow.CurrentPage = -1;
					}

					Permissions permissions = CommandsService.GetPermissions(message.GetAuthor());

					int currentPage = helpWindow.CurrentPage;
					Embed embed = GetHelp(message.GetGuild(), permissions, helpWindow.CurrentPage, out currentPage);

					// Update current page
					helpWindow.CurrentPage = currentPage;

					await message.ModifyAsync(x => x.Embed = embed);
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		public class ActiveHelp
		{
			public ActiveHelp(ulong userId, int currentPage)
			{
				this.UserId = userId;
				this.CurrentPage = currentPage;
				this.LastInteractedWith = DateTime.Now;
			}

			public ulong UserId { get; set; }
			public int CurrentPage { get; set; }
			public DateTime LastInteractedWith { get; set; }
		}
	}
}
