using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace BossRelicsMod;

[ModInitializer(initializerMethod: nameof(Initialize))]
public static class ModInit
{
    private static void Initialize()
    {
        Harmony harmony = new("fastbit-ljb.BossRelics");
        harmony.PatchAll(typeof(ModInit).Assembly);
    }
}

/// <summary>
/// Registers boss-only relic models without adding them to normal relic rolls.
/// EventRelicPool is used as a registration-only pool by this mod.
/// </summary>
[HarmonyPatch]
internal static class BossRelicRegistrationPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventRelicPool), "GenerateAllRelics")]
    private static void RegisterBossRelics(ref IEnumerable<RelicModel> __result)
    {
        __result = __result
            .Append(ModelDb.Relic<SlipperyStickySubstance>())
            .Append(ModelDb.Relic<BrokenRitualHorn>())
            .Append(ModelDb.Relic<KinWarDrum>())
            .Append(ModelDb.Relic<SleepingCarapace>())
            .Append(ModelDb.Relic<OverpressureCore>())
            .Append(ModelDb.Relic<GhostlySwimBladder>())
            .Append(ModelDb.Relic<InsatiableStomach>())
            .Append(ModelDb.Relic<ForbiddenTome>())
            .Append(ModelDb.Relic<KaiserTwinClaws>());
    }
}

/// <summary>
/// Adds a predetermined, per-player relic reward to the matching boss reward
/// screen. It never consumes or modifies the normal boss relic roll.
/// </summary>
[HarmonyPatch]
internal static class BossRelicRewardPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom))]
    private static void AddExclusiveBossRelic(RewardsSet __instance, AbstractRoom room)
    {
        if (room is not CombatRoom combatRoom)
        {
            return;
        }

        RelicModel? canonicalRelic = combatRoom.Encounter switch
        {
            VantomBoss => ModelDb.Relic<SlipperyStickySubstance>(),
            CeremonialBeastBoss => ModelDb.Relic<BrokenRitualHorn>(),
            TheKinBoss => ModelDb.Relic<KinWarDrum>(),
            LagavulinMatriarchBoss => ModelDb.Relic<SleepingCarapace>(),
            WaterfallGiantBoss => ModelDb.Relic<OverpressureCore>(),
            SoulFyshBoss => ModelDb.Relic<GhostlySwimBladder>(),
            TheInsatiableBoss => ModelDb.Relic<InsatiableStomach>(),
            KnowledgeDemonBoss => ModelDb.Relic<ForbiddenTome>(),
            KaiserCrabBoss => ModelDb.Relic<KaiserTwinClaws>(),
            _ => null,
        };

        if (canonicalRelic is null)
        {
            return;
        }

        bool alreadyOwned = __instance.Player.GetRelicById(canonicalRelic.Id) is not null;
        bool alreadyRewarded = __instance.Rewards
            .OfType<RelicReward>()
            .Any(reward => reward.Relic?.Id == canonicalRelic.Id);

        if (!alreadyOwned && !alreadyRewarded)
        {
            __instance.Rewards.Add(new RelicReward(canonicalRelic.ToMutable(), __instance.Player));
        }
    }
}
