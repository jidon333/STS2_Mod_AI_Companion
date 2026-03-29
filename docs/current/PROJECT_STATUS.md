# 프로젝트 현황

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 현재 milestone pointer, current semantic blocker, representative evidence, 또는 harness architecture state가 바뀔 때

## 날짜

- 2026-03-30

## 현재 마일스톤 위치

- 현재 진행 축: `M7 non-combat stability` + `M8 combat stability` + `M9 observer-first speed recovery` + `startup main-menu phase-boundary` + `published-first observer provenance migration` + post-refactor cleanup completion
- 현재 engineering focus:
  1. `Sts2GuiSmokeHarness` cleanup-complete 기준선과 combat micro-stage baseline을 current docs에 고정
  2. explicit event / common combat hot path를 `observer-first, screenshot-on-demand`로 되돌린 speed baseline 유지
  3. unrelated known self-test red (`WaitPostMapNodeRoom -> reward reopen`)를 current owner 구조 안에서 좁게 닫기
- 장기 제품 목표: 사람이 실제 플레이 중 참고하는 `읽기 전용 advisor`

중요한 현재 해석:

- startup / trust / bootstrap / deploy identity는 broad top blocker가 아니다
- `WaitRunLoad` resumed room handoff bug는 이미 닫혔다
- `WaitMainMenu -> EnterRun` logo-animation premature acceptance bug는 current `main`에서 닫혔다
- published-first observer provenance migration은 current `main`에서 active다
- post-refactor cleanup program은 current `main`에서 완료 상태다
- combat stale-end-turn / target plateau family는 current `main`에서 micro-stage + quiet convergence로 닫혔다
- 현재 핵심은 **combat blocker 재발 방지 baseline을 유지하면서 unrelated noncombat self-test red와 남은 coverage gap만 좁게 다루는 것**이다

## 현재 우선순위

### 1. 하네스 구조 기준선

current `main`의 하네스 구조 정리 wave 1-8은 완료됐다.

핵심 결과:

- [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)는 shell-only다
- observer / analysis / decisions / runner / artifacts / self-test가 물리적으로 분리됐다
- `LongRunArtifacts` band 분리, `AutoDecisionProvider` vertical 분리, runner seam 분리, large self-test split이 완료됐다
- 장기 cleanup 기준 문서는 [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)다
- current file owner reference는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)다

### 2. 현재 semantic gap

cleanup program 완료 이후에도 current follow-up은 남아 있다.

현재 가장 중요한 세 signal은 아래다.

1. replay parity suite
   - status: green
   - 해석: old `reward-aftermath-map-handoff` known red는 current `main`에서 닫혔다
2. latest combat blocker fix root
   - root: [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)
   - result: original `combat select non-enemy slot -> stale observer -> auto-end turn -> combat-barrier-wait-plateau` family와 후속 attack-target plateau family가 모두 재현되지 않았다
   - shape:
     - unresolved non-enemy lane는 `end turn`으로 닫히지 않고 lane settle/confirm으로만 진행됐다
     - `step=51`에서 `capture skipped reason=combat-explicit-target-runtime`, `action=click`, `target=combat enemy target 의식의-신수 recenter`
     - `step=51` 이후 settle reason은 generic observer delta가 아니라 `combat-enemy-click-resolved`였다
     - `step=55 -> 60`에서는 unresolved selected lane이 `right-click cancel unresolved selected card`로만 해소됐다
     - run은 `attempt-completed:max-steps-reached`로 끝났고 `failure-summary.json`은 생성되지 않았다
3. latest cleanup proof root
   - root: [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
   - result: representative fresh live root, cleanup-complete `main`에서 `WaitMainMenu -> EnterRun`, `WaitRunLoad -> HandleCombat`, combat wait/re-capture continuity가 모두 비회귀였다
   - shape:
     - `step=3`에서 `captureMode=skipped`, `sceneReasoningMode=observer-only`, `choices=[계속, 멀티플레이, 종료, 설정]` 뒤 `target=continue`
     - `step=16`에서 `WaitRunLoad -> HandleCombat`
     - `step=23 -> 24`, `step=45 -> 46`, `step=57 -> 58`에서 legitimate combat wait 뒤 다음 capture/request로 이어졌고 silent stall은 없었다
     - run은 `max-steps-reached:60`로 종료됐고 `failure-summary.json`은 생성되지 않았다

즉 현재 질문은 더 이상

```text
"reward aftermath 이후 ChooseFirstNode / WaitMap / map-node routing이 막힌다"
```

가 아니다.

현재 authoritative frontier는

```text
"mixed-state noncombat guard cleanup, combat EndTurn pre-actuation drift, combat post-wait recapture continuity는 current main에서 닫혔고,
capture-boundary와 strict lifecycle proof도 current main에서 닫혔고,
combat stale-end-turn / target plateau family도 current main에서 닫혔고,
현재 남은 건 blocker라기보다 unrelated noncombat self-test red와 low-priority coverage frontier다"
```

### 3. current speed baseline

- speed recovery program의 핵심 목표였던 `capture-first -> observer-first, screenshot-on-demand` 전환은 common hot path 기준으로 현재 `main`에 반영됐다
- speed proof root:
  - [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- 해석:
  - explicit event recovery chain `step=8~9`는 `captureMode=skipped`, `sceneReasoningMode=observer-only`
  - common combat chain `step=11~17`도 `captureMode=skipped`, `sceneReasoningMode=observer-only`
  - representative `preflight->request`는 `1155~1466ms` band로 내려왔다
  - same root의 attempt는 `step=19`에서 `returned-main-menu` terminal로 끝났지만, speed proof 관점에서는 captured hot path가 0건이라는 점이 핵심이다
  - 이 root는 semantic blocker root가 아니라 speed/capture baseline evidence로만 본다

## 진행 스냅샷

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust / Deploy Identity | green | broad top blocker 아님, latest live root에서 main-menu phase-boundary도 비회귀 |
| Harness Architecture | green | wave 1-8 complete, shell/module split complete |
| Build / Shared Self-Test | green | current `main` 기준 통과 |
| Harness Self-Test | red | current `main` 기준으로 `WaitPostMapNodeRoom -> reward reopen` known failure 1건이 남아 있다 |
| Replay Golden Suite | green | `replay-test` 통과 |
| Replay Parity Suite | green | `reward-aftermath-map-handoff` 포함 current parity fixtures green |
| Non-Combat Stability | green | reward aftermath map-node continuity closure, fresh live root confirms post-reward progression |
| Combat Stability | green | micro-stage gating, quiet convergence settle, runtime target-summary authority까지 current `main`에 반영됐고 fresh live root에서 stale end-turn / target plateau family가 재현되지 않았다 |
| Live-Run Speed Recovery | green | explicit event / common combat hot path가 observer-first로 복귀했고 representative speed root에서 captured step가 0건 |
| Observer Provenance Migration | green | bridge / tracker / reader / harness control-flow가 published-first baseline으로 정리됐고 compatibility는 legacy surface로만 남는다 |
| Post-Refactor Cleanup Program | green | runner residual, noncombat residue, partial `Program` owner shedding, supervision health split까지 current `main`에 반영 |
| Capture Boundary | green | bounded failure contract와 self-test 위에 fresh live proof [capture-boundary-proof-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1)이 추가됐다 |
| Strict Lifecycle Chain | green | fresh live root [strict-lifecycle-chain-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2)에서 `0001 terminal -> 0002 restart -> 0002 next-attempt-started` chronology와 first-screen proof가 닫혔다 |

## 현재 바로 믿을 수 있는 것

- `build`, `Sts2ModKit.SelfTest`, `replay-test`, `replay-parity-test`는 current `main`에서 green이다
- `Sts2GuiSmokeHarness -- self-test`는 현재 `WaitPostMapNodeRoom -> reward reopen` known failure 1건 때문에 red다
- replay parity는 current `main` 기준 green이다
- `WaitRunLoad -> HandleRewards` resumed room handoff fix는 current `main`에 반영되어 있다
- reward aftermath `ChooseFirstNode` exported-node closure는 current `main`에 반영되어 있다
- `WaitMainMenu`는 current `main`에서 actual `Continue` / `Singleplayer` run-start surface가 나오기 전에는 accepted 되지 않는다
- explicit event recovery와 common combat lane은 current `main`에서 screenshot 기본 경로가 아니다
- representative speed proof root에서 explicit event/combat chain의 `captureMode`는 전부 `skipped`다
- current `main`의 control-flow observer provenance는 published-first이고, published provenance는 legacy `visibleScreen` / `sceneReady` 계열로 다시 채워지지 않는다
- bridge node semantics는 compatibility scene winner를 다시 먹지 않는다
- combat post-action은 더 이상 generic observer delta 하나로 다음 step을 열지 않고, lane micro-stage + quiet convergence로 settle된다
- runtime `combatTargetSummary` raw fact는 current `main`에서 explicit enemy-target authority로 소비된다
- 하네스 구조 refactor는 current `main` 기준 문서화 가능한 수준까지 정리됐다
- post-refactor cleanup program도 current `main` 기준 완료 상태다

## 현재 대표 evidence

### 구조 / validation baseline

- cleanup baseline contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
- current harness architecture:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- human-readable before/after comparison:
  - [GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md)
- glossary:
  - [GUI_SMOKE_HARNESS_GLOSSARY_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

### current semantic closure evidence

- fresh combat blocker fix root:
  - [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)
- combat blocker fix startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/startup-summary.json)
- combat blocker fix session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/session-summary.json)
- combat blocker fix run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/attempts/0001/run.log)
- fresh reward/map closure root:
  - [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1)
- latest cleanup proof root:
  - [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
- latest cleanup proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/startup-summary.json)
- latest cleanup proof session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/session-summary.json)
- latest cleanup proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/attempts/0001/run.log)
- main-menu boundary proof root:
  - [wait-main-menu-run-start-readiness-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1)
- main-menu boundary startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/startup-summary.json)
- main-menu boundary session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/session-summary.json)
- main-menu boundary run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/attempts/0001/run.log)
- speed recovery proof root:
  - [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- speed proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/startup-summary.json)
- speed proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/run.log)
- speed proof terminal summary:
  - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/failure-summary.json)
- observer provenance split proof root:
  - [observer-compat-shadow-retirement-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-compat-shadow-retirement-20260329-live1)
- capture-boundary proof root:
  - [capture-boundary-proof-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1)
- capture-boundary proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/startup-summary.json)
- capture-boundary proof session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/session-summary.json)
- capture-boundary proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/attempts/0001/run.log)
- capture-boundary proof failure summary:
  - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/attempts/0001/failure-summary.json)
- strict lifecycle proof root:
  - [strict-lifecycle-chain-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2)
- strict lifecycle restart chronology:
  - [restart-events.ndjson](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/restart-events.ndjson)
- strict lifecycle supervisor state:
  - [supervisor-state.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/supervisor-state.json)
- strict lifecycle second-attempt first screen:
  - [0001.screen.png](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/attempts/0002/steps/0001.screen.png)
- parity fixture now green:
  - `tests/replay-fixtures/m6-parity/reward-map-handoff.request.json`

## 현재 engineering 해석

### 이미 닫힌 것

- deploy / trust / manual clean boot / authoritative attempt creation
- `WaitRunLoad` resumed room authority handoff
- reward aftermath `ChooseFirstNode` exported reachable map-node handoff
- `reward-aftermath-map-handoff` replay parity known red
- mixed-state local guard residue that duplicated canonical owner truth
- combat EndTurn pre-actuation observer-drift cancellation
- combat EndTurn barrier arming from `observer-drift` history instead of actual sent actions
- screenshot-first explicit event recovery
- screenshot-first common combat attack / target / non-enemy confirm / end-turn chain
- `WaitMainMenu` logo-animation-only premature acceptance
- published provenance backfill from legacy `meta.visibleScreen` / `sceneReady` / `sceneAuthority` / `sceneStability`
- bridge node semantics driven by compatibility scene winner
- monolithic `Program.cs` 중심 구조
- large self-test hotspot 1차 분해
- runner residual / noncombat residue / partial `Program` helper owner / supervision health band hotspot extraction
- fresh live capture-boundary bounded failure proof
- fresh live strict lifecycle restart chronology proof
- combat broader target parity/live proof
- enemy-turn closed play-phase replay/live proof
- combat non-enemy stale-end-turn blocker family
- combat attack-target plateau on stale zero-count target aggregates
- reward back canonical replay proof
- post-node destination continuity handoff coverage

### 아직 열려 있는 것

- `WaitPostMapNodeRoom -> reward reopen` harness self-test regression은 current `main`에서 아직 red다
- some low-priority coverage rows (`event reward substate`, `reward-map loop sentinel`) remain partial

## 다음 작업 원칙

1. 새 semantic fix는 좁게 한다
   - refactor wave는 현재 완료 상태를 reopen하지 않는다
2. 다음 coverage follow-up은 current owner 파일에서만 본다
   - active noncombat red:
     - [Program.PhaseLoopRouting.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)
     - [Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.SelfTests.PhaseRouting.EnterRunAndPostNode.cs)
     - [AutoDecisionProvider.NonCombatSceneState.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs)
   - retained combat owner baseline:
     - [Analysis/CombatMicroStageSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatMicroStageSupport.cs)
     - [Analysis/CombatPostActionObservationSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatPostActionObservationSupport.cs)
     - [Program.AllowedActions.Combat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
     - [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
     - [Analysis/CombatBarrierSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatBarrierSupport.cs)
     - [Analysis/CombatTargetabilitySupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatTargetabilitySupport.cs)
3. semantic blocker, speed evidence, coverage gap을 구분한다
   - noncombat mixed-state와 combat EndTurn barrier family는 닫혔다
   - explicit event/common combat speed baseline도 현재 `main`에서 회복됐다
   - post-refactor cleanup program도 완료됐다
   - 현재 남은 것은 coverage frontier와 일부 low-priority partial row뿐이다

## 한 줄 요약

```text
current main의 smoke harness architecture refactor는 완료됐다.
reward aftermath live/parity gap과 combat stale-end-turn / target plateau family도 current main에서 닫혔다.
explicit event / common combat hot path의 observer-first speed recovery와 published-first observer provenance migration도 current main에 반영됐다.
다만 harness self-test는 현재 `WaitPostMapNodeRoom -> reward reopen` known red 1건이 남아 있다.
다음 follow-up은 이 unrelated noncombat self-test red와 low-priority coverage frontier를 current owner 구조 안에서 보강하는 쪽이다.
```
