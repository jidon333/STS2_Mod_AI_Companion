# 하네스 상태/증거 매트릭스

> 상태: 현재 사용 중
> 기준 문서: 예, for current `main` regression evidence coverage
> 갱신 시점: self-test, replay fixture, representative live root, 또는 상태 계약이 바뀔 때

이 문서는 [HARNESS_REGRESSION_CHECKLIST_KO.md](./HARNESS_REGRESSION_CHECKLIST_KO.md)의 각 상태가 **현재 무엇으로 고정되어 있는지**를 매핑한다.

구현 owner note:

- 아래 evidence text는 current owner file 또는 current self-test family를 가리킨다.
- current owner file map은 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)를 기준으로 본다.
- 이 표의 source of truth는 row 상태와 evidence strength이며, old monolith file name 자체가 아니다.

coverage status 의미:

- `green`: current `main`에서 self-test / replay / representative live root가 모두 충분하거나, 상태 계약을 신뢰할 만큼 evidence가 강함
- `partial`: evidence는 있으나 release/handoff 또는 long-run continuity까지는 덜 고정됨
- `missing`: current `main` 기준 canonical evidence가 부족함

## Immediate Priority Gaps

| Gap | Meaning | Current Status |
|---|---|---|
| `REWARD-CARD-CLICK-PROGRESSION` | `HandleRewards` explicit card reward row가 same-action-stall 없이 실제 progression으로 이어지는지 | partial |
| `COMBAT-BARRIER-BUDGET` | short validation roots에서 `HandleCombat` barrier long-tail이 step-budget 또는 plateau로 새는지 | partial |
| `VAL-LIVE-VIDEO-REVIEW` | ffmpeg live metadata는 남지만 `gdigrab` review video가 usable한지 | partial |
| `MAP-POSTNODE-REWARD-REOPEN` | `WaitPostMapNodeRoom` reward destination reopen contract | green |

## Validation / Boot / Menu

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `VAL-01` | validation | deploy/runtime identity | green | [SMOKE_TEST_CHECKLIST.md](../runbooks/SMOKE_TEST_CHECKLIST.md), [PROJECT_STATUS.md](./PROJECT_STATUS.md) startup/trust roots | current `main` 운영 규칙 고정 |
| `VAL-02` | validation | manual-clean-boot | green | [ROADMAP.md](../ROADMAP.md) M4, [PROJECT_STATUS.md](./PROJECT_STATUS.md) bootstrap/trust roots | trust gate는 current baseline에 포함 |
| `VAL-03` | any phase post-log | capture boundary | green | `Program.SelfTests.CaptureReplay.cs`, fresh live root [capture-boundary-proof-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1), [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1), [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2) | capture timeout bounded failure contract는 green이고, current `main`은 live ffmpeg metadata도 남긴다. 다만 review video usable 여부는 별도 partial gap이다 |
| `MENU-01` | `WaitMainMenu` / `EnterRun` | main menu | green | `Program.SelfTests.CliStartup.cs`, `Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs`, [wait-main-menu-run-start-readiness-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1), [SMOKE_TEST_CHECKLIST.md](../runbooks/SMOKE_TEST_CHECKLIST.md) | continue vs singleplayer contract 고정, logo-animation bootstrap frame는 accepted 금지 |
| `MENU-02` | `EnterRun` | continue lane | green | `GetPostEnterRunPhase`, replay parity summaries, main-menu self-tests | continue preferred 유지 |
| `MENU-03` | `WaitRunLoad` | transition wait | green | `Program.SelfTests.PhaseRouting.RunLoadRecovery.cs`의 explicit transition truth wait-only assertions | explicit transition truth wait-only |
| `MENU-04` | `WaitRunLoad` | stale continue recovery | green | `Program.SelfTests.PhaseRouting.RunLoadRecovery.cs`의 stale continue recovery / retry assertions | stable continue retry 고정 |
| `MENU-05` | `WaitRunLoad` | resumed room branch | green | current self-test family for WaitRunLoad -> reward/event/treasure/combat branch assertions | post-load branch contract strong |

## Map / WaitMap / Post-Node

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `MAP-01` | `ChooseFirstNode` | map-node | green | replay parity fixtures expecting `foregroundOwner=map`, exported map-node self-tests | explicit map node routing strong |
| `MAP-02` | `ChooseFirstNode` / `HandleEvent` | map-overlay foreground | green | `Program.SelfTests.NonCombatForegroundOwnership.cs` map-overlay foreground assertions, `Program.SelfTests.StallSentinel.cs` `map-overlay-noop-loop` sentinel | stale event residue suppression covered |
| `MAP-03` | `WaitMap` | room reopen | green | `Program.SelfTests.NonCombatForegroundOwnership.cs`의 `WaitMap` reopen reward/event/treasure assertions | mixed modal reopen now follows canonical owner/release handoff directly |
| `MAP-04` | `WaitPostMapNodeRoom` | destination room handoff | green | [Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs](../../src/Sts2GuiSmokeHarness/Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs) reward/event/combat/rest/shop destination reopen assertions + representative live roots [observer-first-combat-speed-20260328-live6](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-combat-speed-20260328-live6) and [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1) | reward reopen self-test regression은 current `main`에서 닫혔고, exported room handoff continuity가 self-test/live로 다시 맞춰졌다 |

## Combat

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `COMBAT-01` | `WaitCombat` | combat acceptance | green | `Program.SelfTests.CombatContracts.*.cs`의 WaitCombat acceptance assertions (`ready`, `stable`, `inCombat`) | strong acceptance gate |
| `COMBAT-02` | `HandleCombat` | card select | green | combat opener self-tests, `Program.SelfTests.CombatContracts.NonEnemyAndRuntimeState.cs` explicit-slot assertions, and fresh live root [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9) | screenshot-primary slot selection은 current `main`에서 제거됐고, `live9` first combat가 observer-first card select로 다시 고정됐다 |
| `COMBAT-03` | `HandleCombat` | target lane | green | `Analysis/CombatMicroStageSupport.cs`, `Analysis/CombatPostActionObservationSupport.cs`, `Analysis/CombatTargetabilitySupport.cs`, `Program.SelfTests.CombatContracts.TargetSelection.cs`, parity fixtures `combat-target-wait.request.json` / `combat-target-click.request.json`, fresh live root [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2) | unresolved attack lane는 `end turn`으로 새지 않고, runtime `combatTargetSummary` raw fact까지 explicit target authority로 소비된다 |
| `COMBAT-04` | `HandleCombat` | cancel blocked selection | green | old blocker replay `0167.request.json` family, commit-era closure evidence referenced in current conversation | explicit cancel lane is required contract |
| `COMBAT-05` | `HandleCombat` | end-turn pre-ack | green | current code barriers + self-tests + fresh live roots [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) and [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2) | `auto-end turn`은 `PlayerActionOpen` stage에서만 열리고 unresolved non-enemy/attack lane에서는 금지된다 |
| `COMBAT-06` | `HandleCombat` | acknowledged transit wait | green | `Program.SelfTests.CombatContracts.ParityAndBarriers.cs`, `Program.SelfTests.CombatContracts.FallbacksAndProbeGrace.cs`, fresh live root [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2) | combat settle은 generic delta가 아니라 lane micro-stage terminal signal 또는 quiet convergence로만 종료된다 |
| `COMBAT-07` | `HandleCombat` | enemy-turn closed play phase | green | parity fixtures `combat-enemy-turn-wait.request.json` / `combat-player-turn-reopen.request.json`, cleanup proof root [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run) | enemy turn wait-only와 다음 player-turn reopen이 current `main` replay/live evidence로 고정됨 |
| `COMBAT-08` | `HandleCombat` | next-round reopen | green | fresh live root [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) | repeated `EndTurn` acknowledged transit reopens into new player-turn actions (`2턴 종료` -> `3턴 종료` -> `4턴 종료` -> `5턴 종료` ...) |
| `COMBAT-09` | post-wait recapture | capture/request continuity | green | `Program.SelfTests.CaptureReplay.cs` bounded capture assertions + fresh cleanup proof root [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run) (`step=23 -> 24`, `45 -> 46`, `57 -> 58`) | legitimate combat wait now has fresh current-main live continuity evidence |
| `COMBAT-10` | combat -> rewards | room handoff | green | [PROJECT_STATUS.md](./PROJECT_STATUS.md) long-run continuity roots, repeated combat/reward continuity | strong long-run evidence in status doc |

## Event / Ancient / Treasure / Rest

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `EVENT-01` | `HandleEvent` | explicit event option | green | `Program.SelfTests.EventRewardSubstates.cs` event foreground signature/assertions + speed proof root [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) | explicit event owner keeps the room lane over background map residue and now runs observer-first |
| `EVENT-02` | `HandleEvent` | explicit event proceed | green | `Program.SelfTests.EventRewardSubstates.cs` explicit `EventOption.IsProceed` assertions + `Program.SelfTests.NonCombatForegroundOwnership.cs` event recovery assertions | proceed lane tactical + fast wait |
| `EVENT-03` | `HandleEvent` | ancient dialogue | green | `Program.SelfTests.EventRewardSubstates.cs` ancient dialogue/completion assertions | explicit ancient lane covered |
| `EVENT-04` | `HandleEvent` / `WaitEventRelease` | ancient dialogue / completion | green | `Program.SelfTests.EventRewardSubstates.cs` ancient allowlist, post-phase, release assertions | ancient lane split strong |
| `EVENT-05` | `HandleEvent` | event reward substate | partial | `Program.SelfTests.EventRewardSubstates.cs` canonical `EventSceneState` assertions delegate colorless/reward follow-up into reward lane; reward/event mixed tests still exist | owner/action/release now route through canonical event state, but fresh current-main live evidence is still thin |
| `EVENT-06` | `HandleEvent` / `ChooseFirstNode` | event/map mixed aftermath | green | `Program.SelfTests.NonCombatForegroundOwnership.cs` event/map mixed foreground assertions | event/map mixed aftermath now reads canonical owner truth end-to-end |
| `TREASURE-01` | `ChooseFirstNode` / `HandleEvent` | treasure room | green | treasure room self-tests, WaitMap reopen to treasure, status doc continuity roots | explicit treasure room state strong |
| `REST-01` | `ChooseFirstNode` | rest-site explicit choice | green | rest-site metadata-first self-tests | authoritative metadata contract strong |
| `REST-02` | `ChooseFirstNode` | rest-site post-click release | green | grace / noop / selection-failed self-tests | explicit release/no-op guard present |

## Rewards

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `REWARD-01` | `HandleRewards` | claim lane | green | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` reward fast-path signature and explicit `RelicReward` claim assertions, replay-step exact repro `0031.request.json` from [combat-barrier-release-20260331-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-barrier-release-20260331-live2) | reward-owned explicit `reward-type:*Reward` row는 inspect preview보다 강하며, explicit claim이 남아 있으면 proceed보다 먼저 실행된다 |
| `REWARD-02` | `HandleRewards` | reward card / colorless | partial | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` colorless and reward choice assertions, fresh live root [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1) attempt `0001` | generic bare `card` residue는 reward authority가 아니지만, explicit `CardReward` row 클릭이 live attempt `0001`에서 `same-action-stall`로 끝난 새 front blocker가 남아 있다 |
| `REWARD-03` | `HandleRewards` | reward skip / proceed | green | canonical reward release contract + current self-tests + fresh live root [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1) | same-click reissue suppression과 release-pending은 covered이고, explicit claim surface가 남아 있으면 proceed는 lower priority로 유지된다 |
| `REWARD-04` | `HandleRewards` | reward back | green | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs`, parity fixture `reward-back.request.json` | stale reward residue 위의 explicit back navigation이 current `main`에서 canonical reward lane으로 고정됨 |
| `REWARD-05` | `HandleRewards` | reward teardown / release wait | green | stale reward cleanup self-tests, layered reward state assertions, canonical reward release contract | teardown/release semantics are covered |
| `REWARD-06` | `HandleRewards` / `WaitMap` | reward/map mixed aftermath | green | layered reward state self-tests, released-to-map canonical owner assertions, parity fixture `reward-aftermath-map-handoff`, fresh live root [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1) | map owner truth now reaches exported reachable node selection |
| `REWARD-07` | `WaitMap` | reward -> map handoff | green | reward teardown self-tests, WaitMap reopen assertions, fresh live `step=15` exported map-node click in [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1) | post-reward map-node continuity closed on current `main` |
| `REWARD-09` | `HandleRewards` | same skip reissue suppression | green | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` reward release-pending assertions now prove same-band skip suppression, wait-only allowlist, and shared `WaitMap` handoff under the canonical reward contract | canonical reward release now owns reissue suppression instead of leaving it to the loop sentinel |
| `REWARD-10` | `HandleRewards` / `WaitMap` | reward-map loop sentinel | partial | `Program.SelfTests.StallSentinel.cs` `reward-map-loop` sentinel assertions | sentinel exists, but it should remain fallback not primary flow |

## Shop

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `SHOP-01` | `HandleShop` | shop foreground | green | shop authority helpers + self-tests + [PROJECT_STATUS.md](./PROJECT_STATUS.md) shop continuity roots | strong enough |
| `SHOP-02` | `HandleShop` | bounded purchase | green | shop buy relic/card/potion/card-removal decision code and self-tests | one bounded purchase contract exists |
| `SHOP-03` | `HandleShop` | back / proceed / map handoff | green | self-tests around proceed-before-reopen, status doc shop->map continuity, fresh live root [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4) | transient `HandleShop -> HandleRewards` misroute가 있어도 current `main`에서는 immediate reward-to-shop recovery 뒤 bounded purchase/back/proceed가 이어진다 |

## Terminal / Restart

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `TERM-01` | reward/menu terminal boundaries | terminal boundary | green | `RewardObserverSignals.IsTerminalRunBoundary`, terminal suppression tests, [PROJECT_STATUS.md](./PROJECT_STATUS.md) | gameplay map fallback suppression covered |
| `TERM-02` | terminal -> restart -> next attempt | lifecycle lane | green | fresh live root [strict-lifecycle-chain-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2), [restart-events.ndjson](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/restart-events.ndjson), [supervisor-state.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/supervisor-state.json) | validation-only lifecycle proof mode로 `0001 terminal -> 0002 restart -> 0002 first-screen` chain이 current `main` live root에서 고정됨 |

## Current-Main Evidence Index

### Representative Live Roots

- loader/runtime recovery:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap/trust:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- quartet/accounting:
  - [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- latest parity baseline:
  - green
- latest cleanup proof root:
  - [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
- retained end-turn continuity root:
  - [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1)
- latest speed proof root:
  - [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- latest reward validation root:
  - [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1)
- latest endurance natural-terminal root:
  - [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)

### Current-Main Self-Test Families

- WaitRunLoad transition hold / stale continue recovery / resumed room branching
- map-overlay foreground / canonical foreground owner
- explicit event proceed / ancient completion / event release
- reward fast-path / layered reward state / stale reward cleanup / reward-map-loop sentinel
- rest-site explicit metadata / post-click grace / no-op classification
- treasure room explicit lane
- WaitCombat acceptance / combat opener

## Immediate Follow-Up Backlog

| Priority | Work Item | Why |
|---|---|---|
| P1 | `deck-remove` child-screen explicit owner/export cleanup | `live9`에서 correctness blocker는 없었지만 child-screen에서 transient `captured/enriched` recapture가 남아 있다 |
| P1 | reward card child-screen smoothness cleanup | reward card choice가 `captured/enriched wait` 뒤 observer-first로 회복되며, subtype owner/export를 더 좁힐 여지가 있다 |
| P1 | event reward substate live evidence | `EVENT-05` remains partial |
| P1 | reward-map loop sentinel evidence | `REWARD-10` remains partial |

## Non-Canonical Appendix

아래는 current `main` source-of-truth는 아니지만, 다음 구현 우선순위를 정할 때 참고할 수 있는 recent high-signal branch evidence다.

- low-priority event reward / reward-map sentinel partial coverage

이 appendix는 priority signal일 뿐, canonical coverage status는 위 표의 current `main` evidence만으로 판정한다.
