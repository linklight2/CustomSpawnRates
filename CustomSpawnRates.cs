using System;
using Terraria;
using Terraria.ModLoader;

namespace CustomSpawnRates
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class CustomSpawnRates : Mod
	{
        public const int DefaultSpawnRateMultiplier = 1;
        public const int DefaultSpawnRateDivisor = 1;
        public const float DefaultSpawnMaxMult = 1f;
        public const int DefaultMaxSpawns = 10000;
        public const bool DefaultDisableOnBoss = false;
    }

    public class GlobalNPCRateModifier : GlobalNPC
    {
        public bool IsBossActive()
        {
            bool active = false;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active)
                    continue;

                if (npc.boss)
                {
                    active = true;
                    break;
                }
            }

            return active;
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            SpawnRatesConfig config = ModContent.GetInstance<SpawnRatesConfig>();

            // If the User is not actively using the mod, disable it
            // Changed math formulas to leave the spawn rate completely unmodified if at default values,
            // So this if statement is not needed for now.
            /*
            if (config.SpawnRateMultiplier == CustomSpawnRates.DefaultSpawnRateMultiplier && 
                config.SpawnRateDivisor == CustomSpawnRates.DefaultSpawnRateDivisor && 
                config.MaxSpawns == CustomSpawnRates.DefaultMaxSpawns &&
                config.SpawnMaxMult == CustomSpawnRates.DefaultSpawnMaxMult)
            {
                return;
            }
            */

            if (config.DisableOnBoss && IsBossActive())
            {
                return;
            }

            float spawnRateModifier = (float)config.SpawnRateMultiplier / (float)config.SpawnRateDivisor;
            float trueMaxSpawns = config.MaxSpawns == CustomSpawnRates.DefaultMaxSpawns ? config.MaxSpawns : maxSpawns * spawnRateModifier * config.SpawnMaxMult;

            maxSpawns = (int)trueMaxSpawns;
            spawnRate = (int)((float)spawnRate / spawnRateModifier);
        }
    }
}
