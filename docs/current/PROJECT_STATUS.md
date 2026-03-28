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
- 장기 cleanup 기준 문서는 [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)다
- current file owner reference는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)다

### 2. 현재 semantic gap

구조 작업 이후에도 current semantic follow-up은 남아 있다.

현재 가장 중요한 두 signal은 아래다.

1. replay parity suite
   - status: green
   - 해석: old `reward-aftermath-map-handoff` known red는 current `main`에서 닫혔다
2. latest valid fresh live root
   - root: [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1)
   - result: valid root, request-scoped noncombat scene cache가 reward aftermath / mixed-overlay / combat continuity를 흔들지 않은 채 run은 `max-steps-reached:60`까지 진행했다
   - shape: old `combat-barrier-wait-plateau`는 재현되지 않았고, barrier reason은 계속 `end turn acknowledged; waiting for the next round reopen`으로 유지된 뒤 다음 player-turn reopen으로 정상 복귀했다

즉 현재 질문은 더 이상

```text
"reward aftermath 이후 ChooseFirstNode / WaitMap / map-node routing이 막힌다"
```

가 아니다.

현재 authoritative frontier는

```text
"mixed-state noncombat guard cleanup과 combat EndTurn pre-actuation drift는 current main에서 닫혔고,
현재 남은 건 blocker라기보다 combat post-wait recapture / capture-boundary coverage frontier다"
```

## 진행 스냅샷

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust / Deploy Identity | green | 현재 top blocker 아님 |
| Harness Architecture | green | wave 1-8 complete, shell/module split complete |
| Build / Shared Self-Test | green | current `main` 기준 통과 |
| Harness Self-Test | green | current `main` 기준 통과 |
| Replay Golden Suite | green | `replay-test` 통과 |
| Replay Parity Suite | green | `reward-aftermath-map-handoff` 포함 current parity fixtures green |
| Non-Combat Stability | green | reward aftermath map-node continuity closure, fresh live root confirms post-reward progression |
| Combat Stability | green | latest fresh live root에서 repeated EndTurn barrier acknowledgement/reopen이 유지되고 run이 `max-steps-reached:60`까지 진행 |
| Strict Lifecycle Chain | partial | terminal -> restart -> next-attempt first-screen evidence는 여전히 appendix/work item |

## 현재 바로 믿을 수 있는 것

- `build`, `Sts2ModKit.SelfTest`, `Sts2GuiSmokeHarness -- self-test`, `replay-test`는 current `main`에서 green이다
- replay parity는 current `main` 기준 green이다
- `WaitRunLoad -> HandleRewards` resumed room handoff fix는 current `main`에 반영되어 있다
- reward aftermath `ChooseFirstNode` exported-node closure는 current `main`에 반영되어 있다
- 하네스 구조 refactor는 current `main` 기준 문서화 가능한 수준까지 정리됐다

## 현재 대표 evidence

### 구조 / validation baseline

- cleanup program:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
- current harness architecture:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

### current semantic closure evidence

- fresh reward/map closure root:
  - [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1)
- latest valid live root:
  - [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1)
- latest live startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1/startup-summary.json)
- latest live session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1/session-summary.json)
- latest live run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1/attempts/0001/run.log)
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
- monolithic `Program.cs` 중심 구조
- large self-test hotspot 1차 분해

### 아직 열려 있는 것

- combat post-wait recapture / capture-boundary live proof gap
- combat broader parity/live coverage
- strict lifecycle chain appendix
- some lower-priority noncombat coverage rows (`reward back`, `post-node destination continuity`) remain partial

## 다음 작업 원칙

1. 새 semantic fix는 좁게 한다
   - refactor wave는 현재 완료 상태를 reopen하지 않는다
2. 다음 coverage follow-up은 current owner 파일에서만 본다
   - [Program.AllowedActions.Combat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
   - [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
   - [Analysis/CombatBarrierSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatBarrierSupport.cs)
   - [Analysis/CombatTargetabilitySupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatTargetabilitySupport.cs)
3. semantic blocker와 evidence gap을 구분한다
   - noncombat mixed-state와 combat EndTurn barrier family는 닫혔고, 현재 남은 것은 coverage frontier다

## 한 줄 요약

```text
current main의 smoke harness architecture refactor는 완료됐다.
reward aftermath live/parity gap도 current main에서 닫혔다.
combat EndTurn pre-actuation drift / false barrier arm도 current main에서 닫혔다.
다음 follow-up은 combat / lifecycle coverage를 current owner 구조 안에서 보강하는 쪽이다.
```
