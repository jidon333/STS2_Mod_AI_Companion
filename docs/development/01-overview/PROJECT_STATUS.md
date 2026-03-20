# Project Status

## Date
- 2026-03-20

## Current Milestone Pointer

- current rail: `startup / trust -> trusted attempt`
- active milestone: `M4. Trusted Attempt 확보`
- next milestone: `M5. 하네스 장기 실행 증거 닫기`
- long-term goal: 사람이 실제 플레이 중 참고할 수 있는 `read-only advisor` 완성

## Current Priority

- current critical path is no longer `loader entry / runtime bootstrap`
- latest blocker is `authoritative attempt / session accounting contract`
- immediate goal: bootstrap-first 이후 `restart-events`, `attempt-index`, `session-summary`, `supervisor-state`가 reviewer-friendly하게 같은 이야기를 하도록 고정하는 것

## Progress Snapshot

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust | Late-stage | loader/runtime positive와 bootstrap-first `0001 valid-at-start`는 확보됐고, 남은 갭은 accounting contract hardening이다 |
| Gameplay Safety | Partially Stable | 여러 stale/no-op regressions는 줄었지만, current active milestone은 아니다 |
| Replay / Evidence | Stable Enough To Build On | replay/self-test와 artifact truthfulness는 강해졌고, 이제 chronology/projection semantics를 더 또렷하게 해야 한다 |
| Advisor Product | Queued | valid-trust evidence 위에서 최소 advisor value slice를 정해야 한다 |

## Current State

### What Is Stable Enough To Build On

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)에서 `runtimeExporterInitializedLogged=true`, `harnessBridgeInitializeLogged=true`, `liveSnapshotPresent=true`가 fresh root 기준으로 회복됐다.
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)에서 bootstrap이 non-attempt pre-phase로 분리됐고, 첫 authoritative attempt `0001`이 `trustStateAtStart:"valid"`로 시작한다.
- deploy identity, manual clean boot, runtime-started snapshot evidence를 fresh root에서 artifact-first로 읽을 수 있다.
- replay-step / replay-test / self-test는 여전히 가장 중요한 offline regression gate다.
- combat safety 쪽 최근 regression fix는 유지되고 있지만, 현재 critical path를 대체하지는 않는다.

### What Is The Actual Current Blocker

- bootstrap-first sequencing 자체는 구현됐다.
- 하지만 `restart-events.ndjson`, `attempt-index.ndjson`, `session-summary.json`, `supervisor-state.json`의 semantics가 아직 충분히 명문화되거나 self-test로 고정되지는 않았다.
- 특히 `supervisor-state.json`은 current attempt와 last terminal attempt를 함께 보여 주는데, `lastAttemptId`는 `expectedCurrentAttemptId`나 `lastTerminalAttemptId`보다 의미가 더 흐리다.
- 따라서 남은 리스크는 `current-execution loader/runtime proof 부재`가 아니라 `session/accounting truth를 reviewer와 operator가 같은 방식으로 읽지 못하는 것`이다.

## Authoritative Roots To Read First

- loader/runtime recovery root:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap-first sequencing root:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)

핵심 요약:

- loader/runtime positive signal은 이제 fresh root에서 보인다
- bootstrap은 session-level pre-attempt phase로 분리됐다
- first authoritative attempt는 `0001 + valid-at-start`로 시작할 수 있다
- 다음 일은 더 많은 startup diagnostics가 아니라 chronology/projection contract hardening이다

## What Is Stable

- `prevalidation.json`의 `modsPayloadReconciled`, `deployIdentityVerified`, `manualCleanBootVerified` gate는 fresh roots에서 신뢰 가능한 증거로 쓸 수 있다.
- `startup-runtime-evidence.json`의 runtime exporter / harness bridge / fresh snapshot positive는 현재 실행 기준으로 확인된다.
- bootstrap-first 이후 첫 authoritative attempt `0001`과 restart progression 초기 구간은 artifact로 재현 가능하다.
- known replay spot-check는 유지되고 있다:
  - `0015 --full-request-rebuild => combat select attack slot 4`
  - `0021 --full-request-rebuild => wait`

## What Is Partially Stable

- `attempt-index.ndjson`와 `session-summary.json`은 유용하지만, `restart-events.ndjson`를 대체하는 chronology source로 읽으면 안 된다.
- `supervisor-state.json`는 machine verdict projection으로는 유용하지만, current attempt / terminal attempt semantics는 더 선명하게 고정할 필요가 있다.
- observer authority classes는 암묵적으로 존재하지만 아직 문서 계약으로 분리돼 있지 않다.
- advisor value slice는 아직 후보 비교 전이며, 어떤 scene이 첫 slice인지 확정하지 않았다.

## What Is Not Yet Stable

- chronology/projection contract가 docs + self-test + live root 해석에서 완전히 일치하는 상태
- `M5. 하네스 장기 실행 증거 닫기`를 반복 가능하게 증명하는 fresh-root cadence
- valid-trust evidence 위에서의 최소 advisor value demonstration

## Current Architecture Direction

- `Smoke Harness`는 계속 dev-only validation tool로 유지한다.
- bootstrap은 attempt가 아니라 session-level pre-attempt phase로 다룬다.
- `restart-events.ndjson`는 chronology source로, `attempt-index.ndjson` / `session-summary.json` / `supervisor-state.json`는 projection으로 읽는다.
- observer/export는 mixed `event + polling` 전략을 유지하고, transient snapshot 하나만으로 scene authority를 주지 않는다.
- 더 이상의 startup diagnostic layer 추가나 schema expansion은 현재 우선순위가 아니다.

## Immediate Next Steps

1. `restart-events.ndjson`, `attempt-index.ndjson`, `session-summary.json`, `supervisor-state.json`의 truth contract를 문서와 self-test로 고정한다.
2. observer authority classes를 문서 계약으로 먼저 정리하되, runtime schema는 당장 늘리지 않는다.
3. valid-trust evidence 위에서 평가할 최소 advisor value slice 후보를 2~3개로 좁힌다.
4. `state / knowledge / recommendation` 레이어 분리를 broad refactor 없이 skeleton 수준으로 정리한다.

## Deferred Work

- 별도 bootstrap-validation scenario split
- 새 startup diagnostic schema 또는 추가 sentinel/timeline layer
- reward/shop/event 중 하나를 첫 advisor slice로 성급히 확정하는 일
- `M5` 전의 광범위한 recommendation engine refactor
