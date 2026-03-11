# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-11`
- 기준 게임 버전: `STS2 v0.98.3`
- 기준 도구 체인: `Godot 4.5.1`, `.NET 7`

## 1. 지금 이 저장소는 무엇을 하는가

현재 저장소의 중심은 다음 5개입니다.

1. 게임 안에서 동작하는 read-only native exporter
2. exporter가 남기는 live export 파일 4종
3. 정적 지식 추출 파이프라인
4. 외부 Host + Codex backend
5. WPF 기반 AI 조언 앱

즉, 목표는 “게임을 대신 플레이하는 모드”가 아니라 “게임 밖에서 현재 상황을 읽고 조언을 제공하는 외부 어시스턴트”입니다.

## 2. 현재 확인된 것

### 2.1 빌드/기본 검증

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

아직 미실증:

- reward
- event
- shop
- rest
- combat start / turn start

즉, exporter는 “메인 메뉴까지는 살았다”가 현재 확정 상태이고, gameplay 고가치 화면 coverage는 다음 smoke 대상입니다.

### 2.3 정적 지식 추출

현재 파이프라인:

- `release-scan`
- `assembly-scan`
- `pck-inventory`
- `localization-scan`
- `observed-merge`
- `catalog-build`

이번 단계의 핵심 변화:

- `localization-scan` 추가
- 카드 `title`, `description`, `selectionScreenPrompt` 복구
- card markdown 리포트에 실제 카드 설명 본문 반영

현재 inspect 기준:

- cards: `7748`
- relics: `2366`
- potions: `1078`
- events: `1411`
- shops: `417`
- rewards: `288`
- keywords: `1758`
- localization-scan cards: `1713`
- localization descriptions: `1501`

중요한 해석:

- 전체 card count는 여전히 seed/noise가 많이 섞인 inventory입니다.
- 하지만 그중 `1713`개는 localization 계층까지 닿았고,
- 그중 `1501`개는 실제 설명 본문이 채워졌습니다.

즉, 정적 지식은 이제 “이름만 있는 후보 모음”에서 “카드 설명을 실제로 읽을 수 있는 카탈로그”로 한 단계 올라왔습니다.

### 2.4 Host / WPF

현재 존재:

- `src/Sts2AiCompanion.Host`
- `src/Sts2AiCompanion.Wpf`

현재 확인:

- 빌드 가능
- self-test 범위에 계약 검증 포함
- knowledge slice, prompt builder, companion artifact 경로가 기본 동작

아직 미실증:

- 실제 gameplay live export에 붙은 상태에서 auto advice
- manual `Analyze Now`
- run 종료 summary와 artifact 보존의 end-to-end

## 3. 사용자 입장에서 지금 가능한 것

지금 상태를 사용자 시나리오로 요약하면:

- 게임을 켜면 mod/exporter가 live export 파일을 만들 수 있음
- 외부 앱 구조는 이미 준비되어 있음
- 정적 지식 카탈로그에서는 카드 설명을 읽을 수 있음

하지만 아직 다음은 “코드상 준비됨”이지 “실플레이로 증명됨”은 아닙니다.

- reward/event/shop/rest/combat advice
- WPF에서 실제 live run attach
- gameplay 중 current choices 기반 자동 조언

## 4. 현재 최우선 남은 일

우선순위는 아래 순서로 고정합니다.

1. high-value gameplay smoke 재개
2. `currentChoices` 실증
3. WPF + Host + Codex의 실제 gameplay 연결 검증
4. relic / event / shop localization 확장
5. replay 기반 무인 acceptance harness

## 5. 가장 큰 리스크

현재 리스크는 세 가지입니다.

### 5.1 exporter coverage 리스크

main menu 이후 gameplay 고가치 화면이 아직 충분히 실증되지 않았습니다.

### 5.2 knowledge noise 리스크

catalog 전체 수치는 크지만, 그중 상당수는 여전히 assembly/pck seed입니다.
즉 “카운트가 크다”와 “바로 AI에 써도 된다”는 같은 의미가 아닙니다.

### 5.3 locale/fallback 리스크

카드 본문은 한국어 우선으로 많이 복구됐지만, 영어 fallback은 아직 보수적으로 막아 두거나 버릴 때가 있습니다.
이 부분은 정식 locale/file 경계 복구가 필요하면 후속 PCK reader 단계로 넘어가야 합니다.

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

현재 저장소는 “main menu까지 살아 있는 exporter + 카드 본문까지 들어오기 시작한 정적 지식 파이프라인 + 아직 gameplay smoke가 남은 외부 AI 조언 앱 뼈대” 상태입니다.

## 2026-03-11 추가 상태 업데이트

정적 지식 파이프라인은 이제 `카드 설명 본문만 복구된 초기 상태`를 넘었습니다.

최신 `inspect-static-knowledge` 기준:

- localization cards: 572 / descriptions: 538
- localization relics: 291 / descriptions: 289
- localization potions: 75 / descriptions: 70
- localization events: 197 / descriptions: 95 / options: 203
- localization shops: 21 / descriptions: 18
- localization rewards: 3 / descriptions: 2
- localization keywords: 270 / descriptions: 263

assistant 전용 export도 생성됩니다.

- `artifacts/knowledge/catalog.assistant.json`
- `artifacts/knowledge/catalog.assistant.txt`
- `artifacts/knowledge/assistant/index.json`
- `artifacts/knowledge/assistant/cards.json`
- `artifacts/knowledge/assistant/relics.json`
- `artifacts/knowledge/assistant/events.json`
- `artifacts/knowledge/assistant/shops.json`
- `artifacts/knowledge/assistant/keywords.json`

`assistant/index.json`에는 release metadata, primary locale, provenance, external cross-check hint(`https://spire-codex.com/`)도 함께 남겨 둡니다.

따라서 현재 상태를 한 줄로 요약하면:

- exporter는 아직 gameplay high-value screen smoke가 더 필요하지만,
- 정적 지식 쪽은 카드/유물/포션/이벤트/상점/키워드 동작 설명을 AI가 읽을 수 있는 형태로 빠르게 확장되고 있는 상태입니다.
