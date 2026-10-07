using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace BossRelicsMod;

/// <summary>
/// Soul Fysh's exclusive relic. It adds two temporary Beckons to the draw
/// pile at combat start and grants Intangible every fourth player turn.
/// </summary>
public sealed class GhostlySwimBladder : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new PowerVar<IntangiblePower>(1m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<Beckon>()
            .Append(HoverTipFactory.FromPower<IntangiblePower>());

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner || Owner.PlayerCombatState is not { TurnNumber: 1 })
        {
            return;
        }

        Flash();
        for (int i = 0; (decimal)i < DynamicVars.Cards.BaseValue; i++)
        {
            CardModel card = combatState.CreateCard(ModelDb.Card<Beckon>(), Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Draw,
                Owner,
                CardPilePosition.Random));
        }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player ||
            !participants.Contains(Owner.Creature) ||
            Owner.PlayerCombatState is not { TurnNumber: >= 4 } playerState ||
            playerState.TurnNumber % 4 != 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<IntangiblePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["IntangiblePower"].BaseValue,
            Owner.Creature,
            null);
    }
}
