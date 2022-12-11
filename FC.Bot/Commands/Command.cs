// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.ExceptionServices;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using AngleSharp.Dom;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using FC.Bot.Services;
	using FC.Bot.Utils;
	using Newtonsoft.Json.Linq;
	using NodaTime;
	using NodaTime.Text;

	public class Command
	{
		public readonly MethodInfo Method;
		public readonly Permissions Permission;
		public readonly string CommandString;
		public readonly string Help;
		public readonly string CommandCategory;
		public readonly bool RequiresQuotes;
		public readonly bool ShowWait;
		public readonly WeakReference<object> Owner;
		public List<string>? Shortcuts;

		private const int TaskTimeout = 30000;
		private const int ThinkDelay = 500;
		private const string WaitEmoji = "<a:spinner:815087592822276117>";

		public Command(MethodInfo method, object owner, Permissions permissions, string commandString, string help, string commandCategory, bool requiresQuotes, bool showWait)
		{
			this.Method = method;
			this.Permission = permissions;
			this.CommandString = commandString.ToLower();
			this.Help = help;
			this.CommandCategory = commandCategory;
			this.RequiresQuotes = requiresQuotes;
			this.ShowWait = showWait;
			this.Owner = new WeakReference<object>(owner);

			RequestOptions.Default.RetryMode = RetryMode.AlwaysRetry;
		}

		public List<ParameterInfo> GetNeededParams()
		{
			List<ParameterInfo> results = new List<ParameterInfo>();

			ParameterInfo[] allParamInfos = this.Method.GetParameters();
			for (int i = 0; i < allParamInfos.Length; i++)
			{
				ParameterInfo paramInfo = allParamInfos[i];

				if (paramInfo.ParameterType == typeof(CommandMessage))
					continue;

				if (paramInfo.ParameterType == typeof(string[]))
					continue;

				results.Add(paramInfo);
			}

			return results;
		}

		public async Task Invoke(string[] args, CommandMessage message)
		{
			if (this.Permission > CommandsService.GetPermissions(message.Author))
				throw new UserException("I'm sorry, you don't have permission to do that.");

			if (!this.Owner.TryGetTarget(out object? owner) || owner == null)
				throw new Exception("Attempt to invoke command on null owner.");

			List<object> parameters = new List<object>();
			ParameterInfo[] allParamInfos = this.Method.GetParameters();

			int argCount = 0;
			int neededArgCount = allParamInfos.Length;
			for (int i = 0; i < allParamInfos.Length; i++)
			{
				ParameterInfo paramInfo = allParamInfos[i];

				if (paramInfo.ParameterType == typeof(CommandMessage))
				{
					parameters.Add(message);
					neededArgCount--;
					continue;
				}
				else if (paramInfo.ParameterType == typeof(string[]))
				{
					parameters.Add(args);
					neededArgCount--;
				}
				else if (paramInfo.ParameterType == typeof(Attachment) && message.Message.Attachments.Count == 1)
				{
					parameters.Add(message.Message.Attachments.First());
					neededArgCount--;
				}
				else
				{
					if (argCount >= args.Length)
					{
						parameters.Add("Empty");
						continue;
					}

					string arg;

					if (paramInfo.ParameterType == typeof(string)
						&& i == neededArgCount && args.Length > neededArgCount)
					{
						arg = string.Join(" ", args.Skip(neededArgCount - 1));
						argCount += args.Length - (neededArgCount - 1);
					}
					else
					{
						arg = args[argCount];
						argCount++;
					}

					object param;

					try
					{
						param = await this.Convert(message, arg, paramInfo.ParameterType, this.RequiresQuotes);
					}
					catch (UserException ex)
					{
						throw ex;
					}
					catch (Exception)
					{
						string hint = string.Empty;

						if (paramInfo.ParameterType == typeof(string))
							hint = "\n(Strings must have \"quotes\" around them)";

						throw new ParameterException("I didn't understand the parameter: " + arg + ".\nWas that was supposed to be a " + HelpService.GetTypeName(paramInfo.ParameterType) + " for " + HelpService.GetParam(paramInfo, this.RequiresQuotes) + "?" + hint);
					}

					parameters.Add(param);
				}
			}

			if (parameters.Count != allParamInfos.Length || argCount != args.Length || neededArgCount > args.Length)
				throw new ParameterException("Incorrect number of parameters. I was expecting " + neededArgCount + ", but you sent me " + args.Length + "!");

			try
			{
				await this.Invoke(message, owner, parameters.ToArray());
			}
			catch (UserException ex)
			{
				await message.Channel.SendMessageAsync(ex.Message);
			}
		}

		public async Task InvokeSlash(SocketSlashCommand command) // CommandMessage message, object? owner, )
		{
			object? returnObject;

			if (!this.Owner.TryGetTarget(out object? owner) || owner == null)
				throw new Exception("Attempt to invoke command on null owner.");

			try
			{
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
				returnObject = this.Method.Invoke(owner, null);
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException == null)
					throw ex;

				throw (Exception)ex.InnerException;
			}

			if (returnObject is null)
				return;

			if (returnObject is string rString)
			{
				await command.Channel.SendMessageAsync(rString);
				return;
			}
			else if (returnObject is Task task)
			{
				await this.HandleSlashTask(command, task);
				return;
			}

			throw new Exception("Unknown command return type: " + returnObject.GetType());
		}

		private async Task Invoke(CommandMessage message, object? owner, object[] param)
		{
			object? returnObject;

			try
			{
				returnObject = this.Method.Invoke(owner, param);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException == null)
					throw ex;

				throw (Exception)ex.InnerException;
			}

			if (returnObject is null)
				return;

			if (returnObject is string rString)
			{
				await message.Channel.SendMessageAsync(rString);
				return;
			}
			else if (returnObject is Task task)
			{
				await this.HandleTask(message, task);
				return;
			}

			throw new Exception("Unknown command return type: " + returnObject.GetType());
		}

		private async Task HandleTask(CommandMessage message, Task task)
		{
			// early out for instant tasks
			if (task.IsCompleted && !task.IsFaulted)
			{
				await task;
				await this.HandleTaskResult(message, null, task);
				return;
			}

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// if we take too long, post the think message.
			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < ThinkDelay)
				await Task.Delay(10);

			RestUserMessage? thinkMessage = null;
			if (this.ShowWait && !task.IsCompleted && !task.IsFaulted)
			{
				EmbedBuilder builder = new EmbedBuilder
				{
					Title = message.Message.Content.Truncate(256),
					Description = WaitEmoji,
				};
				thinkMessage = await message.Channel.SendMessageAsync(null, false, builder.Build(), messageReference: message.MessageReference);

				// Discord doesn't like it when we edit embed to soon after posting them, as the edit
				// sometimes doesn't 'stick'.
				await Task.Delay(250);
			}

			// If we take way too long, post an abort message.
			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < TaskTimeout)
				await Task.Delay(10);

			if (sw.ElapsedMilliseconds >= TaskTimeout && thinkMessage != null)
			{
				await thinkMessage.ModifyAsync(x =>
				{
					x.Content = "I'm sorry. I seem to have lost my train of thought...";
					x.Embed = null;
				});

				Log.Write("Task timeout: " + message.Message.Content, "Bot");
				return;
			}

			// Handle tasks that have gone poorly.
			if (task.IsFaulted)
			{
				if (thinkMessage != null)
					await message.Channel.DeleteMessageAsync(thinkMessage);

				if (task.Exception != null)
				{
					Exception ex = task.Exception;

					if (task.Exception.InnerException != null)
						ex = task.Exception.InnerException;

					ExceptionDispatchInfo.Capture(ex).Throw();
				}
				else
				{
					throw new Exception("Task failed");
				}
			}

			await this.HandleTaskResult(message, thinkMessage, task);
		}

		private async Task HandleSlashTask(SocketSlashCommand command, Task task)
		{
			// early out for instant tasks
			if (task.IsCompleted && !task.IsFaulted)
			{
				await task;
				await this.HandleSlashTaskResult(command, null, task);
				return;
			}

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// if we take too long, post the think message.
			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < ThinkDelay)
				await Task.Delay(10);

			RestUserMessage? thinkMessage = null;
			if (this.ShowWait && !task.IsCompleted && !task.IsFaulted)
			{
				EmbedBuilder builder = new EmbedBuilder
				{
					Title = command.Data.Name,
					Description = WaitEmoji,
				};
				thinkMessage = await command.Channel.SendMessageAsync(null, false, builder.Build());

				// Discord doesn't like it when we edit embed to soon after posting them, as the edit
				// sometimes doesn't 'stick'.
				await Task.Delay(250);
			}

			// If we take way too long, post an abort message.
			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < TaskTimeout)
				await Task.Delay(10);

			if (sw.ElapsedMilliseconds >= TaskTimeout && thinkMessage != null)
			{
				await thinkMessage.ModifyAsync(x =>
				{
					x.Content = "I'm sorry. I seem to have lost my train of thought...";
					x.Embed = null;
				});

				Log.Write("Task timeout: " + command.Data.Name, "Bot");
				return;
			}

			// Handle tasks that have gone poorly.
			if (task.IsFaulted)
			{
				if (thinkMessage != null)
					await command.Channel.DeleteMessageAsync(thinkMessage);

				if (task.Exception != null)
				{
					Exception ex = task.Exception;

					if (task.Exception.InnerException != null)
						ex = task.Exception.InnerException;

					ExceptionDispatchInfo.Capture(ex).Throw();
				}
				else
				{
					throw new Exception("Task failed");
				}
			}

			await this.HandleSlashTaskResult(command, thinkMessage, task);
		}

		private async Task HandleTaskResult(CommandMessage message, RestUserMessage? editMessage, Task task)
		{
			string? resultMessage = null;
			Embed? resultEmbed = null;

			if (task is Task<Embed> embedTask)
			{
				resultEmbed = embedTask.Result;
			}
			else if (task is Task<string> stringTask)
			{
				resultMessage = stringTask.Result;
			}
			else if (task is Task<(string, Embed)> bothTask)
			{
				(string taskMessage, Embed taskEmbed) = bothTask.Result;
				resultMessage = taskMessage;
				resultEmbed = taskEmbed;
			}

			if (editMessage != null)
			{
				if (resultMessage == null && resultEmbed == null)
				{
					await editMessage.DeleteAsync();
				}
				else
				{
					await editMessage.ModifyAsync(x =>
					{
						x.Content = resultMessage;
						x.Embed = resultEmbed;
					});
				}

				return;
			}
			else
			{
				if (resultMessage != null || resultEmbed != null)
				{
					await message.Channel.SendMessageAsync(resultMessage, false, resultEmbed);
				}
			}
		}

		private async Task HandleSlashTaskResult(SocketSlashCommand command, RestUserMessage? editMessage, Task task)
		{
			string? resultMessage = null;
			////Embed[] embeds;
			Embed? resultEmbed = null;

			if (task is Task<Embed> embedTask)
			{
				resultEmbed = embedTask.Result;
			}
			else if (task is Task<string> stringTask)
			{
				resultMessage = stringTask.Result;
			}
			else if (task is Task<(string, Embed)> bothTask)
			{
				(string taskMessage, Embed taskEmbed) = bothTask.Result;
				resultMessage = taskMessage;
				resultEmbed = taskEmbed;
			}

			if (editMessage != null)
			{
				if (resultMessage == null && resultEmbed == null)
				{
					await editMessage.DeleteAsync();
				}
				else
				{
					await editMessage.ModifyAsync(x =>
					{
						x.Content = resultMessage;
						x.Embed = resultEmbed;
					});
				}

				return;
			}
			else
			{
				if (resultMessage != null || resultEmbed != null)
				{
					await command.RespondAsync(resultMessage, resultEmbed != null ? new[] { resultEmbed } : null, false);
				}
			}
		}

		private async Task<object> Convert(CommandMessage message, string arg, Type type, bool stringRequiresQuotes = true)
		{
#pragma warning disable SA1121, IDE0049
			if (type == typeof(string))
			{
				if (stringRequiresQuotes && !arg.Contains("\""))
					throw new Exception("strings must be wrapped in quotations");

				return arg.Replace("\"", string.Empty);
			}
			else if (type == typeof(double))
			{
				return double.Parse(arg);
			}
			else if (type == typeof(int))
			{
				return int.Parse(arg);
			}
			else if (type == typeof(uint))
			{
				return uint.Parse(arg);
			}
			else if (type == typeof(UInt64))
			{
				return UInt64.Parse(arg);
			}
			else if (type == typeof(ulong))
			{
				return ulong.Parse(arg);
			}
			else if (type == typeof(bool))
			{
				return bool.Parse(arg);
			}
			else if (type == typeof(SocketTextChannel))
			{
				string str = arg;
				str = str.Replace("<", string.Empty);
				str = str.Replace(">", string.Empty);
				str = str.Replace("#", string.Empty);

				ulong id = ulong.Parse(str);
				SocketChannel channel = Program.DiscordClient.GetChannel(id);
				if (channel is SocketTextChannel)
				{
					return channel;
				}
				else if (channel is null)
				{
					throw new Exception("Invalid channel ID: " + id);
				}
				else
				{
					throw new Exception("Channel is not a Text Channel");
				}
			}
			else if (type == typeof(IEmote))
			{
				IEmote emote = EmoteUtility.Parse(arg);

				if (emote is Emote emoteActual && !emoteActual.IsAvailable())
					throw new UserException("I'm sorry, I don't have that emote.");

				return emote;
			}
			else if (type == typeof(IUser))
			{
				string str = arg;
				str = str.Replace("<", string.Empty);
				str = str.Replace(">", string.Empty);
				str = str.Replace("@", string.Empty);
				str = str.Replace("!", string.Empty);

				ulong id = ulong.Parse(str);
				IUser user = Program.DiscordClient.GetUser(id);

				if (user == null)
					throw new Exception("Invalid user Id: " + arg);

				return user;
			}
			else if (type == typeof(IGuildUser))
			{
				string str = arg;
				str = str.Replace("<", string.Empty);
				str = str.Replace(">", string.Empty);
				str = str.Replace("@", string.Empty);
				str = str.Replace("!", string.Empty);

				ulong id = ulong.Parse(str);
				IGuildUser user = await message.Guild.GetUserAsync(id);

				if (user == null)
					throw new Exception("Invalid user Id: " + arg);

				return user;
			}
			else if (type == typeof(IRole))
			{
				// <@&663326776696504352>
				string str = arg;
				str = str.Replace("<@&", string.Empty);
				str = str.Replace(">", string.Empty);

				ulong id = ulong.Parse(str);
				IRole role = message.Guild.GetRole(id);

				if (role == null)
					throw new Exception("Invalid role Id: " + arg);

				return role;
			}
			else if (type == typeof(Duration))
			{
				string str = arg.ToLower();
				string[] parts = Regex.Split(str, @"(?<=[dhms])");

				Duration duration = Duration.FromSeconds(0);

				foreach (string part in parts)
				{
					if (part.Contains('d'))
					{
						int val = int.Parse(part.Replace('d', '\0'));
						duration += Duration.FromDays(val);
					}
					else if (part.Contains('h'))
					{
						int val = int.Parse(part.Replace('h', '\0'));
						duration += Duration.FromHours(val);
					}
					else if (part.Contains('m'))
					{
						int val = int.Parse(part.Replace('m', '\0'));
						duration += Duration.FromMinutes(val);
					}
					else if (part.Contains('s'))
					{
						int val = int.Parse(part.Replace('s', '\0'));
						duration += Duration.FromSeconds(val);
					}
				}

				return duration;
			}

			throw new Exception("Unsupported parameter type: \"" + type + "\"");
#pragma warning restore
		}
	}
}
