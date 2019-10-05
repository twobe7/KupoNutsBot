// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System.Text;
	using Discord;

	public static class RPGStatusExtensions
	{
		public static Embed ToEmbed(this RPGStatus status, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();

			builder.Title = user.GetName();

			StringBuilder descBuilder = new StringBuilder();
			descBuilder.Append("Level: ");
			descBuilder.AppendLine(status.Level.ToString());

			descBuilder.Append(RPGService.NutEmoteStr);
			descBuilder.Append(" ");
			descBuilder.Append(status.Nuts);

			builder.Description = descBuilder.ToString();

			return builder.Build();
		}
	}
}
