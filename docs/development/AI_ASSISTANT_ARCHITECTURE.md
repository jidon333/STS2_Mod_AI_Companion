# AI 어시스턴트 아키텍처

현재 Phase 1의 구조는 `게임 내부 exporter + 게임 외부 Host + WPF UI + Codex CLI`입니다.

## 1. 전체 흐름

1. 게임 안의 native 모드가 상태를 읽습니다.
2. live export 4종 파일을 기록합니다.
3. `CompanionHost`가 live export를 polling합니다.
4. `KnowledgeCatalogService`가 assistant catalog를 로드합니다.
5. Host가 bounded `KnowledgeSlice`를 만듭니다.
6. `AdvicePromptBuilder`가 `AdviceInputPack`과 prompt text를 만듭니다.
7. `CodexCliClient`가 Codex 세션을 만들거나 이어 붙입니다.
8. advice 결과를 artifact로 저장합니다.
9. WPF가 현재 상태와 advice를 표시합니다.

## 2. Host 레이어의 실제 클래스

`src/Sts2AiCompanion.Host`의 핵심은 아래 클래스들입니다.

- `CompanionHost`
  - polling loop
  - run 경계 감지
  - 자동/수동 advice 요청 조정
  - artifact 경로 관리
- `KnowledgeCatalogService`
  - `catalog.assistant.json` 우선 로드
  - 현재 screen / choices / deck / relics / potions / recent events 기반으로 bounded slice 선택
- `AdvicePromptBuilder`
  - `AdviceInputPack` 생성
  - Codex prompt text 생성
  - advice markdown 생성
- `CodexCliClient`
  - Codex CLI JSON 호출
  - session create/resume
  - schema 파싱

관련 모델:

- `CompanionRunState`
- `AdviceTrigger`
- `KnowledgeSlice`
- `AdviceInputPack`
- `AdviceResponse`
- `CodexSessionState`
- `CompanionHostSnapshot`

## 3. Trigger와 advice 생성 방식

현재 Host는 두 종류의 trigger를 갖습니다.

### 3.1 자동 trigger

live export에서 새 이벤트가 들어오면 Host가 이를 검사합니다.

- trigger source: recent `LiveExportEventEnvelope`
- 판단 로직: `LiveExportSummaryFormatter.EvaluateCodexTrigger(...)`
- 정책:
  - 자동 advice on/off 가능
  - 최소 간격 적용
  - 필요 시 bypass 가능

### 3.2 수동 trigger

WPF에서 아래 버튼을 통해 수동 요청을 넣을 수 있습니다.

- `Analyze Now`
- `Retry Last`

현재 코드상 두 버튼 모두 `RequestManualAdviceAsync()`로 이어집니다.

## 4. Knowledge slice의 실제 의미

현재 Host는 전체 catalog를 Codex에 통째로 보내지 않습니다.

`KnowledgeCatalogService.BuildSlice(...)`는 아래 seed를 기준으로 relevant entry만 뽑습니다.

- current screen
- current choices의 label/value
- deck card id/name
- relics
- potions
- recent events payload

그 뒤:

- `maxEntries`
- `maxBytes`

제약으로 bounded slice를 만듭니다.

즉 현재 AI 입력은 `현재 상황과 가까운 지식만 넣는 구조`입니다.

## 5. Prompt 계약

`AdviceInputPack`은 아래를 담습니다.

- run id
- trigger kind
- manual 여부
- current screen
- state summary text
- `LiveExportSnapshot`
- recent events tail
- selected knowledge entries
- knowledge reasons
- constraints text

prompt text는 현재 아래 원칙을 강제합니다.

- 게임을 자동 플레이하지 말 것
- 현재 상태, 최근 이벤트, knowledge slice만 근거로 쓸 것
- 모르면 추정하지 말고 모른다고 말할 것
- JSON schema를 따를 것

## 6. Advice 계약

Codex 응답은 JSON schema 기반입니다.

핵심 필드:

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
- `codex-session.json`

## 7. Companion artifact 구조

현재 companion artifact root는 아래입니다.

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/live-mirror/`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice.ndjson`
- `artifacts/companion/<run-id>/advice.latest.json`
- `artifacts/companion/<run-id>/advice.latest.md`
- `artifacts/companion/<run-id>/codex-session.json`
- `artifacts/companion/<run-id>/host-status.json`

즉 run별로 `무엇을 보고`, `무엇을 물었고`, `무슨 답을 받았는지`를 추적할 수 있습니다.

## 8. WPF의 실제 역할

`src/Sts2AiCompanion.Wpf`는 현재 아래를 보여줍니다.

- status line
- run line
- current screen
- last updated
- player summary
- deck summary
- relics / potions summary
- current choices
- recent events
- knowledge entries
- latest advice

수동 UI action:

- `Analyze Now`
- `Pause Auto Advice` / `Resume Auto Advice`
- `Retry Last`
- `Refresh Knowledge`
- `Open Artifacts`

## 9. 현재 코드상 있는 것과 아직 실플레이로 닫히지 않은 것

코드상 이미 있는 것:

- live export polling
- bounded knowledge slice
- prompt pack 생성
- Codex CLI 연동
- advice artifact 저장
- WPF 표시와 수동 버튼

아직 실플레이로 닫히지 않은 것:

- reward/event/shop/rest/combat gameplay에서 auto advice
- manual advice의 실제 gameplay end-to-end
- run 종료 summary 흐름
- replay harness 기반 자동 acceptance
