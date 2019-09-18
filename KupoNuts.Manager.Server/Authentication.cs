// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNuts.Manager.Server
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Threading.Tasks;
	using JWT;
	using JWT.Algorithms;
	using JWT.Serializers;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Primitives;

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
			RandomNumberGenerator gen = RNGCryptoServiceProvider.Create();
			byte[] data = new byte[SecretSize];
			gen.GetBytes(data);
			Secret = data;
		}

		public static bool IsAuthenticated(HttpRequest request)
		{
#if DEBUG
			return true;
#else
			if (!request.Headers.TryGetValue("Token", out StringValues val))
				return false;

			if (val.Count != 1)
				return false;

			string token = val[0];
			return VerifyToken(token, "IsAdmin", "true");
#endif
		}

		public static string Authenticate(string discordId)
		{
			Log.Write("User Authenticated: " + discordId, "Manager");

			Dictionary<string, string> claims = new Dictionary<string, string>();
			claims.Add("DiscordID", discordId);
			claims.Add("IsAdmin", "true");
			return GenerateToken(claims);
		}

		public static string GenerateToken(Dictionary<string, string> claims)
		{
			IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
			IJsonSerializer serializer = new JsonNetSerializer();
			IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
			IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

			return encoder.Encode(claims, Secret);
		}

		public static bool VerifyToken(string token, string key, string value)
		{
			IJsonSerializer serializer = new JsonNetSerializer();
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
	}
}
