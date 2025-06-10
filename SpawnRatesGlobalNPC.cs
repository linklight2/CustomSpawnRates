using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace CustomSpawnRates
{
    public class SpawnRatesGlobalNPC : GlobalNPC
    {
        public static int bossActive = 0;
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            var config = ModContent.GetInstance<SpawnRatesConfig>();
            
            // Only do this if the config options allow it
            bool shouldPotentiallyDespawnNPC = config.MinRarityToSpawn != CustomSpawnRates.DefaultMinimumRarityToSpawn;
            shouldPotentiallyDespawnNPC = shouldPotentiallyDespawnNPC || config.MinRarityToSpawn != CustomSpawnRates.DefaultMaximumRarityToSpawn;
            // Only do this if this is a random spawn
            shouldPotentiallyDespawnNPC = shouldPotentiallyDespawnNPC && source is EntitySource_SpawnNPC;

            // Don't affect bosses.
            if (npc.boss)
                bossActive++;
            else if (shouldPotentiallyDespawnNPC)
            {
                // Flag it for a potential despawn
                CustomSpawnRates.AddNPCPotentialDespawn(npc);
            }
        }
        public override void OnKill(NPC npc)
        {
            if (npc.boss)
                bossActive--;
        }
        public bool IsBossActive()
        {
            return bossActive > 0;
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            SpawnRatesConfig config = ModContent.GetInstance<SpawnRatesConfig>();

            if (config.DisableOnBoss && IsBossActive())
                return;

            float spawnRateModifier = config.SpawnRateMultiplier / (float)config.SpawnRateDivisor;

            float trueMaxSpawns;
            if (config.MaxSpawns != CustomSpawnRates.DefaultMaxSpawns)
                trueMaxSpawns = config.MaxSpawns;
            else
                trueMaxSpawns = maxSpawns * spawnRateModifier;

            // 200 is the hardcoded limit for max spawns
            // No more NPCs can fit in the array of NPCs
            maxSpawns = (int)Math.Min(200, Math.Round(trueMaxSpawns));
            spawnRate = (int)Math.Round(spawnRate / spawnRateModifier);
        }

        // This only works for modded NPCs, no effect on vanilla NPCs
        /*
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            SpawnRatesConfig config = ModContent.GetInstance<SpawnRatesConfig>();
            List<int> npcToRemove = new List<int>();

            bool maxDisabled = config.MaxRarityToSpawn < config.MinRarityToSpawn;

            foreach (KeyValuePair<int, float> kvp in pool)
            {
                int npcType = kvp.Key;
                if (npcType == 0) // This is a vanilla npc so it should be handled by another function
                    continue;


                float spawnChance = kvp.Value;

                NPC npc = new NPC();

                npc.SetDefaults(npcType);

                    // This NPC should not be in the spawn pool, it does not fall within spawn constraints.
                    bool removeFromPool = npc.rarity < config.MinRarityToSpawn;
                removeFromPool = removeFromPool || (!maxDisabled && npc.rarity > config.MaxRarityToSpawn);

                if (removeFromPool)
                    npcToRemove.Add(npcType);
            }

            foreach (int npcType in npcToRemove)
                pool.Remove(npcType);
        }
        */
    }
}
