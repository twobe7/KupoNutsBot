// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Services;

	public class Command
	{
		public readonly MethodInfo Method;
		public readonly Permissions Permission;
		public readonly string Help;
		public readonly WeakReference<object> Owner;

		private const int TaskTimeout = 30000;
		private const int EmbedThinkDelay = 500;
		private const string WaitEmoji = "<a:spinner:628526494637096970>";

		public Command(MethodInfo method, object owner, Permissions permissions, string help)
		{
			this.Method = method;
			this.Permission = permissions;
			this.Help = help;
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

			object? owner;
			if (!this.Owner.TryGetTarget(out owner) || owner == null)
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
				else
				{
					if (argCount >= args.Length)
					{
						parameters.Add("Empty");
						continue;
					}

					string arg = args[argCount];
					argCount++;

					object param;

					try
					{
						param = await this.Convert(message, arg, paramInfo.ParameterType);
					}
					catch (UserException ex)
					{
						throw ex;
					}
					catch (Exception)
					{
						string hint = string.Empty;

						if (paramInfo.ParameterType == typeof(string))
							hint = "\n(Strings should have \"quotes\" around them, Kupo!)";

						throw new ParameterException("I didn't understand the parameter: " + arg + ".\nWas that was supposed to be a " + HelpService.GetTypeName(paramInfo.ParameterType) + " for " + HelpService.GetParamName(paramInfo.Name) + "?" + hint);
					}

					parameters.Add(param);
				}
			}

			if (parameters.Count != allParamInfos.Length || argCount != args.Length || neededArgCount != args.Length)
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

			if (returnObject is Task<Embed> tEmbed)
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();
				await this.InvokeEmbedTask(message, tEmbed);
			}
			else if (returnObject is Task<string> tString)
			{
				string str = await tString;
				await message.Channel.SendMessageAsync(str);
			}
			else if (returnObject is Task<bool> tBool)
			{
				bool result = await tBool;
			}
			else if (returnObject is Task<(string, Embed)> tBoth)
			{
				(string msg, Embed embed) = await tBoth;
				await message.Channel.SendMessageAsync(msg, false, embed);
			}
			else if (returnObject is Task task)
			{
				await task;

				Random rn = new Random();
				string str = CommandsService.CommandResponses[rn.Next(CommandsService.CommandResponses.Count)];
				await message.Channel.SendMessageAsync(str);
			}
		}

		private async Task InvokeEmbedTask(CommandMessage message, Task<Embed> task)
		{
			// early out for instant tasks
			if (task.IsCompleted && !task.IsFaulted)
			{
				Embed embed = await task;
				await message.Channel.SendMessageAsync(null, false, embed);
				return;
			}

			Stopwatch sw = new Stopwatch();
			sw.Start();

			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < EmbedThinkDelay)
				await Task.Delay(10);

			RestUserMessage? thinkMessage = null;
			if (!task.IsCompleted && !task.IsFaulted)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Title = message.Message.Content;
				builder.Description = WaitEmoji;
				builder.ThumbnailUrl = "https://www.kuponutbrigade.com/wp-content/uploads/2019/10/think2.png";
				thinkMessage = await message.Channel.SendMessageAsync(null, false, builder.Build());

				// Discord doesn't like it when we edit embed to soon after posting them, as the edit
				// someints doesnt 'stick'.
				await Task.Delay(250);
			}

			while (!task.IsCompleted && !task.IsFaulted && sw.ElapsedMilliseconds < TaskTimeout)
				await Task.Delay(10);

			if (sw.ElapsedMilliseconds >= TaskTimeout && thinkMessage != null)
			{
				EmbedBuilder builder = new EmbedBuilder();
				builder.Description = "I'm sorry. I seem to have lost my train of thought...";

				await thinkMessage.ModifyAsync(x =>
				{
					x.Embed = builder.Build();
				});

				Log.Write("Task timeout: " + message.Message.Content, "Bot");
			}

			if (task.IsFaulted)
			{
				if (thinkMessage != null)
					await message.Channel.DeleteMessageAsync(thinkMessage);

				if (task.Exception != null)
				{
					if (task.Exception.InnerException != null)
						throw task.Exception.InnerException;

					throw task.Exception;
				}
				else
				{
					throw new Exception("Task failed");
				}
			}
			else
			{
				Embed embed = await task;

				if (thinkMessage != null)
				{
					await thinkMessage.ModifyAsync(x =>
					{
						x.Embed = embed;
					});
				}
				else
				{
					await message.Channel.SendMessageAsync(null, false, embed);
				}
			}
		}

		private async Task<object> Convert(CommandMessage message, string arg, Type type)
		{
			#pragma warning disable SA1121
			if (type == typeof(string))
			{
				if (!arg.Contains("\""))
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
				Emote emote = Emote.Parse(arg);

				if (!emote.IsAvailable())
					throw new UserException("I'm sorry, I dont have that emote.");

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

			throw new Exception("Unsupported parameter type: \"" + type + "\"");
			#pragma warning restore
		}
	}
}
