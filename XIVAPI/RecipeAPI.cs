// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using FC.API;

	public static class RecipeAPI
	{
		public static async Task<Recipe> Get(ulong id)
		{
			return await Request.Send<Recipe>($"/recipe/{id}");
		}

		[Serializable]
		public class Recipe : ResponseBase
		{
			public int AmountIngredient0 { get; set; } = 0;
			public int AmountIngredient1 { get; set; } = 0;
			public int AmountIngredient2 { get; set; } = 0;
			public int AmountIngredient3 { get; set; } = 0;
			public int AmountIngredient4 { get; set; } = 0;
			public int AmountIngredient5 { get; set; } = 0;
			public int AmountIngredient6 { get; set; } = 0;
			public int AmountIngredient7 { get; set; } = 0;
			public int AmountIngredient8 { get; set; } = 0;
			public int AmountIngredient9 { get; set; } = 0;
			public int AmountResult { get; set; } = 0;
			public bool CanHq { get; set; } = false;
			public bool CanQuickSynth { get; set; } = false;
			public Item? IntemIngredient0 { get; set; }
			public Item? IntemIngredient1 { get; set; }
			public Item? IntemIngredient2 { get; set; }
			public Item? IntemIngredient3 { get; set; }
			public Item? IntemIngredient4 { get; set; }
			public Item? IntemIngredient5 { get; set; }
			public Item? IntemIngredient6 { get; set; }
			public Item? IntemIngredient7 { get; set; }
			public Item? IntemIngredient8 { get; set; }
			public Item? IntemIngredient9 { get; set; }
			public List<Recipe>? ItemIngredientRecipe0 { get; set; }
			public List<Recipe>? ItemIngredientRecipe1 { get; set; }
			public List<Recipe>? ItemIngredientRecipe2 { get; set; }
			public List<Recipe>? ItemIngredientRecipe3 { get; set; }
			public List<Recipe>? ItemIngredientRecipe4 { get; set; }
			public List<Recipe>? ItemIngredientRecipe5 { get; set; }
			public List<Recipe>? ItemIngredientRecipe6 { get; set; }
			public List<Recipe>? ItemIngredientRecipe7 { get; set; }
			public List<Recipe>? ItemIngredientRecipe8 { get; set; }
			public List<Recipe>? ItemIngredientRecipe9 { get; set; }
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class CraftType
		{
			public int ID { get; set; } = 0;
			public string Name { get; set; } = string.Empty;
		}

		[Serializable]
		public class LevelTable
		{
			public int ClassJobLevel { get; set; } = 0;
			public int Durability { get; set; } = 0;
		}
	}
}