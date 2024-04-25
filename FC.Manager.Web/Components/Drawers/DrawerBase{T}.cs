// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web.Drawers
{
	using System;

	public abstract class DrawerBase<T> : DrawerBase
	{
		public T Value
		{
			get
			{
				return this.GetValue<T>();
			}
			set
			{
				this.SetValue(value);
			}
		}

		public override bool CanEdit(Type type)
		{
			return typeof(T).IsAssignableFrom(type);
		}
	}
}
