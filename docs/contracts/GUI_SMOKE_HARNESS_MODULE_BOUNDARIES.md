# GuiSmokeHarness Cleanup Program

> Status: Live Contract
> Source of truth: Yes
> Update when: cleanup priority, workstream order, observer fact/export contract, or validation cadence changes.

## 0. 먼저 읽을 요약

이 문서는 completed refactor 이후 `GuiSmokeHarness`에 남아 있는 구조 부채와 observer/export/bridge 부채를 순차적으로 정리하기 위한 장기 cleanup program이다.

이 문서가 고정하는 것은 네 가지다.

1. 리팩터링 이후에도 남아 있는 핵심 문제의 우선순위
2. harness와 observer stack을 어떤 원칙으로 정리할지
3. 순차적인 workstream order
4. 각 workstream의 validation / cleanup 규칙

이 문서는 current blocker source of truth가 아니다.
current blocker, latest live root, current handoff는 `docs/current/*`에서 관리한다.

현재 file owner map과 현재 구조 설명은 아래 문서를 본다.

- [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)

## 0-1. 현재 상태 포인터

- 현재 상태: [PROJECT_STATUS.md](../current/PROJECT_STATUS.md)
- 세션 handoff: [AI_SESSION_HANDOFF_KO.md](../current/AI_SESSION_HANDOFF_KO.md)
- regression checklist: [HARNESS_REGRESSION_CHECKLIST_KO.md](../current/HARNESS_REGRESSION_CHECKLIST_KO.md)
- fixture matrix: [HARNESS_FIXTURE_MATRIX_KO.md](../current/HARNESS_FIXTURE_MATRIX_KO.md)

## 1. 이 문서의 역할

이 문서는 예전의 extraction-wave blueprint를 대체한다.

즉 지금부터는 아래를 더 이상 목표로 두지 않는다.

- `Program.cs` shell-only 달성
- observer / analysis / decisions / artifacts / testing의 1차 물리 분리
- large self-test 1차 split

위 작업은 current `main`에서 이미 끝났기 때문이다.

대신 지금부터의 목표는 아래다.

- runtime hot path를 다시 읽히는 구조로 줄인다
- canonical owner truth를 request-scoped로 재사용하게 만든다
- stale adapter / duplicated helper / partial `Program` leakage를 제거한다
- observer/export/bridge를 fact-only 방향으로 되돌린다
- harness만 owner / release / handoff를 결정하게 만든다

## 2. 공통 원칙

### 2.1 역할 분리

observer / export / bridge는 fact only다.
harness는 owner / release / handoff / action selection owner다.

즉 아래처럼 고정한다.

```text
observer/export/bridge
  = raw runtime fact, conflicting fact, source tag, timestamp

harness
  = canonical foreground owner
  = release stage
  = handoff target
  = action lane / same-action suppression
```

### 2.2 우선순위 규칙

- per-step runtime hot path가 low-frequency startup code보다 우선이다
- additive fact export가 destructive cleanup보다 우선이다
- request-scoped truth reuse가 local heuristic cleanup보다 우선이다
- redirect + old body delete는 같은 work unit에서 끝낸다
- blocker fix와 cleanup work unit은 섞지 않는다

### 2.3 금지 규칙

- observer/export/bridge가 winner를 고르는 새 semantic field를 추가하지 않는다
- tracker/bridge가 `sceneReady`, `sceneStability`, `visibleScreen`, `SceneType`를 canonical owner처럼 재정의하지 않는다
- harness가 old synthetic field에만 의존하는 새 code path를 만들지 않는다
- completed refactor wave를 reopen하는 broad rewrite를 하지 않는다

## 3. 현재 남은 문제의 우선순위

### 3.1 Harness Priority Ladder

```text
H0. RunAttemptAsync runtime hot path
H1. request-scoped canonical scene truth 재사용 부족
H2. noncombat action-selection residue
H3. partial Program owner leakage
H4. startup/prevalidation artifact hotspot
H5. large self-test hotspot residuals
```

### 3.2 Observer Priority Ladder

```text
O0. LiveExportStateTracker semantic shaping
O1. InventoryPublisher bridge re-normalization
O2. RuntimeSnapshotReflectionExtractor semantic winner export
O3. harness observer contracts의 provenance 혼합
O4. legacy synthetic field retirement
```

## 4. Priority Detail

### P0. Request-scoped canonical scene truth

먼저 해결해야 할 이유:

- 같은 step에서 reward/event/canonical scene state를 여러 층이 다시 계산하고 있다
- 이 중복이 mixed-state와 allowlist, reasoning, validation drift를 다시 만든다
- observer cleanup을 하더라도 harness 내부 truth reuse가 먼저 정리되어야 blast radius가 줄어든다

핵심 대상:

- [GuiSmokeStepAnalysisContext.cs](../../src/Sts2GuiSmokeHarness/GuiSmokeStepAnalysisContext.cs)
- [AutoDecisionProvider.NonCombatSceneState.cs](../../src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSceneState.cs)
- [Program.SceneReasoning.cs](../../src/Sts2GuiSmokeHarness/Program.SceneReasoning.cs)
- [Program.AllowedActions.NonCombat.cs](../../src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
- [Program.PhaseLoopRouting.cs](../../src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)

목표:

- 같은 request에서 canonical noncombat scene을 한 번만 만들고 재사용
- repeated `BuildRewardSceneState`, `BuildEventSceneState`, `TryBuildCanonicalNonCombatSceneState` 호출을 request context owner로 모음

### P1. Runner hot path seam extraction

먼저 해결해야 할 이유:

- live blocker와 actuation/capture/debug 질문의 대부분이 아직 [Program.Runner.AttemptLifecycle.cs](../../src/Sts2GuiSmokeHarness/Program.Runner.AttemptLifecycle.cs)에 모인다
- capture acceptance, drift recapture, actuation, probe grace, barrier follow-up, post-action phase reconciliation이 한 method body 안에 있다

핵심 대상:

- [Program.Runner.AttemptLifecycle.cs](../../src/Sts2GuiSmokeHarness/Program.Runner.AttemptLifecycle.cs)
- [Program.ProgressAndValidation.cs](../../src/Sts2GuiSmokeHarness/Program.ProgressAndValidation.cs)
- [Program.PhaseLoopRouting.cs](../../src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)

목표 seams:

- request acceptance
- pre-actuation drift and window reconciliation
- action actuation
- post-action probe / progress
- phase reconciliation
- terminal / bounded failure exit

### P2. Noncombat action-selection residue cleanup

먼저 해결해야 할 이유:

- canonical owner/release/handoff는 정리됐지만, 마지막 action candidate materialization은 여전히 rest-site/event/map overlay subtype별 residue가 많다
- 다음 mixed-state edge case는 여기서 다시 새기 쉽다

핵심 대상:

- [AutoDecisionProvider.NonCombatSupport.cs](../../src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSupport.cs)
- [AutoDecisionProvider.NonCombatDecisions.cs](../../src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
- [AutoDecisionProvider.DecisionFactories.cs](../../src/Sts2GuiSmokeHarness/AutoDecisionProvider.DecisionFactories.cs)

목표:

- room lane별 helper band로 축소
- raw fallback / stale wrapper / local preference helper를 canonical scene owner 기반으로 정리

### P3. Observer fact export first

먼저 해결해야 할 이유:

- tracker와 bridge가 이미 `logicalScreen`, `visibleScreen`, `sceneReady`, `sceneStability`, `SceneType`를 계산한다
- harness가 synthetic truth를 소비하고 있어, consumer cleanup만으로는 재발 방지가 안 된다

핵심 대상:

- [RuntimeSnapshotReflectionExtractor.cs](../../src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs)
- [LiveExportStateTracker.cs](../../src/Sts2ModKit.Core/LiveExport/LiveExportStateTracker.cs)
- [InventoryPublisher.cs](../../src/Sts2ModAiCompanion.HarnessBridge/InventoryPublisher.cs)
- [GuiSmokeObserverContracts.cs](../../src/Sts2GuiSmokeHarness/GuiSmokeObserverContracts.cs)

목표:

- raw fact를 additive하게 export
- shaped compatibility field는 유지하되 primary truth에서 내림

### P4. Partial Program owner shedding

먼저 해결해야 할 이유:

- `Program.cs`는 shell-only지만 production helper owner는 여전히 `internal static partial class Program`에 남아 있다
- 결과적으로 cross-file helper leakage가 계속 쉬운 구조다

핵심 대상:

- [Program.StepRequests.cs](../../src/Sts2GuiSmokeHarness/Program.StepRequests.cs)
- [Program.SceneReasoning.cs](../../src/Sts2GuiSmokeHarness/Program.SceneReasoning.cs)
- [Program.AllowedActions.NonCombat.cs](../../src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
- [Program.AllowedActions.Combat.cs](../../src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
- [Program.PhaseLoopRouting.cs](../../src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)

목표:

- partial `Program`이 helper namespace처럼 쓰이는 상태를 줄인다
- thin adapter / duplicated owner를 제거하고 dedicated owner로 이동한다

### P5. Startup / prevalidation governance cleanup

핵심 대상:

- [LongRunArtifacts.Startup.cs](../../src/Sts2GuiSmokeHarness/LongRunArtifacts.Startup.cs)
- [LongRunArtifacts.Supervision.cs](../../src/Sts2GuiSmokeHarness/LongRunArtifacts.Supervision.cs)
- [LongRunArtifacts.Storage.cs](../../src/Sts2GuiSmokeHarness/LongRunArtifacts.Storage.cs)

목표:

- startup stage, deploy identity, runtime evidence, manual clean boot, attempt chronology를 band별로 더 분리
- low-frequency governance code가 one-file hotspot으로 남지 않게 한다

## 5. Ordered Workstreams

아래 순서가 post-refactor cleanup의 canonical order다.

### Workstream 1. Canonical scene cache consolidation

- request-scoped canonical noncombat scene cache를 추가한다
- `Program.SceneReasoning`, `Program.AllowedActions.NonCombat`, `Program.PhaseLoopRouting`, `Observer/NonCombatForegroundOwnership`, decision layer가 같은 cached truth를 읽게 한다
- old local recomputation helper는 같은 workstream에서 정리한다

### Workstream 2. Runner lifecycle seam extraction

- `RunAttemptAsync`를 acceptance / actuation / post-action / reconciliation seam으로 분해한다
- `Program.Runner.AttemptLifecycle.cs`의 method body를 줄이고, semantic helper를 local nested blocks가 아닌 dedicated runner band로 옮긴다

### Workstream 3. Noncombat residue reduction

- `AutoDecisionProvider.NonCombatSupport`를 lane-based support로 줄인다
- raw fallback, stale event/rest/map overlay residue helper를 canonical owner-based helper로 대체한다
- duplicated rest-site/event adapter를 같은 wave에서 삭제한다

### Workstream 4. Additive raw-fact export in runtime extractor

- runtime extractor에 raw screen / raw active screen type / raw overlay / raw modal / raw explicit choice surfaces / raw room facts를 additive하게 export한다
- current shaped fields는 compatibility로 유지한다

### Workstream 5. Tracker compatibility demotion

- `LiveExportStateTracker`는 raw facts를 보존하고, `logicalScreen`, `visibleScreen`, `sceneReady`, `sceneAuthority`, `sceneStability`를 compatibility meta로 내린다
- reward/map mixed-state special-case 같은 winner logic을 새 truth source로 쓰지 않게 한다

### Workstream 6. Bridge passthrough migration

- `InventoryPublisher`가 `SceneType`, `SceneReady`, `SceneStability`를 다시 계산하지 않게 줄인다
- snapshot raw/shaped meta를 provenance 보존 상태로 넘기고, bridge는 publish envelope owner만 유지한다

### Workstream 7. Harness observer contract split

- `ObserverSummary`와 room-state contracts에 raw fact fields와 compatibility fields를 같이 두되 provenance를 명확히 한다
- harness는 dual-read migration을 시작한다
- current synthetic field만 읽는 code path를 줄인다

### Workstream 8. Partial Program owner shedding

- `Program.*` helper band를 purpose-named owner로 더 이동한다
- dedicated owner가 생긴 helper는 same wave에서 partial `Program` wrapper를 삭제한다

### Workstream 9. Startup/prevalidation band cleanup

- `LongRunArtifacts.Startup` 안의 stage update / deploy verification / runtime evidence / lifecycle projection helper를 더 분리한다
- deploy hygiene와 startup chronology를 reviewer가 한두 file에서 찾게 만든다

### Workstream 10. Legacy synthetic field retirement

- dual-read가 안정되면 legacy shaped fields를 family 단위로 내린다
- retire order:
  1. `sceneReady` / `sceneStability`
  2. bridge `SceneType`
  3. tracker `visibleScreen` compatibility rules
  4. winner-shaped lane hints

## 6. Work Unit Rules

- `1 work unit = 1 commit`
- code + doc + deploy/environment change는 한 commit에 섞지 않는다
- additive migration work unit에서는 old compatibility field를 바로 삭제하지 않는다
- redirect-only commit 금지
- new owner 도입 시 old wrapper/delete를 같은 wave 안에서 끝낸다

## 7. Validation

### 7.1 기본 검증

모든 code work unit 공통:

- `cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln`
- `cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test`

### 7.2 Live-required workstreams

fresh live 1회가 필요한 workstream:

- Workstream 1
- Workstream 2
- Workstream 3
- Workstream 5
- Workstream 6
- Workstream 7
- Workstream 10

### 7.3 Observer migration acceptance

observer cleanup 계열은 아래를 만족해야 한다.

- conflicting facts가 있으면 one-winner collapse 대신 둘 다 남는다
- raw fact 없이 compatibility field만 남는 새 path를 만들지 않는다
- harness consumer가 old shaped field와 new raw fact를 비교 가능한 상태를 유지한다
- current parity green이 깨지지 않는다

## 8. Cleanup Rules

### 8.1 stale wrapper / thin adapter

- call site redirect가 끝난 same work unit에서 삭제한다

### 8.2 semantic duplicate

- request-scoped canonical scene cache가 들어간 뒤 삭제한다
- observer dual-read migration 중에는 old/new truth source를 temporary로 공존시킬 수 있다

### 8.3 compatibility field

- additive migration 단계에서는 유지
- harness consumer migration과 self-test parity가 끝나기 전까지 삭제 금지

### 8.4 docs

- current blocker / current live root는 `docs/current/*`
- current structure / file owner map은 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- 이 문서는 long-term cleanup priority와 ordered workstreams만 관리한다

## 9. 완료 기준

아래가 모두 만족되면 post-refactor cleanup program이 끝난 것으로 본다.

- `RunAttemptAsync`가 step-loop coordinator 수준으로 줄어든다
- canonical noncombat scene truth가 request-scoped로 재사용된다
- `AutoDecisionProvider.NonCombatSupport`의 room/subtype residue가 대폭 줄어든다
- tracker/bridge가 winner를 고르는 primary owner가 아니다
- harness observer contracts가 raw fact와 compatibility field provenance를 분리한다
- partial `Program` production owner leakage가 localized helper 수준으로 축소된다
- startup/prevalidation governance hotspot이 reviewer-friendly band로 정리된다

## 10. Related Docs

- current structure map:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](../reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- current status:
  - [PROJECT_STATUS.md](../current/PROJECT_STATUS.md)
- current session handoff:
  - [AI_SESSION_HANDOFF_KO.md](../current/AI_SESSION_HANDOFF_KO.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](./STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](./RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
