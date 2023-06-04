using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	public class UISystem : ModSystem
	{
		public override void UpdateUI(GameTime gameTime) => RecipeBrowser.instance.UpdateUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) => RecipeBrowser.instance.ModifyInterfaceLayers(layers);

		public override void PreSaveAndQuit() => RecipeBrowser.instance.PreSaveAndQuit();

		public override void PostAddRecipes()
		{
			if (!Main.dedServ) {
				LootCacheManager.Setup(RecipeBrowser.instance);
				RecipeBrowserUI.instance.PostSetupContent();
			}
		}
	}
}
