# 아키텍처

> Status: Live Charter
> Source of truth: Yes
> Update when: shared foundation, advisor mode, or harness mode structure changes.

Phase 1의 구조는 이제 `shared foundation + advisor mode + harness mode` 관점으로 읽습니다.

- `shared foundation`
  - 게임 내부 exporter
  - 정적 지식 파이프라인
  - 정규화 상태 / 세션 / 아티팩트 모델
  - collector / diagnostics
  - Codex 계약
- `advisor mode`
  - read-only 외부 조언 경로
  - WPF 사용자 UI
- `harness mode`
  - test-only action / legacy scenario / smoke loop / recovery / evaluation 경로

최종 제품은 여전히 read-only advisor이고, harness는 이 제품을 빠르고 안정적으로 검증하기 위한 test-only 운영 모드입니다.

## 1. 게임 내부: native STS2 모드

게임 프로세스 안에 로드되는 작은 shell입니다.

역할:

- loader 호환성 유지
- runtime config 로드
- Harmony 훅 등록
- read-only 상태 관찰

## 2. runtime exporter

게임 안에서 관찰한 상태를 아래 파일로 내보냅니다.

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

핵심 원칙:

- 게임 상태를 바꾸지 않음
- 파일 쓰기는 background writer queue에서만 처리
- latest 파일은 atomic overwrite

## 3. static knowledge pipeline

게임 외부에서는 정적 지식도 별도로 추출합니다.

현재 단계:

- `release-scan`
- `decompile-scan`
- `assembly-scan`
- `pck-inventory`
- `strict-domain-parse`
- `localization-scan`
- `observed-merge`
- `catalog-build`

역할 구분:

- `assembly-scan`, `pck-inventory`, `localization-scan`은 넓게 수집하는 raw/intermediate 계층
- `strict-domain-parse`는 실제 디컴파일된 모델 소스 기준으로 cards/relics/potions/events/shops/rewards/keywords canonical seed를 만드는 계층
- `observed-merge`는 live export와 `artifacts/companion/*/live-mirror`에서 관찰된 실제 플레이 데이터를 canonical 지식에 보강하는 계층
- `catalog.latest.*`, `catalog.assistant.*`, `assistant/*.json`, `markdown/*.md`는 최종 소비 계층

산출물은 `artifacts/knowledge` 아래에 저장됩니다.

## 4. 공통 foundation / advisor / harness 경계

### 4.1 Shared Foundation

공통 foundation은 human UI 없이도 machine-readable state를 제공해야 합니다.

포함 대상:

- runtime observation/export
- semantic state normalization
- static knowledge / knowledge slice
- session lifecycle
- collector / diagnostics
- Codex prompt / response contracts
- artifact model

현재 구현상 주요 위치:

- `src/Sts2ModKit.Core`
- `src/Sts2ModAiCompanion.Mod`
- `src/Sts2AiCompanion.Foundation`

### 4.2 Advisor Mode

advisor mode는 production read-only surface입니다.

포함 대상:

- advice orchestration
- user-facing recommendation formatting
- WPF presentation
- `Analyze Now`, `Retry Last`, auto advice UX

현재 구현상 주요 위치:

- `src/Sts2AiCompanion.Advisor`
- `src/Sts2AiCompanion.Wpf`
- legacy compatibility를 위한 `src/Sts2AiCompanion.Host`

### 4.3 Harness Mode

harness mode는 test-only action-enabled 검증 경로입니다.

현재는 아래 세 층으로 읽는 것이 맞습니다.

- legacy harness
  - `src/Sts2AiCompanion.Harness`
  - scenario runner / action executor abstraction / recovery / evaluation / replay
- in-game test ingress
  - `src/Sts2ModAiCompanion.HarnessBridge`
  - dormant / arm / inventory / queue / test-only actuation ingress
- GUI smoke harness
  - `src/Sts2GuiSmokeHarness`
  - bootstrap/deploy/trust/session orchestration
  - observer-backed scene authority + screenshot analysis
  - step request / allowed action / decision / actuation loop
  - long-run artifacts / supervisor / plateau diagnostics

용어 규칙:

- `Harness Bridge`는 `src/Sts2ModAiCompanion.HarnessBridge`를 가리킨다.
- `Smoke Scenario Loop`는 `src/Sts2GuiSmokeHarness` 내부 step loop를 가리킨다.
- `Legacy Scenario Runner`는 `src/Sts2AiCompanion.Harness/Scenarios/ScenarioRunner.cs`를 가리킨다.

상세 file owner와 current module map은
[GUI_SMOKE_HARNESS_ARCHITECTURE.md](./reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
를 기준으로 본다.

## 5. 외부 Host

`Sts2AiCompanion.Host`는 live export와 knowledge catalog를 묶어 Codex 요청 단위로 정리하는 레이어입니다.

주요 역할:

- live export polling
- run 경계 추적
- knowledge slice 선택
- prompt pack 생성
- Codex CLI 호출
- JSON event stream에서 `thread.started`를 읽어 `sessionId` 캡처
- advice artifact 저장

## 6. 외부 WPF 앱

`Sts2AiCompanion.Wpf`는 최종 사용자 표면입니다.

보여 주는 것:

- 현재 상태
- 현재 화면
- 선택지
- 최근 이벤트
- 관련 knowledge slice
- 최신 AI 조언

## 7. 안전 경계

- snapshot / restore 경로 유지
- direct exe 대신 Steam URI 사용
- read-only exporter 유지
- 게임이 죽지 않아도 외부 앱은 죽을 수 있어야 함
- 외부 앱이 죽어도 게임은 계속 진행 가능해야 함
- harness action layer는 production build와 계약을 공유하지 않도록 분리

## 8. Harness Architecture Pointer

하네스 내부 구조는 이제 별도 문서로 본다.

- current smoke harness module map:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](./reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- human-readable before/after comparison:
  - [GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](./reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md)
- harness / observer glossary:
  - [GUI_SMOKE_HARNESS_GLOSSARY_KO.md](./reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md)
- cleanup-complete baseline contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](./contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](./contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](./contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
