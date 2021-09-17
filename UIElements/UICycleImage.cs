using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ReLogic.Content;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser.UIElements
{
	internal class UICycleImage : UIElement
	{
		private Asset<Texture2D> texture;
		private int _drawWidth;
		private int _drawHeight;
		private int padding;
		private int textureOffsetX;
		private int textureOffsetY;
		private int states;
		internal string[] hoverTexts;

		public event EventHandler OnStateChanged;

		private int currentState = 0;
		public int CurrentState
		{
			get { return currentState; }
			set
			{
				if (value != currentState)
				{
					currentState = value;
					OnStateChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public UICycleImage(Asset<Texture2D> texture, int states, string[] hoverTexts, int width, int height, int textureOffsetX = 0, int textureOffsetY = 0, int padding = 2)
		{
			this.texture = texture;
			this._drawWidth = width;
			this._drawHeight = height;
			this.textureOffsetX = textureOffsetX;
			this.textureOffsetY = textureOffsetY;
			this.Width.Set((float)width, 0f);
			this.Height.Set((float)height, 0f);
			this.states = states;
			this.padding = padding;
			this.hoverTexts = hoverTexts;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetDimensions();
			Point point = new Point(textureOffsetX, textureOffsetY + ((padding + _drawHeight) * currentState));
			Color color = base.IsMouseHovering ? Color.White : Color.Silver;
			spriteBatch.Draw(texture.Value, new Rectangle((int)dimensions.X, (int)dimensions.Y, this._drawWidth, this._drawHeight), new Rectangle?(new Rectangle(point.X, point.Y, this._drawWidth, this._drawHeight)), color);
			if (IsMouseHovering)
			{
				Main.hoverItemName = hoverTexts[CurrentState];
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			CurrentState = (currentState + 1) % states;
			base.Click(evt);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			CurrentState = (currentState + states - 1) % states;
			base.RightClick(evt);
		}
	}
}
