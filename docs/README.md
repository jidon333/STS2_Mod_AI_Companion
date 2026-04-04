# Docs Guide

이 폴더는 `STS2_Mod_AI_Companion`의 문서를 역할별로 나눈 front-door 인덱스입니다.

목표는 문서를 더 많이 읽게 하는 것이 아니라, `현재 상태`, `계약`, `절차`, `배경`, `시점 기록`, `역사 기록`을 분리해서 유지비와 컨텍스트 비용을 낮추는 것입니다.

## Start Here

기본 reading set은 사람과 AI를 분리한다.

중요:

- `docs/current/` 루트는 `AI/운영 current docs` 전용이다.
- 사람이 읽기 쉽게 풀어쓴 reader 문서는 `docs/current/readers/`에만 둔다.
- 같은 주제의 AI용 current doc과 reader doc을 같은 폴더에 섞어 두지 않는다.

### 사람

1. [PROJECT_STATUS_READER_KO.md](./current/readers/PROJECT_STATUS_READER_KO.md)
   - 사람이 빨리 읽는 현재 상태 설명
2. [ROADMAP.md](./ROADMAP.md)
   - 장기 `M1~M10` milestone 큰 그림
3. [ARCHITECTURE.md](./ARCHITECTURE.md)
   - shared foundation / advisor mode / harness mode 큰 그림

### AI / 운영 세션

1. [PROJECT_STATUS.md](./current/PROJECT_STATUS.md)
   - 현재 마일스톤, blocker, authoritative root
2. [AI_SESSION_HANDOFF_KO.md](./current/AI_SESSION_HANDOFF_KO.md)
   - active session handoff와 bounded next-step context
3. [AI_HANDOFF_PROMPT_KO.md](./current/AI_HANDOFF_PROMPT_KO.md)
   - 새 세션을 current handoff로 시작시키는 wrapper prompt
4. [ROADMAP.md](./ROADMAP.md)
   - 장기 `M1~M10` milestone canonical source
5. [ARCHITECTURE.md](./ARCHITECTURE.md)
   - shared foundation / advisor mode / harness mode 큰 그림
6. [BOUNDARIES.md](./BOUNDARIES.md)
   - 현재 범위와 제외 범위

하네스 내부 구조를 바로 봐야 하는 경우에는 아래 두 문서를 바로 추가로 읽는다.

1. [reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md](./reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
   - current `Sts2GuiSmokeHarness` file owner / runtime layer map
2. [reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md](./reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md)
   - observer / export / bridge / reader / harness consumer를 층별로만 따로 설명한 문서
3. [contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](./contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
   - cleanup-complete baseline contract and maintenance guardrails
4. [reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](./reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md)
   - 사람이 빠르게 읽는 harness refactor before/after comparison
5. [reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md](./reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md)
   - 하네스 / observer / validation 용어집

## Source Of Truth Rules

- 현재 상태, blocker, 다음 작업: [`docs/current/`](./current)
- 장기 milestone 정의: [ROADMAP.md](./ROADMAP.md)
- 제품/하네스 큰 구조와 경계: [ARCHITECTURE.md](./ARCHITECTURE.md), [BOUNDARIES.md](./BOUNDARIES.md)
- 현재 계약/semantics: [`docs/contracts/`](./contracts)
- 실제 운영 절차: [`docs/runbooks/`](./runbooks)
- 배경 설명과 참고 자료: [`docs/reference/`](./reference)
- 입문/예제: [`docs/tutorials/`](./tutorials)
- 시점성 설계/조사 기록: [`docs/snapshots/`](./snapshots)
- 오래된 기록/대체된 문서: [`docs/archive/`](./archive)

중요:

- `snapshot`과 `archive` 문서는 현재 source of truth가 아닙니다.
- current blocker와 active milestone은 `docs/current/` 밖에서 관리하지 않습니다.

## Live Docs

아래 문서만 계속 갱신되는 live 문서로 취급합니다.

| Role | Document | Purpose | Update when |
|---|---|---|---|
| live-current-reader | [current/readers/PROJECT_STATUS_READER_KO.md](./current/readers/PROJECT_STATUS_READER_KO.md) | 사람이 읽는 쉬운 현재 상태 설명 | 현재 상태 설명을 더 쉽게 풀어써야 할 때 |
| live-current | [current/PROJECT_STATUS.md](./current/PROJECT_STATUS.md) | 현재 milestone, blocker, authoritative roots | 현재 상태나 blocker가 바뀔 때 |
| live-current | [current/AI_SESSION_HANDOFF_KO.md](./current/AI_SESSION_HANDOFF_KO.md) | active session handoff와 bounded next-step context | current pointer나 next-step context가 바뀔 때 |
| live-current | [current/AI_HANDOFF_PROMPT_KO.md](./current/AI_HANDOFF_PROMPT_KO.md) | 새 세션을 current handoff로 시작시키는 wrapper prompt | prompt framing이나 required reading set이 바뀔 때 |
| live-charter | [ROADMAP.md](./ROADMAP.md) | 장기 milestone canonical source | 장기 milestone 구조가 바뀔 때 |
| live-charter | [ARCHITECTURE.md](./ARCHITECTURE.md) | 상위 아키텍처와 mode 구조 | shared structure가 바뀔 때 |
| live-charter | [BOUNDARIES.md](./BOUNDARIES.md) | 범위/경계 | product boundary가 바뀔 때 |
| live-reference | [reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md](./reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md) | current smoke harness module/file architecture | harness internal file ownership or runtime layering이 바뀔 때 |
| live-reference | [reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md](./reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md) | observer/export/bridge/reader layering and provenance rules | observer provenance contract or layer responsibility가 바뀔 때 |
| live-reference | [reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](./reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md) | human-readable harness refactor before/after summary | 큰 구조 refactor 결과나 비교 설명을 갱신해야 할 때 |
| live-reference | [reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md](./reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md) | harness / observer / validation glossary | 용어 규칙이나 대표 shorthand 설명이 바뀔 때 |
| live-contract | [contracts/LIVE_EXPORT_SEMANTICS.md](./contracts/LIVE_EXPORT_SEMANTICS.md) | live export 파일 의미 | export semantics가 바뀔 때 |
| live-contract | [contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](./contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md) | cleanup-complete harness / observer baseline contract | maintenance contract나 observer fact/export contract가 바뀔 때 |
| live-contract | [contracts/STARTUP_DEPLOY_CONTROL_LAYER.md](./contracts/STARTUP_DEPLOY_CONTROL_LAYER.md) | startup/deploy/bootstrap sequencing contract | sequencing/deploy/trust contract가 바뀔 때 |
| live-contract | [contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](./contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md) | chronology/projection/state contract | session/accounting contract가 바뀔 때 |
| live-runbook | [runbooks/SMOKE_TEST_CHECKLIST.md](./runbooks/SMOKE_TEST_CHECKLIST.md) | live smoke 절차 | 절차/성공 신호가 바뀔 때 |
| live-runbook | [runbooks/BACKUP_AND_ROLLBACK.md](./runbooks/BACKUP_AND_ROLLBACK.md) | snapshot/restore 절차 | rollback 절차가 바뀔 때 |

## Directory Guide

### `docs/current/`

AI/운영 current docs를 읽는 곳입니다. 매 세션마다 이 경로만 읽으면 current pointer를 따라갈 수 있어야 합니다.

### `docs/current/readers/`

사람이 읽기 쉽게 풀어쓴 현재 상태/구조 reader 문서를 둡니다.
설명용 문서는 이 경로로만 넣고, AI 세션 handoff/current pointer 문서는 넣지 않습니다.

### `docs/contracts/`

현재 runtime/export/startup/accounting semantics를 고정하는 문서들입니다. 구현 상세 walkthrough보다 `authoritative source`, `projection`, `acceptance`, `failure interpretation`에 집중합니다.

### `docs/runbooks/`

실행 절차와 성공/실패 신호만 담는 운영 문서입니다.

### `docs/reference/`

배경 설명과 근거 자료입니다. current state source가 아니며, 필요한 문제를 만났을 때만 펼쳐 봅니다.

### `docs/tutorials/`

처음 들어오는 사람이 큰 그림을 잡거나 예제를 볼 때 사용하는 문서입니다.

### `docs/snapshots/`

특정 시점의 설계/조사/흐름 문서입니다. 현재와 다를 수 있으므로, 최신 판단 근거로 쓰면 안 됩니다.

### `docs/archive/`

역사 기록과 superseded 문서입니다. 운영 판단에는 쓰지 않습니다.

## Superseded Or Absorbed

- `CURRENT_MILESTONE_CHECKLIST.md`는 [PROJECT_STATUS.md](./current/PROJECT_STATUS.md), [AI_SESSION_HANDOFF_KO.md](./current/AI_SESSION_HANDOFF_KO.md), [AI_HANDOFF_PROMPT_KO.md](./current/AI_HANDOFF_PROMPT_KO.md) 로 역할을 흡수했습니다.
- `DOCUMENT_MAP.md`와 각 폴더 `README.md`는 이 문서가 대체합니다.
- `MOD_BEGINNER_GUIDE.md`의 빠른 시작 내용은 [tutorials/MODDING_FROM_ZERO.md](./tutorials/MODDING_FROM_ZERO.md) 에 흡수했습니다.

## Default Reading Sets

### 사람이 상태 파악만 빠르게 할 때

1. [current/readers/PROJECT_STATUS_READER_KO.md](./current/readers/PROJECT_STATUS_READER_KO.md)
2. [current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md](./current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md)
3. [current/readers/ADVISOR_SCENE_MODEL_READER_KO.md](./current/readers/ADVISOR_SCENE_MODEL_READER_KO.md)

### AI / 운영 세션이 current pointer를 잡을 때

1. [current/PROJECT_STATUS.md](./current/PROJECT_STATUS.md)
2. [current/AI_SESSION_HANDOFF_KO.md](./current/AI_SESSION_HANDOFF_KO.md)
3. [current/AI_HANDOFF_PROMPT_KO.md](./current/AI_HANDOFF_PROMPT_KO.md)

### 새로 합류했을 때

1. [ROADMAP.md](./ROADMAP.md)
2. [ARCHITECTURE.md](./ARCHITECTURE.md)
3. [BOUNDARIES.md](./BOUNDARIES.md)
4. [tutorials/MODDING_FROM_ZERO.md](./tutorials/MODDING_FROM_ZERO.md)

### startup / trust / harness를 다룰 때

1. [current/PROJECT_STATUS.md](./current/PROJECT_STATUS.md)
2. [reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md](./reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
3. [contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](./contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
   - 구조 cleanup 세션이면 추가로 읽을 것
4. [reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md](./reference/harness/GUI_SMOKE_HARNESS_OBSERVER_STACK_KO.md)
   - observer/export/bridge 경계를 따로 이해해야 할 때 추가로 읽을 것
5. [reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](./reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md)
   - 전/후 비교가 먼저 필요할 때 추가로 읽을 것
6. [reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md](./reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md)
   - 용어가 헷갈릴 때 추가로 읽을 것
7. [contracts/STARTUP_DEPLOY_CONTROL_LAYER.md](./contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
8. [contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](./contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
9. [runbooks/SMOKE_TEST_CHECKLIST.md](./runbooks/SMOKE_TEST_CHECKLIST.md)

### 자세한 배경이 필요할 때만

- [`docs/reference/`](./reference)
- [`docs/snapshots/`](./snapshots)
- [`docs/archive/`](./archive)
