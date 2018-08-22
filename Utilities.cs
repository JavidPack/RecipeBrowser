using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace RecipeBrowser
{
	static class Utilities
	{
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
	}
}
