using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System;
using Terraria.GameContent.UI.Chat;
using Terraria;
using Terraria.Graphics;

namespace RecipeBrowser
{
	internal class UIRadioButton : UIText
	{
		private Texture2D _toggleTexture;

		public event EventHandler OnSelectedChanged;

		private bool selected = false;
		private bool disabled = false;
		internal bool partOfGroup;
		internal int groupID;
		internal string hoverText;

		public bool Selected
		{
			get { return selected; }
			set
			{
				if (value != selected)
				{
					selected = value;
					OnSelectedChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (disabled) return;
			if (!partOfGroup)
			{
				Selected = !Selected;
				Recalculate();
			}
			else
			{
				(Parent as UIRadioButtonGroup).ButtonClicked(groupID);
			}
		}

		public UIRadioButton(string text, string hoverText, float textScale = 1, bool large = false) : base(text, textScale, large)
		{
			this._toggleTexture = TextureManager.Load("Images/UI/Settings_Toggle");
			text = "   " + text;
			this.hoverText = hoverText;
			SetText(text);
			Recalculate();
		}

		public void SetDisabled(bool disabled = true)
		{
			this.disabled = disabled;
			if (disabled)
			{
				Selected = false;
			}
			TextColor = disabled ? Color.Gray : Color.White;
		}

		public void SetHoverText(string hoverText)
		{
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 pos = new Vector2(innerDimensions.X, innerDimensions.Y);

			//spriteBatch.Draw(checkboxTexture, pos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			Rectangle value = new Rectangle(Selected ? ((_toggleTexture.Width - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width - 2) / 2, this._toggleTexture.Height);
			//Vector2 vector2 = new Vector2((float)value.Width, 0f);
			//position = new Vector2(dimensions.X + dimensions.Width - vector2.X - 10f, dimensions.Y + 2f + num);
			spriteBatch.Draw(this._toggleTexture, pos, new Rectangle?(value), disabled ? Color.Gray : Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

			if (IsMouseHovering)
			{
				Main.hoverItemName = hoverText;
			}
		}
	}
}