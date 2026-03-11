# STS2 Mod AI Companion

Slay the Spire 2의 현재 상태를 게임 밖으로 안전하게 꺼내고, 그 상태와 정적 지식을 바탕으로 AI 조언을 제공하는 외부 어시스턴트 저장소입니다.

현재 저장소의 중심은 아래 네 축입니다.

- 게임 내부 `read-only native runtime exporter`
- 게임 설치본 기반 `정적 지식 추출 파이프라인`
- 외부 `Host + Codex backend`
- 사용자용 `WPF 조언 앱`

## 현재 범위

Phase 1 목표는 아래입니다.

- 게임 상태를 실시간으로 구조화해 live export로 기록
- 중요한 화면에서 선택지와 현재 상태를 외부 프로세스가 읽을 수 있게 유지
- 외부 앱이 현재 상태와 지식 카탈로그를 합쳐 AI 조언을 표시

현재 범위 밖:

- 자동 플레이
- teammate AI
- 멀티플레이 개입
- intrusive patching
- 게임 안 오버레이 우선 구현

## 빠른 시작

1. 로컬 설정을 확인합니다.

```powershell
copy config\ai-companion.sample.json config\ai-companion.local.json
```

2. 솔루션을 빌드합니다.

```powershell
dotnet build STS2_Mod_AI_Companion.sln
```

3. self-test를 실행합니다.

```powershell
dotnet run --project src\Sts2ModKit.SelfTest --no-build
```

4. 현재 설정과 live export 경로를 확인합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- show-config
dotnet run --project src\Sts2ModKit.Tool -- show-live-export-layout
```

5. 정적 지식 카탈로그를 생성하고 확인합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

6. live smoke가 필요하면 snapshot부터 시작합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package
dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke
cmd /c start "" "steam://rungameid/2868840"
dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200
dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20
```

7. 외부 어시스턴트 앱을 실행합니다.

```powershell
dotnet run --project src\Sts2AiCompanion.Wpf
```

## 정적 지식 파이프라인 요약

현재 `extract-static-knowledge`는 아래 순서로 동작합니다.

1. `release-scan`
2. `decompile-scan`
3. `assembly-scan`
4. `pck-inventory`
5. `strict-domain-parse`
6. `localization-scan`
7. `observed-merge`
8. `catalog-build`

핵심 구분:

- `assembly-scan`, `pck-inventory`, `localization-scan`은 넓게 수집하는 raw/intermediate 계층
- `strict-domain-parse`는 실제 디컴파일 소스 기준으로 카드/유물/포션/이벤트 canonical seed를 만드는 계층
- `catalog.latest.*`, `catalog.assistant.*`, `assistant/*.json`, `markdown/*.md`는 사람이 읽거나 AI가 직접 읽는 최종 산출물

`inspect-static-knowledge` 최신 기준:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `438`
- rewards: `292`
- keywords: `2027`

여기서 `cards/relics/potions/events`는 strict parser 기반 canonical 수치입니다. `shops/rewards/keywords`는 아직 broad raw seed가 많이 남아 있어 후속 정규화 대상입니다.

localization coverage 최신 기준:

- cards: `575` / descriptions: `562` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `23` / descriptions: `18`
- rewards: `4` / descriptions: `2`
- keywords: `272` / descriptions: `264`

## 산출물 위치

사람이 읽는 리포트:

- `artifacts\knowledge\markdown\README.md`
- `artifacts\knowledge\markdown\PLAY_GUIDE.md`
- `artifacts\knowledge\markdown\cards.md`
- `artifacts\knowledge\markdown\relics.md`
- `artifacts\knowledge\markdown\potions.md`
- `artifacts\knowledge\markdown\events.md`
- `artifacts\knowledge\markdown\shops.md`
- `artifacts\knowledge\markdown\rewards.md`
- `artifacts\knowledge\markdown\keywords.md`

AI가 직접 읽는 산출물:

- `artifacts\knowledge\catalog.assistant.json`
- `artifacts\knowledge\catalog.assistant.txt`
- `artifacts\knowledge\assistant\index.json`
- `artifacts\knowledge\assistant\cards.json`
- `artifacts\knowledge\assistant\relics.json`
- `artifacts\knowledge\assistant\potions.json`
- `artifacts\knowledge\assistant\events.json`
- `artifacts\knowledge\assistant\shops.json`
- `artifacts\knowledge\assistant\rewards.json`
- `artifacts\knowledge\assistant\keywords.json`

## 현재 실증 상태

실제로 확인된 것:

- 모드 로드
- Harmony startup failure 제거
- main menu까지 live export 4종 생성
- 정적 지식 추출과 assistant export 생성
- Host/WPF 프로젝트 빌드 및 self-test

아직 gameplay로 닫지 못한 것:

- reward / event / shop / rest / combat start / turn start 화면 실증
- 실제 gameplay에서 `currentChoices` 안정 추출
- WPF + Host + Codex의 gameplay end-to-end 조언 흐름

## 저장소 구조

- `src\Sts2ModAiCompanion.Mod`
  - 게임 안에 로드되는 native mod와 runtime exporter
- `src\Sts2ModKit.Core`
  - live export 모델, snapshot/restore, diagnostics, knowledge pipeline 공용 로직
- `src\Sts2ModKit.Tool`
  - build/snapshot/deploy/smoke/inspect/extract CLI
- `src\Sts2ModKit.SelfTest`
  - 비파괴 회귀 테스트
- `src\Sts2AiCompanion.Host`
  - live export watcher, knowledge slice, prompt pack, Codex backend
- `src\Sts2AiCompanion.Wpf`
  - 외부 조언 앱 UI

## 주요 문서

- `docs\ARCHITECTURE.md`
- `docs\BOUNDARIES.md`
- `docs\ROADMAP.md`
- `docs\REALTIME_EXTRACTION.md`
- `docs\SMOKE_TEST_CHECKLIST.md`
- `docs\BACKUP_AND_ROLLBACK.md`
- `docs\development\README.md`
- `docs\development\PROJECT_STATUS.md`
- `docs\development\KNOWLEDGE_EXTRACTION.md`
- `docs\development\GAMEPLAY_RUNTIME_FLOW.md`
- `docs\development\REPO_STRUCTURE.md`
- `docs\development\SPIRE_CODEX_REFERENCE.md`

개발 과정, 트러블슈팅, 초보자용 설명은 `docs\development\` 아래에 정리합니다.
