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
| `VAL-CAPTURE-BOUNDARY` | phase log 이후 capture/request/failure emission을 bounded contract로 고정 | partial |
| `TERM-LIFECYCLE-CHAIN` | terminal -> restart -> next-attempt first-screen evidence | partial |
| `REWARD-AFTERMATH-MAP-HANDOFF` | reward aftermath 이후 map-node continuity, `ChooseFirstNode` / parity alignment | green |

## Validation / Boot / Menu

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `VAL-01` | validation | deploy/runtime identity | green | [SMOKE_TEST_CHECKLIST.md](../runbooks/SMOKE_TEST_CHECKLIST.md), [PROJECT_STATUS.md](./PROJECT_STATUS.md) startup/trust roots | current `main` 운영 규칙 고정 |
| `VAL-02` | validation | manual-clean-boot | green | [ROADMAP.md](../ROADMAP.md) M4, [PROJECT_STATUS.md](./PROJECT_STATUS.md) bootstrap/trust roots | trust gate는 current baseline에 포함 |
| `VAL-03` | any phase post-log | capture boundary | partial | `Program.SelfTests.CaptureReplay.cs` capture boundary assertions and bounded failure emission now exist on current `main`; fresh cleanup-complete live proof is still pending | silent-hang hardening live evidence still needed |
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
| `MAP-04` | `WaitPostMapNodeRoom` | destination room handoff | partial | goal text + phase reconciliation helpers + self-tests around event/combat reopen | reward aftermath continuity와 함께 fresh live closure 필요 |

## Combat

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `COMBAT-01` | `WaitCombat` | combat acceptance | green | `Program.SelfTests.CombatContracts.*.cs`의 WaitCombat acceptance assertions (`ready`, `stable`, `inCombat`) | strong acceptance gate |
| `COMBAT-02` | `HandleCombat` | card select | green | combat opener self-test (`combat select attack slot 1`) + speed proof root [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) | common combat opener is now observer-first (`captureMode=skipped`) |
| `COMBAT-03` | `HandleCombat` | target lane | partial | `Analysis/CombatTargetabilitySupport.cs`, `Program.SelfTests.CombatContracts.TargetSelection.cs`, speed proof root [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) | explicit target lane has fresh observer-first live proof, but broader target parity matrix is still thin |
| `COMBAT-04` | `HandleCombat` | cancel blocked selection | green | old blocker replay `0167.request.json` family, commit-era closure evidence referenced in current conversation | explicit cancel lane is required contract |
| `COMBAT-05` | `HandleCombat` | end-turn pre-ack | green | current code barriers + self-tests + fresh live root [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) | `auto-end turn` now survives pre-actuation drift and sends `key=E` |
| `COMBAT-06` | `HandleCombat` | acknowledged transit wait | green | fresh live root [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) plus `Program.SelfTests.CombatContracts.ParityAndBarriers.cs` | barrier reason now reaches acknowledged transit and waits until next-round reopen |
| `COMBAT-07` | `HandleCombat` | enemy-turn closed play phase | partial | legitimate wait semantics exist in current code and self-tests, but current-main replay/live evidence is thin | keep wait semantics explicit |
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
| `REWARD-01` | `HandleRewards` | claim lane | green | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` reward fast-path signature and `claim reward item` assertions | claim keeps the reward lane ahead of skip/proceed and map routing |
| `REWARD-02` | `HandleRewards` | reward card / colorless | green | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` colorless and reward choice assertions | inspect overlay + card choice covered |
| `REWARD-03` | `HandleRewards` | reward skip / proceed | green | canonical reward release contract + current self-tests | same-click reissue suppression and release-pending are covered |
| `REWARD-04` | `HandleRewards` | reward back | partial | allowlist + analysis candidate path exist; canonical replay/live evidence thin | lower priority but incomplete |
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
| `SHOP-03` | `HandleShop` | back / proceed / map handoff | green | self-tests around proceed-before-reopen, status doc shop->map continuity | stable on current baseline |

## Terminal / Restart

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `TERM-01` | reward/menu terminal boundaries | terminal boundary | green | `RewardObserverSignals.IsTerminalRunBoundary`, terminal suppression tests, [PROJECT_STATUS.md](./PROJECT_STATUS.md) | gameplay map fallback suppression covered |
| `TERM-02` | terminal -> restart -> next attempt | lifecycle lane | missing | [PROJECT_STATUS.md](./PROJECT_STATUS.md), [PROJECT_STATUS_READER_KO.md](./PROJECT_STATUS_READER_KO.md) both call this residual gap out explicitly | strict lifecycle chain still open |

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
| P0 | capture boundary fresh live proof | `VAL-03` remains partial even though bounded failure emission and self-tests exist |
| P1 | strict lifecycle chain evidence | `TERM-02` remains intentionally open |

## Non-Canonical Appendix

아래는 current `main` source-of-truth는 아니지만, 다음 구현 우선순위를 정할 때 참고할 수 있는 recent high-signal branch evidence다.

- capture boundary fresh live proof gap

이 appendix는 priority signal일 뿐, canonical coverage status는 위 표의 current `main` evidence만으로 판정한다.
