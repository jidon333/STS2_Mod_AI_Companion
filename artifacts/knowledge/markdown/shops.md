# 상점

- 전체 항목 수: 7
- 설명 본문이 채워진 항목: 5
- L10N 키 또는 제목이 연결된 항목: 2
- 선택지/옵션 정보가 있는 항목: 2

## 이 섹션이 도와주는 플레이 장면

- 상인 방 진입
- 구매 후보 표시
- 카드 제거 서비스

## 현재 이 섹션에서 확인된 것

- 상점 UI 문구
- 카드 제거 서비스 설명
- 상인 관련 대사 힌트

## 아직 남은 점

- 실제 상품 풀
- 런별 상품 배치 결과

## 주요 L10N/리소스 힌트

- `localization/kor/merchant_room.json`
- `localization/eng/merchant_room.json`

## 항목 목록

### 상점

- ID: `megacrit-sts2-core-rooms-merchantroom`
- 그룹/풀 추정: 상점/UI, merchant-room
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 방에서는 신비로운 상인이 자신의 물건들을 판매합니다.

열심히 모은 [gold]골드[/gold]를 여기에서 사용해보세요!
- 구조 정보:
  - 상점 항목 유형: merchant-room
  - 방 유형: Shop
  - 대사 prefix: MERCHANT.talk.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROOM_MERCHANT`
- 모델 클래스: `MegaCrit.Sts2.Core.Rooms.MerchantRoom`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유물 판매 슬롯

- ID: `megacrit-sts2-core-entities-merchant-merchantrelicentry`
- 그룹/풀 추정: 상점/UI, merchant-relic-entry
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 상점 유물 슬롯입니다. 일반/희귀 유물 2개와 상점 전용 유물 1개를 채웁니다.
- 구조 정보:
  - 상점 항목 유형: merchant-relic-entry
  - 가격 규칙: Relic merchant cost x merchant RNG 0.85-1.15.
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Merchant.MerchantRelicEntry`

### 카드 제거 서비스

- ID: `megacrit-sts2-core-entities-merchant-merchantcardremovalentry`
- 그룹/풀 추정: 상점/UI, merchant-card-removal-service
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 덱에서 카드를 1장 제거합니다. 남은 도전 동안 이 서비스의 비용이 [blue]{Amount}[/blue] 증가합니다.
- 구조 정보:
  - 상점 항목 유형: merchant-card-removal-service
  - 가격 규칙: Base 75, +25 per prior card removal in the run.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MERCHANT`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Merchant.MerchantCardRemovalEntry`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 카드 판매 슬롯

- ID: `megacrit-sts2-core-entities-merchant-merchantcardentry`
- 그룹/풀 추정: 상점/UI, merchant-card-entry
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 상점에서 카드 구매 슬롯을 담당합니다. 캐릭터 카드 5장과 무색 카드 2장을 채우고, 할인 상품 한 칸을 반값으로 표시합니다.
- 구조 정보:
  - 상점 항목 유형: merchant-card-entry
  - 가격 규칙: Common 50 / Uncommon 75 / Rare 150, colorless x1.15, sale 50% off, final merchant RNG 0.95-1.05.
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Merchant.MerchantCardEntry`

### 포션 판매 슬롯

- ID: `megacrit-sts2-core-entities-merchant-merchantpotionentry`
- 그룹/풀 추정: 상점/UI, merchant-potion-entry
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 상점 포션 슬롯입니다. 전투 외 무작위 포션 3개를 배치합니다.
- 구조 정보:
  - 상점 항목 유형: merchant-potion-entry
  - 가격 규칙: Common 50 / Uncommon 75 / Rare 100, final merchant RNG 0.95-1.05.
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Merchant.MerchantPotionEntry`

### Observed Shop

- ID: `shop-0ba1d9f30d29`
- 그룹/풀 추정: 상점/UI, 구매 후보 확인됨
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. MerchantPotion, MerchantPotion2, MerchantPotion3, 도박꾼의 영액
- 확인된 선택지:
  - MerchantPotion
  - MerchantPotion2
  - MerchantPotion3
  - 도박꾼의 영액
  - 민첩 포션
  - 병사의 스튜
  - 숫돌
  - 용과
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Shop

- ID: `shop-b103cdee268d`
- 그룹/풀 추정: 상점/UI, 구매 후보 확인됨
- 플레이 중 참조 시점: 상점 진입 후 카드/서비스 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. MerchantCardHolder5, MerchantPotion, MerchantPotion2, 도박꾼의 영액
- 확인된 선택지:
  - MerchantCardHolder5
  - MerchantPotion
  - MerchantPotion2
  - 도박꾼의 영액
  - 민첩 포션
  - 병사의 스튜
  - 숫돌
  - 용과
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

