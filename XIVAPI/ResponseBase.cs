// This document is intended for use by Kupo Nut Brigade developers.

namespace XIVAPI
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;
	using Newtonsoft.Json;

	public abstract class ResponseBase
	{
		public string? Json;
	}
}
