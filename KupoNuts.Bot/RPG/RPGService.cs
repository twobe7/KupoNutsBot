// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.RPG
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;
	using Discord;
	using Discord.Rest;
	using Discord.WebSocket;
	using KupoNuts.Bot.Commands;
	using KupoNuts.Bot.RPG.Items;
	using KupoNuts.Bot.Services;
	using KupoNuts.RPG;

	public class RPGService : ServiceBase
	{
		public static string NutEmoteStr = "<:kupo_nut:629887117819904020>";
		public static IEmote NutEmote = Emote.Parse(NutEmoteStr);

		public static List<ItemBase> Items = new List<ItemBase>();

		private const double GenerationChance = 0.05;
		private static RPGService? instance;

		private Database<Status> rpgDatabase = new Database<Status>("RPG", 1);
		private Database<Item> itemDatabase = new Database<Item>("RPG_Items", 1);

		public static async Task<Status> GetStatus(IGuildUser user)
		{
			if (instance == null)
				throw new Exception("RPG Service is not running");

			return await instance.rpgDatabase.LoadOrCreate(user.Id.ToString());
		}

		public static async Task<Status> GetStatus(string id)
		{
			if (instance == null)
				throw new Exception("RPG Service is not running");

			return await instance.rpgDatabase.LoadOrCreate(id);
		}

		public static async Task SaveStatus(Status status)
		{
			if (instance == null)
				throw new Exception("RPG Service is not running");

			await instance.rpgDatabase.Save(status);
		}

		public static ItemBase GetItem(string itemName)
		{
			foreach (ItemBase item in Items)
			{
				if (item.Name.ToLower() == itemName.ToLower() || item.Id == itemName)
				{
					return item;
				}
			}

			throw new Exception("Unknown item: " + itemName);
		}

		public override async Task Initialize()
		{
			instance = this;

			await base.Initialize();
			await this.rpgDatabase.Connect();
			await this.itemDatabase.Connect();

			Program.DiscordClient.MessageReceived += this.OnMessageReceived;
			Program.DiscordClient.ReactionAdded += this.OnReactionAdded;

			await this.UpdateItemDatabase();
		}

		public override Task Shutdown()
		{
			Program.DiscordClient.ReactionAdded -= this.OnReactionAdded;
			Program.DiscordClient.MessageReceived -= this.OnMessageReceived;
			return base.Shutdown();
		}

		[Command("UpdateItems", Permissions.Administrators, "imports the items from the item database")]
		public async Task UpdateItemDatabase()
		{
			RPGService.Items.Clear();

			RPGService.Items.Add(new StatusConsumable("0", "Level Up", "Increases your character level by 1", 10, (user, status) =>
			{
				status.Level++;
				return Task.FromResult(user.GetName() + " is now level " + status.Level);
			}));

			RPGService.Items.Add(new StatusConsumable("1", "Level Up x10", "Increases your character level by 10", 100, (user, status) =>
			{
				status.Level += 10;
				return Task.FromResult(user.GetName() + " is now level " + status.Level);
			}));

			List<Item> items = await this.itemDatabase.LoadAll();
			foreach (Item item in items)
			{
				if (item.Id == null || item.Name == null || item.Description == null || item.Cost == null || item.Macro == null)
					continue;

				MacroItem macroItem = new MacroItem((string)item.Id, (string)item.Name, (string)item.Description, (int)item.Cost, (string)item.Macro);
				RPGService.Items.Add(macroItem);
			}
		}

		[Command("SendKupoNuts", Permissions.Everyone, "Sends kupo nuts to the specified user")]
		public async Task<(string, Embed)> SendNuts(CommandMessage message, IGuildUser destinationUser, int count = 1)
		{
			IGuildUser fromUser = message.Author;

			Log.Write(fromUser.GetName() + " sent a Kupo Nut to " + destinationUser.GetName() + " (command)", "Bot");

			(Status fromKarma, Status toKarma) = await this.SendNut(fromUser, destinationUser, count);

			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.Append("Hey ");
			messageBuilder.Append(destinationUser.Mention);
			messageBuilder.Append(", ");
			messageBuilder.Append(fromUser.Mention);
			messageBuilder.Append(" has sent you ");
			messageBuilder.Append(count);
			messageBuilder.Append(" ");
			messageBuilder.Append(NutEmoteStr);

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.AddField(fromUser.GetName(), fromKarma.Nuts);
			embedBuilder.AddField(destinationUser.GetName(), toKarma.Nuts);

			return (messageBuilder.ToString(), embedBuilder.Build());
			////await message.Channel.SendMessageAsync(messageBuilder.ToString(), false, embedBuilder.Build());
		}

		[Command("Shop", Permissions.Everyone, "opens the item shop")]
		[Command("Store", Permissions.Everyone, "opens the item shop")]
		public void Store(CommandMessage message)
		{
			RPG.Store.BeginStore(message.Channel, message.Author);
		}

		[Command("Inventory", Permissions.Everyone, "Shows your current profile")]
		[Command("Profile", Permissions.Everyone, "Shows your current profile")]
		public async Task<Embed> ShowProfile(CommandMessage message)
		{
			return await this.ShowProfile(message, message.Author);
		}

		[Command("Inventory", Permissions.Everyone, "Shows the profile of the specified user")]
		[Command("Profile", Permissions.Everyone, "Shows the profile of the specified user")]
		public async Task<Embed> ShowProfile(CommandMessage message, IGuildUser user)
		{
			Status status = await this.rpgDatabase.LoadOrCreate(user.Id.ToString());
			return status.ToEmbed(user);
		}

		[Command("KupoNuts", Permissions.Everyone, "Shows the kupo nut leaderboards")]
		public async Task<Embed> ShowLeaders(CommandMessage message)
		{
			List<Status> statuses = await this.rpgDatabase.LoadAll();

			statuses.Sort((Status a, Status b) =>
			{
				return -a.Nuts.CompareTo(b.Nuts);
			});

			IGuild guild = message.Guild;

			StringBuilder builder = new StringBuilder();
			int count = 0;
			foreach (Status status in statuses)
			{
				count++;

				if (count > 10)
					break;

				if (status.Id == null)
					continue;

				IGuildUser user = await guild.GetUserAsync(ulong.Parse(status.Id));

				if (user == null)
					continue;

				builder.Append(status.Nuts);
				builder.Append(" - ");
				builder.AppendLine(user.GetName());
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Kupo Nut Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		[Command("Levels", Permissions.Everyone, "Shows the level leaderboards")]
		public async Task<Embed> ShowLevelLeaders(CommandMessage message)
		{
			List<Status> statuses = await this.rpgDatabase.LoadAll();

			statuses.Sort((Status a, Status b) =>
			{
				return -a.Level.CompareTo(b.Level);
			});

			IGuild guild = message.Guild;

			StringBuilder builder = new StringBuilder();
			int count = 0;
			foreach (Status status in statuses)
			{
				count++;

				if (count > 10)
					break;

				if (status.Id == null)
					continue;

				IGuildUser user = await guild.GetUserAsync(ulong.Parse(status.Id));

				if (user == null)
					continue;

				builder.Append(status.Level);
				builder.Append(" - ");
				builder.AppendLine(user.GetName());
			}

			EmbedBuilder embedBuilder = new EmbedBuilder();
			embedBuilder.Title = "Level Leaderboard";
			embedBuilder.Description = builder.ToString();
			return embedBuilder.Build();
		}

		[Command("UseItem", Permissions.Everyone, "Uses an item from your inventory")]
		public async Task<string> UseItem(CommandMessage message, string itemName)
		{
			return await this.UseItem(message, itemName, message.Author);
		}

		[Command("UseItem", Permissions.Everyone, "Uses an item from your inventory")]
		public async Task<string> UseItem(CommandMessage message, string itemName, IGuildUser target)
		{
			ItemBase item;
			try
			{
				item = GetItem(itemName);
			}
			catch (Exception)
			{
				throw new UserException("I couldn't find the item: \"" + itemName + "\"");
			}

			Status status = await this.rpgDatabase.LoadOrCreate(message.Author.Id.ToString());
			Status.ItemStack? stack = status.GetItem(item.Id);

			if (stack == null)
				throw new UserException("You dont have any " + item.Name);

			if (item is Consumable consume)
			{
				stack.Count--;
				await this.rpgDatabase.Save(status);

				return await consume.Use(message.Author, target);
			}
			else
			{
				throw new UserException("That item cannot be consumed");
			}
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			try
			{
				if (message.Author.Id == Program.DiscordClient.CurrentUser.Id)
					return;

				if (message.Author.IsBot)
					return;

				IMessage iMessage = await message.Channel.GetMessageAsync(message.Id);

				if (iMessage is RestUserMessage restMessage)
				{
					Random rn = new Random();
					double roll = rn.NextDouble();
					if (roll < GenerationChance)
					{
						IGuildUser toUser = message.GetAuthor();

						Log.Write(toUser.GetName() + " Found a Kupo Nut with message: \"" + message.Content + "\"", "Bot");

						Status toKarma = await this.rpgDatabase.LoadOrCreate(toUser.Id.ToString());
						toKarma.Nuts++;
						await this.rpgDatabase.Save(toKarma);
						await restMessage.AddReactionAsync(NutEmote);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
		{
			try
			{
				if (reaction.Emote.Name != NutEmote.Name)
					return;

				if (reaction.UserId == Program.DiscordClient.CurrentUser.Id)
					return;

				IUserMessage userMessage = await message.GetOrDownloadAsync();

				IGuildUser toUser = userMessage.GetAuthor();

				IGuild guild = userMessage.GetGuild();
				IGuildUser fromUser = await guild.GetUserAsync(reaction.UserId);

				Log.Write(fromUser.GetName() + " sent a Kupo Nut to " + toUser.GetName() + " (reaction)", "Bot");

				await this.SendNut(fromUser, toUser, 1);
			}
			catch (UserException)
			{
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}

		private async Task<(Status, Status)> SendNut(IGuildUser fromUser, IGuildUser toUser, int count)
		{
			Status fromStatus = await this.rpgDatabase.LoadOrCreate(fromUser.Id.ToString());

			if (fromStatus.Nuts < count)
				throw new UserException("You dont have any more kupo nuts to give!");

			if (toUser.Id == Program.DiscordClient.CurrentUser.Id)
				throw new UserException("You cant send a kupo nuts to a bot... even me.... Kupo Nuts...");

			if (toUser.IsBot)
				throw new UserException("You can't send kupo nuts to a bot!");

			if (toUser.Id == fromUser.Id)
				throw new UserException("You can't send kupo nuts to yourself!");

			Status toStatus = await this.rpgDatabase.LoadOrCreate(toUser.Id.ToString());

			fromStatus.Nuts--;
			toStatus.Nuts++;

			await this.rpgDatabase.Save(fromStatus);
			await this.rpgDatabase.Save(toStatus);

			return (fromStatus, toStatus);
		}
	}
}
