// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.XIVData
{
	/// <summary>
	/// Enum for Data Centre.
	/// </summary>
	/// <remarks>Populated from https://github.com/xivapi/ffxiv-datamining/blob/master/csv/WorldDCGroupType.csv. </remarks>
	public enum DataCentre
	{
		Invalid = 0,
		Elemental = 1,
		Gaia = 2,
		Mana = 3,
		Aether = 4,
		Primal = 5,
		Chaos = 6,
		Light = 7,
		Crystal = 8,
		Materia = 9,
		Beta = 99,
	}

	/// <summary>
	/// Enum for worlds.
	/// </summary>
	/// <remarks>Populated from https://github.com/xivapi/ffxiv-datamining/blob/master/csv/World.csv. </remarks>
	public enum World
	{
		Ravana = 21,
		Bismarck = 22,
		Asura = 23,
		Belias = 24,
		Chaos = 25,
		Hecatoncheir = 26,
		Moomba = 27,
		Pandaemonium = 28,
		Shinryu = 29,
		Unicorn = 30,
		Yojimbo = 31,
		Zeromus = 32,
		Twintania = 33,
		Brynhildr = 34,
		Famfrit = 35,
		Lich = 36,
		Mateus = 37,
		Shemhazai = 38,
		Omega = 39,
		Jenova = 40,
		Zalera = 41,
		Zodiark = 42,
		Alexander = 43,
		Anima = 44,
		Carbuncle = 45,
		Fenrir = 46,
		Hades = 47,
		Ixion = 48,
		Kujata = 49,
		Typhon = 50,
		Ultima = 51,
		Valefor = 52,
		Exodus = 53,
		Faerie = 54,
		Lamia = 55,
		Phoenix = 56,
		Siren = 57,
		Garuda = 58,
		Ifrit = 59,
		Ramuh = 60,
		Titan = 61,
		Diabolos = 62,
		Gilgamesh = 63,
		Leviathan = 64,
		Midgardsormr = 65,
		Odin = 66,
		Shiva = 67,
		Atomos = 68,
		Bahamut = 69,
		Chocobo = 70,
		Moogle = 71,
		Tonberry = 72,
		Adamantoise = 73,
		Coeurl = 74,
		Malboro = 75,
		Tiamat = 76,
		Ultros = 77,
		Behemoth = 78,
		Cactuar = 79,
		Cerberus = 80,
		Goblin = 81,
		Mandragora = 82,
		Louisoix = 83,
		Syldra = 84,
		Spriggan = 85,
		Sephirot = 86,
		Sophia = 87,
		Zurvan = 88,
		Aegis = 90,
		Balmung = 91,
		Durandal = 92,
		Excalibur = 93,
		Gungnir = 94,
		Hyperion = 95,
		Masamune = 96,
		Ragnarok = 97,
		Ridill = 98,
		Sargatanas = 99,
	}
}
