# Advisor Scene Information Model

## 목적

`M9` v1의 목적은 prompt를 만드는 것이 아니라, scene별 의사결정 핵심 정보를 사람이 읽기 좋은 형태로 고정하는 것이다.

이 문서의 범위는:

- raw fact와 human-readable scene model의 경계
- 장면별 decision-relevant field
- `request is not truth source` 원칙

이 문서의 비범위는:

- 추천 생성 로직
- prompt packing
- actuator contract 변경

## 3-Layer Boundary

### 1. Raw facts

원천:

- `observer.state.json`
- screenshot-derived analyzer outputs
- runtime observer `meta`

특징:

- noisy하고 transport-oriented
- 장면별 의미가 아직 정리되지 않았을 수 있다

### 2. Canonical harness scene state

원천:

- `GuiSmokeStepAnalysisContext`
- `RewardSceneState`
- `EventSceneState`
- `ShopSceneState`
- `RestSiteSceneState`
- `CombatReleaseState`
- `CombatRuntimeState`
- `MapOverlayState`

특징:

- harness의 foreground owner / stage / release truth를 이미 정리한다
- actuator logic와 같은 seam을 공유하지만 advisor 출력 자체는 아니다

### 3. Human-readable advisor scene model

원천:

- layer 1 + layer 2를 scene-local decision facts로 재구성

특징:

- 사람이 읽는 상태 표현
- 추천을 포함하지 않음
- actuator vocabulary를 포함하지 않음
- 누락 정보는 `missingFacts`, `observerGaps`로 명시

## Truth Source Rule

- replay/harness truth source는 `observer.state + shared ScreenProvenanceResolver resolved primary provenance + canonical harness scene state`다.
- live side truth source는 `live snapshot + shared ScreenProvenanceResolver resolved primary provenance + normalized scene state + current choices`다.
- `compatibilityCurrentScreen / compatibilityLogicalScreen / compatibilityVisibleScreen / compatibilitySceneReady / compatibilitySceneAuthority / compatibilitySceneStability`는 parity/diagnostics surface일 뿐, primary truth winner가 아니다.
- `recent events`는 현재 live builder의 truth source가 아니라 run-state context/diagnostics 용도로만 유지한다.
- `GuiSmokeStepRequest`는 truth source가 아니다.
- `request`에서 허용되는 사용:
  - `runId`
  - `attemptId`
  - `stepIndex`
  - `phase`
  - `history`
  - `windowBounds`
  - `screenshotPath`
  - `combatCardKnowledge`
- `request.Observer.goal/allowedActions/failureModeHint/reasoningMode` 같은 prompt/actuator 필드는 scene truth 계산에 쓰지 않는다.
- WPF sidecar는 같은 scene model을 scene-aware display formatter로 다시 읽을 수 있지만, display-only knowledge/localization은 label/description 렌더링에만 쓰고 truth source로 쓰지 않는다.

## Common SceneModel Envelope

모든 scene model은 아래 공통 필드를 가진다.

- `schemaVersion`
- `sourceKind`
- `runId`
- `attemptId`
- `stepIndex`
- `phase`
- `sceneType`
- `sceneStage`
- `canonicalOwner`
- `summaryText`
- `playerContext`
- `uiSurfaceInventory`
- `options`
- `missingFacts`
- `observerGaps`
- `confidence`
- `sourceRefs`

구현 위치:

- 공통 contract + summary: `src/Shared/AdvisorSceneModel/`
- display-only sanitizer / knowledge resolver: `src/Shared/AdvisorSceneDisplay/`
- replay builder: `src/Sts2GuiSmokeHarness/AdvisorSubstrate/GuiSmokeAdvisorSceneModelBuilder.cs`
- live builder: `src/Sts2AiCompanion.Host/AdvisorScene/`
- WPF scene-aware formatter: `src/Sts2AiCompanion.Wpf/Display/`

nullable rule:

- replay: `attemptId`, `stepIndex`, `phase`, `requestPath`, `screenshotPath`가 채워진다
- live: 위 필드들은 null일 수 있고, fake replay envelope를 만들지 않는다

## Scene-Specific Fields

### combat

- `lifecycleStage`
- `encounterKind`
- `turn`
- `handCount`
- `handSummary`
- `targetableEnemyCount`
- `hittableEnemyCount`
- `targetingInProgress`
- `enemyIntentSummary?`

### reward

- `explicitAction`
- `releaseStage`
- `claimableRewardPresent`
- `explicitProceedVisible`
- `rewardChoiceVisible`
- `colorlessChoiceVisible`
- `rewardEntryCount`
- `cardProgressionSurfacePresent`
- `options`는 duplicate raw exports를 그대로 담지 않고, unique reward entries + visible control choices로 정규화한다.

### event

- `explicitAction`
- `releaseStage`
- `rewardSubstateActive`
- `explicitProceedVisible`
- `hasExplicitProgression`
- `ancientContractLane`
- `eventIdentity?`
- `optionCount`

### rest-site

- `releaseStage`
- `explicitChoiceVisible`
- `explicitChoiceReady`
- `smithUpgradeActive`
- `smithConfirmVisible`
- `proceedVisible`
- `selectionSettling`
- `actionCount`
- `upgradeCandidateSurfaceVisible`

### shop

- `inventoryOpen`
- `proceedEnabled`
- `backEnabled`
- `cardRemovalVisible`
- `cardRemovalEnabled`
- `cardRemovalUsed`
- `optionCount`
- `affordableOptionCount`
- `serviceCount`
- `itemCount`

### map

- `foregroundVisible`
- `currentNodeArrowVisible`
- `reachableNodePresent`
- `reachableNodeCount`
- `currentNode?`
- `reachableNodeLabels`

## V1 Rules

- v1 목표는 `SceneModel + summary`까지다.
- `AdvisorInputV1`는 다음 wave 전까지 구현하지 않는다.
- summary는 사실 서술만 한다.
- summary는 `click`, `wait`, `fallback`, `candidate`, `targetLabel` 같은 actuator vocabulary를 포함하지 않는다.
- hard gap은 추정하지 않고 `missingFacts`로 남긴다.
- reward option list는 raw observer dump를 그대로 넘기지 않고 scene-level normalization을 거친다.
- live sidecar의 core/advanced 패널은 이 scene model을 사람이 읽기 쉽게 재배치하는 display layer다. raw summary를 그대로 복사하지 않고, scene-aware formatter와 display-only knowledge를 거쳐 보여 줄 수 있다.
