using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.Menus;
using System;
using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser
{
	class RecipeBrowser : Mod
	{
		public RecipeBrowser()
		{
			Properties = new ModProperties()
			{
				Autoload = false,
			};
		}

		bool CheatSheetLoaded = false;

		public override void Load()
		{
			Mod cheatSheet = ModLoader.GetMod("CheatSheet");
			if (cheatSheet == null)
			{
				RegisterHotKey("Toggle Recipe Browser", "OemCloseBrackets");
				CheatSheetLoaded = false;
			}
			else
			{
				CheatSheetLoaded = true;
				//Don't load hotkey
			}
		}

		internal RecipeBrowserWindow recipeBrowser;
		private double pressedToggleRecipeBrowserHotKeyTime;

		public override void AddRecipeGroups()
		{
			if (!Main.dedServ && !CheatSheetLoaded)
			{
				try
				{
					recipeBrowser = new RecipeBrowserWindow(this);
					recipeBrowser.SetDefaultPosition(new Vector2(80, 250));
					recipeBrowser.Visible = false;
					recipeBrowser.hidden = true;
				}
				catch (Exception e)
				{
					ErrorLogger.Log(e.ToString());
				}
			}
		}

		public override void HotKeyPressed(string name)
		{
			if (name == "Toggle Recipe Browser")
			{
				if (Math.Abs(Main.time - pressedToggleRecipeBrowserHotKeyTime) > 20)
				{
					//Main.NewText("rbh" + recipeBrowser.hidden);
					if (recipeBrowser.hidden)
					{
						recipeBrowser.Show();
					}
					else
					{
						recipeBrowser.Hide();
					}
					pressedToggleRecipeBrowserHotKeyTime = Main.time;
				}
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (!CheatSheetLoaded)
			{
				recipeBrowser.Draw(spriteBatch);
				recipeBrowser.Update();
			}
		}
	}
}
