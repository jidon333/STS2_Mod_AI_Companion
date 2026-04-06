# Advisor UI Coverage Matrix

## 목적

이 문서는 `M9 advisor scene substrate` v1의 coverage 기준이다.
목표는 모든 UI 정보를 긁는 것이 아니라, 장면별 조언에 필요한 `decision-relevant scene-local facts`를 안정적으로 구조화하는 것이다.

## 현재 authoritative roots

- fresh live root:
  - `artifacts/gui-smoke/endurance-longrun-20260404-live28`
  - `artifacts/gui-smoke/endurance-longrun-20260404-live29`
- provisional shop root:
  - `artifacts/gui-smoke/verify-shop-slot-source-20260322-0131`
- live sidecar validation roots:
  - fake live export `scene-model-reward-run`
  - fake live export `scene-model-event-run`
  - fake live export `scene-model-shop-run`
  - fake live export `scene-model-map-pending-run`

## Coverage Matrix

| scene | decision question | observable now | missing | source seam | evidence root | needs observer/exporter? |
| --- | --- | --- | --- | --- | --- | --- |
| combat | 지금 턴에서 어떤 플레이 맥락인가 | HP, energy, hand summary, targetability, lifecycle stage, display 가능한 pile/hand detail | enemy intent summary, incoming damage summary | `observer.state.player/meta` + `CombatReleaseState` + `CombatRuntimeState` | `live28` step `0017` | not for v1 summary, yes for higher-quality advice |
| reward | 지금 어떤 보상이 남아 있는가 | normalized reward entries, reward child state, proceed visibility | durable remaining-vs-claimed normalization | `observer.state.currentChoices` + `RewardSceneState` | `live28` step `0052` | not immediately |
| event | 지금 이벤트가 어느 단계이며 선택지는 무엇인가 | option labels/descriptions, ancient phase, proceed visibility, reward substate | canonical event identity/title/page id | `observer.state.currentChoices/meta` + `EventSceneState` | `live29` step `0027` | maybe, if identity becomes decision blocker |
| rest-site | 지금 휴식 vs 재련 판단에 필요한 상태는 무엇인가 | HP, explicit choice visibility, smith/proceed stage | smith candidate delta summary | `observer.state.player/currentChoices` + `RestSiteSceneState` | `live28` step `0101` | maybe, only if smith detail is needed |
| shop | 지금 살 수 있는 것과 제거 가능 여부는 무엇인가 | gold, typed shop options, affordability, removal availability | exact price, richer item effect summary | `observer.state.player/currentChoices/meta` + `ShopSceneState` | provisional `verify-shop-slot-source-20260322-0131` step `0005` | yes for exact price/effect quality |
| map | 지금 갈 수 있는 노드는 무엇인가 | reachable node label/type/coord, overlay stage | route depth, branch context, path forecast | `observer.state.currentChoices/meta` + `MapOverlayState` | `live29` step `0033` | yes for route-level advice |

## V1 Acceptance

- scene summary는 `request goal`, `allowedActions`, `fallback reason`, `targetLabel` 같은 actuator vocabulary를 포함하지 않는다.
- scene truth는 `observer.state + canonical scene state`에서만 만든다.
- `request`는 `runId`, `attemptId`, `stepIndex`, `phase`, `history`, `windowBounds`, `screenshotPath` 같은 replay envelope 용도로만 쓴다.
- `reward/event/shop` compact advice는 model-backed MVP로 유지하고, `combat`는 preview-only / no-call로 유지한다.
- shop coverage는 fresh live refresh 전까지 provisional로 취급한다.

## Compact Advisor Managed Scene Gaps

| gap | category | current owner | current status | notes |
| --- | --- | --- | --- | --- |
| reward direct fact ownership seam | advisor-input | Foundation reward fact builders | pending | direct fact seam은 개선됐지만 opaque reward card info와 fact ownership은 아직 open gap |
| reward/event card-like description gap | display | Runtime extractor + display seam | pending | evaluated hover tip / node display follow-up이 남아 있음 |
| event option duplicate/identity instability | canonicalization | Harness + Foundation | pending | generic seam이 primary지만 canonical event identity/title/page id와 일부 strict/compat residue가 남아 있음 |
| compact missingFacts affecting label match | advisor-input | Foundation compact builders + finalizer | pending | exact-label / no-call degraded safety는 유지하고, gaps는 explicit missing으로 남겨야 함 |
| shop exact price/effect gap | observer/export + display | Runtime extractor + display seam | pending | shop compact advisor MVP는 지원되지만 authoritative live root는 아직 provisional이고 exact price/effect quality는 미완 |
| combat preview-only / no-call boundary | contract | Foundation shared policy | by_design | combat는 compact-managed preview facts surface만 제공하고 model call은 하지 않음 |

- reward child-choice helper-row duplicate canonicalization issue는 committed path에서 정리됐다.
- 현재 reward open gap은 broad duplicate cleanup이 아니라 reward fact ownership / direct fact seam 쪽이다.

## Compact Advisor Notes

- `RewardEventCompactAdvisorInput`는 `SceneModel` 대체물이 아니라 compact advisor용 view다.
- compact input은 `AdviceInputPack` 앞단의 adapter-first layer다.
- compact builder는 `CompanionRunState + normalized state + bounded slice`를 입력으로 받는다.
- 새 retrieval API는 만들지 않고 existing bounded `BuildSlice(...)`를 compact builder가 post-filter 한다.
- current compact-managed scene scope는 `reward/event/shop/combat`다.
- model-call allowed scene은 `reward/event/shop`이고, `combat`는 preview-only / no-call이다.
- unsupported / insufficient / invalid exact-label mismatch는 shared degraded helper로 정리한다.
- Host는 gating/manual orchestration만 담당하고, Foundation이 compact contract/builder/prompt/finalizer/fallback을 소유한다.

## Live Side Seams

- combat:
  - `snapshot.meta.foregroundOwner`
  - `snapshot.meta.foregroundActionLane`
  - `snapshot.meta.combatHandSummary`
  - `snapshot.meta.combatTargetCount`
  - `snapshot.meta.combatTargetableEnemyCount`
- reward:
  - `snapshot.meta.foregroundOwner`
  - `snapshot.meta.foregroundActionLane`
  - `snapshot.meta.rewardProceedVisible`
  - `snapshot.currentChoices`
- event:
  - `snapshot.meta.foregroundOwner`
  - `snapshot.meta.foregroundActionLane`
  - `snapshot.meta.ancientPhase`
  - `snapshot.currentChoices`
- rest-site:
  - `snapshot.meta.restSiteButtonsVisible`
  - `snapshot.meta.restSiteButtonsClickReady`
  - `snapshot.meta.restSiteProceedVisible`
  - `snapshot.meta.restSiteUpgradeScreenVisible`
  - `snapshot.meta.restSiteUpgradeConfirmVisible`
- shop:
  - `snapshot.meta.shopInventoryOpen`
  - `snapshot.meta.shopAffordableOptionCount`
  - `snapshot.meta.shopCardRemovalVisible`
  - `snapshot.player.gold`
  - `snapshot.currentChoices`
- map:
  - `snapshot.meta.foregroundOwner`
  - `snapshot.meta.mapReleaseAuthority`
  - `snapshot.meta.mapSurfacePending`
  - `snapshot.meta.mapCurrentNodeArrowVisible`
  - `snapshot.meta.mapPointCount`
  - `snapshot.currentChoices`

## Live Sidecar Notes

- live artifact root는 `artifacts/companion/<runId>/advisor-scene/`다.
- latest file은 `advisor-scene.latest.json`, change log는 `advisor-scene.ndjson`다.
- current live gaps:
  - combat: `combat-enemy-intent-summary-missing`
  - shop: `shop-item-price-missing`, `shop-item-effect-summary-missing`
  - map: `map-route-context-missing`, `map-current-node-identity-missing`
  - event: non-ancient canonical identity는 여전히 partial이다

## WPF Display Notes

- live sidecar는 scene-aware display formatter를 써서 `sceneType / sceneStage / canonicalOwner`와 `현재 장면 맥락`을 분리해 보여 준다.
- `SceneSummaryText`는 raw summary를 그대로 복사하지 않고, display-only knowledge/localization을 거친 결과를 보여 준다.
- `보이는 선택지`는 기본 플레이에 직접 필요하지 않은 utility/diagnostic choice를 숨긴다.
- advanced 영역은 기본 접힘으로 두고, `confidence / source refs`, `최근 이벤트`, `관련 지식`, `현재 장면 진단`, `AI 입력 요약`, `AI 입력`, `프롬프트 미리보기`, `수집 런 진단`을 그 안에 둔다.

## Follow-up Work Item

- fresh live에서 `shop` scene을 다시 수집해 acceptance-grade root를 만든다.
- 새 root가 생기면 `replay-advisor-scene`으로 schema를 다시 생성해 provisional 표기를 제거한다.
