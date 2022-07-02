using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace RecipeBrowser
{
	public class UIGrid : UIElement
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
		protected UIScrollbar _scrollbar;
		internal UIElement _innerList = new UIGrid.UIInnerList();
		private float _innerListHeight;
		public float ListPadding = 5f;

		public static Asset<Texture2D> moreUpTexture;
		public static Asset<Texture2D> moreDownTexture;

		public int Count
		{
			get
			{
				return this._items.Count;
			}
		}

		// todo, vertical/horizontal orientation, left to right, etc?
		public UIGrid()
		{
			this._innerList.OverflowHidden = false;
			this._innerList.Width.Set(0f, 1f);
			this._innerList.Height.Set(0f, 1f);
			this.OverflowHidden = true;
			base.Append(this._innerList);
		}

		public float GetTotalHeight()
		{
			return this._innerListHeight;
		}

		public void Goto(UIGrid.ElementSearchMethod searchMethod, bool center = false, bool fuzzy = false)
		{
			var innerDimensionHeight = GetInnerDimensions().Height;
			for (int i = 0; i < this._items.Count; i++)
			{
				var item = this._items[i];
				if (searchMethod(item))
				{
					if (fuzzy)
					{
						if (item.Top.Pixels > _scrollbar.ViewPosition && item.Top.Pixels + item.GetOuterDimensions().Height < _scrollbar.ViewPosition + innerDimensionHeight)
							return;
					}
					this._scrollbar.ViewPosition = item.Top.Pixels;
					if (center)
					{
						this._scrollbar.ViewPosition = item.Top.Pixels - innerDimensionHeight / 2 + item.GetOuterDimensions().Height / 2;
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
			float availableWidth = GetInnerDimensions().Width;
			base.RecalculateChildren();
			float top = 0f;
			float left = 0f;
			float maxRowHeight = 0f;
			for (int i = 0; i < this._items.Count; i++)
			{
				var item = this._items[i];
				var outerDimensions = item.GetOuterDimensions();
				if (left + outerDimensions.Width > availableWidth && left > 0)
				{
					top += maxRowHeight + this.ListPadding;
					left = 0;
					maxRowHeight = 0;
				}
				maxRowHeight = Math.Max(maxRowHeight, outerDimensions.Height);
				item.Left.Set(left, 0f);
				left += outerDimensions.Width + this.ListPadding;
				item.Top.Set(top, 0f);
				item.Recalculate();
			}
			this._innerListHeight = top + maxRowHeight;
		}

		private void UpdateScrollbar()
		{
			if (this._scrollbar == null)
			{
				return;
			}
			this._scrollbar.SetView(base.GetInnerDimensions().Height, this._innerListHeight);
		}

		public void SetScrollbar(UIScrollbar scrollbar)
		{
			this._scrollbar = scrollbar;
			this.UpdateScrollbar();
		}

		//internal delegate int ElementSort(UIElement item1, UIElement item2);
		internal Comparison<UIElement> alternateSort;
		public void UpdateOrder()
		{
			if (alternateSort != null)
				this._items.Sort(alternateSort);
			else
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
			if (this._scrollbar != null)
			{
				this._innerList.Top.Set(-this._scrollbar.GetValue(), 0f);
			}
			if (IsMouseHovering)
				Terraria.GameInput.PlayerInput.LockVanillaMouseScroll("RecipeBrowser/UIHorizontalGrid");
			this.Recalculate();
		}

		public bool drawArrows;
		protected override void DrawChildren(SpriteBatch spriteBatch) {
			base.DrawChildren(spriteBatch);
			if (drawArrows) {
				var inner = GetInnerDimensions().ToRectangle();
				if (this._scrollbar.ViewPosition != 0) {
					int centeredX = inner.X + inner.Width / 2 - moreUpTexture.Width() / 2;
					spriteBatch.Draw(moreUpTexture.Value, new Vector2(centeredX, inner.Y), Color.White * .5f);
				}
				if (this._scrollbar.ViewPosition < _innerListHeight - inner.Height) {
					int centeredX = inner.X + inner.Width / 2 - moreUpTexture.Width() / 2;
					spriteBatch.Draw(moreDownTexture.Value, new Vector2(centeredX, inner.Bottom - moreDownTexture.Height()), Color.White * .5f);
				}
			}
		}
	}
}