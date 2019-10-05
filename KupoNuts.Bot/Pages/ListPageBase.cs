// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Pages
{
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public abstract class ListPageBase : PageBase
	{
		private static string cursorEmote = "<:cursor:629914205306224660>";

		private int selectedIndex = 0;

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public override async Task Navigate(Navigation nav)
		{
			if (nav == Navigation.Up && this.selectedIndex > 0)
				this.selectedIndex--;

			if (nav == Navigation.Down && this.selectedIndex < this.GetItemCount() - 1)
				this.selectedIndex++;

			if (nav == Navigation.Yes)
			{
				if (this.GetItemCount() <= 0)
					return;

				await this.Select(this.selectedIndex);
			}
		}

		protected abstract int GetItemCount();

		protected abstract string GetItemName(int index);

		protected abstract string GetItemDesc(int index);

		protected abstract Task Select(int index);

		protected override Task<string> GetContent()
		{
			StringBuilder builder = new StringBuilder();

			int count = this.GetItemCount();
			if (count > 0)
			{
				for (int i = 0; i < this.GetItemCount(); i++)
				{
					if (i == this.selectedIndex)
					{
						builder.Append(cursorEmote);
					}
					else
					{
						builder.Append(Utils.Characters.DoubleTab);
						builder.Append(Utils.Characters.Space);
					}

					builder.AppendLine(this.GetItemName(i));
				}

				builder.AppendLine();
				builder.AppendLine(this.GetItemDesc(this.selectedIndex));
			}
			else
			{
				builder.AppendLine("Empty");
			}

			return Task.FromResult(builder.ToString());
		}
	}
}
