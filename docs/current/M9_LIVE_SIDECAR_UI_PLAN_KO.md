# M9 Live Sidecar UI 계획 (KO)

> 상태: 현재 사용 중
> 기준 브랜치: `main`
> 최종 갱신: 2026-04-04
> 목적: 사령관님이 직접 플레이할 때, 별도 UI에서 현재 scene model과 핵심 정보를 실시간으로 읽을 수 있게 만드는 read-only sidecar 계획

## 한 줄 요약

이번 workstream의 목표는 게임 위 overlay가 아니라, 기존 `WPF advisor window`를 확장해서 `현재 scene / human-readable summary / options / missing facts`를 실시간으로 보여주는 것이다.

## 왜 이 작업을 하나

M9 v1에서 이미 얻은 것은 아래다.

- artifact-first scene model
- human-readable summary
- 장면별 missing facts / observer gaps

지금 부족한 것은:

- 실제 플레이 중 현재 읽는 정보를 바로 확인하는 live feedback surface

즉 이 workstream의 가치는:

- 사령관님이 직접 플레이하면서
- `지금 화면을 시스템이 어떻게 이해하는지`
- `무슨 선택지를 읽고 있는지`
- `무엇을 아직 모르는지`
를 별도 창에서 즉시 볼 수 있게 만드는 것이다.

## 범위

이번 단계에서 하는 것:

1. live export/normalized state에서 advisor scene model을 실시간 생성
2. 기존 WPF sidecar에 scene model 전용 패널 추가
3. 현재 scene summary / options / missing facts / confidence / provenance를 표시
4. read-only 모드 유지

이번 단계에서 하지 않는 것:

1. overlay 주입
2. actuator 실행
3. 게임 화면 직접 클릭/키보드 제어
4. foundation canonical merge
5. polished product UX 완성

## 구현 방향

### 1. Live Scene Model Source

원칙:

- replay builder와 같은 의미 계약을 유지하되,
- live side는 `request`가 없으므로 live snapshot/normalized state 기준으로 scene model을 만든다.

권장 seam:

- live export snapshot
- normalized scene
- current choices
- recent events
- collector/degradation info

가능한 구현 경로:

1. harness-local builder 재사용 범위를 넓힌다
2. 또는 live 전용 adapter를 추가해 같은 scene contract로 매핑한다

중요:

- replay artifact용 builder와 live UI용 builder가 별도 schema를 가지면 안 된다
- schema는 같고, input adapter만 달라야 한다

### 2. Sidecar UI Surface

기존 대상:

- [ShellViewModel.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Wpf/ShellViewModel.cs)
- [MainWindow.xaml](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Wpf/MainWindow.xaml)
- [AdvisorCoordinator.cs](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/src/Sts2AiCompanion.Advisor/AdvisorCoordinator.cs)

추가할 표시 항목:

1. `sceneType`
2. `sceneStage`
3. `canonicalOwner`
4. `summaryText`
5. `options`
6. `missingFacts`
7. `observerGaps`
8. `confidence`
9. `sourceRefs`

우선순위:

- 보기 좋은 UI보다 읽기 정확한 UI
- “지금 화면을 어떻게 읽고 있는가”가 먼저

### 3. Live Feedback Loop

실시간 개발 루프는 아래로 고정한다.

1. 게임 플레이
2. sidecar가 현재 scene model 표시
3. 화면과 scene model을 비교
4. mismatch를 coverage gap 또는 normalization gap으로 기록
5. 문서와 fixture를 갱신

## 구현 단계

### Stage 1. Read-only live panel

목표:

- WPF에 scene model 패널을 추가한다
- advice 패널과 분리해서 “현재 읽는 정보”를 먼저 보여 준다

완료 기준:

- scene type / stage / owner / summary가 실시간으로 보임
- options / missing facts가 보임
- 플레이 중 장면 전환에 따라 갱신됨

### Stage 2. Scene provenance panel

목표:

- `왜 이렇게 읽었는지`를 운영자가 볼 수 있게 한다

표시 항목:

- source refs
- confidence
- degraded / missing reason

완료 기준:

- mismatch가 나왔을 때 “scene model 부족인지 observer 부족인지” 구분 가능

### Stage 3. Read-only advisor integration

목표:

- live scene model을 advisor input 전 단계로 연결

완료 기준:

- reward/event 두 scene에서 scene model 기반 advisor dry run 가능
- 추천과 scene model options label이 정확히 대응

## 현재 진척도

| Item | Status | Notes |
|---|---|---|
| replay scene model | completed | harness-local substrate 존재 |
| human-readable summary | completed | formatter 존재 |
| live sidecar panel | pending | 이번 workstream owner |
| live scene model adapter | pending | replay/live seam 정렬 필요 |
| provenance panel | pending | confidence/source refs UI 필요 |
| read-only advisor live attach | pending | reward/event부터 시작 |

## acceptance

### Gate A. Readability

- sidecar만 보고 현재 장면을 사람이 빠르게 이해할 수 있다
- summary와 options가 실제 화면과 어긋나지 않는다

### Gate B. Non-Invasiveness

- gameplay actuation을 건드리지 않는다
- 하네스 동작과 advisor UI를 섞지 않는다

### Gate C. Contract Consistency

- replay scene model과 live sidecar scene model이 같은 schema를 쓴다
- live path만의 임시 필드가 생기지 않는다

### Gate D. Debuggability

- mismatch가 나왔을 때 source refs / missing facts / observer gaps로 원인을 좁힐 수 있다

## 필수로 먼저 읽을 문서

1. [AGENTS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/AGENTS.md)
2. [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md)
3. [M9_EXECUTION_PLAN_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/M9_EXECUTION_PLAN_KO.md)
4. [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_SCENE_MODEL_READER_KO.md)
5. [ADVISOR_SCENE_INFORMATION_MODEL.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/ADVISOR_SCENE_INFORMATION_MODEL.md)
6. [ADVISOR_INPUT_OUTPUT_CONTRACT.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/contracts/ADVISOR_INPUT_OUTPUT_CONTRACT.md)
7. [ARCHITECTURE.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/ARCHITECTURE.md)

## 작업하면서 반드시 업데이트할 문서

1. [M9_EXECUTION_PLAN_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/M9_EXECUTION_PLAN_KO.md)
   - current progress / next work unit / acceptance gate
2. [ADVISOR_UI_COVERAGE_MATRIX_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md)
   - live sidecar에서 확인된 missing / degraded / new evidence root
3. [ADVISOR_SCENE_MODEL_READER_KO.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/ADVISOR_SCENE_MODEL_READER_KO.md)
   - 사람이 실제로 어떻게 읽는지 바뀌면 예시와 설명 갱신
4. [PROJECT_STATUS.md](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/docs/current/PROJECT_STATUS.md)
   - active M9 workstream 포인터가 바뀌면 갱신

## 구현 세션 프롬프트

```text
역할:
이번 세션은 M9의 `live sidecar UI` workstream이다.
목표는 사령관님이 직접 플레이할 때, 기존 WPF advisor 창에서 현재 scene model과 핵심 정보를 실시간으로 읽을 수 있게 만드는 것이다.

이번 세션은 read-only다.
- actuator 변경 금지
- overlay 주입 금지
- gameplay auto-decision 변경 금지
- foundation merge 금지

반드시 먼저 읽을 문서:
1. AGENTS.md
2. docs/current/PROJECT_STATUS.md
3. docs/current/M9_EXECUTION_PLAN_KO.md
4. docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md
5. docs/current/ADVISOR_SCENE_MODEL_READER_KO.md
6. docs/contracts/ADVISOR_SCENE_INFORMATION_MODEL.md
7. docs/contracts/ADVISOR_INPUT_OUTPUT_CONTRACT.md
8. docs/ARCHITECTURE.md

우선 볼 코드:
1. src/Sts2AiCompanion.Wpf/ShellViewModel.cs
2. src/Sts2AiCompanion.Wpf/MainWindow.xaml
3. src/Sts2AiCompanion.Advisor/AdvisorCoordinator.cs
4. src/Sts2AiCompanion.Host/CompanionHost.cs
5. src/Sts2AiCompanion.Foundation/State/CompanionSceneNormalizer.cs
6. src/Sts2GuiSmokeHarness/AdvisorSubstrate/GuiSmokeAdvisorSceneContracts.cs
7. src/Sts2GuiSmokeHarness/AdvisorSubstrate/GuiSmokeAdvisorSceneModelBuilder.cs
8. src/Sts2GuiSmokeHarness/AdvisorSubstrate/GuiSmokeAdvisorSceneSummaryFormatter.cs

핵심 원칙:
- replay scene model과 live scene model은 같은 schema를 써야 한다
- scene truth는 raw request가 아니라 live snapshot + canonical/normalized scene state에서 만든다
- 사람이 읽는 정보와 actuator vocabulary를 섞지 않는다
- mismatch를 숨기지 말고 missingFacts/observerGaps/confidence로 드러낸다

구현 목표:
1. live side에서 advisor scene model을 만들 adapter/seam 정의
2. WPF sidecar에 scene model 패널 추가
3. 표시 항목:
   - sceneType
   - sceneStage
   - canonicalOwner
   - summaryText
   - options
   - missingFacts
   - observerGaps
   - confidence
   - sourceRefs
4. reward/event/rest-site/shop/map/combat 대표 장면에서 live 갱신이 자연스럽게 보이게 만들기

반드시 작업하면서 업데이트할 문서:
1. docs/current/M9_EXECUTION_PLAN_KO.md
2. docs/current/M9_LIVE_SIDECAR_UI_PLAN_KO.md
3. docs/current/ADVISOR_UI_COVERAGE_MATRIX_KO.md
4. docs/current/ADVISOR_SCENE_MODEL_READER_KO.md
5. 필요 시 docs/current/PROJECT_STATUS.md

검증:
1. 관련 csproj build
2. WPF 실행 후 live export 연결 확인
3. scene transition 시 panel이 갱신되는지 확인
4. reward/event/rest-site/map 최소 4개 장면에서 화면과 표시 정보가 맞는지 확인
5. mismatch가 있으면 missingFacts/observerGaps로 설명 가능한지 확인

최종 보고 형식:
1. Root path chosen for live scene model
2. WPF/UI changes
3. Contract consistency with replay scene model
4. Validation
5. Updated docs
6. Residual risk
```

## 한 줄 결론

이 workstream의 목표는 “AI가 무엇을 추천할지”보다 먼저, “시스템이 지금 화면을 어떻게 읽고 있는지”를 사령관님이 실시간으로 확인할 수 있게 만드는 것이다.
