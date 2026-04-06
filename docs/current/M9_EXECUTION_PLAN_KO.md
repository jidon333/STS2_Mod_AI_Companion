# M9 실행 계획 (KO)

> 상태: 현재 사용 중
> 기준 브랜치: `main`
> 최종 갱신: 2026-04-06
> 목적: `M9 advice-quality`의 세부 계획, 현재 진척도, 다음 work unit, acceptance gate를 한 문서에서 추적

## 한 줄 요약

`M9`의 첫 목표는 AI를 곧바로 더 똑똑하게 만드는 것이 아니라, 현재 하네스가 읽는 게임 상태를 `장면별 human-readable scene model`로 승격시키는 것이다.

읽기 쉬운 안내 문서는 [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/readers/ADVISOR_SCENE_MODEL_READER_KO.md) 를 우선한다.
구조 자체가 헷갈리면 [HARNESS_TO_M9_STRUCTURE_READER_KO.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/readers/HARNESS_TO_M9_STRUCTURE_READER_KO.md) 를 먼저 읽는다.

## 현재 위치

- substrate baseline:
  - fresh live long run 2회 연속 natural terminal 확보
  - practical pointer는 `M5~M8 substrate acceptance complete`, `M9 entry`
- M9 기본 원칙:
  - observer-first
  - read-only
  - artifact-first
  - harness-local proving schema
  - raw fact를 직접 AI 입력으로 쓰지 않음
- 현재 additive 진행:
  - `strategy-principles` background artifact 생성 완료
  - `StrategyPrinciplesService` strict retrieval + `최종 / 보수적 / 공격적` 3-view advice 경로 구현
  - compact-managed scene은 `reward/event/shop`, `combat preview-only / no-call` 경로가 committed contract로 정리됨
  - reward child-choice helper-row duplicate residue는 정리됐지만, reward fact ownership / direct fact seam은 아직 open gap으로 남아 있음
  - live acceptance와 style tuning은 계속 direct-play에서 닫는 중

## M9 범위

이번 milestone에서 먼저 만드는 것은 아래 다섯 가지다.

1. scene UI inventory
2. canonical advisor scene model
3. human-readable scene summary
4. coverage gap inventory
5. read-only advisor input 준비

이번 milestone에서 아직 하지 않는 것은 아래다.

1. actuator contract 확장
2. gameplay auto-decision 교체
3. foundation canonical merge
4. polished real-time overlay UI
5. raw observer dump direct-to-LLM

별도 sidecar 실시간 표시 workstream은 [M9_LIVE_SIDECAR_UI_PLAN_KO.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md) 로 분리한다.

## 장면 우선순위

v1 우선순위는 다음으로 고정한다.

1. reward
2. event
3. rest-site
4. shop
5. map
6. combat

설명:

- reward/event/rest-site/shop/map은 advisor 가치 대비 schema 안정화 비용이 낮다.
- combat는 scene summary부터 시작하되, 고급 조언 depth는 v1 후반 또는 다음 wave로 미룬다.

## Workstream

### WS1. Scene Inventory

목표:

- 장면별로 사람이 실제 판단에 쓰는 UI 정보를 문서화한다.
- `observable now / missing / source seam / evidence root`를 고정한다.

주요 산출물:

- [ADVISOR_UI_COVERAGE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md)

현재 상태:

- `started`

완료 기준:

- reward/event/rest-site/shop/map/combat 6개 scene row가 존재
- 각 row에 `decision question`, `observable now`, `missing`, `source seam`, `evidence root`, `needs observer/exporter?`가 채워짐
- provisional root와 acceptance-grade root가 구분됨

### WS2. Scene Model Contract

목표:

- `raw facts -> canonical harness scene state -> human-readable advisor scene model` 3층 경계를 고정한다.
- scene truth는 `observer.state + canonical scene state`에서만 만든다.

주요 산출물:

- [ADVISOR_SCENE_INFORMATION_MODEL.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/ADVISOR_SCENE_INFORMATION_MODEL.md)
- `src/Shared/AdvisorSceneModel/AdvisorSceneContracts.cs`

현재 상태:

- `in_progress`
- `A2 shared provenance parity` 자동 검증 green, direct play clean-boot manual sweep만 남음

완료 기준:

- common envelope가 고정됨
- reward/event/rest-site/shop/map/combat typed details가 존재
- `request is not truth source` 규칙이 문서/코드에 같이 반영됨
- actuator vocabulary가 scene model에 섞이지 않음

### WS3. Harness-Local Builder And Summary

목표:

- replay artifact에서 advisor scene model을 생성한다.
- 사람이 읽는 summary를 typed model에서 렌더링한다.

주요 산출물:

- [GuiSmokeAdvisorSceneModelBuilder.cs](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/AdvisorSubstrate/GuiSmokeAdvisorSceneModelBuilder.cs)
- `src/Shared/AdvisorSceneModel/AdvisorSceneSummaryFormatter.cs`
- [Program.InspectAndReplay.cs](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/src/Sts2GuiSmokeHarness/Program.InspectAndReplay.cs)

현재 상태:

- `started`

완료 기준:

- `replay-advisor-scene`로 `.request.json + .observer.state.json`에서 advisor scene artifact 생성 가능
- summary text가 typed model 기반으로 만들어짐
- fixture replay를 반복해도 field churn이 크지 않음

### WS4. Advisor Input Contract

목표:

- scene model과 compact-managed advisor input을 분리한다.
- `AdviceInputPack`은 유지하되 compact payload를 additive adapter로 주입한다.
- `Host`는 gating / manual orchestration / adapter wiring만 맡고, `Foundation`이 compact contract, pure builder, prompt path, finalizer, degraded fallback owner가 된다.
- compact builder는 새 retrieval API 없이 기존 `KnowledgeCatalogService.BuildSlice(...)` 결과를 post-filter한다.

주요 산출물:

- [ADVISOR_INPUT_OUTPUT_CONTRACT.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/contracts/ADVISOR_INPUT_OUTPUT_CONTRACT.md)

현재 상태:

- `in_progress`

완료 기준:

- `RewardEventCompactAdvisorInput`의 owner가 Foundation.Contracts로 고정됨
- `AdviceInputPack`은 유지되며 compact payload adapter가 명시됨
- `KnowledgeCatalogService.BuildSlice(...)` 후 compact post-filter 방식이 고정됨
- `recommendedChoiceLabel` exact-label validation과 shared degraded fallback이 문서화됨
- compact-managed scene scope는 `reward/event/shop/combat`, model-call allowed scene은 `reward/event/shop`, `combat`는 preview-only / no-call로 명시됨
- compact input은 `SceneModel` truth source가 아니라 `CompanionRunState + normalized state + bounded slice` 기반의 판단 재료로 문서화됨

### WS5. Coverage Gaps And Export Follow-Up

목표:

- compact-managed scene의 current gap만 고정한다.
- scene model에서 비는 정보를 `missingFacts`와 `observerGaps`로 관리한다.
- exporter 추가는 hard blocker일 때만 제안한다.

현재 상태:

- `in_progress`

완료 기준:

- compact-managed scene gap table이 문서화됨
- `observer`, `exporter`, `canonicalization`, `display`, `advisor-input`가 구분됨
- `shop`은 supported MVP지만 authoritative live root는 provisional로 남는다고 명시됨
- `combat`는 preview-only / no-call boundary로 남고, reward/event/shop gap과 별도 residual risk가 분리됨

### WS6. Compact Advisor Managed Scene MVP

목표:

- reward/event/shop model-backed compact advisor input을 시험한다.
- manual request만 허용하고 auto advice는 기본 OFF로 유지한다.
- `recommendedChoiceLabel` exact-match와 `missingInformation` / `decisionBlockers` 정합성을 본다.
- unsupported/degraded response는 shared factory로 생성한다.
- `combat`는 preview-only facts surface로 유지한다.

현재 상태:

- `in_progress`

완료 기준:

- reward/event/shop compact input이 stable하게 생성됨
- replay 기반 dry run이 reward/event/shop read-only advice를 검증함
- live sidecar manual request로 reward/event/shop advice를 확인 가능함
- `recommendedChoiceLabel`이 current scene option label과 정확히 대응
- unsupported/degraded response가 shared helper로 생성됨
- Host는 manual gating만 담당하고, compact prompt/finalizer/fallback은 Foundation 공용 경로를 지난다
- `BuildSlice(...)` post-filter compact knowledge slice가 replay/live 모두에서 같은 기준으로 동작한다

### WS7. Live Sidecar UI

목표:

- 직접 플레이 중 현재 scene model을 별도 WPF UI에서 실시간으로 읽을 수 있게 한다.
- overlay가 아니라 sidecar window를 우선한다.
- 이번 정보 품질 wave에서는 scene-aware display formatter, core vs advanced layout, display-only knowledge/localization이 핵심이다.

주요 산출물:

- [M9_LIVE_SIDECAR_UI_PLAN_KO.md](/mnt/c/users/jidon/source/repos/STS2_Mod_AI_Companion/docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md)
- `src/Shared/AdvisorSceneModel/*`
- `src/Shared/AdvisorSceneDisplay/*`
- `src/Sts2AiCompanion.Host/AdvisorScene/*`
- `src/Sts2AiCompanion.Wpf`
- `src/Sts2AiCompanion.Wpf/Display/*`

현재 상태:

- `in_progress`

완료 기준:

- live scene type / stage / owner / summary / options / missing facts가 sidecar에 표시됨
- Harness와 Host가 같은 `ScreenProvenanceResolver`로 resolved current/visible/ready/authority/stability를 계산함
- replay schema와 live schema가 일치함
- `artifacts/companion/<runId>/advisor-scene/advisor-scene.latest.json` + `advisor-scene.ndjson`가 생성됨
- unchanged poll에서는 `advisor-scene.ndjson`가 append되지 않음
- `SceneSummaryText`는 raw summary를 그대로 복사하지 않고 scene-aware display formatter를 거친다
- advanced 영역은 기본 접힘으로 유지된다

### WS8. Strategy Principles And 3-View Advice

목표:

- `strategy-principles` artifact를 broad catalog에 섞지 않고 background-only lens로 연결한다.
- reward/event/shop compact prompt에 scene-local strict strategy principles를 최대 3개만 주입한다.
- 조언 출력을 `최종 / 보수적 / 공격적` 3-view로 additive 확장한다.
- canonical top-level 조언은 항상 `FinalView`와 정렬하고, exact-label safety를 유지한다.

주요 산출물:

- `artifacts/knowledge/strategy/strategy-principles.sts.json`
- `src/Sts2AiCompanion.Foundation/Knowledge/StrategyPrinciplesService.cs`
- `src/Sts2AiCompanion.Foundation/Contracts/StrategyPrinciplesContracts.cs`
- `src/Sts2AiCompanion.Foundation/Reasoning/CompactAdvisor/AdviceResponseViewFinalizer.cs`
- `src/Sts2AiCompanion.Wpf/Display/AdviceViewDisplayFormatter.cs`

현재 상태:

- `in_progress`

완료 기준:

- strategy principles가 separate additive field로 `AdviceInputPack`에 들어간다.
- retrieval은 scene-local strict mapping만 사용하고, low confidence는 제외한다.
- compact prompt에 `strategy_principles` section이 들어가되 facts보다 우선하지 않는다.
- `FinalView`, `ConservativeView`, `AggressiveView`가 additive response shape로 연결된다.
- canonical top-level 조언은 항상 `FinalView`와 정렬된다.
- auxiliary invalid label은 해당 view에만 국한되고, valid final response를 degrade시키지 않는다.
- combat는 strategy principles가 있어도 preview-only / no-call을 유지한다.
- WPF가 `최종 / 보수적 / 공격적` 3개 블록을 일관되게 표시한다.

## 현재 진척도 보드

| Item | Status | Notes |
|---|---|---|
| M9 pointer 승격 | completed | current docs 기준 `M9 advice-quality entry` |
| Scene inventory 초안 | in_progress | coverage matrix 존재, provisional shop root 남음 |
| Scene model contract 초안 | in_progress | 3-layer boundary와 typed fields 정의됨 |
| Harness-local scene builder | in_progress | advisor substrate code와 replay path 존재 |
| Human-readable summary | in_progress | formatter 존재, fixture stabilization 필요 |
| Advisor input contract | in_progress | compact-managed scene contract + adapter-first 경로, `reward/event/shop` model-call + `combat` preview-only / no-call 정리 |
| Coverage gap 운영 | in_progress | compact-managed scene gap table, provisional shop root, reward direct fact seam open gap 정리 |
| Compact Advisor Managed Scene MVP | in_progress | `Host` gating + `Foundation` compact builder/finalizer/fallback + reward/event/shop replay dry run, combat preview-only no-call 유지 |
| Live sidecar UI | in_progress | `Host` owner live builder, shared contract, WPF panel, advisor-scene artifact band, shared `ScreenProvenanceResolver` parity까지 구현 완료. direct play clean-boot manual sweep만 남음 |
| Strategy principles artifact | completed | `strategy-principles.sts.json` 생성, background-only note 반영 |
| Strategy lens + 3-view advice | in_progress | separate loader, strict retrieval, prompt wiring, additive 3-view finalizer/WPF까지 연결. live style validation, direct-play sweep, docs sync만 남음 |
| Foundation canonical merge | blocked_by_design | schema 안정화 전 금지 |
| Real-time overlay UI | deferred | 텍스트/artifact proving 이후 단계 |

## Current Evidence Roots

- authoritative fresh live:
  - `artifacts/gui-smoke/endurance-longrun-20260404-live28`
  - `artifacts/gui-smoke/endurance-longrun-20260404-live29`
- provisional shop root:
  - `artifacts/gui-smoke/verify-shop-slot-source-20260322-0131`

## Acceptance Gates

### Gate A. Schema Safety

- scene truth가 `request goal`이나 actuator field에 의존하지 않음
- `missingFacts`는 추정값이 아니라 실제 gap만 올림
- summary에 `allowedActions`, `fallback reason`, `targetLabel` 같은 actuator vocabulary가 없음

### Gate B. Fixture Stability

- 대표 fixture root를 반복 replay해도 field churn이 크지 않음
- scene type / stage / canonical owner가 일관되게 생성됨
- option labels가 visible UI와 맞음

### Gate C. Scope Discipline

- current harness routing / allowed-actions / actuator logic을 건드리지 않음
- observer/exporter 추가는 coverage matrix에서 hard blocker가 드러난 경우에만 진행
- compact input과 read-only advisor model-call은 `reward/event/shop`만 범위로 두고, `combat`는 preview-only / no-call로 유지함

### Gate D. Advisor Readiness

- reward/event/shop 3개 scene에서 compact input 기반 read-only advisor dry run 가능
- 조언 결과를 scene model과 provenance로 설명할 수 있음

## 절대 하지 말아야 할 것

M9에서는 아래 anti-pattern을 절대 허용하지 않는다.

1. live path 전용 임시 precedence patch 추가
- 예:
  - `currentScreen=combat이면 foregroundOwner=map 무시`
  - `combat marker가 2개 이상이면 map 대신 combat`
- 이유:
  - 하네스에서 이미 겪은 authority/handoff 꼬임을 sidecar 경로에서 다시 반복하게 된다.
  - 장면 truth를 예외 규칙 목록으로 관리하기 시작하면 M9 scene model이 금방 drift한다.

2. 하네스와 다른 장면 authority 규칙을 sidecar에서 따로 만들기
- replay/harness와 Host는 같은 `ScreenProvenanceResolver`로 primary `current / visible / ready / authority / stability`를 계산해야 한다.
- `compatibilityCurrentScreen / compatibilityLogicalScreen / compatibilityVisibleScreen / compatibilitySceneReady / compatibilitySceneAuthority / compatibilitySceneStability`는 parity/diagnostics surface일 뿐, primary truth winner가 아니다.
- `foregroundOwner` 하나만 더 믿거나, `currentChoices`만 더 믿는 식의 live-only 해석기는 만들지 않는다.

3. raw meta를 scene truth로 바로 승격
- `foregroundOwner`, `foregroundActionLane`, `currentScene`, `mapCurrentActiveScreen` 같은 단일 meta 값을 곧바로 최종 truth로 쓰지 않는다.
- raw meta는 provenance input이다. 최종 scene truth는 shared provenance/authority resolver를 거쳐야 한다.

4. AI advice / diagnostics가 scene panel publish를 막게 두기
- M9 초반의 주 목적은 `scene model 관측`이다.
- auto advice, knowledge build, diagnostics heavy refresh가 sidecar scene update보다 앞서면 안 된다.
- current 구현에서도 `scene model publish`는 advice/diagnostics보다 먼저 가는 fast path이며, 이 순서를 유지해야 한다.

5. 하네스 전체 decision/routing 로직을 Host로 가져오기
- 필요한 것은 `행동 결정`이 아니라 `read-only authority/provenance contract`다.
- `same-action-stall`, routing, allowed-actions, actuator suppression 같은 하네스 전용 행위 로직을 Host 쪽에 옮기지 않는다.

6. scene model과 advisor input을 다시 섞기
- SceneModel은 fact model이다.
- `recommendedChoice`, `decisionBlockers`, `fallback reason`, `allowedActions`, `reasoningMode`를 scene model 필드로 올리지 않는다.

7. observer gap을 추정값으로 메우기
- 비는 정보는 `missingFacts` / `observerGaps`로 남겨야 한다.
- route context, event identity, shop price/effect, combat intent 같은 값을 sidecar convenience를 위해 추정하지 않는다.

8. foundation merge를 조급하게 진행
- live/replay schema stability와 manual direct-play 검증이 충분하지 않으면 foundation contract로 올리지 않는다.
- M9 v1은 proving schema다. shared source는 허용되지만 foundation canonical merge는 별도 gate를 통과해야 한다.

9. blocker-fix식 임시 heuristic을 scene model에 누적
- timer suppression, recapture count, stale label special-case, 특정 scene 전용 broad suppress를 M9 scene model에 넣지 않는다.
- 장면 authority 문제는 provenance/authority resolver에서 좁게 해결한다.

## 다음 Work Units

### WU1. Shop live refresh

- provisional `shop` root를 fresh direct-play/live root로 교체한다.
- sidecar panel과 `advisor-scene.latest.json`을 기준으로 shop price/effect gap을 다시 기록한다.

### WU2. Direct-play clean boot sweep

- AGENTS 가드레일대로 clean boot 후 reward/event/rest-site/shop/map/combat 진입을 수동 확인한다.
- mismatch가 있으면 `missingFacts / observerGaps / confidence / sourceRefs`가 설명 가능한지 본다.

### WU3. Reward direct fact seam follow-up

- explicit reward scene fact가 knowledge miss보다 우선되는 경계를 더 좁게 정리한다.
- direct fact seam은 개선됐지만 reward fact ownership은 아직 open gap으로 남기고, opaque reward card 정보는 계속 explicit missing으로 유지한다.

### WU4. Direct-play advice sweep and style validation

- clean-boot direct play에서 reward/event/shop manual advice와 combat preview-only surface를 같이 점검한다.
- recommendation label 정합성, 3-view 표시, manual-only / preview-only guard, missingInformation 품질을 다시 확인한다.

## 리스크

1. raw observer/meta가 advisor vocabulary를 오염시키는 것
2. scene model과 advisor input을 너무 빨리 합쳐서 drift가 나는 것
3. provisional root를 acceptance-grade root처럼 취급하는 것
4. reward fact ownership / direct fact seam을 닫힌 문제처럼 문서화하는 것
5. combat를 full recommendation처럼 넓혀 preview-only / no-call guard를 흐리는 것
6. foundation merge를 너무 빨리 해서 unstable schema를 굳히는 것

## 운영 원칙

1. harness는 주력 제품이 아니라 proving ground + acceptance lane으로 본다
2. scene model contract/summary는 `src/Shared/AdvisorSceneModel`에 두고, replay/live builder는 harness/host adapter로 분리한다
3. display-only sanitizer / knowledge resolver는 `src/Shared/AdvisorSceneDisplay`에 두고, WPF scene-aware formatter는 `src/Sts2AiCompanion.Wpf/Display`에 둔다
4. 문서와 fixture를 함께 갱신한다
5. blocker-fix loop와 M9 workstream을 섞지 않는다
6. 새 live blocker가 나오면 M5~M8 reopen으로 분리하고, M9 계획 자체와 혼합하지 않는다

## 완료 선언 조건

아래를 만족하면 M9 wave 1 완료로 본다.

1. scene inventory / scene model / summary / advisor input draft 문서가 모두 current 상태다
2. representative scene fixture에서 advisor scene artifact가 안정적으로 생성된다
3. reward + event + shop read-only advisor dry run과 combat preview-only / no-call contract가 current 문서와 일치한다
4. foundation merge는 하지 않았지만, merge gate를 통과할 조건이 문서/fixture로 명확하다
