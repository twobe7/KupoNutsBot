// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using KupoNuts.Bot.Commands;

	public class NoveltyService : ServiceBase
	{
		public static readonly List<string> Magic8BallAnswers = new List<string>()
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
			builder.Description = Magic8BallAnswers.GetRandom();
			builder.Color = Color.DarkBlue;

			return Task.FromResult(builder.Build());
		}

		[Command("8Ball", Permissions.Everyone, "Ask the magic 8 ball a question. be warned, you might not like the answer~")]
		public Task<Embed> Ask()
		{
			return this.Ask("The Magic 8 Ball");
		}

		[Command("Roll", Permissions.Everyone, "Roll the dice.")]
		public string Roll()
		{
			return this.Roll("1d6");
		}

		[Command("Roll", Permissions.Everyone, "Roll the dice. with the given format: 1d20")]
		public string Roll(string format)
		{
			string[] parts = format.Split('d', 'D');

			if (parts.Length != 2)
				throw new UserException("Invalid dice format! dice should be `[Number of Dice]d[number of faces]` like `1d20` or `2d6`");

			int count = 0;
			if (!int.TryParse(parts[0], out count))
				throw new UserException("I didn't udnerstand the dice number: \"" + parts[0] + "\", was that a number?");

			int faces = 0;
			if (!int.TryParse(parts[1], out faces))
				throw new UserException("I didn't udnerstand the dice faces: \"" + parts[1] + "\", was that a number?");

			if (count <= 0)
				throw new UserException("Number of dice has to be mroe than 0!");

			if (faces <= 0)
				throw new UserException("Number of faces has to be mroe than 0!");

			int total = 0;
			Random rn = new Random();
			for (int i = 0; i < count; i++)
			{
				total += rn.Next(faces) + 1;
			}

			return "You rolled: " + total.ToString();
		}
	}
}
