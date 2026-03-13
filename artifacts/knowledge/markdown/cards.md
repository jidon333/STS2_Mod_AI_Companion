# 카드

- 전체 항목 수: 617
- 설명 본문이 채워진 항목: 562
- L10N 키 또는 제목이 연결된 항목: 575
- 선택지/옵션 정보가 있는 항목: 0

## 이 섹션이 도와주는 플레이 장면

- 카드 보상 화면
- 상점 구매 후보
- 이벤트 특수 선택지
- 현재 덩 구성 파악

## 현재 이 섹션에서 확인된 것

- 카드 표시명
- 한국어/영어 L10N 설명
- 선택 프롬프트
- 모델 클래스와 초상화 경로

## 아직 남은 점

- 강화 단계별 차이
- 실시간 전투 상호작용
- 실플레이 교차 검증

## 주요 L10N/리소스 힌트

- `localization/kor/cards.json`
- `localization/eng/cards.json`
- `localization/kor/card_library.json`

## 항목 목록

### Ftl

- ID: `megacrit-sts2-core-models-cards-ftl`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5 줍니다.
이번 턴에 카드를 {PlayMax:diff()}장보다 적게 사용했다면, 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FTL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Ftl`
- 리소스 경로: `res://images/packed/card_portraits/defect/ftl.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### Hello World

- ID: `megacrit-sts2-core-models-cards-helloworld`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 일반 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HELLO_WORLD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HelloWorld`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/hello_world.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### Null

- ID: `megacrit-sts2-core-models-cards-null`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]약화[/gold]를 2 부여합니다.
[gold]암흑[/gold]을 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NULL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Null`
- 리소스 경로: `res://images/packed/card_portraits/defect/null.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가로막기

- ID: `megacrit-sts2-core-models-cards-intercept`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
이번 턴 동안 다른 플레이어가 받아야 하는 모든 공격의 목표를 자신으로 변경합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: colorless
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INTERCEPT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Intercept`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/intercept.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가속 이탈

- ID: `megacrit-sts2-core-models-cards-boostaway`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]버린 카드 더미[/gold]에 [gold]어지러움[/gold]을 1장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOOST_AWAY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BoostAway`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/boost_away.png+`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 갈가리 찢기

- ID: `megacrit-sts2-core-models-cards-tearasunder`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5 줍니다.
이번 전투 동안 체력을 잃은 횟수만큼 반복합니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다.)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TEAR_ASUNDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TearAsunder`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/tear_asunder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 갈취

- ID: `megacrit-sts2-core-models-cards-pillage`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
공격이 아닌 카드를 뽑을 때까지 카드를 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PILLAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Pillage`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/pillage.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 감마 세례

- ID: `megacrit-sts2-core-models-cards-gammablast`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
[gold]약화[/gold]를 2 부여합니다.
[gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GAMMA_BLAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GammaBlast`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/gamma_blast.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 감염

- ID: `megacrit-sts2-core-models-cards-infection`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, 피해를 3 받습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFECTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Infection`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/infection.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강령의 극의

- ID: `megacrit-sts2-core-models-cards-necromastery`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
[gold]골골이[/gold]가 체력을 잃을 때마다,
모든 적이 동일한 만큼의 체력을 잃습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NECRO_MASTERY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NecroMastery`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/necro_mastery.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강령회

- ID: `megacrit-sts2-core-models-cards-seance`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]의 카드를 1장 [gold]{IfUpgraded:show:영혼+|영혼}[/gold]으로 변화시킵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SEANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Seance`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/seance.pngsY`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강철의 섬광

- ID: `megacrit-sts2-core-models-cards-flashofsteel`
- 그룹/풀 추정: 무색, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5 줍니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLASH_OF_STEEL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FlashOfSteel`
- 리소스 경로: `res://images/packed/card_portraits/colorless/flash_of_steel.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강철의 폭풍

- ID: `megacrit-sts2-core-models-cards-stormofsteel`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 버립니다.
버린 카드의 수만큼 [gold]{IfUpgraded:show:단도+|단도}[/gold]를 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STORM_OF_STEEL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StormOfSteel`
- 리소스 경로: `res://images/packed/card_portraits/silent/storm_of_steel.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강타

- ID: `megacrit-sts2-core-models-cards-bash`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
[gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bash`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/bash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강탈

- ID: `megacrit-sts2-core-models-cards-reave`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
[gold]뽑을 카드 더미[/gold]에 [gold]{IfUpgraded:show:영혼+|영혼}[/gold]을 1장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REAVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reave`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/reave.pngM$`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 강행 돌파

- ID: `megacrit-sts2-core-models-cards-fightthrough`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 13 얻습니다.
[gold]버린 카드 더미[/gold]에 [gold]부상[/gold]을 2장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIGHT_THROUGH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FightThrough`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/fight_through.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거대한 바위

- ID: `megacrit-sts2-core-models-cards-giantrock`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 16 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Token
  - 대상: AnyEnemy
  - 카드 풀: token
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GIANT_ROCK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GiantRock`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/giant_rock.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거상

- ID: `megacrit-sts2-core-models-cards-colossus`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
이번 턴에 [gold]취약[/gold] 상태의 적에게서 받는 피해량이 50% 감소합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLOSSUS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Colossus`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/colossus.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 걷어내기

- ID: `megacrit-sts2-core-models-cards-skim`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SKIM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Skim`
- 리소스 경로: `res://images/packed/card_portraits/defect/skim.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 검날 개선

- ID: `megacrit-sts2-core-models-cards-refineblade`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
다음 턴에, 1를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFINE_BLADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RefineBlade`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/refine_blade.png{`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 검무

- ID: `megacrit-sts2-core-models-cards-bladedance`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단도[/gold]를 3장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLADE_DANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BladeDance`
- 리소스 경로: `res://src/Core/Models/Cards/BladeDance.csd`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 검성

- ID: `megacrit-sts2-core-models-cards-swordsage`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]군주의 칼날[/gold]의 비용이 1 증가합니다. [gold]군주의 칼날[/gold]이 이제 1번 추가로 적중합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SWORD_SAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SwordSage`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/sword_sage.pnggO`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 격노

- ID: `megacrit-sts2-core-models-cards-rage`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 공격 카드를 사용할 때마다, [gold]방어도[/gold]를 {Power:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rage`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/rage.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 격돌

- ID: `megacrit-sts2-core-models-cards-clash`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드가 공격 카드일 때만 사용할 수 있습니다.
피해를 14 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Event
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Clash`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/clash.png$`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 격려

- ID: `megacrit-sts2-core-models-cards-spur`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
[gold]골골이[/gold]가 체력을 5 회복합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+2, Heal+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPUR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Spur`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/spur.pngk`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 경계 엿보기

- ID: `megacrit-sts2-core-models-cards-glimpsebeyond`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 플레이어의 [gold]뽑을 카드 더미[/gold]에 [gold]영혼[/gold]을 3장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AllAllies
  - 카드 풀: necrobinder
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLIMPSE_BEYOND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GlimpseBeyond`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/glimpse_beyond.pngQ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 경계 태세

- ID: `megacrit-sts2-core-models-cards-sentrymode`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]훑어보기[/gold]를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SENTRY_MODE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SentryMode`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/sentry_mode.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 계몽

- ID: `megacrit-sts2-core-models-cards-enlightenment`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 {IfUpgraded:show:전투|턴} 동안, [gold]손[/gold]에 있는 모든 카드의 비용이 1로 감소합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENLIGHTENMENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Enlightenment`
- 리소스 경로: `res://images/packed/card_portraits/event/enlightenment.pngy`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 계산된 도박

- ID: `megacrit-sts2-core-models-cards-calculatedgamble`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 버린 뒤,
버린 카드의 수만큼 카드를 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALCULATED_GAMBLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CalculatedGamble`
- 리소스 경로: `res://images/packed/card_portraits/silent/calculated_gamble.png!,`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고동치는 도끼

- ID: `megacrit-sts2-core-models-cards-thrumminghatchet`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 11 줍니다.
다음 턴 시작 시, 이 카드를 [gold]손[/gold]으로 다시 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THRUMMING_HATCHET`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ThrummingHatchet`
- 리소스 경로: `res://images/packed/card_portraits/colorless/thrumming_hatchet.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고리형 강인함

- ID: `megacrit-sts2-core-models-cards-torictoughness`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
다음 {Turns:diff()}턴 동안, 턴 시작 시 [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TORIC_TOUGHNESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ToricToughness`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/toric_toughness.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고정시키기

- ID: `megacrit-sts2-core-models-cards-fasten`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 수비 카드를 통해 얻는 [gold]방어도[/gold]가 {ExtraBlock:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FASTEN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fasten`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/fasten.pngp`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고철을 보물로

- ID: `megacrit-sts2-core-models-cards-trashtotreasure`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 상태이상 카드를 생성할 때마다, 무작위 구체를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRASH_TO_TREASURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TrashToTreasure`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/trash_to_treasure.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 곡예

- ID: `megacrit-sts2-core-models-cards-acrobatics`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
카드를 1장 버립니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ACROBATICS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Acrobatics`
- 리소스 경로: `res://images/packed/card_portraits/silent/acrobatics.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공격성

- ID: `megacrit-sts2-core-models-cards-aggression`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]버린 카드 더미[/gold]에서 무작위 공격 카드를 1장 [gold]손[/gold]으로 가져오고 [gold]강화[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AGGRESSION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Aggression`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/aggression.pngX`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공중제비

- ID: `megacrit-sts2-core-models-cards-backflip`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BACKFLIP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Backflip`
- 리소스 경로: `res://images/packed/card_portraits/silent/backflip.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공진

- ID: `megacrit-sts2-core-models-cards-resonance`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 1 얻습니다. 모든 적이 [gold]힘[/gold]을 1 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RESONANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Resonance`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/resonance.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공허

- ID: `megacrit-sts2-core-models-cards-void`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 카드를 뽑을 때마다, 1를 잃습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VOID`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Void`
- 리소스 경로: `res://images/packed/card_portraits/status/void.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공허의 부름

- ID: `megacrit-sts2-core-models-cards-callofthevoid`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 카드를 1장 [gold]손[/gold]으로 가져옵니다. 그 카드가 [gold]휘발성[/gold]을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALL_OF_THE_VOID`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CallOfTheVoid`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/call_of_the_void.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공허의 형상

- ID: `megacrit-sts2-core-models-cards-voidform`
- 그룹/풀 추정: Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴을 종료합니다.
매 턴마다 처음으로 내는 {VoidFormPower:choose(1):카드를|카드 2장을} 비용 없이 사용합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VOID_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.VoidForm`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/void_form.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광란

- ID: `megacrit-sts2-core-models-cards-rampage`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
이번 전투 동안 이 카드의 피해량이 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RAMPAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rampage`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/rampage.pngG`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광란의 포식

- ID: `megacrit-sts2-core-models-cards-feedingfrenzy`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 [gold]힘[/gold]을 5 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Strength+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FEEDING_FRENZY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FeedingFrenzy`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/feeding_frenzy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광선 휩쓸기

- ID: `megacrit-sts2-core-models-cards-sweepingbeam`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 6 줍니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SWEEPING_BEAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SweepingBeam`
- 리소스 경로: `res://images/packed/card_portraits/defect/sweeping_beam.png'`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광자 베기

- ID: `megacrit-sts2-core-models-cards-photoncut`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
카드를 1장 뽑습니다.
[gold]손[/gold]에 있는 카드 1장을 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3, Cards+1
- 선택 프롬프트: 뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PHOTON_CUT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PhotonCut`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/photon_cut.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광채

- ID: `megacrit-sts2-core-models-cards-glow`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Stars:starIcons()}을 얻습니다.
카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Stars+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Glow`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/glow.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 괜찮은 전략

- ID: `megacrit-sts2-core-models-cards-welllaidplans`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시, 카드를 최대 {RetainAmount:diff()}장까지 [gold]보존[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WELL_LAID_PLANS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.WellLaidPlans`
- 리소스 경로: `res://images/packed/card_portraits/silent/well_laid_plans.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 괴짜 과학

- ID: `megacrit-sts2-core-models-cards-madscience`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {CardType:choose(Attack|Skill|Power):피해를 12{Violence: {ViolenceHits:만큼 diff()}번|} 줍니다.|[gold]방어도[/gold]를 8 얻습니다.|}{HasRider:{Sapping:
[gold]약화[/gold]를 {SappingWeak:diff()} 부여합니다.
[gold]취약[/gold]을 {SappingVulnerable:diff()} 부여합니다.|}{Choking:
이번 턴에 카드를 사용할 때마다, 대상 적이 체력을 {ChokingDamage:diff()} 잃습니다.|}{Energized:
{EnergizedEnergy:energyIcons()}를 얻습니다.|}{Wisdom:
카드를 {WisdomCards:diff()}장 뽑습니다.|}{Chaos:
무작위 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.|}{Expertise:[gold]힘[/gold]을 {ExpertiseStrength:diff()} 얻습니다.
[gold]민첩[/gold]을 {ExpertiseDexterity:diff()} 얻습니다.|}{Curious:파워 카드의 비용이 {CuriousReduction:diff()} {energyPrefix:energyIcons(1)} 감소합니다.|}{Improvement:전투 종료 시, 무작위 카드를 1장 [gold]강화[/gold]합니다.|}|{CardType:choose(Attack|Skill|Power):
???|
???|???}}
- 구조 정보:
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAD_SCIENCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MadScience`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 구렁이의 형상

- ID: `megacrit-sts2-core-models-cards-serpentform`
- 그룹/풀 추정: 사일런트, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 사용할 때마다, 무작위 적에게 피해를 4 줍니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SERPENT_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SerpentForm`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/serpent_form.png4xqO`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 구르기

- ID: `megacrit-sts2-core-models-cards-dodgeandroll`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 4 얻습니다.
다음 턴에, [gold]방어도[/gold]를 4 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DODGE_AND_ROLL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DodgeAndRoll`
- 리소스 경로: `res://images/packed/card_portraits/silent/dodge_and_roll.pngX`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 군주의 시선

- ID: `megacrit-sts2-core-models-cards-monarchsgaze`
- 그룹/풀 추정: Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적을 공격할 때마다, 대상 적이 이번 턴 동안 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MONARCHS_GAZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MonarchsGaze`
- 리소스 경로: `res://images/packed/card_portraits/regent/monarchs_gaze.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 군주의 칼날

- ID: `megacrit-sts2-core-models-cards-sovereignblade`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {SeekingEdgeAmount:cond:>0?모든 적에게 |}피해를 10{Repeat:choose(1):|만큼 {}번} 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Token
  - 대상: AnyEnemy
  - 카드 풀: token
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOVEREIGN_BLADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SovereignBlade`
- 리소스 경로: `res://images/packed/card_portraits/token/sovereign_blade.pngU~`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 굴뚝

- ID: `megacrit-sts2-core-models-cards-smokestack`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 상태이상 카드를 생성할 때마다, 모든 적에게 피해를 5 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SMOKESTACK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Smokestack`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/smokestack.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 굴러가는 바위

- ID: `megacrit-sts2-core-models-cards-rollingboulder`
- 그룹/풀 추정: 무색, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 모든 적에게 피해를 5 주고 이 효과의 피해량이 {IncrementAmount:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROLLING_BOULDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RollingBoulder`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/rolling_boulder.pngn`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 굴절

- ID: `megacrit-sts2-core-models-cards-refract`
- 그룹/풀 추정: 디펙트, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9만큼 2번 줍니다.
[gold]유리[/gold]를 {Repeat:diff()}번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFRACT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Refract`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/refract.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 궁극의 수비

- ID: `megacrit-sts2-core-models-cards-ultimatedefend`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 11 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ULTIMATE_DEFEND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.UltimateDefend`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/ultimate_defend.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 궁극의 타격

- ID: `megacrit-sts2-core-models-cards-ultimatestrike`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 14 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ULTIMATE_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.UltimateStrike`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/ultimate_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 권역

- ID: `megacrit-sts2-core-models-cards-demesne`
- 그룹/풀 추정: Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 1를 얻고 카드를 추가로 1장 뽑습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEMESNE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Demesne`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/demesne.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 권위 행사

- ID: `megacrit-sts2-core-models-cards-manifestauthority`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
무작위{IfUpgraded:show: [gold]강화[/gold]된} 무색 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MANIFEST_AUTHORITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ManifestAuthority`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/manifest_authority.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 궤도

- ID: `megacrit-sts2-core-models-cards-orbit`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {energyPrefix:energyIcons(4)}를 소모할 때마다,
1를 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ORBIT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Orbit`
- 리소스 경로: `res://images/packed/card_portraits/regent/orbit.png?[`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 귀를 찢는 비명

- ID: `megacrit-sts2-core-models-cards-piercingwail`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 모든 적이 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PIERCING_WAIL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PiercingWail`
- 리소스 경로: `res://images/packed/card_portraits/silent/piercing_wail.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 규칙 준수

- ID: `megacrit-sts2-core-models-cards-normality`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한 턴에 카드를 최대 3장까지만 사용할 수 있습니다.{InCombat:
({CalculatedCards:choose(1):{}장|{}장} 남았습니다)|}
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NORMALITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Normality`
- 리소스 경로: `res://images/packed/card_portraits/curse/normality.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그래플링

- ID: `megacrit-sts2-core-models-cards-grapple`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
이번 턴에 [gold]방어도[/gold]를 얻을 때마다, 대상 적에게 피해를 5 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAPPLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Grapple`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/grapple.pngV`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그렇게 하라

- ID: `megacrit-sts2-core-models-cards-makeitso`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
한 턴에 스킬 카드를 3장 사용할 때마다, 이 카드를 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAKE_IT_SO`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MakeItSo`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/make_it_so.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그림자 걸음

- ID: `megacrit-sts2-core-models-cards-shadowstep`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 버립니다.
다음 턴에, 공격 카드의 피해량이 2배가 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHADOW_STEP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ShadowStep`
- 리소스 경로: `res://images/packed/card_portraits/silent/shadow_step.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그림자 방패

- ID: `megacrit-sts2-core-models-cards-shadowshield`
- 그룹/풀 추정: 디펙트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 11 얻습니다.
[gold]암흑[/gold]을 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHADOW_SHIELD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ShadowShield`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/shadow_shield.png]`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그림자 소모

- ID: `megacrit-sts2-core-models-cards-consumingshadow`
- 그룹/풀 추정: 디펙트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]암흑[/gold]을 {Repeat:diff()}번 [gold]영창[/gold]합니다.
내 턴 종료 시, 가장 왼쪽의 구체를 [gold]발현[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CONSUMING_SHADOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ConsumingShadow`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/consuming_shadow.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 그림자 은신

- ID: `megacrit-sts2-core-models-cards-shadowmeld`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 얻는 [gold]방어도[/gold]가 2배가 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHADOWMELD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shadowmeld`
- 리소스 경로: `res://images/packed/card_portraits/silent/shadowmeld.pngJ_k`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 극적인 입장

- ID: `megacrit-sts2-core-models-cards-dramaticentrance`
- 그룹/풀 추정: 무색, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 11 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: colorless
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DRAMATIC_ENTRANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DramaticEntrance`
- 리소스 경로: `res://images/packed/card_portraits/colorless/dramatic_entrance.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 긁어내기

- ID: `megacrit-sts2-core-models-cards-scrape`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
카드를 4장 뽑습니다.
뽑은 카드 중 비용이 0 {energyPrefix:energyIcons(1)}이 아닌 모든 카드를 버립니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SCRAPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Scrape`
- 리소스 경로: `res://images/packed/card_portraits/defect/scrape.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 금지된 마도서

- ID: `megacrit-sts2-core-models-cards-forbiddengrimoire`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 전투 종료 시, [gold]덱[/gold]에서 카드를 1장 제거합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FORBIDDEN_GRIMOIRE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ForbiddenGrimoire`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/forbidden_grimoire.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 기계학습

- ID: `megacrit-sts2-core-models-cards-machinelearning`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 카드를 추가로 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MACHINE_LEARNING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MachineLearning`
- 리소스 경로: `res://images/packed/card_portraits/defect/machine_learning.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 기량

- ID: `megacrit-sts2-core-models-cards-prowess`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 1 얻습니다.
[gold]민첩[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Dexterity+1, Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PROWESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Prowess`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/prowess.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 기쁨의 선물

- ID: `megacrit-sts2-core-models-cards-bundleofjoy`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 무색 카드를 3장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BUNDLE_OF_JOY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BundleOfJoy`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/bundle_of_joy.png1`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 기사회생

- ID: `megacrit-sts2-core-models-cards-secondwind`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 공격이 아닌 모든 카드를 [gold]소멸[/gold]시킵니다. [gold]소멸[/gold]시킨 카드 1장당 [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SECOND_WIND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SecondWind`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/second_wind.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 길잡이별

- ID: `megacrit-sts2-core-models-cards-guidingstar`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 12 줍니다.
다음 턴에, 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+1, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GUIDING_STAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GuidingStar`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/guiding_star.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 꼬챙이

- ID: `megacrit-sts2-core-models-cards-skewer`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7만큼 X번 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SKEWER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Skewer`
- 리소스 경로: `res://images/packed/card_portraits/silent/skewer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 꽃샘추위

- ID: `megacrit-sts2-core-models-cards-coldsnap`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
[gold]냉기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLD_SNAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ColdSnap`
- 리소스 경로: `res://images/packed/card_portraits/defect/cold_snap.png>@`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 끌어내리기

- ID: `megacrit-sts2-core-models-cards-pullfrombelow`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 전투 동안 사용한 [gold]휘발성[/gold] 카드 1장당 피해를 5 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PULL_FROM_BELOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PullFromBelow`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/pull_from_below.pngS`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나는 무적이다

- ID: `megacrit-sts2-core-models-cards-iaminvincible`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
내 턴 종료 시 이 카드가 [gold]뽑을 카드 더미[/gold] 맨 위에 있다면, 사용합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `I_AM_INVINCIBLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.IAmInvincible`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/i_am_invincible.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나선 관통

- ID: `megacrit-sts2-core-models-cards-helixdrill`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 소모한 {energyPrefix:energyIcons(1)}당 피해를 3 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HELIX_DRILL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HelixDrill`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/helix_drill.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나태

- ID: `megacrit-sts2-core-models-cards-sloth`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 카드를 최대 3장까지만 사용할 수 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: token
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLOTH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Sloth`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/sloth.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 낙인

- ID: `megacrit-sts2-core-models-cards-brand`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
카드를 1장 [gold]소멸[/gold]시킵니다.
[gold]힘[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BRAND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Brand`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/brand.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난도질

- ID: `megacrit-sts2-core-models-cards-mangle`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 15 줍니다.
이번 턴 동안 적이 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MANGLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Mangle`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/mangle.pngP6`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난동

- ID: `megacrit-sts2-core-models-cards-uproar`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5만큼 2번 줍니다.
[gold]뽑을 카드 더미[/gold]에서 무작위 공격 카드를 1장 사용합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UPROAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Uproar`
- 리소스 경로: `res://images/packed/card_portraits/defect/uproar.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난사

- ID: `megacrit-sts2-core-models-cards-volley`
- 그룹/풀 추정: 무색, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 10만큼 X번 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: RandomEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VOLLEY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Volley`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/volley.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난타

- ID: `megacrit-sts2-core-models-cards-thrash`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 4만큼 2번 줍니다.
[gold]손[/gold]에 있는 무작위 공격 카드를 1장 [gold]소멸[/gold]시키고, 그 카드의 피해량을 이 카드에 추가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THRASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Thrash`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/thrash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 날 세우기

- ID: `megacrit-sts2-core-models-cards-seekingedge`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
[gold]군주의 칼날[/gold]이 이제 모든 적을 대상으로 합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SEEKING_EDGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SeekingEdge`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/seeking_edge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 날뛰기

- ID: `megacrit-sts2-core-models-cards-ripandtear`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 7만큼 2번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: RandomEnemy
  - 카드 풀: event
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RIP_AND_TEAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RipAndTear`
- 리소스 경로: `res://images/packed/card_portraits/event/rip_and_tear.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 내세

- ID: `megacrit-sts2-core-models-cards-afterlife`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AFTERLIFE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Afterlife`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/afterlife.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 냉각재

- ID: `megacrit-sts2-core-models-cards-coolant`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 영창된 구체의 종류 하나당 [gold]방어도[/gold]를 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COOLANT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Coolant`
- 리소스 경로: `res://images/packed/card_portraits/defect/coolant.png``
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 냉정함

- ID: `megacrit-sts2-core-models-cards-coolheaded`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]냉기[/gold]를 1번 [gold]영창[/gold]합니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COOLHEADED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Coolheaded`
- 리소스 경로: `res://src/Core/Models/Cards/Coolheaded.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 너만 믿는다

- ID: `megacrit-sts2-core-models-cards-believeinyou`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어가 3를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: colorless
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BELIEVE_IN_YOU`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BelieveInYou`
- 리소스 경로: `res://images/packed/card_portraits/colorless/believe_in_you.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 넘기기

- ID: `megacrit-sts2-core-models-cards-snap`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
[gold]손[/gold]에 있는 카드 1장에 [gold]보존[/gold]을 추가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+3
- 선택 프롬프트: [gold]보존[/gold]을 추가할 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SNAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Snap`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/snap.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 네 주제를 알라

- ID: `megacrit-sts2-core-models-cards-knowthyplace`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]약화[/gold]를 1 부여합니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KNOW_THY_PLACE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.KnowThyPlace`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/know_thy_place.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 녹아내리는 주먹

- ID: `megacrit-sts2-core-models-cards-moltenfist`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
적이 보유한 [gold]취약[/gold]이 2배로 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MOLTEN_FIST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MoltenFist`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/molten_fist.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 뇌우

- ID: `megacrit-sts2-core-models-cards-tempest`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]전기[/gold]를 {IfUpgraded:show:X+1|X}번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TEMPEST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tempest`
- 리소스 경로: `res://images/packed/card_portraits/defect/tempest.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 눈 할퀴기

- ID: `megacrit-sts2-core-models-cards-gofortheeyes`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
적이 공격할 의도가 있다면 [gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GO_FOR_THE_EYES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GoForTheEyes`
- 리소스 경로: `res://images/packed/card_portraits/defect/go_for_the_eyes.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 능숙

- ID: `megacrit-sts2-core-models-cards-finesse`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 4 얻습니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FINESSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Finesse`
- 리소스 경로: `res://images/packed/card_portraits/colorless/finesse.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 니오우의 격분

- ID: `megacrit-sts2-core-models-cards-neowsfury`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]버린 카드 더미[/gold]에서 무작위 카드를 {Cards:choose(1):1장|{:diff()}장} [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEOWS_FURY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NeowsFury`
- 리소스 경로: `res://images/packed/card_portraits/event/neows_fury.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 다리 걸기

- ID: `megacrit-sts2-core-models-cards-legsweep`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]약화[/gold]를 2 부여합니다.
[gold]방어도[/gold]를 11 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Block+3, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LEG_SWEEP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LegSweep`
- 리소스 경로: `res://images/packed/card_portraits/silent/leg_sweep.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 다시 시작

- ID: `megacrit-sts2-core-models-cards-reboot`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 카드를 [gold]뽑을 카드 더미[/gold]에 섞어 넣습니다.
카드를 4장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Cards+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REBOOT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reboot`
- 리소스 경로: `res://images/packed/card_portraits/defect/reboot.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 다중 시전

- ID: `megacrit-sts2-core-models-cards-multicast`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 가장 오른쪽의 구체를 {IfUpgraded:show:X+1|X}번 [gold]발현[/gold]합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MULTI_CAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MultiCast`
- 리소스 경로: `res://images/packed/card_portraits/defect/multi_cast.png@`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 단검 분사

- ID: `megacrit-sts2-core-models-cards-daggerspray`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 4만큼 2번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DAGGER_SPRAY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DaggerSpray`
- 리소스 경로: `res://images/packed/card_portraits/silent/dagger_spray.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 단검 투척

- ID: `megacrit-sts2-core-models-cards-daggerthrow`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
카드를 1장 뽑습니다.
카드를 1장 버립니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DAGGER_THROW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DaggerThrow`
- 리소스 경로: `res://images/packed/card_portraits/silent/dagger_throw.png+`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 단결

- ID: `megacrit-sts2-core-models-cards-rally`
- 그룹/풀 추정: 무색, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 플레이어가 [gold]방어도[/gold]를 12 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AllAllies
  - 카드 풀: colorless
  - 강화 요약: Block+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RALLY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rally`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/rally.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 단도

- ID: `megacrit-sts2-core-models-cards-shiv`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {FanOfKnivesAmount:cond:>0?모든 적에게 |}피해를 4 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Token
  - 대상: AnyEnemy
  - 카드 풀: token
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHIV`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shiv`
- 리소스 경로: `res://images/packed/card_portraits/token/shiv.pngL`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 달의 세례

- ID: `megacrit-sts2-core-models-cards-lunarblast`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 사용한 스킬 카드 1장당 피해를 4 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LUNAR_BLAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LunarBlast`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/lunar_blast.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 대공포

- ID: `megacrit-sts2-core-models-cards-flakcannon`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 [gold]상태이상[/gold] 카드를 [gold]소멸[/gold]시킵니다.
[gold]소멸[/gold]시킨 카드 1장당 무작위 적에게 피해를 8 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: RandomEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLAK_CANNON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FlakCannon`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/flak_cannon.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 대단원의 막

- ID: `megacrit-sts2-core-models-cards-grandfinale`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 카드가 없을 때만 사용할 수 있습니다. 모든 적에게 피해를 50 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Damage+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAND_FINALE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GrandFinale`
- 리소스 경로: `res://images/packed/card_portraits/silent/grand_finale.png7O`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 대령하라

- ID: `megacrit-sts2-core-models-cards-summonforth`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
어디에 있든 [gold]군주의 칼날[/gold]을 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUMMON_FORTH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SummonForth`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/summon_forth.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 대혼란

- ID: `megacrit-sts2-core-models-cards-mayhem`
- 그룹/풀 추정: 무색, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]뽑을 카드 더미[/gold] 맨 위의 카드를 사용합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAYHEM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Mayhem`
- 리소스 경로: `res://images/packed/card_portraits/colorless/mayhem.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 대화재

- ID: `megacrit-sts2-core-models-cards-conflagration`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 {CalculatedDamage:diff()} 줍니다.
이번 턴에 사용한 다른 공격 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: CalculationBase+1, ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CONFLAGRATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Conflagration`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/conflagration.pngIjE`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 덜그럭대기

- ID: `megacrit-sts2-core-models-cards-rattle`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
이번 턴 동안 골골이가 공격한 횟수만큼 반복합니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RATTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rattle`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/rattle.png|`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 덤벼라!

- ID: `megacrit-sts2-core-models-cards-fightme`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5만큼 2번 줍니다.
[gold]힘[/gold]을 2 얻습니다.
대상 적이 [gold]힘[/gold]을 {EnemyStrength:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+1, Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIGHT_ME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FightMe`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/fight_me.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 덮쳐!

- ID: `megacrit-sts2-core-models-cards-sicem`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
이번 턴에 [gold]골골이[/gold]가 이 적을 공격할 때마다, [gold]소환[/gold] 2.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SIC_EM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SicEm`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/sic_em.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 덮치기

- ID: `megacrit-sts2-core-models-cards-pounce`
- 그룹/풀 추정: 사일런트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 12 줍니다.
다음에 사용하는 스킬 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POUNCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Pounce`
- 리소스 경로: `res://images/packed/card_portraits/silent/pounce.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 도망칠 수 없다

- ID: `megacrit-sts2-core-models-cards-noescape`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]종말[/gold]을 {CalculationBase:diff()} 부여하고, 이 적에게 부여된 [gold]종말[/gold] {DoomThreshold:diff()}당, [gold]종말[/gold]을 추가로 {CalculationExtra:diff()} 부여합니다.{IsTargeting:
([gold]종말[/gold]을 {CalculatedDoom:diff()} 부여합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: CalculationBase+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NO_ESCAPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NoEscape`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/no_escape.pngu`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 도발

- ID: `megacrit-sts2-core-models-cards-taunt`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Block+1, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TAUNT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Taunt`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/taunt.png}`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 도약

- ID: `megacrit-sts2-core-models-cards-leap`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LEAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Leap`
- 리소스 경로: `res://images/packed/card_portraits/defect/leap.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 도탄

- ID: `megacrit-sts2-core-models-cards-ricochet`
- 그룹/풀 추정: 사일런트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 3만큼 {Repeat:diff()}번 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: RandomEnemy
  - 카드 풀: silent
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RICOCHET`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Ricochet`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/ricochet.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 독 바르기

- ID: `megacrit-sts2-core-models-cards-envenom`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격 카드가 막히지 않은 피해를 줄 때마다, [gold]중독[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENVENOM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Envenom`
- 리소스 경로: `res://images/packed/card_portraits/silent/envenom.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 독 찌르기

- ID: `megacrit-sts2-core-models-cards-poisonedstab`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
[gold]중독[/gold]을 3 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+2, Poison+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POISONED_STAB`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PoisonedStab`
- 리소스 경로: `res://images/packed/card_portraits/silent/poisoned_stab.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 독백

- ID: `megacrit-sts2-core-models-cards-monologue`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 카드를 사용할 때마다, 이번 턴 동안 [gold]힘[/gold]을 {Power:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MONOLOGUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Monologue`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/monologue.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 독재

- ID: `megacrit-sts2-core-models-cards-tyranny`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 카드를 1장 뽑고 [gold]손[/gold]에 있는 카드를 1장 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TYRANNY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tyranny`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/tyranny.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 돌 갑옷

- ID: `megacrit-sts2-core-models-cards-stonearmor`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]판금[/gold]을 4 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STONE_ARMOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StoneArmor`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/stone_armor.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 돌격!!!!

- ID: `megacrit-sts2-core-models-cards-charge`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 카드를 2장 선택해
[gold]{IfUpgraded:show:하수인 타격+|하수인 타격}[/gold]으로 [gold]변화[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CHARGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Charge`
- 리소스 경로: `res://images/packed/card_portraits/regent/charge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 돌진

- ID: `megacrit-sts2-core-models-cards-dash`
- 그룹/풀 추정: 사일런트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 10 얻습니다.
피해를 10 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3, Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dash`
- 리소스 경로: `res://images/packed/card_portraits/silent/dash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 동기화

- ID: `megacrit-sts2-core-models-cards-synchronize`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 영창된 구체의 종류 하나당 한 턴 동안 [gold]밀집[/gold]을 {CalculationExtra:diff()} 얻습니다.{InCombat:
([gold]밀집[/gold]을 {CalculatedFocus:diff()} 얻습니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SYNCHRONIZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Synchronize`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/synchronize.png}`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 동전기

- ID: `megacrit-sts2-core-models-cards-voltaic`
- 그룹/풀 추정: 디펙트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 전투 동안 [gold]영창[/gold]한 [gold]전기[/gold]의 수만큼 [gold]전기[/gold]를 [gold]영창[/gold]합니다.{InCombat:
([gold]전기[/gold]를 {CalculatedChannels:diff()}번 [gold]영창[/gold]합니다)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VOLTAIC`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Voltaic`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/voltaic.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 되돌리기

- ID: `megacrit-sts2-core-models-cards-rebound`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
이번 턴에 다음으로 사용하는 카드를 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Event
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REBOUND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rebound`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/rebound.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 두들겨 패기

- ID: `megacrit-sts2-core-models-cards-beatdown`
- 그룹/풀 추정: 무색, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버린 카드 더미[/gold]에서 무작위 공격 카드를 3장 사용합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Rare
  - 대상: RandomEnemy
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEAT_DOWN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BeatDown`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beat_down.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 두려움

- ID: `megacrit-sts2-core-models-cards-fear`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+1, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FEAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fear`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/fear.pngZ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 들춰내기

- ID: `megacrit-sts2-core-models-cards-expose`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 대상 적의 모든 [gold]인공물[/gold]과 [gold]방어도[/gold]를 제거합니다.
[gold]취약[/gold]을 {Power:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXPOSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Expose`
- 리소스 경로: `res://images/packed/card_portraits/silent/expose.png&Ve`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 땅고르기

- ID: `megacrit-sts2-core-models-cards-flatten`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
이번 턴에 [gold]골골이[/gold]가 공격했다면, 이 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLATTEN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Flatten`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/flatten.pngUE`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 때가 되었다

- ID: `megacrit-sts2-core-models-cards-timesup`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 대상 적이 보유한 [gold]종말[/gold]만큼 피해를 줍니다.{IsTargeting:
(피해를 {CalculatedDamage:diff()} 줍니다)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TIMES_UP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TimesUp`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/times_up.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 때려눕히기

- ID: `megacrit-sts2-core-models-cards-knockdown`
- 그룹/풀 추정: 무색, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
대상 적이 이번 턴에 다른 플레이어에게서 받는 피해량이 {IfUpgraded:show:3배|2배}가 됩니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KNOCKDOWN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Knockdown`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/knockdown.pngb`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 떨림

- ID: `megacrit-sts2-core-models-cards-tremble`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TREMBLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tremble`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/tremble.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 똘똘 뭉치기

- ID: `megacrit-sts2-core-models-cards-huddleup`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 아군이 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllAllies
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HUDDLE_UP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HuddleUp`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/huddle_up.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 랜턴 열쇠

- ID: `megacrit-sts2-core-models-cards-lanternkey`
- 그룹/풀 추정: Quest, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 막에서 특별한 이벤트를 해금합니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Quest
  - 희귀도: Quest
  - 대상: Self
  - 카드 풀: quest
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LANTERN_KEY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LanternKey`
- 리소스 경로: `res://images/packed/card_portraits/quest/beta/lantern_key.pngrJ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 레이저 포인터

- ID: `megacrit-sts2-core-models-cards-beamcell`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEAM_CELL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BeamCell`
- 리소스 경로: `res://images/packed/card_portraits/defect/beam_cell.pngj`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 로열티

- ID: `megacrit-sts2-core-models-cards-royalties`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 전투 종료 시, [gold]골드[/gold]를 30 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Gold+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROYALTIES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Royalties`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/royalties.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 로켓 펀치

- ID: `megacrit-sts2-core-models-cards-rocketpunch`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
카드를 1장 뽑습니다.
상태이상 카드를 생성 시, 이번 턴 동안 이 카드의 비용이 0 {energyPrefix:energyIcons(1)}으로 감소합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROCKET_PUNCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RocketPunch`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/rocket_punch.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 마름쇠

- ID: `megacrit-sts2-core-models-cards-caltrops`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격을 받을 때마다, 공격한 적에게 피해를 3 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALTROPS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Caltrops`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/caltrops.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 마무리

- ID: `megacrit-sts2-core-models-cards-finisher`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 사용한 공격 카드 1장당 피해를 6 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FINISHER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Finisher`
- 리소스 경로: `res://images/packed/card_portraits/silent/finisher.pngo`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 막아라!!

- ID: `megacrit-sts2-core-models-cards-guards`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드를 원하는 만큼 [gold]{IfUpgraded:show:하수인 희생+|하수인 희생}[/gold]으로 [gold]변화[/gold]시킵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 선택 프롬프트: [gold]변화[/gold]시킬 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GUARDS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Guards`
- 리소스 경로: `res://images/packed/card_portraits/regent/guards.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 만물 절단

- ID: `megacrit-sts2-core-models-cards-omnislice`
- 그룹/풀 추정: 무색, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
가한 피해량만큼 다른 모든 적에게 피해를 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OMNISLICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Omnislice`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/omnislice.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 망각

- ID: `megacrit-sts2-core-models-cards-oblivion`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 카드를 사용할 때마다, 대상 적에게 [gold]종말[/gold]을 3 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Doom+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OBLIVION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Oblivion`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/oblivion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 망치질 시간

- ID: `megacrit-sts2-core-models-cards-hammertime`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold]할 때마다, 모든 아군이 [gold]단조[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAMMER_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HammerTime`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/hammer_time.pngx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 망토와 단검

- ID: `megacrit-sts2-core-models-cards-cloakanddagger`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]단도[/gold]를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLOAK_AND_DAGGER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CloakAndDagger`
- 리소스 경로: `res://images/packed/card_portraits/silent/cloak_and_dagger.pngX`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 매달기

- ID: `megacrit-sts2-core-models-cards-hang`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
모든 매달기 카드가 이 적에게 가하는 피해량이 2배로 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HANG`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hang`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/hang.pngVt(]I`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 매료됨

- ID: `megacrit-sts2-core-models-cards-enthralled`
- 그룹/풀 추정: 저주, Curse, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 카드가 [gold]손[/gold]에 있다면, 다른 카드보다 먼저 사용해야 합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENTHRALLED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Enthralled`
- 리소스 경로: `res://images/packed/card_portraits/curse/beta/enthralled.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 매장

- ID: `megacrit-sts2-core-models-cards-bury`
- 그룹/풀 추정: Attack, 코스트 4
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 52 줍니다.
- 구조 정보:
  - 코스트: 4
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+11
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BURY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bury`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/bury.pngw4`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 맹독

- ID: `megacrit-sts2-core-models-cards-deadlypoison`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]중독[/gold]을 5 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Poison+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEADLY_POISON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DeadlyPoison`
- 리소스 경로: `res://images/packed/card_portraits/silent/deadly_poison.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 메멘토 모리

- ID: `megacrit-sts2-core-models-cards-mementomori`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
이번 턴에 버린 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: CalculationBase+2, ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MEMENTO_MORI`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MementoMori`
- 리소스 경로: `res://images/packed/card_portraits/silent/memento_mori.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 메아리 참격

- ID: `megacrit-sts2-core-models-cards-echoingslash`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 10 줍니다.
적을 처치할 때마다 이 효과를 반복합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ECHOING_SLASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EchoingSlash`
- 리소스 경로: `res://images/packed/card_portraits/silent/echoing_slash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 메아리의 형상

- ID: `megacrit-sts2-core-models-cards-echoform`
- 그룹/풀 추정: 디펙트, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 사용하는 카드가 1번 추가로 사용됩니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ECHO_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EchoForm`
- 리소스 경로: `res://images/packed/card_portraits/defect/echo_form.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 모독

- ID: `megacrit-sts2-core-models-cards-defile`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFILE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Defile`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/defile.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 모드 적용

- ID: `megacrit-sts2-core-models-cards-modded`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 구체 슬롯을 {Repeat:diff()}개 얻습니다.
카드를 1장 뽑습니다. 이 카드의 비용이 1 증가합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MODDED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Modded`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/modded.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 목 조르기

- ID: `megacrit-sts2-core-models-cards-strangle`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
이번 턴에 카드를 사용할 때마다, 대상 적이 체력을 2 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRANGLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Strangle`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/strangle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 몸통 박치기

- ID: `megacrit-sts2-core-models-cards-bodyslam`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]만큼의 피해를 줍니다.{InCombat:
(피해를 {CalculatedDamage:diff()} 줍니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BODY_SLAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BodySlam`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/body_slam.pngc`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 몽둥이질

- ID: `megacrit-sts2-core-models-cards-bludgeon`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 32 줍니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLUDGEON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bludgeon`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/bludgeon.pngP`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무감각

- ID: `megacrit-sts2-core-models-cards-feelnopain`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드가 [gold]소멸[/gold]될 때마다, [gold]방어도[/gold]를 {Power:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FEEL_NO_PAIN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FeelNoPain`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/feel_no_pain.pngI`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무기고

- ID: `megacrit-sts2-core-models-cards-arsenal`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무색 카드를 사용할 때마다, [gold]힘[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ARSENAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Arsenal`
- 리소스 경로: `res://images/packed/card_portraits/regent/arsenal.png2`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무덤 폭발

- ID: `megacrit-sts2-core-models-cards-graveblast`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 4 줍니다.
[gold]버린 카드 더미[/gold]에서 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAVEBLAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Graveblast`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/graveblast.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무덤지기

- ID: `megacrit-sts2-core-models-cards-gravewarden`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 8 얻습니다.
[gold]뽑을 카드 더미[/gold]에 [gold]{IfUpgraded:show:영혼+|영혼}[/gold]을 1장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAVE_WARDEN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GraveWarden`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/grave_warden.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무력화

- ID: `megacrit-sts2-core-models-cards-neutralize`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
[gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+1, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEUTRALIZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Neutralize`
- 리소스 경로: `res://images/packed/card_portraits/silent/neutralize.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무자비

- ID: `megacrit-sts2-core-models-cards-unrelenting`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 12 줍니다.
다음에 사용하는 공격 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNRELENTING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Unrelenting`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/unrelenting.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무적

- ID: `megacrit-sts2-core-models-cards-impervious`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 30 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `IMPERVIOUS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Impervious`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/impervious.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무지개

- ID: `megacrit-sts2-core-models-cards-rainbow`
- 그룹/풀 추정: 디펙트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]전기[/gold]를 1번 [gold]영창[/gold]합니다.
[gold]냉기[/gold]를 1번 [gold]영창[/gold]합니다.
[gold]암흑[/gold]을 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RAINBOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rainbow`
- 리소스 경로: `res://images/packed/card_portraits/defect/rainbow.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무한의 검날

- ID: `megacrit-sts2-core-models-cards-infiniteblades`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]단도[/gold]를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFINITE_BLADES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.InfiniteBlades`
- 리소스 경로: `res://images/packed/card_portraits/silent/infinite_blades.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 물렀거라!

- ID: `megacrit-sts2-core-models-cards-begone`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 4 줍니다.
[gold]손[/gold]에 있는 카드를 1장 선택해 [gold]{IfUpgraded:show:하수인 투하+|하수인 투하}[/gold]로 [gold]변화[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEGONE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Begone`
- 리소스 경로: `res://images/packed/card_portraits/regent/begone.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 미래 예지

- ID: `megacrit-sts2-core-models-cards-thinkingahead`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 뽑습니다.
[gold]손[/gold]에 있는 카드를 1장 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 선택 프롬프트: 뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THINKING_AHEAD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ThinkingAhead`
- 리소스 경로: `res://images/packed/card_portraits/colorless/thinking_ahead.pngb`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 밀집 타격

- ID: `megacrit-sts2-core-models-cards-focusedstrike`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
이번 턴에 [gold]밀집[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOCUSED_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FocusedStrike`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/focused_strike.png*)`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 바리케이드

- ID: `megacrit-sts2-core-models-cards-barricade`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시 [gold]방어도[/gold]가 사라지지 않습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BARRICADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Barricade`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/barricade.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 박멸

- ID: `megacrit-sts2-core-models-cards-exterminate`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 3만큼 {Repeat:diff()}번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Event
  - 대상: AllEnemies
  - 카드 풀: event
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXTERMINATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Exterminate`
- 리소스 경로: `res://images/packed/card_portraits/event/exterminate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 박살

- ID: `megacrit-sts2-core-models-cards-break`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 20 줍니다.
[gold]취약[/gold]을 5 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+5, Vulnerable+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BREAK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Break`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/break.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 박치기

- ID: `megacrit-sts2-core-models-cards-headbutt`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
[gold]버린 카드 더미[/gold]에서 카드를 1장 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 선택 프롬프트: 뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEADBUTT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Headbutt`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/headbutt.png#`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 반복

- ID: `megacrit-sts2-core-models-cards-loop`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 가장 오른쪽의 구체의 지속 능력을{IfUpgraded:show: 2번} 발동시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LOOP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Loop`
- 리소스 경로: `res://images/packed/card_portraits/defect/loop.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 반사

- ID: `megacrit-sts2-core-models-cards-reflect`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 17 얻습니다.
이번 턴에 막은 공격 피해는 공격한 적에게 반사됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFLECT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reflect`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/reflect.pngQ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 반사신경

- ID: `megacrit-sts2-core-models-cards-reflex`
- 그룹/풀 추정: 사일런트, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFLEX`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reflex`
- 리소스 경로: `res://images/packed/card_portraits/silent/reflex.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 반항

- ID: `megacrit-sts2-core-models-cards-defy`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Block+1, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Defy`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/defy.pngtk"`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발견

- ID: `megacrit-sts2-core-models-cards-discovery`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 카드 3장 중 1장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISCOVERY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Discovery`
- 리소스 경로: `res://images/packed/card_portraits/colorless/discovery.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발광

- ID: `megacrit-sts2-core-models-cards-luminesce`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Token
  - 대상: Self
  - 카드 풀: token
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LUMINESCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Luminesce`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/luminesce.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발놀림

- ID: `megacrit-sts2-core-models-cards-footwork`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]민첩[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Dexterity+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOOTWORK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Footwork`
- 리소스 경로: `res://images/packed/card_portraits/silent/footwork.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발병

- ID: `megacrit-sts2-core-models-cards-outbreak`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]중독[/gold]을 {Repeat:choose(1):부여할|{Repeat:diff()}번 부여할} 때마다, 모든 적에게 피해를 11 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OUTBREAK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Outbreak`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/outbreak.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 발화

- ID: `megacrit-sts2-core-models-cards-inflame`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFLAME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Inflame`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/inflame.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 방출

- ID: `megacrit-sts2-core-models-cards-radiate`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 {Stars:starIcons()}을 얻은 횟수만큼 모든 적에게 피해를 3 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RADIATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Radiate`
- 리소스 경로: `res://images/packed/card_portraits/regent/radiate.pngGa`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 방해

- ID: `megacrit-sts2-core-models-cards-distraction`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 스킬 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISTRACTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Distraction`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/distraction.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 배신

- ID: `megacrit-sts2-core-models-cards-backstab`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 11 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BACKSTAB`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Backstab`
- 리소스 경로: `res://images/packed/card_portraits/silent/backstab.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 배터리 충전

- ID: `megacrit-sts2-core-models-cards-chargebattery`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
다음 턴에, 1를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CHARGE_BATTERY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ChargeBattery`
- 리소스 경로: `res://images/packed/card_portraits/defect/charge_battery.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 백색 소음

- ID: `megacrit-sts2-core-models-cards-whitenoise`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 파워 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHITE_NOISE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.WhiteNoise`
- 리소스 경로: `res://images/packed/card_portraits/defect/white_noise.pngF`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 밴시의 외침

- ID: `megacrit-sts2-core-models-cards-bansheescry`
- 그룹/풀 추정: Attack, 코스트 6
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 33 줍니다.
이번 전투 동안 사용한 [gold]휘발성[/gold] 카드 1장당 비용이 2 감소합니다.
- 구조 정보:
  - 코스트: 6
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BANSHEES_CRY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BansheesCry`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/banshees_cry.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 뱀 물기

- ID: `megacrit-sts2-core-models-cards-snakebite`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]중독[/gold]을 7 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Poison+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SNAKEBITE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Snakebite`
- 리소스 경로: `res://images/packed/card_portraits/silent/snakebite.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 버퍼

- ID: `megacrit-sts2-core-models-cards-buffer`
- 그룹/풀 추정: 디펙트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음에 체력을 잃는 것을{BufferPower:choose(1):| 1번} 막아줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BUFFER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Buffer`
- 리소스 경로: `res://images/packed/card_portraits/defect/buffer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 번개 구체

- ID: `megacrit-sts2-core-models-cards-balllightning`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
[gold]전기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BALL_LIGHTNING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BallLightning`
- 리소스 경로: `res://images/packed/card_portraits/defect/ball_lightning.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 범접 불가

- ID: `megacrit-sts2-core-models-cards-untouchable`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNTOUCHABLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Untouchable`
- 리소스 경로: `res://images/packed/card_portraits/silent/untouchable.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 벼락

- ID: `megacrit-sts2-core-models-cards-thunder`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]전기[/gold]를 [gold]발현[/gold]할 때마다, 적중한 적에게 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THUNDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Thunder`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/thunder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 변형

- ID: `megacrit-sts2-core-models-cards-transfigure`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드 1장에 [gold]재사용[/gold]을 1 추가합니다.
그 카드의 비용이 1 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 선택 프롬프트: [gold]재사용[/gold]을 추가할 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRANSFIGURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Transfigure`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/transfigure.png|`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별똥별

- ID: `megacrit-sts2-core-models-cards-fallingstar`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
[gold]약화[/gold]를 1 부여합니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FALLING_STAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FallingStar`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/falling_star.png:`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별무리

- ID: `megacrit-sts2-core-models-cards-patter`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 8 얻습니다.
[gold]활력[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PATTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Patter`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/patter.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별의 망토

- ID: `megacrit-sts2-core-models-cards-cloakofstars`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLOAK_OF_STARS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CloakOfStars`
- 리소스 경로: `res://images/packed/card_portraits/regent/cloak_of_stars.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별의 아이

- ID: `megacrit-sts2-core-models-cards-childofthestars`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {singleStarIcon}을 소모할 때마다, 소모한 {singleStarIcon}당 [gold]방어도[/gold]를 {BlockForStars:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CHILD_OF_THE_STARS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ChildOfTheStars`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/child_of_the_stars.png{`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 별의 파동

- ID: `megacrit-sts2-core-models-cards-astralpulse`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 14 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ASTRAL_PULSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.AstralPulse`
- 리소스 경로: `res://images/packed/card_portraits/regent/astral_pulse.pngq`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 보루

- ID: `megacrit-sts2-core-models-cards-bulwark`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 13 얻습니다.
[gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3, Forge+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BULWARK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bulwark`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/bulwark.pngQ:{`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 보물 지도

- ID: `megacrit-sts2-core-models-cards-spoilsmap`
- 그룹/풀 추정: Quest, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 600 [gold]골드[/gold]를 추가로 얻을 수 있는 장소를 다음 막에 표시합니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Quest
  - 희귀도: Quest
  - 대상: Self
  - 카드 풀: quest
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPOILS_MAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SpoilsMap`
- 리소스 경로: `res://images/packed/card_portraits/quest/beta/spoils_map.pngV`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 봉인된 왕좌

- ID: `megacrit-sts2-core-models-cards-thesealedthrone`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 사용할 때마다, {singleStarIcon}을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_SEALED_THRONE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheSealedThrone`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/the_sealed_throne.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부름

- ID: `megacrit-sts2-core-models-cards-invoke`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에, [gold]소환[/gold] {Summon:diff()} 후 2를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+1, Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INVOKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Invoke`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/invoke.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부메랑 칼날

- ID: `megacrit-sts2-core-models-cards-swordboomerang`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 3만큼 {Repeat:diff()}번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: RandomEnemy
  - 카드 풀: ironclad
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SWORD_BOOMERANG`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SwordBoomerang`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/sword_boomerang.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부식

- ID: `megacrit-sts2-core-models-cards-putrefy`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]약화[/gold]을 {Power:diff()} 부여합니다.
[gold]취약[/gold]을 {Power:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PUTREFY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Putrefy`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/putrefy.pngj}*`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부식성 파도

- ID: `megacrit-sts2-core-models-cards-corrosivewave`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 카드를 뽑을 때마다, 모든 적에게 [gold]중독[/gold]을 {CorrosiveWave:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CORROSIVE_WAVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CorrosiveWave`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/corrosive_wave.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부양

- ID: `megacrit-sts2-core-models-cards-lift`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어에게 [gold]방어도[/gold]를 11 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: colorless
  - 강화 요약: Block+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LIFT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Lift`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/lift.png;`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부팅 과정

- ID: `megacrit-sts2-core-models-cards-bootsequence`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 10 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOOT_SEQUENCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BootSequence`
- 리소스 경로: `res://images/packed/card_portraits/defect/boot_sequence.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부패

- ID: `megacrit-sts2-core-models-cards-decay`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, 피해를 2 받습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DECAY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Decay`
- 리소스 경로: `res://images/packed/card_portraits/curse/decay.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 분노

- ID: `megacrit-sts2-core-models-cards-anger`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
이 카드의 복사본 1장을 [gold]버린 카드 더미[/gold]에 섞어 넣습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ANGER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Anger`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/anger.pngx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 분리

- ID: `megacrit-sts2-core-models-cards-severance`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
[gold]뽑을 카드 더미[/gold], [gold]손[/gold], [gold]버린 카드 더미[/gold]에 [gold]영혼[/gold]을 1장 추가합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SEVERANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Severance`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/severance.pngp`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 분석

- ID: `megacrit-sts2-core-models-cards-parse`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PARSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Parse`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/parse.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 분해

- ID: `megacrit-sts2-core-models-cards-disintegration`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시, 피해를 6 받습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: token
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISINTEGRATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Disintegration`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/disintegration.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불가피한 충돌

- ID: `megacrit-sts2-core-models-cards-collisioncourse`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
[gold]잔해[/gold]를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLLISION_COURSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CollisionCourse`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/collision_course.png'`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불릿 타임

- ID: `megacrit-sts2-core-models-cards-bullettime`
- 그룹/풀 추정: 사일런트, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 더 이상 카드를 뽑을 수 없습니다. 이번 턴 동안 [gold]손[/gold]에 있는 모든 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BULLET_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BulletTime`
- 리소스 경로: `res://images/packed/card_portraits/silent/bullet_time.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불바다

- ID: `megacrit-sts2-core-models-cards-inferno`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 체력을 1 잃습니다.
내 턴 동안 체력을 잃을 때마다, 모든 적에게 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFERNO`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Inferno`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/inferno.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불사

- ID: `megacrit-sts2-core-models-cards-undeath`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
이 카드의 복사본을 1장 [gold]버린 카드 더미[/gold]에 섞어 넣습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNDEATH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Undeath`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/undeath.png(`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불시착

- ID: `megacrit-sts2-core-models-cards-crashlanding`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 21 줍니다.
[gold]손[/gold]을 [gold]잔해[/gold]로 가득 채웁니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRASH_LANDING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CrashLanding`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/crash_landing.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불운

- ID: `megacrit-sts2-core-models-cards-badluck`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 손에 있다면, 체력을 {HpLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BAD_LUCK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BadLuck`
- 리소스 경로: `res://images/packed/card_portraits/curse/beta/bad_luck.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불의 심장

- ID: `megacrit-sts2-core-models-cards-pyre`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴 시작 시, 1를 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PYRE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Pyre`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/pyre.pngVa`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불의의 일격

- ID: `megacrit-sts2-core-models-cards-suckerpunch`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
[gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+2, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUCKER_PUNCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SuckerPunch`
- 리소스 경로: `res://images/packed/card_portraits/silent/sucker_punch.pngj`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불쾌

- ID: `megacrit-sts2-core-models-cards-malaise`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적이 [gold]힘[/gold]을 X{IfUpgraded:show:+1} 잃습니다. [gold]약화[/gold]를 X{IfUpgraded:show:+1} 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MALAISE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Malaise`
- 리소스 경로: `res://images/packed/card_portraits/silent/malaise.pngHr`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불타는 조약

- ID: `megacrit-sts2-core-models-cards-burningpact`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 [gold]소멸[/gold]시킵니다.
카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BURNING_PACT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BurningPact`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/burning_pact.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 블랙홀

- ID: `megacrit-sts2-core-models-cards-blackhole`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {singleStarIcon}을 소모하거나 얻을 때마다, 모든 적에게 피해를 3 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLACK_HOLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BlackHole`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/black_hole.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비관적인 맥박

- ID: `megacrit-sts2-core-models-cards-negativepulse`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
모든 적에게 [gold]종말[/gold]을 7 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: Block+1, Doom+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEGATIVE_PULSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NegativePulse`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/negative_pulse.pngq`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비밀 기술

- ID: `megacrit-sts2-core-models-cards-secrettechnique`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 스킬 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 선택 프롬프트: 손으로 가져올 스킬 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SECRET_TECHNIQUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SecretTechnique`
- 리소스 경로: `res://images/packed/card_portraits/colorless/secret_technique.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비밀 병기

- ID: `megacrit-sts2-core-models-cards-secretweapon`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 공격 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 선택 프롬프트: 손으로 가져올 공격 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SECRET_WEAPON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SecretWeapon`
- 리소스 경로: `res://images/packed/card_portraits/colorless/secret_weapon.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비밀 상자

- ID: `megacrit-sts2-core-models-cards-hiddencache`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Stars:starIcons()}을 얻습니다.
다음 턴에, 3을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HIDDEN_CACHE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HiddenCache`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/hidden_cache.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비상 단추

- ID: `megacrit-sts2-core-models-cards-panicbutton`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 30 얻습니다.
{Turns:diff()}턴 동안 카드를 통해 [gold]방어도[/gold]를 얻을 수 없습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Block+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PANIC_BUTTON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PanicButton`
- 리소스 경로: `res://images/packed/card_portraits/colorless/panic_button.pngw`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비애

- ID: `megacrit-sts2-core-models-cards-melancholy`
- 그룹/풀 추정: Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 13 얻습니다.
누군가 죽을 때마다 이 카드의 비용이 1 감소합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MELANCHOLY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Melancholy`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/melancholy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비열함

- ID: `megacrit-sts2-core-models-cards-sneaky`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어가 적을 공격할 때마다, [gold]방어도[/gold]를 1 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SNEAKY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Sneaky`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/sneaky.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비참함

- ID: `megacrit-sts2-core-models-cards-misery`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
대상 적에게 부여된 모든 해로운 효과를 다른 모든 적에게 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MISERY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Misery`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/misery.pngM`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 비책

- ID: `megacrit-sts2-core-models-cards-upmysleeve`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단도[/gold]를 3장 [gold]손[/gold]으로 가져옵니다.
이 카드의 비용이 1 감소합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UP_MY_SLEEVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.UpMySleeve`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/up_my_sleeve.pngd`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빅뱅

- ID: `megacrit-sts2-core-models-cards-bigbang`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 뽑습니다.
1를 얻습니다.
{Stars:starIcons()}을 얻습니다.
[gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BIG_BANG`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BigBang`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/big_bang.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빙하

- ID: `megacrit-sts2-core-models-cards-glacier`
- 그룹/풀 추정: 디펙트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]냉기[/gold]를 2번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLACIER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Glacier`
- 리소스 경로: `res://images/packed/card_portraits/defect/glacier.png+`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빚

- ID: `megacrit-sts2-core-models-cards-debt`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, [gold]골드[/gold]를 10 잃습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEBT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Debt`
- 리소스 경로: `res://images/packed/card_portraits/curse/debt.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛 거두기

- ID: `megacrit-sts2-core-models-cards-gatherlight`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
{Stars:starIcons()}을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GATHER_LIGHT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GatherLight`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/gather_light.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛나는 타격

- ID: `megacrit-sts2-core-models-cards-shiningstrike`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
{Stars:starIcons()}을 얻습니다.
[gold]뽑을 카드 더미[/gold] 맨 위에 이 카드를 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHINING_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ShiningStrike`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/shining_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛의 흐름

- ID: `megacrit-sts2-core-models-cards-glitterstream`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 11 얻습니다.
다음 턴에, [gold]방어도[/gold]를 {BlockNextTurn:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLITTERSTREAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Glitterstream`
- 리소스 경로: `res://src/Core/Models/Cards/Glitterstream.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 뼛조각

- ID: `megacrit-sts2-core-models-cards-boneshards`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 살아있다면, [gold]골골이[/gold]가 모든 적에게 피해를 {OstyDamage:diff()} 주고 내가 방어도를 9 얻습니다.
[gold]골골이[/gold]가 죽습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+3, Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BONE_SHARDS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BoneShards`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/bone_shards.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사냥

- ID: `megacrit-sts2-core-models-cards-thehunt`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]치명타[/gold]라면, 카드 보상을 추가로 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_HUNT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheHunt`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/the_hunt.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사냥돌

- ID: `megacrit-sts2-core-models-cards-bolas`
- 그룹/풀 추정: 무색, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
다음 턴 시작 시, 이 카드를 [gold]손[/gold]으로 다시 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOLAS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bolas`
- 리소스 경로: `res://images/packed/card_portraits/colorless/bolas.png+D`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사신의 낫

- ID: `megacrit-sts2-core-models-cards-thescythe`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
이 카드의 피해량이 영구적으로 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_SCYTHE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheScythe`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/the_scythe.png#`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사신의 형상

- ID: `megacrit-sts2-core-models-cards-reaperform`
- 그룹/풀 추정: Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격 카드로 피해를 줄 때마다, 그와 동일한 만큼의 [gold]종말[/gold]을 부여합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REAPER_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ReaperForm`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/reaper_form.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사전 타격

- ID: `megacrit-sts2-core-models-cards-setupstrike`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
이번 턴 동안 [gold]힘[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2, Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SETUP_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SetupStrike`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/setup_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사중 시전

- ID: `megacrit-sts2-core-models-cards-quadcast`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 가장 오른쪽의 구체를 {Repeat:diff()}번 [gold]발현[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `QUADCAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Quadcast`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/quadcast.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사혈

- ID: `megacrit-sts2-core-models-cards-bloodletting`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
2를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLOODLETTING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bloodletting`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/bloodletting.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 산산조각

- ID: `megacrit-sts2-core-models-cards-shatter`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 11 줍니다.
모든 구체를 [gold]발현[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: defect
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHATTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shatter`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/shatter.pngu`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 살점 재주

- ID: `megacrit-sts2-core-models-cards-sleightofflesh`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적에게 해로운 효과를 부여할 때마다, 대상 적이 피해를 9 받습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLEIGHT_OF_FLESH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SleightOfFlesh`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/sleight_of_flesh.png``
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 살해

- ID: `megacrit-sts2-core-models-cards-murder`
- 그룹/풀 추정: 사일런트, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
이번 전투 동안 뽑은 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MURDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Murder`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/murder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 생명 삼키기

- ID: `megacrit-sts2-core-models-cards-devourlife`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]영혼[/gold]을 사용할 때마다, [gold]소환[/gold] 1.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEVOUR_LIFE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DevourLife`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/devour_life.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 생산

- ID: `megacrit-sts2-core-models-cards-production`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PRODUCTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Production`
- 리소스 경로: `res://images/packed/card_portraits/colorless/production.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 생존자

- ID: `megacrit-sts2-core-models-cards-survivor`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 8 얻습니다.
카드를 1장 버립니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SURVIVOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Survivor`
- 리소스 경로: `res://images/packed/card_portraits/silent/survivor.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 섀 급습

- ID: `megacrit-sts2-core-models-cards-byrdswoop`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 14 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Event
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BYRD_SWOOP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ByrdSwoop`
- 리소스 경로: `res://images/packed/card_portraits/event/byrd_swoop.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 섀도니스 알

- ID: `megacrit-sts2-core-models-cards-byrdonisegg`
- 그룹/풀 추정: Quest, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]휴식 장소[/gold]에서 부화시킬 수 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Quest
  - 희귀도: Quest
  - 대상: None
  - 카드 풀: quest
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BYRDONIS_EGG`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ByrdonisEgg`
- 리소스 경로: `res://images/packed/card_portraits/quest/byrdonis_egg.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 서류 폭풍

- ID: `megacrit-sts2-core-models-cards-pagestorm`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]휘발성[/gold] 카드를 뽑을 때마다, 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PAGESTORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Pagestorm`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/pagestorm.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 서브루틴

- ID: `megacrit-sts2-core-models-cards-subroutine`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 파워 카드를 사용할 때마다, {energyPrefix:energyIcons(1)}를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUBROUTINE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Subroutine`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/subroutine.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 석회화

- ID: `megacrit-sts2-core-models-cards-calcify`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 가하는 공격의 피해량이 4 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALCIFY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Calcify`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/calcify.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 선제 타격

- ID: `megacrit-sts2-core-models-cards-leadingstrike`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
[gold]단도[/gold]를 {Shivs:diff()}장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LEADING_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LeadingStrike`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/leading_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 선조의 망치

- ID: `megacrit-sts2-core-models-cards-heirloomhammer`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 17 줍니다.
[gold]손[/gold]에 있는 무색 카드를 1장 선택합니다. 그 카드의 복사본을 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+5
- 선택 프롬프트: 무색 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEIRLOOM_HAMMER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HeirloomHammer`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/heirloom_hammer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 설계의 대가

- ID: `megacrit-sts2-core-models-cards-masterplanner`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 스킬 카드를 사용 시, 그 카드가 [gold]교활[/gold]을 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MASTER_PLANNER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MasterPlanner`
- 리소스 경로: `res://images/packed/card_portraits/silent/master_planner.pnga`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 성급함

- ID: `megacrit-sts2-core-models-cards-impatience`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 공격 카드가 없다면, 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `IMPATIENCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Impatience`
- 리소스 경로: `res://images/packed/card_portraits/colorless/impatience.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 성대한 도박

- ID: `megacrit-sts2-core-models-cards-royalgamble`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {singleStarIcon}을 {Stars:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROYAL_GAMBLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RoyalGamble`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/royal_gamble.pngnm`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 소생

- ID: `megacrit-sts2-core-models-cards-reanimate`
- 그룹/풀 추정: Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REANIMATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reanimate`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/reanimate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 소용돌이

- ID: `megacrit-sts2-core-models-cards-whirlwind`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 5만큼 X번 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHIRLWIND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Whirlwind`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/whirlwind.pngI$mwq`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 손기술

- ID: `megacrit-sts2-core-models-cards-handtrick`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
이번 턴 동안 [gold]손[/gold]에 있는 스킬 카드 1장에 [gold]교활[/gold]을 추가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 선택 프롬프트: [gold]교활[/gold]을 추가할 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAND_TRICK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HandTrick`
- 리소스 경로: `res://images/packed/card_portraits/silent/hand_trick.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쇄도

- ID: `megacrit-sts2-core-models-cards-stampede`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시, [gold]손[/gold]에 있는 무작위 공격 카드 {Power:diff()}장이 무작위 적에게 사용됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STAMPEDE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stampede`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/stampede.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쇠락

- ID: `megacrit-sts2-core-models-cards-debilitate`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
다음 3턴 동안 이 적을 대상으로 하는 [gold]취약[/gold]과 [gold]약화[/gold]의 효과가 2배가 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEBILITATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Debilitate`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/debilitate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쇠약의 손길

- ID: `megacrit-sts2-core-models-cards-enfeeblingtouch`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 적이 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENFEEBLING_TOUCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EnfeeblingTouch`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/enfeebling_touch.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쇠퇴

- ID: `megacrit-sts2-core-models-cards-wasteaway`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 얻는 {energyPrefix:energyIcons(1)}가 1 감소합니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: token
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WASTE_AWAY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.WasteAway`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/waste_away.png]`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수렴

- ID: `megacrit-sts2-core-models-cards-convergence`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에,
1와 {Stars:starIcons()}을 얻습니다.
이번 턴에 [gold]손[/gold]에 있는 카드를 [gold]보존[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Stars+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CONVERGENCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Convergence`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/convergence.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수비

- ID: `megacrit-sts2-core-models-cards-defenddefect`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFEND_DEFECT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DefendDefect`
- 리소스 경로: `res://images/packed/card_portraits/defect/defend_defect.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수비

- ID: `megacrit-sts2-core-models-cards-defendironclad`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFEND_IRONCLAD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DefendIronclad`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/defend_ironclad.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수비

- ID: `megacrit-sts2-core-models-cards-defendnecrobinder`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFEND_NECROBINDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DefendNecrobinder`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/defend_necrobinder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수비

- ID: `megacrit-sts2-core-models-cards-defendregent`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFEND_REGENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DefendRegent`
- 리소스 경로: `res://images/packed/card_portraits/regent/defend_regent.pngV:`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수비

- ID: `megacrit-sts2-core-models-cards-defendsilent`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFEND_SILENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DefendSilent`
- 리소스 경로: `res://src/Core/Models/Cards/DefendSilent.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수의

- ID: `megacrit-sts2-core-models-cards-shroud`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]종말[/gold]을 부여할 때마다, [gold]방어도[/gold]를 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHROUD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shroud`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/shroud.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수치

- ID: `megacrit-sts2-core-models-cards-shame`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, [gold]손상[/gold]을 {Frail:diff()} 얻습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHAME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shame`
- 리소스 경로: `res://images/packed/card_portraits/curse/shame.png?`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수확

- ID: `megacrit-sts2-core-models-cards-reap`
- 그룹/풀 추정: Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 27 줍니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Reap`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/reap.png35E`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 순수

- ID: `megacrit-sts2-core-models-cards-purity`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드를 최대 3장까지 소멸시킵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+2
- 선택 프롬프트: [gold]소멸[/gold]시킬 카드를 최대 3장까지 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PURITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Purity`
- 리소스 경로: `res://images/packed/card_portraits/colorless/purity.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 순회

- ID: `megacrit-sts2-core-models-cards-iteration`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 상태이상 카드를 뽑을 시, 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ITERATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Iteration`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/iteration.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 숨겨진 단검

- ID: `megacrit-sts2-core-models-cards-hiddendaggers`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 버립니다.
[gold]{IfUpgraded:show:단도+|단도}[/gold]를 {Shivs:diff()}장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HIDDEN_DAGGERS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HiddenDaggers`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/hidden_daggers.pngN`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 숨겨진 보석

- ID: `megacrit-sts2-core-models-cards-hiddengem`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 있는 무작위 카드 1장이 [gold]재사용[/gold]을 {Replay:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HIDDEN_GEM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HiddenGem`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/hidden_gem.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스택

- ID: `megacrit-sts2-core-models-cards-stack`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버린 카드 더미[/gold]에 있는 카드의 수{IfUpgraded:show: +{CalculationBase}|}만큼 [gold]방어도[/gold]를 얻습니다.{InCombat:
([gold]방어도[/gold]를 {CalculatedBlock:diff()} 얻습니다.)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: CalculationBase+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STACK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stack`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/stack.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스펙트럼 이동

- ID: `megacrit-sts2-core-models-cards-spectrumshift`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 무색 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPECTRUM_SHIFT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SpectrumShift`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/spectrum_shift.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스피너

- ID: `megacrit-sts2-core-models-cards-spinner`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {IfUpgraded:show:[gold]유리[/gold]를 1번 [gold]영창[/gold]합니다.
|}내 턴 시작 시, [gold]유리[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPINNER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Spinner`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/spinner.pngk`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스피드스터

- ID: `megacrit-sts2-core-models-cards-speedster`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 동안 카드를 뽑을 때마다, 모든 적에게 피해를 2 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPEEDSTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Speedster`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/speedster.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 시선 유도

- ID: `megacrit-sts2-core-models-cards-pullaggro`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
[gold]방어도[/gold]를 7 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+1, Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PULL_AGGRO`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PullAggro`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/pull_aggro.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 신기루

- ID: `megacrit-sts2-core-models-cards-mirage`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 부여된 [gold]중독[/gold]과 동일한 만큼의 [gold]방어도[/gold]를 얻습니다.{InCombat:
([gold]방어도[/gold]를 {CalculatedBlock:diff()} 얻습니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MIRAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Mirage`
- 리소스 경로: `res://images/packed/card_portraits/silent/mirage.pngH`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 신성

- ID: `megacrit-sts2-core-models-cards-apotheosis`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 카드를 [gold]강화[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `APOTHEOSIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Apotheosis`
- 리소스 경로: `res://images/packed/card_portraits/event/apotheosis.pngdB`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 신호 증폭

- ID: `megacrit-sts2-core-models-cards-signalboost`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음에 사용하는 파워 카드가 1번 추가로 사용됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SIGNAL_BOOST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SignalBoost`
- 리소스 경로: `res://images/packed/card_portraits/defect/signal_boost.pngM|`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 싸움 준비

- ID: `megacrit-sts2-core-models-cards-expectafight`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 공격 카드 1장당 {energyPrefix:energyIcons(1)}를 얻습니다.{InCombat:
({CalculatedEnergy:energyIcons()}를 얻습니다.)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXPECT_A_FIGHT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ExpectAFight`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/expect_a_fight.png^S`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쑤시기

- ID: `megacrit-sts2-core-models-cards-poke`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Poke`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/poke.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 아드레날린

- ID: `megacrit-sts2-core-models-cards-adrenaline`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다.
카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ADRENALINE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Adrenaline`
- 리소스 경로: `res://images/packed/card_portraits/silent/adrenaline.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 아지랑이

- ID: `megacrit-sts2-core-models-cards-haze`
- 그룹/풀 추정: 사일런트, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 [gold]중독[/gold]을 4 부여합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Poison+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Haze`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/haze.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악랄함

- ID: `megacrit-sts2-core-models-cards-cruelty`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]취약[/gold] 상태의 적이 받는 피해가 25% 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRUELTY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Cruelty`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/cruelty.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악마의 눈

- ID: `megacrit-sts2-core-models-cards-evileye`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 8 얻습니다.
이번 턴 동안 [gold]소멸[/gold]시킨 카드가 있다면 [gold]방어도[/gold]를 추가로 8 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EVIL_EYE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EvilEye`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/evil_eye.pngv`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악마의 방패

- ID: `megacrit-sts2-core-models-cards-demonicshield`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
다른 플레이어에게 자신의 [gold]방어도[/gold]와 동일한 만큼의 [gold]방어도[/gold]를 줍니다.{InCombat:
([gold]방어도[/gold]를 {CalculatedBlock:diff()} 줍니다)|}
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEMONIC_SHIELD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DemonicShield`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/demonic_shield.png2`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악마의 형상

- ID: `megacrit-sts2-core-models-cards-demonform`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]힘[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEMON_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DemonForm`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/demon_form.pngf`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악몽

- ID: `megacrit-sts2-core-models-cards-nightmare`
- 그룹/풀 추정: 사일런트, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 선택합니다.
다음 턴에, 그 카드의 복사본을 3장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 선택 프롬프트: 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NIGHTMARE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Nightmare`
- 리소스 경로: `res://images/packed/card_portraits/silent/nightmare.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 악의

- ID: `megacrit-sts2-core-models-cards-spite`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
이번 턴 동안 체력을 잃었다면, 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPITE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Spite`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/spite.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 안식

- ID: `megacrit-sts2-core-models-cards-relax`
- 그룹/풀 추정: Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 15 얻습니다.
다음 턴에, 카드를 2장 뽑고 2를 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Block+2, Cards+1, Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RELAX`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Relax`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/relax.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 암살

- ID: `megacrit-sts2-core-models-cards-assassinate`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ASSASSINATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Assassinate`
- 리소스 경로: `res://images/packed/card_portraits/silent/assassinate.png&?`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 압도

- ID: `megacrit-sts2-core-models-cards-outmaneuver`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에, 2를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OUTMANEUVER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Outmaneuver`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/outmaneuver.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 압축

- ID: `megacrit-sts2-core-models-cards-compact`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]손[/gold]에 있는 모든 상태이상 카드를 [gold]{IfUpgraded:show:연료+|연료}[/gold]로 [gold]변화[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COMPACT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Compact`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/compact.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 야성

- ID: `megacrit-sts2-core-models-cards-feral`
- 그룹/풀 추정: 디펙트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 비용이 0{energyPrefix:energyIcons(1)}인
공격 카드를{FeralPower:choose(1):| 1장} 사용 시,
그 카드를 [gold]손[/gold]으로 다시 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FERAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Feral`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/feral.pngn`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어둠

- ID: `megacrit-sts2-core-models-cards-darkness`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]암흑[/gold]을 1번 [gold]영창[/gold]합니다.
모든 [gold]암흑[/gold] 구체의 지속 효과를{IfUpgraded:show: 2번|} 발동시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DARKNESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Darkness`
- 리소스 경로: `res://images/packed/card_portraits/defect/darkness.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어둠의 족쇄

- ID: `megacrit-sts2-core-models-cards-darkshackles`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 적이 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DARK_SHACKLES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DarkShackles`
- 리소스 경로: `res://images/packed/card_portraits/colorless/dark_shackles.pngD`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어둠의 포옹

- ID: `megacrit-sts2-core-models-cards-darkembrace`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드가 [gold]소멸[/gold]될 때마다,
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DARK_EMBRACE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DarkEmbrace`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/dark_embrace.png}$`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어려운 결정

- ID: `megacrit-sts2-core-models-cards-decisionsdecisions`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
[gold]손[/gold]에 있는 스킬 카드를 선택해 {Repeat:diff()}번 사용합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+2
- 선택 프롬프트: 스킬 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DECISIONS_DECISIONS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DecisionsDecisions`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/decisions_decisions.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어퍼컷

- ID: `megacrit-sts2-core-models-cards-uppercut`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
[gold]약화[/gold]을 {Power:diff()} 부여합니다.
[gold]취약[/gold]을 {Power:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UPPERCUT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Uppercut`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/uppercut.png_`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 얼음 창

- ID: `megacrit-sts2-core-models-cards-icelance`
- 그룹/풀 추정: 디펙트, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 19 줍니다.
[gold]냉기[/gold]를 {Repeat:diff()}번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ICE_LANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.IceLance`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/ice_lance.pnghCN5`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 에너자이저

- ID: `megacrit-sts2-core-models-cards-doubleenergy`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 에너지가 2배가 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOUBLE_ENERGY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DoubleEnergy`
- 리소스 경로: `res://images/packed/card_portraits/defect/double_energy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 에너지 폭주

- ID: `megacrit-sts2-core-models-cards-energysurge`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 플레이어가 2를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllAllies
  - 카드 풀: defect
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENERGY_SURGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EnergySurge`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/energy_surge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 엔트로피

- ID: `megacrit-sts2-core-models-cards-entropy`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]손[/gold]에 있는 카드를 1장 [gold]변화[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENTROPY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Entropy`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/entropy.png~`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 역병 타격

- ID: `megacrit-sts2-core-models-cards-blightstrike`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
가한 피해량만큼 [gold]종말[/gold]을 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLIGHT_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BlightStrike`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/blight_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연금

- ID: `megacrit-sts2-core-models-cards-alchemize`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 포션을 1개 생성합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ALCHEMIZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Alchemize`
- 리소스 경로: `res://images/packed/card_portraits/colorless/alchemize.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연료

- ID: `megacrit-sts2-core-models-cards-fuel`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Token
  - 대상: Self
  - 카드 풀: token
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FUEL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fuel`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/fuel.png@pF`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연마

- ID: `megacrit-sts2-core-models-cards-abrasive`
- 그룹/풀 추정: 사일런트, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]민첩[/gold]을 1 얻습니다.
[gold]가시[/gold]를 4 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ABRASIVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Abrasive`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/abrasive.pngd`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연명

- ID: `megacrit-sts2-core-models-cards-borrowedtime`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 자신에게 [gold]종말[/gold]을 3 부여합니다.
1를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BORROWED_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BorrowedTime`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/borrowed_time.pnge#`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연쇄

- ID: `megacrit-sts2-core-models-cards-cascade`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold] 맨 위의 무작위 카드를 X{IfUpgraded:show:+1}장 사용합니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CASCADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Cascade`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/cascade.pngT`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 연장

- ID: `megacrit-sts2-core-models-cards-prolong`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에, 현재 [gold]방어도[/gold]와 동일한 만큼의 [gold]방어도[/gold]를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PROLONG`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Prolong`
- 리소스 경로: `res://images/packed/card_portraits/colorless/prolong.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 염원

- ID: `megacrit-sts2-core-models-cards-wish`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: event
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WISH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Wish`
- 리소스 경로: `res://images/packed/card_portraits/event/wish.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영원의 갑옷

- ID: `megacrit-sts2-core-models-cards-eternalarmor`
- 그룹/풀 추정: 무색, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]판금[/gold]을 7 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ETERNAL_ARMOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EternalArmor`
- 리소스 경로: `res://images/packed/card_portraits/colorless/eternal_armor.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영체화

- ID: `megacrit-sts2-core-models-cards-apparition`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]불가침[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `APPARITION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Apparition`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/apparition.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영혼

- ID: `megacrit-sts2-core-models-cards-soul`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Token
  - 대상: Self
  - 카드 풀: token
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOUL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Soul`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/soul.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영혼 폭풍

- ID: `megacrit-sts2-core-models-cards-soulstorm`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
[gold]소멸된 카드 더미[/gold]에 있는 [gold]영혼[/gold] 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOUL_STORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SoulStorm`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/soul_storm.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 예비

- ID: `megacrit-sts2-core-models-cards-prepared`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 뽑습니다.
카드를 1장 버립니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PREPARED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Prepared`
- 리소스 경로: `res://src/Core/Models/Cards/Prepared.cs<8`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 예언

- ID: `megacrit-sts2-core-models-cards-prophesize`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 6장 뽑습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PROPHESIZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Prophesize`
- 리소스 경로: `res://images/packed/card_portraits/regent/prophesize.pngb`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 예측

- ID: `megacrit-sts2-core-models-cards-anticipate`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 [gold]민첩[/gold]을 3 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Dexterity+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ANTICIPATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Anticipate`
- 리소스 경로: `res://images/packed/card_portraits/silent/anticipate.png$*<e`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 오버클럭

- ID: `megacrit-sts2-core-models-cards-overclock`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 뽑습니다.
[gold]버린 카드 더미[/gold]에 [gold]화상[/gold]을 1장 추가합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OVERCLOCK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Overclock`
- 리소스 경로: `res://images/packed/card_portraits/defect/overclock.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 오한

- ID: `megacrit-sts2-core-models-cards-chill`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적 하나당 [gold]냉기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CHILL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Chill`
- 리소스 경로: `res://images/packed/card_portraits/defect/chill.png[`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 완벽한 타격

- ID: `megacrit-sts2-core-models-cards-perfectedstrike`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
보유한 카드 중 이름에 “타격”이 포함된 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PERFECTED_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/perfected_strike.pngT`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 완수

- ID: `megacrit-sts2-core-models-cards-followthrough`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 6 줍니다.
이번 턴에 가장 최근에 사용한 카드가 스킬 카드라면, 모든 적에게 [gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Damage+2, Weak+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOLLOW_THROUGH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FollowThrough`
- 리소스 경로: `res://images/packed/card_portraits/silent/follow_through.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 왕의 발차기

- ID: `megacrit-sts2-core-models-cards-kinglykick`
- 그룹/풀 추정: Attack, 코스트 4
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 24 줍니다.
이 카드를 뽑을 때마다, 이 카드의 비용이 1 감소합니다.
- 구조 정보:
  - 코스트: 4
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KINGLY_KICK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.KinglyKick`
- 리소스 경로: `res://images/packed/card_portraits/regent/kingly_kick.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 왕의 주먹

- ID: `megacrit-sts2-core-models-cards-kinglypunch`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
이 카드를 뽑을 때마다, 이번 전투 동안 이 카드의 피해량이 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KINGLY_PUNCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.KinglyPunch`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/kingly_punch.pngO3`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 요지부동

- ID: `megacrit-sts2-core-models-cards-unmovable`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 카드를 통해 얻는 [gold]방어도[/gold]가 2배가 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNMOVABLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Unmovable`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/unmovable.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 용광로

- ID: `megacrit-sts2-core-models-cards-furnace`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FURNACE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Furnace`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/furnace.png<`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 우박 폭풍

- ID: `megacrit-sts2-core-models-cards-hailstorm`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 [gold]냉기[/gold]를 보유하고 있다면, 모든 적에게 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAILSTORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hailstorm`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/hailstorm.pngpv`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 우정

- ID: `megacrit-sts2-core-models-cards-friendship`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 2 잃습니다.
매 턴 시작 시 1를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FRIENDSHIP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Friendship`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/friendship.png0gz`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 우주 먼지

- ID: `megacrit-sts2-core-models-cards-stardust`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 5만큼 X번 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: RandomEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STARDUST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stardust`
- 리소스 경로: `res://images/packed/card_portraits/regent/stardust.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 우주적 무관심

- ID: `megacrit-sts2-core-models-cards-cosmicindifference`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
[gold]버린 카드 더미[/gold]의 카드 1장을 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 선택 프롬프트: 뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COSMIC_INDIFFERENCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CosmicIndifference`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/cosmic_indifference.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 운명 공유

- ID: `megacrit-sts2-core-models-cards-sharedfate`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]힘[/gold]을 {PlayerStrengthLoss:diff()} 잃습니다.
적이 [gold]힘[/gold]을 {EnemyStrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHARED_FATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SharedFate`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/shared_fate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 원시의 힘

- ID: `megacrit-sts2-core-models-cards-primalforce`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 공격 카드를 [gold]{IfUpgraded:show:거대한 바위+|거대한 바위}[/gold]로 변화시킵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PRIMAL_FORCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PrimalForce`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/primal_force.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 원투 펀치

- ID: `megacrit-sts2-core-models-cards-onetwopunch`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에, 다음에 사용하는 {Attacks:cond:>1?공격 카드 {Attacks:diff()}장이|공격 카드가} 1번 추가로 사용됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ONE_TWO_PUNCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.OneTwoPunch`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/one_two_punch.pngDe`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 위대한 재련

- ID: `megacrit-sts2-core-models-cards-thesmith`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_SMITH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheSmith`
- 리소스 경로: `res://images/packed/card_portraits/regent/the_smith.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 위습

- ID: `megacrit-sts2-core-models-cards-wisp`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WISP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Wisp`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/wisp.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 위풍당당

- ID: `megacrit-sts2-core-models-cards-panache`
- 그룹/풀 추정: 무색, Power, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한 턴에 카드를 5장 사용할 때마다, 모든 적에게 피해를 {PanacheDamage:diff()} 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PANACHE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Panache`
- 리소스 경로: `res://images/packed/card_portraits/colorless/panache.pngb`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유독 가스

- ID: `megacrit-sts2-core-models-cards-noxiousfumes`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 모든 적에게 [gold]중독[/gold]을 {PoisonPerTurn:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NOXIOUS_FUMES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NoxiousFumes`
- 리소스 경로: `res://images/packed/card_portraits/silent/noxious_fumes.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유독성

- ID: `megacrit-sts2-core-models-cards-toxic`
- 그룹/풀 추정: Status, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, 피해를 5 받습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TOXIC`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Toxic`
- 리소스 경로: `res://images/packed/card_portraits/status/toxic.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유령의 형상

- ID: `megacrit-sts2-core-models-cards-wraithform`
- 그룹/풀 추정: 사일런트, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]불가침[/gold]을 2 얻습니다.
내 턴 시작 시, [gold]민첩[/gold]을 1 잃습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WRAITH_FORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.WraithForm`
- 리소스 경로: `res://images/packed/card_portraits/silent/wraith_form.png+`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유리 공예

- ID: `megacrit-sts2-core-models-cards-glasswork`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
[gold]유리[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLASSWORK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Glasswork`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/glasswork.png>`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유성 타격

- ID: `megacrit-sts2-core-models-cards-meteorstrike`
- 그룹/풀 추정: 디펙트, Attack, 코스트 5
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 24 줍니다.
[gold]플라즈마[/gold]를 3번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 5
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `METEOR_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MeteorStrike`
- 리소스 경로: `res://images/packed/card_portraits/defect/meteor_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유성우

- ID: `megacrit-sts2-core-models-cards-meteorshower`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 14 줍니다.
모든 적에게 [gold]약화[/gold]와 [gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+7
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `METEOR_SHOWER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MeteorShower`
- 리소스 경로: `res://images/packed/card_portraits/regent/meteor_shower.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유인

- ID: `megacrit-sts2-core-models-cards-beckon`
- 그룹/풀 추정: Status, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면,
체력을 {HpLoss} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BECKON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Beckon`
- 리소스 경로: `res://images/packed/card_portraits/status/beckon.pngj`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유전 알고리즘

- ID: `megacrit-sts2-core-models-cards-geneticalgorithm`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 {Block:diff()} 얻습니다.
이 카드로 얻는 [gold]방어도[/gold]가 영구적으로 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GENETIC_ALGORITHM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GeneticAlgorithm`
- 리소스 경로: `res://images/packed/card_portraits/defect/genetic_algorithm.pngEE`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 융합

- ID: `megacrit-sts2-core-models-cards-fusion`
- 그룹/풀 추정: 디펙트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]플라즈마[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FUSION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fusion`
- 리소스 경로: `res://images/packed/card_portraits/defect/fusion.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 의심

- ID: `megacrit-sts2-core-models-cards-doubt`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, [gold]약화[/gold]를 1 얻습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOUBT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Doubt`
- 리소스 경로: `res://images/packed/card_portraits/curse/doubt.pngO9E`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이도류

- ID: `megacrit-sts2-core-models-cards-dualwield`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격이나 파워 카드를 1장 선택합니다. 그 카드의 복사본을 {IfUpgraded:show:1장|1장} [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Cards+1
- 선택 프롬프트: 복사할 카드를 선택하세요
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DUAL_WIELD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DualWield`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/dual_wield.pngC`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이중 시전

- ID: `megacrit-sts2-core-models-cards-dualcast`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 가장 오른쪽의 구체를 2번 [gold]발현[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DUALCAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dualcast`
- 리소스 경로: `res://images/packed/card_portraits/defect/dualcast.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이중 타격

- ID: `megacrit-sts2-core-models-cards-twinstrike`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5만큼 2번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TWIN_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TwinStrike`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/twin_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 인지 편향

- ID: `megacrit-sts2-core-models-cards-biasedcognition`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]밀집[/gold]을 4 얻습니다.
내 턴 시작 시, [gold]밀집[/gold]을 1 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BIASED_COGNITION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BiasedCognition`
- 리소스 경로: `res://images/packed/card_portraits/defect/biased_cognition.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 일곱 개의 별

- ID: `megacrit-sts2-core-models-cards-sevenstars`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 7만큼 {Repeat:choose(1):{}번|{}번} 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SEVEN_STARS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SevenStars`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/seven_stars.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 일제 사격

- ID: `megacrit-sts2-core-models-cards-barrage`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]영창[/gold]된 구체 1개당 피해를 5 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BARRAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Barrage`
- 리소스 경로: `res://images/packed/card_portraits/defect/barrage.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 임계 초과

- ID: `megacrit-sts2-core-models-cards-supercritical`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 4를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Energy+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUPERCRITICAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Supercritical`
- 리소스 경로: `res://images/packed/card_portraits/defect/supercritical.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 임명

- ID: `megacrit-sts2-core-models-cards-anointed`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 있는 모든 [gold]희귀[/gold] 카드를 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ANOINTED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Anointed`
- 리소스 경로: `res://images/packed/card_portraits/colorless/anointed.pngK`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 입자 벽

- ID: `megacrit-sts2-core-models-cards-particlewall`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
이 카드를 [gold]손[/gold]으로 다시 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PARTICLE_WALL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ParticleWall`
- 리소스 경로: `res://images/packed/card_portraits/regent/particle_wall.pngF(`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잉크 칼날

- ID: `megacrit-sts2-core-models-cards-bladeofink`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 공격 카드를 사용할 때마다, 이번 턴 동안 [gold]힘[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLADE_OF_INK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BladeOfInk`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/blade_of_ink.pngMT`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잊힌 의식

- ID: `megacrit-sts2-core-models-cards-forgottenritual`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 카드를 [gold]소멸[/gold]시켰다면, 3를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FORGOTTEN_RITUAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ForgottenRitual`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/forgotten_ritual.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 자동화

- ID: `megacrit-sts2-core-models-cards-automation`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 10장 뽑을 때마다, 1를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AUTOMATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Automation`
- 리소스 경로: `res://images/packed/card_portraits/colorless/automation.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 작업 도구

- ID: `megacrit-sts2-core-models-cards-toolsofthetrade`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 카드를 1장 뽑고 카드를 1장 버립니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TOOLS_OF_THE_TRADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ToolsOfTheTrade`
- 리소스 경로: `res://images/packed/card_portraits/silent/tools_of_the_trade.png8"u`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잔상

- ID: `megacrit-sts2-core-models-cards-afterimage`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 사용할 때마다, [gold]방어도[/gold]를 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AFTERIMAGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Afterimage`
- 리소스 경로: `res://images/packed/card_portraits/silent/afterimage.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 장막 관통자

- ID: `megacrit-sts2-core-models-cards-veilpiercer`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
다음에 사용하는 [gold]휘발성[/gold] 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VEILPIERCER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Veilpiercer`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/veilpiercer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 장송가

- ID: `megacrit-sts2-core-models-cards-dirge`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: X번 [gold]소환[/gold] {Summon:diff()}.
[gold]뽑을 카드 더미[/gold]에 [gold]{IfUpgraded:show:영혼+|영혼}[/gold]을 X장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DIRGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dirge`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/dirge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재난

- ID: `megacrit-sts2-core-models-cards-catastrophe`
- 그룹/풀 추정: 무색, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 무작위 카드를 2장 사용합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CATASTROPHE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Catastrophe`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/catastrophe.png#Lf`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재성형

- ID: `megacrit-sts2-core-models-cards-beatintoshape`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5 줍니다.
[gold]단조[/gold] {CalculatedForge:diff()}.
이번 턴에 대상 적을 공격한 다른 횟수마다 추가로 [gold]단조[/gold] {CalculationExtra:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+2, CalculationBase+2, CalculationExtra+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEAT_INTO_SHAPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BeatIntoShape`
- 리소스 경로: `res://images/packed/card_portraits/regent/beat_into_shape.pngq`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재앙

- ID: `megacrit-sts2-core-models-cards-calamity`
- 그룹/풀 추정: 무색, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격 카드를 사용할 때마다, 무작위 공격 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALAMITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Calamity`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/calamity.png1`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재주넘기

- ID: `megacrit-sts2-core-models-cards-flickflack`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 7 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: silent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLICK_FLACK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FlickFlack`
- 리소스 경로: `res://images/packed/card_portraits/silent/flick_flack.pngef`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잭팟

- ID: `megacrit-sts2-core-models-cards-jackpot`
- 그룹/풀 추정: 무색, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 25 줍니다.
비용이 0{energyPrefix:energyIcons(1)}인 무작위 {IfUpgraded:show:[gold]강화[/gold]된 }카드를 3장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JACKPOT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Jackpot`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/jackpot.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잿불

- ID: `megacrit-sts2-core-models-cards-cinder`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 17 줍니다.
[gold]뽑을 카드 더미[/gold] 맨 위의 카드를 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CINDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Cinder`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/cinder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잿빛 타격

- ID: `megacrit-sts2-core-models-cards-ashenstrike`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
[gold]소멸된 카드 더미[/gold]에 있는 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ASHEN_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.AshenStrike`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/ashen_strike.pngt`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잿빛 혼령

- ID: `megacrit-sts2-core-models-cards-spiritofash`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]휘발성[/gold] 카드를 사용할 때마다, [gold]방어도[/gold]를 {BlockOnExhaust:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPIRIT_OF_ASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SpiritOfAsh`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/spirit_of_ash.png8`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 저글링

- ID: `megacrit-sts2-core-models-cards-juggling`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 세 번째로 사용하는 [gold]공격[/gold] 카드의 복사본을 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JUGGLING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Juggling`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/juggling.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 저편의 울음소리

- ID: `megacrit-sts2-core-models-cards-howlfrombeyond`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 16 줍니다.
내 턴 시작 시, [gold]소멸된 카드 더미[/gold]에서 사용됩니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HOWL_FROM_BEYOND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HowlFromBeyond`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/howl_from_beyond.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 적응형 타격

- ID: `megacrit-sts2-core-models-cards-adaptivestrike`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 18 줍니다.
[gold]버린 카드 더미[/gold]에 비용이 0{energyPrefix:energyIcons(1)}인 이 카드의 복사본을 1장 추가합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ADAPTIVE_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.AdaptiveStrike`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/adaptive_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전략가

- ID: `megacrit-sts2-core-models-cards-tactician`
- 그룹/풀 추정: 사일런트, Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 1를 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TACTICIAN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tactician`
- 리소스 경로: `res://images/packed/card_portraits/silent/tactician.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전략의 천재

- ID: `megacrit-sts2-core-models-cards-masterofstrategy`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MASTER_OF_STRATEGY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MasterOfStrategy`
- 리소스 경로: `res://images/packed/card_portraits/colorless/master_of_strategy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전문성

- ID: `megacrit-sts2-core-models-cards-expertise`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 카드가 6장이 될 때까지 카드를 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXPERTISE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Expertise`
- 리소스 경로: `res://images/packed/card_portraits/silent/expertise.png3`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전장의 생존자

- ID: `megacrit-sts2-core-models-cards-wroughtinwar`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
[gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+2, Forge+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WROUGHT_IN_WAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.WroughtInWar`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/wrought_in_war.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투 최면

- ID: `megacrit-sts2-core-models-cards-battletrance`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
이번 턴 동안 더 이상 카드를 뽑을 수 없습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BATTLE_TRANCE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BattleTrance`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/battle_trance.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투의 북소리

- ID: `megacrit-sts2-core-models-cards-drumofbattle`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 2장 뽑습니다.
내 턴 시작 시, [gold]뽑을 카드 더미[/gold] 맨 위의 카드를 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DRUM_OF_BATTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DrumOfBattle`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/drum_of_battle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투의 성과

- ID: `megacrit-sts2-core-models-cards-spoilsofbattle`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Forge+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPOILS_OF_BATTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SpoilsOfBattle`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/spoils_of_battle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투장비

- ID: `megacrit-sts2-core-models-cards-armaments`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
[gold]손[/gold]에 있는 {IfUpgraded:show:모든 카드를|카드를 1장} [gold]강화[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ARMAMENTS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Armaments`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/armaments.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 절대적인 힘

- ID: `megacrit-sts2-core-models-cards-juggernaut`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 얻을 때마다, 무작위 적에게 피해를 5 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JUGGERNAUT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Juggernaut`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/juggernaut.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 점액투성이

- ID: `megacrit-sts2-core-models-cards-slimed`
- 그룹/풀 추정: Status, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLIMED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Slimed`
- 리소스 경로: `res://src/Core/Models/Cards/Slimed.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 점화

- ID: `megacrit-sts2-core-models-cards-ignition`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어가 [gold]플라즈마[/gold]를 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyAlly
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `IGNITION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Ignition`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/ignition.png3`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정렬

- ID: `megacrit-sts2-core-models-cards-alignment`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ALIGNMENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Alignment`
- 리소스 경로: `res://images/packed/card_portraits/regent/alignment.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정면 돌파

- ID: `megacrit-sts2-core-models-cards-breakthrough`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
모든 적에게 피해를 9 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BREAKTHROUGH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Breakthrough`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/breakthrough.pngK`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정밀

- ID: `megacrit-sts2-core-models-cards-accuracy`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단도[/gold]의 피해량이 4 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ACCURACY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Accuracy`
- 리소스 경로: `res://images/packed/card_portraits/silent/accuracy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정밀 사격

- ID: `megacrit-sts2-core-models-cards-pinpoint`
- 그룹/풀 추정: 사일런트, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 17 줍니다.
이번 턴에 스킬을 사용할 때마다 비용이 1 {energyPrefix:energyIcons(1)} 감소합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PINPOINT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Pinpoint`
- 리소스 경로: `res://images/packed/card_portraits/silent/pinpoint.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정밀한 베기

- ID: `megacrit-sts2-core-models-cards-precisecut`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
[gold]손[/gold]에 있는 다른 카드 1장당 피해량이 {ExtraDamage:inverseDiff()} 감소합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: CalculationBase+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PRECISE_CUT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PreciseCut`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/precise_cut.png[`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정복자

- ID: `megacrit-sts2-core-models-cards-conqueror`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단조[/gold] {Forge:diff()}.
이번 턴 동안 대상 적이 [gold]군주의 칼날[/gold]로 받는 피해량이 2배가 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Forge+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CONQUEROR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Conqueror`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/conqueror.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정신 공격

- ID: `megacrit-sts2-core-models-cards-mindblast`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 있는 카드의 수만큼 피해를 줍니다.{InCombat:
(피해를 {CalculatedDamage:diff()} 줍니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MIND_BLAST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MindBlast`
- 리소스 경로: `res://images/packed/card_portraits/colorless/mind_blast.png``
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정신 오염

- ID: `megacrit-sts2-core-models-cards-mindrot`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 카드를 1장 적게 뽑습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: token
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MIND_ROT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MindRot`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/mind_rot.pngY`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정신 폭주

- ID: `megacrit-sts2-core-models-cards-neurosurge`
- 그룹/풀 추정: Power, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 3를 얻습니다.
카드를 2장 뽑습니다.
내 턴 시작 시, 자신에게 [gold]종말[/gold]을 3 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEUROSURGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Neurosurge`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/neurosurge.pngA`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정화

- ID: `megacrit-sts2-core-models-cards-cleanse`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
[gold]뽑을 카드 더미[/gold]의 카드를 1장 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLEANSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Cleanse`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/cleanse.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 제물

- ID: `megacrit-sts2-core-models-cards-offering`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
2를 얻습니다.
카드를 3장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Cards+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OFFERING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Offering`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/offering.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 제압

- ID: `megacrit-sts2-core-models-cards-dominate`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 대상 적이 보유한 [gold]취약[/gold]마다 [gold]힘[/gold]을 {StrengthPerVulnerable:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOMINATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dominate`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/dominate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 제압

- ID: `megacrit-sts2-core-models-cards-suppress`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 11 줍니다.
[gold]약화[/gold]를 3 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+6, Weak+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUPPRESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Suppress`
- 리소스 경로: `res://images/packed/card_portraits/silent/suppress.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 조각 타격

- ID: `megacrit-sts2-core-models-cards-sculptingstrike`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
[gold]손[/gold]에 있는 카드 1장에 [gold]휘발성[/gold]을 추가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 선택 프롬프트: [gold]휘발성[/gold]을 추가할 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SCULPTING_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SculptingStrike`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/sculpting_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 조각모음

- ID: `megacrit-sts2-core-models-cards-defragment`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]밀집[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFRAGMENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Defragment`
- 리소스 경로: `res://images/packed/card_portraits/defect/defragment.pngKG`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 조약의 끝

- ID: `megacrit-sts2-core-models-cards-pactsend`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소멸된 카드 더미[/gold]에 카드가 3장 이상 있을 때만 사용할 수 있습니다.
모든 적에게 피해를 17 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PACTS_END`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PactsEnd`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/pacts_end.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 종말의 날

- ID: `megacrit-sts2-core-models-cards-endofdays`
- 그룹/풀 추정: Skill, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 [gold]종말[/gold]을 29 부여합니다.
체력이 [gold]종말[/gold] 이하인 적을 처치합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: Doom+8
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `END_OF_DAYS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EndOfDays`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/end_of_days.pngK`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죄책감

- ID: `megacrit-sts2-core-models-cards-guilty`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Combats:diff()}번의 전투 후에 [gold]덱[/gold]에서 제거됩니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GUILTY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Guilty`
- 리소스 경로: `res://images/packed/card_portraits/curse/guilty.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 주먹다짐

- ID: `megacrit-sts2-core-models-cards-fisticuffs`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
가한 피해량만큼 [gold]방어도[/gold]를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FISTICUFFS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fisticuffs`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/fisticuffs.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죽어가는 별

- ID: `megacrit-sts2-core-models-cards-dyingstar`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 9 줍니다. 이번 턴 동안 모든 적이 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DYING_STAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DyingStar`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/dying_star.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죽음 인도자

- ID: `megacrit-sts2-core-models-cards-deathbringer`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 [gold]종말[/gold]을 21, [gold]약화[/gold]를 1 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: Doom+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEATHBRINGER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Deathbringer`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/deathbringer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죽음의 무도

- ID: `megacrit-sts2-core-models-cards-dansemacabre`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 비용이 2 이상인 카드를 사용할 때마다, [gold]방어도[/gold]를 3 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DANSE_MACABRE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DanseMacabre`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/danse_macabre.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죽음의 문턱

- ID: `megacrit-sts2-core-models-cards-deathsdoor`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 6 얻습니다.
이번 턴에 [gold]종말[/gold]을 부여했다면, [gold]방어도[/gold]를 추가로 {Repeat:diff()}번 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEATHS_DOOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DeathsDoor`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/deaths_door.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 죽음의 행진

- ID: `megacrit-sts2-core-models-cards-deathmarch`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
내 턴 동안 뽑은 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: CalculationBase+1, ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEATH_MARCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DeathMarch`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/death_march.png-`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 준비 시간

- ID: `megacrit-sts2-core-models-cards-preptime`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, [gold]활력[/gold]을 4 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PREP_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PrepTime`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/prep_time.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 준항성

- ID: `megacrit-sts2-core-models-cards-quasar`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위{IfUpgraded:show: [gold]강화[/gold]된|} 무색 카드 3장 중 1장을 선택해 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `QUASAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Quasar`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/quasar.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 중성자 방패

- ID: `megacrit-sts2-core-models-cards-neutronaegis`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]판금[/gold]을 8 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEUTRON_AEGIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.NeutronAegis`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/neutron_aegis.png?`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쥐어뜯기

- ID: `megacrit-sts2-core-models-cards-rend`
- 그룹/풀 추정: 무색, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
적이 보유한 해로운 효과의 종류 하나당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: ExtraDamage+3, CalculationBase+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rend`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/rend.pngn`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쥐어짜기

- ID: `megacrit-sts2-core-models-cards-squeeze`
- 그룹/풀 추정: Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {CalculatedDamage:diff()} 줍니다.
다른 모든 [gold]골골이[/gold] 공격 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: CalculationBase+5, ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SQUEEZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Squeeze`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/squeeze.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 증강

- ID: `megacrit-sts2-core-models-cards-bulkup`
- 그룹/풀 추정: 디펙트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 구체 슬롯을 {OrbSlots:diff()}개 잃습니다.
[gold]힘[/gold]을 2 얻습니다.
[gold]민첩[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Strength+1, Dexterity+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BULK_UP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BulkUp`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/bulk_up.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 지면 파쇄

- ID: `megacrit-sts2-core-models-cards-crushunder`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 7 줍니다. 모든 적이 이번 턴 동안 [gold]힘[/gold]을 {StrengthLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: regent
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRUSH_UNDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CrushUnder`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/crush_under.pngj&`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 지연

- ID: `megacrit-sts2-core-models-cards-delay`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 11 얻습니다.
다음 턴에,
1를 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Block+2, Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DELAY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Delay`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/delay.png;`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 지옥검

- ID: `megacrit-sts2-core-models-cards-infernalblade`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 공격 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFERNAL_BLADE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.InfernalBlade`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/infernal_blade.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 지옥검무

- ID: `megacrit-sts2-core-models-cards-hellraiser`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이름에 “타격”이 포함된 카드를 뽑을 때마다, 무작위 적에게 사용합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HELLRAISER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hellraiser`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/hellraiser.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 지옥불

- ID: `megacrit-sts2-core-models-cards-fiendfire`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 [gold]소멸[/gold]시킵니다.
[gold]소멸[/gold]시킨 카드 1장당 피해를 7 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIEND_FIRE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FiendFire`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/fiend_fire.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 진정한 끈기

- ID: `megacrit-sts2-core-models-cards-truegrit`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 7 얻습니다.
{IfUpgraded:show:|무작위 }카드를 1장 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRUE_GRIT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TrueGrit`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/true_grit.png_A`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 진정한 오른팔

- ID: `megacrit-sts2-core-models-cards-righthandhand`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
비용이 2 이상인 카드를 사용할 때마다, 이 카드가 [gold]버린 카드 더미[/gold]에서 [gold]손[/gold]으로 돌아옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RIGHT_HAND_HAND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.RightHandHand`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/right_hand_hand.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 집단 공격

- ID: `megacrit-sts2-core-models-cards-gangup`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
이번 턴에 다른 플레이어가 대상 적을 공격한 횟수당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: ExtraDamage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GANG_UP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GangUp`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/gang_up.png'`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 집중 포화

- ID: `megacrit-sts2-core-models-cards-salvo`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 12 줍니다.
이번 턴에 [gold]손[/gold]에 있는 카드를 [gold]보존[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SALVO`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Salvo`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/salvo.pngE`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 짓누르기

- ID: `megacrit-sts2-core-models-cards-squash`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Event
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+2, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SQUASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Squash`
- 리소스 경로: `res://images/packed/card_portraits/event/squash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 짓밟기

- ID: `megacrit-sts2-core-models-cards-stomp`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 12 줍니다.
이번 턴 동안 사용한 공격 카드 1장당 {energyPrefix:energyIcons(1)} 비용이 1 감소합니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STOMP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stomp`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/stomp.pngwQ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 징벌

- ID: `megacrit-sts2-core-models-cards-scourge`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]종말[/gold]을 13 부여합니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Doom+3, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SCOURGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Scourge`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/scourge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쪼기

- ID: `megacrit-sts2-core-models-cards-peck`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 2만큼 {Repeat:diff()}번 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Event
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PECK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Peck`
- 리소스 경로: `res://images/packed/card_portraits/event/peck.pngBx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 찢기

- ID: `megacrit-sts2-core-models-cards-sunder`
- 그룹/풀 추정: 디펙트, Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 24 줍니다.
이 카드로 적을 처치 시, 3을 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+8
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUNDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Sunder`
- 리소스 경로: `res://images/packed/card_portraits/defect/sunder.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 차오르는 독

- ID: `megacrit-sts2-core-models-cards-bubblebubble`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적이 [gold]중독[/gold]을 보유하고 있다면, [gold]중독[/gold]을 9 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Poison+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BUBBLE_BUBBLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BubbleBubble`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/bubble_bubble.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 착수

- ID: `megacrit-sts2-core-models-cards-splash`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 캐릭터의 무작위{IfUpgraded:show: [gold]강화[/gold]된} 공격 카드 3장 중 1장을 선택해 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPLASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Splash`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/splash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 찬란한 불꽃

- ID: `megacrit-sts2-core-models-cards-brightestflame`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
카드를 2장 뽑습니다.
최대 체력을 1 잃습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Energy+1, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BRIGHTEST_FLAME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BrightestFlame`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/brightest_flame.png{W}`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 참호

- ID: `megacrit-sts2-core-models-cards-entrench`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]가 2배가 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENTRENCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Entrench`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/entrench.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 창백한 푸른 점

- ID: `megacrit-sts2-core-models-cards-palebluedot`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한 턴에 카드를 {CardPlay}장 이상 사용했다면, 다음 턴 시작 시 카드를 추가로 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PALE_BLUE_DOT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PaleBlueDot`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/pale_blue_dot.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 창세

- ID: `megacrit-sts2-core-models-cards-genesis`
- 그룹/풀 추정: Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, {StarsPerTurn:starIcons()}을 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GENESIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Genesis`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/genesis.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 창의적인 인공지능

- ID: `megacrit-sts2-core-models-cards-creativeai`
- 그룹/풀 추정: 디펙트, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 파워 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CREATIVE_AI`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CreativeAi`
- 리소스 경로: `res://images/packed/card_portraits/defect/creative_ai.png,U`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 창조의 기둥

- ID: `megacrit-sts2-core-models-cards-pillarofcreation`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 생성할 때마다, [gold]방어도[/gold]를 3 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Block+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PILLAR_OF_CREATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PillarOfCreation`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/pillar_of_creation.png{`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 책략

- ID: `megacrit-sts2-core-models-cards-stratagem`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]를 섞을 때마다, 카드를 1장 선택해 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRATAGEM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stratagem`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/stratagem.pngZ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 척결

- ID: `megacrit-sts2-core-models-cards-eradicate`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 11만큼 X번 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ERADICATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Eradicate`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/eradicate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 천둥

- ID: `megacrit-sts2-core-models-cards-thunderclap`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 4 주고 [gold]취약[/gold]을 1 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THUNDERCLAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Thunderclap`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/thunderclap.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 천상의 권능

- ID: `megacrit-sts2-core-models-cards-celestialmight`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6만큼 {Repeat:diff()}번 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CELESTIAL_MIGHT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CelestialMight`
- 리소스 경로: `res://images/packed/card_portraits/regent/celestial_might.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 천원돌파

- ID: `megacrit-sts2-core-models-cards-heavenlydrill`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8만큼 X번 줍니다.
X가 4 이상이라면 X가 2배가 됩니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEAVENLY_DRILL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HeavenlyDrill`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/heavenly_drill.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 철의 파동

- ID: `megacrit-sts2-core-models-cards-ironwave`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
피해를 5 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2, Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `IRON_WAVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.IronWave`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/iron_wave.pngZ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쳐내기

- ID: `megacrit-sts2-core-models-cards-parry`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]군주의 칼날[/gold]을 사용할 때마다, [gold]방어도[/gold]를 6 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PARRY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Parry`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/parry.pngl`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 초승달 창

- ID: `megacrit-sts2-core-models-cards-crescentspear`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
보유한 카드 중 {singleStarIcon} 비용을 지닌 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRESCENT_SPEAR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CrescentSpear`
- 리소스 경로: `res://images/packed/card_portraits/regent/crescent_spear.png%U`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 초조함

- ID: `megacrit-sts2-core-models-cards-restlessness`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 카드가 없다면, 카드를 2장 뽑고 2를 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+1, Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RESTLESSNESS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Restlessness`
- 리소스 경로: `res://images/packed/card_portraits/colorless/restlessness.pngF`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 초질량

- ID: `megacrit-sts2-core-models-cards-supermassive`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
이번 전투 동안 생성한 카드 1장당 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUPERMASSIVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Supermassive`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/supermassive.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 초토화

- ID: `megacrit-sts2-core-models-cards-devastate`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 30 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+10
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEVASTATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Devastate`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/devastate.pngn`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 촉진제

- ID: `megacrit-sts2-core-models-cards-accelerant`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]중독[/gold]이 {Accelerant:diff()}번 추가로 발동합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ACCELERANT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Accelerant`
- 리소스 경로: `res://images/packed/card_portraits/silent/accelerant.png+`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 추앙

- ID: `megacrit-sts2-core-models-cards-venerate`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Stars:starIcons()}을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Stars+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VENERATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Venerate`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/venerate.png@d`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 추적

- ID: `megacrit-sts2-core-models-cards-tracking`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]약화[/gold] 상태의 적이 공격 카드로 받는 피해가 2배가 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRACKING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tracking`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/tracking.pngZ`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 추진 타격

- ID: `megacrit-sts2-core-models-cards-momentumstrike`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
이 카드의 비용이 0 {energyPrefix:energyIcons(1)}으로 감소합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MOMENTUM_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MomentumStrike`
- 리소스 경로: `res://images/packed/card_portraits/defect/momentum_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 추출

- ID: `megacrit-sts2-core-models-cards-dredge`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버린 카드 더미[/gold]에서 카드를 3장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 선택 프롬프트: [gold]손[/gold]으로 가져올 카드를 {Amount}장 선택하세요
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DREDGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dredge`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/dredge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 축전기

- ID: `megacrit-sts2-core-models-cards-capacitor`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 구체 슬롯을 {Repeat:diff()}개 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CAPACITOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Capacitor`
- 리소스 경로: `res://images/packed/card_portraits/defect/capacitor.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 출몰

- ID: `megacrit-sts2-core-models-cards-haunt`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]영혼[/gold]을 사용할 때마다, 무작위 적이 체력을 {HpLoss:diff()} 잃습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: HpLoss+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAUNT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Haunt`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/haunt.pngx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 충격파

- ID: `megacrit-sts2-core-models-cards-shockwave`
- 그룹/풀 추정: 무색, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 [gold]약화[/gold]와 [gold]취약[/gold]을 {Power:diff()} 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHOCKWAVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Shockwave`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/shockwave.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 측면 공격

- ID: `megacrit-sts2-core-models-cards-flanking`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 대상 적이 다른 플레이어에게 받는 피해량이 2배가 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLANKING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Flanking`
- 리소스 경로: `res://images/packed/card_portraits/silent/flanking.png|`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 치사성

- ID: `megacrit-sts2-core-models-cards-lethality`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 사용하는 공격 카드의 피해량이 50% 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LETHALITY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Lethality`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/lethality.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 카운트다운

- ID: `megacrit-sts2-core-models-cards-countdown`
- 그룹/풀 추정: Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 적에게 [gold]종말[/gold]을 6 부여합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COUNTDOWN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Countdown`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/countdown.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 칼날 부채

- ID: `megacrit-sts2-core-models-cards-fanofknives`
- 그룹/풀 추정: 사일런트, Power, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단도[/gold]가 이제 모든 적을 대상으로 합니다.
[gold]단도[/gold]를 {Shivs:diff()}장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FAN_OF_KNIVES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FanOfKnives`
- 리소스 경로: `res://images/packed/card_portraits/silent/fan_of_knives.pnggA`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 칼날 함정

- ID: `megacrit-sts2-core-models-cards-knifetrap`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 대상 적에게 [gold]소멸된 카드 더미[/gold]에 있는 모든 [gold]단도[/gold]를{IfUpgraded:show: [gold]강화[/gold]한 뒤|} 사용합니다.{InCombat:
([gold]단도[/gold]를 {CalculatedShivs:diff()}장 사용합니다)|}
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KNIFE_TRAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.KnifeTrap`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/knife_trap.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 칼질

- ID: `megacrit-sts2-core-models-cards-slice`
- 그룹/풀 추정: 사일런트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Slice`
- 리소스 경로: `res://images/packed/card_portraits/silent/slice.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 컴파일 드라이버

- ID: `megacrit-sts2-core-models-cards-compiledriver`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
영창된 구체의 종류 하나당 카드를 1장 뽑습니다.{InCombat:
(카드를 {CalculatedCards:diff()}장 뽑습니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COMPILE_DRIVER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CompileDriver`
- 리소스 경로: `res://images/packed/card_portraits/defect/compile_driver.pngg`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타격

- ID: `megacrit-sts2-core-models-cards-strikedefect`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRIKE_DEFECT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StrikeDefect`
- 리소스 경로: `res://src/Core/Models/Cards/StrikeDefect.csNK0`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타격

- ID: `megacrit-sts2-core-models-cards-strikeironclad`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRIKE_IRONCLAD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StrikeIronclad`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/strike_ironclad.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타격

- ID: `megacrit-sts2-core-models-cards-strikenecrobinder`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRIKE_NECROBINDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StrikeNecrobinder`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/strike_necrobinder.png0`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타격

- ID: `megacrit-sts2-core-models-cards-strikeregent`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRIKE_REGENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StrikeRegent`
- 리소스 경로: `res://images/packed/card_portraits/regent/strike_regent.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타격

- ID: `megacrit-sts2-core-models-cards-strikesilent`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRIKE_SILENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.StrikeSilent`
- 리소스 경로: `res://images/packed/card_portraits/silent/strike_silent.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 타락

- ID: `megacrit-sts2-core-models-cards-corruption`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 스킬 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
스킬 카드를 사용할 때마다 그 카드를 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Power
  - 희귀도: Ancient
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CORRUPTION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Corruption`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/corruption.pngB`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탄성 플라스크

- ID: `megacrit-sts2-core-models-cards-bouncingflask`
- 그룹/풀 추정: 사일런트, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 [gold]중독[/gold]을 3만큼 {Repeat:diff()}번 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: RandomEnemy
  - 카드 풀: silent
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOUNCING_FLASK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BouncingFlask`
- 리소스 경로: `res://images/packed/card_portraits/silent/bouncing_flask.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탈바꿈

- ID: `megacrit-sts2-core-models-cards-metamorphosis`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 무작위 공격 카드를 3장 추가합니다. 이번 전투 동안 그 카드들을 비용 없이 사용할 수 있습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Event
  - 대상: Self
  - 카드 풀: event
  - 강화 요약: Cards+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `METAMORPHOSIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Metamorphosis`
- 리소스 경로: `res://images/packed/card_portraits/event/metamorphosis.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탈출구

- ID: `megacrit-sts2-core-models-cards-escapeplan`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 뽑습니다.
뽑은 카드가 스킬 카드라면, [gold]방어도[/gold]를 3 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ESCAPE_PLAN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.EscapePlan`
- 리소스 경로: `res://images/packed/card_portraits/silent/escape_plan.pngZB*!`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탐색 타격

- ID: `megacrit-sts2-core-models-cards-seekerstrike`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 6 줍니다.
[gold]뽑을 카드 더미[/gold]의 카드 3장 중 1장을 선택해 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+3
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SEEKER_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SeekerStrike`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/seeker_strike.png=`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탐욕의 손

- ID: `megacrit-sts2-core-models-cards-handofgreed`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 20 줍니다.
[gold]치명타[/gold]라면, [gold]골드[/gold]를 {Gold:diff()} 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAND_OF_GREED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HandOfGreed`
- 리소스 경로: `res://src/Core/Models/Cards/HandOfGreed.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 태그 팀

- ID: `megacrit-sts2-core-models-cards-tagteam`
- 그룹/풀 추정: 무색, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 11 줍니다.
다른 플레이어가 다음에 대상 적에게 사용하는 공격 카드가 1번 추가로 사용됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: colorless
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TAG_TEAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TagTeam`
- 리소스 경로: `res://images/packed/card_portraits/colorless/tag_team.pngl`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 태양의 타격

- ID: `megacrit-sts2-core-models-cards-solarstrike`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
{Stars:starIcons()}을 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+1, Stars+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOLAR_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SolarStrike`
- 리소스 경로: `res://images/packed/card_portraits/regent/solar_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탱커

- ID: `megacrit-sts2-core-models-cards-tank`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적에게 받는 피해가 2배가 됩니다.
아군이 적에게 받는 피해가 절반이 됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TANK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Tank`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/tank.png5`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Tank: Düşmanlardan iki kat fazla hasar al. Müttefikler düşmanlardan alacakları hasarın yarısını alır.

### 터보

- ID: `megacrit-sts2-core-models-cards-turbo`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 2를 얻습니다.
[gold]뽑을 카드 더미[/gold]에 [gold]공허[/gold]를 1장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TURBO`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Turbo`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/turbo.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: TURBO: 2 elde et. [gold]Atık Deste[/gold]'ne bir [gold]Boşluk[/gold] ekle.

### 테라포밍

- ID: `megacrit-sts2-core-models-cards-terraforming`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]활력[/gold]을 6 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TERRAFORMING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Terraforming`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/terraforming.png&`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 테슬라 코일

- ID: `megacrit-sts2-core-models-cards-teslacoil`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
대상 적을 상대로 모든 [gold]전기[/gold]를 발동시킵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TESLA_COIL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TeslaCoil`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/tesla_coil.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 튕겨내기

- ID: `megacrit-sts2-core-models-cards-deflect`
- 그룹/풀 추정: 사일런트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 4 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEFLECT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Deflect`
- 리소스 경로: `res://images/packed/card_portraits/silent/deflect.png]='`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 틀어막기

- ID: `megacrit-sts2-core-models-cards-gunkup`
- 그룹/풀 추정: 디펙트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 4만큼 {Repeat:diff()}번 줍니다.
[gold]버린 카드 더미[/gold]에 [gold]점액투성이[/gold]를 1장 추가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GUNK_UP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GunkUp`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/gunk_up.pngC`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파괴

- ID: `megacrit-sts2-core-models-cards-havoc`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold] 맨 위의 카드를 사용한 뒤 [gold]소멸[/gold]시킵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HAVOC`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Havoc`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/havoc.png}x`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파괴광선

- ID: `megacrit-sts2-core-models-cards-hyperbeam`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 26 줍니다.
[gold]밀집[/gold]을 3 잃습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AllEnemies
  - 카드 풀: defect
  - 강화 요약: Damage+8
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HYPERBEAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hyperbeam`
- 리소스 경로: `res://images/packed/card_portraits/defect/hyperbeam.pngsj?k+ d`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파수꾼

- ID: `megacrit-sts2-core-models-cards-protector`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {CalculatedDamage:diff()} 줍니다.
[gold]골골이[/gold]의 [gold]최대 체력[/gold]만큼 피해량이 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: CalculationBase+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PROTECTOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Protector`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/protector.png"`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파열

- ID: `megacrit-sts2-core-models-cards-rupture`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 동안 체력을 잃을 때마다, [gold]힘[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Strength+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RUPTURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Rupture`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/rupture.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파종

- ID: `megacrit-sts2-core-models-cards-sow`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 8 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Sow`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/sow.pngg`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 파지직

- ID: `megacrit-sts2-core-models-cards-zap`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]전기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ZAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Zap`
- 리소스 경로: `res://images/packed/card_portraits/defect/zap.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 팔방미인

- ID: `megacrit-sts2-core-models-cards-jackofalltrades`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 무색 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JACK_OF_ALL_TRADES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.JackOfAllTrades`
- 리소스 경로: `res://images/packed/card_portraits/colorless/jack_of_all_trades.png?`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 패권

- ID: `megacrit-sts2-core-models-cards-hegemony`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 15 줍니다.
다음 턴에, 2를 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+3, Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEGEMONY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hegemony`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/hegemony.png?(`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 평형

- ID: `megacrit-sts2-core-models-cards-equilibrium`
- 그룹/풀 추정: 무색, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 13 얻습니다.
이번 턴에 [gold]손[/gold]에 있는 카드를 [gold]보존[/gold]합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EQUILIBRIUM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Equilibrium`
- 리소스 경로: `res://images/packed/card_portraits/colorless/equilibrium.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포격

- ID: `megacrit-sts2-core-models-cards-bombardment`
- 그룹/풀 추정: Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 18 줍니다.
내 턴 시작 시, [gold]소멸된 카드 더미[/gold]에서 사용됩니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BOMBARDMENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bombardment`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/bombardment.png-b`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포석

- ID: `megacrit-sts2-core-models-cards-thegambit`
- 그룹/풀 추정: 무색, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 50 얻습니다.
이번 전투 동안 막히지 않은 공격 피해를 받는다면, 죽습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
  - 강화 요약: Block+25
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_GAMBIT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheGambit`
- 리소스 경로: `res://images/packed/card_portraits/colorless/the_gambit.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포식

- ID: `megacrit-sts2-core-models-cards-feed`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]치명타[/gold]라면, 최대 체력이 3 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2, MaxHp+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FEED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Feed`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/feed.pngz`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포식자

- ID: `megacrit-sts2-core-models-cards-predator`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 15 줍니다.
다음 턴에, 카드를 2장 뽑습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PREDATOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Predator`
- 리소스 경로: `res://src/Core/Models/Cards/Predator.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포악함

- ID: `megacrit-sts2-core-models-cards-vicious`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]취약[/gold]을 부여할 때마다, 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VICIOUS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Vicious`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/vicious.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포집

- ID: `megacrit-sts2-core-models-cards-scavenge`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 1장 [gold]소멸[/gold]시킵니다.
다음 턴에, 2를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Energy+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SCAVENGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Scavenge`
- 리소스 경로: `res://images/packed/card_portraits/defect/scavenge.png+U`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 폭주

- ID: `megacrit-sts2-core-models-cards-burst`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴에 다음에 사용하는 {Skills:choose(1):스킬 카드가|스킬 카드}{IfUpgraded:show: 2장이} 1번 추가로 사용됩니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BURST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Burst`
- 리소스 경로: `res://images/packed/card_portraits/silent/burst.pngY`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 폭탄

- ID: `megacrit-sts2-core-models-cards-thebomb`
- 그룹/풀 추정: 무색, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Turns:diff()}턴 후 턴 종료 시, 모든 적에게 피해를 {BombDamage:diff()} 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_BOMB`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.TheBomb`
- 리소스 경로: `res://images/packed/card_portraits/colorless/the_bomb.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 폭풍

- ID: `megacrit-sts2-core-models-cards-storm`
- 그룹/풀 추정: 디펙트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 파워 카드를 사용할 때마다, [gold]전기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STORM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Storm`
- 리소스 경로: `res://images/packed/card_portraits/defect/storm.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 폼멜 타격

- ID: `megacrit-sts2-core-models-cards-pommelstrike`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 9 줍니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+1, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POMMEL_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PommelStrike`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/pommel_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 풀어놓기

- ID: `megacrit-sts2-core-models-cards-unleash`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {CalculatedDamage:diff()} 줍니다.
[gold]골골이[/gold]의 현재 체력만큼 피해량이 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Basic
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: CalculationBase+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNLEASH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Unleash`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/unleash.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 프레췌

- ID: `megacrit-sts2-core-models-cards-flechettes`
- 그룹/풀 추정: 사일런트, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 스킬 카드 1장당 피해를 5 줍니다.{InCombat:
({CalculatedHits:diff()}번 적중합니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: silent
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLECHETTES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Flechettes`
- 리소스 경로: `res://images/packed/card_portraits/silent/flechettes.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 피뢰침

- ID: `megacrit-sts2-core-models-cards-lightningrod`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 4 얻습니다.
다음 {LightningRodPower:choose(1):턴 시작 시|{:diff()}턴 동안 턴 시작 시}, [gold]전기[/gold]를 1번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LIGHTNING_ROD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LightningRod`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/lightning_rod.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 피의 벽

- ID: `megacrit-sts2-core-models-cards-bloodwall`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
[gold]방어도[/gold]를 16 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLOOD_WALL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BloodWall`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/blood_wall.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 필사의 일격

- ID: `megacrit-sts2-core-models-cards-knockoutblow`
- 그룹/풀 추정: Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 30 줍니다.
이 카드가 적을 처치 시, {Stars:starIcons()}을 얻습니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+8
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `KNOCKOUT_BLOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.KnockoutBlow`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/knockout_blow.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 필사적인 도주

- ID: `megacrit-sts2-core-models-cards-franticescape`
- 그룹/풀 추정: Status, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 보스에게서 멀어집니다.
[gold]모래 구덩이[/gold]가 1 증가합니다.
이 카드의 비용이 1 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Status
  - 희귀도: Status
  - 대상: Self
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FRANTIC_ESCAPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FranticEscape`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/frantic_escape.pngx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 필연적인 결과

- ID: `megacrit-sts2-core-models-cards-foregoneconclusion`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에, [gold]뽑을 카드 더미[/gold]에서 카드를 2장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOREGONE_CONCLUSION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ForegoneConclusion`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/foregone_conclusion.png?`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 핏빛 망토

- ID: `megacrit-sts2-core-models-cards-crimsonmantle`
- 그룹/풀 추정: 아이언클래드, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 체력을 1 잃고 [gold]방어도[/gold]를 8 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRIMSON_MANTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CrimsonMantle`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/beta/crimson_mantle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하나를 위한 모두

- ID: `megacrit-sts2-core-models-cards-allforone`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 10 줍니다.
[gold]버린 카드 더미[/gold]에서 모든 0{energyPrefix:energyIcons(1)} 카드를 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ALL_FOR_ONE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.AllForOne`
- 리소스 경로: `res://images/packed/card_portraits/defect/all_for_one.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하사

- ID: `megacrit-sts2-core-models-cards-largesse`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어가 무작위{IfUpgraded:show: [gold]강화[/gold]된} 무색 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: regent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LARGESSE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Largesse`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/largesse.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하수인 타격

- ID: `megacrit-sts2-core-models-cards-minionstrike`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 7 줍니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Token
  - 대상: AnyEnemy
  - 카드 풀: token
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MINION_STRIKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MinionStrike`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/minion_strike.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하수인 투하

- ID: `megacrit-sts2-core-models-cards-miniondivebomb`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 13 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Token
  - 대상: AnyEnemy
  - 카드 풀: token
  - 강화 요약: Damage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MINION_DIVE_BOMB`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MinionDiveBomb`
- 리소스 경로: `res://images/packed/card_portraits/token/minion_dive_bomb.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하수인 희생

- ID: `megacrit-sts2-core-models-cards-minionsacrifice`
- 그룹/풀 추정: Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 9 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Token
  - 대상: Self
  - 카드 풀: token
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MINION_SACRIFICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.MinionSacrifice`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/minion_sacrifice.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 하이파이브

- ID: `megacrit-sts2-core-models-cards-highfive`
- 그룹/풀 추정: Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 모든 적에게
피해를 {OstyDamage:diff()} 주고
[gold]취약[/gold]을 2 부여합니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AllEnemies
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+2, Vulnerable+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HIGH_FIVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.HighFive`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/high_five.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 할퀴기

- ID: `megacrit-sts2-core-models-cards-maul`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 5만큼 2번 줍니다.
이번 전투 동안 모든 할퀴기 카드의 피해량이 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAUL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Maul`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/maul.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 합성

- ID: `megacrit-sts2-core-models-cards-synthesis`
- 그룹/풀 추정: 디펙트, Attack, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 12 줍니다.
다음에 사용하는 파워 카드의 비용이 0 {energyPrefix:energyIcons(1)}이 됩니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+6
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SYNTHESIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Synthesis`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/synthesis.pngv`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 핫픽스

- ID: `megacrit-sts2-core-models-cards-hotfix`
- 그룹/풀 추정: 디펙트, Skill, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 [gold]밀집[/gold]을 2 얻습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HOTFIX`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hotfix`
- 리소스 경로: `res://images/packed/card_portraits/defect/beta/hotfix.png%`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 해골 군단

- ID: `megacrit-sts2-core-models-cards-legionofbone`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 플레이어가 [gold]소환[/gold] {Summon:diff()}.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AllAllies
  - 카드 풀: necrobinder
  - 강화 요약: Summon+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LEGION_OF_BONE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.LegionOfBone`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/legion_of_bone.pngVz`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 해체

- ID: `megacrit-sts2-core-models-cards-dismantle`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 8 줍니다.
대상 적이 [gold]취약[/gold] 상태라면, 2번 적중합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISMANTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dismantle`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/dismantle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 향수

- ID: `megacrit-sts2-core-models-cards-nostalgia`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 매 턴마다 처음으로 사용하는 공격이나 스킬 카드를 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NOSTALGIA`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Nostalgia`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/nostalgia.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 허상

- ID: `megacrit-sts2-core-models-cards-eidolon`
- 그룹/풀 추정: Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 [gold]소멸[/gold]시킵니다.
이를 통해 카드를 9장 [gold]소멸[/gold]시켰다면, [gold]불가침[/gold]을 1 얻습니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EIDOLON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Eidolon`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/eidolon.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혈류

- ID: `megacrit-sts2-core-models-cards-hemokinesis`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 체력을 {HpLoss:diff()} 잃습니다.
피해를 14 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: Damage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HEMOKINESIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hemokinesis`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/hemokinesis.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 협력

- ID: `megacrit-sts2-core-models-cards-coordinate`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 다른 플레이어에게 [gold]힘[/gold]을 5 줍니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyAlly
  - 카드 풀: colorless
  - 강화 요약: Strength+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COORDINATE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Coordinate`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/coordinate.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 협박

- ID: `megacrit-sts2-core-models-cards-bully`
- 그룹/풀 추정: 아이언클래드, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {CalculatedDamage:diff()} 줍니다.
대상 적이 보유한 [gold]취약[/gold]마다 피해량이 {ExtraDamage:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: ironclad
  - 강화 요약: ExtraDamage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BULLY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bully`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/bully.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혜성

- ID: `megacrit-sts2-core-models-cards-comet`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 33 줍니다.
[gold]약화[/gold]를 3 부여합니다.
[gold]취약[/gold]을 3 부여합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: regent
  - 강화 요약: Damage+11
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COMET`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Comet`
- 리소스 경로: `res://images/packed/card_portraits/regent/beta/comet.pngy(Gu[O`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 호각 불기

- ID: `megacrit-sts2-core-models-cards-whistle`
- 그룹/풀 추정: Attack, 코스트 3
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 33 줍니다.
적을 [gold]기절[/gold]시킵니다.
- 구조 정보:
  - 코스트: 3
  - 유형: Attack
  - 희귀도: Ancient
  - 대상: AnyEnemy
  - 카드 풀: event
  - 강화 요약: Damage+11
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHISTLE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Whistle`
- 리소스 경로: `res://images/packed/card_portraits/event/beta/whistle.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 호위

- ID: `megacrit-sts2-core-models-cards-bodyguard`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]소환[/gold] {Summon:diff()}.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Basic
  - 대상: Self
  - 카드 풀: necrobinder
  - 강화 요약: Summon+2
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BODYGUARD`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Bodyguard`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/beta/bodyguard.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혼돈

- ID: `megacrit-sts2-core-models-cards-chaos`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 구체를 {Repeat:diff()}번 [gold]영창[/gold]합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Repeat+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CHAOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Chaos`
- 리소스 경로: `res://images/packed/card_portraits/defect/chaos.png4`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혼령 포획

- ID: `megacrit-sts2-core-models-cards-capturespirit`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 적이 체력을 3 잃습니다.
[gold]뽑을 카드 더미[/gold]에 [gold]영혼[/gold]을 3장 섞어 넣습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+1, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CAPTURE_SPIRIT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CaptureSpirit`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/capture_spirit.png<:U`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 홀로그램

- ID: `megacrit-sts2-core-models-cards-hologram`
- 그룹/풀 추정: 디펙트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 3 얻습니다.
[gold]버린 카드 더미[/gold]에서 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: defect
  - 강화 요약: Block+2
- 선택 프롬프트: 손으로 가져올 카드를 선택하세요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HOLOGRAM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Hologram`
- 리소스 경로: `res://images/packed/card_portraits/defect/hologram.pngFE`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Hologram: 3 [gold]Blok[/gold] elde et. [gold]Atık Deste[/gold]'nden bir kartı [gold]El[/gold]'ine koy.

### 화력 증폭

- ID: `megacrit-sts2-core-models-cards-stoke`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드를 [gold]소멸[/gold]시킵니다.
[gold]소멸[/gold]시킨 카드의 수만큼 카드를 뽑습니다
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: ironclad
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STOKE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Stoke`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/stoke.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 화상

- ID: `megacrit-sts2-core-models-cards-burn`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, 피해를 2 받습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BURN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Burn`
- 리소스 경로: `res://images/packed/card_portraits/status/burn.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 화염 장벽

- ID: `megacrit-sts2-core-models-cards-flamebarrier`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 2
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 12 얻습니다.
이번 턴에 공격을 받을 때마다, 공격한 적에게 피해를 {DamageBack:diff()} 줍니다.
- 구조 정보:
  - 코스트: 2
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+4
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FLAME_BARRIER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.FlameBarrier`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/flame_barrier.pngr`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 환영검

- ID: `megacrit-sts2-core-models-cards-phantomblades`
- 그룹/풀 추정: 사일런트, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]단도[/gold]가 [gold]보존[/gold]을 얻습니다.
매 턴마다 처음으로 사용하는 [gold]단도[/gold]의 피해량이 9 증가합니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PHANTOM_BLADES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PhantomBlades`
- 리소스 경로: `res://images/packed/card_portraits/silent/beta/phantom_blades.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 황금 도끼

- ID: `megacrit-sts2-core-models-cards-goldaxe`
- 그룹/풀 추정: 무색, Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 전투 동안 사용한 카드의 수와 동일한 만큼의 피해를 줍니다.{InCombat:
(피해를 {CalculatedDamage:diff()} 줍니다)|}
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Rare
  - 대상: AnyEnemy
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GOLD_AXE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.GoldAxe`
- 리소스 경로: `res://images/packed/card_portraits/colorless/gold_axe.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 회수

- ID: `megacrit-sts2-core-models-cards-fetch`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 피해를 {OstyDamage:diff()} 줍니다.
이번 턴에 이 카드를 처음으로 사용했다면, 카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Uncommon
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: OstyDamage+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FETCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Fetch`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/fetch.pnguiI`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 후벼 파기

- ID: `megacrit-sts2-core-models-cards-claw`
- 그룹/풀 추정: 디펙트, Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 3 줍니다.
이번 전투 동안 모든 후벼 파기 카드의 피해량이 {Increase:diff()} 증가합니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: defect
  - 강화 요약: Damage+1
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLAW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Claw`
- 리소스 경로: `res://images/packed/card_portraits/defect/claw.png-`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 후회

- ID: `megacrit-sts2-core-models-cards-regret`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 종료 시 이 카드가 [gold]손[/gold]에 있다면, [gold]손[/gold]에 있는 카드 1장당 체력을 1 잃습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REGRET`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Regret`
- 리소스 경로: `res://images/packed/card_portraits/curse/regret.pngY`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 훑어보기

- ID: `megacrit-sts2-core-models-cards-sweepinggaze`
- 그룹/풀 추정: Attack, 코스트 0
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 무작위 적에게 피해를 {OstyDamage:diff()} 줍니다.
- 구조 정보:
  - 코스트: 0
  - 유형: Attack
  - 희귀도: Token
  - 대상: RandomEnemy
  - 카드 풀: token
  - 강화 요약: OstyDamage+5
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SWEEPING_GAZE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SweepingGaze`
- 리소스 경로: `res://images/packed/card_portraits/token/beta/sweeping_gaze.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 휘갈김

- ID: `megacrit-sts2-core-models-cards-scrawl`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]이 가득 찰 때까지 카드를 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SCRAWL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Scrawl`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/scrawl.png*`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 흉내내기

- ID: `megacrit-sts2-core-models-cards-mimic`
- 그룹/풀 추정: 무색, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다른 플레이어의 [gold]방어도[/gold]와 동일한 만큼의 [gold]방어도[/gold]를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: AnyAlly
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MIMIC`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Mimic`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beta/mimic.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 흐릿함

- ID: `megacrit-sts2-core-models-cards-blur`
- 그룹/풀 추정: 사일런트, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 5 얻습니다.
다음 턴 시작 시 [gold]방어도[/gold]가 사라지지 않습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: silent
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BLUR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Blur`
- 리소스 경로: `res://images/packed/card_portraits/silent/blur.png%`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 흘려보내기

- ID: `megacrit-sts2-core-models-cards-shrugitoff`
- 그룹/풀 추정: 아이언클래드, Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 8 얻습니다.
카드를 1장 뽑습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Common
  - 대상: Self
  - 카드 풀: ironclad
  - 강화 요약: Block+3
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SHRUG_IT_OFF`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.ShrugItOff`
- 리소스 경로: `res://images/packed/card_portraits/ironclad/shrug_it_off.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 희망의 등불

- ID: `megacrit-sts2-core-models-cards-beaconofhope`
- 그룹/풀 추정: 무색, Power, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 동안 [gold]방어도[/gold]를 얻을 때마다, 다른 플레이어가 그 절반만큼 [gold]방어도[/gold]를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Power
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: colorless
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEACON_OF_HOPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.BeaconOfHope`
- 리소스 경로: `res://images/packed/card_portraits/colorless/beacon_of_hope.pngX`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 희미한 빛

- ID: `megacrit-sts2-core-models-cards-glimmer`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 카드를 3장 뽑습니다.
[gold]손[/gold]에 있는 카드를 {PutBack:diff()}장 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Uncommon
  - 대상: Self
  - 카드 풀: regent
  - 강화 요약: Cards+1
- 선택 프롬프트: 뽑을 카드 더미 맨 위에 놓을 카드를 선택하세요
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GLIMMER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Glimmer`
- 리소스 경로: `res://images/packed/card_portraits/regent/glimmer.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 희생

- ID: `megacrit-sts2-core-models-cards-sacrifice`
- 그룹/풀 추정: Skill, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]골골이[/gold]가 살아있다면, 골골이가 죽고 골골이의 최대 체력의 2배만큼의 [gold]방어도[/gold]를 얻습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Skill
  - 희귀도: Rare
  - 대상: Self
  - 카드 풀: necrobinder
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SACRIFICE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Sacrifice`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/sacrifice.pngm`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### @Control@1477

- ID: `control-1477`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### @Control@1654

- ID: `control-1654`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### CARD.ANGER

- ID: `card-anger`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.ARMAMENTS

- ID: `card-armaments`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.BASH

- ID: `card-bash`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.BLOODLETTING

- ID: `card-bloodletting`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.BODY_SLAM

- ID: `card-body-slam`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.BURNING_PACT

- ID: `card-burning-pact`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.CINDER

- ID: `card-cinder`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.COLOSSUS

- ID: `card-colossus`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.DARK_SHACKLES

- ID: `card-dark-shackles`
- 그룹/풀 추정: Skill
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Skill
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.DEFEND_IRONCLAD

- ID: `card-defend-ironclad`
- 그룹/풀 추정: 아이언클래드, Skill
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Skill
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.DEMON_FORM

- ID: `card-demon-form`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.FEED

- ID: `card-feed`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.HEADBUTT

- ID: `card-headbutt`
- 그룹/풀 추정: Attack
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Attack
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.IMPERVIOUS

- ID: `card-impervious`
- 그룹/풀 추정: Skill
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Skill
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.INFECTION

- ID: `card-infection`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.INFLAME

- ID: `card-inflame`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.IRON_WAVE

- ID: `card-iron-wave`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.JUGGLING

- ID: `card-juggling`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.MOLTEN_FIST

- ID: `card-molten-fist`
- 그룹/풀 추정: Attack
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Attack
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.PERFECTED_STRIKE

- ID: `card-perfected-strike`
- 그룹/풀 추정: Attack
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Attack
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.PILLAGE

- ID: `card-pillage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.POMMEL_STRIKE

- ID: `card-pommel-strike`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.SEEKER_STRIKE

- ID: `card-seeker-strike`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.SETUP_STRIKE

- ID: `card-setup-strike`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.SHRUG_IT_OFF

- ID: `card-shrug-it-off`
- 그룹/풀 추정: Skill
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Skill
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.SPITE

- ID: `card-spite`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.STOMP

- ID: `card-stomp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.STONE_ARMOR

- ID: `card-stone-armor`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.STRIKE_IRONCLAD

- ID: `card-strike-ironclad`
- 그룹/풀 추정: 아이언클래드, Attack
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 구조 정보:
  - 유형: Attack
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.SWORD_BOOMERANG

- ID: `card-sword-boomerang`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.THUNDERCLAP

- ID: `card-thunderclap`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.TRUE_GRIT

- ID: `card-true-grit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.TWIN_STRIKE

- ID: `card-twin-strike`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.UNRELENTING

- ID: `card-unrelenting`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CARD.VICIOUS

- ID: `card-vicious`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:card-added`

### CardPreviewContainer

- ID: `cardpreviewcontainer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### EventCardPreviewContainer

- ID: `eventcardpreviewcontainer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### GridCardPreviewContainer

- ID: `gridcardpreviewcontainer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### MessyCardPreviewContainer

- ID: `messycardpreviewcontainer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-choice`

### Drain Power

- ID: `megacrit-sts2-core-models-cards-drainpower`
- 그룹/풀 추정: Attack, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Attack
  - 희귀도: Common
  - 대상: AnyEnemy
  - 카드 풀: necrobinder
  - 강화 요약: Damage+2, Cards+1
- 관찰 로그 반영: 아니오
- 주요 소스: `strict-domain-parse`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.DrainPower`
- 리소스 경로: `res://images/packed/card_portraits/necrobinder/drain_power.pngr`

### 그을음

- ID: `megacrit-sts2-core-models-cards-soot`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SOOT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Soot`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/soot.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 등반가의 골칫거리

- ID: `megacrit-sts2-core-models-cards-ascendersbane`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ASCENDERS_BANE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.AscendersBane`
- 리소스 경로: `res://images/packed/card_portraits/curse/ascenders_bane.pngtz`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 몸부림

- ID: `megacrit-sts2-core-models-cards-writhe`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WRITHE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Writhe`
- 리소스 경로: `res://images/packed/card_portraits/curse/writhe.pngb`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 방울의 저주

- ID: `megacrit-sts2-core-models-cards-curseofthebell`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CURSE_OF_THE_BELL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.CurseOfTheBell`
- 리소스 경로: `res://images/packed/card_portraits/curse/curse_of_the_bell.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 부상

- ID: `megacrit-sts2-core-models-cards-wound`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WOUND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Wound`
- 리소스 경로: `res://images/packed/card_portraits/status/wound.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 상처

- ID: `megacrit-sts2-core-models-cards-injury`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INJURY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Injury`
- 리소스 경로: `res://images/packed/card_portraits/curse/injury.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 서투름

- ID: `megacrit-sts2-core-models-cards-clumsy`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLUMSY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Clumsy`
- 리소스 경로: `res://images/packed/card_portraits/curse/clumsy.pngj`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수면 부족

- ID: `megacrit-sts2-core-models-cards-poorsleep`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POOR_SLEEP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.PoorSleep`
- 리소스 경로: `res://images/packed/card_portraits/curse/beta/poor_sleep.pnga`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어리석음

- ID: `megacrit-sts2-core-models-cards-folly`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FOLLY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Folly`
- 리소스 경로: `res://images/packed/card_portraits/curse/folly.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 어지러움

- ID: `megacrit-sts2-core-models-cards-dazed`
- 그룹/풀 추정: Status, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DAZED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Dazed`
- 리소스 경로: `res://images/packed/card_portraits/status/dazed.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잔해

- ID: `megacrit-sts2-core-models-cards-debris`
- 그룹/풀 추정: Status, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Status
  - 희귀도: Status
  - 대상: None
  - 카드 풀: status
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEBRIS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Debris`
- 리소스 경로: `res://images/packed/card_portraits/status/beta/debris.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탐욕

- ID: `megacrit-sts2-core-models-cards-greed`
- 그룹/풀 추정: 저주, Curse, 코스트 -1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: -1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GREED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.Greed`
- 리소스 경로: `res://images/packed/card_portraits/curse/beta/greed.pngo`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포자 잠식

- ID: `megacrit-sts2-core-models-cards-sporemind`
- 그룹/풀 추정: 저주, Curse, 코스트 1
- 플레이 중 참조 시점: 카드 보상, 상점 구매, 덱 점검 장면에서 직접 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 구조 정보:
  - 코스트: 1
  - 유형: Curse
  - 희귀도: Curse
  - 대상: None
  - 카드 풀: curse
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPORE_MIND`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Cards.SporeMind`
- 리소스 경로: `res://images/packed/card_portraits/curse/beta/spore_mind.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

