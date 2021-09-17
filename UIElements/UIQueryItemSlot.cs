using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	internal class UIQueryItemSlot : UIItemSlot
	{
		public static Asset<Texture2D> backgroundTextureFake = TextureAssets.InventoryBack8;
		internal bool real = true;
		internal string emptyHintText;

		public event Action OnItemChanged;

		public UIQueryItemSlot(Item item) : base(item)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (item.IsAir && IsMouseHovering)
			{
				Main.hoverItemName = emptyHintText;
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			Player player = Main.LocalPlayer;
			if (player.itemAnimation == 0 && player.itemTime == 0)
			{
				if (real)
				{
					Item item = Main.mouseItem.Clone();
					Main.mouseItem = this.item.Clone();
					if (Main.mouseItem.type > 0)
					{
						Main.playerInventory = true;
					}
					this.item = item.Clone();
				}
				else
				{
					item = Main.mouseItem.Clone();
					Main.mouseItem.SetDefaults(0);
					real = true;
				}
				if (item.type == 0) real = true;
				OnItemChanged?.Invoke();
			}
			backgroundTexture = real ? defaultBackgroundTexture : backgroundTextureFake;
		}

		internal virtual void ReplaceWithFake(int type)
		{
			if (real && item.stack > 0)
			{
				//	Main.player[Main.myPlayer].QuickSpawnItem(RecipeBrowserWindow.lookupItemSlot.item.type, RecipeBrowserWindow.lookupItemSlot.item.stack);

				Player player = Main.player[Main.myPlayer];
				item.position = player.Center;
				Item item2 = player.GetItem(player.whoAmI, item, GetItemSettings.GetItemInDropItemCheck);
				if (item2.stack > 0)
				{
					int num = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, item2.type, item2.stack, false, (int)item.prefix, true, false);
					Main.item[num].newAndShiny = false;
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 1f, 0f, 0f, 0, 0, 0);
					}
					else
					{
						// TODO: Detect PreSaveAndQuit only.
						RecipeBrowser.instance.Logger.Warn("You left an item in the recipe browser with a full inventory and have lost the item: " + item2.Name);
					}
				}
				item = new Item();
			}

			item.SetDefaults(type);
			real = type == 0;
			backgroundTexture = real ? defaultBackgroundTexture : backgroundTextureFake;
			OnItemChanged?.Invoke();
		}
	}
}