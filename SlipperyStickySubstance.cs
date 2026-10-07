using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace BossRelicsMod;

/// <summary>
/// Vantom's exclusive relic. The vanilla Slippery power changes the next
/// instance of HP loss to exactly 1, then consumes one stack.
/// </summary>
public sealed class SlipperyStickySubstance : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SlipperyPower>(1m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<SlipperyPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["SlipperyPower"].BaseValue,
            Owner.Creature,
            null);
    }
}
