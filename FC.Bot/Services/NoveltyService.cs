// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

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
	using Tenor;

	public class NoveltyService : ServiceBase
	{
		public const string KupoNut = @"<:kupo_nut:815575569482776607>";

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

		[Command("8Ball", Permissions.Everyone, "Ask the magic 8 ball a question. be warned, you might not like the answer~", CommandCategory.Novelty)]
		public Task<Embed> Ask(string message)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Title = message;
			builder.Description = Magic8BallAnswers.GetRandom();
			builder.Color = Color.DarkBlue;

			return Task.FromResult(builder.Build());
		}

		[Command("8Ball", Permissions.Everyone, "Ask the magic 8 ball a question. be warned, you might not like the answer~", CommandCategory.Novelty)]
		public Task<Embed> Ask()
		{
			return this.Ask("The Magic 8 Ball");
		}

		[Command("Roll", Permissions.Everyone, "Roll the dice.", CommandCategory.Novelty)]
		public string Roll()
		{
			return this.Roll("1d6");
		}

		[Command("Roll", Permissions.Everyone, "Roll the dice. with the given format: 1d20", CommandCategory.Novelty)]
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

		[Command("Choose", Permissions.Everyone, "Let me choose for you", CommandCategory.Novelty)]
		public string Choose(string optionA, string optionB)
		{
			return this.DoChoose(optionA, optionB, null, null);
		}

		[Command("Choose", Permissions.Everyone, "Let me choose for you", CommandCategory.Novelty)]
		public string Choose(string optionA, string optionB, string optionC)
		{
			return this.DoChoose(optionA, optionB, optionC, null);
		}

		[Command("Choose", Permissions.Everyone, "Let me choose for you", CommandCategory.Novelty)]
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

		[Command("Number", Permissions.Everyone, "Displays a random number between the given minimum (inclusive) and maximum (exclusive) values.", CommandCategory.Novelty)]
		public string Number(int max)
		{
			return this.Number(0, max);
		}

		[Command("Number", Permissions.Everyone, "Displays a random number between the given minimum (inclusive) and maximum (exclusive) values.", CommandCategory.Novelty)]
		public string Number(int min, int max)
		{
			if (max <= min)
				throw new UserException("Maximum must be greater than the Minimum!");

			Random rn = new Random();
			int value = rn.Next(min, max);

			return value + "!";
		}

		[Command("et", Permissions.Everyone, "Gets the current Eorzean Time", CommandCategory.XIVData, "EorzeaTime")]
		[Command("EorzeaTime", Permissions.Everyone, "Gets the current Eorzean Time", CommandCategory.XIVData)]
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

		[Command("Timers", Permissions.Everyone, "Display the FFXIV Reset Timers", CommandCategory.XIVData)]
		public void Timers(CommandMessage message)
		{
			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("FFXIV Timers");

			// Current time for timezone
			DateTime now = DateTime.Now;

			// Weekly Timer
			TimeSpan weeklyReset = new TimeSpan(16, 0, 0) - now.TimeOfDay;
			string weeklyResetFormat = string.Empty;

			if (now.DayOfWeek == DayOfWeek.Tuesday && weeklyReset.Hours > 0 && weeklyReset.Minutes > 0 && weeklyReset.Seconds > 0)
			{
				weeklyResetFormat = $"{weeklyReset.Hours.ToString().PadLeft(2, '0')}:{weeklyReset.Minutes.ToString().PadLeft(2, '0')}:{weeklyReset.Seconds.ToString().PadLeft(2, '0')}";
			}
			else
			{
				int daysUntilReset = 0;
				DateTime weeklyResetDay = now;

				if (weeklyResetDay.DayOfWeek == DayOfWeek.Monday && (weeklyReset.Hours < 0 || weeklyReset.Minutes < 0 || weeklyReset.Seconds < 0))
				{
					weeklyReset = new TimeSpan(1, 16, 0, 0) - now.TimeOfDay;
				}
				else
				{
					if (weeklyResetDay.DayOfWeek == DayOfWeek.Tuesday && (weeklyReset.Hours < 0 || weeklyReset.Minutes < 0 || weeklyReset.Seconds < 0))
						weeklyResetDay = weeklyResetDay.AddDays(1);

					while (weeklyResetDay.DayOfWeek != DayOfWeek.Tuesday)
					{
						daysUntilReset++;
						weeklyResetDay = weeklyResetDay.AddDays(1);
					}

					weeklyReset = new TimeSpan(daysUntilReset, 16, 0, 0) - now.TimeOfDay;
				}

				// Add days if required
				if (daysUntilReset != 0)
					weeklyResetFormat = $"{daysUntilReset} days, ";

				// Add hh:mm:ss to string
				weeklyResetFormat += $"{weeklyReset.Hours.ToString().PadLeft(2, '0')}:{weeklyReset.Minutes.ToString().PadLeft(2, '0')}:{weeklyReset.Seconds.ToString().PadLeft(2, '0')}";
			}

			embed.AddField(new EmbedFieldBuilder()
				.WithName("Weekly Reset").WithValue(weeklyResetFormat));

			// Daily Timer
			TimeSpan dailyReset = new TimeSpan(23, 0, 0) - now.TimeOfDay;
			if (dailyReset.Hours < 0 || dailyReset.Minutes < 0 || dailyReset.Seconds < 0)
				dailyReset = new TimeSpan(1, 23, 0, 0) - now.TimeOfDay;

			string dailyResetFormat = $"{dailyReset.Hours.ToString().PadLeft(2, '0')}:{dailyReset.Minutes.ToString().PadLeft(2, '0')}:{dailyReset.Seconds.ToString().PadLeft(2, '0')}";
			embed.AddField(new EmbedFieldBuilder()
				.WithName("Duty/Beast Tribe Daily Reset").WithValue(dailyResetFormat));

			// Grand Company Timer
			TimeSpan gcReset = new TimeSpan(4, 0, 0) - now.TimeOfDay;
			if (gcReset.Hours < 0 || gcReset.Minutes < 0 || gcReset.Seconds < 0)
				gcReset = new TimeSpan(1, 4, 0, 0) - now.TimeOfDay;

			string gcResetFormat = $"{gcReset.Hours.ToString().PadLeft(2, '0')}:{gcReset.Minutes.ToString().PadLeft(2, '0')}:{gcReset.Seconds.ToString().PadLeft(2, '0')}";
			embed.AddField(new EmbedFieldBuilder()
				.WithName("Grand Company Daily Reset").WithValue(gcResetFormat));

			// Post
			message.Channel.SendMessageAsync(embed: embed.Build(), messageReference: message.MessageReference);
		}

		[Command("Flip", Permissions.Everyone, "Flips user", CommandCategory.Novelty)]
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
								////.Replace("l", "l", true, null)
								.Replace("m", "ɯ", true, null)
								.Replace("n", "u", true, null)
								////.Replace("o", "o", true, null)
								.Replace("p", "d", true, null)
								.Replace("q", "b", true, null)
								.Replace("r", "ɹ", true, null)
								////.Replace("s", "s", true, null)
								.Replace("t", "ʇ", true, null)
								.Replace("u", "n", true, null)
								.Replace("v", "ʌ", true, null)
								.Replace("w", "ʍ", true, null)
								////.Replace("x", "x", true, null)
								.Replace("y", "ʎ", true, null);
								////.Replace("z", "z", true, null);

			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				return "I don't think so!\n\n(╯°□°）╯︵ " + name;
			}
			else
			{
				return "(╯°□°）╯︵ " + name;
			}
		}

		[Command(@"Unflip", Permissions.Everyone, "Unflips user", CommandCategory.Novelty)]
		public string Unflip(CommandMessage message, IGuildUser user)
		{
			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				return ":woman_gesturing_no:";
			}

			return user.GetName() + @" ノ( º _ ºノ)";
		}

		[Command("Hug", Permissions.Everyone, "Hugs a user", CommandCategory.Novelty)]
		public async Task<Embed> Hug(CommandMessage message, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = Color.DarkRed;

			if (message.Author.Id == user.Id)
			{
				builder.Title = string.Format("{0} {1}s themselves (kinda weird, _kupo_)", message.Author.GetName(), message.Command);
			}
			else
			{
				builder.Title = string.Format("{0} {1}s {2}", message.Author.GetName(), message.Command, user.GetName());
			}

			RandomAPI.Result tenorResult = await RandomAPI.Random("anime hug");
			builder.ImageUrl = tenorResult.GetBestUrl();

			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			return builder.Build();
			////return string.Format("{0} **{1}**", Hugs.GetRandom(), user.GetName());
		}

		[Command("Sarcasm", Permissions.Everyone, "makes text SaRcAsTiC", CommandCategory.Novelty)]
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

		[Command("Slap", Permissions.Everyone, "Slaps a user", CommandCategory.Novelty)]
		public async Task<Embed> Slap(CommandMessage message, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = Color.DarkRed;

			if (user.Id == Program.DiscordClient.CurrentUser.Id)
			{
				await message.Channel.SendMessageAsync("Nice try, _kupo!_");
				builder.Title = "Kupo Nuts slaps " + message.Author.GetName();
			}
			////else if (user.Id == 294055671396302858)
			////{
			////	await message.Channel.SendMessageAsync("Nice try, _kupo!_");
			////	builder.Title = "Kupo Nuts slaps " + message.Author.GetName();

			////	RandomAPI.Result honeyResult = await RandomAPI.Random("anime slap");
			////	builder.ImageUrl = honeyResult.GetBestUrl();

			////	return builder.Build();
			////}
			else if (message.Author.Id == user.Id)
			{
				builder.Title = message.Author.GetName() + " slaps themselves";
			}
			else
			{
				builder.Title = message.Author.GetName() + " slaps " + user.GetName();
			}

			RandomAPI.Result tenorResult = await RandomAPI.Random("anime slap");
			builder.ImageUrl = tenorResult.GetBestUrl();

			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			return builder.Build();
		}

		[Command("Pet", Permissions.Everyone, "Pet a user", CommandCategory.Novelty, "Pat")]
		[Command("Pat", Permissions.Everyone, "Pat a user", CommandCategory.Novelty)]
		public async Task<Embed> Pat(CommandMessage message, IGuildUser user)
		{
			EmbedBuilder builder = new EmbedBuilder();
			builder.Color = Color.DarkRed;

			if (message.Author.Id == user.Id)
			{
				builder.Title = string.Format("{0} {1}s themselves (kinda weird, _kupo_)", message.Author.GetName(), message.Command);
			}
			else
			{
				builder.Title = string.Format("{0} {1}s {2}", message.Author.GetName(), message.Command, user.GetName());
			}

			RandomAPI.Result tenorResult = await RandomAPI.Random("anime pat");
			builder.ImageUrl = tenorResult.GetBestUrl();

			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			return builder.Build();
		}

		[Command("Rate", Permissions.Everyone, "Rates a user", CommandCategory.Novelty)]
		public async Task Rate(CommandMessage message)
		{
			await this.Rate(message, message.Author);
		}

		[Command("Rate", Permissions.Everyone, "Rates a user", CommandCategory.Novelty)]
		public async Task Rate(CommandMessage message, IGuildUser user)
		{
			int rating = this.GenerateRatingForToday(user, null, 11);

			StringBuilder builder = new StringBuilder();

			builder.AppendLine(string.Format("{0}? I'd rate you {1}/10 kupo nuts, _kupo_!", user.GetName(), rating));
			builder.AppendLine();

			for (int i = 0; i < rating; i++)
			{
				builder.Append(KupoNut);
			}

			await message.Channel.SendMessageAsync(builder.ToString(), messageReference: message.MessageReference);
		}

		[Command("Ship", Permissions.Everyone, "Ships two users with a daily rating", CommandCategory.Novelty)]
		public async Task Ship(CommandMessage message, IGuildUser user)
		{
			await this.Ship(message, message.Author, user);
		}

		[Command("Ship", Permissions.Everyone, "Ships two users with a daily rating", CommandCategory.Novelty)]
		public async Task Ship(CommandMessage message, IGuildUser userA, IGuildUser userB)
		{
			int rating = this.GenerateRatingForToday(userA, userB, 101);

			string response = string.Empty;
			if (rating < 20)
			{
				response = "Oh... uhhh..";
			}
			else if (rating < 50)
			{
				response = "Hmm.. I think";
			}
			else if (rating < 70)
			{
				response = "It's obvious that";
			}
			else if (rating < 100)
			{
				response = "Pretty good..";
			}
			else
			{
				response = "Wow!";
			}

			string responseMsg = string.Format("{0} {1} and {2} are a {3}% match", response, userA.GetName(), userB.GetName(), rating);

			await message.Channel.SendMessageAsync(responseMsg, messageReference: message.MessageReference);
		}

		[Command("ShoutOut", Permissions.Everyone, @"Yeah, let's give a quick shout out to Christina Applegate", CommandCategory.Novelty)]
		public async void QuickShoutOut(CommandMessage message)
		{
			// Remove calling command
			await message.Channel.DeleteMessageAsync(message.Message);

			EmbedBuilder embed = new EmbedBuilder()
				.WithThumbnailUrl("https://cdn.discordapp.com/attachments/825936704023691284/830801754223935488/images_8.png")
				.WithTitle(@"Yeah, let's give a quick shout out to Christina Applegate");
				////.WithDescription(@"Yeah, let's give a quick shout out to Christina Applegate");

			await message.Channel.SendMessageAsync(embed: embed.Build());

			return;
		}

		private int GenerateRatingForToday(IGuildUser userA, IGuildUser? userB, int mod)
		{
			long now = DateTime.Now.Date.Ticks;

			decimal id = userA.Id / 100000M;
			decimal id2 = userB != null ? (userB.Id / 100000M) : 0;

			decimal step = now + id + id2;

			decimal rating = step / now;

			string n = rating.ToString();
			int num = int.Parse(n.Substring(n.Length - 2)) % mod;

			return num;
		}
	}
}
