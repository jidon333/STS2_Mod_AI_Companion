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

## Coverage Matrix

| scene | decision question | observable now | missing | source seam | evidence root | needs observer/exporter? |
| --- | --- | --- | --- | --- | --- | --- |
| combat | 지금 턴에서 어떤 플레이 맥락인가 | HP, energy, hand summary, targetability, lifecycle stage | enemy intent summary, incoming damage summary | `observer.state.player/meta` + `CombatReleaseState` + `CombatRuntimeState` | `live28` step `0017` | not for v1 summary, yes for higher-quality advice |
| reward | 지금 어떤 보상이 남아 있는가 | normalized reward entries, reward child state, proceed visibility | durable remaining-vs-claimed normalization | `observer.state.currentChoices` + `RewardSceneState` | `live28` step `0052` | not immediately |
| event | 지금 이벤트가 어느 단계이며 선택지는 무엇인가 | option labels/descriptions, ancient phase, proceed visibility, reward substate | canonical event identity/title/page id | `observer.state.currentChoices/meta` + `EventSceneState` | `live29` step `0027` | maybe, if identity becomes decision blocker |
| rest-site | 지금 휴식 vs 재련 판단에 필요한 상태는 무엇인가 | HP, explicit choice visibility, smith/proceed stage | smith candidate delta summary | `observer.state.player/currentChoices` + `RestSiteSceneState` | `live28` step `0101` | maybe, only if smith detail is needed |
| shop | 지금 살 수 있는 것과 제거 가능 여부는 무엇인가 | gold, typed shop options, affordability, removal availability | exact price, richer item effect summary | `observer.state.player/currentChoices/meta` + `ShopSceneState` | provisional `verify-shop-slot-source-20260322-0131` step `0005` | yes for exact price/effect quality |
| map | 지금 갈 수 있는 노드는 무엇인가 | reachable node label/type/coord, overlay stage | route depth, branch context, path forecast | `observer.state.currentChoices/meta` + `MapOverlayState` | `live29` step `0033` | yes for route-level advice |

## V1 Acceptance

- scene summary는 `request goal`, `allowedActions`, `fallback reason`, `targetLabel` 같은 actuator vocabulary를 포함하지 않는다.
- scene truth는 `observer.state + canonical scene state`에서만 만든다.
- `request`는 `runId`, `attemptId`, `stepIndex`, `phase`, `history`, `windowBounds`, `screenshotPath` 같은 replay envelope 용도로만 쓴다.
- shop coverage는 fresh live refresh 전까지 provisional로 취급한다.

## Follow-up Work Item

- fresh live에서 `shop` scene을 다시 수집해 acceptance-grade root를 만든다.
- 새 root가 생기면 `replay-advisor-scene`으로 schema를 다시 생성해 provisional 표기를 제거한다.
