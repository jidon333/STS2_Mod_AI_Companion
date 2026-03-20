# 미완 훅과 리스크

> Status: Snapshot
> Source of truth: No
> This is a point-in-time risk list. Current blocker and next work live in [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md) and [AI_HANDOFF_PROMPT_KO.md](../../current/AI_HANDOFF_PROMPT_KO.md).

## 1. 아직 남아 있는 핵심 미완 작업

### 고가치 화면 실증

아직 실제 live smoke로 확실히 닫아야 하는 화면:

- reward
- event
- shop
- rest
- combat start
- turn start

main menu baseline은 확보했지만, 조언 가치가 높은 화면과 하네스에서 중요한 전이 화면은 아직 모두 authoritative하게 닫히지 않았다.

### choice 추출

`currentChoices`는 구조가 이미 있지만, 실제 gameplay 화면에서 얼마나 안정적으로 채워지는지는 추가 검증이 필요하다.

## 2. broad hook에 대한 현재 원칙

다음 계열은 전역 wildcard 방식으로 붙이지 않는다.

- `_Ready`
- `_EnterTree`
- `Show`
- `Refresh`
- `Open`
- `Setup`

하지만 이것은 blanket-ban이 아니다.

현재 원칙:

- broad global hook는 금지
- decompiled source에서 좁혀진 scene-specific candidate는 우선 검토
- 하네스 단계에서는 `scene transition`, `screen ready`, `lifecycle boundary` 판단에 쓸 수 있는 좁은 후보를 먼저 찾는다

즉 `_Ready`, `_EnterTree`, `Open`, `Setup`, `ShowScreen`, `Refresh`는 위험해서 영구 제외하는 것이 아니라, decompiled 흐름으로 좁혀서 검증해야 하는 후보군이다.

## 3. 기술 리스크

### 1. runtime-confirmed whitelist 유지 비용

안전하게 가려면 확인된 메서드만 추가해야 한다. 이 방식은 안전하지만 coverage 확장이 느릴 수 있다.

### 2. observer authority refinement 필요

현재 observer는 `event + polling` 혼합 구조다.

장점:

- polling이 continuous state snapshot과 reconciliation에 유용하다
- hook/event는 scene transition과 screen-ready 판단에 강하다
- 두 경로를 함께 쓰면 coverage와 authority를 동시에 확보할 수 있다

단점:

- decompiled source에서 authority 후보를 먼저 찾지 않으면 polling 순간값에 과도하게 의존하게 된다
- transient polled scene을 scene-ready로 오인하면 하네스 actuation 준비가 오염된다

현재 우선순위는 polling을 버리는 것이 아니라, `decompiled source-first`로 authoritative transition candidate를 찾는 것이다.

### 3. static knowledge의 노이즈

현재 오프라인 추출은 공격적으로 인벤토리를 모으기 때문에 아래 문제가 있다.

- 실제 카드명/유물명이 아닌 리소스 조각이 섞일 수 있음
- 디버그/내부 타입 이름이 많이 들어올 수 있음
- 향후 정규화와 canonical id 정리가 필요함

### 4. WPF/host의 end-to-end 미검증

현재 build와 self-test는 통과했지만, 실제 gameplay와 연결한 end-to-end 시연은 아직 남아 있다.

## 4. 구조적 경계

현재 범위 밖:

- 입력 자동화
- teammate AI
- 멀티플레이 개입
- 인게임 overlay
- 메모리 스캔 기반 치트성 접근

이 경계를 넘기 시작하면 현재 설계가 크게 흔들린다.

## 5. 당장 다음 smoke에서 봐야 할 것

1. reward/event/shop/rest 중 최소 2개 화면이 명시적 screen으로 잡히는가
2. `currentChoices`가 `state.latest.json`과 `state.latest.txt`에 반영되는가
3. runtime log에 semantic hook summary가 남는가
4. startup failure가 재발하지 않는가
5. decompiled flow에서 찾은 transition candidate가 실제 scene-ready 경계와 맞는가
