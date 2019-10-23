// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Bot.Characters
{
	using XIVAPICharacter = XIVAPI.Character;

	public static class CharacterExtensions
	{
		public static XIVAPI.ClassJob? GetClassJob(this XIVAPICharacter self, Jobs id)
		{
			if (self.ClassJobs == null)
				return null;

			foreach (XIVAPI.ClassJob job in self.ClassJobs)
			{
				if (job.Job == null)
					return null;

				if (job.Job.ID != (uint)id)
					continue;

				return job;
			}

			return null;
		}
	}
}
