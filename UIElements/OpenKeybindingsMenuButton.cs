#nullable enable
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace RecipeBrowser.UIElements;

/// <summary>
/// A configuration element that shows a button opening the keybindings UI and scrolls
/// directly to this mod's controls category within the controls menu.
/// Calculates and applies the scroll offset cumulatively on each click, based on the current UI state.
/// </summary>
public class OpenKeybindingsMenuButton : ConfigElement<bool>
{
	public override void OnBind()
	{
		base.OnBind();

		var keybindTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Inputs");
		var keybindUIImage = new UIImageFramed(keybindTexture, keybindTexture.Frame(1, 2, sizeOffsetY: -2)) {
			VAlign = 0f,
			HAlign = 1f,
			Left =new StyleDimension(-40f, 0f),
			Top = new StyleDimension(4f, 0f),
			IgnoresMouseInteraction = true
		};
		Append(keybindUIImage);

		var gotoUIImage = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Forward")) {
			Left = new StyleDimension(-6, 0f),
			Top = new StyleDimension(6, 0f),
			HAlign = 1f,
			IgnoresMouseInteraction = true
		};
		Append(gotoUIImage);

		Height.Set(40f, 0f);
	}

	public override void LeftClick(UIMouseEvent evt) {
		base.LeftClick(evt);
		HandleButtonClick();
	}

	/// <summary>
	/// Handles button click: opens the keybinds UI and scrolls to the correct subcategory for this mod.
	/// </summary>
	private static void HandleButtonClick()
	{
		UIManageControls? controlsUi;

		if (Main.gameMenu)
		{
			Main.MenuUI.SetState(Main.ManageControlsMenu);
			controlsUi = Main.MenuUI.CurrentState as UIManageControls;
		}
		else
		{
			IngameFancyUI.OpenKeybinds();
			controlsUi = Main.InGameUI.CurrentState as UIManageControls;
		}

		if (controlsUi is not null)
		{
			ScrollToSubcategory(controlsUi, RecipeBrowser.instance.Name);
		}
		SoundEngine.PlaySound(SoundID.MenuOpen);
	}

	/// <summary>
	/// Scrolls the controls UI to the header that matches <paramref name="modName"/>.
	/// Calculates offset based on header and view positions each time, without relying on cached values.
	/// </summary>
	private static void ScrollToSubcategory(UIManageControls controls, string modName)
	{
		controls.Recalculate();

		var scrollbar = UIElementHelpers.FindChildOfType<UIScrollbar>(controls);
		if (scrollbar == null)
		{
			return;
		}

		scrollbar.Recalculate();

		var uiList = UIElementHelpers.FindChildOfType<UIList>(
			controls,
			list =>
			{
				var field =
					typeof(UIList).GetField("_scrollbar", BindingFlags.Instance | BindingFlags.NonPublic)
					?? typeof(UIList).GetField("_scrollBar", BindingFlags.Instance | BindingFlags.NonPublic);
				return field?.GetValue(list) == scrollbar;
			}
		);

		if (uiList == null)
		{
			return;
		}

		uiList.Recalculate();

		var headerType = typeof(UIManageControls).Assembly.GetType("Terraria.ModLoader.Config.UI.HeaderElement");
		var headerField = headerType?.GetField("header", BindingFlags.Instance | BindingFlags.NonPublic);
		if (headerType == null || headerField == null)
		{
			return;
		}

		var headers = new List<UIElement>();
		UIElementHelpers.GatherElementsByType(uiList, headerType, headers);

		var target = headers.Find(h =>
		{
			var text = headerField.GetValue(h) as string ?? "";
			return string.Equals(text.Replace(" ", ""), modName, StringComparison.InvariantCultureIgnoreCase);
		});
		if (target == null)
		{
			return;
		}

		target.Recalculate();

		float headerY = target.GetDimensions().Y;
		float viewY = uiList.GetInnerDimensions().Y;
		float previousOffset = scrollbar.ViewPosition;
		float rawOffset = Math.Max(0f, headerY - viewY);

		var maxSizeProp = typeof(UIScrollbar).GetProperty("MaxViewSize", BindingFlags.Instance | BindingFlags.Public);
		float maxView = (float?)(maxSizeProp?.GetValue(scrollbar)) ?? rawOffset;
		float offset = Math.Clamp(previousOffset + rawOffset, 0f, maxView);

		scrollbar.ViewPosition = offset;
	}
}
