using Harmony;
using System;
using System.Diagnostics;
using System.Linq;
using Terraria;

namespace RecipeBrowser
{
	[HarmonyPatch(typeof(Recipe))]
	[HarmonyPatch("FindRecipes")]
	[HarmonyPatch(new Type[] { })]
	public class Recipe_FindRecipes_Patcher
	{
		// This patch Invalidates the precomputed extended craft unless the reason for FindRecipes was just AdjTiles, since we ignore AdjTiles changes.
		static void Postfix()
		{
			if (!new StackTrace().GetFrames().Any(x => x.GetMethod().Name.StartsWith("AdjTiles")))
			{
				RecipeCatalogueUI.instance.InvalidateExtendedCraft();
				//Main.NewText("FindRecipes postfix: InvalidateExtendedCraft");
			}
			else
			{
				//Main.NewText("FindRecipes postfix: skipped");
			}
		}
	}

	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("AdjTiles")]
	[HarmonyPatch(new Type[] { })]
	public class Player_AdjTiles_Patcher
	{
		// This patch will call FindRecipes even if the player inventory is closed, keeping Craft tool buttons accurate.
		static void Postfix()
		{
			// AdjTiles does the opposite. This way recipes will be calculated 
			if (!Main.playerInventory && RecipeBrowserUI.instance.ShowRecipeBrowser) // Inverted condition.
			{
				bool flag = false;
				for (int l = 0; l < Main.LocalPlayer.adjTile.Length; l++)
				{
					if (Main.LocalPlayer.oldAdjTile[l] != Main.LocalPlayer.adjTile[l])
					{
						flag = true;
						break;
					}
				}
				if (Main.LocalPlayer.adjWater != Main.LocalPlayer.oldAdjWater)
				{
					flag = true;
				}
				if (Main.LocalPlayer.adjHoney != Main.LocalPlayer.oldAdjHoney)
				{
					flag = true;
				}
				if (Main.LocalPlayer.adjLava != Main.LocalPlayer.oldAdjLava)
				{
					flag = true;
				}
				if (flag)
				{
					Recipe.FindRecipes();
				}
			}
		}
	}
}
