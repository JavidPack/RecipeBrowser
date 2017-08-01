using Terraria.ModLoader;
using Terraria.GameInput;
using System.Reflection;

namespace RecipeBrowser
{
	class RecipeBrowserPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			//if (!RecipeBrowser.instance.CheatSheetLoaded)
			{
				if (RecipeBrowser.instance.ToggleRecipeBrowserHotKey.JustPressed)
				{
					RecipeBrowser.instance.recipeBrowserTool.visible = !RecipeBrowser.instance.recipeBrowserTool.visible;
					// Debug assistance, allows for reinitializing RecipeBrowserUI
					//if (!RecipeBrowser.instance.recipeBrowserTool.visible)
					//{
					//	RecipeBrowserUI.instance.RemoveAllChildren();
					//	var isInitializedFieldInfo = typeof(Terraria.UI.UIElement).GetField("_isInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
					//	isInitializedFieldInfo.SetValue(RecipeBrowserUI.instance, false);
					//	RecipeBrowserUI.instance.Activate();
					//}
				}
			}
		}
	}
}
