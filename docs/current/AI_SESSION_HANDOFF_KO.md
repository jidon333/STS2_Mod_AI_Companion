# AI 세션 인수인계 문서 (KO)

> 상태: 현재 사용 중
> 기준 브랜치: `main`
> 최종 갱신: 2026-03-28
> 대상: 새 구현 세션, 새 검증 세션, 새 참모 세션

## 문서 목적

이 문서는 current `main` 기준으로 새 세션이 바로 이어서 작업할 수 있게 만드는 handoff다.

현재 이 문서가 고정해야 하는 것은 아래 네 가지다.

1. 지금 기준선은 무엇인가
2. 최근에 무엇이 끝났는가
3. 지금 semantic follow-up은 무엇인가
4. 새 세션이 어디부터 열어야 하는가

current blocker와 long-term 구조 계획을 섞지 않기 위해, 구조 계약은 별도 문서로 분리해 둔다.

## 반드시 먼저 읽을 것

1. [AGENTS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/AGENTS.md)
2. [PROJECT_STATUS.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/PROJECT_STATUS.md)
3. [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
4. [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
5. [HARNESS_REGRESSION_CHECKLIST_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md)
6. [HARNESS_FIXTURE_MATRIX_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md)

## canonical baseline

- branch: `main`
- harness entrypoint shell:
  - [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)
- current architecture map:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- current module boundary contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)

핵심 원칙:

```text
visible/open != canonical foreground owner
currentScreen = logical/flow screen
```

semantic fix는 기존 owner 구조를 다시 우회하지 않고 current owner 파일 안에서만 좁게 한다.

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
- wave 1-8 refactor program은 완료됐다
- 다음 작업은 구조 분리보다 semantic follow-up이다

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

현재 source-of-truth signal은 두 개다.

### 1. replay parity suite

- current baseline: green
- old `reward-aftermath-map-handoff` known red는 current `main`에서 닫혔다

### 2. latest valid fresh live root

- root: [mixed-state-guard-cleanup-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/mixed-state-guard-cleanup-20260328-live1)
- startup: [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/mixed-state-guard-cleanup-20260328-live1/startup-summary.json)
- failure: [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/mixed-state-guard-cleanup-20260328-live1/attempts/0001/failure-summary.json)
- trace: [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/mixed-state-guard-cleanup-20260328-live1/attempts/0001/run.log)

확정 사실:

- root는 valid다
- `WaitRunLoad -> HandleRewards`는 이미 정상 handoff된다
- reward aftermath `step=15`는 여전히 `exported reachable map node`로 진행한다
- mixed-state noncombat cleanup은 regression 없이 유지됐다
- current first blocker는 `HandleCombat` `combat-barrier-wait-plateau`다

## 다음 세션의 기본 목표

다음 semantic work unit이 있다면 목표는 이 하나다.

```text
combat barrier / post-wait / lifecycle frontier를
새 owner 구조 안에서 좁게 다룬다
```

우선 열 파일:

1. [Program.AllowedActions.Combat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
2. [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
3. [Analysis/CombatBarrierSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatBarrierSupport.cs)
4. [Analysis/CombatTargetabilitySupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatTargetabilitySupport.cs)
5. [LongRunArtifacts.Startup.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/LongRunArtifacts.Startup.cs)

## validation baseline

모든 semantic 작업은 기본적으로 아래를 유지해야 한다.

```text
cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln
cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-test
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- replay-parity-test
```

replay parity acceptance:

- green 유지
- 새 failing fixture 추가 금지

live root가 필요한 semantic fix는 fresh run 1회를 추가한다.

## 절대 reopen하지 말 것

1. refactor wave 1-8 자체
2. `WaitRunLoad` resumed room handoff bug
3. `Program.cs` monolith 시절 전제
4. `visible/open == foreground owner` 해석
5. 구조화와 semantic fix를 같은 commit에 섞는 방식

## 한 줄 요약

```text
current main의 하네스 구조 정리와 reward aftermath closure는 끝났다.
새 세션은 old Program.cs monolith를 다시 전제로 두지 말고,
combat / lifecycle coverage frontier를 새 owner 파일에서 보강하라.
```
