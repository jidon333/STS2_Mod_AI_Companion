# 모드 로딩 / 정보 추출 전략 비교

> Status: Reference
> Source of truth: No
> Use this for design rationale and alternatives, not for current-state decisions.

이 문서는 왜 현재 저장소가 지금의 경로를 선택했는지 비교 설명합니다.

## 1. passive save scraping

### 개념

게임이 저장한 save나 설정 파일을 외부에서 읽어 상태를 유추하는 방식입니다.

### 장점

- 구현이 단순할 수 있습니다.
- 게임 내부 패치가 적어도 됩니다.

### 단점

- 실시간성이 떨어집니다.
- 현재 화면과 선택지처럼 순간적인 정보가 잘 안 나옵니다.
- 저장 빈도와 저장 포맷에 크게 의존합니다.

### 결론

보조 fallback으로는 쓸 수 있지만, 1차 입력 경로로는 부족합니다.

## 2. read-only Harmony exporter

### 개념

게임 프로세스 안에서 Harmony 훅과 reflection으로 상태를 읽고, live export 파일로 내보내는 방식입니다.

### 장점

- 실시간성이 좋습니다.
- 화면, 선택지, 최근 변화 같은 정보를 다룰 수 있습니다.
- 외부 앱은 파일만 보면 되므로 구조가 깔끔합니다.

### 단점

- 훅 후보를 잘못 잡으면 startup이 깨질 수 있습니다.
- 런타임 타입 변화에 민감할 수 있습니다.

### 결론

현재 저장소의 주 경로입니다.

## 3. 메모리 스캔 / 외부 메모리 리딩

### 장점

- 모드 없이도 일부 상태를 읽을 수 있습니다.

### 단점

- 안정성과 유지보수성이 낮습니다.
- 범위가 커지면 사실상 별도 역공학 프로젝트가 됩니다.
- 현재 Phase 1 경계와도 맞지 않습니다.

### 결론

현재는 채택하지 않습니다.

## 4. 인게임 overlay / UI 주입

### 장점

- 조언을 게임 내부에 바로 표시할 수 있습니다.

### 단점

- UI 충돌과 intrusive patching 위험이 큽니다.
- 먼저 exporter와 외부 assistant가 안정화되지 않으면 디버깅이 어려워집니다.

### 결론

Phase 1에서는 외부 WPF 앱이 먼저입니다.

## 5. 입력 자동화 / teammate AI

### 장점

- 완전 자동 플레이에 가까운 결과를 낼 수 있습니다.

### 단점

- 현재 프로젝트 경계를 바로 넘습니다.
- 기술적으로도 위험이 커집니다.
- 검증과 롤백이 훨씬 어려워집니다.

### 결론

현재 범위 밖입니다.

## 6. 현재 선택

현재 저장소는 아래 조합을 택합니다.

- 게임 안: read-only Harmony exporter
- 게임 밖: host + WPF + Codex CLI
- 지식: offline scan + observed merge
- 검증: snapshot/deploy/live smoke/restore

이 조합이 현재 목표인 `실시간 AI 조언 어시스턴트`에 가장 맞습니다.
