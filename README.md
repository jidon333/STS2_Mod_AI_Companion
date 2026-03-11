# STS2 Mod AI Companion

Slay the Spire 2용 `외부 AI 조언 어시스턴트`를 구현하고 검증하는 저장소입니다.

현재 저장소의 핵심은 아래 4개입니다.

- 게임 내부의 read-only native runtime exporter
- 정적 지식 카탈로그 추출 파이프라인
- 외부 프로세스 host / Codex backend
- WPF 데스크톱 조언 앱

## 현재 범위

현재 Phase 1은 아래를 목표로 합니다.

- 게임 상태를 실시간으로 안정적으로 추출
- 중요한 화면에서 선택지와 상태를 구조화
- 외부 앱에서 현재 상태와 AI 조언을 함께 표시

현재 일부러 제외하는 것:

- 자동 플레이
- teammate AI
- 멀티플레이 개입
- intrusive patching
- 인게임 overlay 우선 구현

## 빠른 시작

1. `config\ai-companion.sample.json`의 경로를 확인합니다.
2. 솔루션을 빌드합니다.

```powershell
dotnet build STS2_Mod_AI_Companion.sln
```

3. self-test를 돌립니다.

```powershell
dotnet run --project src\Sts2ModKit.SelfTest
```

4. 현재 설정과 live export 경로를 확인합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- show-config
dotnet run --project src\Sts2ModKit.Tool -- show-live-export-layout
```

5. 정적 지식 카탈로그를 생성/확인합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

생성 결과는 `artifacts\knowledge\` 아래의 JSON과 `artifacts\knowledge\markdown\` 아래의 사람이 읽는 Markdown 리포트에서 함께 확인할 수 있습니다.
Markdown 리포트는 `PLAY_GUIDE.md`와 카테고리별 문서를 통해, 이 데이터가 실제 플레이 중 어디에서 쓰이는지도 함께 설명합니다.

6. live smoke가 필요하면 snapshot부터 시작합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package
dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke
cmd /c start "" "steam://rungameid/2868840"
dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200
dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20
```

7. WPF 앱을 실행합니다.

```powershell
dotnet run --project src\Sts2AiCompanion.Wpf
```

## 저장소 구조

- `src\Sts2ModAiCompanion.Mod`
  - native 모드 진입점, runtime exporter, 패키징
- `src\Sts2ModKit.Core`
  - 설정, snapshot/restore, live export 모델, static knowledge 코어
- `src\Sts2ModKit.Tool`
  - 로컬 CLI, smoke/packaging/snapshot/inspect 명령
- `src\Sts2ModKit.SelfTest`
  - 비게임 검증
- `src\Sts2AiCompanion.Host`
  - live export watcher, knowledge slice, Codex 호출, companion artifact 기록
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
- `docs\development\GAMEPLAY_RUNTIME_FLOW.md`
- `docs\development\REPO_STRUCTURE.md`

개발 과정과 트러블슈팅, 초보자용 설명은 `docs\development\` 아래에 한글로 정리합니다.

## 2026-03-11 정적 지식/L10N 갱신 메모

- `artifacts/knowledge/localization-scan.json` 기준으로 카드뿐 아니라 유물, 포션, 이벤트, 상점, 보상, 키워드까지 L10N 추출이 실제로 연결됩니다.
- 최신 `inspect-static-knowledge` 기준 localization coverage는 다음과 같습니다.
  - cards: 572 / descriptions: 538 / selection prompts: 35
  - relics: 291 / descriptions: 289
  - potions: 75 / descriptions: 70
  - events: 197 / descriptions: 95 / options: 203
  - shops: 21 / descriptions: 18
  - rewards: 3 / descriptions: 2
  - keywords: 270 / descriptions: 263
- AI 전용 산출물은 `artifacts/knowledge/assistant/` 아래에 따로 생성됩니다.
  - `cards.json`, `relics.json`, `potions.json`, `events.json`, `shops.json`, `rewards.json`, `keywords.json`, `index.json`
- 이 assistant JSON은 한국어 설명, 선택지, 모델 클래스, 리소스 경로, L10N key, source file hint를 함께 담도록 맞춰져 있습니다.
- 외부 교차검증 힌트로는 `https://spire-codex.com/` 구조를 참고하되, 현재 저장소의 canonical source는 게임 설치본에서 직접 추출한 로컬 산출물입니다.
- 참고 내용과 반영 범위는 `docs/development/SPIRE_CODEX_REFERENCE.md`에 따로 정리했습니다.
