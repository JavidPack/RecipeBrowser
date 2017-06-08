using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using System.Collections.Generic;

namespace RecipeBrowser
{
	class UITabControl : UIElement
	{
		internal UIPanel mainPanel;
		private List<UIPanel> panels;
		private List<UIText> texts;

		public UITabControl()
		{
			panels = new List<UIPanel>();
			texts = new List<UIText>();
			mainPanel = new UIPanel();
			Width = StyleDimension.Fill;
			Height = StyleDimension.Fill;
			mainPanel.SetPadding(6);
			mainPanel.Width = StyleDimension.Fill;
			mainPanel.Height = StyleDimension.Fill;
			mainPanel.BackgroundColor = Color.LightCoral;
		}

		public override void OnInitialize()
		{
			Append(mainPanel);

			SetPanel(0);
		}

		public void AddTab(string label, UIPanel panel)
		{
			UIText text = new UIText(label, 0.85f);
			text.Top.Set(0, 0f);
			//text.HAlign = 1f;
			text.OnClick += Text_OnClick;
			mainPanel.Append(text);
			texts.Add(text);

			panel.Top.Pixels = 20;
			panel.Width = StyleDimension.Fill;
			panel.Height = StyleDimension.Fill;
			panel.Height.Pixels = -20;
			panels.Add(panel);

			// 1 -> 0
			// 2 -> 0, 1
			// 3 -> 0, 0.5, 1
			// 4 -> 0, 0.33, 0.66, 1
			if (texts.Count > 1)
			{
				for (int i = 0; i < texts.Count; i++)
				{
					texts[i].HAlign = i / (texts.Count - 1f);
				}
			}
		}

		private void Text_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIText text = (evt.Target as UIText);
			int index = texts.IndexOf(text);
			SetPanel(index);
		}

		public void SetPanel(int panelIndex)
		{
			if (panelIndex >= 0 && panelIndex < panels.Count)
			{
				panels.ForEach(panel => { if (mainPanel.HasChild(panel)) mainPanel.RemoveChild(panel); });
				texts.ForEach(atext => { atext.TextColor = Color.Gray; });

				mainPanel.Append(panels[panelIndex]);
				texts[panelIndex].TextColor = Color.White;
			}
		}
	}
}
