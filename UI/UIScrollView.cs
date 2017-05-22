using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace RecipeBrowser.UI
{
	internal class UIScrollView : UIView
	{
		private RasterizerState _rasterizerState = new RasterizerState
		{
			ScissorTestEnable = true
		};

		private static Texture2D ScrollbgTexture = Terraria.ModLoader.ModLoader.GetTexture("RecipeBrowser/UI/Images.UIKit.scrollbgEdge");// UIView.GetEmbeddedTexture("Images.UIKit.scrollbgEdge.png");

		private static Texture2D scrollbgFill;

		protected UIScrollBar scrollBar = new UIScrollBar();

		private float width = 150f;

		private float height = 250f;

		private float contentHeight;

		private bool dragging;

		private Vector2 dragAnchor = Vector2.Zero;

		public bool OverrideDrawAndUpdate;

		private float scrollPosition;

		private static Texture2D ScrollbgFill
		{
			get
			{
				if (UIScrollView.scrollbgFill == null)
				{
					Color[] array = new Color[UIScrollView.ScrollbgTexture.Width * UIScrollView.ScrollbgTexture.Height];
					UIScrollView.ScrollbgTexture.GetData<Color>(array);
					Color[] array2 = new Color[UIScrollView.ScrollbgTexture.Width];
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = array[i + (UIScrollView.ScrollbgTexture.Height - 1) * UIScrollView.ScrollbgTexture.Width];
					}
					UIScrollView.scrollbgFill = new Texture2D(UIView.graphics, array2.Length, 1);
					UIScrollView.scrollbgFill.SetData<Color>(array2);
				}
				return UIScrollView.scrollbgFill;
			}
		}

		public float ContentHeight
		{
			get
			{
				return this.contentHeight;
			}
			set
			{
				this.contentHeight = value;
			}
		}

		public float ScrollPosition
		{
			get
			{
				float result = this.scrollPosition;
				if (this.scrollPosition < 0f || base.Height > this.ContentHeight)
				{
					result = 0f;
				}
				if (this.scrollPosition > this.ContentHeight)
				{
					result = this.ContentHeight;
				}
				return result;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f;
				}
				if (value > this.ContentHeight)
				{
					value = this.ContentHeight;
				}
				this.scrollPosition = value;
			}
		}

		public UIScrollView()
		{
			this.scrollBar.onMouseDown += new UIView.ClickEventHandler(this.scrollBar_onMouseDown);
			this.AddChild(this.scrollBar);
		}

		private void scrollBar_onMouseDown(object sender, byte button)
		{
			if (button == 0)
			{
				this.dragging = true;
				this.dragAnchor = new Vector2((float)UIView.MouseX, (float)UIView.MouseY) - this.scrollBar.DrawPosition;
			}
		}

		protected override float GetHeight()
		{
			return this.height;
		}

		protected override float GetWidth()
		{
			return this.width;
		}

		protected override void SetWidth(float width)
		{
			this.width = width;
		}

		protected override void SetHeight(float height)
		{
			this.height = height;
		}

		public override void AddChild(UIView view)
		{
			if (this.children.Count > 0 && this.contentHeight > 0f)
			{
				float num = this.ScrollPosition / this.ContentHeight * (this.ContentHeight - base.Height);
				view.Offset = new Vector2(view.Offset.X, -num);
			}
			base.AddChild(view);
		}

		public void ClearContent()
		{
			if (this.children.Count > 1)
			{
				for (int i = 1; i < this.children.Count; i++)
				{
					base.RemoveChild(base.GetChild(i));
				}
			}
		}

		public override void Update()
		{
			if (this.OverrideDrawAndUpdate)
			{
				this.scrollBar.Update();
			}
			else
			{
				base.Update();
			}
			if (!UIView.MouseLeftButton)
			{
				this.dragging = false;
			}
			float num = base.Height / this.ContentHeight * base.Height;
			if (num < 20f)
			{
				num = 20f;
			}
			if (num > base.Height)
			{
				num = base.Height;
			}
			float num2 = base.Height - num;
			if (this.dragging)
			{
				float num3 = (float)UIView.MouseY - base.DrawPosition.Y + base.Origin.Y - this.dragAnchor.Y;
				float num4 = num3 / num2;
				this.ScrollPosition = this.ContentHeight * num4;
				if (this.scrollPosition > 0f)
				{
					Console.WriteLine();
				}
				using (List<UIView>.Enumerator enumerator = this.children.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						UIView current = enumerator.Current;
						if (current.GetType() != typeof(UIScrollBar))
						{
							float num5 = this.ScrollPosition / this.ContentHeight * (this.ContentHeight - base.Height);
							current.Offset = new Vector2(current.Offset.X, -num5);
						}
					}
					goto IL_1EC;
				}
			}
			if (UIView.ScrollAmount != 0 && this.IsMouseInside())
			{
				this.ScrollPosition -= (float)UIView.ScrollAmount;
				foreach (UIView current2 in this.children)
				{
					if (current2.GetType() != typeof(UIScrollBar))
					{
						float num6 = this.ScrollPosition / this.ContentHeight * (this.ContentHeight - base.Height);
						current2.Offset = new Vector2(current2.Offset.X, -num6);
					}
				}
			}
			IL_1EC:
			float y = this.ScrollPosition / this.ContentHeight * num2;
			this.scrollBar.Height = num;
			this.scrollBar.Position = new Vector2(base.Width - this.scrollBar.Width, y);
		}

		private void DrawScrollbg(SpriteBatch spriteBatch)
		{
			Vector2 drawPosition = base.DrawPosition;
			float num = base.Height - (float)(UIScrollView.ScrollbgTexture.Height * 2);
			drawPosition.X += base.Width - (float)UIScrollView.ScrollbgTexture.Width;
			spriteBatch.Draw(UIScrollView.ScrollbgTexture, drawPosition, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.None, 0f);
			drawPosition.Y += (float)UIScrollView.ScrollbgTexture.Height;
			spriteBatch.Draw(UIScrollView.ScrollbgFill, drawPosition - base.Origin, null, Color.White, 0f, Vector2.Zero, new Vector2(1f, num), SpriteEffects.None, 0f);
			drawPosition.Y += num;
			spriteBatch.Draw(UIScrollView.ScrollbgTexture, drawPosition, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.FlipVertically, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 vector = base.DrawPosition - base.Origin;
			Utils.DrawInvBG(spriteBatch, vector.X, vector.Y, base.Width, base.Height, new Color(33, 15, 91, 255) * 0.685f);
			this.DrawScrollbg(spriteBatch);
			if (vector.X <= (float)Main.screenWidth && vector.Y <= (float)Main.screenHeight && vector.X + base.Width >= 0f && vector.Y + base.Height >= 0f)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, this._rasterizerState);
				Rectangle scissorRectangle = new Rectangle((int)vector.X, (int)vector.Y, (int)base.Width, (int)base.Height);
				if (scissorRectangle.X < 0)
				{
					scissorRectangle.Width += scissorRectangle.X;
					scissorRectangle.X = 0;
				}
				if (scissorRectangle.Y < 0)
				{
					scissorRectangle.Height += scissorRectangle.Y;
					scissorRectangle.Y = 0;
				}
				if ((float)scissorRectangle.X + base.Width > (float)Main.screenWidth)
				{
					scissorRectangle.Width = Main.screenWidth - scissorRectangle.X;
				}
				if ((float)scissorRectangle.Y + base.Height > (float)Main.screenHeight)
				{
					scissorRectangle.Height = Main.screenHeight - scissorRectangle.Y;
				}
				Rectangle scissorRectangle2 = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
				if (this.OverrideDrawAndUpdate)
				{
					this.scrollBar.Draw(spriteBatch);
				}
				else
				{
					base.Draw(spriteBatch);
				}
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle2;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
				this.scrollBar.Draw(spriteBatch);
			}
		}
	}
}
