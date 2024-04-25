// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Web;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FC.Manager.Web.Services;
using FC.Serialization;
using Microsoft.AspNetCore.Http;
using JWT;
using JWT.Algorithms;

public static class Authentication
{
	public const string DiscordScopes = "identify guilds";

	private static Data data;

	public static bool IsAuthenticated => !string.IsNullOrEmpty(data?.AuthToken);

	public static string Token => data?.AuthToken;

	public static List<Data.Guild> Guilds => data?.Guilds;

	public static void Authenticate(Data authenticatedData)
	{
		data = authenticatedData;

        if (data == null)
			throw new Exception("Authentication failed");

		if (string.IsNullOrEmpty(data.AuthToken))
			throw new Exception("Invalid token");

		if (data.Guilds == null || data.Guilds.Count <= 0)
			throw new Exception("You must be in at least one guild");

		Console.WriteLine(">> User has " + data.Guilds.Count + " guilds");

		// set the first available guild as the default
		foreach (Data.Guild guild in data.Guilds)
		{
			if (!guild.CanManageGuild)
				continue;

			RPCService.GuildId = guild.GetId();
			RPCService.CanManageGuild = guild.CanManageGuild;
			break;
		}

		if (RPCService.GuildId == 0)
		{
			// Set to first
			Data.Guild defaultGuild = data.Guilds.GetFirst();
			RPCService.GuildId = defaultGuild.GetId();
			RPCService.CanManageGuild = defaultGuild.CanManageGuild;
		}
	}

	[Serializable]
	public class Data
	{
		public string DiscordUserId { get; set; }
		public string DiscordUserName { get; set; }
		public string AuthToken { get; set; }
		public List<Guild> Guilds { get; set; }

		[Serializable]
		public class Guild
		{
			public const int AdministratorPermission = 0x00000008;
			public const int ManageGuildPermission = 0x00000020;

			public string Id { get; set; }
			public string Name { get; set; }
			public string Icon { get; set; }
			public bool Owner { get; set; }
			public int Permissions { get; set; }
			public bool IsInGuild { get; set; }

			public bool IsAdministrator => (this.Permissions & AdministratorPermission) == AdministratorPermission;
			public bool CanManageGuild => (this.Permissions & ManageGuildPermission) == ManageGuildPermission;

			public ulong GetId() => ulong.Parse(this.Id);
		}
	}

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

	public static bool IsAuthenticatedRequest(HttpRequest request)
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

		Dictionary<string, string> claims = new ()
		{
			{ "DiscordID", discordId },
			{ "IsAuth", "true" }
		};

		foreach (ulong guild in canManageGuilds)
		{
			claims.Add(guild.ToString(), "true");
		}

		return GenerateToken(claims);
	}

	public static string GenerateToken(Dictionary<string, string> claims)
	{
		IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
		JWT.IJsonSerializer serializer = new JsonSerializer();
		IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
		IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

		return encoder.Encode(claims, Secret);
	}

	public static bool VerifyToken(string token, string key, string value = "true")
	{
        JWT.IJsonSerializer serializer = new JsonSerializer();
		IDateTimeProvider provider = new UtcDateTimeProvider();
		IJwtValidator validator = new JwtValidator(serializer, provider);
		IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
		IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);

		string json = decoder.Decode(token, Secret, true);
		Dictionary<string, string> claims = serializer.Deserialize<Dictionary<string, string>>(json);

		if (claims.TryGetValue(key, out string? claim) && claim == value)
			return true;

		return false;
	}

	public class JsonSerializer : JWT.IJsonSerializer
	{
		public T Deserialize<T>(string json)
		{
			return Serializer.Deserialize<T>(json);
		}

		public object Deserialize(Type type, string json)
		{
			throw new NotImplementedException();
		}

		public string Serialize(object obj)
		{
			return Serializer.Serialize(obj);
		}
	}
}
