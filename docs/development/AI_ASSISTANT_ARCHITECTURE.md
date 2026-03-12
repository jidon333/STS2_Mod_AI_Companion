# AI 어시스턴트 아키텍처

현재 Phase 1 구조는 dual-mode로 재해석합니다.

- `shared foundation`
  - `게임 내부 exporter -> live export -> normalized state / knowledge / session / artifact foundation`
- `advisor mode`
  - `AdvisorCoordinator -> Codex reasoning -> advice artifacts -> WPF`
- `harness mode`
  - `ScenarioRunner -> ActionExecutor -> Recovery/Evaluation/Replay`

즉 현재 advisor 경로는 아래 흐름으로 읽는 것이 맞습니다.

`게임 내부 exporter -> live export -> Foundation state/knowledge/session -> AdvisorCoordinator -> CodexCliClient -> advice artifacts -> WPF`

이 문서는 Host / Codex / WPF 경로를 설명합니다.

## 1. 전체 흐름

1. 게임 내부 native mod가 상태를 수집한다.
2. live export 파일을 기록한다.
3. foundation이 live export를 정규화 상태로 변환한다.
4. `KnowledgeCatalogService`가 assistant catalog를 로드하고 relevant slice를 만든다.
5. `AdvisorCoordinator`가 read-only advisor orchestration을 수행한다.
6. `AdvicePromptBuilder`가 `AdviceInputPack`과 prompt text를 만든다.
7. `CodexCliClient`가 Codex CLI를 호출한다.
8. 결과를 advice artifact로 남긴다.
9. WPF가 최신 상태와 advice를 표시한다.

## 2. Foundation / Advisor 레이어 구성

현재 구조는 migration 중입니다.

### 2.1 Shared Foundation

주요 구성:

- `Sts2AiCompanion.Foundation`
  - `CompanionState`
  - `HarnessAction`
  - `RunSessionCoordinator`
  - `ArtifactStore`
  - `CompanionStateMapper`
- `Sts2ModKit.Core`
  - live export / knowledge / diagnostics / configuration

### 2.2 Advisor Mode

현재 advisor orchestration의 주요 구성:

- `AdvisorCoordinator`
  - live snapshot polling façade
  - 수동 / 자동 / 재시도 요청 중개
  - WPF가 읽기 쉬운 advisor snapshot 제공

legacy 경로의 주요 구성:

- `CompanionHost`
  - polling loop
  - run 경계 감지
  - automatic / manual advice 요청
  - companion artifact 관리
- `KnowledgeCatalogService`
  - `catalog.assistant.json` 우선 로드
  - screen / choices / deck / relics / potions / recent events 기반 knowledge slice 구성
- `AdvicePromptBuilder`
  - `AdviceInputPack` 생성
  - Codex prompt text 생성
  - advice markdown 생성
- `CodexCliClient`
  - Codex CLI 실행
  - JSON event stream 파싱
  - thread/session id 추출

장기적으로는 `Sts2AiCompanion.Host`의 shared 성격 코드를 Foundation으로 옮기고, advisor orchestration만 별도 계층에 남기는 방향입니다.

## 3. 핵심 데이터 모델

- `CompanionRunState`
- `AdviceTrigger`
- `KnowledgeSlice`
- `AdviceInputPack`
- `AdviceResponse`
- `CodexSessionState`
- `CompanionHostSnapshot`
- `CompanionCollectorStatus`
- `CompanionCollectorSummary`

## 4. trigger 정책

### automatic trigger

현재 automatic advice 대상:

- `choice-list-presented`
- `reward-screen-opened`
- `event-screen-opened`
- `shop-opened`
- `rest-opened`

정책:

- 런당 Codex 요청 1개만 in-flight
- high-priority trigger는 latest-only coalesce
- `runtime-poll`은 automatic advice trigger에서 제외

### manual trigger

- WPF `Analyze Now`
- tool `analyze-live-once`

### retry trigger

- WPF `Retry Last`
- 마지막 prompt pack을 다시 Codex로 전송

즉 `Analyze Now`와 `Retry Last`는 이제 다른 의미를 가집니다.

## 5. knowledge slice

assistant는 전체 카탈로그를 매번 보내지 않습니다.

`KnowledgeCatalogService.BuildSlice(...)`가 아래 seed를 기준으로 필요한 entry만 고릅니다.

- current screen
- current choices의 label / value
- deck card id / name
- relics
- potions
- recent event payload

그리고 `maxEntries`, `maxBytes` 제한으로 bounded slice를 만듭니다.

## 6. prompt 계약

`AdviceInputPack`에는 아래가 포함됩니다.

- run id
- trigger kind
- manual 여부
- current screen
- summary text
- `LiveExportSnapshot`
- recent events tail
- selected knowledge entries
- knowledge reasons
- assistant constraints

prompt는 아래 원칙을 강제합니다.

- 자동 플레이 금지
- 입력에 없는 정보 추정 금지
- 현재 상태 / 최근 이벤트 / knowledge slice만 근거로 사용
- JSON schema만 반환
- `missingInformation`, `decisionBlockers`를 반드시 채움

## 7. advice 계약

Codex 응답 필드:

- `headline`
- `summary`
- `recommendedAction`
- `recommendedChoiceLabel`
- `reasoningBullets`
- `riskNotes`
- `missingInformation`
- `decisionBlockers`
- `confidence`
- `knowledgeRefs`

Host는 이를 아래 artifact로 남깁니다.

- `advice/advice.latest.json`
- `advice/advice.latest.md`
- `advice/advice.ndjson`
- `codex-trace.ndjson`

## 8. session 정책

목표 정책은 `run당 1개 Codex session`입니다.

현재 구현:

- `thread.started` / 세션 인덱스 기반 `sessionId` 캡처
- `codex-session.json` 저장
- host 재시작 시 같은 run이면 세션 복구 시도
- manual / automatic / retry 요청이 모두 같은 run-scoped session을 공유하도록 구현

아직 남은 것:

- gameplay automatic trigger에서도 같은 `sessionId`가 안정적으로 이어지는지 실증

## 9. collector mode

collector mode에서는 아래를 추가로 수집합니다.

- raw observations
- screen transitions
- choice candidates
- choice decisions
- semantic snapshots
- collector summary

collector summary가 보여주는 것:

- seen screens timeline
- semantic event counts
- missing choices
- placeholder labels
- auto advice failures
- missing information top N
- decision blockers top N
- knowledge usage summary
- runtime fatal errors
- apphang suspicion indicators
- session tracking status
- observed merge counts
- recommended next fixes

## 10. WPF 역할

WPF는 observer / advisor 표면입니다. 게임을 제어하지 않습니다.

표시 항목:

- 연결 상태
- 현재 화면
- 플레이어 상태
- current choices
- recent events
- relevant knowledge
- 최신 advice
- collector diagnostics

사용자 액션:

- `지금 분석`
- `자동 조언 일시중지 / 다시 켜기`
- `다시 시도`
- `지식 새로고침`
- `산출물 열기`
- 모델 / 추론 강도 선택

## 11. Harness와의 관계

advisor mode와 harness mode는 같은 foundation을 공유합니다.

- 공통으로 쓰는 것
  - `CompanionState`
  - knowledge catalog / knowledge slice
  - run/session/artifact vocabulary
  - collector diagnostics
- 분리되는 것
  - advisor: read-only recommendation UX
  - harness: test-only action execution, recovery, evaluation, replay

즉 현재 WPF와 advisor 흐름은 최종 제품 표면이고, harness는 이를 빠르게 검증하기 위한 별도 mode입니다.

## 12. 현재 남은 과제

1. reward / event / shop / rest의 실제 선택지 텍스트 추출
2. semantic screen 유지
3. gameplay automatic advice 실증
4. gameplay session reuse 실증
5. AppHang 원인 축소
6. harness bridge와 scenario runner를 실제 action loop로 닫기
