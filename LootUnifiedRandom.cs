using System;
using Terraria;
using Terraria.Utilities;

namespace RecipeBrowser
{
	internal class LootUnifiedRandom : UnifiedRandom
	{
		//internal static int entropy;
		internal static int loop;

		//internal static bool zero;
		private int[] returns;

		// realRandom avoids deadlock caused by re-rolling prefixes
		internal UnifiedRandom realRandom;

		public LootUnifiedRandom()
		{
			returns = new int[5000];
		}

		public override int Next(int maxValue)
		{
			if (loop == 0 && !realRandom.NextBool(100)) return 0;
			int index = Math.Abs(maxValue) % 5000;
			returns[index]++;
			return (maxValue + returns[index]) % maxValue;
		}

		public override int Next(int minValue, int maxValue)
		{
			if (loop == 0) return minValue;
			if (minValue == maxValue) return minValue;
			int index = Math.Abs(maxValue) % 5000;
			returns[index]++;
			return minValue + ((maxValue + returns[index]) % (maxValue - minValue));
		}

		public override double NextDouble()
		{
			if (loop == 0) return 0.0;
			return base.NextDouble();
		}

		//     public override int Next(int maxValue)
		//     {
		//if (loop == 0) return 0;
		//entropy += 257;
		//if (loop % 7 == 0 && maxValue > 20 && entropy % 13 == 0) return 0;
		//entropy += loop + maxValue;
		//entropy = entropy < 0 ? 0 : entropy;
		//         return (maxValue + entropy) % maxValue;
		//     }

		//     public override int Next(int minValue, int maxValue)
		//     {
		//if (loop == 0) return minValue;
		//entropy += loop + maxValue;
		//         return minValue + ((maxValue + entropy) % (maxValue - minValue));
		//     }
	}
}