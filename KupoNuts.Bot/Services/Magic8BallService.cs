// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.Bot.Commands;

	public class Magic8BallService : ServiceBase
	{
		public static readonly List<string> Answers = new List<string>()
		{
			"It is certain.",
			"It is decidedly so.",
			"Without a doubt.",
			"Yes - definitely.",
			"You may rely on it.",
			"As I see it, yes.",
			"Most likely.",
			"Outlook good.",
			"Yes.",
			"Signs point to yes.",

			"Reply hazy, try again.",
			"Ask again later.",
			"Better not tell you now.",
			"Cannot predict now.",
			"Concentrate and ask again.",

			"Don't count on it.",
			"My reply is no.",
			"My sources say no.",
			"Outlook not so good.",
			"Very doubtful.",
		};

		[Command("8Ball", Permissions.Everyone, "Ask the magic 8 ball a question. be warned, you might not like the answer~")]
		public Task<Embed> Ask(string message)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = message;
			builder.Description = Answers.GetRandom();
			builder.Color = Color.DarkBlue;

			return Task.FromResult(builder.Build());
		}

		[Command("8Ball", Permissions.Everyone, "Ask the magic 8 ball a question. be warned, you might not like the answer~")]
		public Task<Embed> Ask()
		{
			return this.Ask("The Magic 8 Ball");
		}
	}
}
