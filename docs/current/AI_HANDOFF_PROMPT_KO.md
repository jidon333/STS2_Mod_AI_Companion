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

### 3. 지금 가장 중요한 표현은 "Phase 1~3은 닫혔고, 이제 card reward advisor usefulness closure로 넘어간다"이다

- latest roots는 current-execution runtime exporter / harness bridge / fresh snapshot / manual clean boot positive를 보여 준다.
- bootstrap launch는 이제 attempt가 아니라 session-level pre-attempt phase다.
- first authoritative attempt는 실제로 `0001 + trustStateAtStart:"valid"`로 시작한다.
- quartet semantics는 `restart-events` chronology와 projection들이 같은 이야기를 하도록 정렬됐다.
- Host hot path는 advice와 diagnostics가 이전보다 분리됐고, reward scene에는 deterministic middle layer가 들어갔다.
- 따라서 지금 필요한 것은 더 많은 startup diagnostic layer가 아니라 `card reward` 조언의 실제 참고 가치를 닫는 일이다.

### 4. Harness와 제품 경계를 계속 지킨다

- `Smoke Harness`는 dev-only validation tool이다.
- startup/trust/accounting 정리와 gameplay/advice 정책 변경을 섞지 않는다.
- valid-trust evidence 위에서 최소 advisor value slice를 정의하되, 그 전에 broad recommendation refactor로 새지 않는다.

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

### F. gameplay safety 쪽 최근 전진은 유지되지만 current blocker는 아니다

- stale curse/status non-enemy promotion 차단
- enemy-turn closure
- repeated non-enemy stale loop 차단
- slot-4 combat no-op loop 대응
- replay-step / replay-test / self-test 유지

이 성과들은 계속 중요하지만, 현재 top blocker를 대체하지는 않는다.

## 현재 가장 중요한 문제

현재 주 병목은 `valid-trust evidence 위의 card reward advisor usefulness closure`다.

핵심 의미는 아래와 같다.

- deterministic layer는 들어갔지만, 아직 curated reward scenes에서 reviewer가 artifact만 보고 “실제로 참고 가치 있다”를 닫는 acceptance가 없다.
- live/replay parity는 deterministic context alignment 수준에서는 좋아졌지만, usefulness 기준이 아직 product acceptance로 고정되지 않았다.
- reward 외 scene으로 확장하기 전에, `card reward` 하나를 실제 가치 슬라이스로 닫아야 한다.

### 이 문제가 왜 중요한가

- validation-only trap에서 빠져나오려면 좁은 scene 하나에서 실제 조언 가치를 증명해야 한다.
- `card reward`는 deterministic option/facts/trace가 이미 있으므로, 지금 가장 싸게 제품 가치를 확인할 수 있는 장면이다.
- 이 슬라이스를 닫아야 reward 다음 scene 확장도 heuristic 덧칠이 아니라 구조 확장으로 갈 수 있다.

## 다음 구현 계획

### Work Unit

`Card Reward Advisor Value Closure`

### 목표

- valid-trust evidence 위의 `card reward` 장면 3~5개에서, reward advice artifact만으로 reviewer가 실제 참고 가치를 판정할 수 있게 만든다.
- 이미 들어간 deterministic `option set / assessment facts / trace`를 제품 acceptance로 연결한다.
- reward 외 scene은 건드리지 않고, reward slice 하나만 닫는다.

### source of truth

가장 먼저 읽을 것:

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- [RewardDeterministicBuilders.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/RewardDeterministicBuilders.cs)
- [AdvicePromptBuilder.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Reasoning/AdvicePromptBuilder.cs)
- [ReplayAdvisorValidator.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Foundation/Replay/ReplayAdvisorValidator.cs)
- [CompanionHost.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Host/CompanionHost.cs)

### bounded 작업

1. curated `card reward` fixtures/scenes 3~5개를 고른다.
2. reward advice artifact가 아래를 일관되게 채우도록 한다.
   - `recommendedChoiceLabel`
   - `reasoningBullets`
   - `missingInformation`
   - `decisionBlockers`
   - `RewardRecommendationTrace`
3. deterministic `RewardOptionSet / RewardAssessmentFacts / RewardRecommendationTraceSeed`를 reviewer가 읽기 쉬운 형태로 유지하거나 노출한다.
4. live/replay reward path에서 같은 deterministic option labels / fact lines / recommendation output을 비교 가능하게 만든다.
5. reward usefulness acceptance를 self-test/replay/live artifact 기준으로 닫는다.
6. reward 외 scene은 기존 path 그대로 유지한다.

### 금지

- startup/trust/accounting 재작업
- 새 startup diagnostic layer 추가
- observer heuristic 변경
- Host diagnostics 재분리 재논쟁
- event/shop/rest/combat deterministic layer 확장
- broad recommendation engine refactor
- reward 다음 scene을 이번 세션에 같이 열기

### validation loop

1. `dotnet build`
2. `dotnet run --project src/Sts2ModKit.SelfTest`
3. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
4. curated reward live/replay check
5. replay spot-check 유지:
   - `0015 --full-request-rebuild => combat select attack slot 4`
   - `0021 --full-request-rebuild => wait`

### acceptance

- curated `card reward` scenes에서 advice artifact만 보고 reviewer가 usefulness를 판정할 수 있다.
- `recommendedChoiceLabel`은 항상 visible option set 안에 있다.
- deterministic reward context와 advice output이 live/replay에서 비교 가능하다.
- reward 외 scene behavior는 변하지 않는다.

## 구현 백로그 우선순위

1. `Card Reward Advisor Value Closure`
2. `Observer authority consumption contract 문서화`
3. `Reward 다음 scene class 선택(event / shop / rest)`
4. `State / Knowledge / Recommendation architecture 확장`

### 지금 backlog에 넣지 않을 것

- scenario split
- 새 startup diagnostic schema
- reward usefulness closure 전 다음 scene 확장
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

## 중요한 guardrail

1. 더 이상 startup diagnostic layer를 늘리지 않는다.
2. bootstrap은 attempt가 아니라 session-level pre-attempt phase로 유지한다.
3. `trustStateAtStart`의 의미를 바꾸지 않는다.
4. chronology source와 projection을 섞어 읽지 않는다.
5. reward usefulness closure 전에는 reward 외 scene 확장을 하지 않는다.

## 새 세션 권장 시작 루프

### 참모 세션

1. latest authoritative root 읽기
2. blocker를 `card reward usefulness closure` 한 줄로 압축
3. 구현 범위를 reward slice 안으로 고정
4. 구현 세션 프롬프트 승인

### 구현+테스트 세션

1. `dotnet build`
2. `dotnet run --project src/Sts2ModKit.SelfTest`
3. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
4. reward fixture / parity / artifact usefulness 개선
5. docs + self-test 동시 갱신
6. reward usefulness 기준으로 acceptance 판정
