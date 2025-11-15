using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using Terraria;

namespace RecipeBrowser
{
	internal class UIHairDyeItemSlot : UIItemCatalogueItemSlot
	{
		private bool drawError = false;
		private UIItemCatalogueItemSlot headSlot;
		internal bool needsUpdate = false;

		internal static Player drawPlayer;
		internal static bool useDye;
		internal static bool animate;
		internal static bool accessories;
		internal static bool showItems = true;
		public UIHairDyeItemSlot(Item item, float scale = 0.75f) : base(item, scale) {
			this.Width.Set(defaultBackgroundTexture.Width() * scale, 0f);
			this.Height.Set(defaultBackgroundTexture.Height() * 2f * scale, 0f);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Terraria.UI.CalculatedStyle dimensions = base.GetInnerDimensions();
			Rectangle rectangle = dimensions.ToRectangle();

			spriteBatch.Draw(backgroundTexture.Value, dimensions.Position() + new Vector2(0, defaultBackgroundTexture.Height() * scale), null, Color.White, 0f, Vector2.Zero, new Vector2(scale * 0.45f, scale /** 1.5f*/), SpriteEffects.None, 0f);

			base.DrawSelf(spriteBatch);

			var innerDimensions = GetInnerDimensions().ToRectangle();

			Player drawPlayer = (Player)Main.LocalPlayer.Clone();
			int originalHairDye = drawPlayer.hairDye;
			drawPlayer.hairDye = item.hairDye;

			// Somehow this is below backgroundTexture above. 
			Main.PlayerRenderer.DrawPlayerHead(Main.Camera, drawPlayer, innerDimensions.Center.ToVector2() + new Vector2(-4, 16) /* new Vector2(a.Right - 16, a.Y + 8) */ /*+ new Vector2(0, defaultBackgroundTexture.Height() * scale)*/);

			// Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, drawPlayer, innerDimensions.Center.ToVector2() + new Vector2(-4, 26));

			drawPlayer.hairDye = originalHairDye;
		}
	}
}
