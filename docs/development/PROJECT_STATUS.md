# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-11`
- 기준 게임 버전: `STS2 v0.98.3`
- 기준 도구 체인: `Godot 4.5.1`, `.NET 7`

이 문서는 `docs/development` 아래에서 가장 먼저 보는 현재 상태 문서입니다. 현재 구현 범위, 실제로 검증된 것, 아직 코드만 있고 실플레이로 닫히지 않은 것, 다음 우선순위를 한 번에 정리합니다.

## 1. 현재 저장소의 중심

현재 저장소의 중심은 아래 5개입니다.

1. 게임 내부에서 동작하는 read-only native exporter
2. exporter가 남기는 live export 파일 4종
3. 오프라인 정적 지식 추출 파이프라인
4. 외부 Host + Codex backend
5. WPF 기반 AI 조언 앱

즉 목표는 `게임을 대신 플레이하는 모드`가 아니라 `플레이 중인 사람에게 외부 창에서 조언을 주는 어시스턴트`입니다.

## 2. 현재 확인된 것

### 2.1 빌드와 기본 검증

현재 기준으로 아래는 통과 상태입니다.

- `dotnet build STS2_Mod_AI_Companion.sln`
- `dotnet run --project src\Sts2ModKit.SelfTest --no-build`
- `dotnet run --project src\Sts2ModKit.Tool --no-build -- extract-static-knowledge`
- `dotnet run --project src\Sts2ModKit.Tool --no-build -- inspect-static-knowledge`

### 2.2 runtime exporter

현재 실증된 범위:

- 모드 로드
- Harmony startup failure 제거
- main menu까지 live export 4종 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- scene polling 기반 상태 갱신

아직 실플레이 smoke로 닫히지 않은 범위:

- reward
- event
- shop
- rest
- combat start
- turn start

즉 exporter는 `메인 메뉴까지는 산다`가 현재 확정 상태이고, 고가치 gameplay 화면은 다음 smoke에서 닫아야 합니다.

### 2.3 정적 지식 추출

현재 파이프라인:

1. `release-scan`
2. `assembly-scan`
3. `pck-inventory`
4. `localization-scan`
5. `observed-merge`
6. `catalog-build`

최신 `inspect-static-knowledge` 기준 localization coverage:

- cards: `572` / descriptions: `538` / selection prompts: `35`
- relics: `291` / descriptions: `289`
- potions: `75` / descriptions: `70`
- events: `197` / descriptions: `95` / options: `203`
- shops: `21` / descriptions: `18`
- rewards: `3` / descriptions: `2`
- keywords: `270` / descriptions: `263`

전체 inventory count는 여전히 큽니다.

- cards: `7748`
- relics: `2366`
- potions: `1078`
- events: `1411`
- shops: `417`
- rewards: `288`
- keywords: `1758`

해석 원칙:

- 큰 inventory count는 `후보가 많다`는 뜻이지 `바로 AI 입력에 100% 신뢰로 넣어도 된다`는 뜻은 아닙니다.
- 실제 판단에 바로 쓰기 좋은 계층은 `localization-scan`과 `assistant export`입니다.

현재 AI가 우선적으로 읽는 산출물:

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

`KnowledgeCatalogService`는 현재 `catalog.assistant.json`이 있으면 그것을 우선 로드하고, 없을 때만 `catalog.latest.json`으로 내려갑니다.

### 2.4 Host / WPF

현재 코드상 존재:

- `src/Sts2AiCompanion.Host`
- `src/Sts2AiCompanion.Wpf`

현재 코드상 확인된 것:

- `CompanionHost`가 live export polling loop를 올립니다.
- `KnowledgeCatalogService`가 assistant catalog를 reload하고 bounded knowledge slice를 만듭니다.
- `AdvicePromptBuilder`가 `AdviceInputPack`과 markdown advice를 생성합니다.
- `CodexCliClient`가 Codex session create/resume 흐름을 담당합니다.
- WPF는 현재 상태, 현재 화면, choices, recent events, knowledge entries, latest advice를 표시합니다.
- WPF 버튼:
  - `Analyze Now`
  - `Pause/Resume Auto Advice`
  - `Retry Last`
  - `Refresh Knowledge`
  - `Open Artifacts`

아직 실플레이로 닫히지 않은 것:

- actual gameplay live export에 붙은 상태에서 auto advice
- `Analyze Now`와 `Retry Last`의 실플레이 end-to-end
- run 종료 summary와 artifact 보존의 실사용 흐름

## 3. 사용자 관점에서 지금 가능한 것

현재 상태를 사용자 시나리오로 요약하면:

- 게임을 켜면 mod/exporter가 main menu까지 live export를 만들 수 있습니다.
- 외부 앱 구조는 이미 존재하고 빌드됩니다.
- 정적 지식 카탈로그에서는 카드뿐 아니라 유물, 포션, 이벤트, 상점, 키워드 일부 설명도 읽을 수 있습니다.
- assistant export는 AI가 읽기 쉬운 형태로 따로 생성됩니다.

하지만 아직 아래는 `코드상 준비됨`이지 `실플레이로 증명됨`은 아닙니다.

- reward/event/shop/rest/combat advice
- gameplay 중 `currentChoices` 기반 자동 조언
- WPF가 실제 gameplay run에 붙어 advice를 안정적으로 보여주는 흐름

## 4. 현재 최우선 남은 일

우선순위는 아래 순서로 고정합니다.

1. high-value gameplay smoke 재개
2. `currentChoices` 실증
3. WPF + Host + Codex의 실제 gameplay 연결 검증
4. relic / event / shop localization 확장과 정규화
5. replay 기반 무인 acceptance harness

## 5. 현재 가장 큰 리스크

### 5.1 exporter coverage 리스크

main menu 이후 gameplay 고가치 화면이 아직 충분히 실증되지 않았습니다.

### 5.2 knowledge noise 리스크

catalog 전체 수치는 크지만, 그중 상당수는 여전히 assembly/pck seed입니다.

### 5.3 locale/fallback 리스크

한국어 우선 경로는 많이 좋아졌지만, 영어 fallback과 locale 경계는 아직 보수적으로 처리됩니다.

### 5.4 end-to-end 리스크

Host/WPF/Codex는 코드상 연결돼 있지만 실제 gameplay run에서 자동 조언까지 닫힌 상태는 아닙니다.

## 6. 다음 검증 순서

실작업은 아래 순서를 따릅니다.

1. `dotnet build STS2_Mod_AI_Companion.sln`
2. `dotnet run --project src\Sts2ModKit.SelfTest --no-build`
3. `dotnet run --project src\Sts2ModKit.Tool --no-build -- extract-static-knowledge`
4. `dotnet run --project src\Sts2ModKit.Tool --no-build -- inspect-static-knowledge`
5. live smoke가 필요하면:
   - `snapshot`
   - `deploy-native-package`
   - `prepare-live-smoke`
   - `cmd /c start "" "steam://rungameid/2868840"`
   - `inspect-godot-log`
   - `inspect-live-export`

## 7. 한 줄 상태 요약

현재 저장소는 `main menu까지 살아 있는 exporter + 다도메인 L10N이 붙기 시작한 정적 지식 파이프라인 + 실제 gameplay smoke가 남은 외부 AI 조언 앱` 상태입니다.
