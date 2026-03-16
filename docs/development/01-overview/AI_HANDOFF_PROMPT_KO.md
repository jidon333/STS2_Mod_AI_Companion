# AI Handoff Prompt (KO)

## 현재 최우선 목표
- 현재 최우선 목표는 `GUI Smoke Harness`를 `screenshot-first` 기준으로 안정화하는 것이다.
- `Smoke Harness`의 1차 목적은 사람 대신 게임을 플레이하면서 `boot-to-long-run` completion과 observer 검증용 런타임 evidence를 함께 만들어내는 것이다.
- 따라서 행동 결정 authority는 여전히 `스크린샷 + AI 판단`에 있어야 한다.
- 다만 최신 상태에서 `Observer/export`는 더 이상 단순 `telemetry / post-check`에만 머물지 않는다. 현재는 `candidate generation`, `foreground/background 검증 입력`, `replay/inspect diagnostics` 쪽으로 점진 승격 중이다.

## 절대 원칙
1. `Smoke Harness`는 `black-box screenshot-driven tester`다.
2. `Observer/export`는 최종 `action authority`가 아니다.
3. `Smoke Harness`는 화면을 보고 진행하고, `Observer/export`는 그 결과를 기록하고 후보 생성과 검증 입력을 보조한다.
4. `Commander`와 최종 actuation 구조는 유지하되, 현재 개발의 critical path는 `Smoke Harness`다.
5. `runner / supervisor / stall sentinel`은 프로그램 런타임 역할이고, 분석 / 구현 / 테스트 에이전트는 개발 역할이다. 이 둘을 혼동하지 않는다.
6. `Observer/export`를 강화할 때는 반드시 실제 smoke run에서 나온 `screenshot + progress.ndjson + stall-diagnosis.ndjson + supervisor-state.json + state.latest.json + trace.ndjson`를 같이 본다.
7. `Manual Clean Boot`와 trust gate를 통과하지 못한 실행은 정상 gameplay evidence로 취급하지 않는다.

## 현재 아키텍처 상태 요약

- 전체 진척도는 `Overall 76%`다.
- 세부 수치는 `Evidence 90 / Observer 77 / Actuator 66 / Loop 63 / Ops 86`이다.
- 가장 강한 부분은 artifact 기반 판정과 운영 진단이다.
- 가장 약한 부분은 mixed-state에서 다음 행동을 안정적으로 선택하고, 잘못된 반복을 빨리 끝내는 능력이다.
- 현재 최신 blocker는 `reward/map` 자체보다는 그 다음 단계인 `event 뒤 map overlay mixed-state` 처리다.

## 현재 코드베이스 상태

### Observer 쪽
- `combat` truth는 `CombatManager.IsInProgress`를 기준으로 고정되어 있다.
- `state.latest.json.meta`에는 `logicalScreen`, `visibleScreen`, `flowScreen`가 함께 기록된다.
- foundation state model도 이 분리를 따라간다.
- exported cues는 이제 단순 truth export를 넘어 mixed-state 후보 생성과 validation 입력으로 쓰이고 있다.
- 따라서 현재 `Observer/export`의 가장 큰 문제는 combat truth가 아니라, `event / reward / map overlay mixed state`에서 foreground/background authority를 얼마나 정확히 보조하느냐에 가깝다.

### Smoke Harness 쪽
- 현재 핵심 파일은 [Program.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2GuiSmokeHarness\Program.cs) 이다.
- `Smoke Harness`는 현재 아래 primitive를 가진다.
  - `click`
  - `right-click`
  - `press-key`
  - screenshot capture
  - step artifact recording
  - human-readable console/log output
- 전투 입력 primitive는 실제 런타임에서 검증되었다.
  - 공격 카드: 선택 후 적 클릭
  - 방어 카드: 선택 후 같은 카드 다시 클릭
  - 선택 해제: `right-click`
  - 턴 종료: `E`
  - 카드 선택: 숫자키 가능
- stale decision bottleneck은 `provider=auto`에서 완화되었다.
- wait timing도 1차 조정이 끝났다.
  - passive wait: `1000ms`
  - decision wait minimum: `750ms`
  - action settle minimum: `900ms`
  - transition/black-frame settle: `2000ms`
- 운영/검증 명령도 강화되었다.
  - `inspect-session`
  - `replay-step`
  - `replay-test`

### 운영/검증 쪽
- `startup-trace.ndjson`와 `startup-summary.json`이 추가되어 deploy, launch, first-step startup 실패를 더 구조적으로 읽을 수 있다.
- deploy fast-path selection이 강화되어 preferred tool output 선택과 fallback 이유가 더 명확해졌다.
- golden scene regression suite가 추가되어 mixed-state scene selection 회귀를 오프라인으로 재현할 수 있다.

## 지금까지 실제로 한 일
1. `Observer`를 `combat truth` 기준으로 안정화했다.
2. `logicalScreen / visibleScreen / flowScreen` 분리를 observer와 foundation model에 반영했다.
3. `Smoke Harness`를 screenshot-first 방향으로 재정렬하기 시작했다.
4. 전투 승리까지 실제로 수행하면서 combat primitive를 검증했다.
5. human-readable smoke log와 `run.log`를 추가했다.
6. 창 이동 / 듀얼 모니터 / stale bounds 문제를 완화했다.
7. `combat enemy-targeting`을 fixed anchor 위주에서 live hitbox/export 기반으로 상당히 개선했다.
8. reward/map layered state, reward-map recovery, recovery window를 도입해 `reward/map` 병목을 전진시켰다.
9. latest-state sentinel과 `inspect-session` 재계산을 강화해 `reward-map-loop`, `map-transition-stall`, `combat-noop-loop` 분류를 더 잘 하게 만들었다.
10. replay diagnostics, golden scenes, startup tracing을 추가해 회귀 재현성과 운영 진단력을 높였다.

## 현재 가장 중요한 문제
- 현재 critical path는 `Observer`가 아니라 `Smoke Harness`의 `event/map overlay mixed-state progression`이다.
- 핵심 문제는 이제 "event choice를 못 읽는다"가 아니다.
- 실제 최신 blocker는 아래에 가깝다.
  - event가 여전히 foreground인데 map overlay가 background contamination으로 섞여 들어온다.
  - 이때 `current-node arrow`를 실제 reachable next node처럼 오인한다.
  - 그 결과 false map-advance 클릭이나 mixed-state loop가 발생한다.
- user-provided map rule을 반드시 우선 사용해야 한다.
  - 현재 노드 = 빨간 화살표가 가리키는 노드
  - 다음으로 갈 수 있는 노드 = 현재 노드와 점선으로 직접 연결된 인접 노드
  - 아이콘 진하기는 보조 신호일 뿐이다
- 남은 과제로는 `reward back`과 claimable reward extractor의 약점도 아직 있다.

## 지금 당장 해야 할 일
1. `HandleEvent`와 map overlay contamination 모델에서 foreground/background 판단을 더 강하게 만든다.
2. `current-node arrow`와 `reachable next node`를 명시적으로 분리한다.
3. `reward back`과 claimable reward fallback lane을 보강한다.
4. latest-state sentinel과 `inspect-session` 쪽 mixed-state loop terminalization을 계속 강화한다.
5. `replay-step`과 `replay-test` fixture를 늘려 mixed-state regressions를 오프라인으로 먼저 막는다.
6. live smoke session에서는 `startup-trace.ndjson`, `progress.ndjson`, `stall-diagnosis.ndjson`, `supervisor-state.json`를 묶어 검증한다.

## 권장 검증 루프
1. 회귀 의심 장면은 먼저 `replay-step`으로 단일 장면 후보와 suppression을 본다.
2. 관련 fixture가 있으면 `replay-test`로 golden scene suite를 돌린다.
3. 그 다음 실제 `boot-to-long-run` 세션을 실행한다.
4. 실패 시 `inspect-session`과 startup trace artifact로 latest-state와 startup path를 함께 재계산한다.
5. mixed-state 실패는 반드시 screenshot과 artifact를 같이 보고 판단한다.

## 주의사항
- `Smoke Harness`가 `Observer/export`의 stale state를 그대로 따라가면 안 된다.
- phase 판단도 screenshot-first여야 한다.
- `Observer/export`는 후속 로그 분석, candidate narrowing, foreground/background validation에 쓰되 최종 authority로 승격하면 안 된다.
- `screenBounds`나 exported hitbox가 있어도 그것이 자동으로 action authority가 되면 안 된다. action target은 screenshot 의미 판단이 먼저다.
- runtime 역할과 개발 역할을 섞어 쓰면 문서와 보고가 바로 흐려진다. 역할 구분은 [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\05-harness\RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)를 기준으로 맞춘다.

## 현재 작업 파일 주의
- 현재 작업 중 주 파일: [Program.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2GuiSmokeHarness\Program.cs)
- 아래 파일은 현재 worktree에 변경이 있으므로, 문맥 없이 덮어쓰지 말 것.
  - [RuntimeSnapshotReflectionExtractor.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\RuntimeSnapshotReflectionExtractor.cs)
  - [CompanionSceneNormalizer.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation\State\CompanionSceneNormalizer.cs)

## 현재 판단
- 지금은 대규모 리팩터링보다 `Smoke Harness` 플레이스루 안정화가 먼저다.
- `Observer/export`는 이미 중요한 truth source이자 보조 입력이 되었지만, final authority는 아직 아니다.
- 지금 남은 것은 `사람이 하던 테스트 플레이를 AI가 안정적으로 대신 수행`하는 개발 루프를 mixed-state와 loop terminalization까지 닫는 일이다.
