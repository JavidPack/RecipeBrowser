using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RecipeBrowser.UIElements;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RecipeBrowser
{
	public class UISystem : ModSystem
	{
		public override void UpdateUI(GameTime gameTime) => RecipeBrowser.instance.UpdateUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) => RecipeBrowser.instance.ModifyInterfaceLayers(layers);

		public override void PreSaveAndQuit() => RecipeBrowser.instance.PreSaveAndQuit();

        public override void AddRecipes() {
			Condition shimmerTransmutationCondition = new("Mods.RecipeBrowser.Conditions.ShimmerTransmutation", () => false);
            for (int i = 0; i < ItemID.Sets.ShimmerTransformToItem.Length; i++) {
                // For some reason this is set initially but gets overriden later on
                // So I'll skip it 
                if (i == ItemID.LihzahrdBrickWall) 
                    continue;
                int shimmerTransmutation = ItemID.Sets.ShimmerTransformToItem[i];
                if (shimmerTransmutation > 0) {
					Recipe.Create(shimmerTransmutation)
						.AddIngredient(i)
						.AddCondition(shimmerTransmutationCondition)
						.Register();
                }
            }

			// Fix for vanilla hardcoded transmutations
			Recipe.Create(ItemID.RodOfHarmony)
				.AddIngredient(ItemID.RodofDiscord)
				.AddCondition(shimmerTransmutationCondition)
				.AddCondition(Condition.DownedMoonLord)
				.Register();

            Recipe.Create(ItemID.Clentaminator2)
                .AddIngredient(ItemID.Clentaminator)
                .AddCondition(shimmerTransmutationCondition)
                .AddCondition(Condition.DownedMoonLord)
                .Register();

            Recipe.Create(ItemID.BottomlessBucket)
                .AddIngredient(ItemID.BottomlessShimmerBucket)
                .AddCondition(shimmerTransmutationCondition)
                .AddCondition(Condition.DownedMoonLord)
                .Register();

            Recipe.Create(ItemID.BottomlessShimmerBucket)
                .AddIngredient(ItemID.BottomlessBucket)
                .AddCondition(shimmerTransmutationCondition)
                .AddCondition(Condition.DownedMoonLord)
                .Register();

            Recipe.Create(ItemID.LihzahrdWallUnsafe)
                .AddIngredient(ItemID.LihzahrdBrickWall)
                .AddCondition(shimmerTransmutationCondition)
                .AddCondition(Condition.DownedGolem)
                .Register();

            List<KeyValuePair<int, Condition>> luminiteBrickTransmutations = new() {
                new KeyValuePair<int, Condition>(ItemID.HeavenforgeBrick, Condition.MoonPhaseFull),
                new KeyValuePair<int, Condition>(ItemID.LunarRustBrick, Condition.MoonPhaseWaningGibbous),
                new KeyValuePair<int, Condition>(ItemID.AstraBrick, Condition.MoonPhaseThirdQuarter),
                new KeyValuePair<int, Condition>(ItemID.DarkCelestialBrick, Condition.MoonPhaseWaningCrescent),
                new KeyValuePair<int, Condition>(ItemID.MercuryBrick, Condition.MoonPhaseNew),
                new KeyValuePair<int, Condition>(ItemID.StarRoyaleBrick, Condition.MoonPhaseWaxingCrescent),
                new KeyValuePair<int, Condition>(ItemID.CryocoreBrick, Condition.MoonPhaseFirstQuarter),
                new KeyValuePair<int, Condition>(ItemID.CosmicEmberBrick, Condition.MoonPhaseWaxingGibbous)
            };

            foreach (var item in luminiteBrickTransmutations)
            {
                Recipe.Create(item.Key)
                    .AddIngredient(ItemID.LunarBrick)
                    .AddCondition(shimmerTransmutationCondition)
                    .AddCondition(item.Value)
                    .Register();
            }
        }

        public override void PostAddRecipes()
		{
			if (!Main.dedServ) {
				LootCacheManager.Setup(RecipeBrowser.instance);
				RecipeBrowserUI.instance.PostSetupContent();
			}
		}

		public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
			if(!Main.dedServ && messageType == MessageID.PlayerTeam) {
				RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
			}
			return false;
		}

		public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7) {
			if (!Main.dedServ && msgType == MessageID.PlayerTeam) {
				RecipeBrowserUI.instance.favoritePanelUpdateNeeded = true;
			}
			return false;
		}
	}
}
