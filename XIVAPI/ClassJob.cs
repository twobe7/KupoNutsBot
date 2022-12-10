// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using NetStone.Model.Parseables.Character.ClassJob;

	[Serializable]
	public class ClassJob
	{
		public ClassJob()
		{
		}

		public ClassJob(Character.Jobs job, ClassJobEntry classJob)
		{
			this.Name = job.ToDisplayString();
			this.Job = new Class(job);
			this.Level = 0;

			if (classJob == null)
				return;

			this.ExpLevel = (ulong)classJob.ExpCurrent;
			this.ExpLevelMax = (ulong)classJob.ExpMax;
			this.ExpLevelTogo = (ulong)classJob.ExpToGo;
			this.IsSpecialised = classJob.IsSpecialized;
			this.Level = classJob.Level;

			////this.Mettle = classJob.Mettle;
		}

		public Class? Class { get; set; }
		public ulong ExpLevel { get; set; } = 0;
		public ulong ExpLevelMax { get; set; } = 0;
		public ulong ExpLevelTogo { get; set; } = 0;
		public bool IsSpecialised { get; set; } = false;
		public Class? Job { get; set; }
		public int? Level { get; set; } = 0;
		public object? Mettle { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
