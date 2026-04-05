# Advisor Input Output Contract

## 상태

이 문서는 `M9 wave 2` 기준 계약 문서다.
이번 wave의 목표는 `reward/event` 장면에서 `scene-local compact input`을 만든 뒤, 기존 `AdviceInputPack`에 adapter-first로 연결하는 것이다.

## Current Rule

- `SceneModel`과 `RewardEventCompactAdvisorInput`는 분리한다.
- `AdviceInputPack`은 유지한다. compact payload는 nullable additive field로만 추가한다.
- `Host`는 gating / manual orchestration / adapter wiring owner다.
- `Foundation`은 compact contract, pure builders, prompt path, finalizer, degraded fallback owner다.
- `scene model contract + summary`는 계속 `src/Shared/AdvisorSceneModel/`에 유지한다.
- `live sidecar`는 `SceneModel`을 WPF에 직접 표시하지만, advisor input packing에는 compact view만 연결한다.
- compact input의 truth source는 `SceneModel`이 아니라 `CompanionRunState + normalized state + bounded slice`다.
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
- `reward/event facts`
  - deterministic compact facts only
- `missing information`
  - model이 추정하면 안 되는 빈칸
- `knowledge slice`
  - existing bounded `BuildSlice(...)`의 post-filter 결과

추가 규칙:

- 새 retrieval API는 만들지 않는다.
- compact builder는 `BuildSlice(...)`로 얻은 bounded slice를 reward/event option/identity 기준으로 다시 줄인다.
- scene identity와 visible option truth는 `normalized state`를 기준으로 잡고, `SceneModel`은 sidecar/display용 fact surface로만 유지한다.
- reward/event 외 장면은 compact advisor 대상이 아니다.

## Output Compatibility

현재 output schema는 existing `AdviceResponse`를 유지한다.

필수 규칙:

- `recommendedChoiceLabel`은 현재 scene option label과 exact match여야 한다.
- exact match가 아니면 recommendation은 invalid 처리하고 degraded fallback을 반환한다.
- 정보가 부족하면 `summary`를 흐리지 말고 `MissingInformation`과 `DecisionBlockers`를 채워야 한다.

## Foundation Merge Gate

다음 조건을 만족하기 전에는 foundation merge를 금지한다.

- 최소 `2개 scene`에서 schema stability 확인
- 최소 `3개 fixture root`에서 repeated replay 안정성 확인
- 동일 fixture를 반복 replay해도 field churn이 없어야 함
- scene summary에 actuator vocabulary가 섞이지 않아야 함
- reward/event compact path와 shared degraded helper가 live/replay 둘 다에서 동일하게 동작해야 함

## Shared Fallback

- `CompactAdvisorFallbackFactory`는 Foundation 공용 helper다.
- unsupported scene, insufficient compact input, invalid choice label mismatch는 모두 같은 degraded response shape를 사용한다.
- live Codex path와 replay validator path는 같은 finalizer와 같은 fallback factory를 지난다.

## Shop Note

- `shop`은 이번 wave 비범위다.
- fresh live refresh 전까지는 acceptance-grade schema anchor로 쓰지 않는다.
