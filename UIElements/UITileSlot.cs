using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System;
using Terraria.ObjectData;
using Terraria.Map;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	class UITileSlot : UIElement
	{
		public Texture2D backgroundTexture => Main.inventoryBack9Texture;
		public Texture2D selectedTexture => UIRecipeSlot.selectedBackgroundTexture;
		internal float scale = .75f;
		public int order; // usage count
		public int tile;
		public bool selected;
		Texture2D texture;

		public UITileSlot(int tile, int order, float scale = 0.75f)
		{
			this.scale = scale;
			this.order = order;
			this.tile = tile;
			this.Width.Set(backgroundTexture.Width * scale, 0f);
			this.Height.Set(backgroundTexture.Height * scale, 0f);
		}

		public override void Click(UIMouseEvent evt)
		{
			//RecipeCatalogueUI.instance.ToggleTileChooser(false);
			//RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(item.type);
			//RecipeCatalogueUI.instance.TileLookupRadioButton.SetDisabled(false);
			//RecipeCatalogueUI.instance.TileLookupRadioButton.Selected = true;
			if (selected)
				RecipeCatalogueUI.instance.Tile = -1;
			else
				RecipeCatalogueUI.instance.Tile = tile;
		}

		public override void DoubleClick(UIMouseEvent evt)
		{
			//RecipeCatalogueUI.instance.queryItem.ReplaceWithFake(0);
			//RecipeCatalogueUI.instance.Tile = tile;
			//RecipeCatalogueUI.instance.ToggleTileChooser(false);
		}

		public override int CompareTo(object obj)
		{
			UITileSlot other = obj as UITileSlot;
			return -order.CompareTo(other.order);
		}

		// TODO, move this to a real update to prevent purple flash
		public override void Update(GameTime gameTime)
		{
			//texture = null;
			if (texture == null)
			{
				Main.instance.LoadTiles(tile);

				var tileObjectData = TileObjectData.GetTileData(tile, 0, 0);
				if (tileObjectData == null)
				{
					texture = Main.magicPixel;
					return;
				}

				int width = tileObjectData.Width;
				int height = tileObjectData.Height;
				int padding = tileObjectData.CoordinatePadding;

				Main.spriteBatch.End();
				RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width * 16, height * 16);
				Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
				Main.instance.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Begin();

				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						Main.spriteBatch.Draw(Main.tileTexture[tile], new Vector2(i * 16, j * 16), new Rectangle(i * 16 + i * padding, j * 16 + j * padding, 16, 16), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
					}
				}

				Main.spriteBatch.End();
				Main.instance.GraphicsDevice.SetRenderTarget(null);
				Main.spriteBatch.Begin();
				texture = renderTarget;
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (texture == null)
				return;

			CalculatedStyle dimensions = base.GetInnerDimensions();
			Rectangle rectangle = dimensions.ToRectangle();
			spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			if (selected)
				spriteBatch.Draw(selectedTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			int height = texture.Height;
			int width = texture.Width;
			float drawScale = 1f; // larger, uncomment below
			float availableWidth = (float)backgroundTexture.Width * scale;
			if (width /** drawScale*/ > availableWidth || height /** drawScale*/ > availableWidth)
			{
				if (width > height)
				{
					drawScale = availableWidth / width;
				}
				else
				{
					drawScale = availableWidth / height;
				}
			}
			drawScale *= scale;
			Vector2 vector = backgroundTexture.Size() * scale;
			Vector2 position2 = dimensions.Position() + vector / 2f - texture.Size() * drawScale / 2f;
			//Vector2 origin = texture.Size() * (1f / 2f - 0.5f);
			spriteBatch.Draw(texture, position2, null, Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

			if (IsMouseHovering)
			{
				string tileName = Lang.GetMapObjectName(MapHelper.TileToLookup(tile, 0));
				if (tileName == "")
				{
					if (tile < TileID.Count)
						tileName = $"Tile {tile}";
					else
						tileName = Terraria.ModLoader.TileLoader.GetTile(tile).Name + " (err no entry)";
				}
				Main.hoverItemName = tileName;
			}
		}
	}
}
