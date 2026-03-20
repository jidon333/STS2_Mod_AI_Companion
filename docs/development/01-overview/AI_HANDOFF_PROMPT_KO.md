# AI Handoff Prompt (KO)

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
- 장기 milestone 정의는 [ROADMAP.md](../../ROADMAP.md)의 `M1~M10`을 canonical source로 따른다.

### 2. 현재 active milestone은 `M4. Trusted Attempt 확보`다

- `M2. 모드 로드 진입 증명`과 `M3. 런타임 부트스트랩 가동`은 최신 fresh roots 기준으로 materially closed 상태다.
- 현재 critical path는 더 이상 `loader/runtime bring-up`이 아니다.
- 남은 주 병목은 `authoritative attempt / session accounting contract`다.
- 다음 milestone은 `M5. 하네스 장기 실행 증거 닫기`다.

### 3. 지금 가장 중요한 표현은 "bootstrap-first는 구현됐고, 남은 것은 accounting semantics hardening"이다

- latest roots는 current-execution runtime exporter / harness bridge / fresh snapshot / manual clean boot positive를 보여 준다.
- bootstrap launch는 이제 attempt가 아니라 session-level pre-attempt phase다.
- first authoritative attempt는 실제로 `0001 + trustStateAtStart:"valid"`로 시작한다.
- 따라서 지금 필요한 것은 더 많은 startup diagnostic layer가 아니라 chronology/projection contract를 더 선명하게 만드는 일이다.

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
- sequencing 자체보다 남은 건 accounting contract clarity다.

### C. gameplay safety 쪽 최근 전진은 유지되지만 current blocker는 아니다

- stale curse/status non-enemy promotion 차단
- enemy-turn closure
- repeated non-enemy stale loop 차단
- slot-4 combat no-op loop 대응
- replay-step / replay-test / self-test 유지

이 성과들은 계속 중요하지만, 현재 top blocker를 대체하지는 않는다.

## 현재 가장 중요한 문제

현재 주 병목은 `bootstrap-first 이후 authoritative attempt / session accounting contract`다.

핵심 의미는 아래와 같다.

- `restart-events.ndjson`는 append-only chronology source처럼 동작한다.
- `attempt-index.ndjson`는 terminal attempt summary projection에 가깝다.
- `session-summary.json`는 reviewer-facing aggregate projection이다.
- `supervisor-state.json`는 machine verdict projection이다.

즉 지금 남은 문제는 "현재 실행에서 아무것도 안 보인다"가 아니다.

현재 남은 문제는:

- reviewer가 current attempt와 terminal attempt를 한 번에 읽을 수 있도록 계약이 충분히 선명한가
- `lastAttemptId` 같은 legacy/ambiguous field를 어떻게 해석해야 하는가
- chronology source와 projection들의 관계가 docs와 self-test에서 일관되게 닫히는가

### 이 문제가 왜 중요한가

- `M4`를 깔끔하게 닫으려면 first authoritative attempt가 valid로 시작하는 것만으로는 부족하다.
- operator와 reviewer가 동일한 artifact set에서 동일한 결론을 읽어야 한다.
- 이 contract가 흐리면 `M5` long-run evidence closure도 다시 해석 비용이 커진다.

## 다음 구현 계획

### Work Unit

`Authoritative Attempt / Session Accounting Contract Hardening`

### 목표

- `restart-events.ndjson`, `attempt-index.ndjson`, `session-summary.json`, `supervisor-state.json`의 truth contract를 코드와 self-test로 고정한다.
- bootstrap-first 이후 reviewer가 `current attempt`, `last terminal attempt`, `restart target`, `milestone evidence`를 한 번에 해석할 수 있게 만든다.
- schema/file set을 더 늘리지 않고 semantics만 선명하게 만든다.

### source of truth

가장 먼저 읽을 것:

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- [Program.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/Program.cs)
- [LongRunArtifacts.Supervision.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2GuiSmokeHarness/LongRunArtifacts.Supervision.cs)
- [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
- [STARTUP_DEPLOY_CONTROL_LAYER.md](../05-harness/STARTUP_DEPLOY_CONTROL_LAYER.md)

### bounded 작업

1. `restart-events.ndjson`를 canonical chronology source로 고정한다.
2. `attempt-index.ndjson`는 authoritative attempt당 terminal summary projection으로 유지한다.
3. `session-summary.json`은 chronology + attempt projection에서만 파생되도록 정리한다.
4. `supervisor-state.json`는 machine verdict projection으로 유지하되,
   - `expectedCurrentAttemptId`
   - `lastTerminalAttemptId`
   - `latestRestartTargetAttemptId`
   - `latestNextAttemptId`
   를 canonical field로 취급한다.
5. `lastAttemptId`는 wire shape는 유지하되 legacy/ambiguous field로 문서와 reviewer guidance에서 격하한다.
6. chronology -> projection derivation helper를 한 곳으로 모아 `session-summary`와 `supervisor-state`가 같은 해석을 쓰게 만든다.
7. docs와 self-test를 같이 갱신해 contract drift를 막는다.

### 금지

- 새 startup diagnostic layer 추가
- 새 timeline/sentinel/schema field 추가
- 별도 bootstrap-validation scenario split
- gameplay / HandleCombat / observer heuristic / trust threshold 변경
- `reward advisor`를 첫 value slice로 성급히 확정
- broad recommendation engine refactor

### validation loop

1. `dotnet build`
2. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
3. fresh `boot-to-long-run` live run 1회
4. 아래 확인:
   - first authoritative attempt는 `0001`
   - `trustStateAtStart:"valid"`
   - bootstrap launch는 attempt accounting에 포함되지 않음
   - `session-summary.json`과 `supervisor-state.json`가 current/terminal attempt를 서로 다르게 오독하지 않음
5. replay spot-check 유지:
   - `0015 --full-request-rebuild => combat select attack slot 4`
   - `0021 --full-request-rebuild => wait`

### acceptance

- fresh root에서 first authoritative attempt는 `0001 + valid-at-start`
- `restart-events.ndjson` 기준 chronology와 `attempt-index/session-summary/supervisor-state` projection이 reviewer 관점에서 모순되지 않음
- schema/file set 증설 없이 위 semantics가 self-test와 live root 둘 다에서 유지됨

## 구현 백로그 우선순위

1. `Authoritative Attempt / Session Accounting Contract Hardening`
2. `Observer authority class contract 문서화`
3. `Minimum advisor value slice 후보 선정`
4. `State / Knowledge / Recommendation skeleton 설계`

### 지금 backlog에 넣지 않을 것

- scenario split
- 새 startup diagnostic schema
- 첫 advisor slice를 reward/shop/event 중 하나로 단정
- 광범위한 recommendation engine 리팩터링

## 현재 읽어야 할 가장 중요한 artifact

### current pointer 확인용

- [PROJECT_STATUS.md](./PROJECT_STATUS.md)
- [CURRENT_MILESTONE_CHECKLIST.md](./CURRENT_MILESTONE_CHECKLIST.md)

### accounting contract 이해용

- [restart-events.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/restart-events.ndjson)
- [attempt-index.ndjson](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/attempt-index.ndjson)
- [session-summary.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/session-summary.json)
- [supervisor-state.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044/supervisor-state.json)

### runtime/bootstrap positive 이해용

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [startup-runtime-evidence.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700/startup-runtime-evidence.json)
- [prevalidation.json](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700/prevalidation.json)

## 중요한 guardrail

1. 더 이상 startup diagnostic layer를 늘리지 않는다.
2. bootstrap은 attempt가 아니라 session-level pre-attempt phase로 유지한다.
3. `trustStateAtStart`의 의미를 바꾸지 않는다.
4. chronology source와 projection을 섞어 읽지 않는다.
5. gameplay/advice 정책 변경은 `M4` contract hardening과 분리한다.

## 새 세션 권장 시작 루프

### 참모 세션

1. latest authoritative root 읽기
2. blocker를 `session/accounting contract` 한 줄로 압축
3. 구현 범위를 chronology/projection hardening 안으로 고정
4. 구현 세션 프롬프트 승인

### 구현+테스트 세션

1. `dotnet build`
2. `dotnet run --project src/Sts2GuiSmokeHarness -- self-test`
3. chronology/projection derivation 수정
4. docs + self-test 동시 갱신
5. fresh live run 1회
6. accounting contract 기준으로 acceptance 판정
