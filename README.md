# STS2 Mod AI Companion

Slay the Spire 2의 현재 게임 상태를 게임 밖으로 안전하게 꺼내고, 그 상태와 정적 지식 카탈로그를 바탕으로 AI 조언을 제공하는 외부 어시스턴트 프로젝트입니다.

현재 저장소의 중심은 아래 네 축입니다.

- 게임 내부 `read-only runtime exporter`
- 정적 지식 추출 파이프라인
- 외부 `Host + Codex` 조언 경로
- `WPF` 사용자 인터페이스

이 프로젝트는 자동 플레이나 teammate AI를 만들지 않습니다. 목표는 사람이 직접 플레이하는 동안 현재 상태를 이해하고 중요한 선택 순간마다 조언을 제공하는 것입니다.

## 현재까지 확인된 상태

현재 실제로 확인된 항목:

- 모드 로드
- Harmony startup failure 제거
- main menu까지 live export 4종 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- strict parser 기반 정적 지식 카탈로그 생성
- Host / WPF 빌드 및 self-test 통과
- manual advice 경로 동작
  - `Analyze Now`
  - `dotnet run --project src\Sts2ModKit.Tool -- analyze-live-once`
- reward / event / rest / shop hook observed

아직 gameplay 기준으로 끝까지 닫히지 않은 항목:

- reward / event / shop / rest / combat start / turn start 화면의 실제 live smoke
- gameplay 중 `currentChoices`가 모든 고가치 화면에서 안정적으로 잡히는지
- automatic advice가 실제 gameplay trigger에서 `ok`로 생성되는지
- run-scoped Codex session 생성 / 재사용 추적

즉, 지금은 “정적 지식 + exporter + manual advice + UI 뼈대”까지는 확인됐고, 다음 핵심 과제는 “실제 플레이 한 판에서 auto advice가 붙는지”입니다.

## 빠른 시작

1. 로컬 설정 파일을 준비합니다.

```powershell
Copy-Item config\ai-companion.sample.json config\ai-companion.local.json
```

2. 솔루션을 빌드합니다.

```powershell
dotnet build STS2_Mod_AI_Companion.sln
```

3. self-test를 실행합니다.

```powershell
dotnet run --project src\Sts2ModKit.SelfTest --no-build
```

4. 정적 지식 카탈로그를 생성하고 확인합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

5. live smoke를 준비합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package
dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke
cmd /c start "" "steam://rungameid/2868840"
dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200
dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20
```

6. 외부 어시스턴트 UI를 실행합니다.

```powershell
dotnet run --project src\Sts2AiCompanion.Wpf
```

## 정적 지식 추출 파이프라인

현재 `extract-static-knowledge`는 아래 순서로 동작합니다.

1. `release-scan`
2. `decompile-scan`
3. `assembly-scan`
4. `pck-inventory`
5. `strict-domain-parse`
6. `localization-scan`
7. `observed-merge`
8. `catalog-build`

역할은 다음처럼 나뉩니다.

- raw / intermediate
  - `assembly-scan.json`
  - `pck-inventory.json`
  - `localization-scan.json`
- canonical
  - `catalog.latest.json`
  - `catalog.latest.txt`
- assistant
  - `catalog.assistant.json`
  - `catalog.assistant.txt`
  - `assistant/*.json`
- 사람용 markdown
  - `artifacts/knowledge/markdown/*.md`

현재 canonical 기준 counts:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `5`
- rewards: `7`
- keywords: `262`

현재 localization coverage:

- cards: `582` / descriptions: `569` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `4` / descriptions: `4`
- rewards: `3` / descriptions: `2`
- keywords: `245` / descriptions: `245`

## 주요 산출물 위치

사람이 읽는 지식 문서:

- `artifacts/knowledge/markdown/README.md`
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
- `artifacts/knowledge/markdown/cards.md`
- `artifacts/knowledge/markdown/relics.md`
- `artifacts/knowledge/markdown/potions.md`
- `artifacts/knowledge/markdown/events.md`
- `artifacts/knowledge/markdown/shops.md`
- `artifacts/knowledge/markdown/rewards.md`
- `artifacts/knowledge/markdown/keywords.md`

AI가 읽는 지식 파일:

- `artifacts/knowledge/catalog.assistant.json`
- `artifacts/knowledge/catalog.assistant.txt`
- `artifacts/knowledge/assistant/index.json`
- `artifacts/knowledge/assistant/cards.json`
- `artifacts/knowledge/assistant/relics.json`
- `artifacts/knowledge/assistant/potions.json`
- `artifacts/knowledge/assistant/events.json`
- `artifacts/knowledge/assistant/shops.json`
- `artifacts/knowledge/assistant/rewards.json`
- `artifacts/knowledge/assistant/keywords.json`

live export:

- `%AppData%\\SlayTheSpire2\\steam\\<steamId>\\modded\\profile<index>\\ai_companion\\live\\`

companion artifacts:

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice.ndjson`
- `artifacts/companion/<run-id>/advice.latest.json`
- `artifacts/companion/<run-id>/advice.latest.md`
- `artifacts/companion/<run-id>/host-status.json`
- `artifacts/companion/<run-id>/codex-session.json` if session tracking succeeds

## 현재 Codex 연동 상태

현재 확인된 것은 아래입니다.

- Codex CLI 호출: 성공
- manual advice JSON/Markdown 생성: 성공
- WPF / tool에서 manual advice 사용: 가능

아직 확정되지 않은 것은 아래입니다.

- automatic advice가 UTF-8 수정 이후 fresh gameplay trigger에서 성공하는지
- `sessionId`를 안정적으로 잡아 run-scoped Codex session을 이어 붙일 수 있는지

즉, 현재는 “Codex와 실제로 대화해 조언을 받는 것”은 되지만, “런 단위 세션 생성 / 재사용”은 아직 닫히지 않았습니다.

## 저장소 구조

- `src/Sts2ModAiCompanion.Mod`
  - 게임 내부 모드와 runtime exporter
- `src/Sts2ModKit.Core`
  - live export 모델, diagnostics, knowledge pipeline, configuration
- `src/Sts2ModKit.Tool`
  - build / snapshot / deploy / smoke / inspect / knowledge CLI
- `src/Sts2ModKit.SelfTest`
  - 비파괴 self-test
- `src/Sts2AiCompanion.Host`
  - live export watcher, knowledge slice, prompt pack, Codex client
- `src/Sts2AiCompanion.Wpf`
  - 외부 조언 UI

## 관련 문서

- `docs/ARCHITECTURE.md`
- `docs/BOUNDARIES.md`
- `docs/ROADMAP.md`
- `docs/REALTIME_EXTRACTION.md`
- `docs/SMOKE_TEST_CHECKLIST.md`
- `docs/BACKUP_AND_ROLLBACK.md`
- `docs/development/README.md`
- `docs/development/PROJECT_STATUS.md`
- `docs/development/KNOWLEDGE_EXTRACTION.md`
- `docs/development/GAMEPLAY_RUNTIME_FLOW.md`
- `docs/development/AI_ASSISTANT_ARCHITECTURE.md`
- `docs/development/REPO_STRUCTURE.md`
- `docs/development/SPIRE_CODEX_REFERENCE.md`
