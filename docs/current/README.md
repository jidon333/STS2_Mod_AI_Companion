# Current Docs Guide

`docs/current/`는 현재 진행 중인 milestone과 active workstream을 관리하는 `AI/운영 current docs` 경로다.

## 폴더 규칙

### `docs/current/`

AI 세션, 구현 세션, 운영 판단이 직접 참조하는 current pointer 문서만 둔다.

예:

- [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md)
- [AI_SESSION_HANDOFF_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/AI_SESSION_HANDOFF_KO.md)
- [AI_HANDOFF_PROMPT_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/AI_HANDOFF_PROMPT_KO.md)
- [M9_EXECUTION_PLAN_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/M9_EXECUTION_PLAN_KO.md)
- [M9_LIVE_SIDECAR_UI_PLAN_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md)
- [ADVISOR_UI_COVERAGE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md)

### `docs/current/readers/`

사람이 읽기 쉽게 풀어쓴 `reader` 문서만 둔다.

예:

- [PROJECT_STATUS_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/PROJECT_STATUS_READER_KO.md)
- [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/ADVISOR_SCENE_MODEL_READER_KO.md)
- [HARNESS_TO_M9_STRUCTURE_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md)

## 금지 규칙

1. 같은 주제의 AI용 current doc과 reader doc을 같은 폴더에 섞어 두지 않는다.
2. handoff, implementation plan, workstream tracker는 `docs/current/` 루트에 둔다.
3. 쉬운 설명, 구조 해설, 읽는 법 안내는 `docs/current/readers/`에 둔다.
4. reader 문서를 current pointer의 source of truth로 사용하지 않는다.

## 기본 읽기 순서

### AI/운영 세션

1. [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md)
2. [AI_SESSION_HANDOFF_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/AI_SESSION_HANDOFF_KO.md)
3. [M9_EXECUTION_PLAN_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/M9_EXECUTION_PLAN_KO.md)

### 사람

1. [PROJECT_STATUS_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/PROJECT_STATUS_READER_KO.md)
2. [HARNESS_TO_M9_STRUCTURE_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md)
3. [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/readers/ADVISOR_SCENE_MODEL_READER_KO.md)
