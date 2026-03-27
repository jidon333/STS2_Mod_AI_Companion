# 프로젝트 현황

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 현재 milestone pointer, current semantic blocker, representative evidence, 또는 harness architecture state가 바뀔 때

## 날짜

- 2026-03-28

## 현재 마일스톤 위치

- 현재 진행 축: `M7 non-combat stability` + `M8 combat stability` 평가
- 현재 engineering focus:
  1. `Sts2GuiSmokeHarness` 구조 정리 완료 상태를 문서와 current pointer에 반영
  2. 그 위에서 남아 있는 semantic gap을 새 owner 구조 안에서 좁게 다루기
- 장기 제품 목표: 사람이 실제 플레이 중 참고하는 `읽기 전용 advisor`

중요한 현재 해석:

- startup / trust / bootstrap / deploy identity는 현재 top blocker가 아니다
- `WaitRunLoad` resumed room handoff bug는 이미 닫혔다
- 현재 핵심은 **architecture complete 상태에서 남은 semantic gap을 좁게 다루는 것**이다

## 현재 우선순위

### 1. 하네스 구조 기준선

current `main`의 하네스 구조 정리 wave 1-8은 완료됐다.

핵심 결과:

- [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)는 shell-only다
- observer / analysis / decisions / runner / artifacts / self-test가 물리적으로 분리됐다
- `LongRunArtifacts` band 분리, `AutoDecisionProvider` vertical 분리, runner seam 분리, large self-test split이 완료됐다
- canonical 구조 기준 문서는 [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)다
- current file owner reference는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)다

### 2. 현재 semantic gap

구조 작업 이후에도 current semantic follow-up은 남아 있다.

현재 가장 중요한 두 signal은 아래다.

1. replay parity known red
   - fixture: `reward-aftermath-map-handoff`
   - 현재 baseline: single known red
   - 해석: architecture regression이 아니라 existing semantic gap으로 고정
2. latest valid fresh live first blocker
   - root: [longrun-artifacts-split-20260327-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1)
   - blocker: `ChooseFirstNode` `decision-wait-plateau`
   - shape: resumed reward aftermath 이후 map authority는 잡았지만 reachable node click으로 진전하지 못함

즉 현재 질문은 더 이상

```text
"WaitRunLoad가 reward/map mixed state에서 room handoff를 못 한다"
```

가 아니라

```text
"reward aftermath 이후 ChooseFirstNode / WaitMap / map-node routing을
새 owner 구조 안에서 어떻게 닫을 것인가"
```

다.

## 진행 스냅샷

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust / Deploy Identity | green | 현재 top blocker 아님 |
| Harness Architecture | green | wave 1-8 complete, shell/module split complete |
| Build / Shared Self-Test | green | current `main` 기준 통과 |
| Harness Self-Test | green | current `main` 기준 통과 |
| Replay Golden Suite | green | `replay-test` 통과 |
| Replay Parity Suite | partial | single known red `reward-aftermath-map-handoff` 유지 |
| Non-Combat Stability | partial | `WaitRunLoad` handoff는 닫혔고, reward aftermath map-node continuity가 남음 |
| Combat Stability | partial | architecture는 정리됐지만 broader live/parity follow-up이 남음 |
| Strict Lifecycle Chain | partial | terminal -> restart -> next-attempt first-screen evidence는 여전히 appendix/work item |

## 현재 바로 믿을 수 있는 것

- `build`, `Sts2ModKit.SelfTest`, `Sts2GuiSmokeHarness -- self-test`, `replay-test`는 current `main`에서 green이다
- replay parity는 현재 **한 개의 known red만 유지하는 baseline**으로 관리한다
- `WaitRunLoad -> HandleRewards` resumed room handoff fix는 current `main`에 반영되어 있다
- 하네스 구조 refactor는 current `main` 기준 문서화 가능한 수준까지 정리됐다

## 현재 대표 evidence

### 구조 / validation baseline

- module boundary contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
- current harness architecture:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

### current semantic blocker evidence

- latest valid live root:
  - [longrun-artifacts-split-20260327-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1)
- fresh live failure summary:
  - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1/attempts/0001/failure-summary.json)
- fresh live run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/longrun-artifacts-split-20260327-live1/attempts/0001/run.log)
- known parity red fixture family:
  - `tests/replay-fixtures/m6-parity/reward-map-handoff.request.json`

## 현재 engineering 해석

### 이미 닫힌 것

- deploy / trust / manual clean boot / authoritative attempt creation
- `WaitRunLoad` resumed room authority handoff
- monolithic `Program.cs` 중심 구조
- large self-test hotspot 1차 분해

### 아직 열려 있는 것

- reward aftermath 이후 `ChooseFirstNode`가 exported reachable node로 진행하지 못하는 gap
- `reward-aftermath-map-handoff` parity known red
- combat broader parity/live coverage
- strict lifecycle chain appendix

## 다음 작업 원칙

1. 새 semantic fix는 좁게 한다
   - refactor wave는 현재 완료 상태를 reopen하지 않는다
2. 새 blocker는 현재 owner 파일에서만 본다
   - [Observer/GuiSmokeObserverPhaseHeuristics.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/GuiSmokeObserverPhaseHeuristics.cs)
   - [Program.PhaseLoopRouting.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)
   - [Program.AllowedActions.NonCombat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
   - [AutoDecisionProvider.NonCombatSceneState.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs)
   - [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
3. replay known red와 fresh live blocker를 함께 본다
   - 둘이 같은 family인지 먼저 확인하고, 아니면 source-of-truth를 분리한다

## 한 줄 요약

```text
current main의 smoke harness architecture refactor는 완료됐다.
지금은 WaitRunLoad가 아니라 reward aftermath 이후 ChooseFirstNode / parity gap이 남아 있으며,
다음 semantic fix는 새 owner 구조 안에서 좁게 진행해야 한다.
```
