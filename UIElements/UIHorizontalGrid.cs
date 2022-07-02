using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using System;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria;
using Terraria.UI;
using Terraria.GameInput;

namespace RecipeBrowser
{
	public class UIHorizontalGrid : UIElement
	{
		public delegate bool ElementSearchMethod(UIElement element);

		private class UIInnerList : UIElement
		{
			public override bool ContainsPoint(Vector2 point)
			{
				return true;
			}

			protected override void DrawChildren(SpriteBatch spriteBatch)
			{
				Vector2 position = this.Parent.GetDimensions().Position();
				Vector2 dimensions = new Vector2(this.Parent.GetDimensions().Width, this.Parent.GetDimensions().Height);
				foreach (UIElement current in this.Elements)
				{
					Vector2 position2 = current.GetDimensions().Position();
					Vector2 dimensions2 = new Vector2(current.GetDimensions().Width, current.GetDimensions().Height);
					if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
					{
						current.Draw(spriteBatch);
					}
				}
			}
		}

		public List<UIElement> _items = new List<UIElement>();
		protected UIHorizontalScrollbar _scrollbar;
		internal UIElement _innerList = new UIHorizontalGrid.UIInnerList();
		private float _innerListWidth;
		public float ListPadding = 5f;

		public static Asset<Texture2D> moreLeftTexture;
		public static Asset<Texture2D> moreRightTexture;

		public int Count
		{
			get
			{
				return this._items.Count;
			}
		}

		// todo, vertical/horizontal orientation, left to right, etc?
		public UIHorizontalGrid()
		{
			this._innerList.OverflowHidden = false;
			this._innerList.Width.Set(0f, 1f);
			this._innerList.Height.Set(0f, 1f);
			this.OverflowHidden = true;
			base.Append(this._innerList);
		}

		public float GetTotalWidth()
		{
			return this._innerListWidth;
		}

		public void Goto(UIHorizontalGrid.ElementSearchMethod searchMethod, bool center = false)
		{
			for (int i = 0; i < this._items.Count; i++)
			{
				if (searchMethod(this._items[i]))
				{
					this._scrollbar.ViewPosition = this._items[i].Left.Pixels;
					if (center)
					{
						this._scrollbar.ViewPosition = this._items[i].Left.Pixels - GetInnerDimensions().Width / 2 + _items[i].GetOuterDimensions().Width / 2;
					}
					return;
				}
			}
		}

		public virtual void Add(UIElement item)
		{
			this._items.Add(item);
			this._innerList.Append(item);
			this.UpdateOrder();
			this._innerList.Recalculate();
		}

		public virtual void AddRange(IEnumerable<UIElement> items)
		{
			this._items.AddRange(items);
			foreach (var item in items)
				this._innerList.Append(item);
			this.UpdateOrder();
			this._innerList.Recalculate();
		}

		public virtual bool Remove(UIElement item)
		{
			this._innerList.RemoveChild(item);
			this.UpdateOrder();
			return this._items.Remove(item);
		}

		public virtual void Clear()
		{
			this._innerList.RemoveAllChildren();
			this._items.Clear();
		}

		public override void Recalculate()
		{
			base.Recalculate();
			this.UpdateScrollbar();
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);
			if (this._scrollbar != null)
			{
				this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			}
		}

		public override void RecalculateChildren()
		{
			float availableHeight = GetInnerDimensions().Height;
			base.RecalculateChildren();
			float left = 0f;
			float top = 0f;
			float maxRowWidth = 0f;
			for (int i = 0; i < this._items.Count; i++)
			{
				var item = this._items[i];
				var outerDimensions = item.GetOuterDimensions();
				if (top + outerDimensions.Height > availableHeight && top > 0)
				{
					left += maxRowWidth + this.ListPadding;
					top = 0;
					maxRowWidth = 0;
				}
				maxRowWidth = Math.Max(maxRowWidth, outerDimensions.Width);
				item.Top.Set(top, 0f);
				top += outerDimensions.Height + this.ListPadding;
				item.Left.Set(left, 0f);
				item.Recalculate();
			}
			this._innerListWidth = left + maxRowWidth;
		}

		private void UpdateScrollbar()
		{
			if (this._scrollbar == null)
			{
				return;
			}
			this._scrollbar.SetView(base.GetInnerDimensions().Width, this._innerListWidth);
		}

		public void SetScrollbar(UIHorizontalScrollbar scrollbar)
		{
			this._scrollbar = scrollbar;
			this.UpdateScrollbar();
		}

		public void UpdateOrder()
		{
			this._items.Sort(new Comparison<UIElement>(this.SortMethod));
			this.UpdateScrollbar();
		}

		public int SortMethod(UIElement item1, UIElement item2)
		{
			return item1.CompareTo(item2);
		}

		public override List<SnapPoint> GetSnapPoints()
		{
			List<SnapPoint> list = new List<SnapPoint>();
			SnapPoint item;
			if (base.GetSnapPoint(out item))
			{
				list.Add(item);
			}
			foreach (UIElement current in this._items)
			{
				list.AddRange(current.GetSnapPoints());
			}
			return list;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//var r = GetDimensions().ToRectangle();
			//r.Inflate(-10,-10);
			//spriteBatch.Draw(Main.magicPixel, r, Color.Yellow);
			if (this._scrollbar != null)
			{
				this._innerList.Left.Set(-this._scrollbar.GetValue(), 0f);
			}
			if(IsMouseHovering)
				PlayerInput.LockVanillaMouseScroll("RecipeBrowser/UIHorizontalGrid");
			this.Recalculate();
		}

		public bool drawArrows;
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);
			if (drawArrows)
			{
				var inner = GetInnerDimensions().ToRectangle();
				if (this._scrollbar.ViewPosition != 0)
				{
					int centeredY = inner.Y + inner.Height / 2 - moreLeftTexture.Height() / 2;
					spriteBatch.Draw(moreLeftTexture.Value, new Vector2(inner.X, centeredY), Color.White * .5f);
				}
				if (this._scrollbar.ViewPosition < _innerListWidth - inner.Width - 1) // -1 due to odd width leading to 0.5 view position offset. 
				{
					int centeredY = inner.Y + inner.Height / 2 - moreRightTexture.Height() / 2;
					spriteBatch.Draw(moreRightTexture.Value, new Vector2(inner.Right - moreRightTexture.Width(), centeredY), Color.White * .5f);
				}
			}
		}
	}
}