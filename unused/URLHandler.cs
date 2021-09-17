//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System.Collections.Generic;
//using System.Text;
//using Terraria.UI.Chat;
//using Newtonsoft.Json.Linq;
//using Terraria;
//using ReLogic.Graphics;
//using System.Net;
//using System.IO;
//using System;

//namespace RecipeBrowser.TagHandlers
//{
//	public class URLTagHandler : ITagHandler
//	{
//		private class URLSnippet : TextSnippet
//		{
//			private string imageurl;
//			public string sn;
//			// index = x*41 + y

//			public URLSnippet(string imageurl)
//				: base("")
//			{
//				this.imageurl = imageurl;
//				this.Color = Color.Blue;
//				CheckForHover = true;
//			}

//			int resolution = 20;
//			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default(Vector2), Color color = default(Color), float scale = 1f)
//			{
//				size = new Vector2(resolution) * URLTagHandler.GlyphsScale;
//				if (!justCheckingString && color != Color.Black)
//				{

//					Texture2D texture;
//					if (imageCache.TryGetValue(imageurl, out texture))
//					{
//						if (texture != null)
//						{
//							spriteBatch.Draw(texture, position, null, Color.White, 0f, Vector2.Zero, URLTagHandler.GlyphsScale, SpriteEffects.None, 0f);
//							//size = texture.Size();
//						}
//						else
//						{
//							Texture2D texture2D = EmojiSupport.instance.GetTexture("sheet_google_" + resolution);// Main.textGlyphTexture[0];
//							spriteBatch.Draw(texture2D, position, new Rectangle?(new Rectangle((0 / 41) * resolution, (0 % 41) * resolution, resolution, resolution)), color, 0f, Vector2.Zero, URLTagHandler.GlyphsScale, SpriteEffects.None, 0f);
//						}
//					}
//					else
//					{
//						Texture2D texture2D = EmojiSupport.instance.GetTexture("sheet_google_" + resolution);// Main.textGlyphTexture[0];
//						spriteBatch.Draw(texture2D, position, new Rectangle?(new Rectangle((0 / 41) * resolution, (0 % 41) * resolution, resolution, resolution)), color, 0f, Vector2.Zero, URLTagHandler.GlyphsScale, SpriteEffects.None, 0f);
//					}
//				}
//				return true;
//			}

//			public override float GetStringLength(DynamicSpriteFont font)
//			{
//				return resolution * URLTagHandler.GlyphsScale;
//			}

//			public override void OnClick()
//			{
//				ChatManager.AddChatText(FontAssets.MouseText.Value, "[e:" + sn + "]", Vector2.One);
//			}

//			public override void OnHover()
//			{
//				//Main.toolTip = new Item();
//				Main.hoverItemName = $"Emoji code: {sn}";
//				Main.instance.MouseText(Main.hoverItemName, Main.rare, 0);
//			}

//		}

//		//private const int GlyphsPerLine = 25;
//		//private const int MaxGlyphs = 26;
//		public static float GlyphsScale = 1f;
//		//private static Dictionary<string, int> GlyphIndexes = new Dictionary<string, int>
//		//{
//		//	{ "love", 0 },
//		//	{ "oh", 1 },
//		//	{ "tounge", 2 },
//		//	{ "laugh", 3 },
//		//};
//		internal static Dictionary<string, Texture2D> imageCache;


//		public static void Initialize()
//		{
//			if (imageCache == null)
//			{
//				imageCache = new Dictionary<string, Texture2D>();
//			}
//		}

//		TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
//		{
//			Initialize();

//			if (!imageCache.ContainsKey(text))
//			{
//				imageCache[text] = null;
//				using (WebClient client = new WebClient())
//				{
//					client.DownloadDataCompleted += (s, e) => IconDownloadComplete(s, e, text);
//					client.DownloadDataAsync(new Uri(text));
//				}
//			}


//			//if (!int.TryParse(text, out num) || num >= 1620)
//			//{
//			//	return new TextSnippet(text);
//			//}
//			//if (!URLTagHandler.GlyphIndexes.TryGetValue(text.ToLower(), out num))
//			//{
//			//	return new TextSnippet(text);
//			//}
//			return new URLTagHandler.URLSnippet(text)
//			{
//				DeleteWhole = true,

//				Text = "[e:" + text.ToLower() + "]",
//				sn = text.ToLower()
//			};
//		}


//		private void IconDownloadComplete(object sender, DownloadDataCompletedEventArgs e, string url)
//		{
//			byte[] data = e.Result;
//			using (MemoryStream buffer = new MemoryStream(data))
//			{
//				Texture2D imageTexture = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
//				imageCache[url] = imageTexture;
//			}
//		}
//	}
//}
