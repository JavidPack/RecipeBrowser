using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Reflection;
using RecipeBrowser.UIElements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;

namespace RecipeBrowser
{
	class RecipeBrowserClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static RecipeBrowserClientConfig Instance = null;

		[DefaultValue(true)]
		public bool ShowRecipeModSource { get; set; }

		[DefaultValue(true)]
		public bool ShowItemModSource { get; set; }

		[DefaultValue(true)]
		public bool ShowNPCModSource { get; set; }

		[CustomModConfigItem(typeof(OpenKeybindingsMenuButton))]
		[JsonIgnore, ShowDespiteJsonIgnore]
		public bool OpenKeybindingsMenuButton { get; set; }

		[Header("AutomaticSettings")]
		// non-player specific stuff:

		[DefaultValue(typeof(Vector2), "475, 350")]
		[Range(0f, 1920f)]
		public Vector2 RecipeBrowserSize { get; set; }

		[DefaultValue(typeof(Vector2), "400, 400")]
		[Range(0f, 1920f)]
		public Vector2 RecipeBrowserPosition { get; set; }

		[DefaultValue(typeof(Vector2), "-310, 90")]
		[Range(-1920f, 0f)]
		public Vector2 FavoritedRecipePanelPosition { get; set; }

		[DefaultValue(true)]
		public bool OnlyShowFavoritedWhileInInventory { get; set; }

		[DefaultValue(ShowOtherPlayersFavoritedRecipesOption.ShowAll)]
		public ShowOtherPlayersFavoritedRecipesOption ShowOtherPlayersFavoritedRecipes { get; set; }

		internal static void SaveConfig() {
			// in-game ModConfig saving from mod code is not supported yet in tmodloader, and subject to change, so we need to be extra careful.
			// This code only supports client configs, and doesn't call onchanged. It also doesn't support ReloadRequired or anything else.
			try {
				MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
				if (saveMethodInfo != null)
					saveMethodInfo.Invoke(null, new object[] { ModContent.GetInstance<RecipeBrowserClientConfig>() });
				else
					RecipeBrowser.instance.Logger.Warn("In-game SaveConfig failed, code update required");
			}
			catch { }
		}
	}

	public enum ShowOtherPlayersFavoritedRecipesOption
	{
		ShowAll,
		ShowTeamOnly,
		Hide
	}
}
