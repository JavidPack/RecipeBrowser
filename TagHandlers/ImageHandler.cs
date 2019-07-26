using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;
using Terraria.UI.Chat;
using Newtonsoft.Json.Linq;
using Terraria;
using ReLogic.Graphics;
using System.Net;
using System.IO;
using System;
using Terraria.ModLoader;

namespace RecipeBrowser.TagHandlers
{
	public class ImageTagHandler : ITagHandler
	{
		private class ImageSnippet : TextSnippet
		{
			Texture2D texture;
			Texture2D Texture
			{
				get
				{
					if (texture == null)
					{
						texture = ModContent.GetTexture(texturePath);
					}
					return texture;
				}
			}

			string texturePath;
			public ImageSnippet(string texturePath) : base("")
			{
				DeleteWhole = true;
				this.texturePath = texturePath;
			}

			public override Color GetVisibleColor()
			{
				return Color;
			}

			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1f)
			{
				size = Texture.Size();
				if (!justCheckingString && color != Color.Black)
				{
					spriteBatch.Draw(Texture, position, color);
				}
				return true;
			}

			public override float GetStringLength(DynamicSpriteFont font)
			{
				return Texture.Size().X;
			}
		}

		TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
		{
			// TODO: option for scale or absolute size
			// TODO: option for tooltip/translation key
			// TODO: option for frame (animated?)
			if (ModContent.TextureExists(text))
			{
				return new ImageSnippet(text);
			}
			return new TextSnippet(text);
		}
	}
}
