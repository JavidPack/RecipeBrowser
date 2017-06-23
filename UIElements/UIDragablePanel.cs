using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;

namespace RecipeBrowser
{
	class UIDragablePanel : UIPanel
	{
		private static Texture2D dragTexture;
		private Vector2 offset;
		private bool dragging;
		private bool resizeing;
		private bool resizeable;

		public UIDragablePanel(bool resizeable = false)
		{
			OnMouseDown += DragStart;
			OnMouseUp += DragEnd;
			this.resizeable = resizeable;
			if (dragTexture == null)
			{
				dragTexture = TextureManager.Load("Images/UI/PanelBorder");
			}
		}

		//public override void MouseDown(UIMouseEvent evt)
		//{
		//	offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
		//	dragging = true;
		//}

		//public override void MouseUp(UIMouseEvent evt)
		//{
		//	Vector2 end = evt.MousePosition;
		//	dragging = false;

		//	Left.Set(end.X - offset.X, 0f);
		//	Top.Set(end.Y - offset.Y, 0f);

		//	Recalculate();
		//}

		private void DragStart(UIMouseEvent evt, UIElement listeningElement)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (evt.Target == this || evt.Target == RecipeBrowserUI.instance.recipeInfo || evt.Target == RecipeBrowserUI.instance.RadioButtonGroup)
			{
				if (new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + 6, 12 + 6).Contains(evt.MousePosition.ToPoint()))
				{
					offset = new Vector2(evt.MousePosition.X - innerDimensions.X - innerDimensions.Width - 6, evt.MousePosition.Y - innerDimensions.Y - innerDimensions.Height - 6);
					resizeing = true;
				}
				else
				{
					offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
					dragging = true;
				}
			}
		}

		private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target == this || evt.Target == RecipeBrowserUI.instance.recipeInfo || evt.Target == RecipeBrowserUI.instance.RadioButtonGroup)
			{
				//Vector2 end = evt.MousePosition;
				dragging = false;
				resizeing = false;

				//Left.Set(end.X - offset.X, 0f);
				//Top.Set(end.Y - offset.Y, 0f);
				//Recalculate();
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetOuterDimensions();
			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
			}
			if (dragging)
			{
				Left.Set(Main.MouseScreen.X - offset.X, 0f);
				Top.Set(Main.MouseScreen.Y - offset.Y, 0f);
				Recalculate();
			}
			if (resizeing)
			{
				Height.Pixels = Utils.Clamp(Main.MouseScreen.Y - dimensions.Y - offset.Y, 243, 1000);
				//Width.Pixels = Utils.Clamp(Main.MouseScreen.X - dimensions.X - offset.X, 415, 1000);
				Recalculate();
			}
			base.DrawSelf(spriteBatch);
			if (resizeable)
			{
				DrawDragAnchor(spriteBatch, dragTexture, this.BorderColor);
			}
		}

		private void DrawDragAnchor(SpriteBatch spriteBatch, Texture2D texture, Color color)
		{
			CalculatedStyle dimensions = GetDimensions();
			//CalculatedStyle innerDimensions = GetInnerDimensions();

			//Rectangle hitbox = new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + 6, 12 + 6);
			//Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.LightBlue * 0.6f);

			Point point = new Point((int)dimensions.X, (int)dimensions.Y);
			Point point2 = new Point(point.X + (int)dimensions.Width - 12, point.Y + (int)dimensions.Height - 12);
			int width = point2.X - point.X - 12;
			int height = point2.Y - point.Y - 12;
			//spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, 12, 12), new Rectangle?(new Rectangle(12 + 4, 12 + 4, 12, 12)), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X - 2, point2.Y - 2, 12 - 2, 12 - 2), new Rectangle?(new Rectangle(12 + 4, 12 + 4, 12, 12)), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X - 4, point2.Y - 4, 12 - 4, 12 - 4), new Rectangle?(new Rectangle(12 + 4, 12 + 4, 12, 12)), color);
			spriteBatch.Draw(texture, new Rectangle(point2.X - 6, point2.Y - 6, 12 - 6, 12 - 6), new Rectangle?(new Rectangle(12 + 4, 12 + 4, 12, 12)), color);
		}
	}
}
