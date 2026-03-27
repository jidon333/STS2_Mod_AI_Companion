# 하네스 상태/증거 매트릭스

> 상태: 현재 사용 중
> 기준 문서: 예, for current `main` regression evidence coverage
> 갱신 시점: self-test, replay fixture, representative live root, 또는 상태 계약이 바뀔 때

이 문서는 [HARNESS_REGRESSION_CHECKLIST_KO.md](./HARNESS_REGRESSION_CHECKLIST_KO.md)의 각 상태가 **현재 무엇으로 고정되어 있는지**를 매핑한다.

coverage status 의미:

- `green`: current `main`에서 self-test / replay / representative live root가 모두 충분하거나, 상태 계약을 신뢰할 만큼 evidence가 강함
- `partial`: evidence는 있으나 release/handoff 또는 long-run continuity까지는 덜 고정됨
- `missing`: current `main` 기준 canonical evidence가 부족함

## Immediate Priority Gaps

| Gap | Meaning | Current Status |
|---|---|---|
| `VAL-CAPTURE-BOUNDARY` | phase log 이후 capture/request/failure emission을 bounded contract로 고정 | partial |
| `COMBAT-POSTWAIT-RECAPTURE` | legitimate combat wait 후 recapture/re-request continuity | partial |
| `REWARD-POSTCLICK-RELEASE` | reward skip/proceed 후 release pending -> map handoff contract | partial |

## Validation / Boot / Menu

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `VAL-01` | validation | deploy/runtime identity | green | [SMOKE_TEST_CHECKLIST.md](../runbooks/SMOKE_TEST_CHECKLIST.md), [PROJECT_STATUS.md](./PROJECT_STATUS.md) startup/trust roots | current `main` 운영 규칙 고정 |
| `VAL-02` | validation | manual-clean-boot | green | [ROADMAP.md](../ROADMAP.md) M4, [PROJECT_STATUS.md](./PROJECT_STATUS.md) bootstrap/trust roots | trust gate는 current baseline에 포함 |
| `VAL-03` | any phase post-log | capture boundary | partial | `Program.cs` detailed capture boundary self-tests and bounded failure emission now exist on current `main`; fresh live proof is still pending | silent-hang hardening live evidence still needed |
| `MENU-01` | `WaitMainMenu` / `EnterRun` | main menu | green | `Program.cs` main-menu / enter-run self-tests, [SMOKE_TEST_CHECKLIST.md](../runbooks/SMOKE_TEST_CHECKLIST.md) | continue vs singleplayer contract 고정 |
| `MENU-02` | `EnterRun` | continue lane | green | `GetPostEnterRunPhase`, replay parity summaries, main-menu self-tests | continue preferred 유지 |
| `MENU-03` | `WaitRunLoad` | transition wait | green | `Program.cs` `WaitRunLoad should remain wait-only while explicit transition truth is still active.` | explicit transition truth wait-only |
| `MENU-04` | `WaitRunLoad` | stale continue recovery | green | `Program.cs` `WaitRunLoad should retry Continue...`, `ShouldRetryEnterRunFromWaitRunLoad` self-tests | stable continue retry 고정 |
| `MENU-05` | `WaitRunLoad` | resumed room branch | green | `Program.cs` WaitRunLoad -> reward/event/treasure/combat branch assertions | post-load branch contract strong |

## Map / WaitMap / Post-Node

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `MAP-01` | `ChooseFirstNode` | map-node | green | replay parity fixtures expecting `foregroundOwner=map`, exported map-node self-tests | explicit map node routing strong |
| `MAP-02` | `ChooseFirstNode` / `HandleEvent` | map-overlay foreground | green | `Program.cs` map-overlay signature/assertions, `map-overlay-noop-loop` sentinel self-test | stale event residue suppression covered |
| `MAP-03` | `WaitMap` | room reopen | green | `Program.cs` `WaitMap should reopen reward/event/treasure handling...` assertions | mixed modal canonical foreground owner reopen covered |
| `MAP-04` | `WaitPostMapNodeRoom` | destination room handoff | partial | goal text + phase reconciliation helpers + self-tests around event/combat reopen | needs broader canonical live roots |

## Combat

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `COMBAT-01` | `WaitCombat` | combat acceptance | green | `Program.cs` WaitCombat acceptance assertions (`ready`, `stable`, `inCombat`) | strong acceptance gate |
| `COMBAT-02` | `HandleCombat` | card select | green | combat opener self-test (`combat select attack slot 1`) | select-before-target preserved |
| `COMBAT-03` | `HandleCombat` | target lane | partial | combat no-op sentinel and target selection heuristics in `Program.cs` | lane exists, broader parity matrix thin |
| `COMBAT-04` | `HandleCombat` | cancel blocked selection | green | old blocker replay `0167.request.json` family, commit-era closure evidence referenced in current conversation | explicit cancel lane is required contract |
| `COMBAT-05` | `HandleCombat` | end-turn pre-ack | partial | current code barriers + self-tests from prior workstream; current `main` doc status only indirectly references this | barrier semantics exist but matrix is not yet fully replay-backed on current `main` |
| `COMBAT-06` | `HandleCombat` | acknowledged transit wait | partial | safe-transit plateau fix lives outside current `main` baseline as high-signal branch evidence | canonical main coverage incomplete |
| `COMBAT-07` | `HandleCombat` | enemy-turn closed play phase | partial | legitimate wait semantics exist in current code and self-tests, but current-main replay/live evidence is thin | keep wait semantics explicit |
| `COMBAT-08` | `HandleCombat` | next-round reopen | partial | prior replay/live closure referenced in conversation, not yet promoted to current `main` docs | needs current-main replay/live row |
| `COMBAT-09` | post-wait recapture | capture/request continuity | partial | `Program.cs` bounded capture boundary self-tests now cover timeout/exception/unusable-frame paths; fresh current-main combat wait root still missing | live proof of legitimate wait -> next capture/failure still needed |
| `COMBAT-10` | combat -> rewards | room handoff | green | [PROJECT_STATUS.md](./PROJECT_STATUS.md) long-run continuity roots, repeated combat/reward continuity | strong long-run evidence in status doc |

## Event / Ancient / Treasure / Rest

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `EVENT-01` | `HandleEvent` | explicit event option | green | `Program.cs` event foreground signature/assertions | explicit options beat map fallback |
| `EVENT-02` | `HandleEvent` | explicit event proceed | green | `Program.cs` explicit `EventOption.IsProceed` self-tests | proceed lane tactical + fast wait |
| `EVENT-03` | `HandleEvent` | ancient dialogue | green | `Program.cs` ancient dialogue/completion assertions | explicit ancient lane covered |
| `EVENT-04` | `HandleEvent` / `WaitEventRelease` | ancient dialogue / completion | green | `Program.cs` ancient dialogue/completion allowlist, post-phase, release assertions | ancient lane split strong |
| `EVENT-05` | `HandleEvent` | event reward substate | partial | latest-event sentinel uses reward residue vs latest event, reward/event mixed tests exist | reward substate exists but long-run matrix thin |
| `EVENT-06` | `HandleEvent` / `ChooseFirstNode` | event/map mixed aftermath | green | `live5b`-family self-tests in `Program.cs`, canonical foreground owner assertions | canonical foreground owner rule covered for event/map |
| `TREASURE-01` | `ChooseFirstNode` / `HandleEvent` | treasure room | green | treasure room self-tests, WaitMap reopen to treasure, status doc continuity roots | explicit treasure room state strong |
| `REST-01` | `ChooseFirstNode` | rest-site explicit choice | green | rest-site metadata-first self-tests | authoritative metadata contract strong |
| `REST-02` | `ChooseFirstNode` | rest-site post-click release | green | grace / noop / selection-failed self-tests | explicit release/no-op guard present |

## Rewards

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `REWARD-01` | `HandleRewards` | claim lane | green | `Program.cs` reward fast-path signature and `claim reward item` assertion | claim outranks skip/map |
| `REWARD-02` | `HandleRewards` | reward card / colorless | green | colorless and reward choice self-tests in `Program.cs` | inspect overlay + card choice covered |
| `REWARD-03` | `HandleRewards` | reward skip / proceed | partial | explicit reward proceed/skip is allowed and chosen; post-click release contract not fully fixed on current `main` | same-click reissue gap remains |
| `REWARD-04` | `HandleRewards` | reward back | partial | allowlist + analysis candidate path exist; canonical replay/live evidence thin | lower priority but incomplete |
| `REWARD-05` | `HandleRewards` | reward teardown / release wait | partial | stale reward cleanup self-tests, layered reward state assertions | teardown exists, handoff proof incomplete |
| `REWARD-06` | `HandleRewards` / `WaitMap` | reward/map mixed aftermath | partial | layered reward state, stale cleanup, `reward-map-loop` sentinel self-tests | owner underweight and post-click release both remain high-risk |
| `REWARD-07` | `WaitMap` | reward -> map handoff | partial | reward teardown self-tests and WaitMap reopen assertions | release target exists but canonical handoff contract is still split |
| `REWARD-09` | `HandleRewards` | same skip reissue suppression | missing | no canonical current-main reward state machine yet suppresses same skip on the same authority band | immediate gap |
| `REWARD-10` | `HandleRewards` / `WaitMap` | reward-map loop sentinel | partial | `Program.cs` `reward-map-loop` sentinel self-tests | sentinel exists, but it should remain fallback not primary flow |

## Shop

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `SHOP-01` | `HandleShop` | shop foreground | green | shop authority helpers + self-tests + [PROJECT_STATUS.md](./PROJECT_STATUS.md) shop continuity roots | strong enough |
| `SHOP-02` | `HandleShop` | bounded purchase | green | shop buy relic/card/potion/card-removal decision code and self-tests | one bounded purchase contract exists |
| `SHOP-03` | `HandleShop` | back / proceed / map handoff | green | self-tests around proceed-before-reopen, status doc shop->map continuity | stable on current baseline |

## Terminal / Restart

| State ID | Canonical Phase | Owner / Lane | Coverage | Representative Evidence | Notes |
|---|---|---|---|---|---|
| `TERM-01` | reward/menu terminal boundaries | terminal boundary | green | `RewardObserverSignals.IsTerminalRunBoundary`, terminal suppression tests, [PROJECT_STATUS.md](./PROJECT_STATUS.md) | gameplay map fallback suppression covered |
| `TERM-02` | terminal -> restart -> next attempt | lifecycle lane | missing | [PROJECT_STATUS.md](./PROJECT_STATUS.md), [PROJECT_STATUS_READER_KO.md](./PROJECT_STATUS_READER_KO.md) both call this residual gap out explicitly | strict lifecycle chain still open |

## Current-Main Evidence Index

### Representative Live Roots

- loader/runtime recovery:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap/trust:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- quartet/accounting:
  - [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- parity closeout:
  - [verify-m6-parity-closeout-20260324-0034](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/replay-audit/verify-m6-parity-closeout-20260324-0034)

### Current-Main Self-Test Families

- WaitRunLoad transition hold / stale continue recovery / resumed room branching
- map-overlay foreground / canonical foreground owner
- explicit event proceed / ancient completion / event release
- reward fast-path / layered reward state / stale reward cleanup / reward-map-loop sentinel
- rest-site explicit metadata / post-click grace / no-op classification
- treasure room explicit lane
- WaitCombat acceptance / combat opener

## Immediate Follow-Up Backlog

| Priority | Work Item | Why |
|---|---|---|
| P0 | reward post-click release/handoff canonicalization | `REWARD-03` / `REWARD-05` / `REWARD-06` are still partial and `REWARD-09` is missing |
| P0 | combat post-wait recapture canonical coverage | `COMBAT-09` is code-covered and self-test-covered, but still partial until fresh current-main live evidence exists |
| P1 | owner/action/release duplication inventory for rewards and events | same state meaning still split across multiple helpers |
| P1 | current-main live evidence refresh for noncombat mixed aftermath | status docs are stronger than the state-specific matrix today |
| P2 | strict lifecycle chain evidence | `TERM-02` remains intentionally open |

## Non-Canonical Appendix

아래는 current `main` source-of-truth는 아니지만, 다음 구현 우선순위를 정할 때 참고할 수 있는 recent high-signal branch evidence다.

- clean validation branch의 `live9d` post-wait recapture hang
- clean validation branch의 `live10` reward wait plateau
- clean validation branch의 `live11` reward skip reissue / reward-map-loop

이 appendix는 priority signal일 뿐, canonical coverage status는 위 표의 current `main` evidence만으로 판정한다.
