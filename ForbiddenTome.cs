using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BossRelicsMod;

/// <summary>
/// Knowledge Demon's exclusive relic. At combat start it offers three random
/// rare character cards; the chosen temporary card is free this turn and
/// Exhausts. The owner then takes normal damage at the end of turn one.
/// </summary>
public sealed class ForbiddenTome : RelicModel
{
    private const string PenaltyDamageKey = "PenaltyDamage";

    private static readonly MethodInfo? DamageWithCardPlayMethod = typeof(CreatureCmd).GetMethod(
        nameof(CreatureCmd.Damage),
        BindingFlags.Public | BindingFlags.Static,
        null,
        [
            typeof(PlayerChoiceContext),
            typeof(Creature),
            typeof(DamageVar),
            typeof(Creature),
            typeof(CardModel),
            typeof(CardPlay),
        ],
        null);

    private static readonly MethodInfo? DamageWithoutCardPlayMethod = typeof(CreatureCmd).GetMethod(
        nameof(CreatureCmd.Damage),
        BindingFlags.Public | BindingFlags.Static,
        null,
        [
            typeof(PlayerChoiceContext),
            typeof(Creature),
            typeof(DamageVar),
            typeof(Creature),
            typeof(CardModel),
        ],
        null);

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DamageVar(PenaltyDamageKey, 15m, ValueProp.Unpowered),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner || Owner.PlayerCombatState is not { TurnNumber: 1 })
        {
            return;
        }

        List<CardModel> options = CardFactory.GetDistinctForCombat(
                Owner,
                Owner.Character.CardPool
                    .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                    .Where(card => card.Rarity == CardRarity.Rare),
                DynamicVars.Cards.IntValue,
                Owner.RunState.Rng.CombatCardGeneration)
            .ToList();

        if (options.Count == 0)
        {
            return;
        }

        Flash();
        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner);

        if (chosenCard is null)
        {
            return;
        }

        chosenCard.SetToFreeThisTurn();
        chosenCard.AddKeyword(CardKeyword.Exhaust);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
            chosenCard,
            PileType.Hand,
            Owner));
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player ||
            !participants.Contains(Owner.Creature) ||
            Owner.PlayerCombatState is not { TurnNumber: 1 })
        {
            return;
        }

        Flash();
        await DealPenaltyDamage(choiceContext);
    }

    private async Task DealPenaltyDamage(PlayerChoiceContext choiceContext)
    {
        DamageVar damage = (DamageVar)DynamicVars[PenaltyDamageKey];
        MethodInfo method = DamageWithCardPlayMethod
            ?? DamageWithoutCardPlayMethod
            ?? throw new MissingMethodException(
                typeof(CreatureCmd).FullName,
                nameof(CreatureCmd.Damage));

        object?[] arguments = method == DamageWithCardPlayMethod
            ? [choiceContext, Owner.Creature, damage, null, null, null]
            : [choiceContext, Owner.Creature, damage, null, null];

        if (method.Invoke(null, arguments) is not Task task)
        {
            throw new InvalidOperationException("CreatureCmd.Damage did not return a Task.");
        }

        await task;
    }
}
