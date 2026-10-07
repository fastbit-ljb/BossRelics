using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace BossRelicsMod;

/// <summary>
/// Ceremonial Beast's exclusive relic. Once per combat, crossing the 50% HP
/// threshold replaces every living enemy's next move with the vanilla stunned
/// move. Already-stunned enemies are left unchanged to avoid chaining STUNNED
/// as their own resume move.
/// </summary>
public sealed class BrokenRitualHorn : RelicModel
{
    private const string HpThresholdKey = "HpThreshold";

    private bool _triggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(HpThresholdKey, 50m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Stun),
    ];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (_triggeredThisCombat ||
            delta >= 0m ||
            creature != Owner.Creature ||
            creature.IsDead ||
            !CombatManager.Instance.IsInProgress ||
            creature.CombatState is null ||
            (decimal)creature.CurrentHp > creature.MaxHp * (DynamicVars[HpThresholdKey].BaseValue / 100m))
        {
            return;
        }

        _triggeredThisCombat = true;
        Status = RelicStatus.Active;
        Flash();

        List<Creature> enemies = creature.CombatState.HittableEnemies
            .Where(enemy => !enemy.IsDead && !enemy.IsStunned)
            .ToList();

        foreach (Creature enemy in enemies)
        {
            await CreatureCmd.Stun(enemy);
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _triggeredThisCombat = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
