using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	public class UISystem : ModSystem
	{
		public static UISystem Instance => ModContent.GetInstance<UISystem>();

		private RecipeBrowserTool recipeBrowserTool;

		private int lastSeenScreenWidth;
		private int lastSeenScreenHeight;

		private CancellationTokenSource concurrentTaskHandlerToken;
		private Task concurrentTaskHandler;

		public ConcurrentQueue<Task> concurrentTasks = new();

		public override void OnModLoad()
		{
			if (!Main.dedServ /*&& !CheatSheetLoaded*/)
			{
				recipeBrowserTool = new RecipeBrowserTool();
				UIRecipeSlot.favoritedBackgroundTexture = Mod.Assets.Request<Texture2D>("Images/FavoritedOverlay");
				UIRecipeSlot.selectedBackgroundTexture = Mod.Assets.Request<Texture2D>("Images/SelectedOverlay");
				UIRecipeSlot.ableToCraftBackgroundTexture = Mod.Assets.Request<Texture2D>("Images/CanCraftBackground");
				UIRecipeSlot.ableToCraftExtendedBackgroundTexture = Mod.Assets.Request<Texture2D>("Images/CanCraftExtendedBackground");
				UIMockRecipeSlot.ableToCraftBackgroundTexture = Mod.Assets.Request<Texture2D>("Images/CanCraftBackground");
				UICheckbox.checkboxTexture = Mod.Assets.Request<Texture2D>("UIElements/checkBox");
				UICheckbox.checkmarkTexture = Mod.Assets.Request<Texture2D>("UIElements/checkMark");
				UIHorizontalGrid.moreLeftTexture = Mod.Assets.Request<Texture2D>("UIElements/MoreLeft");
				UIHorizontalGrid.moreRightTexture = Mod.Assets.Request<Texture2D>("UIElements/MoreRight");
				Utilities.tileTextures = new Dictionary<int, Texture2D>();

				concurrentTaskHandlerToken = new CancellationTokenSource();
				concurrentTaskHandler = Task.Run(ConcurrentTaskHandler);
			}
		}

		public override void Unload()
		{
			if (!Main.dedServ) {
				concurrentTaskHandlerToken?.Cancel();
				concurrentTaskHandler?.Wait();
			}
		}
		public async Task ConcurrentTaskHandler() {
			var runningTasks = new List<Task>();
			try {
				while (true) {
					if (runningTasks.Count == 4) {
						// this will 'block' unless the tasks themselves are also bound to the cancellation token
						// you could add an extra 'delay task' and check if the returned task is the delay task
						// but you need some way to make sure all of them finish on unload anyway...
						var task2 = await Task.WhenAny(runningTasks);
						try {
							runningTasks.Remove(task2);
							await task2; // catch exceptions from the completed task (or do something with the result)
						}
						catch (OperationCanceledException oce) when (oce.CancellationToken == concurrentTaskHandlerToken.Token) {
							throw;
						}
						catch {
							// task completed with exception
						}
					}

					// runningTasks must be < 4 now
					if (concurrentTasks.TryDequeue(out var task)) {
						if (task.IsCanceled) {
							//Console.WriteLine();
						}
						task.Start(); //throws if you provide it with a 'hot' task
						runningTasks.Add(task);
					}
					else { 
						// free up the thread for a bit before looking for new tasks in the queue
						await Task.Delay(100, concurrentTaskHandlerToken.Token);
					}
				}
			}
			// Causes log message.
			catch (OperationCanceledException oce) when (oce.CancellationToken == concurrentTaskHandlerToken.Token) {
				//somehow ensure all tasks finish?
				await Task.WhenAll(runningTasks);
			} 
		}

		public override void UpdateUI(GameTime gameTime)
		{
			// By doing these triggers here, we can use them even if autopaused.
			if (Main.netMode == NetmodeID.SinglePlayer && (Main.playerInventory || Main.npcChatText != "" || Main.player[Main.myPlayer].sign >= 0 || Main.ingameOptionsWindow || Main.inFancyUI) && Main.autoPause)
				Main.LocalPlayer.GetModPlayer<RecipeBrowserPlayer>().ProcessTriggers(null);
			recipeBrowserTool?.UIUpdate(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			//if (CheatSheetLoaded) return;

			int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (inventoryLayerIndex != -1)
			{
				layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
					"RecipeBrowser: UI",
					delegate
					{
						//if (!CheatSheetLoaded)
						{
							if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight)
							{
								recipeBrowserTool.ScreenResolutionChanged();
								lastSeenScreenWidth = Main.screenWidth;
								lastSeenScreenHeight = Main.screenHeight;
							}
							recipeBrowserTool.UIDraw();
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void PreSaveAndQuit()
		{
			RecipeBrowserUI.instance.CloseButtonClicked(null, null);
			RecipeBrowserUI.instance.ShowRecipeBrowser = false;
		}
	}
}
