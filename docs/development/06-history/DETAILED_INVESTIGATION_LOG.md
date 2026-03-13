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

## 8. harness contamination이 결과 전체를 오염시키는 문제

### 증상

- 사용자가 새 명령을 내리지 않았는데도 main menu에서 character select / run start 쪽으로 자동 진행되는 것처럼 보이는 런이 있었습니다.
- stale harness inbox에 `__start__`, `__ironclad__`, `__confirm__` 같은 이전 액션이 남아 있었습니다.
- bridge load 시점에 test mode / FTUE bypass가 곧바로 활성화되는 경로가 존재했습니다.

### 판단

- 이 상태에서는 black screen, crash, scene contamination, harness PoC 결과를 정상 기준으로 해석하면 안 됩니다.
- 우선순위는 `first reward`가 아니라 `Manual Clean Boot`입니다.
- stale deploy/ABI mismatch가 해결되어도 harness contamination이 남아 있으면 런 전체를 신뢰할 수 없습니다.

### 조치

- bridge 기본 상태를 `dormant`로 재정의했습니다.
- `arm.json`이 없으면 queue를 소비하지 않도록 경계를 재설계했습니다.
- session token이 일치하는 action만 소비하도록 방어선을 추가했습니다.
- bridge load 시 test mode auto-enable은 허용하지 않고, arm 이후에만 가능하도록 바꾸는 방향으로 고정했습니다.

## 9. external command transport를 왜 파일 기반으로 먼저 고정했는가

### 고민

- observer는 파일 기반 live export이고, command만 IPC로 올리는 혼합형이 직관적으로 좋아 보일 수 있었습니다.

### 판단

- 현재 iteration의 핵심은 지연시간보다 contamination 차단과 triage 가능성입니다.
- 이미 `live export`, `harness inbox/outbox`, `status.json`, `trace.ndjson`가 파일 기반이므로, 같은 경계에 `arm.json`과 `inventory.latest.json`을 추가하는 편이 더 안전합니다.
- transport를 섞으면 session sync, boot race, stale command 판정 지점이 늘어납니다.

### 결론

- 이번 단계는 파일 기반 external command contract로 고정합니다.
- latency가 실제 병목으로 확인되기 전까지는 IPC로 올리지 않습니다.

## 10. 멀티 에이전트 Codex orchestration 자체의 실패

### 증상

- 서브 에이전트가 repository 파일을 읽거나 쓰려는 시점에 backend가 `refusing to run unsandboxed` 류 오류를 내며 막혔습니다.
- `fork_context=true`로 다시 붙여도 critical path worker로 신뢰할 수 있는 수준까지는 복구되지 않았습니다.

### 판단

- 이 세션에서는 멀티 에이전트를 주 작업 경로로 쓰면 안 됩니다.
- 특히 구현/빌드/검증을 worker에 의존하는 전략은 지금 상태에서 실무적으로 불안정합니다.

### 조치

- 이번 iteration 동안은 메인 세션 단일 실행 주체 전략으로 전환했습니다.
- 서브 에이전트는 critical path에서 제외하고, 문서/설계 참고용 보조 수단으로만 취급하기로 했습니다.
- 이 판단 자체도 시행착오로 기록해 다음 세션이 같은 시간 손실을 반복하지 않도록 합니다.
