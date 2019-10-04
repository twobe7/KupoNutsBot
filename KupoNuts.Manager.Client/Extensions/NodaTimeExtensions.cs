// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Client
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using NodaTime;

	public static class NodaTimeExtensions
	{
		public static string GetDisplayString(this Instant self)
		{
			DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
			ZonedDateTime zdt = self.InZone(zone);
			StringBuilder builder = new StringBuilder();
			builder.Append(zdt.ToString("hh:mm ", CultureInfo.InvariantCulture));
			builder.Append(zdt.ToString("tt", CultureInfo.InvariantCulture).ToLower());
			builder.Append(zdt.ToString(" dd/MM/yyyy", CultureInfo.InvariantCulture).ToLower());
			return builder.ToString();
		}
	}
}
