using System.ComponentModel;
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
	}
}
