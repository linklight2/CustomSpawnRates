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
        [Range(1, 10000)]
        public int SpawnRateMultiplier;

        [DefaultValue(CustomSpawnRates.DefaultSpawnRateDivisor)]
        [Range(1, 10000)]
        public int SpawnRateDivisor;

        [DefaultValue(CustomSpawnRates.DefaultSpawnMaxMult)]
        [Range(0f, 1f)]
        public float SpawnMaxMult;

        [DefaultValue(CustomSpawnRates.DefaultMaxSpawns)]
        [Range(0, 10000)]
        public int MaxSpawns;

        [DefaultValue(CustomSpawnRates.DefaultDisableOnBoss)]
        public bool DisableOnBoss;

        [DefaultValue(false)]
        public bool DebugMode;

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