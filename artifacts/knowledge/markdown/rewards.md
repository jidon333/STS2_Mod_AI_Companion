# 보상

- 전체 항목 수: 54
- 설명 본문이 채워진 항목: 7
- L10N 키 또는 제목이 연결된 항목: 7
- 선택지/옵션 정보가 있는 항목: 32

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

### Observed Reward

- ID: `reward-03e5b74b6b09`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. BackButton, cardHoverTipContainer, CardPreviewContainer, EventCardPreviewContainer
- 확인된 선택지:
  - BackButton
  - cardHoverTipContainer
  - CardPreviewContainer
  - EventCardPreviewContainer
  - GridCardPreviewContainer
  - HappyCultist
  - Heart
  - MessyCardPreviewContainer
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-17072ab32cc5`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 4턴 종료, @Control@1657, RewardsScreen, 불타는 혈액
- 확인된 선택지:
  - 4턴 종료
  - @Control@1657
  - RewardsScreen
  - 불타는 혈액
  - 수비
  - 완벽한 타격
  - 저주받은 진주
  - 진행
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-1a4b0d487476`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@1657, Card, NCardRewardSelectionScreen, 넘기기
- 확인된 선택지:
  - @Control@1657
  - Card
  - NCardRewardSelectionScreen
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 몸통 박치기
  - 완벽한 타격
  - 천둥
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-5f12a4dff948`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@1477, @Control@1486, Card, NCardRewardSelectionScreen
- 확인된 선택지:
  - @Control@1477
  - @Control@1486
  - Card
  - NCardRewardSelectionScreen
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 잿빛 타격
  - 진정한 끈기
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-602d1656a0ec`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 4턴 종료, @Control@1477, @Control@1657, 강타
- 확인된 선택지:
  - 4턴 종료
  - @Control@1477
  - @Control@1657
  - 강타
  - 불타는 혈액
  - 수비
  - 저주받은 진주
  - 진행
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-636a9bc7630e`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 10 골드, RewardsScreen, 덱에 추가할 카드를 선택하세요., 수비
- 확인된 선택지:
  - 10 골드
  - RewardsScreen
  - 덱에 추가할 카드를 선택하세요.
  - 수비
  - 잿빛 타격
  - 진정한 끈기
  - 타격
  - 탐욕
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-9af996138ef2`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 18 골드, 3턴 종료, RewardsScreen, 넘기기
- 확인된 선택지:
  - 18 골드
  - 3턴 종료
  - RewardsScreen
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 몸통 박치기+
  - 완벽한 타격+
  - 진행
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-a010ad87a99b`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@1657, Card, Hitbox, NCardRewardSelectionScreen
- 확인된 선택지:
  - @Control@1657
  - Card
  - Hitbox
  - NCardRewardSelectionScreen
  - 넘기기
  - 몸통 박치기
  - 완벽한 타격
  - 천둥
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-a58d8862b787`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@1477, @Control@1486, Card, Hitbox
- 확인된 선택지:
  - @Control@1477
  - @Control@1486
  - Card
  - Hitbox
  - NCardRewardSelectionScreen
  - 넘기기
  - 잿빛 타격
  - 진정한 끈기
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-c4314ef5a70c`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. BackButton, CardPreviewContainer, EventCardPreviewContainer, GridCardPreviewContainer
- 확인된 선택지:
  - BackButton
  - CardPreviewContainer
  - EventCardPreviewContainer
  - GridCardPreviewContainer
  - HappyCultist
  - Heart
  - MessyCardPreviewContainer
  - RelicInventory
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-ea78714e4001`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. BackButton, CardPreviewContainer, EventCardPreviewContainer, GridCardPreviewContainer
- 확인된 선택지:
  - BackButton: BackButton
  - CardPreviewContainer: CardPreviewContainer
  - EventCardPreviewContainer: EventCardPreviewContainer
  - GridCardPreviewContainer: GridCardPreviewContainer
  - HappyCultist: HappyCultist
  - Heart: Heart
  - MessyCardPreviewContainer: MessyCardPreviewContainer
  - RelicInventory: RelicInventory
- 관찰 로그 반영: 예
- 주요 소스: `live-snapshot`

### Observed Reward

- ID: `reward-16245c81e834`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@1914, Card, NCardRewardSelectionScreen, 기사회생
- 확인된 선택지:
  - @Control@1914
  - Card
  - NCardRewardSelectionScreen
  - 기사회생
  - 넘기기
  - 분노
  - 사혈
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-3723980c3715`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 11 골드, RewardsScreen, 덱에 추가할 카드를 선택하세요., 몸통 박치기
- 확인된 선택지:
  - 11 골드
  - RewardsScreen
  - 덱에 추가할 카드를 선택하세요.
  - 몸통 박치기
  - 수비
  - 완벽한 타격
  - 화염 포션
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-5640a45b13bf`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 10 골드, RewardsScreen, 넘기기, 덱에 추가할 카드를 선택하세요.
- 확인된 선택지:
  - 10 골드
  - RewardsScreen
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 잿빛 타격
  - 진정한 끈기
  - 파괴
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-7daf85684933`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 11 골드, RewardsScreen, 덱에 추가할 카드를 선택하세요., 몸통 박치기
- 확인된 선택지:
  - 11 골드
  - RewardsScreen
  - 덱에 추가할 카드를 선택하세요.
  - 몸통 박치기
  - 완벽한 타격
  - 천둥
  - 화염 포션
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-8e31751103a8`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 14 골드, RewardsScreen, 공격 포션, 덱에 추가할 카드를 선택하세요.
- 확인된 선택지:
  - 14 골드
  - RewardsScreen
  - 공격 포션
  - 덱에 추가할 카드를 선택하세요.
  - 잊힌 의식+
  - 잿불+
  - 천둥+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-c8344097244f`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 17 골드, Potion, RewardsScreen, 넘기기
- 확인된 선택지:
  - 17 골드
  - Potion
  - RewardsScreen
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 분노
  - 파워 포션
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-102f1c6e1cf7`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. NCardRewardSelectionScreen, RewardsScreen, 덱에 추가할 카드를 선택하세요., 잊힌 의식+
- 확인된 선택지:
  - NCardRewardSelectionScreen
  - RewardsScreen
  - 덱에 추가할 카드를 선택하세요.
  - 잊힌 의식+
  - 잿불+
  - 천둥+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-27e2ed8ed374`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. Card, NCardRewardSelectionScreen, 넘기기, 잊힌 의식+
- 확인된 선택지:
  - Card
  - NCardRewardSelectionScreen
  - 넘기기
  - 잊힌 의식+
  - 잿불+
  - 천둥+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-63aa986f20f6`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 18 골드, RewardsScreen, 기사회생, 덱에 추가할 카드를 선택하세요.
- 확인된 선택지:
  - 18 골드
  - RewardsScreen
  - 기사회생
  - 덱에 추가할 카드를 선택하세요.
  - 분노
  - 사혈
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-6b5a23f87027`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. RewardsScreen, 공격 포션, 덱에 추가할 카드를 선택하세요., 잊힌 의식+
- 확인된 선택지:
  - RewardsScreen
  - 공격 포션
  - 덱에 추가할 카드를 선택하세요.
  - 잊힌 의식+
  - 잿불+
  - 천둥+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-92b10214aa8e`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. NCardRewardSelectionScreen, RewardsScreen, 기사회생, 덱에 추가할 카드를 선택하세요.
- 확인된 선택지:
  - NCardRewardSelectionScreen
  - RewardsScreen
  - 기사회생
  - 덱에 추가할 카드를 선택하세요.
  - 분노
  - 사혈
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-d61460b8e1e7`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 4턴 종료, 불타는 혈액, 은 도가니, 진행
- 확인된 선택지:
  - 4턴 종료
  - 불타는 혈액
  - 은 도가니
  - 진행
  - 타격
  - 핑
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-e0ed2dbbd8d0`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. RewardsScreen, 기사회생, 넘기기, 덱에 추가할 카드를 선택하세요.
- 확인된 선택지:
  - RewardsScreen
  - 기사회생
  - 넘기기
  - 덱에 추가할 카드를 선택하세요.
  - 분노
  - 사혈
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-16efcaf12fe0`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 넘기기, 몸통 박치기, 완벽한 타격, 천둥
- 확인된 선택지:
  - 넘기기
  - 몸통 박치기
  - 완벽한 타격
  - 천둥
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-1cdaa820e306`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 넘기기, 잿빛 타격, 진정한 끈기, 파괴
- 확인된 선택지:
  - 넘기기
  - 잿빛 타격
  - 진정한 끈기
  - 파괴
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-375f7fbeab2b`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 넘기기, 분노, 사전 타격, 천둥
- 확인된 선택지:
  - 넘기기
  - 분노
  - 사전 타격
  - 천둥
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-46d0a1e80307`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. RewardsScreen, 넘기기, 잊힌 의식+, 잿불+
- 확인된 선택지:
  - RewardsScreen
  - 넘기기
  - 잊힌 의식+
  - 잿불+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-4efa92d1d120`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. NCardRewardSelectionScreen, 분노, 사전 타격, 천둥
- 확인된 선택지:
  - NCardRewardSelectionScreen
  - 분노
  - 사전 타격
  - 천둥
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-45158a17cc50`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. RewardsScreen, 넘기기
- 확인된 선택지:
  - RewardsScreen
  - 넘기기
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-4ebb315d59b8`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. RewardsScreen
- 확인된 선택지:
  - RewardsScreen
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-8bee63cc8fcf`
- 그룹/풀 추정: 보상/UI, 선택지 확인됨
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 넘기기
- 확인된 선택지:
  - 넘기기
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Reward

- ID: `reward-425988603a25`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-43004418eff4`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-4d4fc3f8cb12`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-62736bb94b96`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-6ae8506a4da3`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-744827f45492`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-794a69f27d89`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-9ef67c1da78b`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-b0c24f2e5452`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-c10ee1ed7c08`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-c22a621f9f59`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-cfcb71956165`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-d74beb2772ef`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-ea78b5939b19`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

### Observed Reward

- ID: `reward-ed277fbc1cda`
- 그룹/풀 추정: 보상/UI
- 플레이 중 참조 시점: 보상 화면에서 보상 종류와 선택 구조 비교 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:reward-screen-opened`

