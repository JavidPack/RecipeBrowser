using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UIArmorSetCatalogueItemSlot : UIItemCatalogueItemSlot
	{
		internal Tuple<Item, Item, Item, string, int> set;
		internal Item compareItem;
		private bool drawError = false;
		private UIItemCatalogueItemSlot headSlot;
		private UIItemCatalogueItemSlot bodySlot;
		private UIItemCatalogueItemSlot legsSlot;
		internal bool needsUpdate = false;

		internal static Player drawPlayer;
		internal static bool useDye;
		internal static bool animate;
		internal static bool accessories;
		internal static bool showItems = true;
		public UIArmorSetCatalogueItemSlot(Tuple<Item, Item, Item, string, int> set, float scale = 0.75f) : base(set.Item1 != null ? set.Item1 : set.Item2, scale) {
			this.set = set;
			this.compareItem = set.Item1 != null ? set.Item1 : set.Item2;

			this.Width.Set(defaultBackgroundTexture.Width() * scale, 0f);
			this.Height.Set(defaultBackgroundTexture.Height() * 4.6f * scale, 0f); // 50 heigh

			if (set.Item1 != null) {
				Item item = new Item();
				item.SetDefaults(set.Item1.type, false);
				headSlot = new UIItemCatalogueItemSlot(item, scale);
				headSlot.Top.Set(60, 0);
			}
			if (set.Item2 != null) {
				Item item = new Item();
				item.SetDefaults(set.Item2.type, false);
				bodySlot = new UIItemCatalogueItemSlot(item, scale);
				bodySlot.Top.Set(100, 0);
			}
			if (set.Item3 != null) {
				Item item = new Item();
				item.SetDefaults(set.Item3.type, false);
				legsSlot = new UIItemCatalogueItemSlot(item, scale);
				legsSlot.Top.Set(140, 0);
			}
			if (showItems) {
				if (headSlot != null)
					UICommon.AddOrRemoveChild(this, headSlot, showItems);
				if (bodySlot != null)
					UICommon.AddOrRemoveChild(this, bodySlot, showItems);
				if (legsSlot != null)
					UICommon.AddOrRemoveChild(this, legsSlot, showItems);
			}
		}

		private static uint lastUpdate;
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (needsUpdate) {
				if (headSlot != null)
					UICommon.AddOrRemoveChild(this, headSlot, showItems);
				if (bodySlot != null)
					UICommon.AddOrRemoveChild(this, bodySlot, showItems);
				if (legsSlot != null)
					UICommon.AddOrRemoveChild(this, legsSlot, showItems);

				if(showItems)
					this.Height.Set(defaultBackgroundTexture.Height() * 4.6f * scale, 0f); // 50 heigh
				else
					this.Height.Set(defaultBackgroundTexture.Height() * 1.6f * scale, 0f);

				needsUpdate = false;
			}

			if (Main.GameUpdateCount != lastUpdate) {
				lastUpdate = Main.GameUpdateCount;
				if (drawPlayer == null)
					drawPlayer = new Player();
				drawPlayer.skinVariant = Main.LocalPlayer.skinVariant;
				drawPlayer.Male = Main.LocalPlayer.Male;
				drawPlayer.eyeColor = Main.LocalPlayer.eyeColor;
				drawPlayer.hairColor = Main.LocalPlayer.hairColor;
				drawPlayer.skinColor = Main.LocalPlayer.skinColor;
				drawPlayer.shirtColor = Main.LocalPlayer.shirtColor;
				drawPlayer.underShirtColor = Main.LocalPlayer.underShirtColor;
				drawPlayer.shoeColor = Main.LocalPlayer.shoeColor;
				drawPlayer.pantsColor = Main.LocalPlayer.pantsColor;
				drawPlayer.direction = 1;
				drawPlayer.gravDir = 1f;
				drawPlayer.head = -1;
				drawPlayer.body = -1;
				drawPlayer.legs = -1;
				drawPlayer.handon = -1;
				drawPlayer.handoff = -1;
				drawPlayer.back = -1;
				drawPlayer.front = -1;
				drawPlayer.shoe = -1;
				drawPlayer.waist = -1;
				drawPlayer.shield = -1;
				drawPlayer.neck = -1;
				drawPlayer.face = -1;
				drawPlayer.balloon = -1;
				drawPlayer.wings = -1;

				if (useDye) {
					for (int i = 0; i < 10; i++) {
						drawPlayer.dye[i] = Main.LocalPlayer.dye[i].Clone();
					}
				}
				else {
					for (int i = 0; i < 10; i++) {
						drawPlayer.dye[i].TurnToAir();
						drawPlayer.dye[i].dye = 0;
					}
				}

				if (accessories) {
					for (int i = 0; i < 20; i++) {
						drawPlayer.armor[i] = Main.LocalPlayer.armor[i].Clone();
						if (i < 10)
							drawPlayer.hideVisibleAccessory[i] = Main.LocalPlayer.hideVisibleAccessory[i];
					}
				}
				else {
					for (int i = 0; i < 20; i++) {
						drawPlayer.armor[i].TurnToAir();
						if (i < 10)
							drawPlayer.hideVisibleAccessory[i] = true;
					}
				}

				drawPlayer.PlayerFrame();
				drawPlayer.socialIgnoreLight = true;

				if (animate) {
					drawPlayer.bodyFrame = Main.LocalPlayer.bodyFrame;
					drawPlayer.legFrame = Main.LocalPlayer.legFrame;
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			//base.DrawSelf(spriteBatch);

			if (drawPlayer == null)
				return;

			Item head = set.Item1;
			Item body = set.Item2;
			Item leg = set.Item3;
			drawPlayer.head = head?.headSlot ?? -1;
			drawPlayer.body = body?.bodySlot ?? -1;
			drawPlayer.legs = leg?.legSlot ?? -1;

			CalculatedStyle dimensions = base.GetInnerDimensions();
			Rectangle rectangle = dimensions.ToRectangle();

			spriteBatch.Draw(backgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, new Vector2(scale, scale * 1.5f), SpriteEffects.None, 0f);
			if (drawError)
				return;

			var center = new Vector2(rectangle.Center().X, rectangle.Y + 38);

			drawPlayer.direction = 1;
			drawPlayer.Bottom = Main.screenPosition + center + new Vector2(0, 15);

			//Main.gameMenu = true;
			try {
				Main.PlayerRenderer.DrawPlayer(Main.Camera, drawPlayer, drawPlayer.position, drawPlayer.fullRotation, drawPlayer.fullRotationOrigin, 0f);
				UseImmediateMode = true;
			}
			catch (Exception) {
				drawError = true;
			}
			//Main.gameMenu = false;

			if (IsMouseHovering) {
				//Main.HoverItem = item.Clone();
				Main.hoverItemName = set.Item4 + "\nTotal Set Defense: " + set.Item5;

				//Main.HoverItem.SetNameOverride(Main.HoverItem.Name + (Main.HoverItem.modItem != null && ModContent.GetInstance<RecipeBrowserClientConfig>().ShowItemModSource ? " [" + Main.HoverItem.modItem.mod.Name + "]" : ""));
			}
		}
	}


	internal static class ArmorSetFeatureHelper
	{
		internal static List<Tuple<Item, Item, Item, string, int>> sets;
		internal static List<UIArmorSetCatalogueItemSlot> armorSetSlots;
		internal const string ArmorSetsHoverTest = "Armor Sets\n(Warning: May take many seconds to calculate)";

		internal static void Unload() {
			sets = null;
			armorSetSlots = null;
			UIArmorSetCatalogueItemSlot.drawPlayer = null;
		}

		internal static void AppendSpecialUI(UIGrid itemGrid) {
			var panel = new UIPanel();
			panel.Width.Percent = 1f;
			panel.Height.Set(100, 0f);
			panel.Width.Set(162, 0f);
			panel.SetPadding(12);

			var showItemsCheckbox = new UICheckbox("Show Items", "Display the items that make up the set");
			showItemsCheckbox.Selected = UIArmorSetCatalogueItemSlot.showItems;
			showItemsCheckbox.OnSelectedChanged += (s, e) => {
				UIArmorSetCatalogueItemSlot.showItems = showItemsCheckbox.Selected;
				foreach (var item in armorSetSlots) {
					item.needsUpdate = true;
				}
			};
			showItemsCheckbox.Left.Set(0, 0);
			panel.Append(showItemsCheckbox);

			var useDyeCheckbox = new UICheckbox("Use Dye", "Draw armor sets with currently equipped dye");
			useDyeCheckbox.Selected = UIArmorSetCatalogueItemSlot.useDye;
			useDyeCheckbox.OnSelectedChanged += (s, e) => {
				UIArmorSetCatalogueItemSlot.useDye = useDyeCheckbox.Selected;
			};
			useDyeCheckbox.Top.Set(20, 0);
			useDyeCheckbox.Left.Set(0, 0);
			panel.Append(useDyeCheckbox);

			var animateCheckbox = new UICheckbox("Animate", "Mimic player animation");
			animateCheckbox.Selected = UIArmorSetCatalogueItemSlot.animate;
			animateCheckbox.OnSelectedChanged += (s, e) => {
				UIArmorSetCatalogueItemSlot.animate = animateCheckbox.Selected;
			};
			animateCheckbox.Top.Set(40, 0);
			animateCheckbox.Left.Set(0, 0);
			panel.Append(animateCheckbox);

			var accessoriesCheckbox = new UICheckbox("Accessories", "Visualize with current accessories");
			accessoriesCheckbox.Selected = UIArmorSetCatalogueItemSlot.accessories;
			accessoriesCheckbox.OnSelectedChanged += (s, e) => {
				UIArmorSetCatalogueItemSlot.accessories = accessoriesCheckbox.Selected;
			};
			accessoriesCheckbox.Top.Set(60, 0);
			accessoriesCheckbox.Left.Set(0, 0);
			panel.Append(accessoriesCheckbox);

			itemGrid._items.Add(panel);
			itemGrid._innerList.Append(panel);
		}

		internal static void CalculateArmorSets() {
			//new Category("Head", x => x.headSlot != -1, smallHead),
			//new Category("Body", x => x.bodySlot != -1, smallBody),
			//new Category("Legs", x => x.legSlot != -1, smallLegs),
			var testPlayer = new Player();
			testPlayer.whoAmI = 255;
			List<Item> Heads = new List<Item>();
			List<Item> Bodys = new List<Item>();
			List<Item> Legs = new List<Item>();
			for (int type = 1; type < ItemLoader.ItemCount; type++) {
				Item item = new Item();
				item.SetDefaults(type, false);
				if (item.type == 0)
					continue;

				if (item.headSlot != -1)
					Heads.Add(item);
				if (item.bodySlot != -1)
					Bodys.Add(item);
				if (item.legSlot != -1)
					Legs.Add(item);
			}
			sets = new List<Tuple<Item, Item, Item, string, int>>();
			foreach (var head in Heads) {
				foreach (var body in Bodys) {
					foreach (var leg in Legs) {
						testPlayer.statDefense = 0;
						testPlayer.head = head.headSlot;
						testPlayer.body = body.bodySlot;
						testPlayer.legs = leg.legSlot;
						testPlayer.armor[0] = head;
						testPlayer.armor[1] = body;
						testPlayer.armor[2] = leg;

						// TODO: Vanity Set calculation somehow?
						testPlayer.UpdateArmorSets(255);
						if (testPlayer.setBonus != "") {
							string fullSetBonus = testPlayer.setBonus;
							int fullDefenseBonus = testPlayer.statDefense;

							// This section for testing leg-less sets
							testPlayer.legs = -1;
							testPlayer.armor[2] = new Item();
							testPlayer.statDefense = 0;
							testPlayer.UpdateArmorSets(255);
							int noLegsDefenseBonus = testPlayer.statDefense;
							string noLegSetBonus = testPlayer.setBonus;
							testPlayer.legs = leg.legSlot;
							testPlayer.armor[2] = leg;

							// This section for testing head-less sets
							testPlayer.head = -1;
							testPlayer.armor[0] = new Item();
							testPlayer.statDefense = 0;
							testPlayer.UpdateArmorSets(255);
							int noHeadDefenseBonus = testPlayer.statDefense;
							string noHeadSetBonus = testPlayer.setBonus;
							testPlayer.head = head.headSlot;
							testPlayer.armor[0] = head;

							// This section for testing body-less sets
							testPlayer.body = -1;
							testPlayer.armor[1] = new Item();
							testPlayer.statDefense = 0;
							testPlayer.UpdateArmorSets(255);
							int noBodyDefenseBonus = testPlayer.statDefense;
							string noBodySetBonus = testPlayer.setBonus;

							if (noLegSetBonus != "") {
								var tupleToAdd = new Tuple<Item, Item, Item, string, int>(head, body, null, noLegSetBonus, head.defense + body.defense + noLegsDefenseBonus);
								if (!sets.Contains(tupleToAdd))
									sets.Add(tupleToAdd);
							}
							else if (noHeadSetBonus != "") {
								var tupleToAdd = new Tuple<Item, Item, Item, string, int>(null, body, leg, noHeadSetBonus, body.defense + leg.defense + noHeadDefenseBonus);
								if (!sets.Contains(tupleToAdd))
									sets.Add(tupleToAdd);
							}
							else if (noBodySetBonus != "") {
								var tupleToAdd = new Tuple<Item, Item, Item, string, int>(head, null, leg, noBodySetBonus, head.defense + leg.defense + noBodyDefenseBonus);
								if (!sets.Contains(tupleToAdd))
									sets.Add(tupleToAdd);
							}
							else {
								sets.Add(new Tuple<Item, Item, Item, string, int>(head, body, leg, fullSetBonus, head.defense + body.defense + leg.defense + fullDefenseBonus));
							}
						}
					}
				}
			}
			// How to detect "anything goes" sets?
			// Check Head/Body, Head/Legs, etc?

			armorSetSlots = new List<UIArmorSetCatalogueItemSlot>();
			if (armorSetSlots.Count == 0) {
				foreach (var set in sets) {
					var slot = new UIArmorSetCatalogueItemSlot(set);
					armorSetSlots.Add(slot);
				}
			}
		}
	}
}