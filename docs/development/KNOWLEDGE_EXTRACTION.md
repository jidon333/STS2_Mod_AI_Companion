# 정적 지식 추출

이 문서는 `extract-static-knowledge`가 지금 실제로 무엇을 읽고, 어떤 산출물을 만들며, 어디까지 신뢰할 수 있는지를 설명합니다.

## 1. 왜 정적 지식이 필요한가

실시간 exporter만으로는 다음 정보가 비어 있습니다.

- 현재 보이는 카드나 유물의 이름은 알 수 있어도 능력 문장을 모를 수 있음
- 이벤트 선택지가 보이더라도 그 선택지가 어떤 문맥에서 나왔는지 알기 어려움
- 상점 화면에서 상품 이름만 알고 효과나 관련 키워드를 모를 수 있음

그래서 AI 조언은 두 층을 같이 써야 합니다.

- 실시간 상태: 현재 화면, 현재 선택지, 현재 덱/유물/포션, 최근 변화
- 정적 지식: 카드 설명, 유물 설명, 이벤트 문장, 상점 관련 텍스트, 키워드 의미

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
- Godot 로그 atlas 통계
- 현재 live artifact 존재 여부

역할:

- 게임 버전과 커밋 기록
- 현재 기준 런타임 메타데이터
- 추출 파이프라인이 어느 게임 빌드를 기준으로 돌았는지 기록

### 2.2 assembly-scan

입력:

- `data_sts2_windows_x86_64/sts2.dll`
- 인접 managed DLL

역할:

- `MetadataLoadContext`로 managed metadata만 읽음
- 카드/유물/포션/이벤트/상점/보상/키워드 관련 타입 후보 수집
- 모델 클래스명, 네임스페이스, 멤버 seed를 모음

중요:

- 이 단계는 “이름과 구조 후보”를 모으는 단계입니다.
- 아직 실제 설명 본문을 보장하지는 않습니다.

### 2.3 pck-inventory

입력:

- `SlayTheSpire2.pck`

역할:

- PCK 바이너리에서 리소스 경로 문자열을 공격적으로 스캔
- `res://...`, `localization/...`, `images/...`, `src/...` 같은 경로 흔적을 수집
- 카드 초상화 경로, 모델 소스 경로, localization 파일 힌트를 seed로 남김

중요:

- 이 단계는 아직 “정식 PCK 완전 언팩”이 아닙니다.
- 대신 빠르게 리소스 지도와 파일 힌트를 얻는 쪽에 가깝습니다.

### 2.4 localization-scan

이 단계가 이번에 추가된 핵심입니다.

입력:

- `SlayTheSpire2.pck`
- 앞 단계에서 모아 둔 카드 candidate seed

역할:

- PCK 전체를 UTF-8 스트리밍으로 읽음
- localization key/value 패턴을 직접 복구
- 현재는 카드 중심으로 아래 키를 우선 수집
  - `*.title`
  - `*.description`
  - `*.selectionScreenPrompt`
- 한국어를 우선 locale로 취급하고, 다른 locale은 보조 참조로 남김

현재 판단:

- 카드 설명 본문을 채우는 데 실질적으로 가장 중요한 단계입니다.
- 실제로 `곡예`, `강타`, `전율`, `방어` 계열 카드가 설명과 함께 catalog에 들어오기 시작했습니다.

한계:

- PCK 파일 엔트리를 정식으로 해석하는 파서는 아직 아닙니다.
- locale 구분은 “문자 스크립트 + 주변 파일 힌트” 기반 휴리스틱입니다.
- 따라서 영어 fallback은 아직 보수적으로 처리하고 있으며, 일부 locale은 `cjk`, `latin`, `unknown`으로만 남길 수 있습니다.

### 2.5 observed-merge

입력:

- `events.ndjson`
- `state.latest.json`

역할:

- 실제 플레이 중 본 카드/유물/포션/이벤트/상점/보상 선택지를 병합
- offline 후보와 실제 gameplay 관찰을 이어 주는 단계

중요:

- 이 단계의 품질은 live smoke coverage에 강하게 의존합니다.
- 아직 reward/event/shop/rest/combat coverage가 충분하지 않으면 observed 데이터는 빈 상태일 수 있습니다.

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

사람이 읽는 리포트:

- `artifacts/knowledge/markdown/README.md`
- `artifacts/knowledge/markdown/PLAY_GUIDE.md`
- `artifacts/knowledge/markdown/cards.md`
- 그 외 `relics.md`, `potions.md`, `events.md`, `shops.md`, `rewards.md`, `keywords.md`

## 3. 카드 정보는 지금 어디까지 채워지나

이번 단계 이후 카드 엔트리는 아래 정보를 가질 수 있습니다.

- 카드 표시 이름
- 카드 설명 본문
- 선택 화면 프롬프트
- localization key stem
- preferred locale
- 모델 클래스
- 카드 초상화/리소스 경로
- 관찰 여부

대표 예시:

- `MegaCrit.Sts2.Core.Models.Cards.Acrobatics`
  - 표시명: `곡예`
  - 설명: 실제 한국어 카드 설명 본문
  - source: `localization-scan`

즉, 이제 카드 문서는 단순한 이름 목록이 아니라 플레이 중 실제 판단에 쓸 수 있는 설명 문장을 포함하기 시작했습니다.

## 4. 아직 남아 있는 한계

- 카드 외 영역은 아직 카드만큼 깊게 본문이 채워지지 않았습니다.
- 유물, 이벤트, 상점, 포션은 여전히 candidate inventory 비중이 큽니다.
- locale 판별은 휴리스틱이라 다국어 fallback 품질은 아직 안정화가 필요합니다.
- `observed-merge`는 실제 gameplay smoke coverage가 늘어야 더 유의미해집니다.

## 5. 다음 확장 방향

우선순위는 아래 순서입니다.

1. 카드 localization 안정화
2. relic / potion localization 확장
3. events / merchant_room localization 확장
4. live gameplay에서 관찰된 선택지와 offline catalog를 더 강하게 매칭
5. 필요하면 PCK file-entry reader를 추가해 locale/file 경계를 더 정확하게 복구

## 6. 해석 원칙

이 파이프라인이 만들어 내는 지식은 세 등급으로 봐야 합니다.

- 강한 신뢰: `localization-scan`으로 본문이 채워지고, 실제 플레이 관찰과도 맞아떨어지는 항목
- 중간 신뢰: assembly/pck 경로와 모델 클래스는 있으나 본문은 아직 없는 항목
- 낮은 신뢰: 이름/경로 seed만 있고 아직 실제 gameplay나 localization 본문이 없는 항목

AI 조언은 앞으로 이 등급을 같이 참고해야 합니다.

## 2026-03-11 L10N 확장 업데이트

이번 갱신으로 `localization-scan`은 카드만이 아니라 아래 도메인까지 실제로 수집합니다.

- 유물: `AKABEKO` 같은 실제 유물 제목/설명
- 포션: `SWIFT_POTION` 같은 실제 포션 제목/설명
- 이벤트: `ABYSSAL_BATHS` 같은 이벤트 제목, 본문, 페이지 요약, 선택지 옵션
- 상점: `MERCHANT.cardRemovalService.*` 같은 상점/서비스 문구
- 키워드: `FRAIL_POWER`, `BLOCK`, `ENERGY` 같은 상태 이상/키워드/의도 문구

최신 inspect 기준 localization coverage:

- cards: 572 / descriptions: 538 / selection prompts: 35
- relics: 291 / descriptions: 289
- potions: 75 / descriptions: 70
- events: 197 / descriptions: 95 / options: 203
- shops: 21 / descriptions: 18
- rewards: 3 / descriptions: 2
- keywords: 270 / descriptions: 263

즉 현재 파이프라인은 더 이상 `카드만 설명이 있는 상태`가 아닙니다. 카드가 가장 풍부하지만, 유물/포션/이벤트/상점/키워드도 실제 설명 본문과 선택지 정보를 상당 부분 갖기 시작한 상태입니다.

AI가 바로 읽는 구조도 같이 분리되어 있습니다.

- `catalog.assistant.json`
- `catalog.assistant.txt`
- `artifacts/knowledge/assistant/*.json`

이 assistant 산출물은 한국어 우선 텍스트, 옵션 배열, 모델 클래스, 리소스 경로, source file hint, locale 정보를 함께 담아 조언 프롬프트 생성에 바로 쓰도록 구성합니다.

외부 참고 힌트는 `https://spire-codex.com/` 구조를 참고하되, canonical truth는 현재 저장소가 게임 설치본에서 직접 뽑은 JSON/markdown/assistant export입니다.
## 외부 참고 메모

- `Spire Codex` 참고 내용과 현재 저장소에 반영한 범위는 `SPIRE_CODEX_REFERENCE.md`에 따로 정리합니다.
- 핵심 원칙은 동일합니다.
  - 외부 사이트는 교차검증 힌트입니다.
  - 현재 저장소의 canonical source는 로컬 게임 설치본과 런타임 산출물입니다.
