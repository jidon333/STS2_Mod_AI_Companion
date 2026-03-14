# STS2 Harness Review

## Current Review Summary
- the harness critical path has shifted from `observer truth` to `screenshot-first smoke progression`
- observer truth is no longer the main blocker for combat and basic screen transitions
- the current bottleneck is getting the smoke harness to progress through mixed visual/logical room states without being misled by observer state

## Canonical Principles
1. `Smoke Harness` is a black-box tester
2. `Smoke Harness` uses screenshot-first routing and action selection
3. `Observer` provides telemetry, internal truth, and post-run validation
4. `Observer` is not the action authority for smoke progression

## What Has Been Validated
- combat truth via `CombatManager.IsInProgress`
- `logicalScreen / visibleScreen / flowScreen` split
- actual combat play primitives:
  - attack card -> enemy click
  - defend card -> same card click
  - right-click deselect
  - `E` to end turn
- `combat -> rewards` observer transition

## Current Main Risk
- visible map appears while logical flow may still reflect event/reward state
- room progression from map must be driven by screenshot semantics, not observer stale branches
- map node selection must respect the user-provided rule:
  - current node = red arrow node
  - next node = directly connected dotted adjacent node

## What To Improve Next
1. screenshot-first map routing
2. screenshot-first event substate handling
3. chest/shop/rest-site explicit handling rules
4. keep harvesting observer logs from smoke runs and compare them after the run

## Review Conclusion
- the project should keep investing in the smoke harness because it is the fastest way to produce ground-truth gameplay traces
- observer refinement should now follow smoke harness evidence, not lead it