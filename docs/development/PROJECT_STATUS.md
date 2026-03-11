# 프로젝트 상태

기준 시점:

- 날짜: `2026-03-11`
- 게임 버전: `STS2 v0.98.3`
- 기준 추출 체인: `Godot 4.5.1`, `.NET 7`, `ilspycmd 8.2`

이 문서는 “지금 실제로 어디까지 구현되고 검증됐는가”를 한 번에 보는 상태 문서입니다.

## 1. 현재 저장소의 중심

현재 저장소의 중심 기능은 아래 다섯 가지입니다.

1. 게임 내부 `read-only native exporter`
2. live export 파일 4종
3. strict parser 기반 정적 지식 추출
4. 외부 `Host + Codex backend`
5. 사용자용 `WPF 조언 앱`

즉 목표는 “게임을 대신 플레이하는 모드”가 아니라 “플레이 중인 사람에게 게임 밖에서 조언을 주는 외부 어시스턴트”입니다.

## 2. 현재 검증된 것

### 2.1 빌드와 기본 검증

현재 기준으로 아래는 통과 상태입니다.

- `dotnet build STS2_Mod_AI_Companion.sln`
- `dotnet run --project src\Sts2ModKit.SelfTest --no-build`
- `dotnet run --project src\Sts2ModKit.Tool --no-build -- extract-static-knowledge`
- `dotnet run --project src\Sts2ModKit.Tool --no-build -- inspect-static-knowledge`

### 2.2 runtime exporter

실제로 확인된 범위:

- 모드 로드
- Harmony startup failure 제거
- main menu까지 live export 4종 생성
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- scene polling 기반 상태 갱신

아직 gameplay smoke로 닫지 못한 범위:

- reward
- event
- shop
- rest
- combat start
- turn start

즉 exporter는 “메인 메뉴까지 살아 있다”는 점은 확인됐지만, 고가치 gameplay 화면 coverage는 아직 미실증입니다.

### 2.3 정적 지식 추출

현재 파이프라인:

1. `release-scan`
2. `decompile-scan`
3. `assembly-scan`
4. `pck-inventory`
5. `strict-domain-parse`
6. `localization-scan`
7. `observed-merge`
8. `catalog-build`

현재 canonical counts:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `5`
- rewards: `7`
- keywords: `262`

현재 해석:

- `cards/relics/potions/events/shops/rewards/keywords`는 모두 strict parser 기반 canonical입니다.
- `shops`는 상점의 실제 판매 결과가 아니라 `상점`, `카드 제거 서비스`, `카드/포션/유물 판매 슬롯` 같은 의미 단위를 정리한 값입니다.
- `rewards`는 보상 시스템의 의미 단위만 strict semantic entity로 정리한 값입니다.
- `keywords`는 파워, 의도, 카드 키워드를 strict semantic entry로 정리한 값입니다.

현재 localization coverage:

- cards: `582` / descriptions: `569` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `4` / descriptions: `4`
- rewards: `3` / descriptions: `2`
- keywords: `245` / descriptions: `245`

AI가 우선 읽는 산출물:

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

`KnowledgeCatalogService`는 `catalog.assistant.json`을 우선 로드하고, 없을 때만 `catalog.latest.json`으로 내려갑니다.

### 2.4 Host / WPF

코드 기준으로 존재하고 빌드되는 것:

- `src/Sts2AiCompanion.Host`
- `src/Sts2AiCompanion.Wpf`

현재 코드에서 확인된 역할:

- `CompanionHost`가 live export polling loop 수행
- `KnowledgeCatalogService`가 assistant catalog reload와 knowledge slice 선택 수행
- `AdvicePromptBuilder`가 `AdviceInputPack`과 markdown advice 생성
- `CodexCliClient`가 Codex 세션 create/resume 실행
- WPF가 현재 상태, current screen, choices, recent events, knowledge entries, latest advice 표시
- WPF 버튼:
  - `Analyze Now`
  - `Pause/Resume Auto Advice`
  - `Retry Last`
  - `Refresh Knowledge`
  - `Open Artifacts`

아직 gameplay로 닫지 못한 것:

- actual gameplay live export와 연결된 auto advice
- `Analyze Now` / `Retry Last`의 gameplay end-to-end
- run 종료 summary와 artifact 보존의 실제 흐름

## 3. 지금 사용자 관점에서 가능한 것

현재 상태를 사용자 시나리오로 요약하면:

- 게임을 켜면 mod/exporter가 main menu까지 live export를 만든다.
- 외부 Host/WPF 구조는 코드로 존재하고 빌드된다.
- 정적 지식 카탈로그는 카드/유물/포션/이벤트의 실제 설명까지 상당 부분 포함한다.
- assistant export는 AI가 직접 읽기 쉬운 구조로 생성된다.

하지만 아래는 아직 “코드가 준비됐다” 수준이지 gameplay로 완전히 닫히지 않았습니다.

- reward/event/shop/rest/combat 자동 advice
- gameplay 중 `currentChoices` 기반 조언
- WPF가 실제 live gameplay에 붙어 안정적으로 조언을 보여주는 흐름

## 4. 최우선 다음 작업

우선순위:

1. high-value gameplay smoke 확대
2. `currentChoices` 실증
3. WPF + Host + Codex의 실제 gameplay 연결 검증
4. `shops/rewards/keywords`를 gameplay 관찰값과 교차 검증
5. replay 기반 무인 acceptance harness

## 5. 현재 리스크

### 5.1 exporter coverage 리스크

main menu 이후 gameplay 고가치 화면은 아직 충분히 실증되지 않았습니다.

### 5.2 knowledge coverage 리스크

`shops/rewards/keywords`는 strict semantic canonical로 정리됐지만, 실제 런에서 어떤 상품/보상 조합과 선택지가 뜨는지는 gameplay 관찰이 더 필요합니다. 즉 노이즈보다 coverage와 runtime 교차 검증이 남은 과제입니다.

### 5.3 description resolver 리스크

일부 카드/유물 설명에는 SmartFormat placeholder가 완전히 풀리지 않고 남습니다.

### 5.4 end-to-end 리스크

Host/WPF/Codex는 코드로 연결돼 있지만, 실제 gameplay run에서 자동 조언까지 닫힌 상태는 아닙니다.

## 6. 다음 검증 순서

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

## 7. 한 줄 요약

현재 저장소는 `main menu까지 살아 있는 exporter + strict parser 기반 정적 지식 파이프라인 + 외부 Host/WPF 조언 앱 뼈대`까지는 확보한 상태이고, 다음 핵심은 `실제 gameplay high-value 화면과 AI 조언 end-to-end를 실증하는 것`입니다.
