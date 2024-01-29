// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;

	public class EnumAutoCompleteHandler<TEnum> : AutocompleteHandler
	{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameterInfo, IServiceProvider services)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			// Get search from user
			var search = autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == parameterInfo.Name)?.Value.ToString();

			var names = Enum.GetNames(typeof(TEnum));
			var members = names
				.SelectMany(x => typeof(TEnum)
				.GetMember(x)).Where(x =>
					!x.IsDefined(typeof(HideAttribute), true)
					&& x.Name.Contains(search ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
				.Take(25);

			var choices = new List<AutocompleteResult>();

			foreach (var member in members)
			{
				var displayValue = member.GetCustomAttribute<ChoiceDisplayAttribute>()?.Name ?? member.Name;
				choices.Add(new AutocompleteResult
				{
					Name = displayValue,
					Value = member.Name,
				});
			}

			return AutocompletionResult.FromSuccess(choices);
		}
	}
}