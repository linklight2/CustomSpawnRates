using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using tModPorter;
using Microsoft.Xna.Framework;

namespace CustomSpawnRates
{
	public struct NPCPotentialDespawnInfo
    {
        public NPC npc;
        public float spawnX;
        public float spawnY;
        public int timeLeft;
        public NPCPotentialDespawnInfo(NPC npc, float spawnX, float spawnY, int timeLeft)
        {
            this.npc = npc;
            this.spawnX = spawnX;
            this.spawnY = spawnY;
            this.timeLeft = timeLeft;
        }
    }
	public class CustomSpawnRates : Mod
	{
        public const int DefaultSpawnRateMultiplier = 1;
        public const int DefaultSpawnRateDivisor = 1;
        public const int DefaultMaxSpawnsMultiplier = 1;
        public const int DefaultMaxSpawnsDivisor = 1;
        public const int DefaultMaxSpawns = -1;
        public const int DefaultSpawnRate = 0;
        public const int DefaultMinimumRarityToSpawn = 0;
        public const int DefaultMaximumRarityToSpawn = -1;
        public const bool DefaultDisableOnBoss = false;
        public static readonly Vector2 despawnPosition = new Vector2(1, 1);
        public static On_NPC.orig_SpawnNPC TerrariaSpawnNPC;
        private static List<NPCPotentialDespawnInfo> potentialDespawns = new List<NPCPotentialDespawnInfo>();

        public override void Load()
        {
            // This hook breaks after it's called too many times and no longer calls the modified spawn function
            // The only use of it is to store the orig as our own static hook to call the function ourselves.
            On_NPC.SpawnNPC += SpawnNPC;
            On_NPC.NewNPC += CorrectlyTrackWhoAmI;
            // Because of this hook's proximity after spawn NPC and the fact that its called from single player or server only code
            // It was a prime point to insert the modified SpawnNPC code
            On_PressurePlateHelper.Update += DespawnCommonNPCs;
        }
        public static void AddNPCPotentialDespawn(NPC npcToAdd)
        {
            // Flag NPC for despawn
            potentialDespawns.Add(new NPCPotentialDespawnInfo(npcToAdd, npcToAdd.position.X, npcToAdd.position.Y, npcToAdd.timeLeft));

            // Immediately move the NPC far off screen to wait for handler code
            if (npcToAdd.position.Distance(despawnPosition) < Main.offScreenRange * 5)
            {
                npcToAdd.position.X += 10000;
            }
            else
            {
                npcToAdd.position.X = despawnPosition.X;
                npcToAdd.position.Y = despawnPosition.Y;
            }

            npcToAdd.timeLeft = 0;
            npcToAdd.active = false;
        }

        // Temporarily holds the place of potentially despawnable NPCs, stopping them from getting overwritten before they are rarity checked.
        internal static int CorrectlyTrackWhoAmI(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int Target = 255)
        {
            int size = potentialDespawns.Count;
            int modStart = Start;

            if (source is EntitySource_SpawnNPC && size > 0)
            {
                // Look at the end of the list first
                modStart = potentialDespawns[size - 1].npc.whoAmI + 1;
            }

            return orig(source, X, Y, Type, modStart, ai0, ai1, ai2, ai3, Target);
        }

        // The hook keeps breaking for some reason, calling it indirectly instead
        internal static void SpawnNPC(On_NPC.orig_SpawnNPC orig)
        {
            TerrariaSpawnNPC ??= orig;
            TerrariaSpawnNPC();
        }

        // Despawn any NPCs not of the specified rarity. 
        // Repeat the Spawn NPC function up to the ceiling of Log2(configSpawnRate * 10) times if commons are disabled.
        // This is to artificially increase the likelyhood of getting a luckbased rare NPC to spawn by the user-set spawn rate without being too in-efficient.
        internal static void DespawnCommonNPCs(On_PressurePlateHelper.orig_Update orig)
        {
            SpawnRatesConfig config = ModContent.GetInstance<SpawnRatesConfig>();

            float spawnRateModifier = config.SpawnRateMultiplier / config.SpawnRateDivisor;
            int retryMax = (int)Math.Max(1, Math.Ceiling(Math.Log2(spawnRateModifier * spawnRateModifier)));
            bool maxDisabled = config.MaxRarityToSpawn < config.MinRarityToSpawn;

            // Main.NewText(Main.npc.Length.ToString());

            for (int retryCount = 0; retryCount < retryMax; retryCount++)
            {
                foreach (var info in potentialDespawns)
                {
                    bool shouldDespawn = info.npc.rarity < config.MinRarityToSpawn;
                    shouldDespawn = shouldDespawn || (!maxDisabled && info.npc.rarity > config.MaxRarityToSpawn);

                    if (!shouldDespawn)
                    {
                        // Moves the NPC back to the correct spawn position
                        info.npc.active = true;
                        info.npc.timeLeft = info.timeLeft;
                        info.npc.position.X = info.spawnX;
                        info.npc.position.Y = info.spawnY;
                    }
                }

                potentialDespawns.Clear();

                // Only call SpawnNPC() if SpawnNPC hasn't already been called
                // Only call SpawnNPC() if the minimum rarity is not 0 (so we dont artificially boost commons)
                // This is to artificially boost the spawn rates of luck-based NPCs mostly unaffected by the general spawn rate.
                if (retryCount > 0 && config.MinRarityToSpawn > 0)
                    TerrariaSpawnNPC();
            }

            orig();
        }
    }
}
