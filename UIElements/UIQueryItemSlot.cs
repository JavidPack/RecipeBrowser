using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIQueryItemSlot : UIItemSlot
	{
		public static Texture2D backgroundTextureFake = Main.inventoryBack8Texture;
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
				Item item2 = player.GetItem(player.whoAmI, item, false, true);
				if (item2.stack > 0)
				{
					int num = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, item2.type, item2.stack, false, (int)item.prefix, true, false);
					Main.item[num].newAndShiny = false;
					if (Main.netMode == 1)
					{
						NetMessage.SendData(21, -1, -1, null, num, 1f, 0f, 0f, 0, 0, 0);
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