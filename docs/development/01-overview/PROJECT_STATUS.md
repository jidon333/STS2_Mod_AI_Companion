# Project Status

## Date
- 2026-03-14

## Current Priority
- current critical path is `GUI Smoke Harness`
- goal: stabilize `screenshot-first` smoke playthrough so it can drive the game and generate high-quality logs for observer validation

## What Is Stable

### Observer
- `combat` truth is stable via `CombatManager.IsInProgress`
- `logicalScreen`, `visibleScreen`, `flowScreen` are exported
- `combat -> rewards` transition has been observed correctly in real smoke runs

### Smoke Harness primitives
- screenshot capture
- click / right-click / press-key
- human-readable console logging
- `run.log` persistence
- action settle / wait timing tuned for human-paced play
- actual combat primitives verified

## What Is Not Yet Stable
- `event -> visible map -> next room` progression
- screenshot-first room routing across `event / map / rewards / shop / rest-site`
- autonomous end-to-end full run from fresh start to game over

## Current Architecture Direction
- `Smoke Harness` is a dev-only black-box tester
- `Observer` is telemetry and internal truth export
- `Observer` must not dictate Smoke Harness actions
- action authority must remain with screenshot + AI judgment

## Immediate Next Steps
1. improve visible-map progression from event/reward mixed states
2. add treasure chest handling (`center click`)
3. complete `rewards -> map -> next combat -> rewards` autonomously
4. continue using smoke run logs to refine observer fidelity

## Deferred Work
- post-observer refactor of DLL boundaries and bridge/actuator responsibility split
- broader commander integration work
- document pruning beyond canonical architecture and handoff set