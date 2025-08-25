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
using Terraria.Localization;

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

		/// <summary>
		/// Gets the canonical Terraria <see cref="Item.type"/> for the current slot item.
		/// Returns <see cref="ItemID.None"/> if the slot is empty.
		/// </summary>
		internal int CanonicalItemType
		{
			get
			{
				int type = item?.type ?? ItemID.None;
				return type switch
				{
					ItemID.Shellphone or ItemID.ShellphoneSpawn or ItemID.ShellphoneOcean or ItemID.ShellphoneHell =>
						ItemID.ShellphoneDummy,
					ItemID.DontHurtCrittersBookInactive => ItemID.DontHurtCrittersBook,
					ItemID.DontHurtNatureBookInactive => ItemID.DontHurtNatureBook,
					ItemID.DontHurtComboBookInactive => ItemID.DontHurtComboBook,
					ItemID.ClosedVoidBag => ItemID.VoidLens,
					ItemID.UncumberingStone => ItemID.EncumberingStone,
					ItemID.RubblemakerLarge or ItemID.RubblemakerMedium => ItemID.RubblemakerSmall,
					_ => type,
					// From Player.ItemCheck_ManageRightClickFeatures.
					// TODO: We might also want to consider using ItemID.Sets.ShimmerCountsAsItem or Item.GetShimmerEquivalentType to handle modded items with a similar design as well. Modded items probably shouldn't be using the separate Item type approach anyway though.
				};
			}
		}
		
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (item.IsAir && IsMouseHovering)
			{
				// Main.hoverItemName = emptyHintText;
				if (!string.IsNullOrWhiteSpace(emptyHintText))
					Terraria.ModLoader.UI.UICommon.TooltipMouseText(emptyHintText);
			}
		}

		public override void LeftClick(UIMouseEvent evt)
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
					int num = Item.NewItem(Main.LocalPlayer.GetSource_Misc("PlayerDropItemCheck"), (int)player.position.X, (int)player.position.Y, player.width, player.height, item2.type, item2.stack, false, (int)item.prefix, true, false);
					Main.item[num].newAndShiny = false;
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num, 1f, 0f, 0f, 0, 0, 0);
					}
					else
					{
						// TODO: Detect PreSaveAndQuit only.
						RecipeBrowser.instance.Logger.Warn(Language.GetTextValue("Mods.RecipeBrowser.ItemLostInQuerySlotWarning") + item2.Name);
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
