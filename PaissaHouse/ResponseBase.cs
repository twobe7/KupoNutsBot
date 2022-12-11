// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace PaissaHouse
{
	using System;

	public class ResponseBase
	{
		public string? Json;
		public bool IsError => !string.IsNullOrEmpty(this.ErrorMessage);
		public string? ErrorMessage;
	}
}
