# GuiSmokeHarness Module Boundaries

> Status: Live Contract
> Source of truth: Yes
> Update when: target module ownership, extraction order, or shell/file boundaries change.

## 0. 먼저 읽을 요약

이 문서는 `Sts2GuiSmokeHarness`의 장기 리팩터링 방향을 고정하는 구조 계약이다.

핵심은 네 가지다.

- 하네스 production code를 일반적인 C# 프로젝트 수준으로 읽히게 만든다.
- 같은 개념은 production owner를 정확히 하나만 가진다.
- 구조화와 중복 제거는 `No behavior change` 원칙으로 진행한다.
- current blocker 추적과 long-term 구조 계획을 한 문서에 섞지 않는다.

이 문서는 현재 어떤 blocker를 고치고 있는지 설명하는 handoff 문서가 아니다.
또한 reward/event/combat 규칙 자체를 세세하게 기술하는 semantics 문서도 아니다.

이 문서가 고정하려는 것은 아래 세 가지다.

1. `GuiSmokeHarness` 내부의 canonical module boundary
2. 그 경계를 안전하게 만드는 extraction wave 순서
3. dead/stale/duplicate code를 언제 제거할지에 대한 cleanup 규칙

## 0-1. Current state pointer

current blocker, latest authoritative root, active session handoff는 이 문서가 아니라 `docs/current/`에서 관리한다.

- 현재 상태: [PROJECT_STATUS.md](../current/PROJECT_STATUS.md)
- 세션 handoff: [AI_SESSION_HANDOFF_KO.md](../current/AI_SESSION_HANDOFF_KO.md)
- regression checklist: [HARNESS_REGRESSION_CHECKLIST_KO.md](../current/HARNESS_REGRESSION_CHECKLIST_KO.md)

이 문서는 current snapshot이 아니라 long-lived structure contract다.

## 1. 문서 목적

이 문서의 목적은 `Sts2GuiSmokeHarness`를 다음 상태로 수렴시키는 것이다.

- [`Program.cs`](../../src/Sts2GuiSmokeHarness/Program.cs)는 shell-only
- runner는 읽히는 orchestrator만 남는다
- observer는 scene authority와 phase routing의 canonical owner가 된다
- decision layer는 noncombat / map / combat vertical로 나뉜다
- artifacts layer는 startup / supervision / diagnostics / storage / review responsibility를 분리한다
- self-test는 production owner를 따라가는 검증 계층으로 읽힌다

이 문서는 아래 독자를 대상으로 한다.

- 하네스 코드를 수정하려는 개발자
- current blocker를 좁게 보고 싶은 리뷰어
- artifact와 code owner를 빠르게 매칭해야 하는 세션 구현자

## 2. Refactor Principles

장기 리팩터링은 아래 원칙으로 고정한다.

- `No behavior change`
  - 구조화와 중복 제거는 blocker fix와 분리한다.
  - heuristic 변경이나 acceptance 변경은 별도 work unit으로 분리한다.
- `One concept = one owner`
  - 같은 의미를 두 군데 이상에서 해석하지 않는다.
  - `Build*`, `Resolve*`, `Decide*`의 책임을 섞지 않는다.
- `Same-project refactor only`
  - `Sts2GuiSmokeHarness` 프로젝트 내부에서만 구조를 정리한다.
  - 새 assembly나 project는 만들지 않는다.
- `No permanent forwarder`
  - call site를 옮긴 뒤 old body나 thin wrapper를 남기지 않는다.
  - redirect와 old owner 삭제는 같은 wave 안에서 끝낸다.
- `No misc dump file`
  - `Shared`, `Misc`, `TempRefactor`, `Helpers2` 같은 dumping ground를 만들지 않는다.
- `Small work units`
  - 한 commit에는 하나의 owner family만 넣는다.
  - 구조화 중인 파일과 blocker fix를 같은 commit에 섞지 않는다.

파일 크기 목표는 아래로 둔다.

- `Program.cs`: 목표 `<= 500`, hard cap `800`
- production file: 목표 `<= 900`, hard cap `1200`
- self-test file: 목표 `<= 1200`

## 3. Current Structure Snapshot

현재 구조는 “logical modularity는 꽤 생겼지만 physical modularity는 아직 불완전한 상태”로 읽는다.

이미 canonical owner 방향으로 분리된 영역:

- `Observer/`
- `Interop/`
- `Analysis/`
- `Artifacts/`
- `GuiSmokeRuntimeContracts.cs`
- `GuiSmokeReplayContracts.cs`
- `GuiSmokeObserverContracts.cs`
- `GuiSmokeDecisionContracts.cs`
- `LongRunArtifacts.Review.cs`
- `LongRunArtifacts.Startup.cs`
- `LongRunArtifacts.Supervision.cs`
- `LongRunArtifacts.PlateauDiagnostics.cs`
- `LongRunArtifacts.Storage.cs`
- `AutoDecisionProvider.*`
- `Program.StepRequests.cs`
- `Program.SceneReasoning.cs`
- `Program.AllowedActions.*.cs`
- `Program.PhaseFailureHints.cs`
- `Program.PhaseLoopRouting.cs`
- `Program.ProgressAndValidation.cs`

현재 남은 주요 hotspot:

- [`Program.cs`](../../src/Sts2GuiSmokeHarness/Program.cs) `56` lines
- [`Program.Runner.AttemptLifecycle.cs`](../../src/Sts2GuiSmokeHarness/Program.Runner.AttemptLifecycle.cs)
- [`Program.PhaseLoopRouting.cs`](../../src/Sts2GuiSmokeHarness/Program.PhaseLoopRouting.cs)
- [`Program.SelfTests.CombatContracts.cs`](../../src/Sts2GuiSmokeHarness/Program.SelfTests.CombatContracts.cs)
- [`Program.SelfTests.PhaseRouting.cs`](../../src/Sts2GuiSmokeHarness/Program.SelfTests.PhaseRouting.cs)

구조적으로 가장 큰 문제는 아래 둘이다.

- `Program.cs` shell-only는 달성했지만, phase/loop routing helper가 아직 큰 단일 file band로 남아 있다
- `RunAttemptAsync` seam은 분리됐지만 self-test hotspot과 일부 runner/loop helper band가 여전히 크게 남아 있다

## 4. Target Module Boundaries

### 4.1 Shell

- [`Program.cs`](../../src/Sts2GuiSmokeHarness/Program.cs)는 `Main`, command dispatch, top-level exception boundary만 가진다.
- CLI parse와 replay/inspect entrypoint는 shell이 아니라 dedicated owner를 가진다.
- `Program.cs`는 contract, heuristic, long-run artifact owner가 아니다.

### 4.2 Core Contracts

- phase, request/decision/evaluation DTO, observer summary, room-state record, run/session/replay record는 dedicated contract files로 분리한다.
- contract layer는 behavior를 결정하지 않는다.
- contract는 `Observer`, `Decisions`, `Runner`, `Testing`이 공통으로 읽는 기반 타입만 가진다.

### 4.3 Runner

- runner의 owner는 launch, attempt lifecycle orchestration, step loop, terminal classification이다.
- bootstrap/deploy는 runner support owner가 가진다.
- `RunAttemptAsync`는 coordinator만 남고, 내부 단계는 seam별 partial file로 분해한다.
- runner는 scene authority를 재해석하지 않는다.

### 4.4 Observer

- observer layer는 scene authority, foreground ownership, phase routing, transition boundary의 canonical owner다.
- `TryGetPostRunLoadPhase`, `TryGetPostEmbarkPhase`, `ShouldHoldRunLoadBoundary`, room-specific foreground signals는 observer에서만 해석한다.
- decision layer는 observer-derived state를 소비만 한다.

### 4.5 Decisions

- decision layer는 action selection만 담당한다.
- `AutoDecisionProvider`는 coordinator이고, 실제 판단은 noncombat / map / combat vertical owner로 분해한다.
- reward/event/shop/rest-site/treasure canonical scene build는 noncombat owner만 가진다.
- combat target resolution은 combat support owner만 가진다.

### 4.6 Artifacts

- `LongRunArtifacts`는 startup, supervision, diagnostics, storage, review owner로 나뉜다.
- artifact schema, filename, JSON/NDJSON surface는 유지한다.
- `LongRunArtifacts`와 runner는 서로의 internal helper owner가 되지 않는다.

### 4.7 Testing

- self-test는 production owner를 따라가는 verification layer다.
- category file은 assertion과 fixture orchestration만 남기고, production semantics의 canonical owner가 되지 않는다.

## 5. Ordered Extraction Waves

아래 순서가 `GuiSmokeHarness` 장기 리팩터링의 canonical order다.

### Wave 1. Runtime/session/replay core contracts extraction

- phase, history/window bounds, session/run/deploy/trace/replay fixture DTO를 `Program.cs` 밖으로 이동한다.
- 타입 이동만 하고 behavior는 바꾸지 않는다.
- 이 wave가 끝나면 runner/artifact/replay review에서 필요한 pure contracts는 `Program.cs` 밖에서 찾을 수 있어야 한다.

### Wave 2. Observer/decision-facing contracts extraction

- request/decision/evaluation records, observer summary family, room-state records, `IGuiDecisionProvider`를 `Program.cs` 밖으로 이동한다.
- 타입 이동만 하고 behavior는 바꾸지 않는다.
- 이 wave가 끝나면 구조 review 시 `Program.cs`를 열지 않고도 decision/observer contract 소유권을 찾을 수 있어야 한다.

### Wave 3. `AutoDecisionProvider` noncombat vertical split

- reward / event / shop / rest-site / treasure / map-aftermath decision과 scene-state build를 noncombat owner로 모은다.
- observer contract를 다시 구현하지 않고 소비만 하게 만든다.
- noncombat stale wrapper와 duplicate helper는 같은 wave에서 삭제한다.

### Wave 4. `AutoDecisionProvider` combat vertical split

- combat action selection, target resolution, barrier/eligibility support를 combat owner로 모은다.
- combat duplicate helper는 canonical combat support owner만 남긴다.

### Wave 5. Runner seam extraction

- [`Program.Runner.cs`](../../src/Sts2GuiSmokeHarness/Program.Runner.cs)의 `RunAttemptAsync`를 단계별 seam으로 분해한다.
- target seams:
  - attempt setup
  - capture / observer acceptance
  - request + decision
  - actuation
  - post-action / terminal classification
  - artifact flush
- 이 wave가 끝나면 runner는 읽히는 orchestrator여야 한다.

### Wave 6. Shared helper owner 정리

- top-level에 남은 scene signature, plateau fingerprint, bounds normalization, action factory, replay support helper를 의미별 owner로 이동한다.
- 이 wave가 끝나면 `Program.cs`는 shell 외의 helper owner 역할을 하지 않는다.

### Wave 7. Large self-test split

- `CombatContracts`, `PhaseRouting`, `NonCombatDecisionContracts`, `StartupRuntimeEvidence` 같은 large self-test file을 더 세분화한다.
- 공용 fixture builder는 shared test support로 분리한다.

### Wave 8. Final cleanup

- permanent shim, stale wrapper, compiler-proven dead code, temporary partial-file seam을 제거한다.
- 최종 목표는 blocker 추적 시 `Observer 1개 + Decision 1개 + Runner/Artifacts 1개` 정도만 열면 원인을 따라갈 수 있는 구조다.

## 6. Cleanup Rules

dead/stale/duplicate code 제거 시점은 아래 규칙으로 고정한다.

### 6.1 Compiler-proven dead code

- compiler warning이나 search로 증명되는 unused helper는 발견 즉시 제거한다.
- “나중에 한 번에 sweep”하지 않는다.

### 6.2 Stale wrapper / thin adapter

- call site를 새 owner로 옮겼으면 old wrapper는 같은 wave에서 삭제한다.
- redirect만 하고 old body를 남기는 commit은 만들지 않는다.

### 6.3 Semantic duplicate

- semantic duplicate는 canonical owner가 고정된 wave에서만 삭제한다.
- 예:
  - reward/event/shop/rest-site scene-state helper
  - combat target resolution helper
  - room authority 판정 helper

### 6.4 Temporary partial-file shim

- partial split을 위한 임시 seam은 목적 wave가 끝나면 바로 제거한다.
- `partial`은 과도기 수단이지 최종 구조 목표가 아니다.

## 7. Validation And Acceptance

모든 wave는 아래 검증을 유지해야 한다.

- `cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln`
- `cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test`
- `cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test`

parity baseline은 아래로 고정한다.

- existing known red `reward-aftermath-map-handoff` 하나만 유지
- failing fixture 수 증가 금지
- existing failing fixture의 failure shape drift 금지

추가 live validation 규칙:

- `LongRunArtifacts`, runner, decision layer를 건드린 wave는 fresh live run 1회를 추가한다.
- pure contract move나 doc-only wave는 live run 없이 끝낼 수 있다.

최종 acceptance는 아래를 만족해야 한다.

- `Program.cs` shell-only
- canonical owner가 파일 구조와 일치
- blocker review 시 열어야 하는 파일 수가 급격히 줄어든 상태
- stale wrapper와 duplicate semantic family가 장기적으로 다시 쌓이지 않는 구조

## 8. Related Sources

이 문서는 아래 문서들과 함께 읽는다.

- 상위 구조 배경: [ARCHITECTURE.md](../ARCHITECTURE.md)
- 상위 milestone 배경: [ROADMAP.md](../ROADMAP.md)
- runner/supervisor semantics: [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](./RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
- startup/deploy control layer: [STARTUP_DEPLOY_CONTROL_LAYER.md](./STARTUP_DEPLOY_CONTROL_LAYER.md)

역할 분리는 아래처럼 읽는다.

- `docs/contracts/*`: 장기 구조 계약
- `docs/current/*`: current blocker, current handoff, bounded next step
