# Decompiled Source-First Observer Strategy

이 문서는 하네스 observer/scene authority 작업의 현재 기준 문서입니다.

핵심 결론은 두 가지입니다.

- 관측 모델은 `event/hook + polling` 혼합 구조다.
- 다음 하네스 observer 개선은 항상 `decompiled source-first`로 시작한다.

즉 polling을 버리는 것이 아니라, 먼저 decompiled 코드에서 쓸 만한 흐름과 메서드 후보를 찾고, 그 다음 런타임 hook/event와 polling을 각자 맞는 용도로 사용한다.

## 1. 왜 기준이 바뀌었는가

초기 advisor 단계에서는 polling 기반 snapshot과 heuristic scene inference만으로도 충분히 유용했다.

하지만 지금 하네스는 아래 성질이 필요하다.

- `scene transition`을 더 정확히 구분할 것
- `screen ready`를 intermediate state와 분리할 것
- 이후 `dispatch_node`를 다시 열 때 stale/transient inventory를 믿지 않을 것

따라서 observer의 우선순위도 바뀐다.

- 예전 초점: read-only advice를 위한 넓은 coverage
- 현재 초점: harness 검증과 actuation 준비를 위한 더 강한 scene authority

## 2. mixed observer 원칙

현재와 앞으로의 observer는 아래 혼합 구조로 본다.

### event / hook
- `scene transition`
- `screen ready`
- `lifecycle boundary`
- `authoritative state change` 검증

이런 경계 판단에는 event/hook가 더 효율적이고 신뢰도가 높다.

### polling
- continuous state snapshot
- reconciliation
- drift detection
- watchdog
- hook coverage가 없는 구간의 상태 보강

polling은 여전히 필요하다. 문제는 polling 자체가 아니라, transient polled scene만으로 `scene-ready`를 단정하면 안 된다는 점이다.

## 3. decompiled source-first 원칙

하네스 observer 또는 scene authority를 바꾸려면 먼저 `artifacts/knowledge/decompiled` 아래 decompiled 코드를 본다.

우선순위:

1. 어떤 메서드가 실제 scene 진입 / screen 준비 완료를 의미하는지 정적 흐름으로 찾는다.
2. 그 메서드가 broad global hook인지, 좁은 scene-specific candidate인지 분류한다.
3. 좁은 후보라면 runtime hook/event 대상으로 올린다.
4. polling은 그 authority를 보강하고 reconcile하는 계층으로 유지한다.

즉 `_Ready`, `_EnterTree`, `Open`, `Setup`, `ShowScreen`, `Refresh`를 blanket-ban하지 않는다. 대신 decompiled 흐름으로 좁힌 뒤 scene-specific candidate로 검토한다.

## 4. scene authority에 대한 현재 기준

현재 금지:

- transient `state.latest.json` scene만 보고 `scene-ready`라고 단정
- label heuristic만으로 scene boundary 확정
- stabilization이 약한 상태에서 `dispatch_node` 재개

현재 허용:

- event/hook + polling 혼합 observer
- decompiled-backed transition candidate 발굴
- polling 기반 snapshot을 observer-only triage와 reconciliation에 활용

한 줄 기준:

`scene authority should be backed by decompiled flow and runtime confirmation.`

## 5. 다음 분석 우선순위

먼저 볼 흐름:

1. `NMainMenu::_Ready`
2. `NMainMenu.SingleplayerButtonPressed`
3. `NSingleplayerSubmenu.OpenCharacterSelect`
4. `NCharacterSelectScreen.OnEmbarkPressed`

그 다음:

- rewards
- event
- shop
- rest

이 순서는 `main menu -> singleplayer submenu -> character select -> embark` 경계를 먼저 닫고, 그 다음 high-value gameplay screen의 ready/transition authority를 찾기 위함이다.

## 6. 구현 가드레일

- observer는 `event + polling mixed observer`로 유지한다.
- transition/ready 판단은 `transition-oriented hook`를 우선 검토한다.
- polling은 `continuous polling / reconciliation` 역할로 유지한다.
- transient polled scene은 그 자체로 `scene-ready`가 아니다.
- `dispatch_node`는 authoritative scene stabilization이 확보되기 전까지 닫아 둔다.