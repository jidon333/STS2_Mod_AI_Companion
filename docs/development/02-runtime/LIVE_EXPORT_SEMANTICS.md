# LIVE EXPORT 세만틱스

이 문서는 현재 exporter가 내보내는 파일의 의미를 정리합니다.

## 1. 출력 파일 4종

live root 아래의 canonical 출력은 아래 4개입니다.

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

## 2. `events.ndjson`

append-only 로그입니다. 각 줄은 하나의 이벤트 envelope입니다.

핵심 필드:

- `ts`
- `seq`
- `runId`
- `kind`
- `screen`
- `act`
- `floor`
- `payload`

규칙:

- `seq`는 단조 증가해야 합니다.
- 파서가 한 줄을 읽다가 실패하면 그 줄만 무시할 수 있어야 합니다.
- `payload`는 화면별 추가 정보가 들어가는 확장 필드입니다.

## 3. `state.latest.json`

외부 프로그램이 가장 신뢰해야 하는 machine-readable 최신 상태입니다.

대표 내용:

- 현재 화면
- act / floor
- 플레이어 상태
- 덱
- 유물
- 포션
- 현재 선택지
- 최근 변화
- 현재 조우 정보

이 파일은 latest snapshot이므로, 과거 히스토리는 `events.ndjson`로 봐야 합니다.

## 4. `state.latest.txt`

사람과 LLM이 바로 읽기 쉬운 정규화 요약입니다.

현재 고정 섹션:

- run status
- current screen / encounter
- player summary
- deck summary
- relics / potions
- current choices
- recent changes
- extraction warnings

이 파일은 완전한 진실 소스가 아니라 요약본입니다. 구조적 처리에는 JSON을 우선 사용합니다.

## 5. `session.json`

현재 run/session 단위 메타데이터입니다.

예:

- 현재 run id
- 마지막 갱신 시각
- live root
- 일부 exporter 상태

외부 host가 run 경계를 감지할 때 참고합니다.

## 6. trigger semantics

현재 advice trigger의 중심은 화면 경계와 선택지 경계입니다.

우선 대상:

- `choice-list-presented`
- `reward-screen-opened`
- `event-screen-opened`
- `shop-opened`
- `rest-opened`
- `combat-started`
- `turn-started`
- `map-node-entered`

일반 상태 변화는 debounce가 걸리고, 새 선택지가 등장한 경우는 빠르게 반응하는 쪽을 우선합니다.

## 7. polling과 hook의 관계

현재 exporter는 두 경로를 같이 씁니다.

- semantic hook
- scene polling fallback

의미:

- semantic hook가 잡히면 더 좋은 이벤트가 나옵니다.
- semantic hook가 아직 없더라도 polling으로 화면 분류와 snapshot 갱신을 유지합니다.

현재 검증된 baseline은 main menu까지입니다.

## 8. current choices 해석

`currentChoices`는 화면에 따라 의미가 달라집니다.

- reward: 카드/보상 선택지
- event: 이벤트 선택지
- shop: 구매/제거/서비스 선택지
- rest: 휴식/강화/기타 선택지

현재 목표는 각 고가치 화면에서 `label`, `value`, `description`, `kind`를 최대한 보존하는 것입니다.

## 9. degraded 상태

아래 상황은 degraded 또는 partial 상태로 봅니다.

- `events.ndjson`만 있고 `state.latest.json`이 갱신되지 않음
- deployed files는 있는데 live root가 없음
- runtime log가 없고 `godot.log`에 PatchAll failure가 있음
- main menu는 잡히지만 reward/event/shop/rest/combat가 계속 `unknown`

이 경우에는 기능 추가보다 먼저 startup / deploy / live root / hook summary를 확인해야 합니다.
