// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace PaissaHouse
{
	public class ResponseBase : FC.API.ResponseBase
	{
		public bool IsError => !string.IsNullOrEmpty(this.ErrorMessage);
		public string? ErrorMessage;
	}
}
