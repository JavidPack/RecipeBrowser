using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ObjectData;

namespace RecipeBrowser
{
	static class Utilities
	{
		//public static Asset<Texture2D> ToAsset(this Texture2D texture)
		//{
		//	using MemoryStream stream = new();
		//	texture.SaveAsPng(stream, texture.Width, texture.Height);
		//	stream.Position = 0;
		//	return RecipeBrowser.instance.Assets.CreateUntracked<Texture2D>(stream, "any.png");
		//}

		internal static Texture2D StackResizeImage(Asset<Texture2D>[] texture2D, int desiredWidth, int desiredHeight)
		{
			foreach (Asset<Texture2D> asset in texture2D)
				asset.Wait?.Invoke();

			return StackResizeImage(texture2D.Select(asset => asset.Value).ToArray(), desiredWidth, desiredHeight);
		}

		internal static Texture2D StackResizeImage(Texture2D[] texture2D, int desiredWidth, int desiredHeight)
		{
			float overlap = .5f;
			float totalScale = 1 / (1f + ((1 - overlap) * (texture2D.Length - 1)));
			int newWidth = (int)(desiredWidth * totalScale);
			int newHeight = (int)(desiredHeight * totalScale);
			//var texture2Ds = texture2D.Select(x => ResizeImage(x, newWidth, newHeight));

			RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			int index = 0;
			foreach (var texture in texture2D)
			{
				float scale = 1;
				if (texture.Width > newWidth || texture.Height > newHeight)
				{
					if (texture.Height > texture.Width)
						scale = (float)newHeight / texture.Height;
					else
						scale = (float)newWidth / texture.Width;
				}

				Vector2 position = new Vector2(newWidth / 2, newHeight / 2);
				position += new Vector2(index * (1 - overlap) * newWidth, index * (1 - overlap) * newHeight);
				Main.spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0f);
				index++;
			}
			Main.spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);

			Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
			Color[] content = new Color[desiredWidth * desiredHeight];
			renderTarget.GetData<Color>(content);
			mergedTexture.SetData<Color>(content);

			return mergedTexture;
		}

		internal static Texture2D ResizeImage(Asset<Texture2D> asset, int desiredWidth, int desiredHeight)
		{
			asset.Wait?.Invoke();
			return ResizeImage(asset.Value, desiredWidth, desiredHeight);
		}

		internal static Texture2D ResizeImage(Texture2D texture2D, int desiredWidth, int desiredHeight)
		{
			RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, desiredWidth, desiredHeight);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			float scale = 1;
			if (texture2D.Width > desiredWidth || texture2D.Height > desiredHeight)
			{
				if (texture2D.Height > texture2D.Width)
					scale = (float)desiredWidth / texture2D.Height;
				else
					scale = (float)desiredWidth / texture2D.Width;
			}

			//new Vector2(texture2D.Width / 2 * scale, texture2D.Height / 2 * scale) desiredWidth/2, desiredHeight/2
			Main.spriteBatch.Draw(texture2D, new Vector2(desiredWidth / 2, desiredHeight / 2), null, Color.White, 0f, new Vector2(texture2D.Width / 2, texture2D.Height / 2), scale, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);

			Texture2D mergedTexture = new Texture2D(Main.instance.GraphicsDevice, desiredWidth, desiredHeight);
			Color[] content = new Color[desiredWidth * desiredHeight];
			renderTarget.GetData<Color>(content);
			mergedTexture.SetData<Color>(content);

			return mergedTexture;
		}

		internal static Dictionary<int, Texture2D> tileTextures;

		internal static void GenerateTileTexture(int tile)
		{
			Texture2D texture;
			Main.instance.LoadTiles(tile);

			var tileObjectData = TileObjectData.GetTileData(tile, 0, 0);
			if (tileObjectData == null)
			{
				tileTextures[tile] = TextureAssets.MagicPixel.Value;
				return;
			}

			int width = tileObjectData.Width;
			int height = tileObjectData.Height;
			int padding = tileObjectData.CoordinatePadding;

			//Main.spriteBatch.End();
			RenderTarget2D renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width * 16, height * 16);
			Main.instance.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.instance.GraphicsDevice.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Main.spriteBatch.Draw(TextureAssets.Tile[tile].Value, new Vector2(i * 16, j * 16), new Rectangle(i * 16 + i * padding, j * 16 + j * padding, 16, 16), Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
				}
			}

			Main.spriteBatch.End();
			Main.instance.GraphicsDevice.SetRenderTarget(null);

			texture = new Texture2D(Main.instance.GraphicsDevice, width * 16, height * 16);
			Color[] content = new Color[width * 16 * height * 16];
			renderTarget.GetData<Color>(content);
			texture.SetData<Color>(content);
			tileTextures[tile] = texture;
		}

		internal static string GetTileName(int tile)
		{
			string tileName = Lang.GetMapObjectName(MapHelper.TileToLookup(tile, 0));
			if (tileName == "")
			{
				if (tile < TileID.Count)
					tileName = $"Tile {tile}";
				else
					tileName = Terraria.ModLoader.TileLoader.GetTile(tile).Name + " (err no entry)";
			}
			return tileName;
		}

		internal static Color textColor = Color.White; // new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
		internal static Color noColor = Color.LightSalmon; // OrangeRed Red
		internal static Color yesColor = Color.LightGreen; // Green
		internal static Color maybeColor = Color.Yellow; // LightYellow LightGoldenrodYellow Yellow    Goldenrod
	}
}
