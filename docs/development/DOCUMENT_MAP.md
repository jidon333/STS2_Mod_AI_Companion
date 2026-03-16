# Document Map

## Canonical Read Order
1. [AI_HANDOFF_PROMPT_KO.md](01-overview/AI_HANDOFF_PROMPT_KO.md)
2. [PROJECT_STATUS.md](01-overview/PROJECT_STATUS.md)
3. [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](05-harness/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)
4. [HARNESS_LAYER_ARCHITECTURE.md](05-harness/HARNESS_LAYER_ARCHITECTURE.md)
5. [STS2_HARNESS_REVIEW.md](05-harness/STS2_HARNESS_REVIEW.md)
6. [HARNESS_MODE.md](05-harness/HARNESS_MODE.md)
7. [DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md](05-harness/DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md)
8. [WORKLOG.md](06-history/WORKLOG.md)

## Current Interpretation
- `AI_HANDOFF_PROMPT_KO.md` is the operational handoff source for the next coding session.
- `PROJECT_STATUS.md` is the short current-state snapshot with the latest blocker and progress numbers.
- `RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md` is the canonical long-run architecture reference for runtime roles, artifact contracts, and the separation between program roles and development agent roles.
- `HARNESS_LAYER_ARCHITECTURE.md` is the canonical layer split reference.
- `STS2_HARNESS_REVIEW.md` explains why the smoke harness is the critical path and how the bottleneck has moved.
- `HARNESS_MODE.md` and `DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md` should be read together when touching observer, scene authority, or mixed-state handling.
- `WORKLOG.md` records milestone-level shifts and should not be treated as the primary onboarding document.

## Important Current Rule
- `Smoke Harness` is screenshot-first.
- `Observer/export` is not the final action authority.
- `Observer/export` may be used for candidate generation, foreground/background disambiguation, replay diagnostics, and post-run validation.
- If any older document implies observer-only routing, fixed-anchor combat targeting, or a resolved `reward/map` blocker, treat it as obsolete and follow the canonical set above.
