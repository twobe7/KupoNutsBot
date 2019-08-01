// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Events
{
	using System;

	[Serializable]
	public class EventAction
	{
		public EventAction()
		{
		}

		public EventAction(Event evt, Actions action)
		{
			this.Event = evt;
			this.Action = action;
		}

		public enum Actions
		{
			Nothing,
			Update,
			Delete,
			DeleteConfirmed,
		}

		public Event Event
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
