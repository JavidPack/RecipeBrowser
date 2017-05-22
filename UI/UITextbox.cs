using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace RecipeBrowser.UI
{
	internal class UITextbox : UIView
	{
		public delegate void KeyPressedHandler(object sender, char key);

		private RasterizerState _rasterizerState = new RasterizerState
		{
			ScissorTestEnable = true
		};

		private static Texture2D textboxBackground = Terraria.ModLoader.ModLoader.GetTexture("RecipeBrowser/UI/Images.UIKit.textboxEdge");

		private static Texture2D textboxFill;

		private bool focused;

		private static float blinkTime = 1f;

		private static float timer = 0f;

		private bool eventSet;

		private float width = 200f;

		private bool drawCarrot;

		private UILabel label = new UILabel();

		private static int padding = 4;

		private string text = "";

		private int maxCharacters = 20;

		private bool passwordBox;

		public event EventHandler OnTabPress;

		public event EventHandler OnEnterPress;

		public event EventHandler OnLostFocus;

		public event UITextbox.KeyPressedHandler KeyPressed;

		private static Texture2D TextboxFill
		{
			get
			{
				if (UITextbox.textboxFill == null)
				{
					Color[] array = new Color[UITextbox.textboxBackground.Width * UITextbox.textboxBackground.Height];
					UITextbox.textboxBackground.GetData<Color>(array);
					Color[] array2 = new Color[UITextbox.textboxBackground.Height];
					for (int i = 0; i < array2.Length; i++)
					{
						array2[i] = array[UITextbox.textboxBackground.Width - 1 + i * UITextbox.textboxBackground.Width];
					}
					UITextbox.textboxFill = new Texture2D(UIView.graphics, 1, array2.Length);
					UITextbox.textboxFill.SetData<Color>(array2);
				}
				return UITextbox.textboxFill;
			}
		}

		public bool HadFocus
		{
			get
			{
				return this.focused;
			}
		}

		public string Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
			}
		}

		public int MaxCharacters
		{
			get
			{
				return this.maxCharacters;
			}
			set
			{
				this.maxCharacters = value;
			}
		}

		public bool PasswordBox
		{
			get
			{
				return this.passwordBox;
			}
			set
			{
				this.passwordBox = value;
			}
		}

		private string passwordString
		{
			get
			{
				string text = "";
				for (int i = 0; i < this.Text.Length; i++)
				{
					text += "*";
				}
				return text;
			}
		}

		public UITextbox()
		{
			base.onLeftClick += new EventHandler(this.UITextbox_onLeftClick);
			this.label.ForegroundColor = Color.Black;
			this.label.Scale = base.Height / this.label.Height;
			this.label.TextOutline = false;
			this.AddChild(this.label);
		}

		private void UITextbox_onLeftClick(object sender, EventArgs e)
		{
			this.Focus();
		}

		public void Focus()
		{
			if (this.focused)
			{
				return;
			}
			this.focused = true;
			UITextbox.timer = 0f;
			this.eventSet = true;
			Main.blockInput = true;
		//	Main.RemoveKeyEvent();
			keyBoardInput.newKeyEvent += new Action<char>(this.KeyboardInput_newKeyEvent);
		}

		public void Unfocus()
		{
			if (this.focused && this.OnLostFocus != null)
			{
				this.OnLostFocus(this, EventArgs.Empty);
			}
			this.focused = false;
			if (!this.eventSet)
			{
				return;
			}
			this.eventSet = false;
			Main.blockInput = false;
			keyBoardInput.newKeyEvent -= new Action<char>(this.KeyboardInput_newKeyEvent);
		//	Main.AddKeyEvent();
		}

		private void KeyboardInput_newKeyEvent(char obj)
		{
			if (obj.Equals('\b')) // Backspace key
			{
				if (this.Text.Length > 0)
				{
					this.Text = this.Text.Substring(0, this.Text.Length - 1);
					this.SetLabelPosition();
					if (this.KeyPressed != null)
					{
						this.KeyPressed(this, obj);
						return;
					}
				}
			}
			else
			{
				if (obj.Equals('\u001b')) // Escape key
				{
					this.Unfocus();
					return;
				}
				if (obj.Equals('\t'))
				{
					if (this.OnTabPress != null)
					{
						this.OnTabPress(this, new EventArgs());
						return;
					}
				}
				else if (obj.Equals('\r'))
				{
					Main.chatRelease = false;
					if (this.OnEnterPress != null)
					{
						this.OnEnterPress(this, new EventArgs());
						return;
					}
				}
				else if (obj.Equals('\u0001'))
				{
					if (this.Text.Length > 0)
					{
						this.Text = this.Text.Substring(0, 0);
						this.SetLabelPosition();
						if (this.KeyPressed != null)
						{
							this.KeyPressed(this, obj);
							return;
						}
					}
				}
				else if (obj >=0 && obj <= 37)
				{
					return;
				}
				else
				{
					int i = 0;
					while (i < this.label.font.Characters.Count)
					{
						if (this.Text.Length < this.MaxCharacters && obj == Main.fontItemStack.Characters[i])
						{
							this.Text += obj;
							this.SetLabelPosition();
							if (this.KeyPressed != null)
							{
								this.KeyPressed(this, obj);
								return;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
			}
		}

		private void SetLabelPosition()
		{
			this.label.Position = new Vector2((float)UITextbox.padding, 0f);
			Vector2 vector = this.label.font.MeasureString(this.Text + "|") * this.label.Scale;
			if (this.passwordBox)
			{
				vector = this.label.font.MeasureString(this.passwordString + "|") * this.label.Scale;
			}
			if (vector.X > base.Width - (float)(UITextbox.padding * 2))
			{
				this.label.Position = new Vector2((float)UITextbox.padding - (vector.X - (base.Width - (float)(UITextbox.padding * 2))), 0f);
			}
		}

		protected override float GetWidth()
		{
			return this.width;
		}

		protected override void SetWidth(float width)
		{
			this.width = width;
		}

		protected override float GetHeight()
		{
			return (float)UITextbox.textboxBackground.Height;
		}

		public override void Update()
		{
			base.Update();
			if (!this.IsMouseInside() && UIView.MouseLeftButton)
			{
				this.Unfocus();
			}
			if (this.focused)
			{
				UITextbox.timer +=  .1f;//Mod.deltaTime;
				if (UITextbox.timer < UITextbox.blinkTime / 2f)
				{
					this.drawCarrot = true;
				}
				else
				{
					this.drawCarrot = false;
				}
				if (UITextbox.timer >= UITextbox.blinkTime)
				{
					UITextbox.timer = 0f;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(UITextbox.textboxBackground, base.DrawPosition, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.None, 0f);
			int num = (int)base.Width - 2 * UITextbox.textboxBackground.Width;
			Vector2 vector = base.DrawPosition;
			vector.X += (float)UITextbox.textboxBackground.Width;
			spriteBatch.Draw(UITextbox.TextboxFill, vector - base.Origin, null, Color.White, 0f, Vector2.Zero, new Vector2((float)num, 1f), SpriteEffects.None, 0f);
			vector.X += (float)num;
			spriteBatch.Draw(UITextbox.textboxBackground, vector, null, Color.White, 0f, base.Origin, 1f, SpriteEffects.FlipHorizontally, 0f);
			string str = this.Text;
			if (this.PasswordBox)
			{
				str = this.passwordString;
			}
			if (this.drawCarrot && this.focused)
			{
				str += "|";
			}
			this.label.Text = str;
			vector = base.DrawPosition - base.Origin;
			if (vector.X <= (float)Main.screenWidth && vector.Y <= (float)Main.screenHeight && vector.X + base.Width >= 0f && vector.Y + base.Height >= 0f)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, this._rasterizerState);
				Rectangle scissorRectangle = new Rectangle((int)vector.X, (int)vector.Y, (int)base.Width, (int)base.Height);
				if (scissorRectangle.X < 0)
				{
					scissorRectangle.Width += scissorRectangle.X;
					scissorRectangle.X = 0;
				}
				if (scissorRectangle.Y < 0)
				{
					scissorRectangle.Height += scissorRectangle.Y;
					scissorRectangle.Y = 0;
				}
				if ((float)scissorRectangle.X + base.Width > (float)Main.screenWidth)
				{
					scissorRectangle.Width = Main.screenWidth - scissorRectangle.X;
				}
				if ((float)scissorRectangle.Y + base.Height > (float)Main.screenHeight)
				{
					scissorRectangle.Height = Main.screenHeight - scissorRectangle.Y;
				}
				Rectangle scissorRectangle2 = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
				base.Draw(spriteBatch);
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle2;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			}
		}
	}
}
