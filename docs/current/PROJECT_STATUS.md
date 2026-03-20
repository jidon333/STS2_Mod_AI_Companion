# Project Status

> Status: Live Current
> Source of truth: Yes
> Update when: active milestone, current blocker, authoritative roots, or immediate next steps change.

## Date
- 2026-03-21

## Current Milestone Pointer

- current rail: `trusted attempt -> advisor value slice`
- active milestone: `M5. 하네스 장기 실행 증거 닫기`
- next milestone: `M6. Replay/Parity 회귀 게이트 고정`
- long-term goal: 사람이 실제 플레이 중 참고할 수 있는 `read-only advisor` 완성

## Current Priority

- current critical path is no longer `startup / trust / accounting`
- latest blocker is `first combat clearability / first card reward capture reliability`
- immediate goal: decompiled-backed combat runtime state를 export해서 `selected card / targeting / play finished`를 하네스가 screenshot heuristic보다 우선 소비하게 만드는 것

## Progress Snapshot

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust | Materially Closed Enough To Build On | loader/runtime positive, bootstrap-first `0001 valid-at-start`, quartet semantics가 모두 닫혔다 |
| Gameplay Safety | Current Active Blocker | 첫 공격 lane은 일부 개선됐지만, non-enemy/self card selection-confirm-use cycle이 아직 불안정하다 |
| Replay / Evidence | Stable Enough To Build On | curated reward 3개 scenario의 deterministic parity/self-test scaffold는 강하지만, 현재는 reward live bundle보다 first combat clearability가 먼저다 |
| Advisor Product | Acceptance Scaffold Ready | `card reward` deterministic layer와 honesty pass는 들어갔고, first card reward capture가 열리면 reviewer-facing live bundle closure로 복귀할 수 있다 |

## Current State

### What Is Stable Enough To Build On

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)에서 `runtimeExporterInitializedLogged=true`, `harnessBridgeInitializeLogged=true`, `liveSnapshotPresent=true`가 fresh root 기준으로 회복됐다.
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)에서 bootstrap이 non-attempt pre-phase로 분리됐고, 첫 authoritative attempt `0001`이 `trustStateAtStart:"valid"`로 시작한다.
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)에서 `restart-events` chronology와 `session-summary / supervisor-state` projection이 같은 attempt truth를 가리키도록 정렬됐다.
- deploy identity, manual clean boot, runtime-started snapshot evidence를 fresh root에서 artifact-first로 읽을 수 있다.
- replay-step / replay-test / self-test는 여전히 가장 중요한 offline regression gate다.
- `CompanionHost`의 advice path와 diagnostics path는 이전보다 분리됐고, live path는 Foundation prompt/knowledge 경로를 primary로 쓴다.
- `card reward` scene에는 deterministic `option set / assessment facts / trace`가 들어갔고, non-card reward에서는 이 layer가 꺼지도록 scope correction까지 반영됐다.
- reward finalizer는 minimal normalization contract로 줄었고, Host의 second finalization도 제거됐다.
- curated `card reward` 3개 scenario는 self-test에서
  - prompt-pack parity
  - minimal-finalizer honesty
  - model-rationale artifact readability
  - non-first-choice case
  까지 확인된다.
- optional `ffmpeg` review capture는 repo에 통합됐고, 실패 run triage 보조 레이어로 사용할 수 있다.
- combat safety 쪽 최근 regression fix는 유지되지만, 첫 공격 이후 `non-enemy/self` card 사용 사이클은 아직 runtime truth 없이 screenshot heuristic에 과하게 의존한다.

### What Is The Actual Current Blocker

- startup/trust/accounting rail은 지금 당장 다시 열 필요가 없다.
- deterministic reward scaffold도 다시 뜯을 필요가 없다.
- reward live bundle closure는 여전히 중요하지만, 그 전에 실제 live run이 첫 전투를 안정적으로 끝내지 못한다.
- 현재 구체 증상은 첫 공격 뒤 `defend` 같은 non-enemy/self card를 선택해도 선택/확정/소비 사이클을 안정적으로 닫지 못하고, stale no-op carryover와 end-turn fallback이 끼어드는 것이다.
- 즉 남은 리스크는 reward formatting이 아니라 `selected card / targeting / play finished`를 screenshot heuristic으로만 추정하는 데서 오는 combat actuation ambiguity다.

### Current Authoritative Diagnosis

- 첫 공격 lane false no-op는 일부 완화됐다.
- 하지만 non-enemy/self lane은 여전히 unresolved다.
- 현재 가장 중요한 해석은:
  - hand/card 정보 자체는 대체로 맞게 읽고 있다
  - 문제는 `defend` 같은 non-enemy/self 카드의 `selection -> confirm/use -> finished` cycle을 runtime truth 없이 screenshot으로만 닫으려 한다는 점이다
  - 그 사이 old attack no-op carryover가 살아 있어서 end-turn fallback이 끼어든다

## Authoritative Roots To Read First

- loader/runtime recovery root:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap-first sequencing root:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- quartet/accounting contract root:
  - [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- reward bundle evidence-insufficient root:
  - [verify-card-reward-reviewer-bundle-20260320-1545](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-card-reward-reviewer-bundle-20260320-1545)
- first card reward capture failure root:
  - [verify-first-card-reward-capture-20260320-232002-run1](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-first-card-reward-capture-20260320-232002-run1)

핵심 요약:

- loader/runtime positive signal은 이제 fresh root에서 보인다
- bootstrap은 session-level pre-attempt phase로 분리됐다
- first authoritative attempt는 `0001 + valid-at-start`로 시작할 수 있다
- quartet semantics는 reviewer/operator 관점에서 충분히 정렬됐다
- curated reward scaffold는 충분히 정직해졌다
- 다음 일은 reward live bundle을 바로 다시 돌리는 게 아니라, first combat clearability를 막는 runtime state blindspot을 decompiled-backed export로 줄이는 것이다

## What Is Stable

- `prevalidation.json`의 `modsPayloadReconciled`, `deployIdentityVerified`, `manualCleanBootVerified` gate는 fresh roots에서 신뢰 가능한 증거로 쓸 수 있다.
- `startup-runtime-evidence.json`의 runtime exporter / harness bridge / fresh snapshot positive는 현재 실행 기준으로 확인된다.
- bootstrap-first 이후 첫 authoritative attempt `0001`과 quartet semantics는 artifact로 재현 가능하다.
- `CompanionHost`의 live advice path는 Foundation prompt/knowledge path를 primary로 사용한다.
- `card reward` deterministic layer는 `card reward only` gate까지 포함해 self-test로 고정돼 있다.
- curated `card reward` 3개 scenario는 self-test에서 live/replay prompt-pack parity와 reviewer-readable artifact scaffold를 검증한다.
- decompiled 기준으로 combat runtime state의 더 좋은 후보는 이미 확인됐다:
  - `NPlayerHand.InCardPlay`
  - `NPlayerHand._currentCardPlay`
  - `NPlayerHand.IsAwaitingPlay(...)`
  - `NTargetManager.IsInSelection`
  - `NCardPlay.Finished(success)`
  - `CombatHistory.CardPlayStarted/CardPlayFinished`
- known replay spot-check는 유지되고 있다:
  - `0015 --full-request-rebuild => combat select attack slot 4`
  - `0021 --full-request-rebuild => wait`

## What Is Partially Stable

- diagnostics service는 분리됐지만 `collector mode off`에서도 live mirror/current-run write 같은 diagnostics work가 일부 hot path에 남아 있다.
- WPF/Advisor는 normalized scene authority를 우선 소비하지만, choice 표시 등은 여전히 raw snapshot 의존이 남아 있다.
- observer authority classes는 코드상 강하지만, 전 계층 소비 계약이 완전히 문서화된 상태는 아니다.
- reward model-rationale self-test는 여전히 fake client scaffold이므로, 실제 모델 품질 증거로 읽으면 안 된다.
- curated parity/usefulness scaffold는 강해졌지만, 실제 live valid-trust bundle은 아직 없다.
- optional `ffmpeg` review capture는 붙었지만, 현재 사용한 일부 ffmpeg binary는 `offset_x` 옵션을 지원하지 않아 video 파일이 비지 않을 수 있다. video는 review aid일 뿐 canonical evidence가 아니다.

## What Is Not Yet Stable

- `M5. 하네스 장기 실행 증거 닫기`를 반복 가능하게 증명하는 clean long-run cadence
- 첫 전투 이후 non-enemy/self card selection-confirm-use cycle의 runtime-state-backed 안정화
- 고정된 curated `card reward` 3개 scenario의 valid-trust live bundle
- reward 다음 scene class를 어디로 확장할지에 대한 product-value 기준

## Current Architecture Direction

- `Smoke Harness`는 계속 dev-only validation tool로 유지한다.
- bootstrap은 attempt가 아니라 session-level pre-attempt phase로 다룬다.
- `restart-events.ndjson`는 chronology source로, `attempt-index.ndjson` / `session-summary.json` / `supervisor-state.json`는 projection으로 읽는다.
- observer/export는 mixed `event + polling` 전략을 유지하고, transient snapshot 하나만으로 scene authority를 주지 않는다.
- observer/actuator가 애매한 live state에서 막히면, screenshot heuristic을 더 덧칠하기 전에 먼저 `artifacts/knowledge/decompiled`에서 그 state를 직접 설명하는 runtime 기준을 찾는다.
- 즉 scene authority뿐 아니라 combat selection / card-play pending / confirm-ready 같은 판단도 가능하면 decompiled-backed state 기준(`InCardPlay`, `_currentCardPlay`, `IsAwaitingPlay` 같은 실제 runtime 신호 후보)부터 찾고, screenshot rule은 마지막 보조로만 늘린다.
- `card reward` advice는 deterministic `option set / assessment facts / trace`를 거쳐 prompt로 들어간다.
- 더 이상의 startup diagnostic layer 추가나 broad recommendation refactor는 현재 우선순위가 아니다.
- curated reward acceptance set은 다음 reviewer bundle work unit에서도 여전히 3개 scenario로 고정해 읽는다.

## Immediate Next Steps

1. combat card-play runtime state를 decompiled-backed 기준으로 export한다.
   - `selected / pending`
   - `targeting in progress`
   - `play finished`
2. 하네스는 이 runtime state를 screenshot heuristic보다 우선 소비하게 바꾼다.
3. 목표는 first combat clearability를 올려 fresh valid-trust run에서 첫 `card reward` screen을 최소 1건 확보하는 것이다.
4. first card reward capture가 열리면, 그 다음 work unit에서 `valid-trust live reward reviewer bundle`로 복귀한다.

## Deferred Work

- 새 startup diagnostic schema 또는 추가 sentinel/timeline layer
- observer authority runtime schema 확장
- reward 다음 scene을 근거 없이 확장하는 일
- `card reward` usefulness closure 전의 광범위한 recommendation engine refactor
