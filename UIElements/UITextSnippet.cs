using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RecipeBrowser.UIElements
{
	// Same as Terraria.GameContent.UI.Elements.UIText except Hover and Click of TextSnippets are supported
	class UITextSnippet : UIElement
	{
		private object _text = "";
		private float _textScale = 1f;
		private Vector2 _textSize = Vector2.Zero;
		private bool _isLarge;
		private Color _color = Color.White;

		public string Text
		{
			get
			{
				return this._text.ToString();
			}
		}

		public Color TextColor
		{
			get
			{
				return this._color;
			}
			set
			{
				this._color = value;
			}
		}

		public string HoverText { get; set; }

		public UITextSnippet(string text, float textScale = 1f, bool large = false)
		{
			this.InternalSetText(text, textScale, large);
		}

		public UITextSnippet(LocalizedText text, float textScale = 1f, bool large = false)
		{
			this.InternalSetText(text, textScale, large);
		}

		public override void Recalculate()
		{
			this.InternalSetText(this._text, this._textScale, this._isLarge);
			base.Recalculate();
		}

		public void SetText(string text)
		{
			this.InternalSetText(text, this._textScale, this._isLarge);
		}

		public void SetText(LocalizedText text)
		{
			this.InternalSetText(text, this._textScale, this._isLarge);
		}

		public void SetText(string text, float textScale, bool large)
		{
			this.InternalSetText(text, textScale, large);
		}

		public void SetText(LocalizedText text, float textScale, bool large)
		{
			this.InternalSetText(text, textScale, large);
		}

		private void InternalSetText(object text, float textScale, bool large)
		{
			DynamicSpriteFont dynamicSpriteFont = large ? Main.fontDeathText : Main.fontMouseText;
			//Vector2 textSize = new Vector2(dynamicSpriteFont.MeasureString(text.ToString()).X, large ? 32f : 16f) * textScale;
			_textSize = ChatManager.GetStringSize(dynamicSpriteFont, Text, new Vector2(textScale));
			this._text = text;
			this._textScale = textScale;
			//this._textSize = textSize;
			this._isLarge = large;
			this.MinWidth.Set(_textSize.X + this.PaddingLeft + this.PaddingRight, 0f);
			this.MinHeight.Set(_textSize.Y + this.PaddingTop + this.PaddingBottom, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			//spriteBatch.Draw(Main.magicPixel, GetDimensions().ToRectangle(), Color.Red * 0.5f);

			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();
			if (this._isLarge)
			{
				pos.Y -= 10f * this._textScale;
			}
			else
			{
				pos.Y -= 2f * this._textScale;
			}
			pos.X += (innerDimensions.Width - this._textSize.X) * 0.5f;
			//if (this._isLarge)
			//{
			//	Utils.DrawBorderStringBig(spriteBatch, this.Text, pos, this._color, this._textScale, 0f, 0f, -1);
			//	return;
			//}
			//Utils.DrawBorderString(spriteBatch, this.Text, pos, this._color, this._textScale, 0f, 0f, -1);

			if (IsMouseHovering)
				Main.hoverItemName = HoverText;

			var font = _isLarge ? Main.fontDeathText : Main.fontMouseText;
			int hoveredSnippet = -1;
			TextSnippet[] textSnippets = ChatManager.ParseMessage(Text, Color.White).ToArray();
			ChatManager.ConvertNormalSnippets(textSnippets);

			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, textSnippets, pos, 0f, Vector2.Zero, new Vector2(_textScale), out hoveredSnippet);
			if (hoveredSnippet > -1)
			{
				// annoying click. Main.NewText(hoveredSnippet);
				textSnippets[hoveredSnippet].OnHover();
				//if (Main.mouseLeft && Main.mouseLeftRelease)
				//{
				//	textSnippets[hoveredSnippet].OnClick();
				//}
			}
		}
	}
}
