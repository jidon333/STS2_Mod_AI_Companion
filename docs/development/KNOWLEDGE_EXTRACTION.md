# 정적 지식 추출

이 문서는 `extract-static-knowledge`가 무엇을 읽고 어떤 중간 산출물과 최종 산출물을 만드는지 설명합니다.

핵심 원칙은 두 가지입니다.

- raw/intermediate는 넓게 수집한다.
- 사람이 읽는 canonical과 AI가 읽는 assistant export는 엄격하게 정규화한다.

## 1. 왜 정적 지식이 필요한가

runtime exporter만으로는 아래 정보가 부족합니다.

- 카드, 유물, 포션, 이벤트의 설명 본문
- 이벤트 페이지와 옵션 문장
- 상점/보상/키워드의 배경 지식
- 현재 선택지와 관련된 사전 지식

그래서 AI 조언은 아래 두 층을 함께 사용합니다.

- 실시간 상태: 현재 화면, 현재 선택지, 덱, 유물, 포션, 최근 변화
- 정적 지식: 카드/유물/포션/이벤트/키워드 설명과 구조 정보

## 2. 현재 파이프라인

현재 `extract-static-knowledge`는 아래 순서로 동작합니다.

1. `release-scan`
2. `decompile-scan`
3. `assembly-scan`
4. `pck-inventory`
5. `strict-domain-parse`
6. `localization-scan`
7. `observed-merge`
8. `catalog-build`

### 2.1 release-scan

입력:

- `release_info.json`
- atlas 통계
- 현재 live artifact 존재 여부

역할:

- 게임 버전, 커밋, 추출 시점 메타데이터 기록
- 이후 모든 산출물 provenance의 기준 제공

### 2.2 decompile-scan

입력:

- `data_sts2_windows_x86_64/sts2.dll`

역할:

- `ilspycmd`로 `sts2.dll`을 C# 소스로 디컴파일
- `artifacts/knowledge/decompiled/`에 캐시
- 이후 strict parser가 실제 `Models/*` 소스를 읽을 수 있게 준비

현재 목적:

- `substring match` 대신 실제 모델 클래스 기반 파싱으로 넘어가기 위한 기반

주요 산출물:

- `artifacts/knowledge/decompile-scan.json`
- `artifacts/knowledge/decompiled/`

### 2.3 assembly-scan

입력:

- `sts2.dll`
- 인접 managed DLL

역할:

- `MetadataLoadContext`로 메타데이터를 읽어 넓은 후보군을 수집
- 타입명, 네임스페이스, 멤버명, enum seed를 모음

중요:

- 이 단계는 canonical 분류기가 아닙니다.
- raw candidate를 모으는 단계입니다.

주요 산출물:

- `artifacts/knowledge/assembly-scan.json`

### 2.4 pck-inventory

입력:

- `SlayTheSpire2.pck`

역할:

- PCK 전체를 완전 언팩하지 않고 문자열과 리소스 경로 흔적을 수집
- portrait path, resource path, localization file hint를 넓게 모음

중요:

- 이 단계도 canonical 분류기가 아닙니다.
- 여전히 raw/intermediate 계층입니다.

주요 산출물:

- `artifacts/knowledge/pck-inventory.json`

### 2.5 strict-domain-parse

입력:

- `artifacts/knowledge/decompiled/`
- raw resource path map

역할:

- 실제 디컴파일된 모델 소스 기준으로 canonical seed 생성
- 아래 도메인을 엄격 분류
  - cards
  - relics
  - potions
  - events
  - shops
  - rewards
  - keywords

기준 디렉터리:

- `MegaCrit.Sts2.Core.Models.Cards`
- `MegaCrit.Sts2.Core.Models.CardPools`
- `MegaCrit.Sts2.Core.Models.Relics`
- `MegaCrit.Sts2.Core.Models.RelicPools`
- `MegaCrit.Sts2.Core.Models.Potions`
- `MegaCrit.Sts2.Core.Models.Events`
- `MegaCrit.Sts2.Core.Entities.Merchant`
- `MegaCrit.Sts2.Core.Rewards`
- `MegaCrit.Sts2.Core.Models.Powers`
- `MegaCrit.Sts2.Core.MonsterMoves.Intents`
- `MegaCrit.Sts2.Core.Entities.Cards.CardKeyword`

이 단계에서 하는 일:

- 카드 `base(cost, CardType, CardRarity, TargetType)` 읽기
- 카드 pool/color 추정
- 카드 동적 변수와 업그레이드 요약 추출
- 유물 rarity/pool 추출
- 포션 rarity/usage/target 추출
- 이벤트 page/option key 추출
- 상점 서비스/판매 슬롯 의미 단위 추출
- 보상 종류/연결 보상 세트 의미 단위 추출
- 키워드용 파워/의도/카드 키워드 의미 단위 추출

중요:

- `OPTION_*`, `*_BUTTON`, `*_POWER`, compiler-generated 타입은 cards canonical에 들어가지 않습니다.
- 즉 이 단계가 noisy broad scan을 canonical 데이터로 바꾸는 핵심입니다.

주요 산출물:

- `artifacts/knowledge/strict-domain-scan.json`

현재 최신 `inspect-static-knowledge` 기준 strict counts:

- cards: `576`
- relics: `288`
- potions: `63`
- events: `58`
- shops: `5`
- rewards: `7`
- keywords: `262`

### 2.6 localization-scan

입력:

- `SlayTheSpire2.pck`
- strict seed set

역할:

- PCK에서 localization key/value를 직접 복구
- strict parser가 만든 canonical 엔트리에 본문을 병합

현재 복구 대상:

- `*.title`
- `*.description`
- `*.selectionScreenPrompt`
- 이벤트 `pages.*.description`
- 이벤트 `pages.*.options.*.title/description`
- 키워드, 상점, 보상 계열 일부 문구

중요:

- 카드 L10N은 strict card seed에만 붙습니다.
- 즉 `OPTION_HEAL` 같은 일반 문구가 더 이상 cards canonical을 오염시키지 않습니다.

현재 최신 localization coverage:

- cards: `582` / descriptions: `569` / selection prompts: `22`
- relics: `285` / descriptions: `285`
- potions: `75` / descriptions: `70`
- events: `153` / descriptions: `60` / options: `203`
- shops: `4` / descriptions: `4`
- rewards: `3` / descriptions: `2`
- keywords: `245` / descriptions: `245`

주요 산출물:

- `artifacts/knowledge/localization-scan.json`

### 2.7 observed-merge

입력:

- `events.ndjson`
- `state.latest.json`

역할:

- 실제 gameplay에서 관찰한 카드, 유물, 포션, 이벤트, 상점, 보상 선택지를 offline catalog에 병합
- runtime과 offline 지식을 연결

중요:

- 현재는 gameplay high-value smoke가 아직 부족해서 `observed-merge`가 비어 있는 경우가 많습니다.
- 이 단계는 exporter coverage가 넓어질수록 중요해집니다.

주요 산출물:

- `artifacts/knowledge/observed-merge.json`

### 2.8 catalog-build

역할:

- raw/intermediate, strict parser, localization, observed merge 결과를 합쳐 최종 산출물 생성

canonical 산출물:

- `artifacts/knowledge/catalog.latest.json`
- `artifacts/knowledge/catalog.latest.txt`
- `artifacts/knowledge/source-manifest.json`

assistant 산출물:

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

사람용 산출물:

- `artifacts/knowledge/markdown/README.md`
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
- `artifacts/knowledge/markdown/cards.md`
- `artifacts/knowledge/markdown/relics.md`
- `artifacts/knowledge/markdown/potions.md`
- `artifacts/knowledge/markdown/events.md`
- `artifacts/knowledge/markdown/shops.md`
- `artifacts/knowledge/markdown/rewards.md`
- `artifacts/knowledge/markdown/keywords.md`

## 3. 현재 canonical과 raw의 구분

### canonical로 신뢰해도 되는 쪽

- cards
- relics
- potions
- events
- shops
- rewards
- keywords

이 도메인들은 모두 strict-domain-parse를 seed로 쓰므로, 실제 모델 클래스나 의미 단위가 확인된 엔트리만 들어갑니다.

다만 해석은 도메인별로 다릅니다.

- `cards/relics/potions/events`는 실제 모델 클래스 기반 canonical 엔트리입니다.
- `shops`는 상점 시스템의 의미 단위입니다.
  - `상점`
  - `카드 제거 서비스`
  - `카드/포션/유물 판매 슬롯`
- `rewards`는 보상 시스템의 의미 단위입니다.
  - `카드 보상`
  - `골드 보상`
  - `포션 보상`
  - `유물 보상`
  - `카드 제거 보상`
  - `특수 카드 보상`
  - `연결 보상 세트`
- `keywords`는 파워, 의도, 카드 키워드를 strict semantic entry로 정리한 결과입니다.

## 4. AI는 무엇을 읽는가

`KnowledgeCatalogService`는 현재 `catalog.assistant.json`을 우선 로드합니다.

assistant export에는 아래처럼 AI 조언에 직접 필요한 필드가 들어갑니다.

- `className`
- `classId`
- `strictDomain`
- `strictModel`
- `pool`
- `color`
- `cost`
- `type`
- `rarity`
- `target`
- `usage`
- `dynamicVars`
- `upgradeSummary`

즉 AI는 raw candidate가 아니라, 더 작은 assistant 전용 지식 뷰를 읽습니다.

## 5. 현재 한계

- `shops/rewards`는 strict semantic canonical이지만, 실제 런마다 달라지는 상품/보상 조합까지 표현하지는 않습니다.
- `keywords`는 strict semantic canonical이지만, 일부는 localization 본문 대신 summary 기반 설명을 사용합니다.
- 카드 설명에는 일부 unresolved placeholder가 남습니다.
  - 예: `{PlayMax:diff()}`
- `observed-merge`는 gameplay smoke coverage가 넓어져야 의미 있게 채워집니다.

## 6. 다음 단계

우선순위:

1. SmartFormat/description resolver 보강
2. gameplay에서 관찰한 `currentChoices`와 canonical catalog 교차검증
3. high-value gameplay smoke 확대
4. `shops/rewards/keywords`를 runtime 관찰값과 계속 맞추기
