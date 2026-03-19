# STS2 Mod AI Companion

Slay the Spire 2의 현재 게임 상태를 게임 밖으로 안전하게 추출하고, 그 상태와 정적 지식 카탈로그를 바탕으로 AI 조언을 제공하는 외부 어시스턴트 프로젝트입니다.

현재 저장소의 중심은 아래 네 축입니다.

- 게임 내부 `read-only runtime exporter`
- 정적 지식 추출 파이프라인
- 외부 `Host + Codex` 조언 경로
- `WPF` 기반 사용자 UI

이제부터 이 저장소는 단일 advisor 앱만이 아니라, **shared foundation 위에 두 가지 운영 모드가 공존하는 시스템**으로 해석합니다.

- `Production Mode`
  - 사용자가 직접 플레이
  - 외부 프로세스 advisor가 read-only로 상태를 읽고 조언
- `Test Mode`
  - 같은 foundation을 쓰되 test-only action layer로 자동 시나리오를 실행하는 harness

이 프로젝트는 자동 플레이나 teammate AI를 만들지 않습니다. 사람은 직접 플레이하고, 어시스턴트는 현재 화면과 선택지를 해석해 조언만 제공합니다.

## 현재 되는 것

- 모드 로드와 Harmony startup 초기화
- main menu 기준 live export 4종 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- strict parser 기반 정적 지식 카탈로그 생성
- `Host`, `WPF`, `SelfTest`, `Tool` 전체 빌드
- manual advice 경로
  - WPF `지금 분석`
  - `dotnet run --project src\Sts2ModKit.Tool -- analyze-live-once`
- run-scoped Codex `sessionId` 저장과 `codex-session.json` 기반 세션 복구 경로 구현
- collector mode 기반 수집 런 진단 구조
  - raw observation / choice candidate / choice decision / semantic snapshot / collector summary

## 아직 남은 것

- 실제 gameplay에서 `reward / event / shop / rest` 화면 유지 안정화
- 실제 선택지 텍스트 추출 완성
- automatic advice를 gameplay trigger 기준으로 다시 실증
- gameplay 중 run-scoped Codex session 재사용 실증
- AppHang 재현 조건 수집과 원인 축소

즉 지금은 `정적 지식 + exporter + manual advice + session capture + UI`까지는 돌아가고, 다음 병목은 실제 플레이 중 고가치 화면에서 입력 파이프라인과 자동 조언을 닫는 일입니다.

## 빠른 시작

1. 로컬 설정 파일 준비

```powershell
Copy-Item config\ai-companion.sample.json config\ai-companion.local.json
```

2. 전체 빌드

```powershell
dotnet build STS2_Mod_AI_Companion.sln
```

3. self-test 실행

```powershell
dotnet run --project src\Sts2ModKit.SelfTest --no-build
```

4. 정적 지식 재생성 및 점검

```powershell
dotnet run --project src\Sts2ModKit.Tool -- extract-static-knowledge
dotnet run --project src\Sts2ModKit.Tool -- inspect-static-knowledge
```

5. live smoke 준비

```powershell
dotnet run --project src\Sts2ModKit.Tool -- snapshot
dotnet run --project src\Sts2ModKit.Tool -- deploy-native-package
dotnet run --project src\Sts2ModKit.Tool -- prepare-live-smoke
cmd /c start "" "steam://rungameid/2868840"
dotnet run --project src\Sts2ModKit.Tool -- inspect-godot-log --lines 200
dotnet run --project src\Sts2ModKit.Tool -- inspect-live-export --tail 20
```

6. 외부 어시스턴트 UI 실행

```powershell
dotnet run --project src\Sts2AiCompanion.Wpf
```

## collector mode

한 판 플레이에서 최대한 많은 진단 정보를 모으려면 config의 `liveExport.collectorModeEnabled`를 `true`로 켭니다. collector mode가 켜지면 기존 live export 4종 외에 다음 파일이 추가됩니다.

- `raw-observations.ndjson`
- `screen-transitions.ndjson`
- `choice-candidates.ndjson`
- `choice-decisions.ndjson`
- `semantic-snapshots/*`

플레이가 끝난 뒤에는 아래 명령으로 후처리합니다.

```powershell
dotnet run --project src\Sts2ModKit.Tool -- collector-postprocess --lines 200 --tail 40
```

이 명령은 다음을 한 번에 요약합니다.

- request latency
- duplicate / coalesced trigger
- screen overwrite
- state regression
- missing information top N
- decision blockers top N
- knowledge usage
- runtime fatal errors
- apphang suspicion indicators

## 정적 지식 산출물

사람이 읽는 문서:

- `artifacts/knowledge/markdown/README.md`
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
- `artifacts/knowledge/markdown/cards.md`
- `artifacts/knowledge/markdown/relics.md`
- `artifacts/knowledge/markdown/potions.md`
- `artifacts/knowledge/markdown/events.md`
- `artifacts/knowledge/markdown/shops.md`
- `artifacts/knowledge/markdown/rewards.md`
- `artifacts/knowledge/markdown/keywords.md`

AI가 읽는 파일:

- `artifacts/knowledge/catalog.assistant.json`
- `artifacts/knowledge/catalog.assistant.txt`
- `artifacts/knowledge/assistant/index.json`
- `artifacts/knowledge/assistant/*.json`

## companion 산출물

- `artifacts/companion/current-run.json`
- `artifacts/companion/<run-id>/live-mirror/`
- `artifacts/companion/<run-id>/prompt-packs/`
- `artifacts/companion/<run-id>/advice/`
- `artifacts/companion/<run-id>/advice/advice.ndjson`
- `artifacts/companion/<run-id>/advice/advice.latest.json`
- `artifacts/companion/<run-id>/advice/advice.latest.md`
- `artifacts/companion/<run-id>/host-status.json`
- `artifacts/companion/<run-id>/codex-session.json`
- `artifacts/companion/<run-id>/collector-summary.json`
- `artifacts/companion/<run-id>/codex-trace.ndjson`

## 현재 정적 지식 기준선

최신 strict canonical 기준 대략적인 규모:

- cards: `595`
- relics: `296`
- potions: `68`
- events: `62`
- shops: `5`
- rewards: `11`
- keywords: `264`

정적 지식은 이미 AI가 쓸 수 있는 기본 사전 수준입니다. 다음 병목은 지식량이 아니라 실제 gameplay에서 `screen`, `currentChoices`, `deck/state`, `session reuse`, `auto advice`를 안정적으로 연결하는 것입니다.

## Codex 연동 상태

현재 확인된 성공 범위:

- Codex CLI 호출
- manual advice 생성
- latest advice JSON / Markdown 저장
- `thread.started` / 세션 인덱스 기반 `sessionId` 캡처
- `codex-session.json` 저장과 같은 run 기준 host 재기동 후 세션 복구 경로
- `Analyze Now`와 `Retry Last`의 의미 분리
- automatic trigger latest-only coalescing과 `runtime-poll` 제외

현재 미실증 범위:

- gameplay automatic trigger 기준 `ok` advice
- gameplay trigger에서도 같은 `sessionId` 유지

## 관련 문서

처음 문서를 읽을 때는 아래 순서를 권장합니다.

1. `docs/development/DOCUMENT_MAP.md`
2. `docs/development/01-overview/PROJECT_STATUS.md`
3. `docs/development/01-overview/AI_HANDOFF_PROMPT_KO.md`
4. `docs/development/01-overview/DUAL_MODE_ARCHITECTURE.md`
5. `docs/ARCHITECTURE.md`
6. `docs/development/02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
7. `docs/development/04-advisor/AI_ASSISTANT_ARCHITECTURE.md`

- `docs/ARCHITECTURE.md`
- `docs/BOUNDARIES.md`
- `docs/ROADMAP.md`
- `docs/REALTIME_EXTRACTION.md`
- `docs/SMOKE_TEST_CHECKLIST.md`
- `docs/BACKUP_AND_ROLLBACK.md`
- `docs/development/README.md`
- `docs/development/DOCUMENT_MAP.md`
- `docs/development/01-overview/PROJECT_STATUS.md`
- `docs/development/01-overview/DUAL_MODE_ARCHITECTURE.md`
- `docs/development/01-overview/REPO_STRUCTURE.md`
- `docs/development/02-runtime/GAMEPLAY_RUNTIME_FLOW.md`
- `docs/development/03-knowledge/KNOWLEDGE_EXTRACTION.md`
- `docs/development/03-knowledge/SPIRE_CODEX_REFERENCE.md`
- `docs/development/04-advisor/AI_ASSISTANT_ARCHITECTURE.md`
- `docs/development/05-harness/HARNESS_MODE.md`
