using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.TagHandlers
{
	// A clone of ItemTagHandler except OnHover doesn't do MouseText.
	public class ItemHoverFixTagHandler : ITagHandler
	{
		private class ItemHoverFixSnippet : TextSnippet
		{
			private Item _item;
			private bool check;

			public ItemHoverFixSnippet(Item item, bool check = false)
				: base("")
			{
				this._item = item;
				this.Color = ItemRarity.GetColor(item.rare);
				this.check = check;
			}

			public override void OnHover()
			{
				//Main.HoverItem = this._item.Clone();
				//Main.instance.MouseText(this._item.Name, this._item.rare, 0, -1, -1, -1, -1);

				//if (true)
				//{
				//	Main.instance.MouseText(this._item.Name, this._item.rare, 0, -1, -1, -1, -1);
				//}
				//else 
				if (true)
				{
					string stack = _item.stack > 1 ? $" ({_item.stack}) ": "";
					Main.hoverItemName = _item.Name + stack + (_item.modItem != null && ModContent.GetInstance<RecipeBrowserClientConfig>().ShowItemModSource ? " [" + _item.modItem.mod.Name + "]" : "");
				}
				else
				{
					//Main.HoverItem = _item.Clone();
					//Main.hoverItemName = Main.HoverItem.Name + (Main.HoverItem.modItem != null ? " [" + Main.HoverItem.modItem.mod.Name + "]" : "");
				}
			}

			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1f)
			{
				float num = 1f;
				float num2 = 1f;
				if (Main.netMode != 2 && !Main.dedServ)
				{
					Texture2D texture2D = Main.itemTexture[this._item.type];
					Rectangle rectangle;
					if (Main.itemAnimations[this._item.type] != null)
					{
						rectangle = Main.itemAnimations[this._item.type].GetFrame(texture2D);
					}
					else
					{
						rectangle = texture2D.Frame(1, 1, 0, 0);
					}
					if (rectangle.Height > 32)
					{
						num2 = 32f / (float)rectangle.Height;
					}
				}
				num2 *= scale;
				num *= num2;
				if (num > 0.75f)
				{
					num = 0.75f;
				}
				if (!justCheckingString && color != Color.Black)
				{
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = scale * num;

					ItemSlot.Draw(spriteBatch, ref this._item, 14, position - new Vector2(10f) * scale * num, Color.White);
					if (check)
					{
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontItemStack, "✓", position + new Vector2(14, 10), Utilities.yesColor, 0f, Vector2.Zero, new Vector2(0.7f));
					}
					Main.inventoryScale = inventoryScale;
				}
				size = new Vector2(32f) * scale * num;
				return true;
			}

			public override float GetStringLength(DynamicSpriteFont font)
			{
				return 32f * this.Scale * 0.65f;
			}
		}

		TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
		{
			Item item = new Item();
			int type;
			bool check = false;
			if (int.TryParse(text, out type))
			{
				item.netDefaults(type);
			}
			if (item.type <= 0)
			{
				return new TextSnippet(text);
			}
			item.stack = 1;
			// options happen here, we add MID (=ModItemData) options
			if (options != null)
			{
				// don't know why all these options here in vanilla,
				// since it only assumed one option (stack OR prefix, since prefixed items don't stack)
				string[] array = options.Split(new char[]
					{
						','
					});
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Length != 0)
					{
						char c = array[i][0];
						int value2;
						// MID is present, we will override
						if (c == 'd')
						{
							item = ItemIO.FromBase64(array[i].Substring(1));
						}
						else if (c == 'o')
						{
							item.SetNameOverride(array[i].Substring(1));
						}
						else if (c == 'c')
						{
							check = true;
						}
						else if (c != 'p')
						{
							int value;
							if ((c == 's' || c == 'x') && int.TryParse(array[i].Substring(1), out value))
							{
								item.stack = Utils.Clamp<int>(value, 1, item.maxStack);
							}
						}
						else if (int.TryParse(array[i].Substring(1), out value2))
						{
							item.Prefix((int)((byte)Utils.Clamp<int>(value2, 0, ModPrefix.PrefixCount)));
						}
					}
				}
			}
			string str = "";
			if (item.stack > 1)
			{
				str = " (" + item.stack + ")";
			}
			return new ItemHoverFixTagHandler.ItemHoverFixSnippet(item, check)
			{
				Text = "[" + item.AffixName() + str + "]",
				CheckForHover = true,
				DeleteWhole = true
			};
		}

		// we do not alter vanilla ways of doing things
		// this can lead to trouble in future patches
		public static string GenerateTag(Item I)
		{
			string text = "[itemhover";
			// assuming we have modded data, simply write the item as base64
			// do not write other option, base64 holds all the info.
			//if (I.modItem != null || I.globalItems.Any())
			//{
			//	text = text + "/d" + ItemIO.ToBase64(I);
			//}
			//else
			{
				if (I.prefix != 0)
				{
					text = text + "/p" + I.prefix;
				}
				if (I.stack != 1)
				{
					text = text + "/s" + I.stack;
				}
			}

			object obj = text;
			return string.Concat(new object[]
				{
					obj,
					":",
					I.netID,
					"]"
				});
		}

		public static string GenerateTag(int type, int stack, string nameOverride = null, bool check = false)
		{
			List<string> additionals = new List<string>();
			if (!string.IsNullOrEmpty(nameOverride))
				additionals.Add($"o{nameOverride}");
			if (check)
				additionals.Add($"c");
			if (stack > 1)
				additionals.Add($"s{stack}");
			if (additionals.Count > 0)
			{
				string options = "/" + string.Join(",", additionals);
				return $"[itemhover{options}:{type}]";
			}
			return $"[itemhover:{type}]";
		}
	}
}
