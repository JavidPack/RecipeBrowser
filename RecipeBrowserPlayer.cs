using RecipeBrowser.UIElements;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	internal class RecipeBrowserPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			//if (!RecipeBrowser.instance.CheatSheetLoaded)
			{
				if (RecipeBrowser.instance.ToggleRecipeBrowserHotKey.JustPressed)
				{
					RecipeBrowserUI.instance.ShowRecipeBrowser = !RecipeBrowserUI.instance.ShowRecipeBrowser;
					// Debug assistance, allows for reinitializing RecipeBrowserUI
					//if (!RecipeBrowserUI.instance.ShowRecipeBrowser)
					//{
					//	RecipeBrowserUI.instance.RemoveAllChildren();
					//	var isInitializedFieldInfo = typeof(Terraria.UI.UIElement).GetField("_isInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
					//	isInitializedFieldInfo.SetValue(RecipeBrowserUI.instance, false);
					//	RecipeBrowserUI.instance.Activate();
					//}
				}
				if (RecipeBrowser.instance.QueryHoveredItemHotKey.JustPressed)
				{
					if (!Main.HoverItem.IsAir)
					{
						RecipeBrowserUI.instance.ShowRecipeBrowser = true;
						if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.RecipeCatalogue)
						{
							RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.ItemCatalogue)
						{
							ItemCatalogueUI.instance.itemGrid.Goto(delegate (UIElement element)
							{
								UIItemCatalogueItemSlot itemSlot = element as UIItemCatalogueItemSlot;
								if (itemSlot != null && itemSlot.itemType == Main.HoverItem.type)
								{
									ItemCatalogueUI.instance.SetItem(itemSlot);
									return true;
								}
								return false;
							}, true);
						}
						else if (RecipeBrowserUI.instance.CurrentPanel == RecipeBrowserUI.Bestiary)
						{
							BestiaryUI.instance.queryItem.ReplaceWithFake(Main.HoverItem.type);
						}
					}
				}
			}
		}
	}
}