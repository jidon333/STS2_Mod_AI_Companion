# Advisor Input Output Contract

## 상태

이 문서는 `M9 wave 1` 기준 `draft only`다.
현재 구현 범위는 `SceneModel + human-readable summary`까지이며, advisor input packing은 다음 wave에서 다룬다.

## Current Rule

- `SceneModel`과 `AdvisorInputV1`는 분리한다.
- wave 1에서는 `AdviceInputPack` 또는 foundation advisor contract를 변경하지 않는다.
- scene model builder는 harness-local에 유지한다.

## Planned Mapping Direction

다음 wave에서만 아래 방향으로 매핑한다.

- `SceneModel.summaryText`
- `SceneModel.sceneType`
- `SceneModel.sceneStage`
- `SceneModel.options`
- `SceneModel.missingFacts`
- `SceneModel.playerContext`
- 필요한 scene-specific typed details 일부

이 값들을 `AdvisorInputV1` 또는 existing `AdviceInputPack`의 scene-local normalized input으로 투입한다.

## Output Compatibility

현재 output schema는 existing `AdviceResponse`를 유지한다.

필수 규칙:

- on-screen choice 추천이 있을 때 `recommendedChoiceLabel`은 `SceneModel.options[*].label`과 정확히 일치해야 한다.
- 정보가 부족하면 `summary`를 흐리지 말고 `missingInformation`, `decisionBlockers`를 채워야 한다.

## Foundation Merge Gate

다음 조건을 만족하기 전에는 foundation merge를 금지한다.

- 최소 `2개 scene`에서 schema stability 확인
- 최소 `3개 fixture root`에서 repeated replay 안정성 확인
- 동일 fixture를 반복 replay해도 field churn이 없어야 함
- scene summary에 actuator vocabulary가 섞이지 않아야 함

## Shop Note

- `shop`은 현재 provisional root 기반이다.
- fresh live refresh 전까지는 acceptance-grade schema anchor로 쓰지 않는다.
