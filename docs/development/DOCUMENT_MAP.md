# 문서 맵

이 문서는 `STS2_Mod_AI_Companion`의 문서를 **무슨 목적으로 읽어야 하는지**와 **어떤 순서로 읽어야 하는지**를 한 번에 보여주는 메타 문서입니다.

## 0. 지금 당장 유지할 canonical 문서

현재 우선순위는 `Smoke Harness`와 `Observer`를 빠르게 안정화하는 것입니다. 그래서 아래 문서만 active canonical로 봅니다.

1. `01-overview/AI_HANDOFF_PROMPT_KO.md`
2. `01-overview/PROJECT_STATUS.md`
3. `05-harness/HARNESS_LAYER_ARCHITECTURE.md`
4. `05-harness/HARNESS_MODE.md`
5. `05-harness/DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md`
6. `05-harness/STS2_HARNESS_REVIEW.md`
7. `06-history/WORKLOG.md`

나머지 문서는 reference 또는 history로 보고, 구현 중 의사결정 authority로 쓰지 않습니다.

## 1. 가장 먼저 읽을 순서

처음 프로젝트를 파악할 때는 아래 순서를 권장합니다.

1. `01-overview/PROJECT_STATUS.md`
   - 지금 실제로 어디까지 구현됐는지, 무엇이 검증됐고 무엇이 아직 남았는지 빠르게 파악합니다.
2. `01-overview/DUAL_MODE_ARCHITECTURE.md`
   - 저장소를 `shared foundation + advisor mode + harness mode`로 어떻게 재해석하는지 설명합니다.
3. `ARCHITECTURE.md`
   - 저장소 전체 아키텍처를 상위 관점에서 봅니다.
4. `02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
   - 실제 게임을 켰을 때 mod, live export, host, WPF가 어떤 순서로 움직이는지 봅니다.
5. `04-advisor/AI_ASSISTANT_ARCHITECTURE.md`
   - advisor 경로에서 state, knowledge, prompt, Codex, artifact가 어떻게 연결되는지 봅니다.
6. `05-harness/HARNESS_MODE.md`
   - test-only harness가 어떤 계약과 목표를 갖는지 봅니다.
7. `01-overview/REPO_STRUCTURE.md`
   - 저장소의 실제 폴더와 프로젝트가 어느 층에 속하는지 확인합니다.
8. `03-knowledge/KNOWLEDGE_EXTRACTION.md`
   - 정적 지식 파이프라인과 산출물 구조를 이해합니다.

## 2. 문서별 간략 설명

### 상위 문서

- `README.md`
  - 저장소의 가장 바깥 진입점입니다. 빌드, smoke test, 주요 산출물, 관련 문서 링크를 제공합니다.
- `docs/ARCHITECTURE.md`
  - 저장소 전체를 `foundation / advisor / harness` 관점으로 요약한 상위 아키텍처 문서입니다.
- `docs/BOUNDARIES.md`
  - production read-only 계약, 테스트와 프로덕션의 경계, 금지 사항을 설명합니다.
- `docs/ROADMAP.md`
  - Phase 1 이후 우선순위와 장기 계획을 설명합니다.
- `docs/REALTIME_EXTRACTION.md`
  - runtime exporter와 live export 계약 중심 문서입니다.
- `docs/SMOKE_TEST_CHECKLIST.md`
  - 실제 게임 smoke test 운영 절차 문서입니다.
- `docs/BACKUP_AND_ROLLBACK.md`
  - snapshot/restore/rollback 절차를 설명합니다.

### 개발 문서

- `docs/development/README.md`
  - 개발 문서 인덱스입니다. 읽기 순서를 간단히 안내합니다.
- `docs/development/DOCUMENT_MAP.md`
  - 이 문서입니다. 문서별 목적과 추천 읽기 순서를 설명합니다.
- `docs/development/01-overview/PROJECT_STATUS.md`
  - 현재 구현/검증 상태를 가장 빠르게 파악하는 상태 문서입니다.
- `docs/development/01-overview/DUAL_MODE_ARCHITECTURE.md`
  - dual-mode 재정렬 계획과 구조적 이유를 설명합니다.
- `docs/development/05-harness/HARNESS_MODE.md`
  - harness mode 전용 개념과 운영 목표를 설명합니다.
- `docs/development/05-harness/DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md`
  - 하네스 observer를 `decompiled source-first`, `event + polling mixed observer` 기준으로 읽는 문서입니다.
- `docs/development/01-overview/MODDING_FROM_ZERO.md`
  - 초보자 기준으로 STS2 모딩과 현재 저장소 핵심 개념을 설명합니다.
- `docs/development/02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
  - 실제 플레이 중 어떤 데이터 흐름이 일어나는지 서술합니다.
- `docs/development/02-runtime/LOAD_CHAIN.md`
  - 게임 로드부터 외부 어시스턴트 UI까지 데이터 체인을 설명합니다.
- `docs/development/01-overview/REPO_STRUCTURE.md`
  - 폴더/프로젝트를 실제 플레이 흐름과 dual-mode 구조 기준으로 설명합니다.
- `docs/development/02-runtime/LIVE_EXPORT_SEMANTICS.md`
  - live export 필드 의미와 해석 규칙을 설명합니다.
- `docs/development/03-knowledge/KNOWLEDGE_EXTRACTION.md`
  - decompile/localization/strict parser/observed merge/assistant export를 설명합니다.
- `docs/development/04-advisor/AI_ASSISTANT_ARCHITECTURE.md`
  - advisor 경로의 host, prompt, Codex, artifact, WPF 관계를 설명합니다.
- `docs/development/04-advisor/WPF_USER_FLOW.md`
  - 사용자가 실제로 보게 되는 advisor UI 흐름을 설명합니다.
- `docs/development/02-runtime/PENDING_HOOKS_AND_RISKS.md`
  - 아직 붙지 않은 훅, 남은 리스크, 보류한 경로를 정리합니다.
- `docs/development/06-history/WORKLOG.md`
  - 큰 작업 흐름을 시간순으로 기록한 요약 문서입니다.
- `docs/development/06-history/DETAILED_INVESTIGATION_LOG.md`
  - 트러블슈팅과 시행착오, 폐기한 가설을 자세히 남긴 문서입니다.
- `docs/development/02-runtime/MOD_LOADING_STRATEGIES.md`
  - 현재 방식과 대안 방식을 비교 설명합니다.
- `docs/development/01-overview/MOD_BEGINNER_GUIDE.md`
  - 저장소를 처음 보는 사람을 위한 입문 안내입니다.
- `docs/development/03-knowledge/SPIRE_CODEX_REFERENCE.md`
  - 외부 참고 자료인 Spire Codex를 어떻게 참고했는지 정리한 문서입니다.

### 산출물 문서

- `artifacts/knowledge/markdown/README.md`
  - 사람이 읽는 정적 지식 리포트의 진입점입니다.
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
  - 실제 플레이 중 어떤 리포트를 먼저 보면 좋은지 설명합니다.
- `artifacts/knowledge/assistant/index.json`
  - AI가 읽는 structured knowledge export의 인덱스입니다.

## 3. 목적별 추천 읽기 경로

### A. 지금 상태만 빨리 알고 싶을 때

1. `README.md`
2. `docs/development/01-overview/PROJECT_STATUS.md`
3. `docs/development/01-overview/DUAL_MODE_ARCHITECTURE.md`
4. `docs/development/04-advisor/AI_ASSISTANT_ARCHITECTURE.md`

### B. 실제 게임 플레이 중 데이터 흐름을 알고 싶을 때

1. `docs/development/02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
2. `docs/development/02-runtime/LOAD_CHAIN.md`
3. `docs/development/02-runtime/LIVE_EXPORT_SEMANTICS.md`
4. `docs/REALTIME_EXTRACTION.md`

### C. 정적 지식 파이프라인을 알고 싶을 때

1. `docs/development/03-knowledge/KNOWLEDGE_EXTRACTION.md`
2. `artifacts/knowledge/markdown/README.md`
3. `artifacts/knowledge/assistant/index.json`

### D. harness까지 포함한 전체 구조를 알고 싶을 때

1. `docs/development/01-overview/DUAL_MODE_ARCHITECTURE.md`
2. `docs/development/05-harness/HARNESS_MODE.md`
3. `docs/development/05-harness/DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md`
4. `docs/ARCHITECTURE.md`
5. `docs/development/01-overview/REPO_STRUCTURE.md`

## 4. 현재 판단

현재는 `docs/development/README.md`가 개발 문서 인덱스 역할을 하고 있고, `README.md`에는 관련 문서 링크가 있습니다.

하지만 이 문서처럼:

- 문서별 목적을 짧게 설명하고
- 목적별 읽기 경로를 분기해 주는
- 전용 메타 문서

는 별도로 있는 편이 더 낫습니다.

즉, 이 문서가 그 역할을 맡습니다.
