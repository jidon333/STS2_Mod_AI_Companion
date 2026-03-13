# 범위와 경계

현재 Phase 1에서 포함하는 것:

- read-only native runtime exporter
- static knowledge extraction
- 외부 프로세스 host / Codex backend
- WPF 조언 앱
- snapshot / restore / live smoke 검증

현재 Phase 1에서 제외하는 것:

- 자동 플레이
- teammate AI
- 멀티플레이 개입
- intrusive patching
- hidden memory scraping 중심 설계

핵심 원칙:

- 게임 상태를 바꾸는 대신 관찰한다.
- 조언은 외부 앱에서 제공한다.
- 게임이 계속 진행 가능한 구조를 유지한다.
- smoke 실패 시 restore 경로가 있어야 한다.
