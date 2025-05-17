using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CustomSpawnRates
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class CustomSpawnRates : Mod
	{
        public const int DefaultSpawnRateMultiplier = 1;
        public const int DefaultSpawnRateDivisor = 1;
        public const int DefaultMaxSpawnsMultiplier = 1;
        public const int DefaultMaxSpawnsDivisor = 1;
        public const int DefaultMaxSpawns = -1;
        public const int DefaultSpawnRate = 0;
        public const bool DefaultDisableOnBoss = false;
        public const bool DefaultDisableCalculateMaxSpawnsWithSpawnRate = false;
    }

    public class GeneralSpawnRateMultiplier : GlobalNPC
    {
        public static int bossActive = 0;
        public static int npcActive = 0;
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (Main.netMode != Terraria.ID.NetmodeID.Server)
                return;

            npcActive++;
            if (npc.boss)
                bossActive++;
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode != Terraria.ID.NetmodeID.Server)
                return;

            npcActive--;
            if (npc.boss)
                bossActive--;

            // Only Debug code from this point onwards
            if (!ModContent.GetInstance<SpawnRatesConfig>().DebugMode)
                return;

            int currentBossActiveCount = 0;
            int currentNpcActiveCount = 0;

            foreach (NPC activeNpc in Main.ActiveNPCs)
            {
                currentBossActiveCount++;
                if (activeNpc.boss)
                    currentBossActiveCount++;
            }

            if (currentBossActiveCount != bossActive)
            {
                Main.NewText("bossActive != currentBossActiveCount.", Microsoft.Xna.Framework.Color.Red);
                bossActive = currentBossActiveCount;
            }

            if (currentNpcActiveCount != npcActive)
            {
                Main.NewText("bossActive != currentBossActiveCount.", Microsoft.Xna.Framework.Color.White);
                npcActive = currentNpcActiveCount;
            }
        }
        public bool IsBossActive()
        {
            return bossActive > 0;
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            SpawnRatesConfig config = ModContent.GetInstance<SpawnRatesConfig>();

            // If the User is not actively using the mod, disable it
            // Changed math formulas to leave the spawn rate and max spawns unmodified if at default values,
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

            float spawnRateModifier = config.SpawnRateMultiplier / (float)config.SpawnRateDivisor;
            float maxSpawnsModifier = config.MaxSpawnsMultiplier / (float)config.MaxSpawnsDivisor;

            float trueMaxSpawns;

            if (config.MaxSpawns != CustomSpawnRates.DefaultMaxSpawns)
            {
                trueMaxSpawns = config.MaxSpawns;
            }
            else if (config.SpawnRate != CustomSpawnRates.DefaultSpawnRate || config.DisableCalculateMaxSpawnsWithSpawnRate)
            {
                trueMaxSpawns = maxSpawns * maxSpawnsModifier;
            }
            else
            {
                trueMaxSpawns = maxSpawns * spawnRateModifier * maxSpawnsModifier;
            }

            maxSpawns = (int)trueMaxSpawns;

            if (config.SpawnRate == CustomSpawnRates.DefaultSpawnRate)
            { 
                spawnRate = (int)(spawnRate / spawnRateModifier);
            }
            else
            {
                spawnRate = config.SpawnRate;
            }
        }
    }
}
