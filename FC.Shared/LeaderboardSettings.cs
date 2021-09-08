// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using FC.Attributes;

	public class LeaderboardSettings : SettingsEntry
	{
		/// <summary>
		/// Gets or sets a Discord Role that can give rep to User. 0 for disabled, 1 for all roles.
		/// </summary>
		[InspectorRole]
		public string ReputationAddRole { get; set; } = "1";

		/// <summary>
		/// Gets or sets a Discord Role that can remove rep from User. 0 for disabled, 1 for all roles.
		/// </summary>
		[InspectorRole]
		public string ReputationRemoveRole { get; set; } = "0";

		/// <summary>
		/// Gets or sets a value indicating whether Rep can only be given once per day or is unlimited.
		/// </summary>
		[InspectorCheckBox]
		public bool LimitReputationPerDay { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether Currency can be gained at random from messages.
		/// </summary>
		[InspectorCheckBox]
		public bool AllowPassiveCurrencyGain { get; set; } = true;

		/// <summary>
		/// Gets or sets number of currency games (Slots, Blackjack, etc) that can be played per day per user.
		/// -1 allows unlimited games (cooldown applies).
		/// </summary>
		[InspectorTooltip("Set to -1 or 2880 for unlimited games (cooldown applies).")]
		[Range(-1, 2880)]
		public int CurrencyGamesAllowedPerDay { get; set; } = 2880;
	}
}
