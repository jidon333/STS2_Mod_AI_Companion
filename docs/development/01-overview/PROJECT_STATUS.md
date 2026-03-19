# Project Status

## Date
- 2026-03-19

## Current Milestone Pointer

- current rail: `startup / trust`
- active milestone: `M2. 모드 로드 진입 증명`
- next milestone: `M3. 런타임 부트스트랩 가동`
- long-term goal: 사람이 실제 플레이 중 참고할 수 있는 `read-only advisor` 완성

## Current Priority

- current critical path is no longer `event -> map overlay mixed-state`
- latest blocker is `startup/load-chain`
- immediate goal: prove why the current run does not produce current-execution loader/runtime signals even when startup artifact truthfulness is already high

## Progress Snapshot

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust | In Progress | startup timeline truthfulness improved, but loader entry and trusted run are still not closed |
| Gameplay Safety | Partially Stable | several stale/no-op regressions were reduced, but authoritative valid-trust gameplay evidence is still limited |
| Replay / Evidence | Partially Stable | parity and artifact quality advanced, but live authoritative replay proof is not yet fully closed |
| Advisor Product | Queued | product surface remains downstream of startup/trust and gameplay stability |

## Current State

### What Is Stable Enough To Build On

- startup artifact truthfulness improved materially via launch delta, module-initializer probe, and startup timeline capture
- `session-summary.json` now reflects live restart state more honestly and no longer overcounts invalid attempts as success
- several combat safety regressions were reduced:
  - stale curse/status non-enemy promotion
  - enemy-turn closure
  - repeated non-enemy stale loop
  - slot-4 combat no-op loop
- replay-step / replay-test / self-test remain the primary offline regression gate

### What Is The Actual Current Blocker

- latest startup roots still do not show current-execution loader/runtime signals
- current-execution sentinel, runtime exporter, harness bridge, and fresh snapshot are all absent in the latest authoritative startup roots
- startup artifact truthfulness is now good enough to say the blocker is in `loader / runtime bootstrap`, not in simple observer heuristics

## Authoritative Roots To Read First

- startup sentinel root:
  - [verify-startup-sentinel-20260319-193531](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-sentinel-20260319-193531)
- startup timeline root:
  - [verify-startup-timeline-20260319-212903](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-startup-timeline-20260319-212903)

핵심 요약:

- latest diagnosis is still conservative
- `earlier positive`와 `later negative`는 이제 같은 root 안에서 분리해 읽을 수 있다
- 하지만 root cause itself is still open: `primary DLL load-chain`이 current execution에서 실제로 initializer/exporter edge까지 닿는지 아직 닫히지 않았다

## What Is Stable

- startup timeline / reviewer summary artifact now separates `latest` from `ever`
- stale snapshot, fresh snapshot, and no-snapshot states are no longer flattened into one missing-state explanation
- deploy verification normalization and startup runtime-config rewrite are stable enough to use in repeated startup investigations
- combat replay spot-checks for known regressions remain intact while startup work proceeds

## What Is Partially Stable

- module-initializer probe exists, but current-execution sentinel is still absent in the latest roots
- runtime exporter and harness bridge paths still need current-execution proof, not just historical traces
- trusted attempt creation is blocked by startup/load-chain, so gameplay confidence cannot yet be promoted to authoritative acceptance

## What Is Not Yet Stable

- current-execution loader entry proof
- runtime bootstrap proof in fresh startup runs
- trusted attempt generation on fresh clean-boot runs
- authoritative live parity proof on a valid-trust attempt

## Current Architecture Direction

- `Smoke Harness` remains a dev-only validation tool, not the product
- startup/trust and gameplay must stay separated in diagnosis and acceptance
- observer/export remains a mixed `event + polling` observer and must not be promoted to scene authority from transient snapshots alone
- authoritative acceptance is still artifact-first and layer-specific

## Immediate Next Steps

1. debug `ModManager.TryLoadModFromPck(...)` branch behavior directly instead of adding more diagnostic layers
2. compare current deployed payload, `.pck` contents, manifest, and primary DLL identity against the decompiled loader contract
3. recover at least one current-execution positive loader/runtime signal
4. only after loader entry and runtime bootstrap are proven, return to trusted attempt closure and gameplay validation

## Deferred Work

- product-surface refinement beyond the current advisor concept
- broader gameplay policy quality work not backed by valid-trust live evidence
- non-critical document pruning outside the canonical overview/handoff set
