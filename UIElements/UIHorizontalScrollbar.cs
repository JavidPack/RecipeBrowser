using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.UI;
using Terraria.ID;

namespace RecipeBrowser.UIElements
{
	public class UIHorizontalScrollbar : UIElement
	{
		private float _viewPosition;
		private float _viewSize = 1f;
		private float _maxViewSize = 20f;
		private bool _isDragging;
		private bool _isHoveringOverHandle;
		private float _dragXOffset;
		private Asset<Texture2D> _texture;
		private Asset<Texture2D> _innerTexture;

		public float ViewPosition
		{
			get
			{
				return this._viewPosition;
			}
			set
			{
				this._viewPosition = MathHelper.Clamp(value, 0f, this._maxViewSize - this._viewSize);
			}
		}

		public UIHorizontalScrollbar()
		{
			this.Height.Set(20f, 0f);
			this.MaxHeight.Set(20f, 0f);
			this._texture = RecipeBrowser.instance.Assets.Request<Texture2D>("UIElements/ScrollbarHorizontal"); //TextureManager.Load("Terraria/Images/UI/Scrollbar");
			this._innerTexture = RecipeBrowser.instance.Assets.Request<Texture2D>("UIElements/ScrollbarInnerHorizontal"); //TextureManager.Load("Terraria/Images/UI/ScrollbarInner");
			this.PaddingLeft = 5f;
			this.PaddingRight = 5f;
		}

		public void SetView(float viewSize, float maxViewSize)
		{
			viewSize = MathHelper.Clamp(viewSize, 0f, maxViewSize);
			this._viewPosition = MathHelper.Clamp(this._viewPosition, 0f, maxViewSize - viewSize);
			this._viewSize = viewSize;
			this._maxViewSize = maxViewSize;
		}

		public float GetValue()
		{
			return this._viewPosition;
		}

		private Rectangle GetHandleRectangle()
		{
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			if (this._maxViewSize == 0f && this._viewSize == 0f)
			{
				this._viewSize = 1f;
				this._maxViewSize = 1f;
			}
			//return new Rectangle((int)innerDimensions.X, (int)(innerDimensions.Y + innerDimensions.Height * (this._viewPosition / this._maxViewSize)) - 3, 20, (int)(innerDimensions.Height * (this._viewSize / this._maxViewSize)) + 7);
			return new Rectangle((int)(innerDimensions.X + innerDimensions.Width * (this._viewPosition / this._maxViewSize)) - 3, (int)innerDimensions.Y, (int)(innerDimensions.Width * (this._viewSize / this._maxViewSize)) + 7, 20);
		}

		private void DrawBar(SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color)
		{
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y - 6, dimensions.Width, 6), new Rectangle?(new Rectangle(0, 0, texture.Width, 6)), color);
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle?(new Rectangle(0, 6, texture.Width, 4)), color);
			//spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y + dimensions.Height, dimensions.Width, 6), new Rectangle?(new Rectangle(0, texture.Height - 6, texture.Width, 6)), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X - 6, dimensions.Y, 6, dimensions.Height), new Rectangle(0, 0, 6, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle(6, 0, 4, texture.Height), color);
			spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width, dimensions.Y, 6, dimensions.Height), new Rectangle(texture.Width - 6, 0, 6, texture.Height), color);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetDimensions();
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			if (this._isDragging)
			{
				float num = UserInterface.ActiveInstance.MousePosition.X - innerDimensions.X - this._dragXOffset;
				this._viewPosition = MathHelper.Clamp(num / innerDimensions.Width * this._maxViewSize, 0f, this._maxViewSize - this._viewSize);
			}
			Rectangle handleRectangle = this.GetHandleRectangle();
			Vector2 mousePosition = UserInterface.ActiveInstance.MousePosition;
			bool isHoveringOverHandle = this._isHoveringOverHandle;
			this._isHoveringOverHandle = handleRectangle.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y));
			if (!isHoveringOverHandle && this._isHoveringOverHandle && Main.hasFocus)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
			this.DrawBar(spriteBatch, this._texture.Value, dimensions.ToRectangle(), Color.White);
			this.DrawBar(spriteBatch, this._innerTexture.Value, handleRectangle, Color.White * ((this._isDragging || this._isHoveringOverHandle) ? 1f : 0.85f));
		}

		public override void LeftMouseDown(UIMouseEvent evt)
		{
			base.LeftMouseDown(evt);
			if (evt.Target == this)
			{
				Rectangle handleRectangle = this.GetHandleRectangle();
				if (handleRectangle.Contains(new Point((int)evt.MousePosition.X, (int)evt.MousePosition.Y)))
				{
					this._isDragging = true;
					this._dragXOffset = evt.MousePosition.X - (float)handleRectangle.X;
					return;
				}
				CalculatedStyle innerDimensions = base.GetInnerDimensions();
				float num = UserInterface.ActiveInstance.MousePosition.X - innerDimensions.X - (float)(handleRectangle.Width >> 1);
				this._viewPosition = MathHelper.Clamp(num / innerDimensions.Width * this._maxViewSize, 0f, this._maxViewSize - this._viewSize);
			}
		}

		public override void LeftMouseUp(UIMouseEvent evt)
		{
			base.LeftMouseUp(evt);
			this._isDragging = false;
		}
	}

	public class FixedUIHorizontalScrollbar : UIHorizontalScrollbar
	{
		internal UserInterface userInterface;

		public FixedUIHorizontalScrollbar(UserInterface userInterface)
		{
			this.userInterface = userInterface;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void LeftMouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.LeftMouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}

	public class InvisibleFixedUIHorizontalScrollbar : FixedUIHorizontalScrollbar
	{
		public InvisibleFixedUIHorizontalScrollbar(UserInterface userInterface) : base(userInterface)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			//base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void LeftMouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.LeftMouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
}