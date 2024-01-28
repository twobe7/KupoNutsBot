// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Discord.Interactions;

using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// Requires the user invoking the command to have a specified permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireNameOrIdAttribute : PreconditionAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RequireNameOrIdAttribute"/> class.
	/// Requires that the user has provided either name or Id parameters.
	/// </summary>
	public RequireNameOrIdAttribute()
	{
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
	{
		bool hasServerName = false;
		bool hasCharacterName = false;

		if (context.Interaction is not SocketSlashCommand slashCommand)
		{
			return Task.FromResult(PreconditionResult.FromError("Interaction is not a slash command"));
		}

		var options = slashCommand.Data.Options.FirstOrDefault()?.Options;
		if (options == null)
		{
			return Task.FromResult(PreconditionResult.FromError("Requires either CharacterId or Server and CharacterName specified"));
		}

		foreach (var option in options)
		{
			switch (option.Name)
			{
				case "character-id":
					// Character Id specified, command will use id
					return Task.FromResult(PreconditionResult.FromSuccess());
				case "server-name":
					hasServerName = true; break;
				case "character-name":
					hasCharacterName = true; break;
				default:
					break;
			}
		}

		if (hasServerName && hasCharacterName)
		{
			// All conditions met for character search lookup
			return Task.FromResult(PreconditionResult.FromSuccess());
		}

		return Task.FromResult(PreconditionResult.FromError("Requires either CharacterId or Server and CharacterName specified"));
	}
}