# 프로젝트 현황 쉬운 설명

> 상태: 현재 사용 중
> 대상 독자: 한국어 사용자, 개발자, 새로 합류한 작업자
> 기준 문서: [PROJECT_STATUS.md](./PROJECT_STATUS.md)

## 지금 어디까지 왔나

지금은 “하네스 구조를 사람이 읽을 수 있게 정리하는 작업”, “대표 hot path를 observer-first로 되돌리는 작업”, “observer provenance / runner / noncombat cleanup”이 큰 틀에서 끝난 상태이고, 그 위에 **anti-drift recovery wave**와 **post-live9 blocker closure wave**까지 한 번 더 들어간 상태입니다.

쉽게 말하면:

- 예전에는 `Program.cs` 한 파일이 너무 커서, 뭘 고쳤는지 추적하기가 어려웠습니다.
- 지금은 하네스가 `runner / observer / analysis / decisions / artifacts / self-test`로 나뉘어 있습니다.
- 그래서 앞으로 남은 문제는 “구조를 어떻게 나눌까”보다 “남은 coverage gap을 어디서 좁힐까” 쪽입니다.

## 지금 안정됐다고 볼 수 있는 것

### 1. 하네스 구조

- `Program.cs`는 이제 거의 shell만 남았습니다.
- startup/deploy, step loop, observer, decision, artifact, self-test가 분리됐습니다.
- 관련 문서는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)로 현재 구조를 읽고, [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](../contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)로 cleanup completion contract를 읽으면 됩니다.

### 2. 기본 검증 세트

current `main`에서 아래는 green입니다.

- `dotnet build`
- `Sts2ModKit.SelfTest`
- `Sts2GuiSmokeHarness -- self-test`
- `Sts2GuiSmokeHarness -- replay-test`
- `Sts2GuiSmokeHarness -- replay-parity-test`

즉, current code/test baseline 기준으로는 self-test까지 다시 green입니다.

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

cleanup completion 기준의 latest representative live root는 [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run) 입니다.

- `step=3`에서 actual `Continue` surface가 export된 뒤에만 `EnterRun`으로 handoff했습니다.
- `step=16`에서 `WaitRunLoad -> HandleCombat` handoff가 clean하게 이어졌습니다.
- `step=23 -> 24`, `step=45 -> 46`, `step=57 -> 58`에서 legitimate combat wait 뒤 다음 capture/request로 이어져 post-wait recapture continuity도 fresh live로 확인됐습니다.
- run은 `max-steps-reached:60`까지 진행했고 `failure-summary.json`은 생성되지 않았습니다.

speed baseline 기준의 historical proof root는 [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9) 입니다.

- 이 root는 semantic blocker root가 아니라 speed proof root입니다.
- attempt는 `returned-main-menu`로 끝났지만, speed 관점에서는 explicit event / common combat chain에서 captured hot path가 0건이었습니다.
- 다만 그 뒤 long blocker-fix loop에서 combat false-confirm / speed regression이 한 번 다시 들어왔고, current `main`에는 이를 되돌린 anti-drift recovery commits (`84e4647`, `5ebe718`, `3a24338`)가 반영돼 있습니다.
- fresh authoritative live rerun은 [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9) 로 이미 다시 찍혔습니다.
- 이제 바로 필요한 다음 단계는 `deck-remove` / reward card child-screen의 explicit owner/export gap을 좁혀 transient `captured/enriched` recapture를 줄이는 것입니다.

최근 전투 blocker root [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2) 기준으로는:

- non-enemy 카드 선택 직후 stale observer를 보고 `auto-end turn` 하던 family가 닫혔고
- attack lane이 stale zero-count target aggregate 때문에 plateau 나던 family도 닫혔습니다

현재 combat는:

- lane micro-stage를 계산하고
- generic delta 하나로 다음 step을 열지 않고
- runtime `combatTargetSummary` raw fact도 explicit target authority로 소비합니다

최근 shop recovery root [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4) 기준으로는:

- 예전에 한 번 보였던 `HandleShop -> HandleRewards -> shop plateau` family가 그대로 멈추지 않고
- `HandleRewards -> HandleShop`로 바로 되돌아와서
- 상점 열기, 구매, back, proceed가 계속 진행됐습니다

즉 지금은 전투 blocker도 닫혀 있고, shop misroute plateau도 current `main` 기준 active blocker는 아닙니다.

즉 지금 남은 핵심은 unrelated self-test red가 아닙니다. automated suite는 green이고, 새 authoritative live blocker가 하나 올라왔습니다.

그 blocker는 [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21) 의 ancient event option plateau입니다.

쉽게 말하면:

- 화면에는 `해독한다`, `부순다` 같은 실제 클릭 가능한 이벤트 버튼이 이미 보입니다.
- 하네스도 `event-option` 버튼 두 개를 bounds와 함께 보고 있습니다.
- 그런데 routing은 이것을 ancient 전용 option lane으로 해석한 뒤,
- ancient 전용 semantic이 없다는 이유로 그 버튼을 ancient explicit option으로 인정하지 못하고
- `wait`만 반복하다 plateau로 끝납니다.

즉 이번 문제는:

```text
이벤트를 못 봤다
```

가 아니라

```text
ancient special-case lane과 generic explicit event-option button contract가 안 맞는다
```

입니다.

이건 broad mixed-state 재발이 아니라, 3월 23일 ancient completion recovery wave에서 들어온 오래된 special-case가 recent explicit-truth cleanup 뒤에 드러난 것입니다.

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
하네스 구조 정리, reward aftermath mixed-state cleanup, published-first observer provenance migration, post-refactor cleanup program은 끝났고,
anti-drift recovery wave와 reward-pick / combat carryover closure까지 current `main`에 반영됐다.
이제 immediate follow-up은 old screenshot/broad-fallback 철학으로 되돌아가지 않은 채
`live21` ancient event option blocker를 generic explicit `NEventOptionButton` contract 기준으로 닫는 것이다.
그 다음에야 `deck-remove` / low-priority coverage frontier로 다시 돌아가면 된다.
```
