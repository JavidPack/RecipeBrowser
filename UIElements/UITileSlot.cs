using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ObjectData;
using Terraria.Map;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	class UITileSlot : UIElement
	{
		public Texture2D backgroundTexture => TextureAssets.InventoryBack9.Value;
		public Asset<Texture2D> selectedTexture => UIRecipeSlot.selectedBackgroundTexture;
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

		public override void Update(GameTime gameTime)
		{
			if (texture == null)
			{
				if (!Utilities.tileTextures.ContainsKey(tile))
				{
					Utilities.GenerateTileTexture(tile);
				}
				texture = Utilities.tileTextures[tile];
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
				spriteBatch.Draw(selectedTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

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
				Main.hoverItemName = Utilities.GetTileName(tile);
			}
		}
	}

	class UITileNoSlot : UIElement
	{
		internal float scale = .75f;
		public int order;
		public int tile;
		Texture2D texture;

		public UITileNoSlot(int tile, int order, float scale = 0.75f)
		{
			this.scale = scale;
			this.order = order;
			this.tile = tile;
			this.Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
			this.Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
		}

		public override int CompareTo(object obj)
		{
			UITileSlot other = obj as UITileSlot;
			return -order.CompareTo(other.order);
		}

		public override void Update(GameTime gameTime)
		{
			if (texture == null)
			{
				if (!Utilities.tileTextures.ContainsKey(tile))
				{
					Utilities.GenerateTileTexture(tile);
				}
				texture = Utilities.tileTextures[tile];
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (texture == null)
				return;

			CalculatedStyle dimensions = base.GetInnerDimensions();
			Rectangle rectangle = dimensions.ToRectangle();

			int height = texture.Height;
			int width = texture.Width;
			float drawScale = 1f; // larger, uncomment below
			float availableWidth = (float)TextureAssets.InventoryBack9.Value.Width * scale;
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
			Vector2 vector = TextureAssets.InventoryBack9.Size() * scale;
			Vector2 position2 = dimensions.Position() + vector / 2f - texture.Size() * drawScale / 2f;
			//Vector2 origin = texture.Size() * (1f / 2f - 0.5f);
			spriteBatch.Draw(texture, position2, null, Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

			if (IsMouseHovering)
			{
				Main.hoverItemName = Utilities.GetTileName(tile);
			}
		}
	}
}
