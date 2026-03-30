# 프로젝트 현황

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 현재 milestone pointer, current semantic blocker, representative evidence, 또는 harness architecture state가 바뀔 때

## 날짜

- 2026-03-31

## 현재 마일스톤 위치

- 현재 진행 축: `M5 authoritative long-run blocker loop` + post-refactor cleanup completion + `post-live8 anti-drift recovery`
- 현재 engineering focus:
  1. anti-drift rules를 current docs와 current owner code에 다시 고정
  2. fresh live baseline 위에서 `live21` ancient event option blocker를 explicit `NEventOptionButton` contract로 닫기
  3. 그 다음에야 `deck-remove` coverage frontier와 low-priority smoothness gap으로 다시 돌아가기
- 장기 제품 목표: 사람이 실제 플레이 중 참고하는 `읽기 전용 advisor`

중요한 현재 해석:

- startup / trust / bootstrap / deploy identity는 broad top blocker가 아니다
- `WaitRunLoad` resumed room handoff bug는 이미 닫혔다
- `WaitMainMenu -> EnterRun` logo-animation premature acceptance bug는 current `main`에서 닫혔다
- published-first observer provenance migration은 current `main`에서 active다
- post-refactor cleanup program은 current `main`에서 완료 상태다
- combat stale-end-turn / target plateau family는 current `main`에서 micro-stage + quiet convergence로 닫혔다
- explicit shop foreground 위에 stale reward misroute가 끼어들던 `HandleShop -> HandleRewards -> decision-wait-plateau` family는 current `main`에서 즉시 `HandleRewards -> HandleShop` 회복으로 닫혔다
- 현재 핵심은 **cleanup-complete baseline을 reopen하지 않고 explicit truth 기반 current-main authority contracts를 유지한 채 `live21` ancient event option blocker를 먼저 닫는 것**이다

## 현재 우선순위

### 1. 하네스 구조 기준선

current `main`의 하네스 구조 정리 wave 1-8은 완료됐다.

핵심 결과:

- [Program.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.cs)는 shell-only다
- observer / analysis / decisions / runner / artifacts / self-test가 물리적으로 분리됐다
- `LongRunArtifacts` band 분리, `AutoDecisionProvider` vertical 분리, runner seam 분리, large self-test split이 완료됐다
- 장기 cleanup 기준 문서는 [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)다
- current file owner reference는 [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)다

### 2. 현재 semantic gap

cleanup program 완료 이후에도 current follow-up은 남아 있다.

현재 가장 중요한 signal은 아래다.

1. replay parity suite
   - status: green
   - 해석: old `reward-aftermath-map-handoff` known red는 current `main`에서 닫혔다
2. latest combat blocker fix root
   - root: [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)
   - result: original `combat select non-enemy slot -> stale observer -> auto-end turn -> combat-barrier-wait-plateau` family와 후속 attack-target plateau family가 모두 재현되지 않았다
   - shape:
     - unresolved non-enemy lane는 `end turn`으로 닫히지 않고 lane settle/confirm으로만 진행됐다
     - `step=51`에서 `capture skipped reason=combat-explicit-target-runtime`, `action=click`, `target=combat enemy target 의식의-신수 recenter`
     - `step=51` 이후 settle reason은 generic observer delta가 아니라 `combat-enemy-click-resolved`였다
     - `step=55 -> 60`에서는 unresolved selected lane이 `right-click cancel unresolved selected card`로만 해소됐다
     - run은 `attempt-completed:max-steps-reached`로 끝났고 `failure-summary.json`은 생성되지 않았다
3. latest post-live9 closure commits
   - commits:
     - `15dd5cf` `Prioritize reward-pick child-screen export facts`
     - `d9e4c01` `Make combat barrier self-test use live target ids`
     - `10028f8` `Make combat target-selection self-test live-faithful`
     - `ed0a379` `Wait on unresolved attack-lane churn`
     - `9c6621b` `Narrow combat target-summary carryover`
   - result:
     - repeated `reward-pick` plateau family는 [boot-to-long-run-20260330-live18](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live18)에서 재현되지 않았다
     - combat target-selection / carryover self-test red는 current `main`에서 green으로 닫혔다
     - stale `combatTargetSummary` 기반 attack-lane resurrection/no-op family는 current `main`에서 좁혀졌다
4. latest live blocker
   - root: [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21)
   - result: combat family는 재현되지 않았지만, `HandleEvent`에서 `decision-wait-plateau`가 발생했다
   - shape:
     - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21/attempts/0001/run.log) `step=92~101`은 계속 `waiting for explicit ancient event option buttons`
     - [0101.request.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21/attempts/0001/steps/0101.request.json) 에는 이미 actionable `event-option` 버튼 두 개가 bounds와 함께 들어 있다
     - 하지만 ancient explicit option 판정은 `ancient-event-option:*` / `source:ancient-option-button` / `ancient-event` semantic을 기대하고, live node는 generic `event-option:*` + `source:event-option-button`만 가진다
     - 결과적으로 ancient lane은 active인데 decision은 generic explicit button을 ancient option으로 인정하지 못해 `wait`만 반복한다
   - interpretation:
     - broad mixed-state regression이 아니라 **ancient special-case lane과 generic explicit event-option button export의 contract mismatch**다
5. latest anti-drift recovery commits
   - commits:
     - `bc53c34` `Retire broad card-selection subtype fallback`
     - `f29cc5d` `Retire screenshot-first noncombat recovery paths`
     - `52ebabd` `Reinforce harness anti-drift rules in current docs`
     - `84e4647` `Recover explicit combat and noncombat authority contracts`
     - `5ebe718` `Refresh replay fixtures for exported map routing`
     - `3a24338` `Retire combat screenshot and stale target fallbacks`
   - result:
     - `build`, `self-test`, `replay-test`, `replay-parity-test` are green on current `main`
     - combat false-confirm lane, screenshot-primary combat selection, stale target recenter fallback, exported-map routing drift, and reward screenshot residue were tightened back toward explicit truth
     - fresh authoritative live rerun root [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9) added current-main live closure evidence
     - `live8` combat false-confirm plateau는 재현되지 않았고 run은 `max-steps-reached:120`으로 종료됐다
6. latest anti-drift recovery live root
   - root: [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)
   - result: post-recovery current `main`에서 combat false-confirm family는 재현되지 않았고, 첫 전투는 observer-first attack select + explicit target click으로 진행됐다
   - shape:
     - `step=41` attack slot select는 `captureMode=skipped`, `sceneReasoningMode=observer-only`였다
     - `step=42`는 explicit combat enemy target click으로 이어졌다
     - `deck-remove` child screen과 reward card choice에서만 transient `captured/enriched` recapture가 끼었다
     - run은 `max-steps-reached:120`으로 끝났고 `failure-summary.json`은 생성되지 않았다
7. latest cleanup proof root
   - root: [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
   - result: representative fresh live root, cleanup-complete `main`에서 `WaitMainMenu -> EnterRun`, `WaitRunLoad -> HandleCombat`, combat wait/re-capture continuity가 모두 비회귀였다
   - shape:
     - `step=3`에서 `captureMode=skipped`, `sceneReasoningMode=observer-only`, `choices=[계속, 멀티플레이, 종료, 설정]` 뒤 `target=continue`
     - `step=16`에서 `WaitRunLoad -> HandleCombat`
     - `step=23 -> 24`, `step=45 -> 46`, `step=57 -> 58`에서 legitimate combat wait 뒤 다음 capture/request로 이어졌고 silent stall은 없었다
     - run은 `max-steps-reached:60`로 종료됐고 `failure-summary.json`은 생성되지 않았다
8. latest shop recovery root
   - root: [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4)
   - result: old `HandleShop -> HandleRewards from screen=shop -> decision-wait-plateau phase=HandleRewards screen=shop` family는 current `main`에서 plateau 없이 회복됐다
   - shape:
     - `step=22`와 `step=42`에서 transient `HandleShop -> HandleRewards from screen=shop`가 여전히 관찰된다
     - 하지만 각각 `step=24`, `step=43`에서 즉시 `HandleRewards -> HandleShop from screen=shop`가 일어나고 shop lane이 계속 진행된다
     - 이후 `step=25 -> 27`, `step=44 -> 46`에서 open-inventory / buy / back / proceed가 실제로 진행됐다
     - run은 combat, reward, rest-site까지 계속 이어진 뒤 `max-steps-reached:120`으로 종료됐고 `failure-summary.json`은 생성되지 않았다

즉 현재 질문은 더 이상

```text
"reward aftermath 이후 ChooseFirstNode / WaitMap / map-node routing이 막힌다"
```

가 아니다.

historical frontier는

```text
"mixed-state noncombat guard cleanup, combat EndTurn pre-actuation drift, combat post-wait recapture continuity는 current main에서 닫혔고,
capture-boundary와 strict lifecycle proof도 current main에서 닫혔고,
combat stale-end-turn / target plateau family도 current main에서 닫혔고,
현재 next step은 `live9` 이후 남은 `deck-remove` / `reward-pick` child-screen explicit owner/export gap을 좁히는 것이다"
```

### 3. current speed baseline

- speed recovery program의 핵심 목표였던 `capture-first -> observer-first, screenshot-on-demand` 전환은 historical baseline으로 current docs에 남아 있다
- speed proof root:
  - [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- 해석:
  - explicit event recovery chain `step=8~9`는 `captureMode=skipped`, `sceneReasoningMode=observer-only`
  - common combat chain `step=11~17`도 `captureMode=skipped`, `sceneReasoningMode=observer-only`
  - representative `preflight->request`는 `1155~1466ms` band로 내려왔다
  - same root의 attempt는 `step=19`에서 `returned-main-menu` terminal로 끝났지만, speed proof 관점에서는 captured hot path가 0건이라는 점이 핵심이다
  - `live8` regression root에서 combat false-confirm + screenshot-heavy slowdown이 다시 드러났지만, current `main`에는 이를 되돌린 recovery commits (`84e4647`, `5ebe718`, `3a24338`)가 반영됐다
  - fresh live root [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)에서 combat false-confirm plateau는 재현되지 않았고 common combat lane은 다시 observer-first로 진행됐다
  - 다만 `deck-remove`와 reward card child screen은 여전히 transient `captured/enriched` recapture가 남아 있어, overall speed/smoothness baseline은 partial로 유지한다
  - 이 root는 semantic blocker root가 아니라 speed/capture baseline evidence로만 본다

## 진행 스냅샷

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust / Deploy Identity | green | broad top blocker 아님, latest live root에서 main-menu phase-boundary도 비회귀 |
| Harness Architecture | green | wave 1-8 complete, shell/module split complete |
| Build / Shared Self-Test | green | current `main` 기준 통과 |
| Harness Self-Test | green | `84e4647` current-main recovery 이후 self-test green |
| Replay Golden Suite | green | `replay-test` 통과 |
| Replay Parity Suite | green | `reward-aftermath-map-handoff` 포함 current parity fixtures green |
| Non-Combat Stability | green | reward aftermath map-node continuity closure, fresh live root confirms post-reward progression |
| Combat Stability | green | `live9`에서 false-confirm plateau가 재현되지 않았고 first combat가 observer-first select + explicit target click으로 진행 |
| Live-Run Speed Recovery | partial | combat hot path fresh live proof는 회복됐지만 `deck-remove` / reward card child screen에서 transient `captured/enriched` residue가 남아 있음 |
| Observer Provenance Migration | green | bridge / tracker / reader / harness control-flow가 published-first baseline으로 정리됐고 compatibility는 legacy surface로만 남는다 |
| Post-Refactor Cleanup Program | green | runner residual, noncombat residue, partial `Program` owner shedding, supervision health split까지 current `main`에 반영 |
| Capture Boundary | green | bounded failure contract와 self-test 위에 fresh live proof [capture-boundary-proof-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1)이 추가됐다 |
| Strict Lifecycle Chain | green | fresh live root [strict-lifecycle-chain-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2)에서 `0001 terminal -> 0002 restart -> 0002 next-attempt-started` chronology와 first-screen proof가 닫혔다 |

## 현재 바로 믿을 수 있는 것

- `build`, `Sts2ModKit.SelfTest`, `replay-test`, `replay-parity-test`는 current `main`에서 green이다
- `Sts2GuiSmokeHarness -- self-test`도 current `main`에서 green이다
- replay parity는 current `main` 기준 green이다
- `WaitRunLoad -> HandleRewards` resumed room handoff fix는 current `main`에 반영되어 있다
- reward aftermath `ChooseFirstNode` exported-node closure는 current `main`에 반영되어 있다
- `WaitMainMenu`는 current `main`에서 actual `Continue` / `Singleplayer` run-start surface가 나오기 전에는 accepted 되지 않는다
- current combat/noncombat owner code는 screenshot-primary fallback이 아니라 explicit truth 우선 기준으로 다시 묶여 있다
- historical speed proof root에서 explicit event/combat chain의 `captureMode`는 전부 `skipped`였고, current `main`은 그 baseline을 fresh live로 다시 확인해야 하는 상태다
- fresh live root `live9`에서 combat false-confirm plateau는 재현되지 않았고 first combat의 select/target chain은 다시 `observer-only` / explicit target authority로 진행됐다
- `15dd5cf` 이후 repeated reward-pick plateau family는 current `main`에서 닫혔다
- `9c6621b` 이후 stale combat carryover/no-op family도 current `main`에서 닫혔다
- current `main`의 latest authoritative live blocker는 `live21` ancient event option wait plateau다
- current `main`의 control-flow observer provenance는 published-first이고, published provenance는 legacy `visibleScreen` / `sceneReady` 계열로 다시 채워지지 않는다
- bridge node semantics는 compatibility scene winner를 다시 먹지 않는다
- combat post-action은 더 이상 generic observer delta 하나로 다음 step을 열지 않고, lane micro-stage + quiet convergence로 settle된다
- runtime `combatTargetSummary` raw fact는 current `main`에서 explicit enemy-target authority로 소비된다
- `AwaitingCardPlayConfirm`는 current `main`에서 stale history/barrier shadow만으로 열리지 않고, positive runtime/export evidence가 있어야 열린다
- explicit shop foreground는 stale reward leftovers보다 강하며, 잘못 `HandleRewards`로 들어가도 current `main`에서는 즉시 `HandleShop`으로 회복된다
- 하네스 구조 refactor는 current `main` 기준 문서화 가능한 수준까지 정리됐다
- post-refactor cleanup program도 current `main` 기준 완료 상태다
- exported map routing은 current `main`에서 `click exported reachable node` 우선 계약으로 다시 정렬됐다

## 현재 대표 evidence

### 구조 / validation baseline

- cleanup baseline contract:
  - [GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/GUI_SMOKE_HARNESS_MODULE_BOUNDARIES.md)
- current harness architecture:
  - [GUI_SMOKE_HARNESS_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_ARCHITECTURE.md)
- human-readable before/after comparison:
  - [GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_REFACTOR_BEFORE_AFTER.md)
- glossary:
  - [GUI_SMOKE_HARNESS_GLOSSARY_KO.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/reference/harness/GUI_SMOKE_HARNESS_GLOSSARY_KO.md)
- startup/deploy sequencing:
  - [STARTUP_DEPLOY_CONTROL_LAYER.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/STARTUP_DEPLOY_CONTROL_LAYER.md)
- runner/supervisor chronology:
  - [RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/RUNNER_SUPERVISOR_AGENT_ARCHITECTURE.md)

### current semantic closure evidence

- fresh combat blocker fix root:
  - [combat-target-summary-20260330-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2)
- fresh shop recovery root:
  - [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4)
- anti-drift recovery live root:
  - [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)
- combat blocker fix startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/startup-summary.json)
- combat blocker fix session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/session-summary.json)
- combat blocker fix run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-target-summary-20260330-live2/attempts/0001/run.log)
- shop recovery run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4/attempts/0001/run.log)
- anti-drift recovery validation summary:
  - [validation-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9/attempts/0001/validation-summary.json)
- anti-drift recovery run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9/attempts/0001/run.log)
- fresh reward/map closure root:
  - [reward-aftermath-owner-truth-20260328-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/reward-aftermath-owner-truth-20260328-live1)
- latest cleanup proof root:
  - [20260329-162955-boot-to-long-run](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run)
- latest cleanup proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/startup-summary.json)
- latest cleanup proof session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/session-summary.json)
- latest cleanup proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/20260329-162955-boot-to-long-run/attempts/0001/run.log)
- main-menu boundary proof root:
  - [wait-main-menu-run-start-readiness-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1)
- main-menu boundary startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/startup-summary.json)
- main-menu boundary session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/session-summary.json)
- main-menu boundary run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/wait-main-menu-run-start-readiness-20260329-live1/attempts/0001/run.log)
- speed recovery proof root:
  - [observer-first-speed-20260328-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9)
- speed proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/startup-summary.json)
- speed proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/run.log)
- speed proof terminal summary:
  - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-first-speed-20260328-live9/attempts/0001/failure-summary.json)
- observer provenance split proof root:
  - [observer-compat-shadow-retirement-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/observer-compat-shadow-retirement-20260329-live1)
- capture-boundary proof root:
  - [capture-boundary-proof-20260329-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1)
- capture-boundary proof startup summary:
  - [startup-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/startup-summary.json)
- capture-boundary proof session summary:
  - [session-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/session-summary.json)
- capture-boundary proof run log:
  - [run.log](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/attempts/0001/run.log)
- capture-boundary proof failure summary:
  - [failure-summary.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/capture-boundary-proof-20260329-live1/attempts/0001/failure-summary.json)
- strict lifecycle proof root:
  - [strict-lifecycle-chain-20260329-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2)
- strict lifecycle restart chronology:
  - [restart-events.ndjson](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/restart-events.ndjson)
- strict lifecycle supervisor state:
  - [supervisor-state.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/supervisor-state.json)
- strict lifecycle second-attempt first screen:
  - [0001.screen.png](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/strict-lifecycle-chain-20260329-live2/attempts/0002/steps/0001.screen.png)
- parity fixture now green:
  - `tests/replay-fixtures/m6-parity/reward-map-handoff.request.json`

## 현재 engineering 해석

### 이미 닫힌 것

- deploy / trust / manual clean boot / authoritative attempt creation
- `WaitRunLoad` resumed room authority handoff
- reward aftermath `ChooseFirstNode` exported reachable map-node handoff
- `reward-aftermath-map-handoff` replay parity known red
- mixed-state local guard residue that duplicated canonical owner truth
- combat EndTurn pre-actuation observer-drift cancellation
- combat EndTurn barrier arming from `observer-drift` history instead of actual sent actions
- screenshot-first explicit event recovery
- screenshot-first common combat attack / target / non-enemy confirm / end-turn chain
- `WaitMainMenu` logo-animation-only premature acceptance
- published provenance backfill from legacy `meta.visibleScreen` / `sceneReady` / `sceneAuthority` / `sceneStability`
- bridge node semantics driven by compatibility scene winner
- monolithic `Program.cs` 중심 구조
- large self-test hotspot 1차 분해
- runner residual / noncombat residue / partial `Program` helper owner / supervision health band hotspot extraction
- fresh live capture-boundary bounded failure proof
- fresh live strict lifecycle restart chronology proof
- combat broader target parity/live proof
- enemy-turn closed play-phase replay/live proof
- combat non-enemy stale-end-turn blocker family
- combat attack-target plateau on stale zero-count target aggregates
- reward back canonical replay proof
- post-node destination continuity handoff coverage
- repeated reward-pick child-screen plateau family
- stale combat target-summary carryover / attack-lane churn family

### 아직 열려 있는 것

- no known red in `build` / `self-test` / `replay-test` / `replay-parity-test`
- current authoritative live blocker는 `live21` ancient event option wait plateau다
- ancient event option phase에서 explicit `NEventOptionButton` contract와 ancient special-case lane이 어긋난다
- `deck-remove` child screen explicit owner/export gap 때문에 transient `captured/enriched` recapture가 남아 있다
- some low-priority coverage rows (`event reward substate`, `reward-map loop sentinel`) remain partial

## 다음 작업 원칙

1. 새 semantic fix는 좁게 한다
   - refactor wave는 현재 완료 상태를 reopen하지 않는다
2. 다음 coverage follow-up은 current owner 파일에서만 본다
   - active noncombat smoothness owner:
      - [RuntimeSnapshotReflectionExtractor.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs)
      - [Observer/AncientEventObserverSignals.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Observer/AncientEventObserverSignals.cs)
      - [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
      - [Program.AllowedActions.NonCombat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
   - retained combat owner baseline:
     - [Analysis/CombatMicroStageSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatMicroStageSupport.cs)
     - [Analysis/CombatPostActionObservationSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatPostActionObservationSupport.cs)
     - [Program.AllowedActions.Combat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
     - [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
     - [Analysis/CombatBarrierSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatBarrierSupport.cs)
     - [Analysis/CombatTargetabilitySupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatTargetabilitySupport.cs)
3. semantic blocker, speed evidence, coverage gap을 구분한다
   - noncombat mixed-state와 combat EndTurn barrier family는 닫혔다
   - explicit event/common combat speed baseline도 현재 `main`에서 회복됐다
   - post-refactor cleanup program도 완료됐다
   - 현재 immediate blocker는 ancient event option contract mismatch이고, 그 다음이 `deck-remove` coverage frontier와 일부 low-priority partial row다

## 한 줄 요약

```text
current main의 smoke harness architecture refactor는 완료됐다.
reward aftermath live/parity gap과 combat stale-end-turn / target plateau family도 current main에서 닫혔다.
anti-drift recovery 뒤 `live9` fresh root까지 확보되어 combat false-confirm plateau는 재현되지 않았다.
reward-pick repeated plateau와 combat target-summary carryover family도 current main에서 닫혔다.
다만 새 authoritative live blocker로 `live21` ancient event option wait plateau가 올라왔다.
다음 follow-up은 ancient option phase를 generic explicit `NEventOptionButton` contract와 다시 정렬한 뒤, 그 다음 `deck-remove` coverage frontier와 low-priority partial row를 보강하는 쪽이다.
```
