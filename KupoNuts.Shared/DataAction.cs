// This document is intended for use by Kupo Nut Brigade developers.
#pragma warning disable SA1402

namespace KupoNuts
{
	using System;

	public enum Actions
	{
		Nothing,
		Update,
		Delete,
		DeleteConfirmed,
	}

	[Serializable]
	public class DataAction<T>
		where T : EntryBase
	{
		public DataAction()
		{
		}

		public DataAction(T entry, Actions action)
		{
			this.Data = entry;
			this.Action = action;
		}

		public T? Data
		{
			get;
			set;
		}

		public Actions Action
		{
			get;
			set;
		}
	}
}
