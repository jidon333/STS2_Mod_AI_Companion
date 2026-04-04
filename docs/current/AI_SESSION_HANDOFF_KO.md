# AI 세션 인수인계 문서 (KO)

> 상태: 현재 사용 중
> 기준 브랜치: `main`
> 최종 갱신: 2026-04-04
> 대상: 새 구현 세션, 새 검증 세션, 새 참모 세션

> 2026-04-04 current pointer:
> 이 문서 본문은 `live21 ancient option` wave 중심의 historical handoff가 많이 남아 있다.
> 현재 active pointer는 [PROJECT_STATUS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/PROJECT_STATUS.md) 를 우선한다.
> 최신 기준으로는 ancient option contract, `ChooseFirstNode <-> event`, post-node combat takeover -> generic map wait, explicit relic claim -> proceed inversion, `rest-site release pending over map overlay`, rest-site click-ready/proceed, accepted-heal release-pending, bounded `Slippery Bridge` family가 닫혔다.
> decisive 2026-04-04 roots:
> - [endurance-longrun-20260404-live28](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live28): fresh live long run이 `player-defeated` natural terminal까지 갔다.
> - [endurance-longrun-20260404-live29](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live29): second fresh live long run도 `player-defeated` natural terminal까지 갔다.
> - [endurance-longrun-20260404-live27](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live27): accepted heal aftermath가 old `rest-site-post-click-noop`로 죽던 마지막 decisive root다.
> current `main`의 latest decisive stabilization wave는 `41d53a0` + `9b50b9c` + `68e91fd` 기준으로 end-turn barrier hold, confirmed hand-card play gating, accepted-heal release-pending canonicalization을 포함한다.
> current active blocker는 없다. fresh live long run 2회 연속 success가 확보됐고, practical milestone position은 M5 완료를 넘어 M6~M8 substrate complete, M9 entry로 읽는 편이 맞다.

## 문서 목적

이 문서는 current `main` 기준으로 새 세션이 바로 이어서 작업할 수 있게 만드는 handoff다.

현재 이 문서가 고정해야 하는 것은 아래 네 가지다.

1. 지금 기준선은 무엇인가
2. 최근에 무엇이 끝났는가
3. 지금 semantic follow-up은 무엇인가
4. 새 세션이 어디부터 열어야 하는가

current handoff answer:

1. current baseline은 `fresh live long run 2회 연속 natural terminal`이다
2. M5 authoritative long-run blocker loop는 complete 상태다
3. M6 replay/parity, M7 non-combat stability, M8 combat stability는 current substrate acceptance 관점에서 함께 닫힌 상태다
4. 새 세션의 기본 시작점은 blocker-fix loop가 아니라 `M9 advice-quality`다. 다만 fresh live에서 새 blocker가 생기면 M5~M8 reopen으로 별도 취급한다

current blocker와 long-term 구조 계획을 섞지 않기 위해, 구조 계약은 별도 문서로 분리해 둔다.

## 반드시 먼저 읽을 것

1. [AGENTS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/AGENTS.md)
2. [PROJECT_STATUS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/PROJECT_STATUS.md)
3. [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
4. [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
5. [HARNESS_REGRESSION_CHECKLIST_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md)
6. [HARNESS_FIXTURE_MATRIX_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md)
7. [M9_EXECUTION_PLAN_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/M9_EXECUTION_PLAN_KO.md)
8. [M9_LIVE_SIDECAR_UI_PLAN_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md)

## 필요할 때만 추가로 읽을 것

아래 문서는 current pointer source가 아니라 `사람이 읽기 쉬운 reader`다.

1. [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/readers/ADVISOR_SCENE_MODEL_READER_KO.md)
2. [HARNESS_TO_M9_STRUCTURE_READER_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md)

## 이번 wave 핵심 가치

어제 리팩터링과 이번 blocker-fix loop에서 다시 고정한 원칙은 아래 다섯 가지다.

1. `explicit truth > broad fallback`
2. `visible/open != canonical foreground owner`
3. observer/export/bridge는 fact transport만 담당하고, owner/release/handoff 판단은 harness가 맡는다.
4. truth가 부족하면 screenshot이나 broad helper를 더하지 말고, export를 늘리거나 `wait`로 남긴다.
5. 더 정확한 path가 들어가면 obsolete fallback은 같은 wave에서 같이 제거한다.
6. final lane은 final actionable surface로만 뒷받침한다. metadata-only lane promotion은 금지하고, lane-vs-surface contradiction이 생기면 plateau가 아니라 explicit contract mismatch로 드러낸다. same-family reconciliation surface가 있을 때만 bounded reconciliation을 허용한다.

anti-drift note:

- long-running blocker-fix loop에서는 AI가 이미 걷어낸 broad helper, mixed-state rescue, screenshot fallback을 다시 되살리는 경향이 있다.
- work unit 하나가 끝날 때마다 `현재 owner가 explicit truth로 결정되는가`, `새 precise path 옆에 옛 fallback이 남아 있지 않은가`를 다시 점검해야 한다.
- decompiled runtime truth와 `AutoSlay`는 code transplant 대상이 아니라 contract reference다.

## canonical baseline

- branch: `main`
- harness entrypoint shell:
  - [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)
- current architecture map:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- current cleanup baseline contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)

핵심 원칙:

```text
visible/open != canonical foreground owner
currentScreen = logical/flow screen
```

semantic fix는 기존 owner 구조를 다시 우회하지 않고 current owner 파일 안에서만 좁게 한다.

## Historical Snapshot (2026-03-31)

새 세션이 가장 먼저 알아야 하는 것은 아래 다섯 가지다.

1. current `main`의 `build`, `self-test`, `replay-test`, `replay-parity-test`는 여전히 green이다.
2. repeated `reward-pick` plateau family는 `15dd5cf` `Prioritize reward-pick child-screen export facts` 이후 [boot-to-long-run-20260330-live18](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live18)에서 재현되지 않았다.
3. combat target-selection / carryover self-test red는 `d9e4c01`, `10028f8`, `ed0a379`, `9c6621b` 이후 green으로 닫혔다.
4. stale `combatTargetSummary` 기반 attack-lane churn/no-op family는 [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21)에서 재현되지 않았다.
5. 이 섹션의 blocker 서술은 2026-03-31 historical snapshot이다. current blocker/current follow-up은 문서 상단 current pointer와 [PROJECT_STATUS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/PROJECT_STATUS.md)를 우선한다.

latest important commits:

1. `15dd5cf` `Prioritize reward-pick child-screen export facts`
2. `d9e4c01` `Make combat barrier self-test use live target ids`
3. `10028f8` `Make combat target-selection self-test live-faithful`
4. `ed0a379` `Wait on unresolved attack-lane churn`
5. `9c6621b` `Narrow combat target-summary carryover`

latest blocker evidence:

1. failure root:
   - [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21)
2. failure summary:
   - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21/attempts/0001/failure-summary.json)
3. trace:
   - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21/attempts/0001/run.log)
4. decisive request:
   - [0101.request.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21/attempts/0001/steps/0101.request.json)

current blocker shape:

```text
phase=HandleEvent
reason=waiting for explicit ancient event option buttons
allowedActions=[click event choice, wait]
observer.actionNodes already contain actionable event-option buttons with bounds
```

핵심 invariant:

```text
ancient residue facts may remain diagnostic,
but the final actionable lane must be backed by the exported actionable surface.
If that surface is only generic event-option buttons, ancient-option metadata may not promote the lane.
If no same-family reconciliation surface remains, fail as explicit contract mismatch instead of plateau masking.
```

즉 문제는:

```text
ancient option 화면을 못 봤다
```

가 아니라

```text
ancient lane이라고 판단한 뒤,
실제 보이는 generic explicit NEventOptionButton을
"ancient explicit option"으로는 인정하지 못해서
decision이 wait로만 떨어진다
```

이다.

이 family의 historical root는 아래다.

1. `96d3db6` `Fix ancient completion release reopen recovery`
2. `b7314e8` `Fix ancient completion release reopen recovery`
3. `57a67a0` `Extract observer routing modules from Program.cs`
4. `94896e9` `Extract GuiSmokeHarness decision contracts and provider verticals`
5. `091b4d7` `Collapse mixed-state guard residue onto canonical owner truth`

즉 어젯밤 새로 만든 코드라기보다, **3월 23일에 들어온 ancient special-case가 extraction 이후에도 그대로 남아 있다가 recent explicit-truth cleanup 뒤에 드러난 것**이다.

## 최근 완료된 work unit

current `main`에는 아래 구조화 커밋이 이미 들어가 있다.

1. `57a67a0` observer routing modules extraction
2. `e702ead` harness support types extraction
3. `347b72e` program shells and self-test suites extraction
4. `5449623` runner bootstrap/deploy split
5. `744f29f` long-run artifacts band split
6. `8875ef2` runtime/replay contracts extraction
7. `94896e9` decision contracts and provider vertical split
8. `3083453` analysis and combat support band extraction
9. `0769383` attempt lifecycle runner isolation
10. `e248960` remaining `Program.cs` helper band split
11. `7628141` large self-test split
12. `9ea2e6d` temporary self-test shim cleanup

요약:

- `Program.cs` monolith 단계는 끝났다
- old wave 1-8 refactor program은 완료됐다
- `GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md`는 cleanup-complete baseline contract로 유지한다
- explicit event / common combat hot path speed recovery도 current `main`에 반영됐다
- `WaitMainMenu -> EnterRun` logo-animation premature acceptance bug도 current `main`에서 닫혔다
- published-first observer provenance migration도 current `main`에 반영됐다
- `compatibility retirement` 1단계도 current `main`에 반영됐다
- combat stale-end-turn / target plateau family는 current `main`에서 micro-stage + quiet convergence + runtime target-summary authority로 닫혔다
- explicit shop foreground 위 stale reward misroute plateau family는 current `main`에서 immediate reward-to-shop recovery로 닫혔다
- `Combat Release + Reward Aftermath` wave live root [combat-release-reward-aftermath-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-release-reward-aftermath-20260401-live1) 는 historical pivot root이고, 그 뒤 2026-04-03 wave에서 rest-site와 bounded `Slippery Bridge` follow-up까지 current pointer가 갱신됐다
- anti-drift recovery wave `bc53c34`, `f29cc5d`, `52ebabd`, `84e4647`, `5ebe718`, `3a24338`도 current `main`에 반영됐다
- `MAP-04` reward reopen self-test regression은 current `main`에서 닫혔다
- `build`, `self-test`, `replay-test`, `replay-parity-test`는 current `main`에서 green이다
- current code/test baseline은 explicit combat/noncombat authority contracts를 다시 고정했고, fresh authoritative live rerun [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)도 확보됐다

## current architecture state

현재 파일 구조는 아래처럼 읽는다.

- shell / CLI:
  - [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)
  - [Program.Cli.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Cli.cs)
  - [Program.InspectAndReplay.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.InspectAndReplay.cs)
- runner:
  - [Program.Runner.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.cs)
  - [Program.Runner.Bootstrap.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.Bootstrap.cs)
  - [Program.Runner.Deploy.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.Deploy.cs)
  - [Program.Runner.AttemptLifecycle.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.AttemptLifecycle.cs)
- observer:
  - [Observer/GuiSmokeObserverPhaseHeuristics.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/GuiSmokeObserverPhaseHeuristics.cs)
  - [Observer/RootSceneTransitionObserverSignals.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/RootSceneTransitionObserverSignals.cs)
  - [Observer/NonCombatForegroundOwnership.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/NonCombatForegroundOwnership.cs)
- decisions:
  - [AutoDecisionProvider.NonCombatSceneState.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs)
  - [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
  - [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
- phase/request glue:
  - [Program.StepRequests.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.StepRequests.cs)
  - [Program.SceneReasoning.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.SceneReasoning.cs)
  - [Program.AllowedActions.NonCombat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
  - [Program.PhaseLoopRouting.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)
- artifacts / supervision:
  - [LongRunArtifacts.Startup.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/LongRunArtifacts.Startup.cs)
  - [LongRunArtifacts.Supervision.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/LongRunArtifacts.Supervision.cs)
  - [LongRunArtifacts.PlateauDiagnostics.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/LongRunArtifacts.PlateauDiagnostics.cs)

## 현재 semantic follow-up

현재 source-of-truth signal은 네 개다.

### 1. replay parity suite

- current baseline: green
- old `reward-aftermath-map-handoff` known red는 current `main`에서 닫혔다

### 2. latest combat blocker fix root

- commits:
  - `577d4ee` `Add combat micro-stage gating for unresolved lanes`
  - `8f70570` `Replace combat delta wake with stage-aware settle observation`
  - `20bb88e` `Recover combat target authority from runtime target summary`
- root: [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)
- startup: [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/startup-summary.json)
- session summary: [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/session-summary.json)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/attempts/0001/run.log)

확정 사실:

- unresolved non-enemy lane는 더 이상 stale observer snapshot 하나로 `auto-end turn`으로 닫히지 않는다
- combat post-action은 generic delta 하나가 아니라 lane micro-stage terminal signal 또는 quiet convergence로 settle된다
- runtime `combatTargetSummary` raw fact가 explicit enemy target authority로 소비된다
- `step=51`에서 `capture skipped reason=combat-explicit-target-runtime`, `action=click`, `target=combat enemy target 의식의-신수 recenter`가 실제로 나왔다
- run은 `attempt-completed:max-steps-reached`로 끝났고 `failure-summary.json`은 생성되지 않았다

### 3. latest cleanup proof root

- root: [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
- startup: [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/startup-summary.json)
- session summary: [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/session-summary.json)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/attempts/0001/run.log)

확정 사실:

- `WaitMainMenu`는 logo-animation / `choiceExtractorPath=generic` bootstrap frame를 그대로 accepted 하지 않는다
- `step=3`에서 actual `Continue` surface가 export된 뒤에만 `EnterRun`이 `target=continue`를 클릭했다
- old `EnterRun` `main menu actions not yet visible` plateau는 재현되지 않는다
- `WaitRunLoad`는 `step=16`에서 `HandleCombat`로 정상 handoff됐다
- `step=23 -> 24`, `step=45 -> 46`, `step=57 -> 58`에서 legitimate combat wait 뒤 다음 capture/request로 이어져 post-wait recapture continuity가 fresh live로 추가 확인됐다
- run은 `max-steps-reached:60`까지 진행했고 `failure-summary.json`은 생성되지 않았다

### 3-1. latest compatibility retirement proof root

- commit: `0c26301` `Retire compatibility shadow from observer primary flow`
- root: [compat-primary-flow-retirement-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/compat-primary-flow-retirement-20260329-live2)
- startup: [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/compat-primary-flow-retirement-20260329-live2/startup-summary.json)
- session summary: [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/compat-primary-flow-retirement-20260329-live2/session-summary.json)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/compat-primary-flow-retirement-20260329-live2/attempts/0001/run.log)

확정 사실:

- `ObserverSnapshotReader`와 `InventoryPublisher`의 primary flow는 compat shadow를 다시 먹지 않게 정리됐다
- `EnterRun` 조기 클릭 regression root였던 `compat-primary-flow-retirement-20260329-live1`의 collapsed main-menu action layout 문제를 좁게 닫았다
- `step=3`에서 distinct bounds의 `Continue` surface가 export된 뒤에만 `target=continue` 클릭이 나왔다
- `step=16`에서 `WaitRunLoad -> HandleCombat from screen=combat`가 정상적으로 열렸다
- run은 `max-steps-reached:60`까지 진행했고 `failure-summary.json`은 생성되지 않았다

### 3-2. latest shop recovery proof root

- commit: `835becb` `Recover explicit shop authority from stale reward misrouting`
- root: [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4/attempts/0001/run.log)

확정 사실:

- old `HandleShop -> HandleRewards from screen=shop -> decision-wait-plateau phase=HandleRewards screen=shop` family는 current `main`에서 plateau 없이 회복된다
- `step=22 -> 24`, `step=42 -> 43`에서 transient misroute는 보이지만 곧바로 `HandleRewards -> HandleShop` alternate branch가 일어난다
- 이후 open-inventory / buy / back / proceed가 실제로 이어지고, run은 combat / reward / rest-site까지 계속 진행한 뒤 `max-steps-reached:120`으로 끝난다
- `failure-summary.json`은 생성되지 않았다

### 4. latest speed proof root

- root: [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- startup: [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/startup-summary.json)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/run.log)
- terminal summary: [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/failure-summary.json)

확정 사실:

- 이 root는 semantic blocker root가 아니라 speed/capture baseline proof다
- explicit event lane `step=8~9`는 `captureMode=skipped`, `sceneReasoningMode=observer-only`
- common combat chain `step=11~17`도 `captureMode=skipped`, `sceneReasoningMode=observer-only`
- representative `preflight->request`는 `1155~1466ms` band다
- attempt는 `returned-main-menu`로 끝났지만, speed 관점에서는 captured hot path가 0건이라는 점이 핵심이다

### 5. latest post-node continuity coverage proof

- commit: `c85b75f` `Strengthen post-node reward handoff coverage`
- self-test owner:
  - [Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs)
- representative current-main live roots:
  - [observer-first-combat-speed-20260328-live6](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-combat-speed-20260328-live6)
  - [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1)

확정 사실:

- `WaitPostMapNodeRoom`는 current `main`에서 reward/event/combat/rest/shop destination handoff를 모두 self-test로 고정한다
- representative current-main live roots에서도 `WaitPostMapNodeRoom -> ChooseFirstNode` 뒤 reward/event/rest/combat continuity가 남아 있다
- `MAP-04`는 current matrix 기준 `green`이다

## 다음 세션의 기본 목표

현재 기준으로 immediate priority는 coverage frontier보다 **`live21` ancient event option blocker closure**다.

```text
1. decompiled runtime truth + AutoSlay contract로 ancient event option phase를 먼저 다시 읽는다
2. exporter가 ancient option lane fact와 generic explicit `NEventOptionButton` fact를 additive하게 내는지 확인한다
3. harness는 dialogue/completion만 ancient 특화 유지하고, option phase는 generic explicit event-option button contract를 먼저 믿게 정리한다
4. self-test / replay / parity를 다시 잠근 뒤 clean deploy / Manual Clean Boot / identity verify 후 fresh long-run live root를 다시 만든다
```

현재 바로 열 파일:

1. [RuntimeSnapshotReflectionExtractor.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs)
2. [AutoDecisionProvider.NonCombatSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSupport.cs)
3. [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
4. [Program.Runner.AttemptDecisionFlow.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.AttemptDecisionFlow.cs)
5. [Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.SelfTests.NonCombatDecisionContracts.SubtypesAndEvents.cs)
6. [EventRoomHandler.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/knowledge/decompiled/MegaCrit/sts2/Core/AutoSlay/Handlers/Rooms/EventRoomHandler.cs)

## validation baseline

모든 semantic 작업은 기본적으로 아래를 유지해야 한다.

```text
cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln
cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test
```

현재 known red:

```text
none
```

replay parity acceptance:

- green 유지
- 새 failing fixture 추가 금지

current live baseline note:

- `live8`는 combat false-confirm + speed regression을 드러낸 authoritative regression root였다
- `84e4647`, `5ebe718`, `3a24338` 이후 fresh live root [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)에서 그 plateau는 재현되지 않았다
- `15dd5cf`, `10028f8`, `ed0a379`, `9c6621b` 이후 reward-pick repeated plateau와 stale combat carryover family는 재현되지 않았다
- historical authoritative blocker at that time was [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21)의 `HandleEvent` ancient option wait plateau다. current pointer는 문서 상단 2026-04-04 current pointer와 [PROJECT_STATUS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/PROJECT_STATUS.md)를 우선한다

## 절대 reopen하지 말 것

1. refactor wave 1-8 자체
2. `WaitRunLoad` resumed room handoff bug
3. `Program.cs` monolith 시절 전제
4. `visible/open == foreground owner` 해석
5. 구조화와 semantic fix를 같은 commit에 섞는 방식

## 한 줄 요약

```text
current main의 하네스 구조 정리와 anti-drift recovery wave는 current code/test baseline에서 닫혔다.
새 세션은 old Program.cs monolith, screenshot-first recovery, broad mixed-state fallback을 다시 가져오지 말고,
먼저 `live21` ancient event option blocker를 decompiled truth와 AutoSlay 기준으로 닫아라.
핵심은 dialogue/completion만 ancient 특화로 남기고, option phase는 generic explicit `NEventOptionButton` contract를 먼저 믿게 정리하는 것이다.
그 다음 clean deploy / Manual Clean Boot 뒤 fresh authoritative live root를 다시 만들고,
그 다음 first blocker 1개만 decompiled truth와 AutoSlay contract 기준으로 다뤄라.
```
