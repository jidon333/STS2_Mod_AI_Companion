# AI Handoff Prompt (KO)

> Status: Live Current
> Source of truth: Yes
> Update when: next implementation work unit, guardrails, or acceptance focus changes.

## 문서 목적

이 문서는 새 `참모 세션`과 새 `구현+테스트 세션`이 지금 상태를 빠르게 이어받기 위한 hand-off 문서다.

이 문서가 답해야 하는 질문은 네 가지다.

1. 지금 프로젝트의 실제 방향성은 무엇인가
2. 지금까지 무엇을 해결했고 무엇은 아직 안 끝났는가
3. 다음 세션이 바로 손대야 할 가장 중요한 문제는 무엇인가
4. 어떤 검증 루프로 판단해야 false success를 피할 수 있는가

중요:

- 이 문서는 현재 시점의 작업 hand-off다.
- roadmap 전체를 대체하지 않는다.
- authoritative 판정은 항상 runtime artifact 기준이다.

## 현재 추천 역할 구조

현재 권장 세션 구조는 아래 두 축이다.

1. `참모 세션`
   - 문제 분해
   - 우선순위 결정
   - 구현 결과 검토
   - artifact 기준 판정
2. `구현+테스트 통합 세션`
   - 코드 수정
   - build / self-test / replay-test / live run
   - 재현 root 생성
   - 결과 보고

지금 단계에서는 `구현`과 `테스트`를 강하게 분리하기보다, 하나의 구현 세션이 짧은 디버그-수정-재검증 루프를 직접 닫는 편이 더 효율적이다.

## 현재 방향성

### 1. 장기 목표는 여전히 read-only advisor 제품이다

- 전체 목표는 `Phase 1: 외부 프로세스 AI 조언 어시스턴트 완성`이다.
- 최종적으로는 사람이 실제 플레이 중 참고 가능한 `read-only advisor`를 만든다.
- 장기 milestone 정의는 [ROADMAP.md](../ROADMAP.md)의 `M1~M10`을 canonical source로 따른다.

### 2. 현재 active milestone은 `M5. 하네스 장기 실행 증거 닫기`다

- `M2. 모드 로드 진입 증명`, `M3. 런타임 부트스트랩 가동`, `M4. Trusted Attempt 확보`는 최신 roots와 self-test 기준으로 materially closed 상태다.
- 현재 critical path는 더 이상 `startup / trust / accounting`이 아니다.
- 공식 next milestone은 `M6. Replay/Parity 회귀 게이트 고정`이다.
- 지금 구현 workstream은 `M5` 위에서 첫 advisor value slice를 닫는 것이다.

### 3. 지금 가장 중요한 표현은 "Phase 1~3과 honesty pass는 닫혔고, 이제 reward bundle 전에 first combat clearability를 막는 runtime-state blindspot을 줄여야 한다"이다

- latest roots는 current-execution runtime exporter / harness bridge / fresh snapshot / manual clean boot positive를 보여 준다.
- bootstrap launch는 이제 attempt가 아니라 session-level pre-attempt phase다.
- first authoritative attempt는 실제로 `0001 + trustStateAtStart:"valid"`로 시작한다.
- quartet semantics는 `restart-events` chronology와 projection들이 같은 이야기를 하도록 정렬됐다.
- Host hot path는 advice와 diagnostics가 이전보다 분리됐고, reward scene에는 deterministic middle layer가 들어갔다.
- reward scaffold는 honesty pass와 curated parity acceptance 강화까지 끝났다.
- 따라서 지금 필요한 것은 더 많은 startup diagnostic layer나 scaffold polishing이 아니라, first combat clearability를 막는 `selected / targeting / play finished` blindspot을 decompiled-backed runtime state로 줄이는 일이다.

### 4. Harness와 제품 경계를 계속 지킨다

- `Smoke Harness`는 dev-only validation tool이다.
- startup/trust/accounting 정리와 gameplay/advice 정책 변경을 섞지 않는다.
- valid-trust evidence 위에서 최소 advisor value slice를 정의하되, 그 전에 broad recommendation refactor로 새지 않는다.

### 5. observer/actuator가 state 판단에서 막히면 decompiled-backed 기준부터 찾는다

- scene authority 작업뿐 아니라 combat actuation blocker에서도 같은 원칙을 적용한다.
- screenshot heuristic이나 polling grace를 더 늘리기 전에, 먼저 `artifacts/knowledge/decompiled`에서 현재 막힌 상태를 직접 표현하는 runtime 기준이 있는지 찾는다.
- 예:
  - `card selected`
  - `card play pending`
  - `confirm ready`
  - `targeting in progress`
  - `selection mode vs play mode`
- 현재 combat 쪽에서는 `NPlayerHand.InCardPlay`, `_currentCardPlay`, `IsAwaitingPlay(...)`, `HoveredModelTracker` 같은 decompiled-backed 후보가 먼저 검토 대상이다.
- 현재 combat 쪽에서 추가로 중요한 후보는 `NTargetManager.IsInSelection`, `NTargetManager.HoveredNode`, `NCardPlay.Finished(success)`, `CombatHistory.CardPlayStarted/CardPlayFinished`다.
- 원칙:
  - `decompiled state criteria first`
  - `runtime export/hook second`
  - `screenshot heuristic last`

## 지금까지 실제로 전진한 일

### A. loader/runtime positive recovery

대표 root:

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)

확인된 점:

- `prevalidation.json`에서 `manualCleanBootVerified=true`
- `supervisor-state.json`에서 `trustState:"valid"`
- `startup-runtime-evidence.json`에서
  - `runtimeExporterInitializedLogged=true`
  - `harnessBridgeInitializeLogged=true`
  - `liveSnapshotPresent=true`

의미:

- direct loader debugging work unit은 현재 milestone 관점에서 충분히 성과를 냈다.
- 더 이상 `ModManager.TryLoadModFromPck(...)` branch debugging이 current critical path는 아니다.

### B. bootstrap-first sequencing closure

대표 root:

- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)

확인된 점:

- `prevalidation.json`에서 `manualCleanBootVerified=true`
- `attempt-index.ndjson` first entry가 `attemptId:"0001"`, `trustStateAtStart:"valid"`
- `restart-events.ndjson` first authoritative launch가 바로 `0001`
- `supervisor-state.json`에서 `trustState:"valid"`, `milestoneState:"done"`

의미:

- bootstrap은 attempt loop 밖 pre-phase로 분리됐다.
- `trustStateAtStart` 의미는 바꾸지 않고 first authoritative attempt를 valid-at-start로 만들었다.
- sequencing 자체는 현재 main blocker가 아니다.

### C. quartet/accounting contract closure

대표 root:

- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)

확인된 점:

- `restart-events.ndjson`는 chronology source로 읽는다.
- `attempt-index.ndjson`는 terminal summary projection으로 유지된다.
- `session-summary.json`의 `activeAttemptId`와 `supervisor-state.json`의 `expectedCurrentAttemptId`는 같은 chronology projection을 가리킨다.
- `lastAttemptId`는 canonical truth가 아니라 legacy alias로 내려갔다.

의미:

- quartet/accounting contract는 현재 구현의 blocker가 아니다.
- 다만 위 root의 tail은 수동 종료 개입이 섞였으므로 recovery/gameplay 해석 근거로는 쓰지 않는다.

### D. Host responsibility split / Foundation convergence

확인된 점:

- `CompanionHost` hot path는 advice orchestration 중심으로 가벼워졌고, collector/postmortem 쪽은 diagnostics service 뒤로 빠졌다.
- live advice path는 Host-local primary 구현보다 Foundation prompt/knowledge 경로를 primary로 사용한다.
- manual / retry-last / auto advice path와 collector mode on/off regression은 유지됐다.

의미:

- Host 과적재는 줄었고, 다음 deterministic middle layer를 꽂을 구조가 마련됐다.
- 다만 collector mode off에서도 diagnostics write 일부가 hot path에 남아 있는 것은 residual risk로 남는다.

### E. deterministic card reward layer

확인된 점:

- reward scene에는 deterministic `RewardOptionSet`, `RewardAssessmentFacts`, `RewardRecommendationTraceSeed`가 들어간다.
- live/replay path는 같은 deterministic reward context를 공유한다.
- scope correction 이후 non-card reward에서는 deterministic fields가 null fallback으로 유지된다.

의미:

- `card reward`는 이제 첫 advisor value slice 후보가 아니라 실제 구현된 pilot slice다.
- 남은 것은 deterministic context를 더 늘리는 일이 아니라 usefulness acceptance를 닫는 일이다.

### F. reward artifact honesty pass + curated parity tightening

확인된 점:

- finalizer는 minimal normalization contract로 줄었다.
- Host second finalization은 제거됐다.
- curated reward 3개 scenario는 self-test에서
  - prompt-pack parity
  - minimal-finalizer honesty
  - model-rationale readability
  - non-first-choice case
  를 같이 본다.

의미:

- reward scaffold는 이전보다 훨씬 정직해졌다.
- 하지만 이건 `usefulness 완료`가 아니라 `usefulness를 평가할 scaffold 준비 완료`다.

### G. gameplay safety 쪽 최근 전진은 유지되지만, current blocker는 first combat clearability다

- stale curse/status non-enemy promotion 차단
- enemy-turn closure
- repeated non-enemy stale loop 차단
- slot-4 combat no-op loop 대응
- replay-step / replay-test / self-test 유지

이 성과들은 계속 중요하지만, 현재 top blocker는 reward bundle formatting이 아니라 first combat clearability와 first card reward capture다.

## 현재 가장 중요한 문제

현재 주 병목은 `첫 전투 이후 non-enemy/self card selection-confirm-use cycle을 runtime truth 없이 screenshot heuristic으로만 추정한다`는 점이다.

핵심 의미는 아래와 같다.

- deterministic reward layer와 honesty scaffold는 이미 있다.
- 하지만 실제 live run은 첫 공격 뒤 `defend` 같은 non-enemy/self 카드의 선택/확정/소비 사이클을 안정적으로 닫지 못한다.
- reward reviewer bundle은 여전히 다음 중요한 목표지만, 지금은 그 장면 자체에 도달하는 것이 먼저다.

### 이 문제가 왜 중요한가

- validation-only trap에서 빠져나오려면 좁은 scene 하나에서 실제 조언 가치를 live evidence로 증명해야 한다.
- 그런데 지금은 그 scene까지 가는 live combat lane이 먼저 막혀 있다.
- 따라서 screenshot rule을 더 덧칠하기보다, decompiled-backed runtime state를 export하고 하네스가 그 state를 primary로 소비하게 만드는 것이 가장 큰 레버리지다.

## 다음 구현 계획

### Work Unit

`Combat Card-Play Runtime State Export + Harness Consumption`

### 목표

- combat card-play의 핵심 runtime state 3개를 decompiled-backed 기준으로 export한다.
  - `selected / play pending`
  - `targeting in progress`
  - `play finished`
- 하네스는 이 runtime state를 screenshot heuristic보다 우선 소비하게 만든다.
- 이번 세션의 직접 성공 정의는 fresh valid-trust run에서 첫 `card reward` screen을 최소 1건 확보하는 것이다.
- reward reviewer bundle closure는 이 work unit 다음으로 미룬다.

### source of truth

가장 먼저 읽을 것:

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- [verify-card-reward-reviewer-bundle-20260320-1545](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-card-reward-reviewer-bundle-20260320-1545)
- [verify-first-card-reward-capture-20260320-232002-run1](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-first-card-reward-capture-20260320-232002-run1)
- [RewardDeterministicBuilders.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/RewardDeterministicBuilders.cs)
- [AdvicePromptBuilder.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/AdvicePromptBuilder.cs)
- [ReplayAdvisorValidator.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Replay/ReplayAdvisorValidator.cs)
- [CompanionHost.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Host/CompanionHost.cs)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs)
- [RuntimeSnapshotReflectionExtractor.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs)
- decompiled reference:
  - `MegaCrit.Sts2.Core.Nodes.Combat.NPlayerHand`
  - `MegaCrit.Sts2.Core.Nodes.Combat.NCardPlay`
  - `MegaCrit.Sts2.Core.Nodes.Combat.NMouseCardPlay`
  - `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager`
  - `MegaCrit.Sts2.Core.Combat.History.CombatHistory`

### bounded 작업

1. decompiled-backed runtime 기준을 먼저 확정한다.
   - selection/pending: `NPlayerHand.InCardPlay`, `_currentCardPlay`, `IsAwaitingPlay(...)`
   - targeting: `NTargetManager.IsInSelection`, `HoveredNode`, `TargetingBegan/Ended`
   - finished: `NCardPlay.Finished(success)`, `CombatHistory.CardPlayStarted/CardPlayFinished`
2. snapshot export 또는 필요한 최소 hook로 위 state를 live observation에 노출한다.
3. 하네스는 `selected card / targeting / play finished` 판단에서 이 runtime state를 primary로 소비한다.
4. screenshot heuristic (`HasSelectedNonEnemyConfirmEvidence(...)` 등)은 fallback으로만 남긴다.
5. fresh valid-trust run에서 첫 공격 이후 non-enemy/self lane이 더 이상 old attack no-op carryover 때문에 immediately end-turn fallback으로 무너지지 않는지 본다.
6. bounded budget 안에서 첫 `card reward` screen 최소 1건 확보를 목표로 한다.
7. reward reviewer bundle closure는 이 work unit 종료 후 다음 세션으로 넘긴다.

### 금지

- startup/trust/accounting 재작업
- 새 startup diagnostic layer 추가
- broad combat policy refactor
- reward reviewer bundle schema 작업 재개
- event/shop/rest/combat deterministic layer 확장
- observer blindspot을 screenshot heuristic 덧칠만으로 해결하는 것

### validation loop

1. `dotnet build`
2. `dotnet run --project src/Sts2ModKit.SelfTest`
3. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
4. fresh valid-trust combat run(s)
5. first `card reward` capture 여부 확인
6. replay spot-check 유지:
   - `0015 --full-request-rebuild => combat select attack slot 4`
   - `0021 --full-request-rebuild => wait`

### acceptance

- `selected / targeting / play finished` runtime state가 live observation으로 드러난다.
- 하네스가 이 state를 primary로 소비해 non-enemy/self card cycle에서 false no-op / too-early end-turn를 줄인다.
- fresh valid-trust run에서 첫 `card reward` screen을 최소 1건 확보한다.
- reward 외 scene behavior를 불필요하게 넓게 바꾸지 않는다.

## 구현 백로그 우선순위

1. `Combat Card-Play Runtime State Export + Harness Consumption`
2. `First Card Reward Capture Reliability`
3. `Valid-Trust Card Reward Reviewer Bundle`
4. `Observer authority consumption contract 문서화`
5. `Reward 다음 scene class 선택(event / shop / rest)`

### 지금 backlog에 넣지 않을 것

- scenario split
- 새 startup diagnostic schema
- valid-trust reward bundle closure 전 다음 scene 확장
- 광범위한 recommendation engine 리팩터링

## 현재 읽어야 할 가장 중요한 artifact

### current pointer 확인용

- [PROJECT_STATUS.md](./PROJECT_STATUS.md)
- [docs/README.md](../README.md)

### startup/trust/accounting이 닫혔는지 확인용

- [restart-events.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/restart-events.ndjson)
- [attempt-index.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/attempt-index.ndjson)
- [session-summary.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/session-summary.json)
- [supervisor-state.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/supervisor-state.json)
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)

### runtime/bootstrap positive 이해용

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [startup-runtime-evidence.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700/startup-runtime-evidence.json)
- [prevalidation.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700/prevalidation.json)

### reward deterministic/value slice 이해용

- [RewardDeterministicBuilders.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/RewardDeterministicBuilders.cs)
- [AdvicePromptBuilder.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/AdvicePromptBuilder.cs)
- [ReplayAdvisorValidator.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Replay/ReplayAdvisorValidator.cs)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2ModKit.SelfTest/Program.cs)

## 중요한 guardrail

1. 더 이상 startup diagnostic layer를 늘리지 않는다.
2. bootstrap은 attempt가 아니라 session-level pre-attempt phase로 유지한다.
3. `trustStateAtStart`의 의미를 바꾸지 않는다.
4. chronology source와 projection을 섞어 읽지 않는다.
5. reward usefulness closure 전에는 reward 외 scene 확장을 하지 않는다.
6. 이번 work unit의 curated acceptance set은 3개 scenario로 고정한다.

## 새 세션 권장 시작 루프

### 참모 세션

1. latest authoritative root 읽기
2. blocker를 `valid-trust live reward bundle 부재` 한 줄로 압축
3. 구현 범위를 reward slice 안으로 고정
4. 구현 세션 프롬프트 승인

### 구현+테스트 세션

1. `dotnet build`
2. `dotnet run --project src/Sts2ModKit.SelfTest`
3. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
4. valid-trust live reward scenario 수집
5. scenario별 parity + usefulness reviewer bundle 작성
6. docs + bundle summary 갱신
7. bounded budget 안에서 acceptance 또는 partial closure 판정
