using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossRelicsMod;

/// <summary>
/// Waterfall Giant's exclusive relic. Manually played cards build Pressure;
/// replays and auto-played cards are intentionally excluded. At ten Pressure,
/// the counter is emptied and unpowered damage is dealt to every enemy.
/// </summary>
public sealed class OverpressureCore : RelicModel
{
    private const string PressureThresholdKey = "PressureThreshold";

    private int _pressure;
    private bool _isActivating;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => IsActivating
        ? DynamicVars[PressureThresholdKey].IntValue
        : Pressure;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(PressureThresholdKey, 10m),
        new DamageVar(15m, ValueProp.Unpowered),
    ];

    private int Pressure
    {
        get => _pressure;
        set
        {
            AssertMutable();
            _pressure = value;
            RefreshDisplay();
        }
    }

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            RefreshDisplay();
        }
    }

    private void RefreshDisplay()
    {
        int threshold = DynamicVars[PressureThresholdKey].IntValue;
        Status = !IsActivating && Pressure == threshold - 1
            ? RelicStatus.Active
            : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        Pressure = 0;
        IsActivating = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress ||
            cardPlay.Card.Owner != Owner ||
            cardPlay.IsAutoPlay ||
            !cardPlay.IsFirstInSeries)
        {
            return;
        }

        Pressure++;
        int threshold = DynamicVars[PressureThresholdKey].IntValue;
        if (Pressure < threshold)
        {
            return;
        }

        IsActivating = true;
        Pressure = 0;
        Flash();

        List<Creature> enemies = Owner.Creature.CombatState?.HittableEnemies.ToList() ?? [];
        if (enemies.Count > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                enemies,
                DynamicVars.Damage,
                Owner.Creature);
        }

        IsActivating = false;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Pressure = 0;
        IsActivating = false;
        return Task.CompletedTask;
    }
}
