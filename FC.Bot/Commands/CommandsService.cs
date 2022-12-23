// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Json;
	using System.Reactive.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using AngleSharp;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Services;

	public class CommandsService : ServiceBase
	{
		public readonly DiscordSocketClient DiscordClient;

		private static Dictionary<string, List<Command>> commandHandlers = new Dictionary<string, List<Command>>();
		private static Dictionary<string, List<Command>> groupedCommandHandlers = new Dictionary<string, List<Command>>();
		private static Dictionary<ulong, string> prefixCache = new Dictionary<ulong, string>();

		private static List<HelpCommand> helpCommands = new List<HelpCommand>();

		public CommandsService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		public static string GetPrefix(IGuild guild)
		{
			return GetPrefix(guild.Id);
		}

		public static string GetPrefix(ulong guildId)
		{
			if (prefixCache.ContainsKey(guildId))
				return prefixCache[guildId];

			return "?";
		}

		public static void BindCommands(object obj)
		{
			Dictionary<MethodInfo, List<CommandAttribute>> commands = CommandAttribute.GetCommands(obj.GetType());

			foreach ((MethodInfo method, List<CommandAttribute> attributes) in commands)
			{
				foreach (CommandAttribute attribute in attributes)
				{
					if (!commandHandlers.ContainsKey(attribute.CommandLower))
						commandHandlers.Add(attribute.CommandLower, new List<Command>());

					Command cmd = new Command(method, obj, attribute.Permissions, attribute.CommandLower, attribute.Help, attribute.CommandCategory.ToDisplayString(), attribute.RequiresQuotes, attribute.ShowWait);
					commandHandlers[attribute.CommandLower].Add(cmd);

					// Do not add Hidden commands to Help list
					if (attribute.CommandCategory == CommandCategory.Hidden)
						continue;

					// Add the shortcut command to grouped dictionary
					if (!string.IsNullOrWhiteSpace(attribute.CommandParent))
					{
						HelpCommand? helpCmd = helpCommands.FirstOrDefault(x => x.CommandName == attribute.CommandParent);
						if (helpCmd == null)
						{
							helpCommands.Add(new HelpCommand(attribute.CommandParent, attribute.CommandCategory, attribute.Help, attribute.Permissions, attribute.Command));
						}
						else
						{
							if (!helpCmd.CommandShortcuts.Contains(attribute.Command))
								helpCmd.CommandShortcuts.Add(attribute.Command);
						}
					}
					else
					{
						HelpCommand? helpCmd = helpCommands.FirstOrDefault(x => x.CommandName == attribute.Command);
						if (helpCmd == null)
						{
							helpCommands.Add(new HelpCommand(attribute.Command, attribute.CommandCategory, attribute.Help, attribute.Permissions));
						}
						else
						{
							helpCmd.CommandCount++;
						}
					}

					Log.Write("Registered command: \"" + attribute.Command + "\"", "Bot");
				}
			}
		}

		public static IReadOnlyCollection<string> GetCommands()
		{
			return commandHandlers.Keys;
		}

		public static Dictionary<string, List<Command>> GetGroupedCommands()
		{
			return groupedCommandHandlers;
		}

		public static List<HelpCommand> GetGroupedHelpCommands()
		{
			return helpCommands;
		}

		public static List<Command> GetCommands(string command)
		{
			if (!commandHandlers.ContainsKey(command.ToLower()))
				throw new UserException("I don't know that command.");

			return commandHandlers[command.ToLower()];
		}

		public static Permissions GetPermissions(IGuildUser user)
		{
			if (user is SocketGuildUser guildUser)
			{
				foreach (SocketRole role in guildUser.Roles)
				{
					if (role.Permissions.Administrator)
					{
						return Permissions.Administrators;
					}
				}
			}

			return Permissions.Everyone;
		}

		public override async Task Initialize()
		{
			this.DiscordClient.MessageReceived += this.OnMessageReceived;

			ScheduleService.RunOnSchedule(this.Update, 15);
			await this.Update();
		}

		public override Task Shutdown()
		{
			this.DiscordClient.MessageReceived -= this.OnMessageReceived;

			return Task.CompletedTask;
		}

		private async Task Update()
		{
			prefixCache.Clear();
			foreach (SocketGuild guild in this.DiscordClient.Guilds)
			{
				GuildSettings settings = await SettingsService.GetSettings<GuildSettings>(guild.Id);

				prefixCache.Add(guild.Id, settings.Prefix);
			}
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			// Ignore messages that did not come from users
			if (message is not SocketUserMessage)
				return;

			// Ignore our own messages
			if (message.Author.Id == this.DiscordClient.CurrentUser.Id)
				return;

			ulong guildId = message.GetGuild().Id;
			string prefix = GetPrefix(guildId);

			// Special case to display prefix for guild
			if (message.Content.Contains(this.DiscordClient.CurrentUser.Mention.Replace("!", string.Empty)) && message.Content.Contains("prefix"))
			{
				await message.Channel.SendMessageAsync($"This discord's prefix is `{prefix}`", messageReference: new MessageReference(message.Id));
				return;
			}

			// That tiktok thing
			if (message.Content.Contains(".tiktok.com"))
			{
				bool isValidUrl = Uri.TryCreate(message.Content, UriKind.Absolute, out Uri? uriResult)
					&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				if (isValidUrl && uriResult != null)
				{
					using var client = new HttpClient();
					var response = await client.GetStringAsync($"https://unshorten.me/s/{uriResult.AbsoluteUri}");

					if (response.IndexOf("?_") != -1)
					{
						response = response[..response.IndexOf("?_")];
					}

					response = response.Replace("tiktok", "vxtiktok");

					await message.Channel.SendMessageAsync(text: response, messageReference: new MessageReference(message.Id));
					return;
				}
			}

			// Ignore messages that do not start with the command character
			if (!message.Content.StartsWith(prefix))
				return;

			string command = message.Content[prefix.Length..];
			command = command.TrimStart(' ', '	');

			// replace funky quote-left and quote-right with normal quotes.
			command = command.Replace('”', '"');
			command = command.Replace('“', '"');

			// command must contain an actual command (don't process "?" as a command)
			if (command.Length <= 0)
				return;

			// the first letter of the input must be a letter or a number (don't process "?????" or ">>>" as a command)
			char first = command[0];
			if (!char.IsLetter(first) && !char.IsNumber(first))
				return;

			await this.ProcessCommandInput(message, command);
		}

		private async Task ProcessCommandInput(SocketMessage message, string command)
		{
			string[] parts = Regex.Split(command, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

			command = parts[0];
			List<string> args = new ();

			if (parts.Length > 1)
			{
				for (int i = 1; i < parts.Length; i++)
				{
					string arg = parts[i];

					if (string.IsNullOrEmpty(arg))
						continue;

					arg = arg.Replace("\\n", "\n");
					args.Add(arg);
				}
			}

			command = command.ToLower();
			CommandMessage cmdMessage = new CommandMessage(command, message);

			if (args.Count == 1 && args[0] == "?")
			{
				Embed embed = await HelpService.GetHelp(cmdMessage);
				await message.Channel.SendMessageAsync(null, false, embed, messageReference: cmdMessage.MessageReference);
				return;
			}

			Log.Write("Received command: " + command + " with " + message.Content + " From user: " + message.Author.Id, "Bot");
			_ = Task.Run(async () => await this.RunCommand(command, args.ToArray(), cmdMessage));
		}

		private async Task RunCommand(string commandStr, string[] args, CommandMessage message)
		{
			if (Program.Initializing)
			{
				IUserMessage waitMessage = await message.Channel.SendMessageAsync("Just drinking my morning coffee... Give me a minute...");

				while (Program.Initializing)
				{
					await Task.Delay(1000);
				}

				await waitMessage.DeleteAsync();
			}

			if (commandHandlers.ContainsKey(commandStr))
			{
				if (message.Channel is not SocketTextChannel textChannel)
					return;

				using (textChannel.EnterTypingState())
				{
					Exception? lastException = null;
					foreach (Command command in commandHandlers[commandStr])
					{
						try
						{
							lastException = null;
							await command.Invoke(args, message);
							break;
						}
						catch (ParameterException ex)
						{
							lastException = ex;
						}
						catch (Exception ex)
						{
							lastException = ex;
							break;
						}
					}

					if (lastException != null)
					{
						if (lastException is UserException userEx)
						{
							await message.Channel.SendMessageAsync(userEx.Message);
						}
						else if (lastException is ParameterException paramEx)
						{
							await message.Channel.SendMessageAsync(paramEx.Message);
							await message.Channel.SendMessageAsync(null, false, await HelpService.GetHelp(message, commandStr));
						}
						else if (lastException is NotImplementedException)
						{
							IUserMessage sentMessage = await message.Channel.SendMessageAsync("I'm sorry, seems like I don't quite know how to do that yet.");

							// Clear user and bot message
							_ = this.ClearSentMessage(message.Message);
							_ = this.ClearSentMessage(sentMessage);
						}
						else if (lastException is WebException webEx)
						{
							HttpStatusCode? status = (webEx.Response as HttpWebResponse)?.StatusCode;
							IUserMessage sentMessage;
							if (status == null || status != HttpStatusCode.ServiceUnavailable)
							{
								// Send message to user
								sentMessage = await message.Channel.SendMessageAsync("I'm sorry, something went wrong while handling that.");

								// Log exception
								await Utils.Logger.LogExceptionToDiscordChannel(lastException, message);
							}
							else
							{
								sentMessage = await message.Channel.SendMessageAsync("I'm sorry, the service is unavailable right now.");
							}

							// Clear user and bot message
							_ = this.ClearSentMessage(message.Message);
							_ = this.ClearSentMessage(sentMessage);
						}
						else
						{
							// Send message to user
							IUserMessage sentMessage = await message.Channel.SendMessageAsync("I'm sorry, something went wrong while handling that.");

							// Log exception
							await Utils.Logger.LogExceptionToDiscordChannel(lastException, message);

							// Clear user and bot message
							_ = this.ClearSentMessage(message.Message);
							_ = this.ClearSentMessage(sentMessage);
						}
					}
				}
			}
			else
			{
				IUserMessage sentMessage = await message.Channel.SendMessageAsync("I'm sorry, I didn't understand that command.");

				// Clear user and bot message
				_ = this.ClearSentMessage(message.Message);
				_ = this.ClearSentMessage(sentMessage);
			}
		}

		private async Task ClearSentMessage(IMessage message, int delay = 5000)
		{
			await Task.Delay(delay);
			await message.DeleteAsync();
		}

		private async Task LogExceptionToDiscordChannel(CommandMessage message, Exception exception)
		{
			// Get Settings - check if both bot server and exception channel is given
			Settings settings = Settings.Load();
			if (!string.IsNullOrWhiteSpace(settings?.BotDiscordServer) && !string.IsNullOrWhiteSpace(settings?.BotLogExceptionsChannel))
			{
				// Get the guild
				SocketGuild kupoNutsGuild = this.DiscordClient.GetGuild(ulong.Parse(settings.BotDiscordServer));

				if (kupoNutsGuild == null)
					throw new Exception("Unable to access guild");

				// Post message
				string exceptionMessage = $"Server: {message.Guild.Name}\nUser: {message.Author.GetName()}\nMessage: {message.Message.Content}.\n`{exception}`";
				await kupoNutsGuild.GetTextChannel(ulong.Parse(settings.BotLogExceptionsChannel)).SendMessageAsync(exceptionMessage);
			}
		}
	}
}
