using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RecipeBrowser
{
	class RecipeBrowserClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(true)]
		[Label("Show Recipe Mod Source")]
		[Tooltip("Show which mod adds which recipe in the recipe catalog. Disable for immersion.")]
		public bool ShowRecipeModSource { get; set; }

		[DefaultValue(true)]
		[Label("Show Item Mod Source")]
		[Tooltip("Show which mod adds which item in the recipe catalog. Disable for immersion.")]
		public bool ShowItemModSource { get; set; }

		[DefaultValue(true)]
		[Label("Show NPC Mod Source")]
		[Tooltip("Show which mod adds which NPC in the bestiary. Disable for immersion.")]
		public bool ShowNPCModSource { get; set; }

		[Header("Automatic Settings")]
		// non-player specific stuff:

		[DefaultValue(typeof(Vector2), "475, 350")]
		[Range(0f, 1920f)]
		[Label("Recipe Browser Size")]
		[Tooltip("Size of the Recipe Browser UI. This will automatically save, no need to adjust")]
		public Vector2 RecipeBrowserSize { get; set; }

		[DefaultValue(typeof(Vector2), "400, 400")]
		[Range(0f, 1920f)]
		[Label("Recipe Browser Poisition")]
		[Tooltip("Position of the Recipe Browser UI. This will automatically save, no need to adjust")]
		public Vector2 RecipeBrowserPosition { get; set; }

		[DefaultValue(typeof(Vector2), "-310, 90")]
		[Range(-1920f, 0f)]
		[Label("Favorited Recipes Poisition")]
		[Tooltip("Position of the Favorited Recipes UI. This will automatically save, no need to adjust")]
		public Vector2 FavoritedRecipePanelPosition { get; set; }

		[DefaultValue(true)]
		[Label("Only Show Pinned While Inventory Open")]
		[Tooltip("Automatically show and hide pinned recipes panel. This will automatically save, no need to adjust")]
		public bool OnlyShowPinnedWhileInInventory { get; set; }

		internal static void SaveConfig() {
			// in-game ModConfig saving from mod code is not supported yet in tmodloader, and subject to change, so we need to be extra careful.
			// This code only supports client configs, and doesn't call onchanged. It also doesn't support ReloadRequired or anything else.
			MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
			if (saveMethodInfo != null)
				saveMethodInfo.Invoke(null, new object[] { ModContent.GetInstance< RecipeBrowserClientConfig>() });
			else
				RecipeBrowser.instance.Logger.Warn("In-game SaveConfig failed, code update required");
		}
	}
}
