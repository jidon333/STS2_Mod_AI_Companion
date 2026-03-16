# Project Status

## Date
- 2026-03-16

## Current Priority
- current critical path is `GUI Smoke Harness`
- latest blocker is `event -> map overlay mixed-state` after event resolution
- goal: stabilize `boot-to-long-run` screenshot-first smoke playthrough so it can drive the game, close artifact-based completion, and generate high-quality evidence for validation

## Progress Snapshot

| Area | Percent |
|---|---:|
| Overall | 76% |
| Evidence | 90% |
| Observer | 77% |
| Actuator | 66% |
| Loop | 63% |
| Ops | 86% |

## What Is Stable

- `combat` truth is stable via `CombatManager.IsInProgress`
- `logicalScreen`, `visibleScreen`, `flowScreen` are exported
- `combat` enemy-targeting has moved substantially from fixed anchors to live hitbox/export-backed targeting
- `reward/map` recovery has advanced via layered state and a short recovery window
- `inspect-session` latest-state recalculation and stall sentinel classification are materially stronger than before
- `replay-step`, `replay-test`, and the golden scene regression suite now exist for offline validation
- startup tracing and deploy fast-path hardening improved boot diagnostics and ops reliability

## What Is Partially Stable

- `Observer/export` is no longer only telemetry and post-check; it now helps with candidate generation and validation input, but it is still not the final action authority
- `reward back` navigation and claimable reward extraction exist, but they remain weaker than the main reward/map lane
- mixed-state sentinel classifications such as `reward-map-loop`, `map-transition-stall`, and `combat-noop-loop` are improved, but loop terminalization still needs reinforcement

## Smoke Harness primitives

- screenshot capture
- click / right-click / press-key
- human-readable console logging
- `run.log` persistence
- action settle / wait timing tuned for human-paced play
- actual combat primitives verified

## What Is Not Yet Stable
- `event` foreground and `map overlay` background handling in the same frame
- distinguishing a `current-node arrow` from a real reachable next node on the overlaid map
- autonomous long-run closure when mixed-state loops need early terminalization
- reward back / claimable reward fallback coverage

## Current Architecture Direction
- `Smoke Harness` is a dev-only black-box tester
- `Observer/export` is telemetry, internal truth export, candidate generation support, and validation input
- `Observer/export` must not dictate the final Smoke Harness action by itself
- action authority must remain with screenshot + AI judgment

## Immediate Next Steps
1. harden `event/map overlay` foreground authority and contamination suppression
2. stop treating the `current-node arrow` as a reachable next node candidate
3. strengthen `reward back` and claimable reward fallback handling
4. expand replay fixtures for mixed-state and loop terminalization regressions
5. keep validating live sessions with `inspect-session`, `startup-trace.ndjson`, and golden scene replay

## Deferred Work
- resident `supervisor / stall sentinel` processes
- lightweight orchestration layer above current human-coordinated workflow
- AI-first fallback lane for novel screens
- post-observer refactor of DLL boundaries and bridge/actuator responsibility split
- broader commander integration work
- document pruning beyond canonical architecture and handoff set
