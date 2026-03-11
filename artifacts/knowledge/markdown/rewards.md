# 보상

- 전체 항목 수: 7
- 설명 본문이 채워진 항목: 7
- L10N 키 또는 제목이 연결된 항목: 7
- 선택지/옵션 정보가 있는 항목: 0

## 이 섹션이 도와주는 플레이 장면

- 전투 보상 화면
- 이벤트 보상 화면
- 카드 선택 UI

## 현재 이 섹션에서 확인된 것

- 보상 UI 문구
- 선택 화면 리소스 힌트

## 아직 남은 점

- 실제 보상 풀 구성
- 관찰 기반 보강

## 주요 L10N/리소스 힌트

- `localization/kor/card_reward_ui.json`
- `localization/eng/card_reward_ui.json`
- `localization/kor/card_selection.json`

## 항목 목록

### 골드 보상

- ID: `megacrit-sts2-core-rewards-goldreward`
- 그룹/풀 추정: 보상/UI, Gold
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 골드 보상을 제시합니다. 훔쳐간 골드를 되찾는 변형도 포함됩니다.
- 구조 정보:
  - 보상 유형: Gold
  - 보상 세트 인덱스: 1
  - 매칭 키: COMBAT_REWARD_GOLD | COMBAT_REWARD_GOLD_STOLEN
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- L10N key: `COMBAT_REWARD_GOLD`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.GoldReward`
- 리소스 경로: `ui/reward_screen/reward_icon_money.png`

### 연결된 보상

- ID: `megacrit-sts2-core-rewards-linkedrewardset`
- 그룹/풀 추정: 보상/UI, LinkedSet
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 세트에서 [blue]1[/blue]개의 보상만을 선택할 수 있습니다.
- 구조 정보:
  - 보상 유형: LinkedSet
  - 매칭 키: LINKED_REWARDS
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LINKED_REWARDS`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.LinkedRewardSet`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유물 보상

- ID: `megacrit-sts2-core-rewards-relicreward`
- 그룹/풀 추정: 보상/UI, Relic
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 유물 보상을 제시합니다.
- 구조 정보:
  - 보상 유형: Relic
  - 보상 세트 인덱스: 3
  - 매칭 키: RELIC_REWARD
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- L10N key: `RELIC_REWARD`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.RelicReward`

### 카드 보상

- ID: `megacrit-sts2-core-rewards-cardreward`
- 그룹/풀 추정: 보상/UI, Card
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 카드 [blue]3[/blue]장으로 구성된 팩입니다. 카드를 [blue]1[/blue]장 선택해 [gold]덱[/gold]에 추가할 수 있습니다.
- 구조 정보:
  - 보상 유형: Card
  - 보상 세트 인덱스: 5
  - 매칭 키: COMBAT_REWARD_ADD_CARD | CARD_REWARD
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CARD_REWARD`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.CardReward`
- 리소스 경로: `ui/reward_screen/reward_icon_rare.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 카드 제거 보상

- ID: `megacrit-sts2-core-rewards-cardremovalreward`
- 그룹/풀 추정: 보상/UI, RemoveCard
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 보상 화면에서 카드 제거 서비스를 제시합니다.
- 구조 정보:
  - 보상 유형: RemoveCard
  - 보상 세트 인덱스: 7
  - 매칭 키: COMBAT_REWARD_CARD_REMOVAL
- 선택 프롬프트: 덱에서 제거할 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COMBAT_REWARD_CARD_REMOVAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.CardRemovalReward`
- 리소스 경로: `ui/reward_screen/reward_icon_card_removal.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 특수 카드 보상

- ID: `megacrit-sts2-core-rewards-specialcardreward`
- 그룹/풀 추정: 보상/UI, SpecialCard
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 특수 카드 단일 보상을 제시합니다.
- 구조 정보:
  - 보상 유형: SpecialCard
  - 보상 세트 인덱스: 4
  - 매칭 키: COMBAT_REWARD_ADD_SPECIAL_CARD | SPECIAL_CARD_REWARD
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- L10N key: `COMBAT_REWARD_ADD_SPECIAL_CARD`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.SpecialCardReward`
- 리소스 경로: `ui/reward_screen/reward_icon_special_card.png`

### 포션 보상

- ID: `megacrit-sts2-core-rewards-potionreward`
- 그룹/풀 추정: 보상/UI, Potion
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 포션 보상을 제시합니다.
- 구조 정보:
  - 보상 유형: Potion
  - 보상 세트 인덱스: 2
  - 매칭 키: POTION_REWARD
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- L10N key: `POTION_REWARD`
- 모델 클래스: `MegaCrit.Sts2.Core.Rewards.PotionReward`

