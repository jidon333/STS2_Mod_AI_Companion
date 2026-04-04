# 프로젝트 현황

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 현재 milestone pointer, current semantic blocker, representative evidence, 또는 harness architecture state가 바뀔 때

## 날짜

- 2026-04-04

## 현재 마일스톤 위치

- 현재 진행 축: `M9 advice-quality entry`, M5~M8 substrate acceptance completed
- 현재 engineering focus:
  1. fresh live long run 2회 연속 success를 current docs/handoff baseline으로 승격하고, M5 exit를 문서상으로도 확정하기
  2. replay/parity gate, 비전투 흐름, 전투 안전성은 current `main`에서 M5 acceptance에 사실상 포함된 것으로 정리하고 M6~M8을 reopened blocker가 없는 한 closed substrate baseline으로 다루기
  3. 다음 active workstream을 harness substrate stabilization이 아니라 `M9 advice-quality`로 전환하되, 새 blocker가 나오면 M5~M8 reopen으로 따로 분류하기
- 장기 제품 목표: 사람이 실제 플레이 중 참고하는 `읽기 전용 advisor`

중요한 현재 해석:

- current `main`의 active pointer는 더 이상 `combat EndTurn submit-path mismatch`가 아니다
- latest authoritative fresh roots [endurance-longrun-20260404-live28](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live28) 와 [endurance-longrun-20260404-live29](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live29) 가 연속으로 `player-defeated` natural terminal까지 갔다
- current `main` HEAD `68e91fd` 는 accepted heal 이후 rest-site release-pending을 canonical contract로 고정했고, live29에서 heal aftermath `rest -> release pending -> proceed -> map` continuity가 fresh root로 다시 확인됐다
- current `main` 기준으로 M5 acceptance는 충족됐다
- current milestone layout상 M6 replay/parity gate, M7 비전투 안정화, M8 전투 안정화는 이미 M5 acceptance evidence를 지탱하는 하부 조건으로 current `main`에서 같이 충족된 상태다
- 따라서 current position은 `M5 late acceptance-evidence`가 아니라 `M9 entry with M5~M8 substrate complete`로 읽는 것이 더 정확하다
- startup / trust / bootstrap / deploy identity는 broad top blocker가 아니다
- `WaitRunLoad` resumed room handoff bug는 이미 닫혔다
- `WaitMainMenu -> EnterRun` logo-animation premature acceptance bug는 current `main`에서 닫혔다
- published-first observer provenance migration은 current `main`에서 active다
- post-refactor cleanup program은 current `main`에서 완료 상태다
- ancient option contract family는 live에서 닫혔다
- `ChooseFirstNode <-> event` mixed-state와 post-node combat takeover -> generic map wait family도 live에서 닫혔다
- explicit relic reward claim -> `proceed after resolving rewards` inversion은 replay exact repro 기준으로 닫혔고, fresh live rerun에서 재현되지 않았다
- explicit shop foreground 위에 stale reward misroute가 끼어들던 `HandleShop -> HandleRewards -> decision-wait-plateau` family는 current `main`에서 즉시 `HandleRewards -> HandleShop` 회복으로 닫혔다
- fresh authoritative endurance root [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)는 valid-trust single attempt가 `player-defeated` natural terminal까지 `stepCount=479`로 진행될 수 있음을 보여줬다
- fresh root [endurance-longrun-20260404-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live21)도 `player-defeated` natural terminal로 끝났고, current `main`이 아직 long run natural terminal에 도달할 수 있음을 다시 확인했다
- `rest-site -> map handoff release-pending`과 `rest-site click-ready -> proceed` family는 live root [rest-site-click-ready-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/rest-site-click-ready-20260403-live1)에서 닫혔고, run은 rest-site 이후 treasure/event continuity까지 진행한 뒤에야 새 event blocker를 드러냈다
- `rest-site-click-ready-20260403-live1`의 새 blocker는 `HandleEvent` `미끄러운 다리`에서 semantic `버틴다`를 label-only same-action처럼 소비하던 false progression family였다
- current `main`에는 `afbee31`과 `c8f6766`가 반영돼 있고, bounded `Slippery Bridge` exit (`OVERCOME`) + event-choice lineage-aware stall fingerprint가 들어가 있다
- exact live artifact root [slippery-bridge-overcome-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live1)는 old semantic `버틴다` loop가 `player-defeated`까지 이어지는 failure shape를 남긴다
- fresh smoke root [slippery-bridge-overcome-20260403-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live2)는 valid-trust attempt `0001`이 `max-steps-reached:260`까지 no-stall로 진행됐음을 보여주지만, 이번 seed는 `미끄러운 다리`를 직접 다시 밟지는 않았다
- `72daabf` 이후 stale end-turn transit reopen family는 current `main`에서 다시 same-snapshot auto-end-turn 재발사 owner로 남지 않는다
- `41d53a0`은 unchanged reopened player-open snapshot에서 EndTurn barrier를 premature release하지 않게 좁혔고, saved live22 exact repro는 current replay-step에서 `auto-end turn`이 아니라 explicit `combat barrier wait barrier=EndTurn`로 바뀌었다
- fresh root [endurance-longrun-20260404-live22](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live22)는 old family가 `same-action-stall`로 드러난 마지막 root이고, decisive step [0393.request.json](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live22/attempts/0001/steps/0393.request.json) 기준 root cause는 `combat end-turn barrier premature release -> same-snapshot auto-end-turn reissue`였다
- fresh root [endurance-longrun-20260404-live24](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live24)는 2026-04-04 earlier pointer에서 마지막 combat blocker를 드러낸 historical root다
- 그 blocker는 이후 `9b50b9c` 와 `68e91fd` 를 거쳐 current `main` fresh live baseline에서는 active blocker가 아니다
- live25, live28, live29는 모두 `player-defeated` natural terminal이고, live28/live29는 fresh 2연속 success evidence다
- live ffmpeg review artifact retention은 2026-04-03 roots에서 success/failure 둘 다 확인됐지만, `window-hwnd gdigrab` review video usable-state는 아직 partial이라 screenshot/request artifact가 source of truth다

## 2026-04-04 pointer update

이 문서의 아래 historical sections에는 2026-04-03 pointer가 많이 남아 있다. 현재 active pointer는 이 섹션과 문서 상단 bullets를 우선한다.

- current `main`의 latest relevant commits:
  - `41d53a0` `Keep end-turn barrier until post-submit progress`
  - `9b50b9c` `Wait for confirmed hand-card plays before end turn`
  - `184a2eb` `Release smith confirm aftermath to rest-site proceed`
  - `68e91fd` `Canonicalize rest-site release-pending after accepted heal`
- current code가 유지하는 closure:
  - ancient option contract
  - `ChooseFirstNode <-> event`
  - post-node combat takeover -> generic map wait
  - explicit relic claim -> proceed inversion
  - rest-site release-pending over map overlay
  - rest-site click-ready/proceed
  - rest-site accepted-heal -> release-pending -> proceed
  - bounded `Slippery Bridge` `OVERCOME` path
- current active blocker는 없다. fresh live long run 2회 연속 success가 이미 확보됐다
- decisive acceptance roots:
  - [endurance-longrun-20260404-live28](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live28)
  - [endurance-longrun-20260404-live29](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260404-live29)
- acceptance reading:
  1. M5 authoritative long-run blocker loop: complete
  2. M6 replay/parity gate: complete on current code baseline
  3. M7 non-combat stability: complete for current substrate acceptance
  4. M8 combat stability: complete for current substrate acceptance
  5. next active milestone: `M9 advice-quality`

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
2. ancient / event / post-node handoff family
   - roots:
     - [boot-to-long-run-20260330-live21](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live21)
     - [ancient-contract-20260331-105147](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/ancient-contract-20260331-105147)
     - [choosefirstnode-handoff-20260331-140432-live01](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/choosefirstnode-handoff-20260331-140432-live01)
   - result:
     - old `live21` ancient option contract mismatch는 current `main`에서 닫혔다
     - `ChooseFirstNode <-> event` mixed-state도 live에서 재현되지 않았다
     - `ChooseFirstNode -> combat takeover -> generic map wait` family도 current `main`에서 닫혔다
3. reward claim contract recovery
   - root: [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1)
   - result:
     - exact replay repro였던 explicit relic claim -> `proceed after resolving rewards` inversion은 current `main`에서 닫혔다
     - fresh live rerun에서도 relic/proceed inversion은 다시 나오지 않았다
     - 대신 attempt `0001`에서 `same-action-stall phase=HandleRewards target=reward card choice screen=rewards`가 새 front blocker로 드러났다
4. combat release + reward aftermath live proof
   - root: [combat-release-reward-aftermath-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-release-reward-aftermath-20260401-live1)
   - result:
     - `ChooseFirstNode -> combat takeover -> generic map wait`는 재발하지 않았다
     - `HandleCombat -> HandleRewards` handoff와 reward precedence는 live에서 정상 동작했다
     - 이 root가 드러낸 `same-action-stall phase=ChooseFirstNode target=exported reachable map node screen=rest-site` family는 이후 2026-04-03 wave에서 current `main` 기준으로 닫혔다
5. rest-site release-pending + click-ready proof
   - roots:
     - [rest-site-click-ready-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/rest-site-click-ready-20260403-live1)
   - result:
     - `rest site: rest -> visible proceed -> treasure/event continuity`가 live에서 다시 확인됐다
     - repeated exported node click / generic map wait는 rest-site aftermath에서 재발하지 않았다
     - 새 front blocker는 rest-site가 아니라 `HandleEvent` `미끄러운 다리` same-action family로 이동했다
6. bounded slippery bridge wave
   - roots:
     - [slippery-bridge-overcome-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live1)
     - [slippery-bridge-overcome-20260403-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live2)
   - result:
     - old failure shape는 semantic `버틴다` loop가 `player-defeated`로 이어지는 bounded event misrouting이다
     - current code는 exact live artifact replay에서 `OVERCOME`와 event-choice lineage를 우선한다
     - fresh live smoke root `live2`는 `max-steps-reached:260`까지 no-stall로 진행됐지만, direct post-fix in-game `미끄러운 다리` repro는 아직 남아 있다
7. combat barrier tail family
   - roots:
     - [combat-takeover-barrier-20260331-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/combat-takeover-barrier-20260331-live1)
     - [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1)
   - result:
     - short validation roots에서는 `combat-barrier-wait-plateau`와 `combat-barrier-step-budget-exhausted` family가 여전히 관찰된다
     - dominant shape는 `HandleCombat`에서 `EnemyClick`/`EndTurn` barrier ownership이 늦게 풀리는 long-tail이다
8. fresh endurance proof root
   - root: [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)
   - result:
     - valid-trust single attempt가 `stepCount=479`까지 실제로 진행됐다
     - 종료는 harness stall이 아니라 natural `player-defeated` terminal이었다
     - 이번 root에서는 short triage roots에서 보였던 reward stall / combat barrier family가 authoritative terminal cause로 올라오지 않았다
9. live review capture state
   - roots:
     - [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1)
     - [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)
   - result:
     - ffmpeg metadata and retained review artifacts are emitted on current `main`
     - but `window-hwnd gdigrab` review video is still black and cannot be treated as gameplay evidence

즉 현재 질문은 더 이상

```text
"ancient option contract나 ChooseFirstNode event mixed-state가 아직 current blocker인가"
```

가 아니다.

historical frontier는

```text
"현재 `main`은 한 번의 authoritative long-run을 natural terminal까지 끌고 갈 수 있다.
하지만 2026-04-03 wave 이후 short validation follow-up은 `reward card progression`, `combat-barrier-step-budget-exhausted`, direct `Slippery Bridge` live repro, live review usable-state로 재정렬됐다.
다음 step은 closed ancient/event/ChooseFirstNode/rest-site family를 reopen하지 않고 이 네 gap을 분리해서 정리하는 것이다."
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
| Non-Combat Stability | partial | ancient/event/ChooseFirstNode family와 rest-site release-pending family는 닫혔고, bounded `Slippery Bridge` progression도 code/self-test + smoke evidence가 있다. 다만 direct post-fix live repro와 some child-screen smoothness evidence는 아직 partial이다 |
| Combat Stability | partial | stale non-enemy/target family는 닫혔지만 short roots에서 `combat-barrier-step-budget-exhausted` / `combat-barrier-wait-plateau` long-tail이 남아 있음 |
| Live-Run Speed Recovery | partial | hot path observer-first baseline은 유지되지만 `deck-remove` / reward card child screen transient capture와 black review video가 남아 있음 |
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
- `7bb6512`, `4244a35`, `bf933cc` 이후 ancient option contract mismatch와 `ChooseFirstNode <-> event` mixed-state는 live에서 다시 authoritative blocker로 올라오지 않았다
- [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)는 current `main`이 valid-trust single attempt를 natural `player-defeated` terminal까지 끌고 갈 수 있음을 보여준다
- exact replay repro였던 explicit relic reward claim -> `proceed after resolving rewards` inversion은 current `main`에서 닫혔다
- current short-root follow-up family는 `reward card choice progression`, `combat-barrier-step-budget-exhausted / wait-plateau`, direct post-fix `Slippery Bridge` live repro, review-video usable-state다
- ffmpeg metadata recording과 review artifact retention은 current `main`에서 붙지만, `window-hwnd gdigrab` review video usable-state는 아직 partial이다
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
- fresh reward validation root:
  - [verify-reward-claim-20260401-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/verify-reward-claim-20260401-live1)
- fresh endurance proof root:
  - [endurance-longrun-20260401-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/endurance-longrun-20260401-live2)
- fresh shop recovery root:
  - [boot-to-long-run-20260330-live4](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live4)
- anti-drift recovery live root:
  - [boot-to-long-run-20260330-live9](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/boot-to-long-run-20260330-live9)
- ancient contract closure root:
  - [ancient-contract-20260331-105147](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/ancient-contract-20260331-105147)
- choose-first-node handoff closure root:
  - [choosefirstnode-handoff-20260331-140432-live01](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/choosefirstnode-handoff-20260331-140432-live01)
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
- current short validation frontier는 단일 blocker 하나로 고정돼 있지 않다
- fresh live root [rest-site-click-ready-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/rest-site-click-ready-20260403-live1)는 rest-site family를 닫은 뒤 `HandleEvent` `미끄러운 다리` same-action family를 드러냈다
- current code는 [slippery-bridge-overcome-20260403-live1](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live1) exact artifact 기준 bounded `OVERCOME` exit로 수정됐고, fresh smoke root [slippery-bridge-overcome-20260403-live2](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/artifacts/gui-smoke/slippery-bridge-overcome-20260403-live2)는 `max-steps-reached:260` no-stall이다
- 다만 post-fix direct in-game `미끄러운 다리` continuation repro는 아직 없고, reward card progression / combat barrier long-tail / review-video usable-state는 계속 partial이다
- `deck-remove` child screen explicit owner/export gap과 some low-priority coverage rows (`event reward substate`, `reward-map loop sentinel`) remain partial

## 다음 작업 원칙

1. 새 semantic fix는 좁게 한다
   - refactor wave는 현재 완료 상태를 reopen하지 않는다
2. 다음 coverage follow-up은 current owner 파일에서만 본다
   - active noncombat smoothness owner:
      - [RuntimeSnapshotReflectionExtractor.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2ModAiCompanion.Mod/Runtime/RuntimeSnapshotReflectionExtractor.cs)
      - [AutoDecisionProvider.NonCombatSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatSupport.cs)
      - [AutoDecisionProvider.NonCombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.NonCombatDecisions.cs)
      - [Program.Runner.AttemptDecisionFlow.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.Runner.AttemptDecisionFlow.cs)
      - [Program.AllowedActions.NonCombat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.NonCombat.cs)
   - retained combat owner baseline:
     - [Analysis/CombatMicroStageSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatMicroStageSupport.cs)
     - [Analysis/CombatPostActionObservationSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatPostActionObservationSupport.cs)
     - [Program.AllowedActions.Combat.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.AllowedActions.Combat.cs)
     - [AutoDecisionProvider.CombatDecisions.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AutoDecisionProvider.CombatDecisions.cs)
      - [Analysis/CombatBarrierSupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatBarrierSupport.cs)
      - [Analysis/CombatTargetabilitySupport.cs](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Analysis/CombatTargetabilitySupport.cs)
3. semantic blocker, speed evidence, coverage gap을 구분한다
   - ancient/event/ChooseFirstNode family는 닫혔고, current `main`은 natural-terminal long-run sample 하나를 확보했다
   - short-root reward/combat/video gap과 `Slippery Bridge` direct live repro gap은 별도 family로 본다
   - explicit event/common combat speed baseline과 post-refactor cleanup program은 유지한다
   - rest-site release-pending family는 current `main`에서 닫혔고, immediate follow-up은 reward card progression, combat barrier ownership release, `Slippery Bridge` direct live repro, review-video capture usability다

## 한 줄 요약

```text
current `main`은 ancient option contract, `ChooseFirstNode <-> event`, post-node combat takeover -> generic map wait, explicit relic claim -> proceed inversion을 닫은 뒤,
fresh authoritative endurance root에서 `stepCount=479` natural `player-defeated` terminal까지 도달했다.
이후 2026-04-03 wave는 `rest-site -> map handoff`와 rest-site click-ready/proceed family를 live에서 닫았고, bounded `Slippery Bridge` routing과 event-lineage stall semantics를 current `main`에 올렸다.
즉 M5 acceptance shape evidence는 late-stage까지 왔지만, short validation follow-up에는 reward card progression, combat barrier long-tail, direct `Slippery Bridge` live repro, review-video usable-state가 아직 남아 있다.
```
