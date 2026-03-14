# AI Handoff Prompt (KO)

## 현재 최우선 목표
- 현재 최우선 목표는 `GUI Smoke Harness`를 `screenshot-first` 기준으로 안정화하는 것이다.
- `Smoke Harness`의 1차 목적은 사람 대신 게임을 플레이하면서 `Observer` 검증용 런타임 로그를 만들어내는 것이다.
- 따라서 행동 결정 authority는 `스크린샷 + AI 판단`에 있어야 하고, `Observer`는 `telemetry / post-check / internal truth export`로만 사용해야 한다.

## 절대 원칙
1. `Smoke Harness`는 `black-box screenshot-driven tester`다.
2. `Observer`는 `action authority`가 아니다.
3. `Smoke Harness`는 화면을 보고 진행하고, `Observer`는 그 결과를 기록하고 사후 검증 대상이 된다.
4. `Commander`와 최종 actuation 구조는 유지하되, 현재 개발의 critical path는 `Smoke Harness`다.
5. `Observer`를 강화할 때는 반드시 실제 smoke run에서 나온 `screenshot + state.latest.json + events.ndjson + inventory.latest.json + trace.ndjson`를 같이 본다.

## 현재 코드베이스 상태

### Observer 쪽
- `combat` truth는 `CombatManager.IsInProgress`를 기준으로 고정되어 있다.
- `state.latest.json.meta`에는 `logicalScreen`, `visibleScreen`, `flowScreen`가 함께 기록된다.
- foundation state model도 이 분리를 따라간다.
- 따라서 현재 `Observer`의 가장 큰 문제는 combat truth가 아니라, smoke run을 따라가며 `room / branch / reward-map mixed state`를 얼마나 잘 기록하는가이다.

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

## 지금까지 실제로 한 일
1. `Observer`를 `combat truth` 기준으로 안정화했다.
2. `logicalScreen / visibleScreen / flowScreen` 분리를 observer와 foundation model에 반영했다.
3. `Smoke Harness`를 screenshot-first 방향으로 재정렬하기 시작했다.
4. 전투 승리까지 실제로 수행하면서 combat primitive를 검증했다.
5. human-readable smoke log와 `run.log`를 추가했다.
6. 창 이동 / 듀얼 모니터 / stale bounds 문제를 완화했다.
7. reward-map mixed state가 실제 게임 구조상 가능하다는 점을 decompiled code와 runtime evidence로 반영했다.

## 현재 가장 중요한 문제
- 현재 critical path는 `Observer`가 아니라 `Smoke Harness`의 `visible map -> next room progression`이다.
- 특히 `event -> visible map -> next room` 구간에서 screenshot 기준 다음 진행 노드를 안정적으로 고르는 것이 아직 완전히 닫히지 않았다.
- user-provided map rule을 반드시 우선 사용해야 한다.
  - 현재 노드 = 빨간 화살표가 가리키는 노드
  - 다음으로 갈 수 있는 노드 = 현재 노드와 점선으로 직접 연결된 인접 노드
  - 아이콘 진하기는 보조 신호일 뿐이다

## 지금 당장 해야 할 일
1. `Smoke Harness`에서 `Observer` 기반 branching을 더 줄인다.
2. `ChooseFirstNode`와 `HandleEvent`에서 `visible map` 판단과 `직접 연결 노드` 탐지를 더 정확히 만든다.
3. 보물상자 화면은 `가운데 클릭` 규칙을 넣는다.
4. `rewards -> map -> next combat -> rewards`를 사람 개입 없이 한 번 더 완주한다.
5. 그 결과 로그로 `Observer`의 reward/map/event/shop/rest fidelity를 보강한다.

## 주의사항
- `Smoke Harness`가 `Observer`의 stale state를 따라가면 안 된다.
- phase 판단도 screenshot-first여야 한다.
- `Observer`는 후속 로그 분석과 internal truth 확인에만 쓴다.
- `screenBounds`가 있어도 그것이 action authority가 되면 안 된다. action target은 screenshot 의미 판단이 먼저다.

## 현재 작업 파일 주의
- 현재 작업 중 주 파일: [Program.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2GuiSmokeHarness\Program.cs)
- 아래 파일은 현재 worktree에 변경이 있으므로, 문맥 없이 덮어쓰지 말 것.
  - [RuntimeSnapshotReflectionExtractor.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\RuntimeSnapshotReflectionExtractor.cs)
  - [CompanionSceneNormalizer.cs](C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation\State\CompanionSceneNormalizer.cs)

## 현재 판단
- 지금은 대규모 리팩터링보다 `Smoke Harness` 플레이스루 안정화가 먼저다.
- `Observer`는 이미 중요한 truth source를 확보했다.
- 지금 남은 것은 `사람이 하던 테스트 플레이를 AI가 안정적으로 대신 수행`하는 개발 루프를 닫는 일이다.