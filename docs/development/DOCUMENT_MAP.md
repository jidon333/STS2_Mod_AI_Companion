# Document Map

## Canonical Roles

- [ROADMAP.md](../ROADMAP.md)
  - 장기 제품/전달 milestone의 canonical source
  - `M1~M10` 정의는 여기서 고정한다
- [PROJECT_STATUS.md](01-overview/PROJECT_STATUS.md)
  - 현재 milestone pointer와 현재 blocker를 가장 짧게 보여주는 문서
- [CURRENT_MILESTONE_CHECKLIST.md](01-overview/CURRENT_MILESTONE_CHECKLIST.md)
  - 현재 운영 포인터만 짧게 모아 둔 shortcut 문서
- [AI_HANDOFF_PROMPT_KO.md](01-overview/AI_HANDOFF_PROMPT_KO.md)
  - 다음 구현 세션/참모 세션용 operational handoff
  - live implementation backlog와 다음 work unit도 여기서 함께 관리한다

## Practical Read Order

1. [PROJECT_STATUS.md](01-overview/PROJECT_STATUS.md)
2. [AI_HANDOFF_PROMPT_KO.md](01-overview/AI_HANDOFF_PROMPT_KO.md)
3. [ROADMAP.md](../ROADMAP.md)
4. [CURRENT_MILESTONE_CHECKLIST.md](01-overview/CURRENT_MILESTONE_CHECKLIST.md)
5. [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
6. [STARTUP_DEPLOY_CONTROL_LAYER.md](05-harness/STARTUP_DEPLOY_CONTROL_LAYER.md)
7. [HARNESS_LAYER_ARCHITECTURE.md](05-harness/HARNESS_LAYER_ARCHITECTURE.md)
8. [STS2_HARNESS_REVIEW.md](05-harness/STS2_HARNESS_REVIEW.md)
9. [HARNESS_MODE.md](05-harness/HARNESS_MODE.md)
10. [DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md](05-harness/DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md)
11. [WORKLOG.md](06-history/WORKLOG.md)

## Current Interpretation

- `ROADMAP.md`는 장기 목표와 10단계 milestone 정의를 고정한다.
- `PROJECT_STATUS.md`는 "지금 current rail이 어디인가"를 한 장으로 보여준다.
- `CURRENT_MILESTONE_CHECKLIST.md`는 중복 상태 설명을 피하고 현재 운영 포인터만 짧게 모아 둔다.
- `AI_HANDOFF_PROMPT_KO.md`는 다음 구현 세션이 바로 읽고 실행할 수 있는 handoff 문서다.
- live backlog는 별도 backlog 문서로 늘리지 않고 `PROJECT_STATUS.md`의 immediate next steps와 `AI_HANDOFF_PROMPT_KO.md`의 next work unit에만 둔다.
- `RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md`는 runtime roles, artifact contracts, done semantics의 canonical reference다.
- `STARTUP_DEPLOY_CONTROL_LAYER.md`는 exact `run -> deploy -> verification -> bootstrap phase -> bootstrap teardown -> authoritative attempt 0001 -> step loop` path를 볼 때 가장 중요하다.
- `HARNESS_LAYER_ARCHITECTURE.md`는 harness / observer / advisor layer split의 canonical reference다.
- `STS2_HARNESS_REVIEW.md`는 왜 smoke harness가 critical path였고 bottleneck이 어떻게 이동했는지 설명한다.
- `HARNESS_MODE.md`와 `DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md`는 observer, scene authority, mixed-state 작업 시 같이 읽는다.
- `WORKLOG.md`는 history이며, primary onboarding document가 아니다.

## Important Current Rule

- `Phase 1 완료`, `현재 active milestone 완료`, `세션 1회 done`은 서로 다른 개념이다.
- startup/trust, gameplay, advisor product surface는 acceptance를 섞지 않는다.
- 현재 active milestone은 `M4. Trusted Attempt 확보`다.
- current blocker는 더 이상 `loader debugging`이나 `event/map overlay`가 아니라 `authoritative attempt / session accounting contract`다.
- any older document that still claims the main blocker is `event/map overlay`, `combat no-op loop`, or `ModManager.TryLoadModFromPck direct loader debugging` should be treated as historical context, not the current critical path.
