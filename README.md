# Boss Relics / 首领专属遗物

Adds predetermined exclusive relic rewards to specific Slay the Spire 2 bosses.

为《杀戮尖塔2》的九名首领添加固定掉落的专属遗物。专属遗物作为额外奖励出现，不会替换原版首领遗物。

## v0.9.3

- No game-version or Steam-branch restriction; intended for all currently available versions.
- Built against the latest public-beta damage-modifier API.
- Fixed Forbidden Tome freezing the combat at the end of turn one by selecting the correct damage-command overload at runtime on both current branches.

- Vantom drops **Slippery Sticky Substance / 滑溜的粘稠物质**.
- At the start of each combat, its owner gains 1 stack of the vanilla Slippery power.
- Ceremonial Beast drops **Broken Ritual Horn / 破碎祭角**.
- Once per combat, falling to 50% HP or less stuns every living enemy for 1 turn.
- Broken Ritual Horn uses a simplified, lower-fidelity icon based on the actual Ceremonial Beast's cyan antlers, green neck feathers, and golden ritual ornaments.
- The Kin drops **Kin War Drum / 同族战鼓**.
- From turn 2 onward, if no Attack was played last turn, the first Attack this turn is played twice.
- Lagavulin Matriarch drops **Sleeping Carapace / 沉眠甲壳**.
- At the start of turns 3, 6, 9, and so on, all living enemies lose 1 Strength.
- Waterfall Giant drops **Overpressure Core / 过压核心**.
- Every manually played card adds 1 Pressure. At 10 Pressure, clear it and deal 15 damage to all enemies.
- Soul Fysh drops **Ghostly Swim Bladder / 幽魂鱼鳔**.
- At combat start, shuffle 2 temporary Beckons into the draw pile. Every 4 turns, gain 1 Intangible.
- The Insatiable drops **Insatiable Stomach / 无厌胃袋**.
- Gain 1 Energy each turn. At combat start, shuffle 2 temporary Frantic Escapes into the draw pile.
- Knowledge Demon drops **Forbidden Tome / 禁忌典籍**.
- At combat start, choose 1 of 3 random Rare character cards. It costs 0 this turn and Exhausts; take 15 damage at the end of turn 1.
- Kaiser Crab drops **Kaiser Twin Claws / 凯撒双钳**.
- Each turn, the first card that damages an enemy deals 150% normal damage; all other card damage deals 80%. The multipliers do not stack.
- The exclusive reward is generated independently for every player in multiplayer.
- The relic is registered outside normal random relic pools.

Developer console tests:

- `relic add SLIPPERY_STICKY_SUBSTANCE`
- `relic add BROKEN_RITUAL_HORN`
- `relic add KIN_WAR_DRUM`
- `relic add SLEEPING_CARAPACE`
- `relic add OVERPRESSURE_CORE`
- `relic add GHOSTLY_SWIM_BLADDER`
- `relic add INSATIABLE_STOMACH`
- `relic add FORBIDDEN_TOME`
- `relic add KAISER_TWIN_CLAWS`
- `fight VANTOM_BOSS`
- `fight CEREMONIAL_BEAST_BOSS`
- `fight THE_KIN_BOSS`
- `fight LAGAVULIN_MATRIARCH_BOSS`
- `fight WATERFALL_GIANT_BOSS`
- `fight SOUL_FYSH_BOSS`
- `fight THE_INSATIABLE_BOSS`
- `fight KNOWLEDGE_DEMON_BOSS`
- `fight KAISER_CRAB_BOSS`

## Build / 构建

Requires .NET 9 SDK and Python with Pillow.

需要 .NET 9 SDK，以及安装了 Pillow 的 Python。

Set `STS2_GAME_DIR` to the Slay the Spire 2 installation directory, then run:

将 `STS2_GAME_DIR` 设置为《杀戮尖塔2》的安装目录，然后运行：

```powershell
dotnet build .\BossRelics.csproj -c Release
python .\tools\build_pck.py
```

Outputs / 输出：

- `bin/Release/net9.0/BossRelics.dll`
- `build/BossRelics.pck`
- `BossRelics.json`

