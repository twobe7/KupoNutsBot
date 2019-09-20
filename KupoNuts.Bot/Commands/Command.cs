// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading.Tasks;
	using Discord;
	using Discord.WebSocket;
	using KupoNuts.Bot.Services;

	public class Command
	{
		public readonly MethodInfo Method;
		public readonly Permissions Permission;
		public readonly string Help;
		public readonly WeakReference<object> Owner;

		public Command(MethodInfo method, object owner, Permissions permissions, string help)
		{
			this.Method = method;
			this.Permission = permissions;
			this.Help = help;
			this.Owner = new WeakReference<object>(owner);
		}

		public List<ParameterInfo> GetNeededParams()
		{
			List<ParameterInfo> results = new List<ParameterInfo>();

			ParameterInfo[] allParamInfos = this.Method.GetParameters();
			for (int i = 0; i < allParamInfos.Length; i++)
			{
				ParameterInfo paramInfo = allParamInfos[i];

				if (paramInfo.ParameterType == typeof(SocketMessage))
					continue;

				if (paramInfo.ParameterType == typeof(string[]))
					continue;

				results.Add(paramInfo);
			}

			return results;
		}

		public async Task Invoke(string[] args, SocketMessage message)
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

				if (paramInfo.ParameterType == typeof(SocketMessage))
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

			object? returnObject = this.Method.Invoke(owner, parameters.ToArray());

			if (returnObject is Task<string> tString)
			{
				string str = await tString;
				await message.Channel.SendMessageAsync(str);
			}
			else if (returnObject is Task<Embed> tEmbed)
			{
				Embed embed = await tEmbed;
				await message.Channel.SendMessageAsync(null, false, embed);
			}
			else if (returnObject is Task task)
			{
				await task;
			}
		}

		private async Task<object> Convert(SocketMessage message, string arg, Type type)
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
				return Emote.Parse(arg);
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
				IGuildUser user = await message.GetGuild().GetUserAsync(id);

				if (user == null)
					throw new Exception("Invalid user Id: " + arg);

				return user;
			}

			throw new Exception("Unsupported parameter type: \"" + type + "\"");
			#pragma warning restore
		}
	}
}
