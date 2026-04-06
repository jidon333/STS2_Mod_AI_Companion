# Advisor Input Output Contract

## 상태

이 문서는 `M9 strategy-principles + 3-view advice` 기준 계약 문서다.
현재 목표는 compact-managed scene 경로를 current committed contract와 맞추면서, `background-only strategy principles`와 `최종 / 보수적 / 공격적` additive advice view를 기존 `AdviceInputPack` / `AdviceResponse` 위에 안전하게 연결하는 것이다.

## Current Rule

- `SceneModel`과 `RewardEventCompactAdvisorInput`는 분리한다.
- `AdviceInputPack`은 유지한다. compact payload는 nullable additive field로만 추가한다.
- `Host`는 gating / manual orchestration / adapter wiring owner다.
- `Foundation`은 compact contract, pure builders, prompt path, finalizer, degraded fallback owner다.
- `scene model contract + summary`는 계속 `src/Shared/AdvisorSceneModel/`에 유지한다.
- `live sidecar`는 `SceneModel`을 WPF에 직접 표시하지만, advisor input packing에는 compact view만 연결한다.
- compact input의 truth source는 `SceneModel`이 아니라 `CompanionRunState + normalized state + bounded slice`다.
- current compact-managed scene scope는 `reward/event/shop/combat`다.
- model call을 허용하는 scene은 `reward/event/shop`이고, `combat`는 preview-only / no-call로 유지한다.
- `strategy principles`는 compact input의 일부가 아니다. `AdviceInputPack.StrategyPrinciples` separate additive field로만 전달한다.
- `strategy principles`는 background lens일 뿐이며, `visible options`, explicit scene facts, `MissingInformation`, `DecisionBlockers`를 override하지 않는다.
- `StrategyPrinciplesService`는 broad catalog merge 없이 별도 artifact loader로 유지한다.
- strategy retrieval은 scene-local strict mapping만 사용한다. substring / broad semantic search는 허용하지 않는다.
- 새 retrieval API는 만들지 않는다. compact builder는 existing bounded `BuildSlice(...)`를 post-filter한다.
- unsupported / degraded response는 shared factory에서 생성한다.

## Compact Mapping Direction

compact builder는 `RewardEventCompactAdvisorInput`을 아래 값으로 채운다.

- `run context`
  - act / floor
  - hp / max hp
  - gold
  - potion count
  - relic count
  - deck count
- `scene identity`
  - `sceneType`
  - `sceneStage`
  - `canonicalOwner`
- `visible options`
  - label
  - enabled / disabled
  - typed kind
  - value
  - display description
- `player summary`
  - compact deck aggregate summary
  - key relic summary
  - key potion summary
- `scene-specific facts`
  - reward / event / shop deterministic compact facts
  - combat preview facts
- `missing information`
  - model이 추정하면 안 되는 빈칸
- `knowledge slice`
  - existing bounded `BuildSlice(...)`의 post-filter 결과

별도 background lens:

- `strategy principles`
  - `AdviceInputPack.StrategyPrinciples`
  - title / summary / transfer confidence만 유지한 compact prompt shape
  - 최대 3개
  - 없으면 empty여도 정상

추가 규칙:

- 새 retrieval API는 만들지 않는다.
- compact builder는 `BuildSlice(...)`로 얻은 bounded slice를 scene-local option/identity 기준으로 다시 줄인다.
- scene identity와 visible option truth는 `normalized state`를 기준으로 잡고, `SceneModel`은 sidecar/display용 fact surface로만 유지한다.
- current compact-managed scene scope는 `reward/event/shop/combat`다.
- model call을 허용하는 scene은 `reward/event/shop`이고, `combat`는 preview-only / no-call이다.
- `strategy principles`는 strict scene-local mapping으로만 선택한다.
- `strategy principles`는 prompt에서 facts 뒤에 위치하는 secondary lens이며, compact fact를 대신하지 않는다.
- reward assessment facts는 explicit visible scene fact seam을 우선하지만, reward fact ownership / direct fact seam은 아직 open gap이다.

## Output Compatibility

현재 output schema는 existing `AdviceResponse`를 유지한다.

additive response view:

- `FinalView`
- `ConservativeView`
- `AggressiveView`

필수 규칙:

- `recommendedChoiceLabel`은 현재 scene option label과 exact match여야 한다.
- exact match가 아니면 recommendation은 invalid 처리하고 degraded fallback을 반환한다.
- 정보가 부족하면 `summary`를 흐리지 말고 `MissingInformation`과 `DecisionBlockers`를 채워야 한다.
- top-level canonical 조언은 항상 `FinalView`와 정렬된다.
- `FinalView`가 없으면 finalizer가 top-level canonical 필드로 `FinalView`를 합성한다.
- auxiliary view(`ConservativeView`, `AggressiveView`)는 null일 수 있다.
- auxiliary view label이 visible option과 exact match하지 않으면 그 view label만 null로 내리고, valid final response는 그대로 유지한다.
- `FinalView` label mismatch만 canonical degraded를 유발한다.
- combat preview는 strategy principles가 있어도 preview-only / no-call 정책을 유지한다.

## Foundation Merge Gate

다음 조건을 만족하기 전에는 foundation merge를 금지한다.

- 최소 `2개 scene`에서 schema stability 확인
- 최소 `3개 fixture root`에서 repeated replay 안정성 확인
- 동일 fixture를 반복 replay해도 field churn이 없어야 함
- scene summary에 actuator vocabulary가 섞이지 않아야 함
- reward/event/shop compact path와 combat preview-only / no-call path, shared degraded helper가 live/replay 둘 다에서 동일하게 동작해야 함

## Shared Fallback

- `CompactAdvisorFallbackFactory`는 Foundation 공용 helper다.
- unsupported scene, insufficient compact input, invalid choice label mismatch는 모두 같은 degraded response shape를 사용한다.
- live Codex path와 replay validator path는 같은 finalizer와 같은 fallback factory를 지난다.

## Shop Status Note

- `shop`은 current compact-managed / model-call scene이다.
- 다만 fresh live refresh 전까지는 acceptance-grade live schema anchor로 쓰지 않고 provisional root로 유지한다.
- exact price / richer effect summary gap은 아직 open이다.
