using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace RecipeBrowser.UI
{
	internal class UILabel : UIView
	{
		public SpriteFont font;

		private string text = "";

		private bool textOutline;

		private float width;

		private float height;

		public static SpriteFont defaultFont
		{
			get
			{
				return Main.fontDeathText;
			}
		}

		public string Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
				this.SetWidthHeight();
			}
		}

		public bool TextOutline
		{
			get
			{
				return this.textOutline;
			}
			set
			{
				this.textOutline = value;
			}
		}

		public UILabel(string text)
		{
			this.font = UILabel.defaultFont;
			this.Text = text;
		}

		public UILabel()
		{
			this.font = UILabel.defaultFont;
			this.Text = "";
		}

		protected override Vector2 GetOrigin()
		{
			return base.GetOrigin();
		}

		private void SetWidthHeight()
		{
			if (this.Text != null)
			{
				Vector2 vector = this.font.MeasureString(this.Text);
				this.width = vector.X;
				this.height = vector.Y;
				return;
			}
			this.width = 0f;
			this.height = 0f;
		}

		protected override float GetWidth()
		{
			return this.width * base.Scale;
		}

		protected override float GetHeight()
		{
			if (this.height == 0f)
			{
				return this.font.MeasureString("H").Y * base.Scale;
			}
			return this.height * base.Scale;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (this.Text != null)
			{
				if (this.TextOutline)
				{
					Utils.DrawBorderStringFourWay(spriteBatch, this.font, this.Text, base.DrawPosition.X, base.DrawPosition.Y, base.ForegroundColor, Color.Black, base.Origin / base.Scale, base.Scale);
				}
				else
				{
					spriteBatch.DrawString(this.font, this.Text, base.DrawPosition, base.ForegroundColor, 0f, base.Origin / base.Scale, base.Scale, SpriteEffects.None, 0f);
				}
			}
			base.Draw(spriteBatch);
		}
	}
}
