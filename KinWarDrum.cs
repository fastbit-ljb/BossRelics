using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace BossRelicsMod;

/// <summary>
/// The Kin's exclusive relic. Starting on turn two, not playing an Attack on
/// the previous turn causes the first Attack of this turn to gain one extra
/// play. The replay runs through the vanilla card-play pipeline and therefore
/// costs no additional Energy.
/// </summary>
public sealed class KinWarDrum : RelicModel
{
    private bool _attackPlayedLastTurn;
    private bool _attackPlayedThisTurn;
    private bool _replayUsedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic),
    ];

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!CombatManager.Instance.IsInProgress ||
            card.Owner != Owner ||
            card.Type != CardType.Attack ||
            Owner.PlayerCombatState is null ||
            Owner.PlayerCombatState.TurnNumber <= 1 ||
            _attackPlayedLastTurn ||
            _replayUsedThisTurn)
        {
            return playCount;
        }

        return playCount + 1;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        _replayUsedThisTurn = true;
        Status = RelicStatus.Normal;
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatManager.Instance.IsInProgress &&
            cardPlay.Card.Owner == Owner &&
            cardPlay.Card.Type == CardType.Attack)
        {
            _attackPlayedThisTurn = true;
        }

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
            _replayUsedThisTurn = false;
            Status = Owner.PlayerCombatState is { TurnNumber: > 1 } && !_attackPlayedLastTurn
                ? RelicStatus.Active
                : RelicStatus.Normal;
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
        {
            _attackPlayedLastTurn = _attackPlayedThisTurn;
            _attackPlayedThisTurn = false;
            _replayUsedThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _attackPlayedLastTurn = false;
        _attackPlayedThisTurn = false;
        _replayUsedThisTurn = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
