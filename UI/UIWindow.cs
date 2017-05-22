using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace RecipeBrowser.UI
{
	internal class UIWindow : UIView
	{
		private bool clickAndDrag = true;

		private bool dragging;

		private Vector2 dragAnchor = Vector2.Zero;

		private float width = 500f;

		private float height = 300f;

		private bool _constrainInsideParent = false;

		public bool CanMove;

		public bool ClickAndDrag
		{
			get
			{
				return this.clickAndDrag;
			}
			set
			{
				this.clickAndDrag = value;
			}
		}

		public UIWindow()
		{
			base.BackgroundColor = new Color(33, 15, 91, 255) * 0.685f;
			base.onMouseDown += new UIView.ClickEventHandler(this.UIWindow_onMouseDown);
			base.onMouseUp += new UIView.ClickEventHandler(this.UIWindow_onMouseUp);
		}

		private void UIWindow_onMouseUp(object sender, byte button)
		{
			if (this.dragging)
			{
				this.dragging = false;
			}
		}

		private void UIWindow_onMouseDown(object sender, byte button)
		{
			if (this.CanMove && button == 0)
			{
				this.dragging = true;
				this.dragAnchor = new Vector2((float)UIView.MouseX, (float)UIView.MouseY) - base.DrawPosition;
			}
		}

		protected override void SetWidth(float width)
		{
			this.width = width;
		}

		protected override void SetHeight(float height)
		{
			this.height = height;
		}

		protected override float GetHeight()
		{
			return this.height;
		}

		protected override float GetWidth()
		{
			return this.width;
		}

		public override void Update()
		{
			base.Update();
			if (this.dragging)
			{
				base.Position = new Vector2((float)UIView.MouseX, (float)UIView.MouseY) - this.dragAnchor;
				if (this._constrainInsideParent)
				{
					if (base.Position.X - base.Origin.X < 0f)
					{
						base.X = base.Origin.X;
					}
					else if (base.Position.X + base.Width - base.Origin.X > base.Parent.Width)
					{
						base.X = base.Parent.Width - base.Width + base.Origin.X;
					}
					if (base.Y - base.Origin.Y < 0f)
					{
						base.Y = base.Origin.Y;
						return;
					}
					if (base.Y + base.Height - base.Origin.Y > base.Parent.Height)
					{
						base.Y = base.Parent.Height - base.Height + base.Origin.Y;
					}
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (base.Visible)
			{
				Utils.DrawInvBG(spriteBatch, base.DrawPosition.X - base.Origin.X, base.DrawPosition.Y - base.Origin.Y, base.Width, base.Height, base.BackgroundColor);
			}
			base.Draw(spriteBatch);
		}
	}
}
