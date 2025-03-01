using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.TagHandlers {
	/// <summary>
	/// Handles [image/...] tags, allowing to display images/textures where a text would be displayed.
	/// </summary>
	public class ImageTagHandler : ITagHandler {
		/// <summary>
		/// Simple structure to hold the options for the image tag.
		/// </summary>
		private class ImageTagOptions {
			public string Tooltip { get; set; }
			public float Scale { get; set; } = 1f;
			public int VerticalOffset { get; set; }
		}

		/// <summary>
		/// A custom <see cref="TextSnippet"/> that displays the given texture as an image instead of text.
		/// </summary>
		private class ImageTagSnippet : TextSnippet {
			private static Dictionary<string, Asset<Texture2D>> _textureCache = new Dictionary<string, Asset<Texture2D>>();
			private string Tooltip { get; }
			private Asset<Texture2D> Texture { get; }
			public int VerticalOffset { get; }

			/// <summary>
			/// Creates a new snippet to display an image in place of text.
			/// </summary>
			/// <param name="texturePath">The path to the texture to be shown.</param>
			/// <param name="tooltip">The tooltip to be shown when hovering over the image.</param>
			/// <param name="scale">The scale of the image.</param>
			/// <param name="vOffset">Vertical offset to move the image X rows up or down.</param>
			public ImageTagSnippet(string texturePath, string tooltip = null, float scale = 1f, int vOffset = 0) : base("", Color.White, scale) {
				DeleteWhole = true;
				Tooltip = tooltip;
				VerticalOffset = vOffset;

				if (!_textureCache.TryGetValue(texturePath, out Asset<Texture2D> texture)) {
					if (ModContent.HasAsset(texturePath)) {
						texture = ModContent.Request<Texture2D>(texturePath);
						_textureCache[texturePath] = texture;
					} else {
						RecurrentErrorLogger.MaybeLog($"ImageTagSnippet: Texture not found: {texturePath}");
					}
				}
				Texture = texture;
			}

			/// <summary>
			/// Called when the mouse hovers over the image. Displays the tooltip if one have been set.
			/// </summary>
			public override void OnHover() {
				if (!string.IsNullOrWhiteSpace(Tooltip)) {
					// TODO: This might lead to conflicts between tooltips, which would make the tooltip flicker. Make sure to handle this case.
					UICommon.TooltipMouseText(Tooltip);
				}
			}

			public override Color GetVisibleColor() {
				return Color;
			}

			/// <inheritdoc/>
			/// <remarks>
			/// This draws the image at a given position with the configured scale and vertical offset.
			/// </remarks>
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f) {
				if (Texture == null) {
					size = Vector2.Zero;
					return false;
				}

				Vector2 textureSize = Texture.Size();
				textureSize.X *= Scale;
				textureSize.Y *= Scale;

				textureSize.Y += VerticalOffset;

				size = textureSize;

				// TODO: Its not clear why this comparison is done and what it is supposed to do.
				if (!justCheckingString && color != Color.Black) {
					spriteBatch.Draw(Texture.Value, position + new Vector2(0, VerticalOffset), null, color, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
				}

				return true;
			}

			/// <summary>
			/// Returns the correct length of the image based on the configured scale.
			/// </summary>
			/// <param name="font">The font to be used to calculate the length. This is not used in this implementation.</param>
			/// <returns>The length of the image.</returns>
			public override float GetStringLength(DynamicSpriteFont font) {
				return Texture.Size().X * Scale;
			}
		}

		/// <summary>
		/// A dictionary that maps the option key to the corresponding parser function.
		/// The parser function is responsible for parsing the option value and setting the corresponding property in the <see cref="ImageTagOptions"/> object.
		/// It should have the signature: <c>void ParserFunction(string optionValue, ImageTagOptions options)</c>
		/// </summary>
		private static readonly Dictionary<char, Action<string, ImageTagOptions>> optionParsers = new Dictionary<char, Action<string, ImageTagOptions>> {
			['t'] = ParseTooltip,
			['s'] = ParseScale,
			['v'] = ParseVerticalOffset
		};

		/// <summary>
		/// Parses the raw options string into a dictionary of key-value pairs. e.g.: "tBugNet,s0.8,v2" = { 't' => "BugNet", 's' => 0.8f, 'v' => 2 }
		/// </summary>
		/// <param name="options">A comma-separated list of pairs, where the first char is the key, and the rest is the value.</param>
		/// <returns></returns>
		private Dictionary<char, string> ParseRawOptions(string options) {
			Dictionary<char, string> parsedOptions = new Dictionary<char, string>();

			if (string.IsNullOrWhiteSpace(options)) {
				return parsedOptions;
			}

			string[] pairs = options.Split(',');
			foreach (string pair in pairs) {
				// TODO: Two characters are always expected, maybe add some logging or error handling.
				if (pair.Length < 2) {
					continue;
				}

				char key = pair[0];
				string value = pair[1..];

				parsedOptions[key] = value;
			}

			return parsedOptions;
		}

		/// <summary>
		/// Handler for the 't' option. Sets the tooltip for the image, that will be shown when hovering over it.
		/// </summary>
		private static void ParseTooltip(string optionValue, ImageTagOptions opts) {
			opts.Tooltip = optionValue.Replace(';', ':');
		}

		/// <summary>
		/// Handler for the 's' option. Sets the scale of the image.
		/// </summary>
		private static void ParseScale(string optionValue, ImageTagOptions opts) {
			if (float.TryParse(optionValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale)) {
				opts.Scale = scale;
			} else {
				opts.Scale = 1f;
			}
		}

		/// <summary>
		/// Handler for the 'v' option. Sets the vertical offset of the image (in rows).
		/// </summary>
		private static void ParseVerticalOffset(string optionValue, ImageTagOptions opts) {
			if (int.TryParse(optionValue, out int offset)) {
				opts.VerticalOffset = offset;
			} else {
				opts.VerticalOffset = 0;
			}
		}

		/// <summary>
		/// Main method of the handler. Parses the given text and options and converts it into a <see cref="TextSnippet"/> if possible.
		/// </summary>
		/// <param name="text">The texture path of the image to be displayed.</param>
		/// <param name="baseColor">The color of the text. This is not used in this implementation.</param>
		/// <param name="options">A comma-separated list of options to configure the image. e.g.: "tBugNet,s0.8,v2"</param>
		/// <returns>A <see cref="ImageTagSnippet"/> that draws the image, or falls back to a simple <see cref="TextSnippet"/> if the image could not be loaded.</returns>
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			ImageTagOptions tagOptions = new ImageTagOptions();
			Dictionary<char, string> parsedKeyValues = ParseRawOptions(options);

			// Apply each existing parser to the extracted option pairs.
			foreach (KeyValuePair<char, string> parsedOption in parsedKeyValues) {
				char key = parsedOption.Key;
				string value = parsedOption.Value;

				if (optionParsers.TryGetValue(key, out Action<string, ImageTagOptions> parser)) {
					parser(value, tagOptions);
				}
			}

			try {
				return new ImageTagSnippet(text, tagOptions.Tooltip, tagOptions.Scale, tagOptions.VerticalOffset);
			} catch {
				return new TextSnippet(text);
			}
		}
	}
}