using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BossRelicsMod;

/// <summary>
/// The Queen's exclusive relic. The first three eligible cards drawn each
/// turn become Bound. The first Bound card played is replayed once, then the
/// remaining Bound cards are locked until the end of the turn.
/// </summary>
public sealed class QueensPuppetStrings : RelicModel
{
    private bool _boundCardPlayed;

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromAffliction<Bound>();

    [SavedProperty]
    public bool BoundCardPlayed
    {
        get => _boundCardPlayed;
        private set
        {
            AssertMutable();
            _boundCardPlayed = value;
            Status = value ? RelicStatus.Normal : RelicStatus.Active;
        }
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        ICombatState? combatState = Owner.Creature.CombatState;
        if (combatState is null ||
            card.Owner != Owner ||
            combatState.CurrentSide != Owner.Creature.Side ||
            !ModelDb.Affliction<Bound>().CanAfflict(card))
        {
            return;
        }

        int afflictedThisTurn = CombatManager.Instance.History.Entries
            .OfType<CardAfflictedEntry>()
            .Count(entry =>
                entry.HappenedThisTurn(combatState) &&
                entry.Actor == Owner.Creature &&
                entry.Affliction is Bound);

        if (afflictedThisTurn >= DynamicVars.Cards.IntValue)
        {
            return;
        }

        await CardCmd.AfflictAndPreview<Bound>(
            [card],
            DynamicVars.Cards.BaseValue,
            CardPreviewStyle.None);
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner != Owner || card.Affliction is not Bound)
        {
            return true;
        }

        return !BoundCardPlayed;
    }

    public override int ModifyCardPlayCount(
        CardModel card,
        Creature? target,
        int playCount)
    {
        if (card.Owner != Owner ||
            card.Affliction is not Bound ||
            BoundCardPlayed)
        {
            return playCount;
        }

        return playCount + 1;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner == Owner && card.Affliction is Bound && !BoundCardPlayed)
        {
            BoundCardPlayed = true;
            Flash();
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
            BoundCardPlayed = false;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature))
        {
            return Task.CompletedTask;
        }

        BoundCardPlayed = false;
        IEnumerable<CardModel> allCards =
            Owner.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();

        foreach (CardModel card in allCards)
        {
            if (card.Affliction is Bound)
            {
                CardCmd.ClearAffliction(card);
            }
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        BoundCardPlayed = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
