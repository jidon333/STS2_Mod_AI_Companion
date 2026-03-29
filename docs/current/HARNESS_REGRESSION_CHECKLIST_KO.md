# Harness Regression Checklist

> Status: Active checklist
> Source of truth: Yes, for harness state/action regression coverage on current `main`
> Update when: a state contract, owner rule, release/handoff rule, or canonical evidence changes

## 목적

이 문서는 `Sts2GuiSmokeHarness`의 상태별 행동 계약을 현재 `main` 기준으로 고정한다.

핵심 목표는 아래 세 가지다.

1. 같은 상태를 여러 helper가 제각각 해석하지 못하게 한다.
2. 각 상태의 `owner -> action -> release -> handoff`를 명시한다.
3. mixed-state 수정 시 이전 경로를 삭제해야 하는 범위를 체크리스트로 강제한다.

이 문서의 기준선은 현재 `main`이다. clean validation branch와 외부 worktree evidence는 source of truth가 아니라 참고 appendix로만 본다.

구현 owner note:

- 이 문서의 state row는 authoritative다.
- 본문 아래의 `Current evidence`는 current owner file 또는 current self-test family를 가리킨다.
- current owner file map은 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)를 기준으로 보고, cleanup/deletion 우선순위는 [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](../contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)를 따른다.

## 공통 계약

### 공통 용어

- `authority`: 현재 상태를 확정하는 raw observer/runtime truth
- `canonical foreground owner`: 지금 행동 우선권을 가진 상태 주체. `currentScreen`이나 `visibleScreen`과 동일어가 아니다.
- `foreground action lane`: 현재 owner가 열어야 하는 explicit action family
- `release`: 방금 action을 눌렀을 때 owner를 유지할지 내려놓을지 결정하는 조건
- `handoff`: release 후 다음 상태가 owner를 가져가는 단계
- `mixed residue`: 배경이나 stale open/visible 흔적으로 남아 있으나 canonical foreground owner는 아닌 신호
- `reissue suppression`: 같은 authority band에서 같은 action을 다시 누르지 않게 막는 규칙
- `bounded failure`: 복구가 안 될 때 침묵하지 않고 명시적으로 실패를 기록하는 규칙

### 최상위 규칙

1. active-screen truth > explicit room/screen authority > explicit action surface > residue
2. `visible/open != canonical foreground owner`
3. noncombat는 `action fired -> release pending -> handoff` 계약을 반드시 가진다
4. 같은 authority band에서 같은 action 재발행 금지
5. raw observer/runtime truth가 heuristic보다 우선한다

### 구현 적용 규칙

- mixed-state 버그를 고칠 때는 새 helper를 추가하기 전에 기존 owner/action/release 판단 경로를 찾아 같이 제거하거나 wrapper화한다
- `GetAllowedActions`, `Decide*`, `Analyze*`, `GetPost*Phase`, loop sentinel이 같은 상태 계약을 공유해야 한다
- self-test / replay / live root 중 무엇으로 고정했는지 반드시 문서와 매트릭스에 남긴다

## Startup / Validation

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `VAL-01` Deploy identity valid | deploy exit code 정상, DLL timestamp/identity 일치, stale mod payload 부재 | Validation | rebuild, redeploy, verify identity | gameplay debugging before identity check | valid identity 확보 후 startup gate로 handoff | stale deploy, partial copy는 invalid root로 강등 | [SMOKE_TEST_CHECKLIST.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/runbooks/SMOKE_TEST_CHECKLIST.md), [AGENTS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/AGENTS.md) |
| `VAL-02` Manual clean boot | Steam URI clean boot, no stale token, no stale `actions.ndjson`, no auto-progress | Validation | launch via Steam URI, observe boot, reject invalid preconditions | trusting harness results before clean boot | `manualCleanBootVerified=true` 후 authoritative attempt로 handoff | bootstrap-only chronology는 gameplay evidence가 아님 | [AGENTS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/AGENTS.md), [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `VAL-03` Valid trusted attempt | `manualCleanBootVerified=true`, `firstAttemptCreated=true`, trusted attempt started | Validation | create attempt, collect startup summary | gameplay blocker 해석 before valid attempt | valid attempt 생성 후 main menu / resumed run routing | invalid root는 gameplay blocker로 분류 금지 | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `VAL-04` Capture bounded failure | capture timeout / exception / unusable capture가 explicit failure로 남음 | Validation | capture, timeout classification, failure summary write | silent stall after phase log | capture 성공 시 next step, 실패 시 bounded failure | transition wait는 허용되나 무기한 침묵은 금지 | `Program.Runner.AttemptLifecycle.cs`, `Program.SelfTests.CaptureReplay.cs` |
| `VAL-05` Invalid root classification | startup/deploy/boot mismatch, terminal boundary before gameplay, launch mismatch | Validation | classify invalid, stop blocker analysis | treating invalid root as gameplay regression | explicit invalid classification 후 stop | invalid root는 evidence matrix에서 non-canonical appendix로만 유지 | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |

## Main Menu / EnterRun / WaitRunLoad

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `MENU-01` Stable main menu continue | `currentScreen=main-menu` or `visibleScreen=main-menu`, continue visible/actionable | Main menu / continue | `click continue`, `click singleplayer`, `wait` | treating stable continue as run-load transition | `continue` click -> `WaitRunLoad`, `singleplayer` -> character path | stale menu residue does not override explicit run screen | `Program.Cli.cs`, `Program.SelfTests.CliStartup.cs` |
| `MENU-02` Singleplayer submenu | `currentScreen=singleplayer-submenu` or submenu authority | Main menu / submenu | normal mode, character routing | continue retry while submenu already foreground | submenu selection -> character select / embark | background main-menu labels are residue | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `MENU-03` Character select | `currentScreen=character-select` or post-submenu authority | Character select | `click ironclad`, `click character confirm`, `wait` | continue or main-menu action reuse | confirm -> embark | lingering menu elements are residue | `Program.SelfTests.CliStartup.cs`, `Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs` |
| `MENU-04` Embark | embark authority or post-character flow | Embark | `click embark`, room-specific reopen if observer already moved | waiting on embark when room authority already explicit | room authority -> `HandleEvent` / `ChooseFirstNode` / other reconciled phase | canonical foreground owner beats stale embark | `Observer/GuiSmokeObserverPhaseHeuristics.cs`, `Program.SelfTests.PhaseRouting.EmbarkWaits.cs` |
| `LOAD-01` WaitRunLoad transition hold | explicit transition truth, root-scene transition in progress, run node not ready | WaitRunLoad / passive wait | `wait` only | continue retry while transition still active | release to post-run-load room phase when room/run authority explicit | transient polled scene is not scene-ready by itself | `Observer/RootSceneTransitionObserverSignals.cs`, `Program.SelfTests.PhaseRouting.RunLoadRecovery.cs` |
| `LOAD-02` WaitRunLoad stale continue recovery | stable main menu, no transition evidence, continue still visible | Main menu / continue retry | reopen EnterRun actions, retry continue | passive wait-only plateau | retry continue -> `WaitRunLoad` again | terminal main-menu without continue is not recoverable continue | `Observer/WaitRunLoadRecoverySignals.cs`, `Program.SelfTests.PhaseRouting.RunLoadRecovery.cs` |
| `LOAD-03` WaitRunLoad room branch | resumed run already in reward/event/combat/treasure/map | Room owner / room lane | branch to room-specific handler | idling in WaitRunLoad after room authority explicit | handoff to room handler immediately | background menu residue ignored once run authority explicit | `Observer/GuiSmokeObserverPhaseHeuristics.cs`, `Program.SelfTests.PhaseRouting.RunLoadRecovery.cs` |

## Map / WaitMap / PostNode

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `MAP-01` Pure map foreground | explicit map current active screen, `activeScreenType=NMapScreen`, reachable map node | Map / map-node | exported reachable node, visible reachable node, map back, wait | event/reward/shop action reuse | node click -> `WaitPostMapNodeRoom` or `WaitMap` | stale room labels are residue | `Program.AllowedActions.NonCombat.cs`, `Observer/MapForegroundReconciliation.cs` |
| `MAP-02` Map overlay foreground over stale room | map overlay visible, stale event choice/reward residue, current-node arrow or reachable node | Map / map-overlay | exported reachable node, map back, wait | stale event proceed, stale reward skip | release only when room overlay closes or node chosen | stale room choice marked as contamination, not owner | `Observer/GuiSmokeMapOverlayHeuristics.cs`, `Program.SelfTests.NonCombatForegroundOwnership.cs` |
| `MAP-03` WaitMap reopen to room owner | map visible but explicit reward/event/rest owner persists | room owner / room lane | reopen correct handler instead of node click | map action while explicit room affordance still active | release only after room affordance disappears | map background is residue while room owner explicit | `Program.PhaseLoopRouting.cs`, `Program.SelfTests.NonCombatForegroundOwnership.cs` |
| `MAP-04` WaitPostMapNodeRoom room reconciliation | post-node room authority not yet settled | WaitPostMapNodeRoom / passive room reconcile | wait, branch when room authority explicit | forcing combat or room action from stale map frame | release to room handler when event/reward/combat/rest/shop authority explicit | transient room flashes do not override explicit room authority | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |

## Combat

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `COMBAT-01` Player turn open | `inCombat=true`, `IsPlayPhase=true`, `IsEnemyTurnStarted=false`, player actions enabled | Combat / play lane | select card, target enemy, end turn, cancel selection | map/event/reward fallback | release on card selection, targeting, or end turn ack | background map contamination suppressed | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `COMBAT-02` Card selection | selected-card truth or explicit attack slot action | Combat / select-card | legal card select hotkey or click | direct target click before legal selection | release to targeting or resolve no-target action | screenshot-only slot drift is disallowed | `AutoDecisionProvider.CombatDecisions.cs`, `Program.SelfTests.CombatContracts.*.cs` |
| `COMBAT-03` Target selection | explicit enemy target nodes/body rects, selected card requires target | Combat / target | click exported enemy target | blind fixed anchor click, end turn while target unresolved | resolve action or cancel selection | target bounds beat screenshot anchor | `Analysis/CombatTargetabilitySupport.cs`, `Program.SelfTests.CombatContracts.TargetSelection.cs` |
| `COMBAT-04` Blocked-open selection cancel | selected card unresolved, action window not advancing, cancel lane explicit | Combat / cancel-selection | right-click cancel selected card | auto-end turn fallback while unresolved selection remains | cancel -> reopen combat lane | stale selected card must be cleared before end turn | `AutoDecisionProvider.CombatDecisions.cs`, `Program.SelfTests.CombatContracts.NoOpAndBlockedLane.cs` |
| `COMBAT-05` End turn pre-ack | end turn fired, no turn-transition acknowledgement yet | Combat / end-turn barrier | wait only | reopen player action, target, or map fallback | release to acknowledged transit or bounded failure | no timer-based actuation | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md), [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) |
| `COMBAT-06` End turn acknowledged transit | transition ack present, enemy-turn/closed-play-phase band in progress | Combat / transit wait | wait only | premature plateau classification, reissuing end turn | release on next-round reopen, or bounded failure if frozen | authoritative turn progress must suppress plateau | `Program.PhaseLoopRouting.cs`, `Program.SelfTests.CombatContracts.ParityAndBarriers.cs` |
| `COMBAT-07` Enemy turn closed play phase | `IsPlayPhase=false`, `IsEnemyTurnStarted=true` or closed action window | Combat / passive wait | wait only | act during enemy turn | recapture to next combat step | legitimate wait is not a blocker | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `COMBAT-08` Next-round reopen | `currentRound > armedRound` and reopened player action window | Combat / play lane | new combat actions | reactivating old end-turn barrier | release complete, new lane begins | same old source must not re-block reopened turn | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `COMBAT-09` Post-wait recapture | legitimate wait with `requiresRecapture=true` | Combat / recapture | capture next step, or explicit failure if capture hangs | silent hang after phase log | next step capture or bounded failure summary | phase log without capture/failure is forbidden | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `COMBAT-10` Combat resolved -> rewards | combat room clears and reward authority emerges | Reward / reward lane | branch to `HandleRewards` | staying in combat policy on rewards | handoff when reward owner explicit | lingering combat labels are residue | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |

## Event / Ancient / Treasure / Rest

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `EVENT-01` Explicit event option | `currentScreen=event`, event option buttons visible, no stronger map owner | Event / explicit option | click explicit event choice | map fallback while explicit event option foreground active | option click -> stay in event or handoff per explicit room state | map arrows in background are contamination only | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.EventRewardSubstates.cs` |
| `EVENT-02` Semantic event choice | ambiguous event screen without explicit option metadata | Event / semantic option | semantic event option, then explicit choice fallback | map routing while canonical event owner remains stronger | option click -> event or room transition | semantic reasoning only while ownership ambiguous | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.EventRewardSubstates.cs` |
| `EVENT-03` Explicit event proceed | `eventProceed` semantic exported, `EventOption.IsProceed` lane present | Event / proceed | click explicit proceed | map-only routing while event proceed still foreground active | proceed -> event release or room handoff | stale map overlay does not override explicit proceed | `Observer/EventProceedObserverSignals.cs`, `Program.SelfTests.EventRewardSubstates.cs` |
| `EVENT-04` Ancient dialogue / completion | ancient dialogue state active or explicit ancient completion visible | Event / ancient lane | ancient dialogue progression, ancient completion, then `WaitEventRelease` | generic proceed, map node click before release | ancient completion -> `WaitEventRelease` -> map | map current active screen does not win until ancient lane disappears | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md), `Observer/AncientEventObserverSignals.cs`, `Program.SelfTests.EventRewardSubstates.cs` |
| `EVENT-05` Event reward substate | event screen but reward owner/action lane explicit | Reward / reward lane | reward choice, reward skip/proceed | generic event option while reward substate foreground active | release to event or map after reward teardown | event text becomes background residue | `AutoDecisionProvider.NonCombatSceneState.cs`, `Program.SelfTests.EventRewardSubstates.cs` |
| `EVENT-06` Event -> map mixed aftermath | map explicit canonical foreground owner, stale event residue remains | Map / map-node | exported reachable node, map back, wait | stale event proceed or generic event choice | handoff to map until room changes | lingering event proceed is residue when map owner explicit | `Observer/NonCombatForegroundOwnership.cs`, `Program.SelfTests.NonCombatForegroundOwnership.cs` |
| `TREASURE-01` Treasure room chest/relic/proceed | treasure room authority active | Treasure / treasure lane | chest, relic holder, treasure proceed | map routing while treasure owner active | treasure proceed -> map handoff | background map ignored until treasure lane completes | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `REST-01` Explicit rest-site choice | rest-site option metadata or explicit rest choices visible | RestSite / explicit choice | rest, smith, hatch | map routing while rest choice foreground active | option click -> smith grid or rest proceed | map overlay is residue until choice resolves | `Observer/RestSiteObserverSignals.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |
| `REST-02` Smith grid / confirm | smith upgrade grid or confirm visible | RestSite / smith lane | click smith card, click smith confirm | map routing or generic proceed before confirm | confirm -> rest-site release/map | stale rest choices secondary while smith grid foreground active | `Observer/RestSiteChoiceSupport.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |

## Rewards

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `REWARD-01` Explicit reward item claim | reward foreground active, explicit reward node/choice visible | Rewards / claim | claim reward item before proceed/map | map fallback or reward skip before claimable item if claim exists | claim -> remain in `HandleRewards` while more reward affordance exists | map background may be visible but cannot steal owner | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` |
| `REWARD-02` Reward card / colorless | reward card row or colorless reward choice state visible | Rewards / card-choice | reward card choice, colorless card choice, reward skip/proceed, back when explicit | visible map advance while reward card visible | card pick -> remain in reward or move to next explicit reward state | inspect previews are not claim actions | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` |
| `REWARD-03` Reward skip/proceed | explicit reward proceed/skip visible, no higher-priority claim item | Rewards / skip-proceed | reward skip or proceed | map fallback before reward affordance disappears | skip/proceed -> release pending then map handoff | map background visible is residue while skip/proceed still foreground active | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` |
| `REWARD-04` Reward back navigation | explicit reward back visible and reward panel still active | Rewards / back | reward back | map node click before back resolves | back -> remain in reward or release to map based on state | back is reward lane, not map lane | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` |
| `REWARD-05` Reward teardown aftermath | reward foreground dropped, map current or teardown flag active | WaitMap / passive release | wait only | reopening reward claim/skip, direct map fallback before release completes | handoff through `WaitMap` | stale reward bounds are residue and must be hard-rejected | `AutoDecisionProvider.NonCombatSceneState.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` |
| `REWARD-06` Reward/map mixed foreground | reward explicit affordance present, map context visible underneath | Rewards / reward lane | claim reward item or skip/proceed; map actions suppressed | visible map advance, first reachable node while reward affordance remains | release only after reward affordance disappears or reward owner drops | background map is contamination, not owner | `AutoDecisionProvider.NonCombatSceneState.cs`, `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs`, `Program.SelfTests.NonCombatForegroundOwnership.cs` |
| `REWARD-07` Reward -> map handoff | reward affordance gone, map current or teardown explicit | WaitMap / map reconcile | wait, then map reopen | repeated reward skip or direct node click before release settles | `WaitMap` -> map routing | stale off-window reward only does not keep reward owner | `Program.PhaseLoopRouting.cs`, `Program.SelfTests.NonCombatForegroundOwnership.cs` |
| `REWARD-09` Same reward skip reissue suppression | prior reward skip fired and same reward authority band lingers | Rewards / release-pending | wait/release only until new authority appears | same reward skip click reissue | release to map or explicit new reward affordance | same-action reissue without new authority forbidden | `Program.SelfTests.NonCombatDecisionContracts.RewardContracts.cs` reward release-pending assertions |
| `REWARD-10` Reward-map loop detection | repeated reward/map loop targets on reward-authoritative screen | Sentinel / bounded failure | classify reward-map-loop after bounded repeats | infinite click loop | bounded failure summary or recovery branch | reward explicit choices + map contamination must be recorded | `Program.PhaseFailureHints.cs`, `Program.SelfTests.StallSentinel.cs` |

## Shop

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `SHOP-01` Inventory closed | shop authority active, merchant button enabled, inventory closed | Shop / open-inventory | open merchant inventory | map routing while shop foreground active | inventory open -> purchase lane | map residue ignored while shop owner active | `Observer/ShopObserverSignals.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |
| `SHOP-02` Bounded purchase | inventory open and affordable relic/card/potion exists | Shop / buy | buy one affordable item | map routing or proceed before purchase/back resolution | purchase -> stay in shop until back/proceed | shop foreground suppresses map routing | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md) |
| `SHOP-03` Card removal | card removal enabled | Shop / removal | explicit shop removal action | treating removal as normal card buy | removal -> stay in shop, then back/proceed | shop owner remains active | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |
| `SHOP-04` Back then proceed | no further bounded purchase, back or proceed visible | Shop / back-proceed | back, then proceed | reopening inventory after bounded purchase when proceed enabled | proceed -> map handoff | stale merchant visuals do not keep foreground after teardown | `AutoDecisionProvider.NonCombatDecisions.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |
| `SHOP-05` Shop -> map handoff | shop teardown or proceed complete | WaitMap / map reconcile | wait, then reopen map | shop action reuse after teardown | handoff to map only after shop owner drops | stale visible shop remnant is residue | `Program.PhaseLoopRouting.cs`, `Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs` |

## Terminal / Restart / Lifecycle Appendix

| State ID | Authority | Foreground owner / lane | Allowed actions | Forbidden actions | Release / handoff | Mixed residue rule | Current evidence |
|---|---|---|---|---|---|---|---|
| `TERM-01` Terminal run boundary | reward/game-over/unlock/timeline/main-menu return boundary | Terminal / passive | wait, classify terminal | reopening gameplay map fallback or reward actions | handoff to restart/menu handling only | gameplay residue must not override terminal boundary | `Observer/RewardObserverSignals.cs`, `Program.SelfTests.LongRunSupervisor.cs` |
| `TERM-02` Restart chronology | authoritative restart events and next-attempt-started chronology | Validation lifecycle | record restart, next attempt | trusting bootstrap-only session as gameplay restart | next attempt first screen evidence | lifecycle evidence is separate from gameplay blocker analysis | [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md), `LongRunArtifacts.Startup.cs`, `Program.SelfTests.StartupRuntimeEvidence.cs` |

## Immediate Gap Inventory

| Priority | Family | Current contract status | Main risk | Required follow-up |
|---|---|---|---|---|
| P0 | Lifecycle restart evidence | product path separate from gameplay | terminal -> restart -> next-attempt first-screen chain remains appendix | validation/lifecycle appendix로 유지하며 별도 추적 |

## 문서 사용 규칙

1. mixed-state 버그를 고치기 전에 해당 state row를 먼저 찾는다.
2. 새 helper를 추가하기 전에 같은 의미를 가진 기존 helper row와 evidence를 확인한다.
3. 수정 후에는 `HARNESS_FIXTURE_MATRIX_KO.md`에서 해당 row를 `green` 또는 `partial`로 갱신한다.
4. 같은 action을 두 번 누르게 만드는 수정은 `reissue suppression` row를 먼저 추가하거나 수정한 뒤에만 허용한다.

## Non-Canonical Appendix

current `main` 바깥의 clean validation branch/worktree에서 드러난 최근 regression families는 source of truth가 아니므로 본문 기준선에는 넣지 않는다. 다만 아래 family는 이후 current `main`에 같은 shape가 들어오는지 감시 대상으로 유지한다.

- strict lifecycle chain proof gap
