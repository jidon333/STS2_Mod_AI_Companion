# AI 어시스턴트 아키텍처

현재 Phase 1 구조는 다음입니다.

`게임 내부 exporter -> live export -> CompanionHost -> KnowledgeCatalogService -> AdvicePromptBuilder -> CodexCliClient -> advice artifacts -> WPF`

이 문서는 그중 Host / Codex / WPF 경로를 설명합니다.

## 1. 전체 흐름

1. 게임 내부 native mod가 상태를 수집합니다.
2. live export 4종 파일을 기록합니다.
3. `CompanionHost`가 live export를 polling합니다.
4. `KnowledgeCatalogService`가 assistant catalog를 로드하고 relevant slice를 만듭니다.
5. `AdvicePromptBuilder`가 `AdviceInputPack`과 prompt text를 생성합니다.
6. `CodexCliClient`가 Codex CLI를 호출합니다.
7. 결과를 advice artifact로 저장합니다.
8. WPF가 최신 상태와 advice를 표시합니다.

## 2. Host 레이어 구성

`src/Sts2AiCompanion.Host`의 주요 구성은 아래와 같습니다.

- `CompanionHost`
  - polling loop
  - run 경계 감지
  - 자동 / 수동 advice 요청
  - companion artifact 관리
- `KnowledgeCatalogService`
  - `catalog.assistant.json` 우선 로드
  - current screen / choices / deck / relics / potions / recent events 기준 knowledge slice 선택
- `AdvicePromptBuilder`
  - `AdviceInputPack` 생성
  - Codex prompt text 생성
  - markdown advice 생성
- `CodexCliClient`
  - Codex CLI 실행
  - output schema 기반 JSON 응답 파싱
  - session id 추적 시도

## 3. 주요 데이터 모델

- `CompanionRunState`
- `AdviceTrigger`
- `KnowledgeSlice`
- `AdviceInputPack`
- `AdviceResponse`
- `CodexSessionState`
- `CompanionHostSnapshot`

이 구조 덕분에 WPF는 “지금 화면과 선택지, 최근 이벤트, 관련 지식, 최신 조언”을 한 번에 표시할 수 있습니다.

## 4. trigger 구조

### 4.1 자동 trigger

자동 trigger는 live export 이벤트를 읽고 `LiveExportSummaryFormatter.EvaluateCodexTrigger(...)`로 판정합니다.

대상 이벤트:

- `choice-list-presented`
- `reward-screen-opened`
- `event-screen-opened`
- `map-node-entered`
- 이후 combat 관련 trigger

### 4.2 수동 trigger

수동 trigger는 아래 두 경로가 사용합니다.

- WPF `Analyze Now`
- tool command `analyze-live-once`

이 경로는 현재 실제로 성공했습니다.

## 5. knowledge slice 구조

assistant는 전체 카탈로그를 매번 보내지 않습니다.

`KnowledgeCatalogService.BuildSlice(...)`는 아래 seed를 기준으로 필요한 entry만 선택합니다.

- current screen
- current choices의 label / value
- deck card id / name
- relics
- potions
- recent event payload

그리고 `maxEntries`, `maxBytes` 제한으로 bounded slice를 만듭니다.

즉 실제 AI 입력은 “현재 상황에 필요한 지식만 잘라낸 구조”입니다.

## 6. prompt 계약

`AdviceInputPack`은 대략 아래를 포함합니다.

- run id
- trigger kind
- manual 여부
- current screen
- state summary text
- `LiveExportSnapshot`
- recent events tail
- selected knowledge entries
- knowledge reasons
- assistant constraints

prompt는 아래 원칙을 강제합니다.

- 자동 플레이 금지
- 입력에 없는 정보는 추정하지 않기
- 현재 상태와 최근 이벤트, knowledge slice만 근거로 답하기
- JSON schema를 따르기

## 7. advice 계약

Codex 응답 JSON 필드:

- `headline`
- `summary`
- `recommendedAction`
- `recommendedChoiceLabel`
- `reasoningBullets`
- `riskNotes`
- `confidence`
- `knowledgeRefs`

Host는 이를 아래 artifact로 저장합니다.

- `advice.latest.json`
- `advice.latest.md`
- `advice.ndjson`

## 8. 현재 Codex 상태

이 부분은 현재 구현과 실증을 분리해서 봐야 합니다.

### 성공한 것

- Codex CLI 호출
- manual advice 생성
- latest advice JSON / Markdown 저장

### 아직 닫히지 않은 것

- automatic advice의 최신 gameplay 재실증
- run-scoped session id capture / resume

현재 최신 성공 advice에서도 `sessionId`는 `null`이고 `codex-session.json`은 생성되지 않았습니다. 따라서 지금 상태는 “Codex와 대화해 조언을 받는 것”은 성공, “런 단위 세션을 계속 이어 붙이는 것”은 미완료입니다.

## 9. WPF 역할

WPF는 게임을 제어하지 않습니다. 외부 observer / advisor 표면입니다.

보여주는 것:

- 연결 상태
- 현재 화면
- 플레이어 상태
- current choices
- recent events
- 관련 knowledge entries
- 최신 advice

수동 동작:

- `Analyze Now`
- `Pause/Resume Auto Advice`
- `Retry Last`
- `Refresh Knowledge`
- `Open Artifacts`

## 10. 지금 남은 아키텍처 과제

1. automatic advice를 gameplay high-value screen에서 다시 실증
2. session id capture / resume 경로를 닫기
3. reward / shop / rest choice label 정제
4. replay harness 기반 무인 acceptance 추가
