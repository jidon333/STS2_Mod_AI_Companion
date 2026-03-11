# 포션

- 전체 항목 수: 68
- 설명 본문이 채워진 항목: 63
- L10N 키 또는 제목이 연결된 항목: 63
- 선택지/옵션 정보가 있는 항목: 0

## 이 섹션이 도와주는 플레이 장면

- 전투 보상 화면
- 상점 구매
- 현재 포션 슬롯

## 현재 이 섹션에서 확인된 것

- 포션 표시명
- 한국어 설명
- 모델 클래스와 리소스 경로

## 아직 남은 점

- 사용 조건
- 가격과 획득 규칙

## 주요 L10N/리소스 힌트

- `localization/kor/potions.json`
- `localization/eng/potions.json`
- `localization/kor/potion_lab.json`

## 항목 목록

### 강장제

- ID: `megacrit-sts2-core-models-potions-fortifier`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 3배로 만듭니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FORTIFIER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.Fortifier`
- 리소스 경로: `res://images/potions/fortifier.pngu`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거대화 포션

- ID: `megacrit-sts2-core-models-potions-gigantificationpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음에 사용하는 공격 카드의 피해량이 3배로 증가합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GIGANTIFICATION_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.GigantificationPotion`
- 리소스 경로: `res://images/potions/gigantification_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공격 포션

- ID: `megacrit-sts2-core-models-potions-attackpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 공격 카드 [blue]3[/blue]장 중 [blue]1[/blue]장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ATTACK_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.AttackPotion`
- 리소스 경로: `res://images/potions/attack_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 과일 주스

- ID: `megacrit-sts2-core-models-potions-fruitjuice`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 최대 체력을 [blue]5[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: AnyTime
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FRUIT_JUICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FruitJuice`
- 리소스 경로: `res://images/potions/fruit_juice.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광기의 손길

- ID: `megacrit-sts2-core-models-potions-touchofinsanity`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에서 카드를 1장 선택합니다. 이번 전투 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 선택 프롬프트: 비용을 없앨 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TOUCH_OF_INSANITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.TouchOfInsanity`
- 리소스 경로: `res://images/potions/touch_of_insanity.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광휘의 팅크

- ID: `megacrit-sts2-core-models-potions-radianttincture`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다. 다음 [blue]3[/blue]턴 동안 추가로 {energyPrefix:energyIcons(1)}를 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RADIANT_TINCTURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.RadiantTincture`
- 리소스 경로: `res://images/potions/radiant_tincture.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 교활함 포션

- ID: `megacrit-sts2-core-models-potions-cunningpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]강화[/gold]된 [gold]단도[/gold]를 [blue]3[/blue]장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CUNNING_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.CunningPotion`
- 리소스 경로: `res://images/potions/cunning_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 구울 항아리

- ID: `megacrit-sts2-core-models-potions-potofghouls`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]영혼[/gold]을 [blue]2[/blue]장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POT_OF_GHOULS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PotOfGhouls`
- 리소스 경로: `res://images/potions/pot_of_ghouls.pngh`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 단지 속의 유령

- ID: `megacrit-sts2-core-models-potions-ghostinajar`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]불가침[/gold]을 [blue]1[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GHOST_IN_A_JAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.GhostInAJar`
- 리소스 경로: `res://images/potions/ghost_in_a_jar.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 도박꾼의 영액

- ID: `megacrit-sts2-core-models-potions-gamblersbrew`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 원하는 만큼 카드를 버리고 버린 만큼 카드를 뽑습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 선택 프롬프트: 교체할 카드를 원하는 만큼 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GAMBLERS_BREW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.GamblersBrew`
- 리소스 경로: `res://images/potions/gamblers_brew.pngk`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 딱정벌레 주스

- ID: `megacrit-sts2-core-models-potions-beetlejuice`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Repeat:choose(1):다음 턴에|다음 [blue]{}[/blue]턴 동안} 적이 가하는 공격의 피해량이 [blue]{DamageDecrease}%[/blue] 감소합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEETLE_JUICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BeetleJuice`
- 리소스 경로: `res://images/potions/beetle_juice.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 마잘레스의 선물

- ID: `megacrit-sts2-core-models-potions-mazalethsgift`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]의식[/gold]을 [blue]1[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAZALETHS_GIFT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.MazalethsGift`
- 리소스 경로: `res://images/potions/mazaleths_gift.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 명확성 추출물

- ID: `megacrit-sts2-core-models-potions-clarity`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 [blue]1[/blue]장 뽑습니다. 다음 {ClarityPower:choose(1):턴|[blue]{}[/blue]턴} 시작 시, 카드를 추가로 [blue]1[/blue]장 뽑습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLARITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.Clarity`
- 리소스 경로: `res://images/potions/clarity.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 몸풀기 포션

- ID: `megacrit-sts2-core-models-potions-flexpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 [blue]5[/blue] 얻습니다. 내 턴 종료 시 [gold]힘[/gold]을 [blue]5[/blue] 잃습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLEX_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FlexPotion`
- 리소스 경로: `res://images/potions/flex_potion.png~2~}`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 묘약

- ID: `megacrit-sts2-core-models-potions-cureall`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다. 카드를 [blue]2[/blue]장 뽑습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CURE_ALL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.CureAll`
- 리소스 경로: `res://images/potions/cure_all.png@`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무색 포션

- ID: `megacrit-sts2-core-models-potions-colorlesspotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 무색 카드 [blue]3[/blue]장 중 [blue]1[/blue]장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLORLESS_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.ColorlessPotion`
- 리소스 경로: `res://images/potions/colorless_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무쇠의 심장

- ID: `megacrit-sts2-core-models-potions-heartofiron`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]판금[/gold]을 [blue]7[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEART_OF_IRON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.HeartOfIron`
- 리소스 경로: `res://images/potions/heart_of_iron.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 물교기 기름

- ID: `megacrit-sts2-core-models-potions-fyshoil`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 [blue]1[/blue] 얻고 [gold]민첩[/gold]을 [blue]1[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FYSH_OIL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FyshOil`
- 리소스 경로: `res://images/potions/fysh_oil.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 민첩 포션

- ID: `megacrit-sts2-core-models-potions-dexteritypotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]민첩[/gold]을 [blue]2[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEXTERITY_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.DexterityPotion`
- 리소스 경로: `res://images/potions/dexterity_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 밀집 포션

- ID: `megacrit-sts2-core-models-potions-focuspotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]밀집[/gold]을 [blue]2[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOCUS_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FocusPotion`
- 리소스 경로: `res://images/potions/focus_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발광수 포션

- ID: `megacrit-sts2-core-models-potions-glowwaterpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 [gold]소멸[/gold]시킵니다. 카드를 [blue]10[/blue]장 뽑습니다.
- 구조 정보:
  - 희귀도: Event
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLOWWATER_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.GlowwaterPotion`
- 리소스 경로: `res://images/potions/glowwater_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 방어도 포션

- ID: `megacrit-sts2-core-models-potions-blockpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 [blue]12[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLOCK_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BlockPotion`
- 리소스 경로: `res://images/potions/block_potion.png+b`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별 포션

- ID: `megacrit-sts2-core-models-potions-starpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Stars:starIcons()}을 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STAR_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.StarPotion`
- 리소스 경로: `res://images/potions/star_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 병 속의 가능성

- ID: `megacrit-sts2-core-models-potions-bottledpotential`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 카드를 [gold]뽑을 카드 더미[/gold]에 섞어넣습니다. 카드를 [blue]5[/blue]장 뽑습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOTTLED_POTENTIAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BottledPotential`
- 리소스 경로: `res://images/potions/bottled_potential.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 병 속의 배

- ID: `megacrit-sts2-core-models-potions-shipinabottle`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 [blue]10[/blue] 얻습니다. 다음 턴에 [gold]방어도[/gold]를 [blue]10[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHIP_IN_A_BOTTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.ShipInABottle`
- 리소스 경로: `res://images/potions/ship_in_a_bottle.png:`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 병 속의 요정

- ID: `megacrit-sts2-core-models-potions-fairyinabottle`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 사망 시, 그 대신 이 포션이 버려지고 최대 체력의 [blue]30%[/blue]만큼 체력을 회복합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: Automatic
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FAIRY_IN_A_BOTTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FairyInABottle`
- 리소스 경로: `res://images/potions/fairy_in_a_bottle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 병사의 스튜

- ID: `megacrit-sts2-core-models-potions-soldiersstew`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 전투 동안 이름에 [gold]타격[/gold]이 포함된 카드가 [gold]재사용[/gold]을 [blue]1[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOLDIERS_STEW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.SoldiersStew`
- 리소스 경로: `res://images/potions/soldiers_stew.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 복제액

- ID: `megacrit-sts2-core-models-potions-duplicator`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 사용하는 다음 카드가 1번 추가로 사용됩니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DUPLICATOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.Duplicator`
- 리소스 경로: `res://images/potions/duplicator.pngm`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 뼈다귀 영액

- ID: `megacrit-sts2-core-models-potions-bonebrew`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] [blue]{Summon}[/blue].
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BONE_BREW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BoneBrew`
- 리소스 경로: `res://images/potions/bone_brew.pngS`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 속도 포션

- ID: `megacrit-sts2-core-models-potions-speedpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]민첩[/gold]을 [blue]5[/blue] 얻습니다. 내 턴 종료 시 [gold]민첩[/gold]을 [blue]5[/blue] 잃습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPEED_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.SpeedPotion`
- 리소스 경로: `res://images/potions/speed_potion.pngnW&`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 속박 포션

- ID: `megacrit-sts2-core-models-potions-shacklingpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 모든 적이 [gold]힘[/gold]을 [blue]7[/blue] 잃습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AllEnemies
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHACKLING_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.ShacklingPotion`
- 리소스 경로: `res://images/potions/shackling_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 속박의 포션

- ID: `megacrit-sts2-core-models-potions-potionofbinding`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 [gold]약화[/gold]를 [blue]1[/blue], [gold]취약[/gold]을 [blue]1[/blue] 부여합니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AllEnemies
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_OF_BINDING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PotionOfBinding`
- 리소스 경로: `res://images/potions/potion_of_binding.pngB`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수용의 포션

- ID: `megacrit-sts2-core-models-potions-potionofcapacity`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]구체 슬롯[/gold]을 [blue]{Repeat}[/blue]개 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_OF_CAPACITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PotionOfCapacity`
- 리소스 경로: `res://images/potions/potion_of_capacity.png!`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스네코 기름

- ID: `megacrit-sts2-core-models-potions-sneckooil`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 [blue]7[/blue]장 뽑습니다. 이번 턴 동안 [gold]손[/gold]에 있는 카드의 비용을 무작위로 변경합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SNECKO_OIL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.SneckoOil`
- 리소스 경로: `res://images/potions/snecko_oil.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스킬 포션

- ID: `megacrit-sts2-core-models-potions-skillpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 스킬 카드 [blue]3[/blue]장 중 [blue]1[/blue]장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SKILL_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.SkillPotion`
- 리소스 경로: `res://images/potions/skill_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 신속 포션

- ID: `megacrit-sts2-core-models-potions-swiftpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 [blue]3[/blue]장 뽑습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SWIFT_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.SwiftPotion`
- 리소스 경로: `res://images/potions/swift_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 안정된 혈청

- ID: `megacrit-sts2-core-models-potions-stableserum`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드를 [blue]{Repeat}[/blue]턴 동안 [gold]보존[/gold]합니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STABLE_SERUM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.StableSerum`
- 리소스 경로: `res://images/potions/stable_serum.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 암흑의 정수

- ID: `megacrit-sts2-core-models-potions-essenceofdarkness`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 [gold]구체 슬롯[/gold]에 [gold]암흑[/gold]을 [gold]영창[/gold]합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ESSENCE_OF_DARKNESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.EssenceOfDarkness`
- 리소스 경로: `res://images/potions/essence_of_darkness.png6`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 액상 기억

- ID: `megacrit-sts2-core-models-potions-liquidmemories`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버린 카드 더미[/gold]에서 카드를 1장 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드의 비용이 [blue]0[/blue] {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LIQUID_MEMORIES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.LiquidMemories`
- 리소스 경로: `res://images/potions/liquid_memories.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 액상 청동

- ID: `megacrit-sts2-core-models-potions-liquidbronze`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]가시[/gold]를 [blue]3[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LIQUID_BRONZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.LiquidBronze`
- 리소스 경로: `res://images/potions/liquid_bronze.pngK`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 약화 포션

- ID: `megacrit-sts2-core-models-potions-weakpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]약화[/gold]를 [blue]3[/blue] 부여합니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WEAK_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.WeakPotion`
- 리소스 경로: `res://images/potions/weak_potion.pngX`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 에너지 포션

- ID: `megacrit-sts2-core-models-potions-energypotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENERGY_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.EnergyPotion`
- 리소스 경로: `res://images/potions/energy_potion.pngQ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 엔트로피의 영액

- ID: `megacrit-sts2-core-models-potions-entropicbrew`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 비어있는 모든 포션 슬롯을 무작위 포션으로 채웁니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: AnyTime
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENTROPIC_BREW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.EntropicBrew`
- 리소스 경로: `res://images/potions/entropic_brew.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 역겨운 포션

- ID: `megacrit-sts2-core-models-potions-foulpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 누구에게나 피해를 [blue]12[/blue] 줍니다.
[gold]상인[/gold]에게 던질 수 있으며, 상인에게 던질 시 피해를 주는 대신 [gold]골드[/gold]를 [blue]100[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Event
  - 사용 제약: AnyTime
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOUL_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FoulPotion`
- 리소스 경로: `res://images/potions/foul_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 예견의 방울

- ID: `megacrit-sts2-core-models-potions-dropletofprecognition`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 카드를 1장 선택해 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DROPLET_OF_PRECOGNITION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.DropletOfPrecognition`
- 리소스 경로: `res://images/potions/droplet_of_precognition.png?s`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 오로바스산

- ID: `megacrit-sts2-core-models-potions-orobicacid`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 공격, 스킬, 파워 카드를 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드들을 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OROBIC_ACID`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.OrobicAcid`
- 리소스 경로: `res://images/potions/orobic_acid.pngo`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 왕의 기개

- ID: `megacrit-sts2-core-models-potions-kingscourage`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] [blue]{Forge}[/blue].
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KINGS_COURAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.KingsCourage`
- 리소스 경로: `res://images/potions/kings_courage.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 우주적 혼합물

- ID: `megacrit-sts2-core-models-potions-cosmicconcoction`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]강화[/gold]된 무색 카드를 [blue]3[/blue]장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COSMIC_CONCOCTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.CosmicConcoction`
- 리소스 경로: `res://images/potions/cosmic_concoction.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재련의 축복

- ID: `megacrit-sts2-core-models-potions-blessingoftheforge`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 남은 전투 동안 [gold]강화[/gold]합니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLESSING_OF_THE_FORGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BlessingOfTheForge`
- 리소스 경로: `res://images/potions/blessing_of_the_forge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재생 포션

- ID: `megacrit-sts2-core-models-potions-regenpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]재생[/gold]을 [green]5[/green] 얻습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REGEN_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.RegenPotion`
- 리소스 경로: `res://images/potions/regen_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잿물

- ID: `megacrit-sts2-core-models-potions-ashwater`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드를 원하는 만큼 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: Self
- 선택 프롬프트: [gold]소멸[/gold]시킬 카드를 원하는 만큼 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ASHWATER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.Ashwater`
- 리소스 경로: `res://images/potions/ashwater.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정제된 혼돈

- ID: `megacrit-sts2-core-models-potions-distilledchaos`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold] 위에서부터 [blue]{Repeat}[/blue]장의 카드를 사용합니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISTILLED_CHAOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.DistilledChaos`
- 리소스 경로: `res://images/potions/distilled_chaos.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 종말의 포션

- ID: `megacrit-sts2-core-models-potions-potionofdoom`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]종말[/gold]을 [blue]33[/blue] 부여합니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_OF_DOOM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PotionOfDoom`
- 리소스 경로: `res://images/potions/potion_of_doom.png(`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 종언의 가루

- ID: `megacrit-sts2-core-models-potions-powdereddemise`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적의 턴 종료 시 적이 체력을 [blue]{Demise}[/blue] 잃습니다.
- 구조 정보:
  - 희귀도: Uncommon
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POWDERED_DEMISE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PowderedDemise`
- 리소스 경로: `res://images/potions/powdered_demise.png;`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 중독 포션

- ID: `megacrit-sts2-core-models-potions-poisonpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]중독[/gold]을 [blue]6[/blue] 부여합니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POISON_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PoisonPotion`
- 리소스 경로: `res://images/potions/poison_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 취약 포션

- ID: `megacrit-sts2-core-models-potions-vulnerablepotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]취약[/gold]을 [blue]3[/blue] 부여합니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VULNERABLE_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.VulnerablePotion`
- 리소스 경로: `res://images/potions/vulnerable_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파워 포션

- ID: `megacrit-sts2-core-models-potions-powerpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 파워 카드 [blue]3[/blue]장 중 [blue]1[/blue]장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: Self
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POWER_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PowerPotion`
- 리소스 경로: `res://images/potions/power_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포션 모양 돌

- ID: `megacrit-sts2-core-models-potions-potionshapedrock`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 [blue]15[/blue] 줍니다.
- 구조 정보:
  - 희귀도: Token
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_SHAPED_ROCK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.PotionShapedRock`
- 리소스 경로: `res://images/potions/potion_shaped_rock.png$V`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 폭발성 앰플

- ID: `megacrit-sts2-core-models-potions-explosiveampoule`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 [blue]10[/blue] 줍니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AllEnemies
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXPLOSIVE_AMPOULE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.ExplosiveAmpoule`
- 리소스 경로: `res://images/potions/explosive_ampoule.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 피 포션

- ID: `megacrit-sts2-core-models-potions-bloodpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 최대 체력의 [green]{HealPercent}%[/green]만큼 체력을 회복합니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: AnyTime
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLOOD_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.BloodPotion`
- 리소스 경로: `res://images/potions/blood_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 행운의 활력소

- ID: `megacrit-sts2-core-models-potions-luckytonic`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버퍼[/gold]를 [blue]1[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Rare
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LUCKY_TONIC`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.LuckyTonic`
- 리소스 경로: `res://images/potions/lucky_tonic.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 화염 포션

- ID: `megacrit-sts2-core-models-potions-firepotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 [blue]20[/blue] 줍니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyEnemy
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIRE_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.FirePotion`
- 리소스 경로: `res://images/potions/fire_potion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 힘 포션

- ID: `megacrit-sts2-core-models-potions-strengthpotion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 [blue]2[/blue] 얻습니다.
- 구조 정보:
  - 희귀도: Common
  - 사용 제약: CombatOnly
  - 대상: AnyPlayer
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRENGTH_POTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Potions.StrengthPotion`
- 리소스 경로: `res://images/potions/strength_potion.png|`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### POTION.ATTACK_POTION

- ID: `potion-attack-potion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:potion-changed`

### POTION.DEXTERITY_POTION

- ID: `potion-dexterity-potion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:potion-changed`

### POTION.FAIRY_IN_A_BOTTLE

- ID: `potion-fairy-in-a-bottle`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:potion-changed`

### POTION.SKILL_POTION

- ID: `potion-skill-potion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:potion-changed`

### POTION.WEAK_POTION

- ID: `potion-weak-potion`
- 그룹/풀 추정: 포션 관련 항목
- 플레이 중 참조 시점: 전투 보상, 상점 구매, 현재 포션 슬롯 해석
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:potion-changed`

