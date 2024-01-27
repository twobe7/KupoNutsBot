// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Interactions;
	using Discord.WebSocket;
	using FC.Bot.Commands;
	using Tenor;

	[Group("novelty", "Pass the time")]
	public class NoveltyService : ServiceBase
	{
		public const string KupoNut = @"<:kupo_nut:815575569482776607>";

		public static readonly List<string> Magic8BallAnswers = new ()
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

		public static readonly List<string> Hugs = new ()
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

		public static readonly List<(string weather, int chance)> WeatherRates = new ()
		{
			("Clear Skies", 25),
			("Fair Skies", 45),
			("Clouds", 10),
			("Rain", 10),
			("Fog", 5),
			("Showers", 5),
		};

		public readonly DiscordSocketClient DiscordClient;

		public NoveltyService(DiscordSocketClient discordClient)
		{
			this.DiscordClient = discordClient;
		}

		[SlashCommand("8ball", "Ask the magic 8 ball a question. be warned, you might not like the answer")]
		public async Task Ask(string message)
		{
			EmbedBuilder builder = new ()
			{
				Title = message,
				Description = Magic8BallAnswers.GetRandom(),
				Color = Color.DarkBlue,
			};

			await this.RespondAsync(embeds: new Embed[] { builder.Build() });
		}

		[SlashCommand("blame", "Blames someone")]
		public async Task Blame()
		{
			await this.DeferAsync();

			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			if (this.Context.Channel is SocketGuildChannel guildChannel)
			{
				int val = new Random().Next(guildChannel.Guild.Users.Count);
				IGuildUser target = guildChannel.Guild.Users.ElementAt(val);

				if (target.Id == this.DiscordClient.CurrentUser.Id)
					await this.FollowupAsync("This is my fault. =(");

				await this.FollowupAsync("This is your fault, " + target.GetName() + ".");
			}

			await this.FollowupAsync("This is your fault, " + guildUser.GetName() + ".");
		}

		[SlashCommand("roll", "Roll the dice. Format: 1d20")]
		public async Task Roll(string format = "1d6")
		{
			if (format.ToLower() == "housing")
			{
				// Post meme message
				await this.RespondAsync(text: "You rolled a...0. May you have better luck next time.");
				return;
			}

			string[] parts = format.Split('d', 'D');

			if (parts.Length != 2)
				throw new UserException("Invalid dice format! Dice should be `[Number of Dice]d[number of faces]` like `1d20` or `2d6`");

			if (!int.TryParse(parts[0], out int count))
				throw new UserException("I didn't understand the dice number: \"" + parts[0] + "\", was that a number?");

			if (!int.TryParse(parts[1], out int faces))
				throw new UserException("I didn't understand the dice faces: \"" + parts[1] + "\", was that a number?");

			if (count <= 0)
				throw new UserException("Number of dice has to be more than 0!");

			if (faces <= 0)
				throw new UserException("Number of faces has to be more than 0!");

			int total = 0;
			Random rn = new ();
			for (int i = 0; i < count; i++)
			{
				total += rn.Next(faces) + 1;
			}

			await this.RespondAsync($"You rolled: {total}");
		}

		[SlashCommand("choose", "Let me choose for you")]
		public async Task Choose(string optionA, string optionB, string? optionC = null, string? optionD = null)
		{
			var options = new List<string>() { optionA, optionB };

			if (!string.IsNullOrWhiteSpace(optionC))
				options.Add(optionC);

			if (!string.IsNullOrWhiteSpace(optionD))
				options.Add(optionD);

			int value = new Random().Next(options.Count);

			await this.RespondAsync($"I choose... {options[value]}!");
		}

		[SlashCommand("number", "Displays a random number between the given minimum (inclusive) and maximum (exclusive) values.")]
		public async Task Number(int min = 0, int max = 1)
		{
			if (max <= min)
				throw new UserException("Maximum must be greater than the Minimum!");

			await this.RespondAsync($"{new Random().Next(min, max)}!");
		}

		[SlashCommand("eorzeatime", "Gets the current Eorzean Time")]
		public async Task EorzeanTime()
		{
			await this.RespondAsync($"It is currently: {GetEorzeanTime():HH:mm}");
		}

		[SlashCommand("timers", "Display the FFXIV Reset Timers")]
		public async Task Timers()
		{
			EmbedBuilder embed = new EmbedBuilder()
				.WithTitle("FFXIV Timers");

			// Current time
			DateTime now = DateTime.UtcNow;
			int weeklyResetHourUtc = 8;
			int dailyResetHourUtc = 15;
			int gcResetHourUtc = 20;

			// Weekly Timer
			TimeSpan weeklyReset;
			string weeklyResetFormat = string.Empty;

			if (now.DayOfWeek == DayOfWeek.Tuesday && now.Hour >= weeklyResetHourUtc)
			{
				weeklyReset = now.Date.AddDays(7).AddHours(weeklyResetHourUtc) - now;
			}
			else
			{
				int daysUntilReset = 0;
				DateTime weeklyResetDay = now;

				while (weeklyResetDay.DayOfWeek != DayOfWeek.Tuesday)
				{
					weeklyResetDay = weeklyResetDay.AddDays(1);
					daysUntilReset++;
				}

				weeklyReset = now.Date.AddDays(daysUntilReset).AddHours(weeklyResetHourUtc) - now;
			}

			// Add days if required
			if (weeklyReset.Days > 0)
				weeklyResetFormat = $"{weeklyReset.Days} days, ";

			// Add hh:mm:ss to string
			weeklyResetFormat += $"{PadLeft(weeklyReset.Hours)}:{PadLeft(weeklyReset.Minutes)}:{PadLeft(weeklyReset.Seconds)}";

			embed.AddField(new EmbedFieldBuilder()
				.WithName("Weekly Reset").WithValue(weeklyResetFormat));

			// Daily Timer
			TimeSpan dailyReset = new TimeSpan(dailyResetHourUtc, 0, 0) - now.TimeOfDay;
			if (dailyReset.Hours < 0 || dailyReset.Minutes < 0 || dailyReset.Seconds < 0)
				dailyReset = new TimeSpan(1, dailyResetHourUtc, 0, 0) - now.TimeOfDay;

			string dailyResetFormat = $"{PadLeft(dailyReset.Hours)}:{PadLeft(dailyReset.Minutes)}:{PadLeft(dailyReset.Seconds)}";
			embed.AddField(new EmbedFieldBuilder()
				.WithName("Duty/Beast Tribe Daily Reset").WithValue(dailyResetFormat));

			// Grand Company Timer
			TimeSpan gcReset = new TimeSpan(gcResetHourUtc, 0, 0) - now.TimeOfDay;
			if (gcReset.Hours < 0 || gcReset.Minutes < 0 || gcReset.Seconds < 0)
				gcReset = new TimeSpan(1, gcResetHourUtc, 0, 0) - now.TimeOfDay;

			string gcResetFormat = $"{PadLeft(gcReset.Hours)}:{PadLeft(gcReset.Minutes)}:{PadLeft(gcReset.Seconds)}";
			embed.AddField(new EmbedFieldBuilder()
				.WithName("Grand Company Daily Reset").WithValue(gcResetFormat));

			// Post
			await this.RespondAsync(embed: embed.Build());
		}

		[SlashCommand("flip-user", "Flips user")]
		public async Task Flip(SocketGuildUser user)
		{
			if (this.Context.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			// Handle attempt to flip the bot
			string flipName = user.Id == this.DiscordClient.CurrentUser.Id
				? guildUser.GetName()
				: user.GetName();

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
								.Replace("m", "ɯ", true, null)
								.Replace("n", "u", true, null)
								.Replace("p", "d", true, null)
								.Replace("q", "b", true, null)
								.Replace("r", "ɹ", true, null)
								.Replace("t", "ʇ", true, null)
								.Replace("u", "n", true, null)
								.Replace("v", "ʌ", true, null)
								.Replace("w", "ʍ", true, null)
								.Replace("y", "ʎ", true, null);

			var response = user.Id == this.DiscordClient.CurrentUser.Id
				? "I don't think so!\n\n(╯°□°）╯︵ " + name
				: "(╯°□°）╯︵ " + name;

			await this.RespondAsync(response);
		}

		[SlashCommand("unflip-user", "Unflips user")]
		public string Unflip(SocketGuildUser user)
		{
			if (user.Id == this.DiscordClient.CurrentUser.Id)
				return ":woman_gesturing_no:";

			return user.GetName() + @" ノ( º _ ºノ)";
		}

		[SlashCommand("sarcasm", "makes text SaRcAsTiC")]
		public async Task Sarcasm(string text)
		{
			char[] characters = new char[text.Length];
			bool upper = true;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				characters[i] = upper ? char.ToUpper(c) : char.ToLower(c);
				upper = !upper;
			}

			await this.RespondAsync(new string(characters));
		}

		[SlashCommand("hug", "Hugs a user")]
		public async Task Hug(SocketGuildUser user)
		{
			await this.DeferAsync();
			await FollowUpTenorResult(this.Context, "anime hug", user);
		}

		[SlashCommand("spray", "Sprays a user")]
		public async Task Spray(SocketGuildUser user)
		{
			await this.DeferAsync();
			await FollowUpTenorResult(this.Context, "spraywater", user);
		}

		[SlashCommand("slap", "Slaps a user")]
		public async Task Slap(SocketGuildUser user)
		{
			await this.DeferAsync();
			await FollowUpTenorResult(this.Context, "anime slap", user);
		}

		[SlashCommand("pet", "Pet a user")]
		public async Task Pat(SocketGuildUser user)
		{
			await this.DeferAsync();
			await FollowUpTenorResult(this.Context, "anime pat", user);
		}

		[SlashCommand("rate", "Rates a user")]
		public async Task Rate(SocketGuildUser? user = null)
		{
			if (this.Context.User is SocketGuildUser guildUser)
				user ??= guildUser;

			if (user == null)
				return;

			int rating = GenerateRatingForToday(user, null, 11);

			StringBuilder builder = new StringBuilder()
				.AppendLine($"{user.GetName()}? I'd rate you {rating}/10 kupo nuts, _kupo_!")
				.AppendLine();

			for (int i = 0; i < rating; i++)
			{
				builder.Append(KupoNut);
			}

			await this.RespondAsync(builder.ToString());
		}

		[SlashCommand("ship", "Ships two users with a daily rating")]
		public async Task Ship(SocketGuildUser userA, SocketGuildUser? userB = null)
		{
			if (this.Context.User is SocketGuildUser guildUser)
				userB ??= guildUser;

			int rating = GenerateRatingForToday(userA, userB, 101);

			string response = rating switch
			{
				< 20 => "Oh... uhhh..",
				< 50 => "Hmm.. I think",
				< 70 => "It's obvious that",
				< 95 => "Pretty good..",
				_ => "Wow!",
			};

			string responseMsg = $"{response} {userA.GetName()} and {userB?.GetName()} are a {rating}% match";
			await this.RespondAsync(responseMsg);
		}

		[Command("ShoutOut", Permissions.Everyone, @"Yeah, let's give a quick shout out to Christina Applegate", CommandCategory.Novelty)]
		public async void QuickShoutOut(CommandMessage message)
		{
			// Remove calling command
			await message.DeleteMessage();

			EmbedBuilder embed = new EmbedBuilder()
				.WithThumbnailUrl("https://cdn.discordapp.com/attachments/825936704023691284/830801754223935488/images_8.png")
				.WithTitle(@"Yeah, let's give a quick shout out to Christina Applegate");

			await message.Channel.SendMessageAsync(embed: embed.Build());

			return;
		}

		/// <summary>
		/// Returns the current weather and time of Island Sanctuary.
		/// </summary>
		/// <remarks>Data from https://ffxiv.pf-n.co/skywatcher.</remarks>
		[SlashCommand("island-santuary-weather", "Get Island Sanctuary Weather")]
		public async Task GetIslandSantuaryWeather()
		{
			await this.DeferAsync();

			var currentWeather = "Unknown";
			var upcomingWeather = "Unknown";
			var currentTime = GetEorzeanTime().ToString("HH:mm");

			// Get Seed
			DateTime epoch = new (1970, 1, 1, 0, 0, 0);
			var seed = Math.Floor((DateTime.Now.ToUniversalTime() - epoch).TotalMilliseconds / 1400000);

			// Hash Seed
			var timeChunk = ((seed + 1) % 3) * 8;

			var hashedSeed = GetHashedSeed(seed, timeChunk);
			var upcomingHashedSeed = GetHashedSeed(seed, (timeChunk + 8) % 24);

			// Get Weather
			var cumChance = 0;
			foreach (var (weather, chance) in WeatherRates)
			{
				cumChance += chance;
				if (cumChance > hashedSeed)
				{
					if (currentWeather == "Unknown")
						currentWeather = weather;
				}

				if (cumChance > upcomingHashedSeed)
				{
					if (upcomingWeather == "Unknown")
						upcomingWeather = weather;
				}
			}

			var response = $"Time: {currentTime}.\nCurrent Weather: {currentWeather}\nUpcoming Weather: {upcomingWeather}";
			await this.FollowupAsync(response);
		}

		private static async Task FollowUpTenorResult(IInteractionContext ctx, string search, SocketGuildUser user)
		{
			if (ctx.User is not IGuildUser guildUser)
				throw new UserException("Unable to process user.");

			EmbedBuilder builder = new EmbedBuilder()
				.WithColor(Color.DarkRed);

			if (ctx.Interaction is SocketSlashCommand slashCommand)
			{
				var commandName = slashCommand.Data.Options.FirstOrDefault()?.Name;
				builder.Title = guildUser.Id == user.Id
					? $"{guildUser.GetName()} {commandName}s themselves (kinda weird, _kupo_)"
					: $"{guildUser.GetName()} {commandName}s {user.GetName()}";
			}

			RandomAPI.Result tenorResult = await RandomAPI.Random(search);
			builder.ImageUrl = tenorResult.GetBestUrl();

			await ctx.Interaction.FollowupAsync(embeds: new Embed[] { builder.Build() });
		}

		private static string PadLeft(int value, int numberToPad = 2)
		{
			return value.ToString().PadLeft(numberToPad, '0');
		}

		private static uint GetHashedSeed(double seed, double timeChunk)
		{
			var hashBase = (int)((Math.Floor(seed / 3) * 100) + timeChunk);
			var step1 = (uint)((hashBase << 11) ^ hashBase) >> 0;
			var step2 = ((step1 >> 8) ^ step1) >> 0;
			return step2 % 100;
		}

		private static DateTime GetEorzeanTime()
		{
			/* (1 Eorzean hour = 175 seconds) */

			DateTime epoch = new (1970, 1, 1, 0, 0, 0);
			double eorzeaConstant = 20.571428571428573;

			double timeSeconds = (DateTime.Now.ToUniversalTime() - epoch).TotalSeconds;
			double eorzeaSeconds = timeSeconds * eorzeaConstant;
			DateTime et = epoch + TimeSpan.FromSeconds(eorzeaSeconds);

			return et;
		}

		private static int GenerateRatingForToday(IGuildUser userA, IGuildUser? userB, int mod)
		{
			long now = DateTime.Now.Date.Ticks;

			decimal id = userA.Id / 100000M;
			decimal id2 = userB != null ? (userB.Id / 100000M) : 0;

			decimal step = now + id + id2;
			decimal rating = step / now;

			string n = rating.ToString();
			int num = int.Parse(n[^2..]) % mod;

			return num;
		}
	}
}
