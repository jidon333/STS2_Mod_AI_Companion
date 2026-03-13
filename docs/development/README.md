# 개발 문서 안내

이 폴더는 `STS2_Mod_AI_Companion` 저장소의 개발 과정, 현재 상태, 트러블슈팅, 초보자용 설명을 모아 둔 작업 문서 모음입니다.

문서별 목적과 추천 읽기 경로를 한 번에 보려면 먼저 `DOCUMENT_MAP.md`를 보면 됩니다.

현재 개발 문서는 아래처럼 주제별 폴더로 나뉩니다.

- `01-overview`
  - 상태, 상위 구조, 입문 문서
- `02-runtime`
  - 실제 게임 플레이 중 런타임 흐름과 live export
- `03-knowledge`
  - 정적 지식 추출과 외부 참고
- `04-advisor`
  - advisor mode와 WPF 사용자 흐름
- `05-harness`
  - test harness mode
- `06-history`
  - 작업 기록과 상세 조사 로그

처음 읽을 때는 아래 순서를 권장합니다.

1. `DOCUMENT_MAP.md`
   - 문서별 목적과 읽기 경로를 정리한 메타 문서입니다.
2. `01-overview/PROJECT_STATUS.md`
   - 지금 무엇이 구현됐고 무엇이 아직 남아 있는지 빠르게 확인합니다.
3. `01-overview/DUAL_MODE_ARCHITECTURE.md`
   - 저장소를 `shared foundation + advisor mode + harness mode`로 어떻게 재해석하는지 설명합니다.
4. `05-harness/HARNESS_MODE.md`
   - test-only action-enabled harness가 production read-only advisor와 어떻게 분리되는지 설명합니다.
5. `01-overview/MODDING_FROM_ZERO.md`
   - Slay the Spire 2 모딩과 현재 저장소의 핵심 개념을 처음부터 설명합니다.
6. `02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
   - 실제 게임을 켰을 때 mod, live export, Host, WPF가 어떤 순서로 움직이는지 설명합니다.
7. `02-runtime/LOAD_CHAIN.md`
   - `mods` 폴더 로드부터 WPF 조언 창까지 데이터가 흐르는 전체 경로를 설명합니다.
8. `01-overview/REPO_STRUCTURE.md`
   - 저장소 구조를 실제 플레이 흐름과 연결해서 설명합니다.
9. `02-runtime/LIVE_EXPORT_SEMANTICS.md`
   - `events.ndjson`, `state.latest.json`, `state.latest.txt`, `session.json`의 의미와 해석 규칙을 설명합니다.
10. `03-knowledge/KNOWLEDGE_EXTRACTION.md`
   - 카드, 유물, 이벤트, 상점 정보를 어떻게 오프라인/관찰 기반으로 수집하는지 설명합니다.
11. `04-advisor/AI_ASSISTANT_ARCHITECTURE.md`
   - live export, knowledge slice, Codex session, WPF UI가 어떻게 연결되는지 설명합니다.
12. `04-advisor/WPF_USER_FLOW.md`
   - 사용자가 앱을 켜고 게임을 실행했을 때 어떤 화면과 버튼을 보게 되는지 시나리오 중심으로 설명합니다.
13. `02-runtime/PENDING_HOOKS_AND_RISKS.md`
    - 아직 붙이지 않은 훅, 아직 검증되지 않은 화면, 구조적 리스크를 정리합니다.
14. `06-history/WORKLOG.md`
    - 큰 작업 흐름을 시간순으로 요약한 기록입니다.
15. `06-history/DETAILED_INVESTIGATION_LOG.md`
    - 실패 원인, 로그 근거, 시행착오, 폐기한 가설까지 자세히 적어 둔 조사 로그입니다.
16. `02-runtime/MOD_LOADING_STRATEGIES.md`
    - 왜 현재 경로를 선택했고 다른 접근은 왜 택하지 않았는지 비교 설명합니다.
17. `01-overview/MOD_BEGINNER_GUIDE.md`
    - 저장소 구조와 첫 실행 명령을 빠르게 익히기 위한 입문 가이드입니다.

문서 작성 기준은 아래와 같습니다.

- 모든 개발 문서는 한국어로 작성합니다.
- 구현 변경과 문서 변경을 분리하지 않습니다.
- 문서는 가능한 한 경로, 명령, 로그 근거, 현재 한계를 함께 적습니다.
- `docs/` 상위 문서는 현재 계약과 사용 절차를 설명하고, `docs/development/`는 구현 과정과 조사 기록을 설명합니다.

현재 상태를 가장 빠르게 동기화해서 보려면 아래 3개를 기준 문서로 봅니다.

- `01-overview/PROJECT_STATUS.md`
- `01-overview/DUAL_MODE_ARCHITECTURE.md`
- `03-knowledge/KNOWLEDGE_EXTRACTION.md`
- `04-advisor/AI_ASSISTANT_ARCHITECTURE.md`

`06-history/WORKLOG.md`와 `06-history/DETAILED_INVESTIGATION_LOG.md`는 역사 기록이므로, 현재 상태 판단은 위 기준 문서를 우선합니다.

## 최근 추가로 읽어둘 것

- `artifacts/knowledge/markdown/README.md`
  - 사람이 읽는 정적 지식 리포트의 진입점입니다.
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
  - 실제 플레이 중 어떤 화면에서 어떤 리포트를 먼저 보면 되는지 설명합니다.
- `artifacts/knowledge/assistant/index.json`
  - AI가 읽는 정적 지식 export의 인덱스입니다.
  - 카드/유물/이벤트/상점/키워드 JSON 파일과 provenance, release metadata, cross-check hint를 함께 담습니다.
## 외부 참고

- `03-knowledge/SPIRE_CODEX_REFERENCE.md`
  - `https://github.com/ptrlrd/spire-codex`와 `https://spire-codex.com/`에서 무엇을 참고했고, 무엇을 참고하지 않았는지 정리한 문서입니다.
