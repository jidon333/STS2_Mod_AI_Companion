# Worklog

## 2026-03-16

### Mixed-state bottleneck moved forward again
- Kept the core `screenshot-first` rule for Smoke Harness progression.
- Promoted `Observer/export` from pure telemetry/post-check toward candidate generation and validation input, while still keeping it below final action authority.
- Improved combat enemy-targeting substantially by moving from fixed-anchor assumptions toward live hitbox/export-backed targeting.
- Advanced `reward/map` recovery with layered state and a short recovery window.

### New current blocker
- The primary blocker is no longer "cannot read event choices."
- The latest blocker is `event -> map overlay mixed-state` handling after event resolution.
- The concrete failure mode is over-promoting map overlay foreground and mistaking the `current-node arrow` for a reachable next node.

### Validation and ops improvements
- Strengthened latest-state sentinel and `inspect-session` recalculation.
- Added `replay-step`, `replay-test`, and golden scene regression coverage for offline reproduction.
- Added `startup-trace.ndjson` and `startup-summary.json` so boot/deploy/first-step failures can be diagnosed separately from gameplay loops.

### Remaining gaps
- `reward back` and claimable reward extraction still need reinforcement.
- Mixed-state loop terminalization is improved but not closed.

## 2026-03-14

### Observer / Smoke Harness direction reset
- Re-centered the architecture around a strict rule:
  - `Smoke Harness` plays the game via screenshot-first AI decisions
  - `Observer` exports telemetry and internal truth for later analysis
- Stopped treating observer as the action authority for smoke progression.

### Observer truth improvements already in place
- Locked combat truth to `CombatManager.IsInProgress`.
- Added `logicalScreen`, `visibleScreen`, and `flowScreen` export.
- Confirmed `combat -> rewards` transition through real smoke-run evidence.

### Smoke Harness runtime improvements
- Added human-readable console logging and mirrored it into `run.log`.
- Added click/right-click/key primitives and validated them in live combat.
- Verified combat interaction rules:
  - attack card -> enemy click
  - defend card -> same card click
  - right-click deselect
  - `E` end turn
- Reduced slow wait loops by tuning baseline waits.
- Relaxed stale decision handling for `provider=auto` so the harness no longer discards its own local decisions too aggressively.

### Current blocker
- The primary blocker is now `event / reward mixed state -> visible map -> next room progression`.
- Screenshot routing for map progression still needs improvement.
- User-provided rule for map progression is now canonical:
  - current node is the node marked by the red arrow
  - next selectable nodes are only directly dotted-connected adjacent nodes

### Current priority
- Finish screenshot-first smoke playthrough stability before broad refactoring.
- Use resulting runtime logs to continue tightening observer fidelity.
