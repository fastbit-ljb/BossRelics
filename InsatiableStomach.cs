using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace BossRelicsMod;

/// <summary>
/// The Insatiable's exclusive relic. It grants one additional maximum Energy
/// and shuffles two temporary Frantic Escapes into the draw pile each combat.
/// </summary>
public sealed class InsatiableStomach : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<FranticEscape>()
            .Prepend(HoverTipFactory.ForEnergy(this));

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        return player == Owner
            ? amount + DynamicVars.Energy.BaseValue
            : amount;
    }

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
        List<CardModel> cards = [];
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            cards.Add(combatState.CreateCard<FranticEscape>(Owner));
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(
            cards,
            PileType.Draw,
            Owner,
            CardPilePosition.Random));
    }
}
