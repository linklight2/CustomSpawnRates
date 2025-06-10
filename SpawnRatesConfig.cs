using System;
using System.ComponentModel;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.Config;

namespace CustomSpawnRates
{
    public class SpawnRatesConfig : ModConfig
    {
        [DefaultValue(CustomSpawnRates.DefaultSpawnRateMultiplier)]
        [Range(1, 1000)]
        public int SpawnRateMultiplier;

        [DefaultValue(CustomSpawnRates.DefaultSpawnRateDivisor)]
        [Range(1, 1000)]
        public int SpawnRateDivisor;

        [DefaultValue(CustomSpawnRates.DefaultMaxSpawns)]
        [Range(-1, 200)]
        public int MaxSpawns;

        [Header("Advanced-Options")]

        [DefaultValue(CustomSpawnRates.DefaultMinimumRarityToSpawn)]
        [Range(0, 5)]
        public int MinRarityToSpawn;

        [DefaultValue(CustomSpawnRates.DefaultMaximumRarityToSpawn)]
        [Range(-1, 5)]
        public int MaxRarityToSpawn;

        [DefaultValue(CustomSpawnRates.DefaultDisableOnBoss)]
        public bool DisableOnBoss;

        public override ConfigScope Mode => ConfigScope.ServerSide;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
        {
            if (!NetMessage.DoesPlayerSlotCountAsAHost(whoAmI))
            {
                message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost", Array.Empty<object>());
                return false;
            }
            return true;
        }
    }
}