using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RecipeBrowser
{
	class UIDragablePanel : UIPanel
	{
		Vector2 offset;
		bool dragging = false;

		public UIDragablePanel()
		{
			OnMouseDown += DragStart;
			OnMouseUp += DragEnd;
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
			if (evt.Target == this || evt.Target == RecipeBrowserUI.instance.recipeInfo || evt.Target == RecipeBrowserUI.instance.RadioButtonGroup)
			{
				offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
				dragging = true;
			}
		}

		private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target == this || evt.Target == RecipeBrowserUI.instance.recipeInfo || evt.Target == RecipeBrowserUI.instance.RadioButtonGroup)
			{
				Vector2 end = evt.MousePosition;
				dragging = false;

				Left.Set(end.X - offset.X, 0f);
				Top.Set(end.Y - offset.Y, 0f);

				Recalculate();
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
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
			base.DrawSelf(spriteBatch);
		}
	}
}
