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
using ReLogic.Content;
using Terraria.ModLoader;

namespace RecipeBrowser.TagHandlers
{
	public class ImageTagHandler : ITagHandler
	{
		private class ImageSnippet : TextSnippet
		{
			Asset<Texture2D> texture;
			Asset<Texture2D> Texture
			{
				get
				{
					if (texture == null)
					{
						texture = ModContent.Request<Texture2D>(texturePath);
					}
					return texture;
				}
			}
			private string tooltip;
			public int vOffset;
			private float otherScale;

			string texturePath;
			public ImageSnippet(string texturePath, string tooltip = null, float scale = 1f, float otherScale = 1f) : base("", Color.White, scale)
			{
				DeleteWhole = true;
				this.texturePath = texturePath;
				this.tooltip = tooltip;
				this.otherScale = otherScale;
			}

			public override void OnHover()
			{
				if (!string.IsNullOrEmpty(tooltip))
				{
					Main.hoverItemName = tooltip;
				}
			}

			public override Color GetVisibleColor()
			{
				return Color;
			}

			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1f)
			{
				size = Texture.Size() * new Vector2(otherScale) + new Vector2(0, vOffset);
				if (!justCheckingString && color != Color.Black)
				{
					//size = Texture.Size() * new Vector2(1, scale) + new Vector2(0, vOffset); // TODO: Why was `new Vector2( 1, scale)`??
					//spriteBatch.Draw(Texture, position, color);
					spriteBatch.Draw(Texture.Value, position + new Vector2(0, vOffset), null, color, 0, Vector2.Zero, otherScale, SpriteEffects.None, 0);
					//if (scale > 1)
					//	Main.NewText(size);
					//size = Vector2.Zero;
				}
				return true;
			}

			public override float GetStringLength(DynamicSpriteFont font)
			{
				return Texture.Size().X * Scale;
			}
		}

		TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
		{
			// TODO: option for scale or absolute size
			// TODO: option for tooltip/translation key
			// TODO: option for frame (animated?)
			// tTooltip
			// f0;0;20;20
			// 
			string tooltip = null;
			float scale = 1f;
			int vOffset = 0;
			string[] array = options.Split(',');
			foreach (var option in array)
			{
				if (option.Length != 0)
				{
					if (option[0] == 't')
						tooltip = option.Substring(1).Replace(';', ':');
					if (option[0] == 's')
						float.TryParse(option.Substring(1), out scale);
					if (option[0] == 'v')
						int.TryParse(option.Substring(1), out vOffset);
				}
				//return new TextSnippet("<" + text.Replace("\\[", "[").Replace("\\]", "]") + ">", baseColor, 1f);
			}
			if (ModContent.HasAsset(text))
			{
				return new ImageSnippet(text, tooltip, 1f, scale)
				{
					vOffset = vOffset
				};
			}
			return new TextSnippet(text);
		}
	}
}
