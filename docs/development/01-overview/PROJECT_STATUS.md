# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-12`
- 게임 버전: `STS2 v0.98.3`
- 주요 도구: `Godot 4.5.1`, `.NET 7`, `ilspycmd 8.2`

이 문서는 저장소가 실제로 어디까지 구현 / 검증됐는지 빠르게 보는 상태 문서입니다.

## 1. 현재 저장소의 중심

현재 저장소는 이제 단일 조언 앱이 아니라 `shared foundation + advisor mode + harness mode` 구조를 향해 재정렬되는 중입니다.

현재 중심 기능은 아래 다섯 가지입니다.

1. 게임 내부 `read-only runtime exporter`
2. strict parser 기반 정적 지식 추출 파이프라인
3. 공통 run/session/artifact 및 collector foundation
4. 외부 `Host + Codex` 조언 경로
5. `WPF` 사용자 인터페이스

최종 제품 비전은 여전히 사람이 직접 플레이하는 동안 게임 밖에서 상태를 읽고 조언을 주는 외부 어시스턴트입니다. 다만 개발/QA를 위해 같은 foundation 위에 test-only harness mode를 병렬로 추가하는 방향으로 구조를 바꾸고 있습니다.

## 2. 현재 검증된 것

### 2.1 빌드와 기본 검증

현재 기준으로 아래는 통과 상태입니다.

- `dotnet build STS2_Mod_AI_Companion.sln`
- `dotnet run --project src\Sts2ModKit.SelfTest --no-build`
- `dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge`
- `dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge`
- `dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 10 --tail 5`

### 2.2 runtime exporter

이미 확인된 범위:

- 모드 로드
- Harmony startup failure 제거
- main menu 기준 live export 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- reward / event / rest / shop hook observed
- collector mode 산출물 구조 추가
  - `raw-observations.ndjson`
  - `screen-transitions.ndjson`
  - `choice-candidates.ndjson`
  - `choice-decisions.ndjson`
  - `semantic-snapshots`

### 2.3 정적 지식 파이프라인

현재 strict canonical 기준:

- cards: `595`
- relics: `296`
- potions: `68`
- events: `62`
- shops: `5`
- rewards: `11`
- keywords: `264`

정적 지식은 이미 AI가 사용할 기본 사전 수준입니다. 현재 병목은 지식량이 아니라 gameplay 상태 추출과 자동 조언 안정성입니다.

### 2.4 Host / WPF / Codex

이미 확인된 것:

- `CompanionHost` polling loop
- `KnowledgeCatalogService` knowledge slice 선택
- `AdvicePromptBuilder` prompt pack / markdown 생성
- `CodexCliClient` Codex CLI JSON 연동
- WPF 상태 / 선택지 / recent events / knowledge / advice 표시
- manual advice 경로
  - WPF `Analyze Now`
  - tool `analyze-live-once`
- `sessionId` 캡처와 `codex-session.json` 저장
- 같은 run 기준 host 재기동 후 세션 복구 경로 구현
- `Retry Last`가 마지막 prompt pack 재전송으로 분리
- WPF의 모델 / 추론 선택, 분석중 상태, 경과 시간 표시

### 2.5 dual-mode 구조 골격

이미 추가된 것:

- `Sts2AiCompanion.Foundation`
  - 공통 계약, 정규화 상태 모델, run/session/artifact foundation
- `Sts2AiCompanion.Advisor`
  - advisor 전용 orchestration façade
- `Sts2AiCompanion.Harness`
  - harness runner / policy / evaluator / replay 스켈레톤
- `Sts2ModAiCompanion.HarnessBridge`
  - test-only action ingress bridge 스켈레톤

아직 남은 것:

- 실제 action executor 구현
- unattended scenario completion
- harness acceptance loop 실증

## 3. 이번 단계에서 반영된 것

### 3.1 runtime 안정화

- `LiveExportAtomicFileWriter.WriteJsonAtomic(...)` 호환 오버로드 추가
- append-only 파일 기록을 `FileShare.ReadWrite|Delete` 기반 shared writer로 변경
- exporter startup에 writer compatibility / core dll identity 로그 추가
- deploy 단계에 SHA256 기반 stale-DLL 진단 추가
- exporter startup identity에 mod/core DLL 정보와 writer compatibility를 추가
- 배포 시 stale DLL 경고와 SHA256 검증 추가

### 3.2 자동 조언 스케줄링

- high-priority automatic advice trigger를 coalesce
- 런당 in-flight Codex 요청 1개 유지
- `runtime-poll`은 automatic advice trigger 대상에서 제외
- `Analyze Now`와 `Retry Last` 의미 분리
  - `Analyze Now`: 현재 snapshot 기준 새 advice 생성
  - `Retry Last`: 마지막 prompt pack 재전송

### 3.3 screen / choice / state merge

- reward/event/rest/shop screen episode를 더 오래 유지
- partial `runtime-poll`이 즉시 `combat`으로 덮지 못하도록 merge 강화
- `currentChoices`를 high-value screen에서 더 오래 유지
- non-authoritative player/deck overwrite 방지
- reward/shop placeholder label 필터 강화
- collector summary에 state regression, screen overwrite, knowledge usage, runtime fatal errors, apphang suspicion 추가

### 3.4 UI / logging

- WPF 한글 UI 복구
- 상태 패널 전부 복사 가능한 읽기 전용 텍스트 박스 유지
- `분석중 / 재시도중 / 실패 / 취소 / 제한` 상태 구분
- advice artifact / collector status에 `missingInformation`, `decisionBlockers`, `knowledge refs`, `knowledge usage count` 노출

## 4. 현재 미검증 / 미완료

### 4.1 gameplay end-to-end

아직 실제 gameplay 기준으로 끝까지 닫히지 않은 것:

- reward / event / shop / rest에서 실제 선택지 텍스트 추출
- reward / event / shop / rest에서 `currentScreen` 유지
- automatic advice가 실제 gameplay trigger에서 `ok`로 생성되는지
- gameplay trigger에서도 같은 `sessionId`가 재사용되는지

### 4.2 최신 collector 산출물 실증

collector mode 코드는 들어갔고, `WriteJsonAtomic` 호환성 수정, shared append writer, startup writer-compat logging, SHA256 deploy 진단도 모두 코드에 반영됐습니다. 다만 마지막 수집 런 로그는 구배포본 영향으로 ABI mismatch 흔적이 남아 있으므로, 최신 배포본 기준 실제 gameplay 재검증이 필요합니다.

### 4.3 AppHang

마지막 실제 플레이에서는 `Application Hang`이 기록됐습니다. disposed object polling, stale exporter worker failure, localization formatting failure가 같이 보였기 때문에, 다음 collector run에서 hang 직전 징후를 다시 수집해야 합니다.

### 4.4 harness mode

현재 harness는 구조와 계약만 들어간 상태입니다.

- 아직 안 된 것
  - bridge action execution
  - scenario runner의 closed-loop 실제 진행
  - recovery / evaluator / replay 실증
- 즉 지금 단계는 `자동화가 가능한 구조`까지 온 것이고, `사람 없이 STS2 테스트를 완주`하는 단계는 다음 구현 대상입니다.

## 5. 현재 우선순위

1. 최신 빌드 재배포 후 collector mode 수집 런 1회
2. reward / event / shop / rest 중 최소 3개 화면 확보
3. auto advice / session reuse / currentChoices / screen persistence 검증
4. collector summary 기준 일괄 수정
5. foundation에서 harness action / scenario / recovery 경로를 실제로 닫는 첫 PoC 구현

## 6. 한 줄 요약

지금 저장소는 `shared foundation + advisor surface`와 harness 골격까지 세워졌고, 다음 병목은 `실제 gameplay high-value 화면에서 automatic advice와 session reuse를 안정적으로 끝까지 연결하는 것`과 `test-only harness를 최소 시나리오 1개까지 실제 실행 가능하게 만드는 것`입니다.

## Update (2026-03-13): Manual Clean Boot gate와 external control 경계

- 현재 가장 큰 리스크는 black screen 단정이 아니라 `harness contamination`입니다.
- `harness.enabled = true` 자체는 더 이상 곧바로 action-enabled 상태를 뜻하지 않습니다.
- 최신 경계 구현은 `dormant bridge + arm/session token + stale queue replay 차단`을 기준으로 재정렬되었습니다.
- 브리지는 `arm.json`이 없으면 queue를 소비하지 않고, stale `actions.ndjson`가 남아 있어도 자동 진행을 유발하지 않는 것이 목표입니다.
- test mode / FTUE 우회는 브리지 로드 시 자동 활성화되면 안 되고, arm 이후에만 허용됩니다.
- external harness control의 목표 형태는 `observer + actuator` DLL이며, 외부 세션이 screenshot + live export + runtime log + harness inventory를 보고 `nodeId`를 선택하는 구조입니다.
- action 단위는 semantic label이 아니라 `nodeId (+ inventoryId + sessionToken)`로 고정하는 방향으로 전환했습니다.
- 현재 최신 코드 기준으로 `inventory.latest.json`, `dispatch_node`, `arm-harness-session`, `disarm-harness-session`, `inspect-harness-control`, `dispatch-harness-node` 경로를 추가했습니다.
- 다만 이 최신 경계 작업에 대한 build/runtime 재검증은 아직 다시 수행하지 않았으므로, 다음 신뢰 게이트는 여전히 `Manual Clean Boot`입니다.

## Manual Clean Boot validated (2026-03-13)
- Steam URI boot에서 rm.json 없이 stale ctions.ndjson를 남겨도 bridge가 dormant 상태를 유지하고 ction-ignored만 기록하는 것을 확인했다.
- live state는 main-menu에 머물렀고 esults.ndjson는 생성되지 않았다.
- 현재 기준선은 복구됐다. 다음 단계는 publish-only inventory observer 복구다.

