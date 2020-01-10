// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using FC.Bot.Commands;
	using NodaTime;
	using NodaTime.Extensions;

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

		public static readonly List<string> Hugs = new List<string>()
		{
			@"(づ｡◕‿‿◕｡)づ",
			@"(づ￣ ³￣)づ",
			@"(っ`▽｀)っ",
			@"(っಠ‿ಠ)っ",
			@"(.づ◡﹏◡)づ.",
			@"(っ˘̩╭╮˘̩)っ",
			@"＼(^o^)／",
			@"(つ◉益◉)つ",
			@"(oﾟ▽ﾟ)o",
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
				throw new UserException("I didn't understand the dice number: \"" + parts[0] + "\", was that a number?");

			int faces = 0;
			if (!int.TryParse(parts[1], out faces))
				throw new UserException("I didn't understand the dice faces: \"" + parts[1] + "\", was that a number?");

			if (count <= 0)
				throw new UserException("Number of dice has to be more than 0!");

			if (faces <= 0)
				throw new UserException("Number of faces has to be more than 0!");

			int total = 0;
			Random rn = new Random();
			for (int i = 0; i < count; i++)
			{
				total += rn.Next(faces) + 1;
			}

			return "You rolled: " + total.ToString();
		}

		[Command("Choose", Permissions.Everyone, "Let me choose for you")]
		public string Choose(string optionA, string optionB)
		{
			return this.DoChoose(optionA, optionB, null, null);
		}

		[Command("Choose", Permissions.Everyone, "Let me choose for you")]
		public string Choose(string optionA, string optionB, string optionC)
		{
			return this.DoChoose(optionA, optionB, optionC, null);
		}

		[Command("Choose", Permissions.Everyone, "Let me choose for you")]
		public string Choose(string optionA, string optionB, string optionC, string optionD)
		{
			return this.DoChoose(optionA, optionB, optionC, optionD);
		}

		public string DoChoose(string optionA, string optionB, string? optionC, string? optionD)
		{
			int count = 2;
			if (!string.IsNullOrEmpty(optionC))
				count = 3;

			if (!string.IsNullOrEmpty(optionD))
				count = 4;

			Random rn = new Random();
			int value = rn.Next(count);

			switch (value)
			{
				case 0: return "I choose... " + optionA + "!";
				case 1: return "I choose... " + optionB + "!";
				case 2: return "I choose... " + optionC + "!";
				case 3: return "I choose... " + optionD + "!";
			}

			throw new Exception("Failed to choose a valid option");
		}

		[Command("Number", Permissions.Everyone, "Displays a random number between the given minimum (inclusive) and maximum (exclusive) values.")]
		public string Number(int max)
		{
			return this.Number(0, max);
		}

		[Command("Number", Permissions.Everyone, "Displays a random number between the given minimum (inclusive) and maximum (exclusive) values.")]
		public string Number(int min, int max)
		{
			if (max <= min)
				throw new UserException("Maximum must be greater than the Minimum!");

			Random rn = new Random();
			int value = rn.Next(min, max);

			return value + "!";
		}

		[Command("et", Permissions.Everyone, "Gets the current Eorzean Time")]
		[Command("EorzeaTime", Permissions.Everyone, "Gets the current Eorzean Time")]
		public string EorzeanTime()
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);
			double eorzeaConstant = 20.571428571428573;

			// time is off by 19 minutes on windows, but not Linux...
			double offset = 19 * 60;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				offset = 0;

			double timeSeconds = (DateTime.Now.ToUniversalTime() - epoch).TotalSeconds;
			double eorzeaSeconds = (timeSeconds * eorzeaConstant) + offset;
			DateTime et = epoch + TimeSpan.FromSeconds(eorzeaSeconds);

			return "It is currently: " + et.ToString("HH:mm");
		}

		[Command("Flip", Permissions.Everyone, "Flips user")]
		public string Flip(CommandMessage message, IGuildUser user)
		{
			string flipName = string.Empty;

			// Trying to flip the bot
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				flipName = message.Author.GetName();
			}
			else
			{
				flipName = user.GetName();
			}

			// Reverse string
			char[] array = flipName.ToCharArray();
			Array.Reverse(array);

			string name = new string(array)
								.Replace("a", "ɐ", true, null)
								.Replace("b", "q", true, null)
								.Replace("c", "ɔ", true, null)
								.Replace("d", "p", true, null)
								.Replace("e", "ǝ", true, null)
								.Replace("f", "ɟ", true, null)
								.Replace("g", "ƃ", true, null)
								.Replace("h", "ɥ", true, null)
								.Replace("i", "ᴉ", true, null)
								.Replace("j", "ɾ", true, null)
								.Replace("k", "ʞ", true, null)
								.Replace("l", "l", true, null)
								.Replace("m", "ɯ", true, null)
								.Replace("n", "u", true, null)
								.Replace("o", "o", true, null)
								.Replace("p", "d", true, null)
								.Replace("q", "b", true, null)
								.Replace("r", "ɹ", true, null)
								.Replace("s", "s", true, null)
								.Replace("t", "ʇ", true, null)
								.Replace("u", "n", true, null)
								.Replace("v", "ʌ", true, null)
								.Replace("w", "ʍ", true, null)
								.Replace("x", "x", true, null)
								.Replace("y", "ʎ", true, null)
								.Replace("z", "z", true, null);

			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				return "I don't think so!\n\n(╯°□°）╯︵ " + name;
			}
			else
			{
				return "(╯°□°）╯︵ " + name;
			}
		}

		[Command(@"Unflip", Permissions.Everyone, @"Unflips user")]
		public string Unflip(CommandMessage message, IGuildUser user)
		{
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				return ":woman_gesturing_no:";
			}

			return user.GetName() + @" ノ( º _ ºノ)";
		}

		[Command("Hug", Permissions.Everyone, "Hugs a user")]
		public string Hug(CommandMessage message, IGuildUser user)
		{
			return string.Format("{0} **{1}**", Hugs.GetRandom(), user.GetName());
		}

		[Command("sarcasm", Permissions.Everyone, "makes text SaRcAsTiC")]
		public Task<string> Sarcasm(CommandMessage message, string text)
		{
			char[] characters = new char[text.Length];
			bool upper = true;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				characters[i] = upper ? char.ToUpper(c) : char.ToLower(c);
				upper = !upper;
			}

			text = new string(characters);
			return Task.FromResult(text);
		}
	}
}
