// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server
{
	using System.Collections.Generic;
	using System.Security.Cryptography;
	using FC.Serialization;
	using JWT;
	using JWT.Algorithms;
	using Microsoft.AspNetCore.Http;

	public static class Authentication
	{
		private const int SecretSize = 256;

		public static byte[] Secret
		{
			get;
			private set;
		}

		public static void GenerateSecret()
		{
			RandomNumberGenerator gen = RandomNumberGenerator.Create();
			byte[] data = new byte[SecretSize];
			gen.GetBytes(data);
			Secret = data;
		}

		public static bool IsAuthenticated(HttpRequest request)
		{
			if (!request.Headers.TryGetValue("Token", out Microsoft.Extensions.Primitives.StringValues val))
				return false;

			if (val.Count != 1)
				return false;

			string token = val[0];
			return VerifyToken(token, "IsAuth");
		}

		public static string Authenticate(string discordId, List<ulong> canManageGuilds)
		{
			Log.Write("User Authenticated: " + discordId, "Manager");

			Dictionary<string, string> claims = new Dictionary<string, string>();
			claims.Add("DiscordID", discordId);
			claims.Add("IsAuth", "true");

			foreach (ulong guild in canManageGuilds)
			{
				claims.Add(guild.ToString(), "true");
			}

			return GenerateToken(claims);
		}

		public static string GenerateToken(Dictionary<string, string> claims)
		{
			IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
			IJsonSerializer serializer = new JsonSerializer();
			IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
			IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

			return encoder.Encode(claims, Secret);
		}

		public static bool VerifyToken(string token, string key, string value = "true")
		{
			IJsonSerializer serializer = new JsonSerializer();
			IDateTimeProvider provider = new UtcDateTimeProvider();
			IJwtValidator validator = new JwtValidator(serializer, provider);
			IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
			IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

			string json = decoder.Decode(token, Secret, true);
			Dictionary<string, string> claims = serializer.Deserialize<Dictionary<string, string>>(json);

			if (claims.ContainsKey(key) && claims[key] == value)
				return true;

			return false;
		}

		public class JsonSerializer : IJsonSerializer
		{
			public T Deserialize<T>(string json)
			{
				return Serializer.Deserialize<T>(json);
			}

			public string Serialize(object obj)
			{
				return Serializer.Serialize(obj);
			}
		}
	}
}
