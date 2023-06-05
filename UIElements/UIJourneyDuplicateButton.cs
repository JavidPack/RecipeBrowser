using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ID;
using ReLogic.Content;
using Terraria.DataStructures;
using System;

namespace RecipeBrowser.UIElements
{
	internal class UIJourneyDuplicateButton : UIElement
	{
		public static Asset<Texture2D> duplicateOn;
		public static Asset<Texture2D> duplicateOff;
		CraftPath.JourneyDuplicateItemNode duplicationNode;

		public UIJourneyDuplicateButton(CraftPath.JourneyDuplicateItemNode duplicationNode) {
			this.duplicationNode = duplicationNode;
			this.Width.Set(duplicateOn.Width(), 0f);
			this.Height.Set(duplicateOn.Height(), 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			bool ableToDuplicate = AbleToDuplicate();
			CalculatedStyle dimensions = base.GetDimensions();
			spriteBatch.Draw((IsMouseHovering && ableToDuplicate ? duplicateOn : duplicateOff).Value, dimensions.Position(), null, Color.White, 0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);
			//ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, ableToDuplicate ? "✓" : "X", dimensions.Position() + new Vector2(14f, 10f), ableToDuplicate ? Utilities.yesColor : Color.LightSalmon, 0f, Vector2.Zero, new Vector2(0.7f));
			if (IsMouseHovering) {
				Main.hoverItemName = ableToDuplicate ? "Duplicate" : "";
			}
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			if (AbleToDuplicate()) {
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			if (AbleToDuplicate()) {
				int stack = duplicationNode.stack;
				while(stack > 0) {
					Item duplicateItem = ContentSamples.ItemsByType[duplicationNode.itemid].Clone();
					int itemStack = Math.Min(stack, duplicateItem.maxStack);
					duplicateItem.stack = itemStack;
					duplicateItem.OnCreated(new JourneyDuplicationItemCreationContext());
					duplicateItem = Main.player[Main.myPlayer].GetItem(Main.myPlayer, duplicateItem, GetItemSettings.InventoryEntityToPlayerInventorySettings);
					if (duplicateItem.stack > 0) {
						Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("PlayerDropItemCheck"), duplicateItem, duplicateItem.stack);
					}

					SoundEngine.PlaySound(SoundID.MenuTick);
					stack -= itemStack;
				}
			}
		}

		// Probably not necessary to check again...
		bool AbleToDuplicate() {
			if (Main.GameModeInfo.IsJourneyMode) {
				if (RecipePath.ItemFullyResearched(duplicationNode.itemid)) {
					return true;
				}
			}
			return false;
		}
	}
}