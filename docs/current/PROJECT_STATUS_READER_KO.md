# 프로젝트 현황 쉬운 설명

> 상태: 현재 사용 중
> 대상 독자: 한국어 사용자, 개발자, 새로 합류한 작업자
> 기준 문서: [PROJECT_STATUS.md](./PROJECT_STATUS.md)

## 지금 어디까지 왔나

지금은 “하네스 구조를 사람이 읽을 수 있게 정리하는 작업”이 큰 틀에서 끝난 상태입니다.

쉽게 말하면:

- 예전에는 `Program.cs` 한 파일이 너무 커서, 뭘 고쳤는지 추적하기가 어려웠습니다.
- 지금은 하네스가 `runner / observer / analysis / decisions / artifacts / self-test`로 나뉘어 있습니다.
- 그래서 앞으로 남은 문제는 “구조를 어떻게 나눌까”보다 “남은 semantic gap을 어디서 좁힐까” 쪽입니다.

## 지금 안정됐다고 볼 수 있는 것

### 1. 하네스 구조

- `Program.cs`는 이제 거의 shell만 남았습니다.
- startup/deploy, step loop, observer, decision, artifact, self-test가 분리됐습니다.
- 관련 문서도 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md), [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](../contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md) 기준으로 읽으면 됩니다.

### 2. 기본 검증 세트

current `main`에서 아래는 green입니다.

- `dotnet build`
- `Sts2ModKit.SelfTest`
- `Sts2GuiSmokeHarness -- self-test`
- `Sts2GuiSmokeHarness -- replay-test`

### 3. 예전에 막던 큰 blocker

`WaitRunLoad`가 resumed reward/map mixed state에서 room phase handoff를 못 하던 문제는 이미 닫혔습니다.

즉 이제는 예전 handoff bug를 다시 파고들 단계가 아닙니다.

## 지금 남아 있는 핵심

현재 남은 건 “reward aftermath 이후 map 쪽으로 자연스럽게 넘어가는가”입니다.

대표 신호는 두 개입니다.

1. latest valid live root에서는 `ChooseFirstNode`에서 `decision-wait-plateau`가 남아 있음
2. replay parity에는 `reward-aftermath-map-handoff`라는 known red가 하나 남아 있음

즉 요약하면:

```text
reward aftermath 이후
WaitMap / ChooseFirstNode / map-node routing family가
아직 완전히 닫히지 않았다
```

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

### 3. replay red와 live blocker를 같이 본다

parity에서 붉은 장면 하나가 있고 live에서도 비슷한 family가 보이면, 그 둘이 같은 원인인지 먼저 확인해야 합니다.

## 한 줄 요약

```text
하네스 구조 정리는 끝났고,
이제 남은 건 reward aftermath 이후 map routing gap을
새 owner 구조 안에서 좁게 닫는 것이다.
```
