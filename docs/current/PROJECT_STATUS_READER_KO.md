# 프로젝트 현황 쉬운 설명

> 상태: 현재 사용 중
> 대상 독자: 한국어 사용자, 개발자, 새로 합류한 작업자
> 기준 문서: [PROJECT_STATUS.md](./PROJECT_STATUS.md)

## 지금 어디까지 왔나

지금은 “하네스 구조를 사람이 읽을 수 있게 정리하는 작업”과 “대표 hot path를 observer-first로 되돌리는 작업”이 큰 틀에서 끝난 상태입니다.

쉽게 말하면:

- 예전에는 `Program.cs` 한 파일이 너무 커서, 뭘 고쳤는지 추적하기가 어려웠습니다.
- 지금은 하네스가 `runner / observer / analysis / decisions / artifacts / self-test`로 나뉘어 있습니다.
- 그래서 앞으로 남은 문제는 “구조를 어떻게 나눌까”보다 “남은 semantic gap과 coverage를 어디서 좁힐까” 쪽입니다.

## 지금 안정됐다고 볼 수 있는 것

### 1. 하네스 구조

- `Program.cs`는 이제 거의 shell만 남았습니다.
- startup/deploy, step loop, observer, decision, artifact, self-test가 분리됐습니다.
- 관련 문서는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)로 현재 구조를 읽고, [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](../contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)로 남은 cleanup 우선순위를 읽으면 됩니다.

### 2. 기본 검증 세트

current `main`에서 아래는 green입니다.

- `dotnet build`
- `Sts2ModKit.SelfTest`
- `Sts2GuiSmokeHarness -- self-test`
- `Sts2GuiSmokeHarness -- replay-test`

### 3. 예전에 막던 큰 blocker

`WaitRunLoad`가 resumed reward/map mixed state에서 room phase handoff를 못 하던 문제는 이미 닫혔습니다.

즉 이제는 예전 handoff bug를 다시 파고들 단계가 아닙니다.

### 4. live-run 속도

- explicit event recovery와 common combat chain은 이제 screenshot 기본 경로가 아닙니다.
- representative speed proof root [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) 에서
  - `step=8~9` explicit event lane
  - `step=11~17` common combat lane
  가 모두 `captureMode=skipped`, `sceneReasoningMode=observer-only`로 진행했습니다.
- 대표 `preflight->request`는 `1155~1466ms` band였습니다.

## 지금 남아 있는 핵심

reward aftermath 이후 map 쪽으로 자연스럽게 넘어가는 문제는 current `main`에서 닫혔습니다.

대표 신호는 두 개입니다.

1. fresh live root [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1) 에서 reward aftermath `step=15`가 `exported reachable map node`로 진행함
2. replay parity의 `reward-aftermath-map-handoff`도 green으로 닫힘

즉 요약하면:

```text
reward aftermath 이후
WaitMap / ChooseFirstNode / map-node routing family는
current main에서 닫혔다
```

semantic continuity 기준의 latest valid fresh live root는 [request-scoped-scene-cache-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/request-scoped-scene-cache-20260328-live1) 입니다.

- 이 root는 valid입니다.
- request-scoped noncombat scene cache work unit이 reward aftermath / mixed-overlay / combat continuity를 흔들지 않았습니다.
- reward/map mixed-state cleanup은 regression 없이 통과했습니다.
- `HandleCombat` `step=17`의 `auto-end turn`은 실제 `key sent key=E`로 전송됐고, old `combat-barrier-wait-plateau`는 재현되지 않았습니다.
- run은 `max-steps-reached:60`까지 진행했습니다.

speed baseline 기준의 latest proof root는 [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) 입니다.

- 이 root는 semantic blocker root가 아니라 speed proof root입니다.
- attempt는 `returned-main-menu`로 끝났지만, speed 관점에서는 explicit event / common combat chain에서 captured hot path가 0건이었습니다.
- 즉 지금은 “카드 한 장 내는데 10초” family를 만드는 screenshot-first common lane은 current `main`의 대표 경로가 아닙니다.

즉 지금 남은 핵심은 noncombat mixed-state나 combat EndTurn barrier blocker가 아니라, combat post-wait recapture / capture-boundary coverage와 observer provenance cleanup입니다.

## 지금 작업할 때의 원칙

### 1. 큰 리팩터링은 다시 열지 않는다

구조 분리 작업은 현재 기준선으로 본다.

앞으로는:

- 새 semantic gap을 좁게 수정하고
- 관련 self-test / replay / live evidence만 확인하는 식으로 가는 게 맞습니다.

### 2. 새 문제는 새 owner 파일에서 본다

예전처럼 `Program.cs` 하나만 보면 안 됩니다.

지금은 보통 아래 중 2~3개 파일만 보면 됩니다.

- [Program.PhaseLoopRouting.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)
- [Program.AllowedActions.NonCombat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
- [Observer/GuiSmokeObserverPhaseHeuristics.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/GuiSmokeObserverPhaseHeuristics.cs)
- [AutoDecisionProvider.NonCombatSceneState.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs)
- [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)

### 3. replay와 live family를 같이 본다

replay fixture와 live blocker가 비슷한 family를 가리키면, 둘이 같은 owner/action/release contract를 공유하는지 먼저 확인해야 합니다.

## 한 줄 요약

```text
하네스 구조 정리, reward aftermath mixed-state cleanup, common hot path speed recovery는 끝났고,
이제 남은 건 combat/lifecycle coverage와 observer provenance cleanup을
새 owner 구조 안에서 current-main 기준으로 보강하는 것이다.
```
