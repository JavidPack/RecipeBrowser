//using RecipeBrowser.Menus;
//using RecipeBrowser.UI;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;

//namespace RecipeBrowser.CustomUI
//{
//	class UISlideWindow : UIWindow
//	{
//		internal static float moveSpeed = 10f;
//		internal float lerpAmount;

//		internal Vector2 shownPosition = new Vector2(10, 100);
//		internal Vector2 hiddenPosition = new Vector2(300, 300);
//		internal Vector2 defaultPosition = new Vector2(200, 200);

//		internal bool hidden;
//		internal bool arrived;

//		private bool _selected;
//		internal bool selected
//		{
//			get { return _selected; }
//			set
//			{
//				if (value == false)
//				{
//					if (_selected && arrived && !hidden)
//						setShowHidePositions();
//					hidden = true;
//				}
//				else
//				{
//					hidden = false;
//					Visible = true;
//				}
//				arrived = false;
//				_selected = value;
//			}
//		}

//		internal void setShowHidePositions()
//		{
//			shownPosition = Position;
//			hiddenPosition = Position;

//			if (Position.X + Width/2 > Main.screenWidth / 2)
//			{
//				hiddenPosition.X = Main.screenWidth;
//			}
//			else
//			{
//				hiddenPosition.X = - Width;
//			}
//		}

//		public UISlideWindow()
//		{

//		}

//		protected override bool IsMouseInside()
//		{
//			if (hidden) return false;
//			return base.IsMouseInside();
//		}

//		internal void Hide()
//		{
//			hidden = true;
//			arrived = false;
//			setShowHidePositions();
//		}

//		internal void SetDefaultPosition(Vector2 vector2)
//		{
//			Position = vector2;
//			defaultPosition = Position;
//			setShowHidePositions();
//		}

//		internal void Show()
//		{
//			hidden = false;
//			arrived = false;
//			Visible = true;

//			if(shownPosition.X > Main.screenWidth -25 || shownPosition.Y > Main.screenHeight - 25)
//			{
//				shownPosition = defaultPosition;
//			}
//			if (shownPosition.X < -Width+25 || shownPosition.Y < -Height + 25)
//			{
//				shownPosition = defaultPosition;
//			}
//		}

//		public override void Update()
//		{
//			if (!arrived)
//			{
//				if (this.hidden)
//				{
//					this.lerpAmount -= .01f * moveSpeed;
//					if (this.lerpAmount < 0f)
//					{
//						this.lerpAmount = 0f;
//						arrived = true;
//						this.Visible = false;
//					}
//					base.Position = Vector2.SmoothStep(hiddenPosition, shownPosition, lerpAmount);
//				}
//				else
//				{
//					this.lerpAmount += .01f * moveSpeed;
//					if (this.lerpAmount > 1f)
//					{
//						this.lerpAmount = 1f;
//						arrived = true;
//					}
//					base.Position = Vector2.SmoothStep(hiddenPosition, shownPosition, lerpAmount);
//				}
//			}

//			base.Update();
//		}

//	}
//}
