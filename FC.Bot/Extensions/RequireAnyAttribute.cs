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
public class RequireAnyAttribute : PreconditionAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RequireAnyAttribute"/> class.
	/// Requires that the user has provided any parameters.
	/// </summary>
	public RequireAnyAttribute()
	{
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
	{
		if (context.Interaction is not SocketSlashCommand slashCommand)
		{
			return Task.FromResult(PreconditionResult.FromError("Interaction is not a slash command"));
		}

		var options = slashCommand.Data.Options.FirstOrDefault()?.Options;
		if (options == null || options?.Count == 0)
		{
			return Task.FromResult(PreconditionResult.FromError("At least one parameter is required"));
		}

		// At least one options has been provided
		return Task.FromResult(PreconditionResult.FromSuccess());
	}
}