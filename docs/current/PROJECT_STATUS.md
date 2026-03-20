# Project Status

> Status: Live Current
> Source of truth: Yes
> Update when: active milestone, current blocker, authoritative roots, or immediate next steps change.

## Date
- 2026-03-20

## Current Milestone Pointer

- current rail: `trusted attempt -> advisor value slice`
- active milestone: `M5. 하네스 장기 실행 증거 닫기`
- next milestone: `M6. Replay/Parity 회귀 게이트 고정`
- long-term goal: 사람이 실제 플레이 중 참고할 수 있는 `read-only advisor` 완성

## Current Priority

- current critical path is no longer `startup / trust / accounting`
- latest blocker is `valid-trust evidence 위의 card reward advisor usefulness closure`
- immediate goal: deterministic `card reward` advice artifact를 reviewer가 실제로 참고 가치 있다고 판정할 수 있게 닫는 것

## Progress Snapshot

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust | Materially Closed Enough To Build On | loader/runtime positive, bootstrap-first `0001 valid-at-start`, quartet semantics가 모두 닫혔다 |
| Gameplay Safety | Partially Stable | 여러 stale/no-op regressions는 줄었지만, current active milestone은 아니다 |
| Replay / Evidence | Stable Enough To Build On | replay/self-test와 reward deterministic context parity 기반은 다음 advisor slice 검증을 받쳐 줄 수 있다 |
| Advisor Product | Pilot In Progress | `card reward` deterministic middle layer는 들어갔고, 이제 usefulness closure가 남았다 |

## Current State

### What Is Stable Enough To Build On

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)에서 `runtimeExporterInitializedLogged=true`, `harnessBridgeInitializeLogged=true`, `liveSnapshotPresent=true`가 fresh root 기준으로 회복됐다.
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)에서 bootstrap이 non-attempt pre-phase로 분리됐고, 첫 authoritative attempt `0001`이 `trustStateAtStart:"valid"`로 시작한다.
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)에서 `restart-events` chronology와 `session-summary / supervisor-state` projection이 같은 attempt truth를 가리키도록 정렬됐다.
- deploy identity, manual clean boot, runtime-started snapshot evidence를 fresh root에서 artifact-first로 읽을 수 있다.
- replay-step / replay-test / self-test는 여전히 가장 중요한 offline regression gate다.
- `CompanionHost`의 advice path와 diagnostics path는 이전보다 분리됐고, live path는 Foundation prompt/knowledge 경로를 primary로 쓴다.
- `card reward` scene에는 deterministic `option set / assessment facts / trace`가 들어갔고, non-card reward에서는 이 layer가 꺼지도록 scope correction까지 반영됐다.
- combat safety 쪽 최근 regression fix는 유지되고 있지만, 현재 critical path를 대체하지는 않는다.

### What Is The Actual Current Blocker

- startup/trust/accounting rail은 지금 당장 다시 열 필요가 없다.
- 현재 부족한 것은 `card reward` advice가 실제로 도움이 되는지에 대한 reviewer-facing closure다.
- deterministic reward layer는 이미 들어갔지만, curated card reward scenes에서
  - `recommendedChoiceLabel`
  - `reasoningBullets`
  - `missingInformation`
  - `decisionBlockers`
  - `RewardRecommendationTrace`
  를 근거로 “실제로 참고 가치 있다”를 닫는 acceptance가 아직 없다.
- 따라서 남은 리스크는 더 이상의 startup diagnostic gap이 아니라 `first advisor value slice`를 아직 제품 가치로 증명하지 못했다는 점이다.

## Authoritative Roots To Read First

- loader/runtime recovery root:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap-first sequencing root:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- quartet/accounting contract root:
  - [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)

핵심 요약:

- loader/runtime positive signal은 이제 fresh root에서 보인다
- bootstrap은 session-level pre-attempt phase로 분리됐다
- first authoritative attempt는 `0001 + valid-at-start`로 시작할 수 있다
- quartet semantics는 reviewer/operator 관점에서 충분히 정렬됐다
- 다음 일은 더 많은 startup diagnostics가 아니라 `card reward advisor usefulness`를 닫는 것이다

## What Is Stable

- `prevalidation.json`의 `modsPayloadReconciled`, `deployIdentityVerified`, `manualCleanBootVerified` gate는 fresh roots에서 신뢰 가능한 증거로 쓸 수 있다.
- `startup-runtime-evidence.json`의 runtime exporter / harness bridge / fresh snapshot positive는 현재 실행 기준으로 확인된다.
- bootstrap-first 이후 첫 authoritative attempt `0001`과 quartet semantics는 artifact로 재현 가능하다.
- `CompanionHost`의 live advice path는 Foundation prompt/knowledge path를 primary로 사용한다.
- `card reward` deterministic layer는 `card reward only` gate까지 포함해 self-test로 고정돼 있다.
- known replay spot-check는 유지되고 있다:
  - `0015 --full-request-rebuild => combat select attack slot 4`
  - `0021 --full-request-rebuild => wait`

## What Is Partially Stable

- diagnostics service는 분리됐지만 `collector mode off`에서도 live mirror/current-run write 같은 diagnostics work가 일부 hot path에 남아 있다.
- WPF/Advisor는 normalized scene authority를 우선 소비하지만, choice 표시 등은 여전히 raw snapshot 의존이 남아 있다.
- observer authority classes는 코드상 강하지만, 전 계층 소비 계약이 완전히 문서화된 상태는 아니다.
- reward advice usefulness는 deterministic context는 생겼지만 reviewer-facing closure가 아직 없다.

## What Is Not Yet Stable

- `M5. 하네스 장기 실행 증거 닫기`를 반복 가능하게 증명하는 clean long-run cadence
- curated `card reward` scenes에서 live/replay를 함께 묶은 usefulness acceptance
- reward 다음 scene class를 어디로 확장할지에 대한 product-value 기준

## Current Architecture Direction

- `Smoke Harness`는 계속 dev-only validation tool로 유지한다.
- bootstrap은 attempt가 아니라 session-level pre-attempt phase로 다룬다.
- `restart-events.ndjson`는 chronology source로, `attempt-index.ndjson` / `session-summary.json` / `supervisor-state.json`는 projection으로 읽는다.
- observer/export는 mixed `event + polling` 전략을 유지하고, transient snapshot 하나만으로 scene authority를 주지 않는다.
- `card reward` advice는 deterministic `option set / assessment facts / trace`를 거쳐 prompt로 들어간다.
- 더 이상의 startup diagnostic layer 추가나 broad recommendation refactor는 현재 우선순위가 아니다.

## Immediate Next Steps

1. curated `card reward` scenes 3~5개를 정하고, valid-trust/live 또는 replay 기준의 평가 세트를 만든다.
2. reward advice artifact만 보고 reviewer가 usefulness를 판정할 수 있는 acceptance를 고정한다.
3. live/replay reward parity에서 deterministic option/fact context와 recommendation output을 같이 확인한다.
4. reward slice를 닫은 뒤에만 다음 scene class(`event / shop / rest`)를 선택한다.

## Deferred Work

- 새 startup diagnostic schema 또는 추가 sentinel/timeline layer
- observer authority runtime schema 확장
- reward 다음 scene을 근거 없이 확장하는 일
- `card reward` usefulness closure 전의 광범위한 recommendation engine refactor
