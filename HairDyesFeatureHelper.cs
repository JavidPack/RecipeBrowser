using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RecipeBrowser
{
	internal static class HairDyesFeatureHelper
	{
		internal static List<UIHairDyeItemSlot> HairDyeSlots;

		internal static void GenerateHairDyeSlots() {
			HairDyeSlots = new();
			for (int type = 1; type < ItemLoader.ItemCount; type++) {
				Item item = ContentSamples.ItemsByType[type];
				if (item.hairDye > -1) {
					UIHairDyeItemSlot hairSlot = new(item);
					HairDyeSlots.Add(hairSlot);
				}
			}
		}
	}
}
