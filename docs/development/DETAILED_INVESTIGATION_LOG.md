# 상세 조사 로그

이 문서는 증상, 시도, 실패 원인, 최종 판단을 조금 더 자세히 남기는 조사 로그입니다.

## 1. native route 재확인

### 증상

- 초기에 저장소가 scaffold 수준이라 실제 STS2 live state를 안정적으로 꺼낼 수 있는 경로가 부족했습니다.

### 시도

- scaffold와 기존 speed mod 저장소를 읽어 native route, packaging, snapshot/restore 습관을 비교했습니다.

### 판단

- 현재 저장소는 `passive save scraping`보다 `read-only Harmony exporter`를 중심으로 가는 것이 맞다고 판단했습니다.

## 2. Harmony `PatchAll` startup failure

### 증상

- `godot.log`에 startup 시점 예외가 발생했고 live export가 bootstrap 이후 진행되지 않았습니다.

### 원인

- broad hook 후보와 void 메서드 postfix 처리 경로가 섞이면서 Harmony가 안전하게 패치하지 못했습니다.

### 조치

- hook registry를 더 좁게 잡았습니다.
- startup identity와 hook summary를 런타임 로그에 남기도록 정리했습니다.
- polling fallback을 유지해 main menu baseline을 복구했습니다.

## 3. smoke 절차의 맹점

### 증상

- `inspect-godot-log --lines 200`가 startup failure를 놓칠 수 있었습니다.

### 원인

- tail 위주 확인 절차는 로그 초반 예외를 놓칠 수 있습니다.

### 조치

- inspect 결과에 startup findings를 따로 노출하는 방향으로 진단 레이어를 보강했습니다.

## 4. 정적 지식 추출이 비어 있던 문제

### 초기 상태

- live observation만으로는 카드/유물/이벤트/상점 본문 지식이 거의 채워지지 않았습니다.

### 판단

- 관찰 기반 카탈로그가 충분히 채워질 때까지 기다리지 말고 오프라인 스캔을 공격적으로 같이 진행해야 했습니다.

### 조치

- `assembly-scan`
- `pck-inventory`
- `observed-merge`

를 묶어 `extract-static-knowledge`에서 함께 생성하도록 변경했습니다.

## 5. inspect-static-knowledge의 이상한 출력

### 증상

- 한 번은 extract 직후 inspect가 intermediate artifact를 못 찾고 0건처럼 보였습니다.

### 원인

- extract와 inspect를 병렬로 돌려 inspect가 먼저 읽었습니다.

### 판단

- 코드 버그가 아니라 실행 순서 문제였습니다.

### 현재 처리

- static knowledge 검증은 extract 이후 inspect를 순차 실행하는 것으로 다시 확인했습니다.

## 6. WPF / host 도입 시의 최소 원칙

### 판단

- 모드가 외부 프로세스를 spawn하는 구조는 넣지 않습니다.
- WPF 앱이 먼저 켜져 있거나 나중에 켜져도 live export에 attach하는 구조가 더 안전합니다.

### 이유

- 실패 시 게임과 외부 앱을 분리해서 볼 수 있습니다.
- Phase 1 경계와도 잘 맞습니다.

## 7. 남은 조사 포인트

- reward/event/shop/rest/combat에서 semantic hook가 실제로 어느 타입/메서드에 잘 걸리는가
- PCK inventory 결과를 실제 카드/유물/이벤트 canonical id로 어떻게 정규화할 것인가
- replay harness를 어떤 fixture 세트로 구성할 것인가
