# 개발 문서 안내

이 폴더는 `STS2_Mod_AI_Companion` 저장소의 개발 과정, 현재 상태, 트러블슈팅, 초보자용 설명을 모아 둔 작업 문서 모음입니다.

처음 읽을 때 권장 순서는 아래와 같습니다.

1. `PROJECT_STATUS.md`
   - 현재 무엇이 구현되었고 무엇이 남아 있는지 빠르게 파악합니다.
2. `MODDING_FROM_ZERO.md`
   - Slay the Spire 2 모딩과 현재 저장소의 기본 개념을 처음부터 설명합니다.
3. `LOAD_CHAIN.md`
   - 게임 시작부터 외부 WPF 조언 앱까지 데이터가 흐르는 전체 경로를 설명합니다.
4. `REPO_STRUCTURE.md`
   - 루트 폴더, `src`, `docs`, `artifacts`, `config`와 핵심 파일의 역할을 설명합니다.
5. `LIVE_EXPORT_SEMANTICS.md`
   - `events.ndjson`, `state.latest.json`, `state.latest.txt`, `session.json`의 의미와 해석 규칙을 설명합니다.
6. `KNOWLEDGE_EXTRACTION.md`
   - 카드, 유물, 이벤트, 상점 정보를 어떤 방식으로 오프라인/관찰 기반으로 수집하는지 설명합니다.
7. `AI_ASSISTANT_ARCHITECTURE.md`
   - live export, knowledge slice, Codex session, WPF UI가 어떻게 연결되는지 설명합니다.
8. `WPF_USER_FLOW.md`
   - 사용자가 앱을 켜고 게임을 실행한 뒤 어떤 화면과 버튼을 보게 되는지 시나리오 중심으로 설명합니다.
9. `PENDING_HOOKS_AND_RISKS.md`
   - 아직 붙이지 않은 훅, 아직 검증되지 않은 화면, 구조적 리스크를 정리합니다.
10. `WORKLOG.md`
    - 큰 흐름을 시간순으로 요약한 작업 기록입니다.
11. `DETAILED_INVESTIGATION_LOG.md`
    - 실패 원인, 로그 근거, 시행착오, 버린 가설까지 상세하게 남긴 조사 로그입니다.
12. `MOD_LOADING_STRATEGIES.md`
    - 왜 현재 경로를 선택했는지, 다른 접근은 왜 쓰지 않았는지 비교 설명합니다.
13. `MOD_BEGINNER_GUIDE.md`
    - 저장소 구조와 첫 실행 명령을 빠르게 익히기 위한 입문 가이드입니다.

현재 문서 원칙은 아래와 같습니다.

- 모든 개발 문서는 한글로 작성합니다.
- 구현 변경과 문서 변경을 분리하지 않습니다.
- 문서에는 가능한 한 경로, 명령, 로그 근거, 현재 한계를 함께 적습니다.
- `docs/` 상위 문서는 현재 계약과 사용 절차를 설명하고, `docs/development/`는 조사와 구현 과정을 설명합니다.
