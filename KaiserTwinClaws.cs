using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossRelicsMod;

/// <summary>
/// Kaiser Crab's exclusive relic. Each turn, the first card that damages an
/// enemy deals 150% of its normal damage. All later card damage deals 80%.
/// These are mutually exclusive multipliers and are never multiplied together.
/// </summary>
public sealed class KaiserTwinClaws : RelicModel
{
    private const decimal FirstCardMultiplier = 1.5m;
    private const decimal OtherCardMultiplier = 0.8m;

    private bool _firstDamageUsed;
    private CardModel? _boostedCard;

    public override RelicRarity Rarity => RelicRarity.Event;

    [SavedProperty]
    public bool FirstDamageUsed
    {
        get => _firstDamageUsed;
        private set
        {
            AssertMutable();
            _firstDamageUsed = value;
            Status = value ? RelicStatus.Normal : RelicStatus.Active;
        }
    }

    private CardModel? BoostedCard
    {
        get => _boostedCard;
        set
        {
            AssertMutable();
            _boostedCard = value;
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target is null ||
            target.Side == Owner.Creature.Side ||
            cardSource is null ||
            cardSource.Owner != Owner)
        {
            return 1m;
        }

        if (FirstDamageUsed)
        {
            return OtherCardMultiplier;
        }

        // Preview the currently applicable multiplier without consuming it.
        if (cardPlay is null)
        {
            return FirstCardMultiplier;
        }

        if (BoostedCard is null)
        {
            BoostedCard = cardSource;
            Status = RelicStatus.Normal;
            Flash();
        }

        return cardSource == BoostedCard
            ? FirstCardMultiplier
            : OtherCardMultiplier;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (BoostedCard != cardPlay.Card || !cardPlay.IsLastInSeries)
        {
            return Task.CompletedTask;
        }

        FirstDamageUsed = true;
        BoostedCard = null;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
        {
            FirstDamageUsed = false;
            BoostedCard = null;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        FirstDamageUsed = false;
        BoostedCard = null;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
