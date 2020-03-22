// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Client.Drawers
{
	using System;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using Microsoft.AspNetCore.Components;

	public abstract class DrawerBase : ComponentBase
	{
		[Parameter]
		public PropertyInfo Property { get; set; }

		[Parameter]
		public object Target { get; set; }

		public string Label
		{
			get
			{
				if (this.Property == null)
					return string.Empty;

				string name = this.Property.Name;
				name = Regex.Replace(name, "(\\B[A-Z])", " $1");
				return name;
			}
		}

		public Type GetValueType()
		{
			if (this.Property == null)
				return null;

			return this.Property.PropertyType;
		}

		public T GetValue<T>()
		{
			if (this.Property == null)
				return default(T);

			object val = this.Property.GetValue(this.Target);

			if (typeof(T).IsAssignableFrom(this.Property.PropertyType))
				return (T)val;

			throw new Exception("Property: " + this.Property + " is not type: " + typeof(T));
		}

		public void SetValue(object val)
		{
			this.Property.SetValue(this.Target, val);
		}
	}
}
