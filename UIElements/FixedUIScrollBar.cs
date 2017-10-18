using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RecipeBrowser
{
	public class FixedUIScrollbar : UIScrollbar
	{
		internal UserInterface userInterface;

		public FixedUIScrollbar(UserInterface userInterface)
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

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}

	public class InvisibleFixedUIScrollbar : FixedUIScrollbar
	{
		public InvisibleFixedUIScrollbar(UserInterface userInterface) : base(userInterface)
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			//base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = userInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
}