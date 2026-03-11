# 정적 지식 추출

이 문서는 `extract-static-knowledge`가 현재 무엇을 읽고, 어떤 산출물을 만들며, 어디까지 신뢰할 수 있는지를 설명합니다.

## 1. 왜 정적 지식이 필요한가

실시간 exporter만으로는 다음 정보가 부족합니다.

- 현재 보이는 카드/유물/포션/이벤트/상점 문구의 의미
- 선택지에 붙어 있는 설명 본문과 키워드 의미
- 현재 화면에서 AI가 참조해야 할 배경 지식

그래서 AI 조언은 두 층을 같이 씁니다.

- 실시간 상태: 현재 화면, 현재 선택지, 현재 덱/유물/포션, 최근 변화
- 정적 지식: 카드 설명, 유물 설명, 이벤트 페이지/옵션 문장, 상점 서비스 문구, 키워드 의미

## 2. 현재 파이프라인

`extract-static-knowledge`는 현재 아래 순서로 동작합니다.

1. `release-scan`
2. `assembly-scan`
3. `pck-inventory`
4. `localization-scan`
5. `observed-merge`
6. `catalog-build`

### 2.1 release-scan

입력:

- `release_info.json`
- Godot atlas 통계
- 현재 live artifact 존재 여부

역할:

- 게임 버전/커밋 기록
- 추출 시점 메타데이터 기록
- 이후 catalog provenance의 기준점 제공

### 2.2 assembly-scan

입력:

- `data_sts2_windows_x86_64/sts2.dll`
- 인접 managed DLL

역할:

- `MetadataLoadContext`로 managed metadata를 읽습니다.
- 카드/유물/포션/이벤트/상점/보상/키워드 관련 타입 후보를 모읍니다.
- 모델 클래스명, 네임스페이스, 멤버 seed를 만듭니다.

중요:

- 이 단계는 `구조 후보`를 만드는 단계입니다.
- 설명 본문이 없는 seed가 많이 남는 것은 정상입니다.

### 2.3 pck-inventory

입력:

- `SlayTheSpire2.pck`

역할:

- PCK 바이너리에서 리소스 경로 문자열을 공격적으로 스캔합니다.
- `res://...`, `localization/...`, `images/...`, `src/...` 흔적을 수집합니다.
- 카드 초상화, 모델 소스 경로, localization 파일 힌트를 seed로 남깁니다.

중요:

- 이 단계는 아직 `정식 PCK 완전 언팩`이 아닙니다.
- 대신 빠르게 리소스 지도를 만드는 역할입니다.

### 2.4 localization-scan

현재 지식 품질을 끌어올리는 핵심 단계입니다.

입력:

- `SlayTheSpire2.pck`
- 앞 단계에서 모은 seed catalog

역할:

- PCK 전체를 UTF-8 스트리밍으로 읽습니다.
- localization key/value 패턴을 직접 복구합니다.
- 현재는 카드만이 아니라 아래 도메인을 함께 수집합니다.
  - cards
  - relics
  - potions
  - events
  - shops
  - rewards
  - keywords

현재 복구하는 대표 필드:

- `*.title`
- `*.description`
- `*.selectionScreenPrompt`
- 이벤트 `pages.*.description`
- 이벤트 `pages.*.options.*.title/description`
- 일부 상점/보상/키워드 문구

현재 최신 inspect 기준 localization coverage:

- cards: `572` / descriptions: `538` / selection prompts: `35`
- relics: `291` / descriptions: `289`
- potions: `75` / descriptions: `70`
- events: `197` / descriptions: `95` / options: `203`
- shops: `21` / descriptions: `18`
- rewards: `3` / descriptions: `2`
- keywords: `270` / descriptions: `263`

한계:

- locale 판별은 휴리스틱이 섞여 있습니다.
- 영어 fallback은 아직 보수적으로 처리합니다.
- 일부 domain은 카드만큼 깊게 정규화되지 않았습니다.

### 2.5 observed-merge

입력:

- `events.ndjson`
- `state.latest.json`

역할:

- 실제 플레이에서 본 카드/유물/포션/이벤트/상점/보상 선택지를 병합합니다.
- offline 후보와 gameplay 관찰을 연결합니다.

중요:

- 이 단계의 품질은 high-value gameplay smoke coverage에 강하게 의존합니다.
- reward/event/shop/rest/combat가 아직 충분히 실증되지 않으면 observed 데이터는 비어 있을 수 있습니다.

### 2.6 catalog-build

최종 산출물:

- `artifacts/knowledge/catalog.latest.json`
- `artifacts/knowledge/catalog.latest.txt`
- `artifacts/knowledge/source-manifest.json`

중간 산출물:

- `artifacts/knowledge/assembly-scan.json`
- `artifacts/knowledge/pck-inventory.json`
- `artifacts/knowledge/localization-scan.json`
- `artifacts/knowledge/observed-merge.json`

사람이 읽는 산출물:

- `artifacts/knowledge/markdown/README.md`
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
- `artifacts/knowledge/markdown/cards.md`
- `artifacts/knowledge/markdown/relics.md`
- `artifacts/knowledge/markdown/potions.md`
- `artifacts/knowledge/markdown/events.md`
- `artifacts/knowledge/markdown/shops.md`
- `artifacts/knowledge/markdown/rewards.md`
- `artifacts/knowledge/markdown/keywords.md`

AI가 읽는 산출물:

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

`KnowledgeCatalogService`는 현재 `catalog.assistant.json`을 우선 로드합니다.

## 3. 현재 어떤 정보가 실질적으로 들어오나

현재 카드 엔트리는 아래 정보를 가질 수 있습니다.

- 표시 이름
- 설명 본문
- 선택 화면 프롬프트
- localization key stem
- preferred locale
- 모델 클래스
- 카드 초상화/리소스 경로
- observed 여부

현재 다른 domain도 같은 방향으로 확장되고 있습니다.

- 유물: 제목 + 설명
- 포션: 제목 + 설명
- 이벤트: 제목 + 본문 + 페이지 + 옵션
- 상점: 서비스 문구
- 보상: 일부 설명
- 키워드: 제목 + 설명

즉 현재 파이프라인은 더 이상 `카드만 설명이 있는 상태`가 아닙니다. 카드는 가장 풍부하지만, 유물/포션/이벤트/상점/키워드도 실제 설명 본문을 갖기 시작한 상태입니다.

## 4. 신뢰도 해석 원칙

이 파이프라인이 만들어 내는 지식은 세 등급으로 봅니다.

- 강한 신뢰: `localization-scan`으로 본문이 채워지고, gameplay 관찰과도 맞아떨어지는 항목
- 중간 신뢰: assembly/pck 경로와 모델 클래스는 있으나 본문은 아직 불완전한 항목
- 낮은 신뢰: 이름/경로 seed만 있고 localization 본문과 gameplay 관찰이 아직 없는 항목

AI 조언은 앞으로 이 등급을 같이 참고해야 합니다.

## 5. 현재 남아 있는 한계

- 전체 inventory count는 여전히 seed/noise가 큽니다.
- locale/fallback 품질은 아직 완전히 안정화되지 않았습니다.
- event/shop/reward 정규화는 카드만큼 깊지 않습니다.
- `observed-merge`는 gameplay smoke가 늘어야 더 의미 있어집니다.

## 6. 다음 확장 방향

우선순위는 아래 순서입니다.

1. relic / potion / event / shop 정규화 보강
2. gameplay에서 관찰된 선택지와 offline catalog를 더 강하게 매칭
3. locale/file 경계를 더 정확히 복구
4. 필요하면 PCK file-entry reader를 추가

## 7. 외부 참고 원칙

외부 참고 힌트는 `https://spire-codex.com/` 구조를 참고하되, canonical truth는 현재 저장소가 게임 설치본에서 직접 뽑은 JSON/markdown/assistant export입니다.

- `Spire Codex` 참고 내용과 현재 저장소에 반영한 범위는 `SPIRE_CODEX_REFERENCE.md`에 따로 정리합니다.
- 외부 사이트는 교차검증 힌트이지 source of truth가 아닙니다.
