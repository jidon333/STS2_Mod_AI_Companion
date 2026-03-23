# AI 인수인계 프롬프트 (KO)

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 다음 구현 작업 단위, guardrail, 또는 acceptance 초점이 바뀔 때

## 문서 목적

이 문서는 새 `참모 세션`과 새 `구현+테스트 세션`이 지금 상태를 빠르게 이어받기 위한 인수인계 문서다.

이 문서가 답해야 하는 질문은 네 가지다.

1. 지금 프로젝트의 실제 방향성은 무엇인가
2. 지금까지 무엇을 해결했고 무엇은 아직 안 끝났는가
3. 다음 세션이 바로 손대야 할 가장 중요한 문제는 무엇인가
4. 어떤 검증 루프로 판단해야 false success를 피할 수 있는가

중요:

- 이 문서는 현재 시점의 작업 인수인계 문서다.
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

### 참모/구현 세션 운영 원칙

- 애매한 runtime 문제를 만났을 때는 timer-based suppression이나 recapture-count 억제를 먼저 제안하지 않는다.
- 먼저 `artifacts/knowledge/decompiled`와 current runtime export에서 room-scoped / screen-scoped explicit state를 찾는다.
- 특히 modal aftermath 문제에서는 `visible/open`과 foreground ownership을 구분한다.
- local inspection만으로 root cause가 충분히 좁혀지지 않으면, 구현 방향을 고정하기 전에 서브에이전트와 교차검증한다.
- 임시 억제보다 explicit state machine을 우선한다.

## 현재 방향성

### 1. 장기 목표는 여전히 읽기 전용 advisor 제품이다

- 전체 목표는 `1단계: 외부 프로세스 AI 조언 어시스턴트 완성`이다.
- 최종적으로는 사람이 실제 플레이 중 참고 가능한 `읽기 전용 advisor`를 만든다.
- 장기 milestone 정의는 [ROADMAP.md](../ROADMAP.md)의 `M1~M10`을 canonical source로 따른다.

### 2. 현재 제품 workstream은 이제 `M6 -> M7 -> M8 평가`다

- `M2. 모드 로드 진입 증명`, `M3. 런타임 부트스트랩 가동`, `M4. Trusted Attempt 확보`는 materially closed 상태다.
- product sequencing 관점에서는 `M5`를 여기서 닫고 다음 평가 단계로 넘어간다.
- 공식 현재 마일스톤 위치는 이제 `M6. Replay/Parity 회귀 게이트 고정`으로 읽는다.
- 그 다음은 `M7. 비전투 진행 안정화`, `M8. 전투 안정화`를 차례로 평가하고, 그 결과 위에서 `M9` 준비를 시작한다.

중요한 구분:

- ROADMAP과 long-run lifecycle contract 문서가 정의하는 엄격한 `M5 done`은 아직 아니다.
- 엄격한 lifecycle semantics는 여전히:
  - `terminal`
  - `restart`
  - `next attempt first screen`
  체인을 요구한다.
- 지금은 이 엄격한 gap을 `residual lifecycle automation gap`으로 분리 추적하고, 현재 제품 workstream은 `M6~M8` 평가로 이동한다.

### 3. 지금 가장 중요한 표현은 "first combat clearability blocker는 더 이상 현재 최상위 blocker가 아니다"이다

- 최신 trusted roots는 이미:
  - repeated combat clear
  - repeated reward/card reward capture
  - reward -> map -> exported node
  - shop -> map handoff
  - elite combat 진입
  - natural terminal boundary
  까지 갔다.
- 즉 지금 문서에 남아 있는 낡은 표현인
  - `first combat clearability`
  - `first card reward capture`
  는 더 이상 top blocker로 두면 안 된다.
- 대신 지금 해야 할 일은:
- `M6`에서 parity gate를 정식 audit하고
  - `M7`에서 남은 non-combat mixed-state risks를 정리하고
  - `M8`에서 현재 combat stability를 어디까지 닫을지 평가하고
  - `M9` 준비팩을 정의하는 것이다.

### 4. Harness와 제품 경계를 계속 지킨다

- `Smoke Harness`는 개발용 validation tool이다.
- startup/trust/accounting 정리와 gameplay/advice 정책 변경을 섞지 않는다.
- strict harness lifecycle semantics와 product milestone sequencing을 구분한다.
- broad recommendation refactor는 아직 아니다.

### 5. observer/actuator가 state 판단에서 막히면 decompiled-backed 기준부터 찾는다

- scene authority 작업뿐 아니라 combat actuation, modal aftermath, mixed-state ownership에서도 같은 원칙을 적용한다.
- screenshot heuristic이나 polling grace를 더 늘리기 전에, 먼저 `artifacts/knowledge/decompiled`에서 현재 막힌 상태를 직접 표현하는 runtime 기준이 있는지 찾는다.
- 원칙:
  - `decompiled state criteria first`
  - `runtime export/hook second`
  - `screenshot heuristic last`

## 지금까지 실제로 전진한 일

### A. loader/runtime positive recovery

대표 root:

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)

의미:

- 현재 실행 기준으로 runtime exporter / harness bridge / fresh snapshot positive를 다시 확보했다.
- direct loader debugging은 이제 current critical path가 아니다.

### B. bootstrap-first sequencing closure

대표 root:

- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)

의미:

- bootstrap은 pre-attempt phase로 분리됐다.
- first authoritative attempt `0001`은 `trustStateAtStart:"valid"`로 시작한다.
- sequencing 자체는 current blocker가 아니다.

### C. quartet/accounting contract closure

대표 root:

- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)

의미:

- `restart-events.ndjson`는 chronology source다.
- `attempt-index.ndjson`, `session-summary.json`, `supervisor-state.json`는 projection이다.
- quartet semantics는 더 이상 current blocker가 아니다.

### D. ancient/map mixed-state closure

대표 root:

- [verify-ancient-ownership-normalization-continue-20260323-2157](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-ancient-ownership-normalization-continue-20260323-2157)

의미:

- ancient completion 이후 `foregroundOwner=map`, `choiceExtractorPath=map`, exported node click contract가 valid root로 닫혔다.
- old `ChooseFirstNode -> HandleEvent` reopen problem은 현재 최상위 blocker가 아니다.

### E. harness speed-up closure

대표 root:

- [verify-speedup-continue-20260323-2230](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-speedup-continue-20260323-2230)

의미:

- dead time reduction은 valid root로 확인됐다.
- speed-up은 current evidence에서 regression으로 보이지 않는다.

### F. long-run gameplay continuity closure

대표 roots:

- [verify-long-run-speedup-continue-20260323-2249](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2249)
- [verify-long-run-speedup-continue-20260323-2301](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301)

의미:

- repeated combat clear
- repeated reward/card reward capture
- reward/map/shop/combat continuity
- old 120-step ceiling 돌파
- natural `player-defeated` terminal boundary

까지 valid root로 확보됐다.

즉 `M5`를 계속 first combat blocker 문구에 묶어 둘 이유가 줄어들었다.

### G. deterministic reward/advice scaffold는 계속 중요하지만, current blocker는 아니다

- deterministic reward layer
- honesty pass
- curated parity scaffold

는 여전히 `M9` 준비의 핵심 기반이다.

다만 지금은 이 scaffold를 더 다듬는 것보다, `M6~M8`를 evidence 기준으로 평가하는 것이 먼저다.

## 현재 가장 중요한 문제

현재 가장 중요한 문제는 “새 runtime blindspot 하나를 바로 구현할 것인가”가 아니다.

현재 가장 중요한 문제는:

1. `M6`를 정식으로 마감할 수 있는가
2. `M7`을 이미 실질적으로 닫힌 것으로 볼 수 있는가, 아니면 generic event/modal/overlay/rest aftermath가 아직 활성 위험인가
3. `M8`를 어디까지 닫았고, elite defeat evidence를 어떻게 해석할 것인가
4. 그 위에서 `M9`를 어떤 scene set과 어떤 acceptance band로 시작할 것인가

즉 지금은 implementation-first보다 **평가와 순서 정리가 먼저**다.

## 현재 남아 있는 잔여 gap

엄격한 harness lifecycle semantics 기준 잔여 gap은 아래다.

- terminal 이후 restart 자동화
- restart 이후 next attempt first screen evidence

최신 natural terminal root [verify-long-run-speedup-continue-20260323-2301](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301) 기준으로:

- [supervisor-state.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301/supervisor-state.json)
  - `milestoneState = "failed"`
  - `blockers = ["restart-missing-after:0001", "session-aborted-before-milestone"]`

이 gap은 중요하지만, 현재 최상위 blocker를 다시 `M5`에 고정하는 대신 잔여 lifecycle 후속 작업으로 추적한다.

## 다음 세션의 기본 규칙

1. 현재 제품 workstream은 `M6 -> M7 -> M8 평가`다.
2. `M5 strict lifecycle done`과 `M5 product closeout`을 혼동하지 않는다.
3. 현재 문서에서 `first combat clearability`를 top blocker로 되살리지 않는다.
4. ancient/map/speed-up closure는 regression evidence가 없으면 다시 열지 않는다.
5. 엄격한 terminal/restart continuity는 별도 lifecycle automation 작업으로 추적한다.

## 다음 작업 축

### 작업 단위 A

`M6 Replay/Parity Closeout Audit`

목표:

- 최신 ownership/speed-up work 이후 parity-sensitive request가 여전히 rebuild / non-rebuild semantic alignment를 유지하는지 정식 audit

### 작업 단위 B

`M7 Non-Combat Stability Evaluation`

목표:

- generic event / modal / overlay / rest aftermath가 실제 활성 blocker인지, 아니면 이미 수용 가능한 잔여 문제인지 평가

### 작업 단위 C

`M8 Combat Stability Evaluation`

목표:

- current combat safety와 combat quality를 분리해서 읽고
- repeated clear roots와 elite defeat root를 바탕으로 `closed / partial / open` 판정을 내린다

### 작업 단위 D

`M9 Readiness Pack Definition`

목표:

- advice를 평가할 representative scene set
- acceptance artifact bundle
- honesty/usefulness 기준
- sabotage 금지선

을 current docs에 고정

## 기준 자료

가장 먼저 읽을 것:

- [docs/current/PROJECT_STATUS.md](./PROJECT_STATUS.md)
- [ROADMAP.md](../ROADMAP.md)
- [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- [verify-ancient-ownership-normalization-continue-20260323-2157](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-ancient-ownership-normalization-continue-20260323-2157)
- [verify-speedup-continue-20260323-2230](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-speedup-continue-20260323-2230)
- [verify-long-run-speedup-continue-20260323-2249](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2249)
- [verify-long-run-speedup-continue-20260323-2301](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301)

## 하지 말아야 할 것

- `first combat clearability` stale diagnosis를 현재 최상위 blocker로 재사용
- 엄격한 lifecycle gap 하나 때문에 `M6~M8` 평가 전체를 멈추는 것
- regression evidence 없이 ancient/map/speed-up rail을 다시 뜯는 것
- decompiled/runtime state보다 timer-based suppression을 먼저 늘리는 것
- `M9` readiness criteria 없이 advice usefulness 논의를 먼저 확장하는 것
