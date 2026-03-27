# AI 세션 인수인계 문서 (KO)

> 상태: 현재 사용 중
> 기준 브랜치: `main`
> 최종 갱신: 2026-03-27
> 대상: 새 구현 세션, 새 참모 세션, 새 검증 세션

## 문서 목적

이 문서는 오염된 세션을 버리고 새 세션에서 바로 작업을 이어가기 위한 현재 상태 인수인계 문서다.

이 문서가 답해야 하는 질문은 다음이다.

1. 지금 기준선은 무엇인가
2. 최근에 무엇을 끝냈는가
3. 지금 첫 authoritative blocker는 무엇인가
4. 다음 세션이 바로 어디를 고쳐야 하는가
5. 지금 어떤 코드를 믿고 어떤 코드는 의심해야 하는가
6. 이후 구조 개선은 어떤 순서로 해야 하는가

중요:

- authoritative source는 항상 **current `main` + current-main artifact**다.
- clean validation branch/worktree evidence는 appendix급 참고 자료일 뿐, canonical status는 아니다.
- `currentScreen`은 visual top-most layer가 아니라 **logical/flow screen**이다.
- owner 용어는 `top-layer owner`가 아니라 **`canonical foreground owner`**를 쓴다.

## 반드시 먼저 읽을 것

1. [AGENTS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/AGENTS.md)
2. [HARNESS_REGRESSION_CHECKLIST_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md)
3. [HARNESS_FIXTURE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md)
4. [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md)

## 장기 목표와 현재 engineering 목표

### 장기 목표

- 최종 제품 목표는 여전히 **읽기 전용 advisor**다.
- 하네스는 제품이 아니라 **개발용 validation tool**이다.
- long-term milestone은 [ROADMAP.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/ROADMAP.md)를 canonical source로 따른다.

### 현재 engineering 목표

현재 engineering 목표는 다음 두 축이다.

1. mixed-state owner/action/release/handoff를 current `main` 기준으로 신뢰 가능한 상태기계로 정리
2. 35k 줄짜리 [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs)를 인간이 유지보수 가능한 구조로 분리

단, 우선순위는 항상:

1. **first authoritative blocker 수정**
2. 그 다음에 **no-behavior-change structural extraction**

이다.

## canonical baseline

현재 새 세션이 기준으로 삼아야 할 것은 다음이다.

- branch: `main`
- current harness entrypoint: [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs)
- state/checklist source:
  - [HARNESS_REGRESSION_CHECKLIST_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_REGRESSION_CHECKLIST_KO.md)
  - [HARNESS_FIXTURE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/HARNESS_FIXTURE_MATRIX_KO.md)

현재 owner 해석 원칙은 이 문장으로 고정한다.

```text
canonical foreground owner =
active-screen truth
> explicit room/screen authority
> explicit action surface
> residue
```

보이면 foreground라는 뜻이 아니다.

```text
visible/open != canonical foreground owner
```

## 최근 완료된 work unit

아래 커밋들은 모두 **이미 current `main`에 들어가 있다**.

1. `91f8711` `Recover validation lane guardrails`
2. `20c3044` `Cancel blocked open combat selections before end turn`
3. `ce038d3` `Canonicalize reward owner release contracts`
4. `33f5edf` `Canonicalize post-wait combat recapture boundary`
5. `bc84751` `Canonicalize event map release handoff`
6. `4e0cd4a` `Extract shared noncombat owner release contracts`

이 리팩터링 체인의 선행 기반으로 이미 current `main`에 있었던 중요한 커밋은:

1. `cb479e2` `Fix top-layer noncombat foreground ownership`
2. `439f0c3` `Speed up stable noncombat foreground handling`

## Stage 1~4에서 실제로 바뀐 것

### Stage 1. Rewards canonicalization

커밋:

- `ce038d3`

핵심 코드:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L18941)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L27784)

무엇이 바뀌었나:

- `RewardSceneState`를 도입해 reward owner/action/release/handoff를 한 번만 계산하게 만들었다.
- reward allowlist, decision, diagnostics, post-phase, loop sentinel이 같은 reward state를 읽게 정리했다.
- reward skip/proceed 이후 `ReleasePending`과 `same skip reissue suppression` 계약을 넣었다.

### Stage 2. Combat post-wait recapture boundary

커밋:

- `33f5edf`

핵심 코드:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L592)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L1016)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L33831)

무엇이 바뀌었나:

- combat policy를 바꾼 게 아니라, legitimate wait 뒤 capture boundary를 bounded contract로 고정했다.
- `TryCaptureDetailed(...)`와 `CaptureBoundaryResult`를 넣어:
  - capture 성공
  - explicit capture failure
  - failure summary
  중 하나가 반드시 남도록 만들었다.
- silent hang을 bounded failure로 바꿨다.

### Stage 3. Event/map release handoff

커밋:

- `bc84751`

핵심 코드:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L19005)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L27902)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L16791)

무엇이 바뀌었나:

- `EventSceneState`를 도입해 event owner/action/release/handoff를 한 번만 계산하게 만들었다.
- explicit proceed, ancient completion, reward substate, map residue를 제각각 따로 해석하지 않게 정리했다.
- 이미 green인 `EVENT-06`, `MAP-03`은 behavior 유지 + wrapper화만 했다.

### Stage 4. Shared noncombat owner/release contracts

커밋:

- `4e0cd4a`

핵심 코드:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L18924)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L28068)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L29073)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L27427)

무엇이 바뀌었나:

- `ICanonicalNonCombatSceneState` 공통 계약을 도입했다.
- reward/event/shop/rest/treasure가 공통 축을 공유하게 했다:
  - `CanonicalForegroundOwner`
  - `ReleaseStage`
  - `HandoffTarget`
  - `SuppressSameActionReissue`
  - `AllowsFastForegroundWait`
  - foreground/background debug kind
- fast-wait, WaitMap reopen, contradictory map fallback suppression, foreground/background diagnostics가 이 공통 계약을 읽게 바뀌었다.

Stage 4 중간에 잡은 회귀:

1. ambiguous event scene이 rest-site wrapper에 잘못 먹히던 문제
2. map node `휴식 (1,2)`가 rest-site legacy label로 오인되던 문제
3. reward `ReleasePending`인데 shared handoff가 `HandleRewards`로 남던 문제

## 현재 first authoritative blocker

### canonical root

- root: [stage4-shared-noncombat-20260327-live1](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1)
- startup: [startup-summary.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/startup-summary.json)
- failure: [failure-summary.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/attempts/0001/failure-summary.json)
- trace: [run.log](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/stage4-shared-noncombat-20260327-live1/attempts/0001/run.log)

### root validity

이 root는 valid다.

- `deployCommandExitCode = 0`
- `windowDetected = true`
- `manualCleanBootEvaluationFinished = true`
- `firstAttemptCreated = true`

즉 gameplay blocker로 해석해도 되는 root다.

### 실제 진행

1. `WaitMainMenu` 진입
2. main menu 캡처 확보
3. `EnterRun`에서 `continue` 클릭 성공
4. `WaitRunLoad` 진입
5. 이후 step `8~37` 동안 계속:
   - `logical=rewards`
   - `visible=map`
   - `ready=false`
   - `stability=stabilizing`
   - `extractor=reward`
   - explicit reward choices visible
6. `WaitRunLoad` timeout으로 종료

### direct cause

직접 원인은 Stage 4 shared contract 자체가 아니라 **`WaitRunLoad` phase gate**다.

핵심 함수:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L22678)

현재 로직은:

- `observer.SceneReady == false`이고
- `character-select` 예외가 아니면
- 바로 `false`를 반환한다

그런데 이번 live는 이미:

- `currentScreen = rewards`
- `choiceExtractorPath = reward`
- explicit reward choices visible
- `transitionInProgress = false`
- `rootSceneIsRun = true`

즉 room authority는 충분한데, `sceneReady=false` gate 때문에 `TryGetPostEmbarkPhase(...)`까지 내려가지 못하고 계속 `WaitRunLoad`에 머문다.

### 왜 이게 Stage 4 regression이 아닌가

이 blocker는 Stage 4 shared noncombat logic이 실제로 room phase에서 실행되기 전에 막힌다.

- run이 끝날 때까지 phase는 한 번도 `HandleRewards`로 넘어가지 않았다.
- 즉 reward/event/shared contract는 phase handoff 이전 단계에서 멈췄다.

정확한 해석:

```text
pre-Stage-4 phase handoff bug가
Stage 4 이후 current-main live에서 다시 surfaced됐다
```

즉 “shared noncombat extraction이 rewards를 망가뜨렸다”가 아니라, **`WaitRunLoad -> room phase` handoff gap**이 current-main에서 first blocker로 드러난 것이다.

## 다음 세션의 즉시 목표

다음 세션의 work unit은 이것 하나다.

```text
WaitRunLoad가 sceneReady=false만으로 room handoff를 막지 않도록 수정
```

정확한 타깃:

- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L22678)
- 관련:
  - [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L28068)
  - [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L20547)
  - [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs#L16791)

고칠 방향:

- `sceneReady=false`여도
- `transitionInProgress=false`
- `rootSceneIsRun=true`
- canonical noncombat owner or explicit room authority가 있으면
- `TryGetPostEmbarkPhase(...)`까지 내려가서
  - `HandleRewards`
  - `HandleEvent`
  - `HandleShop`
  - `ChooseFirstNode`
  같은 room phase로 탈출시켜야 한다

### acceptance

최소 acceptance는 다음이다.

1. build 통과
2. `Sts2ModKit.SelfTest` 통과
3. `Sts2GuiSmokeHarness -- self-test` 통과
4. fresh current-main live root 1회
5. `WaitRunLoad`가 이번 reward/map mixed resumed state에서 더 이상 timeout 나지 않음
6. 새 authoritative blocker가 나오면 그 1개만 보고

## 구조 개선 계획

이제 [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs) 는 35k 줄이라, behavior 안정화와 병행해 구조 분리가 필요하다.

단, 순서는 중요하다. 먼저 blocker를 닫고, 그 다음 no-behavior-change extraction으로 간다.

권장 extraction 순서:

1. **contracts / scene-state types**
   - `RewardSceneState`, `EventSceneState`, shared enums/interfaces
2. **self-test / replay code**
   - `RunSelfTest`, fixture builders, replay assertions
3. **infrastructure/runtime I/O**
   - capture, window, artifact, video
4. **raw observer signal helpers**
   - reward/event/shop/rest/treasure/root-scene observers
5. **decision layer**
   - noncombat
   - combat
   - analysis
   - alternate branch routing
6. `Program.cs`는 최종적으로 CLI + orchestration만 남긴다

구조 분리 때 지켜야 할 원칙:

- no-behavior-change commit 단위로 한다
- decision target, wait reason, branch kind, failure summary shape가 바뀌면 extraction이 아니라 regression으로 본다
- `green` 상태를 구조 분리 중에 다시 열면 안 된다

## 새 세션이 절대 오해하면 안 되는 것

1. `currentScreen`은 logical/flow screen이다
2. `visible/open != canonical foreground owner`
3. clean validation branch evidence는 appendix only다
4. current `main`이 canonical baseline이다
5. 이미 green인 상태는 freeze다
6. timer/count heuristic으로 mixed-state를 덮지 않는다
7. 같은 owner/action/release 판단을 새 helper에 또 복제하지 않는다

## validation branch / appendix evidence 위치

아래는 여전히 high-signal reference이지만, canonical status 자체는 아니다.

- `live9d` post-wait recapture hang
- `live10` reward wait plateau
- `live11` reward skip reissue / reward-map-loop

이들은 current-main blocker로 다시 확정되기 전까지는 appendix threat model로만 쓴다.

## 필수 검증 루프

모든 구현 세션은 기본적으로 아래를 돌린다.

```text
cmd.exe /c dotnet build STS2_Mod_AI_Companion.sln
cmd.exe /c dotnet run --project src/Sts2ModKit.SelfTest/Sts2ModKit.SelfTest.csproj --no-build
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj --no-build -- self-test
```

live root가 필요한 단계에서는:

```text
cmd.exe /c dotnet run --project src/Sts2GuiSmokeHarness/Sts2GuiSmokeHarness.csproj -- run --scenario boot-to-long-run --provider auto --run-root <fresh-root> --max-attempts 1 --max-steps <n> --stop-on-first-terminal --stop-on-first-loop
```

## repo hygiene

현재 repo에는 unrelated dirty가 매우 많다.

새 세션은:

- 자신의 work unit 파일만 stage/commit
- unrelated dirty inventory는 손대지 않음
- deploy/manual clean boot guardrail은 항상 준수

## 현재 반드시 알아야 하는 요약

한 줄 요약:

```text
Stage 1~4 리팩터링은 current main에 모두 들어갔고 build/self-test는 green이다.
지금 첫 authoritative current-main blocker는 shared noncombat contract가 아니라
WaitRunLoad가 sceneReady=false gate 때문에 resumed reward/map mixed state를 room phase로 넘기지 못하는 문제다.
다음 세션은 그 handoff를 고치고, 그 이후에만 Program.cs 구조 분리를 본격적으로 시작하면 된다.
```
