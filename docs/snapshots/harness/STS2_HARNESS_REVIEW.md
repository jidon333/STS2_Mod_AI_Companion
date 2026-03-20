# STS2 Harness Review

> Status: Snapshot
> Source of truth: No
> This is a point-in-time harness review. Current blocker and contract live in [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md) and [contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](../../contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md).

## Current Review Summary
- the harness critical path has shifted from `observer truth` to `reward/map recovery`, and now further to `event/map overlay mixed-state`
- raw observer truth is no longer the main blocker for combat and basic screen transitions
- the current bottleneck is no longer "cannot read event choices" but "models map overlay foreground incorrectly and mistakes the current-node arrow for a reachable next node"

## Canonical Principles
1. `Smoke Harness` is a black-box tester
2. `Smoke Harness` uses screenshot-first routing and action selection
3. `Observer/export` provides telemetry, internal truth, candidate generation help, and validation input
4. `Observer/export` is not the final action authority for smoke progression

## What Has Been Validated
- combat truth via `CombatManager.IsInProgress`
- `logicalScreen / visibleScreen / flowScreen` split
- live hitbox/export-backed combat enemy targeting reduced dependence on old fixed anchors
- actual combat play primitives:
  - attack card -> enemy click
  - defend card -> same card click
  - right-click deselect
  - `E` to end turn
- `combat -> rewards` observer transition
- `reward/map` layered state and recovery window moved the blocker forward from reward/map oscillation to later mixed-state overlay handling
- `inspect-session` and latest-state sentinel recalculation now classify `reward-map-loop`, `map-transition-stall`, and `combat-noop-loop` more reliably
- `replay-step`, `replay-test`, and the golden scene regression suite provide offline reproduction for scene-selection regressions
- startup tracing and deploy fast-path hardening improved startup failure diagnosis

## Current Main Risk
- event foreground can remain authoritative while map overlay cues appear in the background
- room progression must still be driven by screenshot semantics, with observer/export only as supporting evidence
- the main failure mode is:
  - the harness sees map overlay evidence too aggressively
  - the `current-node arrow` is promoted as though it were a real reachable next node
  - this leads to false map-advance clicks and mixed-state loops
- map node selection must still respect the user-provided rule:
  - current node = red arrow node
  - next node = directly connected dotted adjacent node

## What To Improve Next
1. stronger event foreground vs map contamination modeling
2. explicit rejection of `current-node arrow` as a reachable-next-node candidate
3. `reward back` and claimable reward extractor reinforcement
4. mixed-state loop terminalization hardening in sentinel and `inspect-session`
5. more replay fixtures for `event / reward / map` contamination cases

## Review Conclusion
- the project should keep investing in the smoke harness because it is the fastest way to produce ground-truth gameplay traces
- observer/export refinement should follow smoke harness evidence and assist it, not replace screenshot-first authority
