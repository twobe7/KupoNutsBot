// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace Tenor
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;
	using FC;

	[Serializable]
	public abstract class ResponseBase
	{
		public string? Json;
	}
}
