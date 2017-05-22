using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RecipeBrowser.UI
{
	internal class UIImage : UIView
	{
		private Texture2D texture;

		private SpriteEffects _spriteEfftct;

		private Rectangle? sourceRectangle = null;

		public Texture2D Texture
		{
			get
			{
				return this.texture;
			}
			set
			{
				this.texture = value;
			}
		}

		private float width
		{
			get
			{
				return (float)this.texture.Width;
			}
		}

		private float height
		{
			get
			{
				return (float)this.texture.Height;
			}
		}

		public SpriteEffects SpriteEffect
		{
			get
			{
				return this._spriteEfftct;
			}
			set
			{
				this._spriteEfftct = value;
			}
		}

		public Rectangle SourceRectangle
		{
			get
			{
				if (!this.sourceRectangle.HasValue)
				{
					this.sourceRectangle = new Rectangle?(default(Rectangle));
				}
				return this.sourceRectangle.Value;
			}
			set
			{
				this.sourceRectangle = new Rectangle?(value);
			}
		}

		public int SR_X
		{
			get
			{
				return this.SourceRectangle.X;
			}
			set
			{
				this.SourceRectangle = new Rectangle(value, this.SourceRectangle.Y, this.SourceRectangle.Width, this.SourceRectangle.Height);
			}
		}

		public int SR_Y
		{
			get
			{
				return this.SourceRectangle.X;
			}
			set
			{
				this.SourceRectangle = new Rectangle(this.SourceRectangle.X, value, this.SourceRectangle.Width, this.SourceRectangle.Height);
			}
		}

		public int SR_Width
		{
			get
			{
				return this.SourceRectangle.X;
			}
			set
			{
				this.SourceRectangle = new Rectangle(this.SourceRectangle.X, this.SourceRectangle.Y, value, this.SourceRectangle.Height);
			}
		}

		public int SR_Height
		{
			get
			{
				return this.SourceRectangle.X;
			}
			set
			{
				this.SourceRectangle = new Rectangle(this.SourceRectangle.X, this.SourceRectangle.Y, this.SourceRectangle.Width, value);
			}
		}

		public UIImage(Texture2D texture)
		{
			this.Texture = texture;
		}

		public UIImage()
		{
		}

		protected override float GetWidth()
		{
			if (this.sourceRectangle.HasValue)
			{
				return (float)this.sourceRectangle.Value.Width * base.Scale;
			}
			return this.width * base.Scale;
		}

		protected override float GetHeight()
		{
			if (this.sourceRectangle.HasValue)
			{
				return (float)this.sourceRectangle.Value.Height * base.Scale;
			}
			return this.height * base.Scale;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (base.Visible)
			{
				spriteBatch.Draw(this.texture, base.DrawPosition, this.sourceRectangle, base.ForegroundColor * base.Opacity, 0f, base.Origin / base.Scale, base.Scale, this.SpriteEffect, 0f);
			}
			base.Draw(spriteBatch);
		}
	}
}
