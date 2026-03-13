# STS2_Mod_AI_Companion 작업 인계 프롬프트

아래 지시를 그대로 읽고, 현재 저장소의 작업을 이어서 진행하라.

## 1. 프로젝트 정체성과 최종 목표

이 프로젝트의 **궁극적인 완성물**은 다음이다.

- 별도 프로세스에서 사용자의 STS2 게임을 지켜보며
- 게임 상태를 읽고 해석한 뒤
- 사람 대신 플레이하지는 않지만
- 적절한 맥락적 조언을 제공하는
- **read-only AI companion**

즉 최종 제품은 여전히 **Production Mode의 external observer advisor**다.

동시에 이 저장소는 이제 단일 advisor 앱이 아니라, 아래 두 운영 모드가 공존하는 **dual-mode architecture**로 재해석되어야 한다.

- `Production Mode`
  - read-only external advisor
  - 사용자가 직접 플레이
  - 외부 WPF/UI와 Codex가 상태를 읽고 조언
- `Test Mode`
  - action-enabled automated harness
  - 사람 개입 없이 회귀 테스트 / 시나리오 테스트 / recovery / replay 수행
  - production mode를 더 빨리 검증하기 위한 test-only 모드

중요:

- production purity를 해치면 안 된다.
- production mod는 계속 read-only여야 한다.
- action layer는 test-only harness path에만 존재해야 한다.

## 2. 이 저장소를 어떻게 이해해야 하는가

이 저장소는 더 이상 “조언 앱만 있는 repo”가 아니다.
현재 구조는 아래 세 층으로 이해해야 한다.

### Shared Foundation
- runtime exporter
- live export / collector / diagnostics
- normalized state vocabulary
- static knowledge extraction
- knowledge slice / prompt contract
- run/session/artifact model
- Codex orchestration contracts

### Advisor Mode
- WPF UI
- manual analyze / retry / auto advice UX
- user-facing recommendation generation
- read-only interaction

### Harness Mode
- test-only action ingress
- scenario runner
- policy engine
- recovery manager
- pass/fail evaluator
- replay controller

## 3. 반드시 먼저 읽을 문서

이 작업을 이어받는 AI는 아래 문서를 반드시 먼저 읽고 숙지해야 한다.

1. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\DOCUMENT_MAP.md`
   - 전체 문서 지도
2. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\PROJECT_STATUS.md`
   - 현재 구현/검증 상태
3. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\DUAL_MODE_ARCHITECTURE.md`
   - dual-mode 상위 구조
4. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\ARCHITECTURE.md`
   - 상위 아키텍처 요약
5. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\02-runtime\GAMEPLAY_RUNTIME_FLOW.md`
   - 게임 실행 중 데이터 흐름
6. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\04-advisor\AI_ASSISTANT_ARCHITECTURE.md`
   - advisor 경로
7. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\05-harness\HARNESS_MODE.md`
   - harness 경로
8. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\03-knowledge\KNOWLEDGE_EXTRACTION.md`
   - 정적 지식 파이프라인
9. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\REPO_STRUCTURE.md`
   - 프로젝트/폴더 구조

하네스 observer / scene authority 작업 전에는 아래 문서를 추가 기준 문서로 반드시 읽는다.

- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\05-harness\DECOMPILED_SOURCE_FIRST_OBSERVER_STRATEGY.md`
  - `decompiled source-first`, `event + polling mixed observer`, `transition-oriented hook` 원칙
필요 시 추가로 읽을 문서:

- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\REALTIME_EXTRACTION.md`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\SMOKE_TEST_CHECKLIST.md`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\02-runtime\PENDING_HOOKS_AND_RISKS.md`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\06-history\WORKLOG.md`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\06-history\DETAILED_INVESTIGATION_LOG.md`

## 4. 현재 핵심 코드 컨텍스트

### Runtime / Exporter
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\RuntimeExportContext.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\RuntimeSnapshotReflectionExtractor.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\RuntimeHookCatalog.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.Mod\Runtime\AiCompanionRuntimeIdentity.cs`

### Core / LiveExport / Knowledge
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModKit.Core\LiveExport\LiveExportModels.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModKit.Core\LiveExport\LiveExportStateTracker.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModKit.Core\LiveExport\LiveExportPaths.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModKit.Core\Knowledge\*`

### Foundation / Advisor / Harness
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Advisor`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Harness`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.HarnessBridge`

특히 먼저 봐야 할 파일:

- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation\Contracts\CompanionState.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation\Contracts\HarnessAction.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Foundation\State\CompanionStateMapper.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Advisor\AdvisorCoordinator.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Harness\Scenarios\ScenarioRunner.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Harness\Policies\DeterministicPolicyEngine.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Harness\Recovery\RecoveryManager.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModAiCompanion.HarnessBridge\HarnessBridgeHost.cs`
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2ModKit.Tool\HarnessCommands.cs`

### Legacy / Migration Shim
- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\src\Sts2AiCompanion.Host`

이 프로젝트는 완전히 제거 대상이 아니라 **migration shim**으로 남아 있다.
새 코드는 가능하면 Foundation / Advisor / Harness 쪽에 두고, Host는 호환 목적의 얇은 계층으로만 취급하라.

## 5. 지금까지 달성한 성과

### Production-side
- runtime exporter 기본 기동
- main menu 기준 live export 생성
- strict canonical 정적 지식 추출
- WPF advisor UI 기본형
- manual advice 경로
- Codex session capture / same-run session reuse 기본형
- collector mode와 collector summary 구조

### Knowledge
- 카드/유물/포션/이벤트/상점/보상/키워드 strict canonical 정리
- 사람용 Markdown과 assistant용 JSON export 모두 존재
- prompt pack에 knowledge slice 실제 포함

### Architecture
- dual-mode 문서화 완료
- Foundation / Advisor / Harness / HarnessBridge 프로젝트 골격 추가
- WPF가 advisor 계층을 보도록 1차 분리 시작

## 6. 현재 상태에서 가장 중요한 사실

이것은 꼭 이해해야 한다.

### A. 정적 지식은 “부족해서 못 하는 상태”가 아니다
- 현재 병목은 정적 지식량보다 runtime state/choice extraction과 auto advice 안정성이다.

### B. production advisor는 아직 실제 gameplay end-to-end로 닫히지 않았다
- main menu까지는 실증
- reward/event/shop/rest에서 `currentScreen`, `currentChoices`, `auto advice`는 아직 미완료

### C. harness는 아직 “자동화 가능한 구조”까지만 왔지, 자동 테스트가 닫힌 것은 아니다
- scaffolding은 있음
- 실제 unattended scenario 완주는 아직 실패

### D. 하네스 observer 접근 방식은 이제 `decompiled source-first + event/polling mixed observer`다
- polling은 버릴 대상이 아니라 continuous state, reconciliation, watchdog 용도로 계속 중요하다.
- 다만 `scene transition`, `screen ready`, `lifecycle boundary` 판단은 polling 순간값만으로 확정하지 않는다.
- 다음 하네스 observer 작업은 먼저 `artifacts/knowledge/decompiled`에서 흐름과 메서드 후보를 찾고, 그 다음 runtime hook/event로 검증한다.
- 특히 `NMainMenu::_Ready`, `NMainMenu.SingleplayerButtonPressed`, `NSingleplayerSubmenu.OpenCharacterSelect`, `NCharacterSelectScreen.OnEmbarkPressed`를 먼저 보라.
- transient polled scene이 남아 있는 동안 `dispatch_node`를 다시 열지 마라.

## 7. 현재 가장 큰 미해결 문제

### 1. 런타임 배포 정합성 문제
현재 로그상 가장 치명적인 문제:

- `Method not found: LiveExportAtomicFileWriter.WriteJsonAtomic(...)`

이 오류가 runtime log에 반복되면 현재 런은 신뢰하면 안 된다.
즉, 최신 코드가 들어갔다고 생각해도 실제 게임 `mods` 폴더의 DLL이 stale일 수 있다.

중요한 최근 상태:
- source에는 compatibility overload, writer compatibility logging, SHA256 deploy 진단이 들어갔다.
- 하지만 실제 최근 런에서는 여전히 stale DLL 흔적이 보였다.
- 따라서 **다음 작업 시작 전 최신 build + deploy 정합성 확인이 1순위**다.

### 2. harness first-reward 무인 시나리오 미완성
현재 최소 harness PoC 목표는 아래다.

- `main menu`
- `새 런 시작`
- `아이언클래드 선택`
- `첫 전투 노드 선택`
- `첫 보상 화면에서 첫 카드 선택`
- `맵 복귀`

현재 실패 포인트:
- map -> combat 전이가 false positive일 수 있음
- `player-hand-unavailable`
- overlay contamination
- `Dismisser / Exclaim / Question`
- `NSendFeedbackScreen`
- `NMultiplayerTimeoutOverlay`
- bridge-stalled recovery가 잘못 `cancel`을 눌러 이상 UI로 들어갈 수 있음

### 3. 검은 화면 / 응답은 현재 정상 테스트 상태가 아니다
중요:
- 최근 검은 화면 런은 **정상 테스트 상태가 아님**
- 렌더링 자체가 망가졌다기보다 stale deploy + harness 입력 꼬임 + runtime exception 상태일 가능성이 높다
- 이 상태의 결과를 기반으로 성공/실패를 판단하면 안 된다

### 4. auto advice / currentChoices / screen persistence
reward / event / shop / rest에서
- `currentScreen`이 semantic하게 유지되지 않음
- `currentChoices`가 실제 선택지가 아니라 내부 UI 이름일 수 있음
- auto advice가 너무 늦거나 중복되거나 degraded로 끝남

## 8. 최근 하네스 관련 중요 발견

### bridge action 전략
- 하네스 action executor의 기본 경로는 **test-only in-mod bridge**
- 외부 UI automation은 보조 수단일 뿐, 주 경로가 아님

### main menu / start path
decompiled 근거상 가장 안정적인 경로:

- `NMainMenu -> SingleplayerButtonPressed`
- 필요 시 `NSingleplayerSubmenu.OpenCharacterSelect`
- `NCharacterSelectButton.Select()`
- `NCharacterSelectScreen.OnEmbarkPressed(null)`

즉 generic UI label click보다 **decompiled semantic path-first**가 더 중요하다.

### overlay contamination
현재 harness와 runtime extraction에서 다음이 state/choice를 오염시키는 핵심 후보다.

- `NMultiplayerTimeoutOverlay`
- `NSendFeedbackScreen`
- `Dismisser`
- `Exclaim`
- `Question`

이들은 실제 choice나 player state로 승격되면 안 된다.

## 9. 반드시 지켜야 할 작업 원칙

1. Production Mode는 계속 read-only로 유지한다.
2. action은 HarnessBridge 같은 test-only 경로에만 둔다.
3. WPF에 harness concern을 섞지 않는다.
4. Foundation state vocabulary를 advisor/harness가 공통으로 사용하게 만든다.
5. stale deployment 상태에서는 어떤 테스트 결과도 신뢰하지 않는다.
6. manual gameplay smoke를 당장 사용자에게 다시 요구하지 않는다. 이번 iteration은 가능한 한 무개입으로 간다.

## 10. 다음 AI가 즉시 해야 할 우선순위

### 1순위: stale deploy / ABI mismatch부터 끊어라
다음 작업 시작 시 가장 먼저 해야 할 일:

1. 최신 솔루션 build
2. production mod + harness bridge 최신 deploy
3. deploy된 `Sts2ModKit.Core.dll`와 mod DLL identity 확인
4. runtime log에서 `WriteJsonAtomic` mismatch가 사라졌는지 확인

이 문제가 남아 있으면 그 아래 모든 결과는 신뢰 금지.

### 2순위: harness first-reward PoC를 닫아라
이번 iteration의 harness 목표는 딱 하나다.

- `run-harness-scenario --scenario scenarios/smoke.ironclad.first-reward.json`
- 사람이 직접 플레이하지 않고
- first reward 선택까지 자동으로 완료
- `artifacts/harness/<run-id>/evaluation.json`에서 `passed=true`

### 3순위: replay 기반 advisor regression을 닫아라
사람 개입 없이 production advisor 흐름을 계속 검증할 수 있게

- replay fixture 생성/관리
- prompt-pack 생성
- advice.latest.json/md 생성
- degraded/fallback artifact 생성

을 CLI와 테스트로 고정하라.

### 4순위: runtime choice/state 안정화
reward/event/shop/rest에서
- `currentScreen`
- `currentChoices`
- deck/player state
- auto advice trigger

가 실제 의미 단위로 잡히도록 collector artifact 기준으로 보정하라.

## 11. 지금 바로 참고할 명령

### 빌드 / 기본 검증
```powershell
dotnet build STS2_Mod_AI_Companion.sln
dotnet run --project src\Sts2ModKit.SelfTest --no-build
```

### 정적 지식
```powershell
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

### collector 후처리
```powershell
dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 200 --tail 40
```

### harness 시나리오
```powershell
dotnet run --project src\Sts2ModKit.Tool -- run-harness-scenario --scenario scenarios\smoke.ironclad.first-reward.json
```

## 12. 다른 AI에게 그대로 줄 최종 지시문

아래를 그대로 사용하라.

---

당신은 `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion` 저장소의 작업을 이어받는 개발 AI다.

이 프로젝트의 최종 제품은 **read-only external advisor**다. 사용자가 직접 STS2를 플레이하고, 외부 프로세스가 게임 상태를 읽어 문맥적 조언을 제공한다. 사람 대신 플레이하지 않는다.

동시에 이 저장소는 이제 **dual-mode architecture**로 재정렬되고 있다.

- `Production Mode`: read-only advisor
- `Test Mode`: action-enabled automated harness

둘은 별도 프로젝트가 아니라 **shared foundation**을 공유한다.

반드시 먼저 아래 문서를 읽어라.

1. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\DOCUMENT_MAP.md`
2. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\PROJECT_STATUS.md`
3. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\DUAL_MODE_ARCHITECTURE.md`
4. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\ARCHITECTURE.md`
5. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\02-runtime\GAMEPLAY_RUNTIME_FLOW.md`
6. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\04-advisor\AI_ASSISTANT_ARCHITECTURE.md`
7. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\05-harness\HARNESS_MODE.md`
8. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\03-knowledge\KNOWLEDGE_EXTRACTION.md`
9. `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\docs\development\01-overview\REPO_STRUCTURE.md`

반드시 기억할 현재 사실:

- 정적 지식은 이미 충분히 모였고, 현재 병목은 runtime state / choice extraction / auto advice 안정성이다.
- recent runtime logs에서 `WriteJsonAtomic` ABI mismatch가 다시 보였으므로 stale deploy 가능성이 높다.
- 최근 검은 화면 런은 정상 테스트 상태가 아니며 결과를 신뢰하면 안 된다.
- harness는 현재 scaffolding과 부분 구현까진 되었지만 first-reward unattended PoC는 아직 실패한다.
- 가장 중요한 next milestone은 `first reward unattended harness scenario 1개 완주`다.

핵심 코드 컨텍스트:

- Runtime exporter:
  - `src\Sts2ModAiCompanion.Mod\Runtime\RuntimeExportContext.cs`
  - `src\Sts2ModAiCompanion.Mod\Runtime\RuntimeSnapshotReflectionExtractor.cs`
  - `src\Sts2ModAiCompanion.Mod\Runtime\RuntimeHookCatalog.cs`
  - `src\Sts2ModAiCompanion.Mod\Runtime\AiCompanionRuntimeIdentity.cs`
- Foundation:
  - `src\Sts2AiCompanion.Foundation\Contracts\CompanionState.cs`
  - `src\Sts2AiCompanion.Foundation\Contracts\HarnessAction.cs`
  - `src\Sts2AiCompanion.Foundation\State\CompanionStateMapper.cs`
- Advisor:
  - `src\Sts2AiCompanion.Advisor\AdvisorCoordinator.cs`
- Harness:
  - `src\Sts2AiCompanion.Harness\Scenarios\ScenarioRunner.cs`
  - `src\Sts2AiCompanion.Harness\Policies\DeterministicPolicyEngine.cs`
  - `src\Sts2AiCompanion.Harness\Recovery\RecoveryManager.cs`
  - `src\Sts2AiCompanion.Harness\State\LiveCompanionStateSource.cs`
- Test-only bridge:
  - `src\Sts2ModAiCompanion.HarnessBridge\HarnessBridgeHost.cs`
- Tooling:
  - `src\Sts2ModKit.Tool\HarnessCommands.cs`

작업 우선순위:

1. 최신 build/deploy 정합성부터 복구하고, runtime log에서 `WriteJsonAtomic` mismatch가 사라졌는지 확인하라.
2. `scenarios/smoke.ironclad.first-reward.json`를 사람이 개입하지 않고 끝까지 돌리는 harness PoC를 닫아라.
3. reward/event/shop/rest에서 `currentScreen`, `currentChoices`, deck/player state 오염을 줄여라.
4. replay 기반 production advisor regression도 사람이 플레이하지 않아도 돌아가게 하라.

하네스 구현 원칙:

- action executor 기본 경로는 `test-only in-mod bridge`
- 외부 UI automation은 보조 경로일 뿐, 주 경로가 아니다
- production read-only purity를 해치지 마라
- WPF에 harness concern을 섞지 마라

특히 유의할 현재 문제:

- `NMultiplayerTimeoutOverlay`, `NSendFeedbackScreen`, `Dismisser`, `Exclaim`, `Question`가 state/choice를 오염시킬 수 있다
- `main menu -> singleplayer -> character select`는 generic click이 아니라 decompiled semantic path-first로 가야 한다
- stale deploy 상태에서는 어떤 harness/artifact 결과도 신뢰 금지

현재 iteration의 최종 목표:

- 사람이 직접 플레이하지 않고
- harness가 first reward 선택까지 자동으로 진행하며
- `artifacts/harness/<run-id>/evaluation.json`에서 `passed=true`
- 동시에 replay 기반 advisor regression도 유지되는 상태

작업 중 문서도 같이 갱신하라. 특히:

- `PROJECT_STATUS.md`
- `DUAL_MODE_ARCHITECTURE.md`
- `AI_ASSISTANT_ARCHITECTURE.md`
- `HARNESS_MODE.md`
- `WORKLOG.md`

---


## 13. 2026-03-13 추가 인계 사항

- 최신 하네스 경계 작업은 `dormant bridge + arm/session token + stale queue replay 차단`을 기준으로 정리되고 있습니다.
- bridge는 `inventory.latest.json`을 export하고, external preferred command는 `dispatch_node`입니다.
- external commander의 transport는 현재 단계에서 파일 기반으로 유지합니다.
- screenshot 수집 루프는 아직 별도 범위이고, bridge/tool/file contract만 먼저 닫는 것이 이번 단계의 목표입니다.
- 멀티 에이전트 Codex worker는 이 세션에서 안정적으로 신뢰하기 어려웠으므로, 구현과 검증은 단일 실행 주체 전략을 우선합니다.
