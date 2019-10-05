// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System.Text;
	using Discord;
	using KupoNuts.RPG;

	public static class StatusExtensions
	{
		public static Embed ToEmbed(this Status status, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = user.GetName();

			StringBuilder descBuilder = new StringBuilder();
			descBuilder.Append("Level: ");
			descBuilder.Append(status.Level.ToString());
			descBuilder.Append(Utils.Characters.DoubleTab);

			descBuilder.Append(RPGService.NutEmoteStr);
			descBuilder.Append(" ");
			descBuilder.AppendLine(status.Nuts.ToString());
			descBuilder.AppendLine();

			builder.Description = descBuilder.ToString();

			return builder.Build();
		}
	}
}
