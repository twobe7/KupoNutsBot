// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;

	public class ItemAutocompleteHandler : AutocompleteHandler
	{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			// Get search from user
			var search = autocompleteInteraction.Data.Options.FirstOrDefault(x => x.Name == parameter.Name)?.Value.ToString();

			// max - 25 suggestions at a time (API limit)
			var response = string.IsNullOrWhiteSpace(search)
				? Items.AutocompleteItems.Take(25)
				: Items.AutocompleteItems.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).Take(25);

			return AutocompletionResult.FromSuccess(response);
		}
	}
}
