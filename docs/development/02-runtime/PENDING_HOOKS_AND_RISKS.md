# 미완 훅과 리스크

## 1. 아직 남아 있는 핵심 미완 작업

### 고가치 화면 실증

아직 실제 live smoke로 확실히 닫아야 하는 화면:

- reward
- event
- shop
- rest
- combat start
- turn start

main menu baseline은 확보했지만, 조언 가치가 높은 화면은 아직 모두 실증된 상태가 아닙니다.

### choice 추출

`currentChoices`는 구조가 이미 있지만, 실제 gameplay 화면에서 얼마나 안정적으로 채워지는지는 추가 검증이 필요합니다.

## 2. 현재 의도적으로 안 붙인 훅

다음 계열은 broad하고 위험해서 기본 후보군에서 제외합니다.

- `_Ready`
- `_EnterTree`
- `Show`
- `Refresh`

이유:

- 너무 많은 공통 노드에 걸릴 수 있습니다.
- startup failure나 과도한 전역 훅 성격으로 번질 수 있습니다.
- 이전에 `void` 메서드 postfix와 결합되며 Harmony 예외를 유발한 적이 있습니다.

## 3. 기술 리스크

### 1. runtime-confirmed whitelist 유지 비용

안전하게 가려면 확인된 메서드만 추가해야 합니다. 이 방식은 안전하지만 coverage 확장이 느릴 수 있습니다.

### 2. polling fallback 의존

hook coverage가 부족한 구간은 polling에 의존합니다.

장점:

- startup 안정성이 좋음

단점:

- 화면 의미가 얕게 잡힐 수 있음
- semantic event가 부족할 수 있음

### 3. static knowledge의 노이즈

현재 오프라인 추출은 공격적으로 인벤토리를 모으기 때문에 아래 문제가 있습니다.

- 실제 카드명/유물명이 아닌 리소스 조각이 섞일 수 있음
- 디버그/내부 타입 이름이 많이 들어올 수 있음
- 향후 정규화와 canonical id 정리가 필요함

### 4. WPF/host의 end-to-end 미검증

현재 build와 self-test는 통과했지만, 실제 gameplay와 연결한 end-to-end 시연은 아직 남아 있습니다.

## 4. 구조적 경계

현재 범위 밖:

- 입력 자동화
- teammate AI
- 멀티플레이 개입
- 인게임 overlay
- 메모리 스캔 기반 치트성 접근

이 경계를 넘기 시작하면 현재 설계가 크게 흔들립니다.

## 5. 당장 다음 smoke에서 봐야 할 것

1. reward/event/shop/rest 중 최소 2개 화면이 명시적 screen으로 잡히는가
2. `currentChoices`가 `state.latest.json`과 `state.latest.txt`에 반영되는가
3. runtime log에 semantic hook summary가 남는가
4. startup failure가 재발하지 않는가
