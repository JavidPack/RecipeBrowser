using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace RecipeBrowser.UIElements;

internal sealed class ModFilterDropdown : UIPanel
{
	private const float PanelWidth = 300f;
	private const float Pad = 6f;
	private const float ListPad = 4f;
	private const float TopCoverBar = 2f;

	private readonly string[] _mods;
	private readonly Func<int, string> _getDisplayName;
	private readonly List<ModFilterDropdownRow> _rows = [];

	internal ModFilterDropdown(string[] mods, int selectedIndex, Func<int, string> getDisplayName) {
		_mods = mods ?? [];
		_getDisplayName = getDisplayName ?? (_ => string.Empty);

		Width.Set(PanelWidth, 0f);
		Height.Set(-50f, 1f);
		Top.Set(20f, 0f);
		Left.Set(-PanelWidth, 1f);
		SetPadding(Pad);
		BackgroundColor = Color.DarkRed;

		BuildContent(selectedIndex);
	}

	internal event EventHandler<int> SelectedIndexChanged;

	internal void SelectIndex(int index) {
		if (_rows.Count == 0) {
			return;
		}

		int clamped = Math.Clamp(index, 0, _rows.Count - 1);
		OnRowSelected(clamped);
	}

	private void BuildContent(int selectedIndex) {
		var inner = new UIPanel {
			Width = { Pixels = 0, Percent = 1f },
			Height = { Pixels = 0f, Percent = 1f },
			Top = { Pixels = 0f, Percent = 0f },
			BackgroundColor = new Color(200, 50, 50, 255),
		};
		inner.SetPadding(Pad);
		Append(inner);

		var list = new UIList {
			Width = { Pixels = 0f, Percent = 1f },
			Height = { Pixels = 0f, Percent = 1f },
			ListPadding = ListPad,
		};
		inner.Append(list);

		var scrollbar = new InvisibleFixedUIScrollbar(RecipeBrowserUI.instance.userInterface) {
			Height = { Pixels = -12f, Percent = 1f },
			Top = { Pixels = Pad, Percent = 0f },
			HAlign = 1f,
		};
		list.SetScrollbar(scrollbar);
		Append(scrollbar);

		for (int i = 0; i < _mods.Length; i++) {
			string text = _getDisplayName(i);
			var row = new ModFilterDropdownRow(i, text, selectedIndex == i, OnRowSelected);
			_rows.Add(row);
			list.Add(row);
		}

		if (_rows.Count > 1) {
			_rows[^1].MarginBottom = -list.ListPadding;
		}

		var topCover = new UIImage(TextureAssets.MagicPixel) {
			IgnoresMouseInteraction = true,
			Color = BackgroundColor,
			ScaleToFit = true,
		};
		topCover.Top.Set(-(TopCoverBar + Pad - 2f), 0f);
		topCover.Left.Set(-69f, 1f);
		topCover.Width.Set(63f, 0f);
		topCover.Height.Set(TopCoverBar, 0f);
		Append(topCover);

		OnRowSelected(selectedIndex);
	}

	private void OnRowSelected(int index) {
		for (int i = 0; i < _rows.Count; i++) {
			_rows[i].SetSelected(i == index);
		}

		SelectedIndexChanged?.Invoke(this, index);
	}

	private sealed class ModFilterDropdownRow : UIPanel
	{
		private const float TextScale = 0.85f;

		private readonly string _fullText;
		private readonly UIText _label;

		private bool _selected;
		private bool _hasComputedTruncation;
		private bool _isTruncated;

		private int Index { get; }

		internal ModFilterDropdownRow(int index, string displayText, bool selected, Action<int> onSelect) {
			_fullText = displayText;
			_selected = selected;
			Index = index;

			Width.Set(0f, 1f);
			Height.Set(30f, 0f);

			_label = new UIText(displayText, TextScale) { VAlign = 0.5f };
			Append(_label);

			OnLeftClick += (_, _) => onSelect?.Invoke(Index);
			OnMouseOver += (_, _) => {
				if (!_selected) {
					BackgroundColor = Color.DarkRed * 0.3f;
					BorderColor = Color.DarkRed * 0.3f;
				}
			};
			OnMouseOut += (_, _) => Refresh();

			Refresh();
		}

		internal void SetSelected(bool selected) {
			_selected = selected;
			Refresh();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (!_hasComputedTruncation) {
				ComputeTruncationOnce();
				_hasComputedTruncation = true;
			}

			if (_isTruncated && IsMouseHovering) {
				UICommon.TooltipMouseText(_fullText);
			}
			if (IsMouseHovering) {
				RecipeBrowserUI.modHoverIndex = Index;
				RecipeBrowserUI.instance.UpdateModHoverImage();
			}
		}

		private void Refresh() {
			BackgroundColor = _selected ? Color.DarkRed : Color.Transparent;
			BorderColor = _selected ? Color.DarkRed : Color.Transparent;
		}

		private void ComputeTruncationOnce() {
			float availablePx = GetInnerDimensions().Width;
			if (availablePx <= 0f) {
				_label.SetText(string.Empty);
				_isTruncated = true;
				return;
			}

			DynamicSpriteFont font = FontAssets.MouseText.Value;
			float maxUnits = availablePx / TextScale;

			float fullUnits = font.MeasureString(_fullText).X;
			if (fullUnits <= maxUnits) {
				_label.SetText(_fullText);
				_isTruncated = false;
				return;
			}

			const string ellipsis = "...";
			float ellipsisUnits = font.MeasureString(ellipsis).X;
			if (ellipsisUnits > maxUnits) {
				_label.SetText(string.Empty);
				_isTruncated = true;
				return;
			}

			int minLength = 0, maxLength = _fullText.Length;
			while (minLength < maxLength) {
				int candidateLength = (minLength + maxLength + 1) >> 1;
				float units = font.MeasureString(_fullText[..candidateLength]).X + ellipsisUnits;
				if (units <= maxUnits) {
					minLength = candidateLength;
				}
				else {
					maxLength = candidateLength - 1;
				}
			}

			_label.SetText(_fullText[..minLength] + ellipsis);
			_isTruncated = true;
		}
	}
}
