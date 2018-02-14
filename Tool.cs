using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.UI;

namespace RecipeBrowser
{
	// UIState needs UserInterface for Scrollbar fixes
	// Tool should store data? does it even matter?
	internal abstract class Tool
	{
		//internal bool visible;
		//internal string toggleTooltip;
		internal UserInterface userInterface;

		internal UIModState uistate;
		//	Type uistateType;

		public Tool(Type uistateType)
		{
			userInterface = new UserInterface();
			//	this.uistateType = uistateType;
			uistate = (UIModState)Activator.CreateInstance(uistateType, new object[] { userInterface });
			//uistate = (UIModState)Activator.CreateInstance(uistateType);

			//uistate = new LootUI(userInterface);
			uistate.Activate();
			//uistate.userInterface = userInterface;
			userInterface.SetState(uistate);
		}

		/// <summary>
		/// Initializes this Tool. Called during Load.
		/// Useful for initializing data.
		/// </summary>
		internal virtual void Initialize()
		{
		}

		/// <summary>
		/// Initializes this Tool. Called during Load after Initialize only on SP and Clients.
		/// Useful for initializing UI.
		/// </summary>
		internal virtual void ClientInitialize() { }

		internal virtual void ScreenResolutionChanged()
		{
			userInterface?.Recalculate();
		}

		internal virtual void UIUpdate(GameTime gameTime)
		{
			//if (visible)
			{
				userInterface?.Update(gameTime);
			}
		}

		internal virtual void UIDraw()
		{
			//if (visible)
			{
				uistate.ReverseChildren();
				uistate.Draw(Main.spriteBatch);
				uistate.ReverseChildren();
			}
		}

		internal virtual void DrawUpdateToggle()
		{
		}

		internal virtual void Toggled()
		{
		}

		internal virtual void PostSetupContent()
		{
			if (!Main.dedServ)
			{
			}
		}
	}
}