# 키워드/의도

- 전체 항목 수: 1758
- 설명 본문이 채워진 항목: 3
- L10N 키 또는 제목이 연결된 항목: 3
- 선택지/옵션 정보가 있는 항목: 0

## 이 섹션이 도와주는 플레이 장면

- 카드 설명 해석
- 상태 이상/버프 파악
- 몬스터 Intent 해석

## 현재 이 섹션에서 확인된 것

- 키워드 제목
- 한국어 설명
- 관련 파워/Intent 클래스

## 아직 남은 점

- 실제 카드 문구와 교차 검증
- 게임 내 의도 로직과의 연계

## 주요 L10N/리소스 힌트

- `localization/kor/card_keywords.json`
- `localization/eng/card_keywords.json`
- `localization/kor/intents.json`

## 항목 목록

### 희망의 등불

- ID: `megacrit-sts2-core-monstermoves-intents-buffintent`
- 그룹/풀 추정: 의도/Intent, 설명 확보
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 동안 [gold]방어도[/gold]를 얻을 때마다, 다른 플레이어가 그 절반만큼 [gold]방어도[/gold]를 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BEACON_OF_HOPE_POWER`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.BuffIntent`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 힘

- ID: `megacrit-sts2-core-monstermoves-intents-attackintent`
- 그룹/풀 추정: 의도/Intent, 설명 확보
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 힘은 공격 카드의 피해량을 증가시킵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STRENGTH_POWER`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 힘 지배

- ID: `megacrit-sts2-core-monstermoves-intents-abstractintent`
- 그룹/풀 추정: 의도/Intent, 설명 확보
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 처치 시, 빼앗은 모든 [gold]힘[/gold]이 플레이어에게 돌아갑니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POSSESS_STRENGTH_POWER`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.AbstractIntent`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### "filename": "intent buff

- ID: `filename----intent-buff-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_buff.png",`

### "filename": "intent death blow

- ID: `filename----intent-death-blow-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_death_blow.png",`

### "filename": "intent debuff

- ID: `filename----intent-debuff-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_debuff.png",`

### "filename": "intent defend

- ID: `filename----intent-defend-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_defend.png",`

### "filename": "intent escape

- ID: `filename----intent-escape-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_escape.png",`

### "filename": "intent heal

- ID: `filename----intent-heal-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_heal.png",`

### "filename": "intent hidden

- ID: `filename----intent-hidden-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_hidden.png",`

### "filename": "intent sleep

- ID: `filename----intent-sleep-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_sleep.png",`

### "filename": "intent stun

- ID: `filename----intent-stun-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_stun.png",`

### "filename": "intent summon

- ID: `filename----intent-summon-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_summon.png",`

### "filename": "intent unknown

- ID: `filename----intent-unknown-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "intent_unknown.png",`

### "image": "intent atlas

- ID: `image----intent-atlas-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"image": "intent_atlas.png",`

### <0>  Encounter Or Deprecated

- ID: `0---encounterordeprecated`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <1>  Encounter Or Deprecated

- ID: `1---encounterordeprecated`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <5>  Encounter Info Ctor Param Init

- ID: `5---encounterinfoctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <7>  Keywords Ctor Param Init

- ID: `7---keywordsctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <9>  Encounter Stats Ctor Param Init

- ID: `9---encounterstatsctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <>c

- ID: `megacrit-sts2-core-entities-intents-intentanimdata---c`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Intents.IntentAnimData+<>c`

### <>c

- ID: `megacrit-sts2-core-models-encounters-corpseslugsnormal---c`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CorpseSlugsNormal+<>c`

### <>c

- ID: `megacrit-sts2-core-models-encounters-corpseslugsweak---c`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CorpseSlugsWeak+<>c`

### <>c

- ID: `megacrit-sts2-core-models-encounters-slitheringstranglernormal---c`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlitheringStranglerNormal+<>c`

### <>c

- ID: `megacrit-sts2-core-saves-encounterstats---c`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Saves.EncounterStats+<>c`

### <>c  Display Class10 0

- ID: `megacrit-sts2-core-models-encounters-bowlbugsnormal---c--displayclass10-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BowlbugsNormal+<>c__DisplayClass10_0`

### <>c  Display Class10 1

- ID: `megacrit-sts2-core-models-encounters-bowlbugsnormal---c--displayclass10-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BowlbugsNormal+<>c__DisplayClass10_1`

### <>c  Display Class12 0

- ID: `megacrit-sts2-core-saves-encounterstats---c--displayclass12-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Saves.EncounterStats+<>c__DisplayClass12_0`

### <>c  Display Class13 0

- ID: `megacrit-sts2-core-saves-encounterstats---c--displayclass13-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Saves.EncounterStats+<>c__DisplayClass13_0`

### <>c  Display Class26 0

- ID: `megacrit-sts2-core-models-encountermodel---c--displayclass26-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.EncounterModel+<>c__DisplayClass26_0`

### <>c  Display Class26 1

- ID: `megacrit-sts2-core-models-encountermodel---c--displayclass26-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.EncounterModel+<>c__DisplayClass26_1`

### <>c  Display Class4 0

- ID: `megacrit-sts2-core-monstermoves-intents-singleattackintent---c--displayclass4-0`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.SingleAttackIntent+<>c__DisplayClass4_0`

### <>c  Display Class5 0

- ID: `megacrit-sts2-core-models-encounters-rubyraidersnormal---c--displayclass5-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.RubyRaidersNormal+<>c__DisplayClass5_0`

### <>c  Display Class5 1

- ID: `megacrit-sts2-core-models-encounters-rubyraidersnormal---c--displayclass5-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.RubyRaidersNormal+<>c__DisplayClass5_1`

### <>c  Display Class6 0

- ID: `megacrit-sts2-core-monstermoves-intents-multiattackintent---c--displayclass6-0`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.MultiAttackIntent+<>c__DisplayClass6_0`

### <>c  Display Class7 0

- ID: `megacrit-sts2-core-monstermoves-intents-multiattackintent---c--displayclass7-0`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.MultiAttackIntent+<>c__DisplayClass7_0`

### <Boss Encounters Visited>k  Backing Field

- ID: `bossencountersvisited-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Bot Keyword>k  Backing Field

- ID: `botkeyword-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Info>b  1

- ID: `create-encounterinfo-b--1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Info>b  42 0

- ID: `create-encounterinfo-b--42-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Info>b  42 2

- ID: `create-encounterinfo-b--42-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Metric>b  1

- ID: `create-encountermetric-b--1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Metric>b  49 0

- ID: `create-encountermetric-b--49-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Metric>b  49 2

- ID: `create-encountermetric-b--49-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Stats>b  1

- ID: `create-encounterstats-b--1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Stats>b  214 0

- ID: `create-encounterstats-b--214-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Encounter Stats>b  214 2

- ID: `create-encounterstats-b--214-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Keywords>b  1

- ID: `create-keywords-b--1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Keywords>b  61 0

- ID: `create-keywords-b--61-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Keywords>b  61 2

- ID: `create-keywords-b--61-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create List Encounter Metric>b  119 0

- ID: `create-listencountermetric-b--119-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create List Encounter Stats>b  570 0

- ID: `create-listencounterstats-b--570-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Custom Description Encounter Source Id>k  Backing Field

- ID: `customdescriptionencountersourceid-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Elite Encounter Ids>k  Backing Field

- ID: `eliteencounterids-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Elite Encounters Visited>k  Backing Field

- ID: `eliteencountersvisited-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Id>k  Backing Field

- ID: `encounterid-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 0

- ID: `encounterinfopropinit-b--43-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 1

- ID: `encounterinfopropinit-b--43-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 10

- ID: `encounterinfopropinit-b--43-10`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 11

- ID: `encounterinfopropinit-b--43-11`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 12

- ID: `encounterinfopropinit-b--43-12`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 13

- ID: `encounterinfopropinit-b--43-13`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 14

- ID: `encounterinfopropinit-b--43-14`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 15

- ID: `encounterinfopropinit-b--43-15`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 16

- ID: `encounterinfopropinit-b--43-16`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 17

- ID: `encounterinfopropinit-b--43-17`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 2

- ID: `encounterinfopropinit-b--43-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 3

- ID: `encounterinfopropinit-b--43-3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 4

- ID: `encounterinfopropinit-b--43-4`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 5

- ID: `encounterinfopropinit-b--43-5`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 6

- ID: `encounterinfopropinit-b--43-6`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 7

- ID: `encounterinfopropinit-b--43-7`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 8

- ID: `encounterinfopropinit-b--43-8`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Info Prop Init>b  43 9

- ID: `encounterinfopropinit-b--43-9`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 0

- ID: `encountermetricpropinit-b--50-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 1

- ID: `encountermetricpropinit-b--50-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 2

- ID: `encountermetricpropinit-b--50-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 3

- ID: `encountermetricpropinit-b--50-3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 4

- ID: `encountermetricpropinit-b--50-4`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Metric Prop Init>b  50 5

- ID: `encountermetricpropinit-b--50-5`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Slots>k  Backing Field

- ID: `encounterslots-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter State>k  Backing Field

- ID: `encounterstate-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 0

- ID: `encounterstatspropinit-b--215-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 1

- ID: `encounterstatspropinit-b--215-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 2

- ID: `encounterstatspropinit-b--215-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 3

- ID: `encounterstatspropinit-b--215-3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 4

- ID: `encounterstatspropinit-b--215-4`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 5

- ID: `encounterstatspropinit-b--215-5`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 6

- ID: `encounterstatspropinit-b--215-6`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats Prop Init>b  215 7

- ID: `encounterstatspropinit-b--215-7`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter Stats>k  Backing Field

- ID: `encounterstats-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <encounter>5  6

- ID: `encounter-5--6`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounter>k  Backing Field

- ID: `encounter-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounters Seen>k  Backing Field

- ID: `encountersseen-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Encounters>k  Backing Field

- ID: `encounters-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get All Boss Encounters>b  55 0

- ID: `get-allbossencounters-b--55-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get All Elite Encounters>b  52 0

- ID: `get-alleliteencounters-b--52-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get All Encounters>b  59 0

- ID: `get-allencounters-b--59-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get All Regular Encounters>b  49 0

- ID: `get-allregularencounters-b--49-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get All Weak Encounters>b  46 0

- ID: `get-allweakencounters-b--46-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Get Elite Encounters>b  38 0

- ID: `geteliteencounters-b--38-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Get Elite Encounters>b  38 1

- ID: `geteliteencounters-b--38-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Get Elite Encounters>b  39 0

- ID: `geteliteencounters-b--39-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Get Elite Encounters>b  39 1

- ID: `geteliteencounters-b--39-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Increment Encounter Loss>b  0

- ID: `incrementencounterloss-b--0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Intent Container>k  Backing Field

- ID: `intentcontainer-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Intent Position>k  Backing Field

- ID: `intentposition-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Intents>k  Backing Field

- ID: `intents-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Is Debug Hide Mp Intents>k  Backing Field

- ID: `isdebughidempintents-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Is Debug Hiding Intent>k  Backing Field

- ID: `isdebughidingintent-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 0

- ID: `keywordspropinit-b--62-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 1

- ID: `keywordspropinit-b--62-1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 2

- ID: `keywordspropinit-b--62-2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 3

- ID: `keywordspropinit-b--62-3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 4

- ID: `keywordspropinit-b--62-4`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 5

- ID: `keywordspropinit-b--62-5`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 6

- ID: `keywordspropinit-b--62-6`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 7

- ID: `keywordspropinit-b--62-7`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Keywords Prop Init>b  62 8

- ID: `keywordspropinit-b--62-8`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Killed By Encounter>k  Backing Field

- ID: `killedbyencounter-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Normal Encounter Ids>k  Backing Field

- ID: `normalencounterids-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Normal Encounters Visited>k  Backing Field

- ID: `normalencountersvisited-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Perform Intent>d  71

- ID: `megacrit-sts2-core-nodes-combat-ncreature--performintent-d--71`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NCreature+<PerformIntent>d__71`

### <Player Intent Handler>k  Backing Field

- ID: `playerintenthandler-k--backingfield`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Refresh Intents>b  72 0

- ID: `refreshintents-b--72-0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Refresh Intents>d  72

- ID: `megacrit-sts2-core-nodes-combat-ncreature--refreshintents-d--72`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NCreature+<RefreshIntents>d__72`

### AbstractIntent

- ID: `res---src-core-monstermoves-intents-abstractintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/AbstractIntent.cs`

### Add Keyword

- ID: `addkeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Boss Encounters

- ID: `allbossencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Elite Encounters

- ID: `alleliteencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Encounters

- ID: `allencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Regular Encounters

- ID: `allregularencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Weak Encounters

- ID: `allweakencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Anim Hide Intent

- ID: `animhideintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Apply Keyword

- ID: `applykeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### AttackIntent

- ID: `res---src-core-monstermoves-intents-attackintent-cs3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/AttackIntent.cs3`

### Axebots Normal

- ID: `megacrit-sts2-core-models-encounters-axebotsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.AxebotsNormal`

### Boss Encounter

- ID: `bossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Boss Encounters Visited

- ID: `bossencountersvisited`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Bot Keyword

- ID: `botkeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Bowlbugs Normal

- ID: `megacrit-sts2-core-models-encounters-bowlbugsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BowlbugsNormal`

### Bowlbugs Weak

- ID: `megacrit-sts2-core-models-encounters-bowlbugsweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BowlbugsWeak`

### BuffIntent

- ID: `res---src-core-monstermoves-intents-buffintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/BuffIntent.cs`

### Bygone Effigy Elite

- ID: `megacrit-sts2-core-models-encounters-bygoneeffigyelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BygoneEffigyElite`

### Byrdonis Elite

- ID: `megacrit-sts2-core-models-encounters-byrdoniselite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ByrdonisElite`

### Canonical Encounter

- ID: `canonicalencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Canonical Keywords

- ID: `canonicalkeywords`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Ceremonial Beast Boss

- ID: `megacrit-sts2-core-models-encounters-ceremonialbeastboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CeremonialBeastBoss`

### Chompers Normal

- ID: `megacrit-sts2-core-models-encounters-chompersnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ChompersNormal`

### Construct Menagerie Normal

- ID: `megacrit-sts2-core-models-encounters-constructmenagerienormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ConstructMenagerieNormal`

### Corpse Slugs Normal

- ID: `megacrit-sts2-core-models-encounters-corpseslugsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CorpseSlugsNormal`

### Corpse Slugs Weak

- ID: `megacrit-sts2-core-models-encounters-corpseslugsweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CorpseSlugsWeak`

### Create Encounter Info

- ID: `create-encounterinfo`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create Encounter Metric

- ID: `create-encountermetric`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create Encounter Stats

- ID: `create-encounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create Keywords

- ID: `create-keywords`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create List Encounter Metric

- ID: `create-listencountermetric`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create List Encounter Stats

- ID: `create-listencounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Cubex Construct Normal

- ID: `megacrit-sts2-core-models-encounters-cubexconstructnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CubexConstructNormal`

### Cultists Normal

- ID: `megacrit-sts2-core-models-encounters-cultistsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.CultistsNormal`

### Custom Description Encounter Source Id

- ID: `customdescriptionencountersourceid`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Death Blow Intent

- ID: `megacrit-sts2-core-monstermoves-intents-deathblowintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.DeathBlowIntent`

### DeathBlowIntent

- ID: `res---src-core-monstermoves-intents-deathblowintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/DeathBlowIntent.cs`

### Debuff Intent

- ID: `megacrit-sts2-core-monstermoves-intents-debuffintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.DebuffIntent`

### DebuffIntent

- ID: `res---src-core-monstermoves-intents-debuffintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/DebuffIntent.cs`

### Debug Toggle Intent

- ID: `debugtoggleintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Decimillipede Elite

- ID: `megacrit-sts2-core-models-encounters-decimillipedeelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.DecimillipedeElite`

### Defend Intent

- ID: `megacrit-sts2-core-monstermoves-intents-defendintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.DefendIntent`

### DefendIntent

- ID: `res---src-core-monstermoves-intents-defendintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/DefendIntent.cs`

### Deprecated Encounter

- ID: `megacrit-sts2-core-models-encounters-deprecatedencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.DeprecatedEncounter`

### Devoted Sculptor Weak

- ID: `megacrit-sts2-core-models-encounters-devotedsculptorweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.DevotedSculptorWeak`

### Doormaker Boss

- ID: `megacrit-sts2-core-models-encounters-doormakerboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.DoormakerBoss`

### Elite Encounter

- ID: `eliteencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Elite Encounter Ids

- ID: `eliteencounterids`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### elite Encounters

- ID: `eliteencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Elite Encounters Visited

- ID: `eliteencountersvisited`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter

- ID: `encounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Id

- ID: `encounterid`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Info

- ID: `encounterinfo`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Info

- ID: `megacrit-sts2-gameinfo-objects-encounterinfo`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.GameInfo.Objects.EncounterInfo`

### Encounter Info Ctor Param Init

- ID: `encounterinfoctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Info Prop Init

- ID: `encounterinfopropinit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Info Serialize Handler

- ID: `encounterinfoserializehandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Metric

- ID: `encountermetric`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Metric

- ID: `megacrit-sts2-core-runs-metrics-encountermetric`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Runs.Metrics.EncounterMetric`

### Encounter Metric Prop Init

- ID: `encountermetricpropinit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Metric Serialize Handler

- ID: `encountermetricserializehandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Model

- ID: `megacrit-sts2-core-models-encountermodel`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.EncounterModel`

### Encounter Or Deprecated

- ID: `encounterordeprecated`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### encounter Quote

- ID: `encounterquote`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Slots

- ID: `encounterslots`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter State

- ID: `encounterstate`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Stats

- ID: `encounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Stats

- ID: `megacrit-sts2-core-saves-encounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Saves.EncounterStats`

### Encounter Stats Ctor Param Init

- ID: `encounterstatsctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Stats Prop Init

- ID: `encounterstatspropinit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounter Tag

- ID: `megacrit-sts2-core-entities-encounters-encountertag`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Encounters.EncounterTag`

### Encounters

- ID: `encounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Encounters Seen

- ID: `encountersseen`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Entomancer Elite

- ID: `megacrit-sts2-core-models-encounters-entomancerelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.EntomancerElite`

### Escape Intent

- ID: `megacrit-sts2-core-monstermoves-intents-escapeintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.EscapeIntent`

### EscapeIntent

- ID: `res---src-core-monstermoves-intents-escapeintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/EscapeIntent.cs`

### Exoskeletons Normal

- ID: `megacrit-sts2-core-models-encounters-exoskeletonsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ExoskeletonsNormal`

### Exoskeletons Weak

- ID: `megacrit-sts2-core-models-encounters-exoskeletonsweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ExoskeletonsWeak`

### Fabricator Normal

- ID: `megacrit-sts2-core-models-encounters-fabricatornormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FabricatorNormal`

### Flyconid Normal

- ID: `megacrit-sts2-core-models-encounters-flyconidnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FlyconidNormal`

### Fogmog Normal

- ID: `megacrit-sts2-core-models-encounters-fogmognormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FogmogNormal`

### Fossil Stalker Normal

- ID: `megacrit-sts2-core-models-encounters-fossilstalkernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FossilStalkerNormal`

### Frog Knight Normal

- ID: `megacrit-sts2-core-models-encounters-frogknightnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FrogKnightNormal`

### From Keyword

- ID: `fromkeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Fuzzy Wurm Crawler Weak

- ID: `megacrit-sts2-core-models-encounters-fuzzywurmcrawlerweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FuzzyWurmCrawlerWeak`

### Generate All Encounters

- ID: `generateallencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Elite Encounters

- ID: `geteliteencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Intent Description

- ID: `getintentdescription`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Intent Label

- ID: `getintentlabel`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Intents

- ID: `getintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Or Create Encounter Stats

- ID: `getorcreateencounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Globe Head Normal

- ID: `megacrit-sts2-core-models-encounters-globeheadnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.GlobeHeadNormal`

### Gremlin Merc Normal

- ID: `megacrit-sts2-core-models-encounters-gremlinmercnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.GremlinMercNormal`

### Has Intent Tip

- ID: `hasintenttip`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Has Seen Encounter

- ID: `hasseenencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Has Unplayable Keyword

- ID: `hasunplayablekeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Haunted Ship Normal

- ID: `megacrit-sts2-core-models-encounters-hauntedshipnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.HauntedShipNormal`

### Heal Intent

- ID: `megacrit-sts2-core-monstermoves-intents-healintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.HealIntent`

### HealIntent

- ID: `res---src-core-monstermoves-intents-healintent-csp---8`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/HealIntent.csP~~?8{`

### Hidden Intent

- ID: `megacrit-sts2-core-monstermoves-intents-hiddenintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.HiddenIntent`

### HiddenIntent.cs

- ID: `res---src-core-monstermoves-intents-hiddenintent-cs-n`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/HiddenIntent.cs.N`

### hide Intents

- ID: `hideintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### hide Mp Intents

- ID: `hidempintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Hunter Killer Normal

- ID: `megacrit-sts2-core-models-encounters-hunterkillernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.HunterKillerNormal`

### Increment Encounter Loss

- ID: `incrementencounterloss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Infested Prisms Elite

- ID: `megacrit-sts2-core-models-encounters-infestedprismselite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.InfestedPrismsElite`

### Inklets Normal

- ID: `megacrit-sts2-core-models-encounters-inkletsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.InkletsNormal`

### intent

- ID: `intent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent

- ID: `res---scenes-combat-intent-tscn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/combat/intent.tscn`

### intent

- ID: `scenes-combat-intent-tscn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/combat/intent.tscn`

### Intent Anim Data

- ID: `megacrit-sts2-core-entities-intents-intentanimdata`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Intents.IntentAnimData`

### intent atlas

- ID: `res---images-atlases-intent-atlas-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/atlases/intent_atlas.png`

### intent atlas.png

- ID: `images-atlases-intent-atlas-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/atlases/intent_atlas.png.import`

### intent atlas.png 86e200e292ed9a712cebf2afab1f3162.bptc

- ID: `godot-imported-intent-atlas-png-86e200e292ed9a712cebf2afab1f3162-bptc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_atlas.png-86e200e292ed9a712cebf2afab1f3162.bptc.ctex`

### intent atlas.png 86e200e292ed9a712cebf2afab1f3162.bptc

- ID: `path-bptc--res----godot-imported-intent-atlas-png-86e200e292ed9a712cebf2afab1f3162-bptc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.bptc="res://.godot/imported/intent_atlas.png-86e200e292ed9a712cebf2afab1f3162.bptc.ctex"`

### intent attack 1

- ID: `filename----attack-intent-attack-1-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "attack/intent_attack_1.png",`

### intent attack 1

- ID: `res---images-packed-intents-attack-intent-attack-1-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/attack/intent_attack_1.png`

### intent attack 1.png

- ID: `images-packed-intents-attack-intent-attack-1-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/attack/intent_attack_1.png.import`

### intent attack 1.png 01ff5669b21f8c81783406b0ba9315d0

- ID: `godot-imported-intent-attack-1-png-01ff5669b21f8c81783406b0ba9315d0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_attack_1.png-01ff5669b21f8c81783406b0ba9315d0.ctex`

### intent attack 1.png 01ff5669b21f8c81783406b0ba9315d0

- ID: `path--res----godot-imported-intent-attack-1-png-01ff5669b21f8c81783406b0ba9315d0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_attack_1.png-01ff5669b21f8c81783406b0ba9315d0.ctex"`

### intent attack 2

- ID: `filename----attack-intent-attack-2-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "attack/intent_attack_2.png",`

### intent attack 2

- ID: `res---images-packed-intents-attack-intent-attack-2-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/attack/intent_attack_2.png`

### intent attack 2.png

- ID: `images-packed-intents-attack-intent-attack-2-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/attack/intent_attack_2.png.import`

### intent attack 2.png 10525ee2c7c80c5ef7789e963df8a855

- ID: `godot-imported-intent-attack-2-png-10525ee2c7c80c5ef7789e963df8a855-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_attack_2.png-10525ee2c7c80c5ef7789e963df8a855.ctex`

### intent attack 2.png 10525ee2c7c80c5ef7789e963df8a855

- ID: `path--res----godot-imported-intent-attack-2-png-10525ee2c7c80c5ef7789e963df8a855-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_attack_2.png-10525ee2c7c80c5ef7789e963df8a855.ctex"`

### intent attack 3

- ID: `filename----attack-intent-attack-3-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "attack/intent_attack_3.png",`

### intent attack 3

- ID: `res---images-packed-intents-attack-intent-attack-3-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/attack/intent_attack_3.png`

### intent attack 3.png

- ID: `images-packed-intents-attack-intent-attack-3-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/attack/intent_attack_3.png.import`

### intent attack 3.png 3f74548d8a510f629a7d3590571729f3

- ID: `godot-imported-intent-attack-3-png-3f74548d8a510f629a7d3590571729f3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_attack_3.png-3f74548d8a510f629a7d3590571729f3.ctex`

### intent attack 3.png 3f74548d8a510f629a7d3590571729f3

- ID: `path--res----godot-imported-intent-attack-3-png-3f74548d8a510f629a7d3590571729f3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_attack_3.png-3f74548d8a510f629a7d3590571729f3.ctex"`

### intent attack 4

- ID: `filename----attack-intent-attack-4-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "attack/intent_attack_4.png",`

### intent attack 4

- ID: `res---images-packed-intents-attack-intent-attack-4-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/attack/intent_attack_4.png^`

### intent attack 4.png

- ID: `images-packed-intents-attack-intent-attack-4-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/attack/intent_attack_4.png.import`

### intent attack 4.png 17e5ccd38049eebaaf138779dfbc672d

- ID: `godot-imported-intent-attack-4-png-17e5ccd38049eebaaf138779dfbc672d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_attack_4.png-17e5ccd38049eebaaf138779dfbc672d.ctex`

### intent attack 4.png 17e5ccd38049eebaaf138779dfbc672d

- ID: `path--res----godot-imported-intent-attack-4-png-17e5ccd38049eebaaf138779dfbc672d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_attack_4.png-17e5ccd38049eebaaf138779dfbc672d.ctex"`

### intent attack 5

- ID: `filename----attack-intent-attack-5-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "attack/intent_attack_5.png",`

### intent attack 5

- ID: `res---images-packed-intents-attack-intent-attack-5-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/attack/intent_attack_5.png`

### intent attack 5.png

- ID: `images-packed-intents-attack-intent-attack-5-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/attack/intent_attack_5.png.import`

### intent attack 5.png 5f7101f418b70b651919c15fdde6d069

- ID: `godot-imported-intent-attack-5-png-5f7101f418b70b651919c15fdde6d069-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_attack_5.png-5f7101f418b70b651919c15fdde6d069.ctex`

### intent attack 5.png 5f7101f418b70b651919c15fdde6d069

- ID: `path--res----godot-imported-intent-attack-5-png-5f7101f418b70b651919c15fdde6d069-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_attack_5.png-5f7101f418b70b651919c15fdde6d069.ctex"`

### intent buff

- ID: `res---images-packed-intents-intent-buff-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_buff.png`

### intent buff 00

- ID: `filename----buff-intent-buff-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_00.png",`

### intent buff 00

- ID: `res---images-packed-intents-buff-intent-buff-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_00.png`

### intent buff 00.png

- ID: `images-packed-intents-buff-intent-buff-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_00.png.import`

### intent buff 00.png 53ffae72641447370691e05ba1de6afd

- ID: `godot-imported-intent-buff-00-png-53ffae72641447370691e05ba1de6afd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_00.png-53ffae72641447370691e05ba1de6afd.ctex`

### intent buff 00.png 53ffae72641447370691e05ba1de6afd

- ID: `path--res----godot-imported-intent-buff-00-png-53ffae72641447370691e05ba1de6afd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_00.png-53ffae72641447370691e05ba1de6afd.ctex"`

### intent buff 01

- ID: `filename----buff-intent-buff-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_01.png",`

### intent buff 01

- ID: `res---images-packed-intents-buff-intent-buff-01-pngl`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_01.pngl`

### intent buff 01.png

- ID: `images-packed-intents-buff-intent-buff-01-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_01.png.importP`

### intent buff 01.png 52ff906bcaf64f3be46f02a083fb94e6

- ID: `godot-imported-intent-buff-01-png-52ff906bcaf64f3be46f02a083fb94e6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_01.png-52ff906bcaf64f3be46f02a083fb94e6.ctex`

### intent buff 01.png 52ff906bcaf64f3be46f02a083fb94e6

- ID: `path--res----godot-imported-intent-buff-01-png-52ff906bcaf64f3be46f02a083fb94e6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_01.png-52ff906bcaf64f3be46f02a083fb94e6.ctex"`

### intent buff 02

- ID: `filename----buff-intent-buff-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_02.png",`

### intent buff 02

- ID: `res---images-packed-intents-buff-intent-buff-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_02.png:`

### intent buff 02.png

- ID: `images-packed-intents-buff-intent-buff-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_02.png.import`

### intent buff 02.png 330fa4b11be83cf03bad0cba612b70af

- ID: `godot-imported-intent-buff-02-png-330fa4b11be83cf03bad0cba612b70af-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_02.png-330fa4b11be83cf03bad0cba612b70af.ctex`

### intent buff 02.png 330fa4b11be83cf03bad0cba612b70af

- ID: `path--res----godot-imported-intent-buff-02-png-330fa4b11be83cf03bad0cba612b70af-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_02.png-330fa4b11be83cf03bad0cba612b70af.ctex"`

### intent buff 03

- ID: `filename----buff-intent-buff-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_03.png",`

### intent buff 03

- ID: `res---images-packed-intents-buff-intent-buff-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_03.png`

### intent buff 03.png

- ID: `images-packed-intents-buff-intent-buff-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_03.png.import`

### intent buff 03.png 60f7a75e8c6cde6edf9ade2241e0f8dc

- ID: `godot-imported-intent-buff-03-png-60f7a75e8c6cde6edf9ade2241e0f8dc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_03.png-60f7a75e8c6cde6edf9ade2241e0f8dc.ctex`

### intent buff 03.png 60f7a75e8c6cde6edf9ade2241e0f8dc

- ID: `path--res----godot-imported-intent-buff-03-png-60f7a75e8c6cde6edf9ade2241e0f8dc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_03.png-60f7a75e8c6cde6edf9ade2241e0f8dc.ctex"`

### intent buff 04

- ID: `filename----buff-intent-buff-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_04.png",`

### intent buff 04

- ID: `res---images-packed-intents-buff-intent-buff-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_04.png~`

### intent buff 04.png

- ID: `images-packed-intents-buff-intent-buff-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_04.png.import`

### intent buff 04.png 2da85baaf53f384df23c11309b1f51f8

- ID: `godot-imported-intent-buff-04-png-2da85baaf53f384df23c11309b1f51f8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_04.png-2da85baaf53f384df23c11309b1f51f8.ctex`

### intent buff 04.png 2da85baaf53f384df23c11309b1f51f8

- ID: `path--res----godot-imported-intent-buff-04-png-2da85baaf53f384df23c11309b1f51f8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_04.png-2da85baaf53f384df23c11309b1f51f8.ctex"`

### intent buff 05

- ID: `filename----buff-intent-buff-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_05.png",`

### intent buff 05

- ID: `res---images-packed-intents-buff-intent-buff-05-png2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_05.png2`

### intent buff 05.png

- ID: `images-packed-intents-buff-intent-buff-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_05.png.import`

### intent buff 05.png 3ff0fb3cfdf107eda891e72ee855aace

- ID: `godot-imported-intent-buff-05-png-3ff0fb3cfdf107eda891e72ee855aace-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_05.png-3ff0fb3cfdf107eda891e72ee855aace.ctex`

### intent buff 05.png 3ff0fb3cfdf107eda891e72ee855aace

- ID: `path--res----godot-imported-intent-buff-05-png-3ff0fb3cfdf107eda891e72ee855aace-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_05.png-3ff0fb3cfdf107eda891e72ee855aace.ctex"`

### intent buff 06

- ID: `filename----buff-intent-buff-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_06.png",`

### intent buff 06

- ID: `res---images-packed-intents-buff-intent-buff-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_06.png`

### intent buff 06.png

- ID: `images-packed-intents-buff-intent-buff-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_06.png.import@`

### intent buff 06.png cae0849f7be93dcc4b73fe9a70b11a20

- ID: `godot-imported-intent-buff-06-png-cae0849f7be93dcc4b73fe9a70b11a20-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_06.png-cae0849f7be93dcc4b73fe9a70b11a20.ctex`

### intent buff 06.png cae0849f7be93dcc4b73fe9a70b11a20

- ID: `path--res----godot-imported-intent-buff-06-png-cae0849f7be93dcc4b73fe9a70b11a20-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_06.png-cae0849f7be93dcc4b73fe9a70b11a20.ctex"`

### intent buff 07

- ID: `filename----buff-intent-buff-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_07.png",`

### intent buff 07

- ID: `res---images-packed-intents-buff-intent-buff-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_07.png`

### intent buff 07.png

- ID: `images-packed-intents-buff-intent-buff-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_07.png.import`

### intent buff 07.png 769d2d4428d237756210d6e9fd0699f4

- ID: `godot-imported-intent-buff-07-png-769d2d4428d237756210d6e9fd0699f4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_07.png-769d2d4428d237756210d6e9fd0699f4.ctex`

### intent buff 07.png 769d2d4428d237756210d6e9fd0699f4

- ID: `path--res----godot-imported-intent-buff-07-png-769d2d4428d237756210d6e9fd0699f4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_07.png-769d2d4428d237756210d6e9fd0699f4.ctex"`

### intent buff 08

- ID: `filename----buff-intent-buff-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_08.png",`

### intent buff 08

- ID: `res---images-packed-intents-buff-intent-buff-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_08.png*`

### intent buff 08.png

- ID: `images-packed-intents-buff-intent-buff-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_08.png.import``

### intent buff 08.png c91635de99f03f18083f6f974811654d

- ID: `godot-imported-intent-buff-08-png-c91635de99f03f18083f6f974811654d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_08.png-c91635de99f03f18083f6f974811654d.ctex`

### intent buff 08.png c91635de99f03f18083f6f974811654d

- ID: `path--res----godot-imported-intent-buff-08-png-c91635de99f03f18083f6f974811654d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_08.png-c91635de99f03f18083f6f974811654d.ctex"`

### intent buff 09

- ID: `filename----buff-intent-buff-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_09.png",`

### intent buff 09

- ID: `res---images-packed-intents-buff-intent-buff-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_09.png`

### intent buff 09.png

- ID: `images-packed-intents-buff-intent-buff-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_09.png.import`

### intent buff 09.png 49b8aedc89828c39c37e2e20cc90c7ae

- ID: `godot-imported-intent-buff-09-png-49b8aedc89828c39c37e2e20cc90c7ae-ctex0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_09.png-49b8aedc89828c39c37e2e20cc90c7ae.ctex0`

### intent buff 09.png 49b8aedc89828c39c37e2e20cc90c7ae

- ID: `path--res----godot-imported-intent-buff-09-png-49b8aedc89828c39c37e2e20cc90c7ae-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_09.png-49b8aedc89828c39c37e2e20cc90c7ae.ctex"`

### intent buff 10

- ID: `filename----buff-intent-buff-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_10.png",`

### intent buff 10

- ID: `res---images-packed-intents-buff-intent-buff-10-png-t`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_10.png*T`

### intent buff 10.png

- ID: `images-packed-intents-buff-intent-buff-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_10.png.import`

### intent buff 10.png a9943cbe87a84e0faa541e000a3953f3

- ID: `godot-imported-intent-buff-10-png-a9943cbe87a84e0faa541e000a3953f3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_10.png-a9943cbe87a84e0faa541e000a3953f3.ctex`

### intent buff 10.png a9943cbe87a84e0faa541e000a3953f3

- ID: `path--res----godot-imported-intent-buff-10-png-a9943cbe87a84e0faa541e000a3953f3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_10.png-a9943cbe87a84e0faa541e000a3953f3.ctex"`

### intent buff 11

- ID: `filename----buff-intent-buff-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_11.png",`

### intent buff 11

- ID: `res---images-packed-intents-buff-intent-buff-11-pngl`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_11.pngL`

### intent buff 11.png

- ID: `images-packed-intents-buff-intent-buff-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_11.png.import`

### intent buff 11.png b35d20ae1e56a07ef08801a3514e8b0c

- ID: `godot-imported-intent-buff-11-png-b35d20ae1e56a07ef08801a3514e8b0c-ctexp2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_11.png-b35d20ae1e56a07ef08801a3514e8b0c.ctexP2`

### intent buff 11.png b35d20ae1e56a07ef08801a3514e8b0c

- ID: `path--res----godot-imported-intent-buff-11-png-b35d20ae1e56a07ef08801a3514e8b0c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_11.png-b35d20ae1e56a07ef08801a3514e8b0c.ctex"`

### intent buff 12

- ID: `filename----buff-intent-buff-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_12.png",`

### intent buff 12

- ID: `res---images-packed-intents-buff-intent-buff-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_12.png`

### intent buff 12.png

- ID: `images-packed-intents-buff-intent-buff-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_12.png.import`

### intent buff 12.png 2cf5951596d9bc876fae8d104df92a43

- ID: `godot-imported-intent-buff-12-png-2cf5951596d9bc876fae8d104df92a43-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_12.png-2cf5951596d9bc876fae8d104df92a43.ctex`

### intent buff 12.png 2cf5951596d9bc876fae8d104df92a43

- ID: `path--res----godot-imported-intent-buff-12-png-2cf5951596d9bc876fae8d104df92a43-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_12.png-2cf5951596d9bc876fae8d104df92a43.ctex"`

### intent buff 13

- ID: `filename----buff-intent-buff-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_13.png",`

### intent buff 13

- ID: `res---images-packed-intents-buff-intent-buff-13-pngtgyc`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_13.pngTGyC[`

### intent buff 13.png

- ID: `images-packed-intents-buff-intent-buff-13-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_13.png.import0]`

### intent buff 13.png bfde0ca25dfd8f68804d97eefad2d06c

- ID: `godot-imported-intent-buff-13-png-bfde0ca25dfd8f68804d97eefad2d06c-ctexpo`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_13.png-bfde0ca25dfd8f68804d97eefad2d06c.ctexpO`

### intent buff 13.png bfde0ca25dfd8f68804d97eefad2d06c

- ID: `path--res----godot-imported-intent-buff-13-png-bfde0ca25dfd8f68804d97eefad2d06c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_13.png-bfde0ca25dfd8f68804d97eefad2d06c.ctex"`

### intent buff 14

- ID: `filename----buff-intent-buff-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_14.png",`

### intent buff 14

- ID: `res---images-packed-intents-buff-intent-buff-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_14.png`

### intent buff 14.png

- ID: `images-packed-intents-buff-intent-buff-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_14.png.import`

### intent buff 14.png 46aaa88003fa8ceafb0a94c6ac5e3af2

- ID: `godot-imported-intent-buff-14-png-46aaa88003fa8ceafb0a94c6ac5e3af2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_14.png-46aaa88003fa8ceafb0a94c6ac5e3af2.ctex`

### intent buff 14.png 46aaa88003fa8ceafb0a94c6ac5e3af2

- ID: `path--res----godot-imported-intent-buff-14-png-46aaa88003fa8ceafb0a94c6ac5e3af2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_14.png-46aaa88003fa8ceafb0a94c6ac5e3af2.ctex"`

### intent buff 15

- ID: `filename----buff-intent-buff-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_15.png",`

### intent buff 15

- ID: `res---images-packed-intents-buff-intent-buff-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_15.png`

### intent buff 15.png

- ID: `images-packed-intents-buff-intent-buff-15-png-importpz`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_15.png.importPz`

### intent buff 15.png ccee59dd80ae27d2f9fb572c518de4d1

- ID: `godot-imported-intent-buff-15-png-ccee59dd80ae27d2f9fb572c518de4d1-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_15.png-ccee59dd80ae27d2f9fb572c518de4d1.ctex`

### intent buff 15.png ccee59dd80ae27d2f9fb572c518de4d1

- ID: `path--res----godot-imported-intent-buff-15-png-ccee59dd80ae27d2f9fb572c518de4d1-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_15.png-ccee59dd80ae27d2f9fb572c518de4d1.ctex"`

### intent buff 16

- ID: `filename----buff-intent-buff-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_16.png",`

### intent buff 16

- ID: `res---images-packed-intents-buff-intent-buff-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_16.png`

### intent buff 16.png

- ID: `images-packed-intents-buff-intent-buff-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_16.png.import`

### intent buff 16.png de9b1fc0a3f894e98b7674528564b4f0

- ID: `godot-imported-intent-buff-16-png-de9b1fc0a3f894e98b7674528564b4f0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_16.png-de9b1fc0a3f894e98b7674528564b4f0.ctex {`

### intent buff 16.png de9b1fc0a3f894e98b7674528564b4f0

- ID: `path--res----godot-imported-intent-buff-16-png-de9b1fc0a3f894e98b7674528564b4f0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_16.png-de9b1fc0a3f894e98b7674528564b4f0.ctex"`

### intent buff 17

- ID: `filename----buff-intent-buff-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_17.png",`

### intent buff 17

- ID: `res---images-packed-intents-buff-intent-buff-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_17.png`

### intent buff 17.png

- ID: `images-packed-intents-buff-intent-buff-17-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_17.png.importp`

### intent buff 17.png 63bfd5e91a90d59b7e9122f84dedf277

- ID: `godot-imported-intent-buff-17-png-63bfd5e91a90d59b7e9122f84dedf277-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_17.png-63bfd5e91a90d59b7e9122f84dedf277.ctex`

### intent buff 17.png 63bfd5e91a90d59b7e9122f84dedf277

- ID: `path--res----godot-imported-intent-buff-17-png-63bfd5e91a90d59b7e9122f84dedf277-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_17.png-63bfd5e91a90d59b7e9122f84dedf277.ctex"`

### intent buff 18

- ID: `filename----buff-intent-buff-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_18.png",`

### intent buff 18

- ID: `res---images-packed-intents-buff-intent-buff-18-pngw`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_18.pngw+@`

### intent buff 18.png

- ID: `images-packed-intents-buff-intent-buff-18-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_18.png.import`

### intent buff 18.png 14dcebec6cfb0b37afd05efb72f93dae

- ID: `godot-imported-intent-buff-18-png-14dcebec6cfb0b37afd05efb72f93dae-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_18.png-14dcebec6cfb0b37afd05efb72f93dae.ctex@`

### intent buff 18.png 14dcebec6cfb0b37afd05efb72f93dae

- ID: `path--res----godot-imported-intent-buff-18-png-14dcebec6cfb0b37afd05efb72f93dae-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_18.png-14dcebec6cfb0b37afd05efb72f93dae.ctex"`

### intent buff 19

- ID: `filename----buff-intent-buff-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_19.png",`

### intent buff 19

- ID: `res---images-packed-intents-buff-intent-buff-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_19.png`

### intent buff 19.png

- ID: `images-packed-intents-buff-intent-buff-19-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_19.png.import`

### intent buff 19.png 3d7f1c9e03ba465d2f63da2d7610e20c

- ID: `godot-imported-intent-buff-19-png-3d7f1c9e03ba465d2f63da2d7610e20c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_19.png-3d7f1c9e03ba465d2f63da2d7610e20c.ctex`

### intent buff 19.png 3d7f1c9e03ba465d2f63da2d7610e20c

- ID: `path--res----godot-imported-intent-buff-19-png-3d7f1c9e03ba465d2f63da2d7610e20c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_19.png-3d7f1c9e03ba465d2f63da2d7610e20c.ctex"`

### intent buff 20

- ID: `filename----buff-intent-buff-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_20.png",`

### intent buff 20

- ID: `res---images-packed-intents-buff-intent-buff-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_20.png`

### intent buff 20.png

- ID: `images-packed-intents-buff-intent-buff-20-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_20.png.import`

### intent buff 20.png 1d87b13ca83a41aacab6f1e72a7baf34

- ID: `godot-imported-intent-buff-20-png-1d87b13ca83a41aacab6f1e72a7baf34-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_20.png-1d87b13ca83a41aacab6f1e72a7baf34.ctex``

### intent buff 20.png 1d87b13ca83a41aacab6f1e72a7baf34

- ID: `path--res----godot-imported-intent-buff-20-png-1d87b13ca83a41aacab6f1e72a7baf34-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_20.png-1d87b13ca83a41aacab6f1e72a7baf34.ctex"`

### intent buff 21

- ID: `filename----buff-intent-buff-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_21.png",`

### intent buff 21

- ID: `res---images-packed-intents-buff-intent-buff-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_21.png`

### intent buff 21.png

- ID: `images-packed-intents-buff-intent-buff-21-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_21.png.import`

### intent buff 21.png df5a5f9be4360ff16f5867f94afe733a

- ID: `godot-imported-intent-buff-21-png-df5a5f9be4360ff16f5867f94afe733a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_21.png-df5a5f9be4360ff16f5867f94afe733a.ctex`

### intent buff 21.png df5a5f9be4360ff16f5867f94afe733a

- ID: `path--res----godot-imported-intent-buff-21-png-df5a5f9be4360ff16f5867f94afe733a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_21.png-df5a5f9be4360ff16f5867f94afe733a.ctex"`

### intent buff 22

- ID: `filename----buff-intent-buff-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_22.png",`

### intent buff 22

- ID: `res---images-packed-intents-buff-intent-buff-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_22.png`

### intent buff 22.png

- ID: `images-packed-intents-buff-intent-buff-22-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_22.png.import@`

### intent buff 22.png 8e2042aba009ef2b50f7aa3f621c2e0f

- ID: `godot-imported-intent-buff-22-png-8e2042aba009ef2b50f7aa3f621c2e0f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_22.png-8e2042aba009ef2b50f7aa3f621c2e0f.ctex`

### intent buff 22.png 8e2042aba009ef2b50f7aa3f621c2e0f

- ID: `path--res----godot-imported-intent-buff-22-png-8e2042aba009ef2b50f7aa3f621c2e0f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_22.png-8e2042aba009ef2b50f7aa3f621c2e0f.ctex"`

### intent buff 23

- ID: `filename----buff-intent-buff-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_23.png",`

### intent buff 23

- ID: `res---images-packed-intents-buff-intent-buff-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_23.png`

### intent buff 23.png

- ID: `images-packed-intents-buff-intent-buff-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_23.png.import`

### intent buff 23.png 65f7ec8a0f00e4bdd1e5cc08da4a4b4f

- ID: `godot-imported-intent-buff-23-png-65f7ec8a0f00e4bdd1e5cc08da4a4b4f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_23.png-65f7ec8a0f00e4bdd1e5cc08da4a4b4f.ctex`

### intent buff 23.png 65f7ec8a0f00e4bdd1e5cc08da4a4b4f

- ID: `path--res----godot-imported-intent-buff-23-png-65f7ec8a0f00e4bdd1e5cc08da4a4b4f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_23.png-65f7ec8a0f00e4bdd1e5cc08da4a4b4f.ctex"`

### intent buff 24

- ID: `filename----buff-intent-buff-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_24.png",`

### intent buff 24

- ID: `res---images-packed-intents-buff-intent-buff-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_24.png`

### intent buff 24.png

- ID: `images-packed-intents-buff-intent-buff-24-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_24.png.import``

### intent buff 24.png e3de7c5da980ec425f04e6feb118d429

- ID: `godot-imported-intent-buff-24-png-e3de7c5da980ec425f04e6feb118d429-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_24.png-e3de7c5da980ec425f04e6feb118d429.ctex`

### intent buff 24.png e3de7c5da980ec425f04e6feb118d429

- ID: `path--res----godot-imported-intent-buff-24-png-e3de7c5da980ec425f04e6feb118d429-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_24.png-e3de7c5da980ec425f04e6feb118d429.ctex"`

### intent buff 25

- ID: `filename----buff-intent-buff-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_25.png",`

### intent buff 25.png

- ID: `images-packed-intents-buff-intent-buff-25-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_25.png.import`

### intent buff 25.png b2c22a8123cab5f65fdd99c57aebaaec

- ID: `godot-imported-intent-buff-25-png-b2c22a8123cab5f65fdd99c57aebaaec-ctex0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_25.png-b2c22a8123cab5f65fdd99c57aebaaec.ctex0`

### intent buff 25.png b2c22a8123cab5f65fdd99c57aebaaec

- ID: `path--res----godot-imported-intent-buff-25-png-b2c22a8123cab5f65fdd99c57aebaaec-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_25.png-b2c22a8123cab5f65fdd99c57aebaaec.ctex"`

### intent buff 26

- ID: `filename----buff-intent-buff-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_26.png",`

### intent buff 26

- ID: `res---images-packed-intents-buff-intent-buff-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_26.png`

### intent buff 26.png

- ID: `images-packed-intents-buff-intent-buff-26-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_26.png.import`

### intent buff 26.png 4ff7083690274647e6e6b7c594c1fae7

- ID: `godot-imported-intent-buff-26-png-4ff7083690274647e6e6b7c594c1fae7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_26.png-4ff7083690274647e6e6b7c594c1fae7.ctex`

### intent buff 26.png 4ff7083690274647e6e6b7c594c1fae7

- ID: `path--res----godot-imported-intent-buff-26-png-4ff7083690274647e6e6b7c594c1fae7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_26.png-4ff7083690274647e6e6b7c594c1fae7.ctex"`

### intent buff 27

- ID: `filename----buff-intent-buff-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_27.png",`

### intent buff 27

- ID: `res---images-packed-intents-buff-intent-buff-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_27.png`

### intent buff 27.png

- ID: `images-packed-intents-buff-intent-buff-27-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_27.png.import`

### intent buff 27.png 100aaa345dc9ddaf08cb4ddadd76d746

- ID: `godot-imported-intent-buff-27-png-100aaa345dc9ddaf08cb4ddadd76d746-ctexp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_27.png-100aaa345dc9ddaf08cb4ddadd76d746.ctexP`

### intent buff 27.png 100aaa345dc9ddaf08cb4ddadd76d746

- ID: `path--res----godot-imported-intent-buff-27-png-100aaa345dc9ddaf08cb4ddadd76d746-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_27.png-100aaa345dc9ddaf08cb4ddadd76d746.ctex"`

### intent buff 28

- ID: `filename----buff-intent-buff-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_28.png",`

### intent buff 28

- ID: `res---images-packed-intents-buff-intent-buff-28-png-zi`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_28.png>zi`

### intent buff 28.png

- ID: `images-packed-intents-buff-intent-buff-28-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_28.png.import`

### intent buff 28.png ba6c12581ca35604fe6ff394dee2c265

- ID: `godot-imported-intent-buff-28-png-ba6c12581ca35604fe6ff394dee2c265-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_28.png-ba6c12581ca35604fe6ff394dee2c265.ctex`

### intent buff 28.png ba6c12581ca35604fe6ff394dee2c265

- ID: `path--res----godot-imported-intent-buff-28-png-ba6c12581ca35604fe6ff394dee2c265-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_28.png-ba6c12581ca35604fe6ff394dee2c265.ctex"`

### intent buff 29

- ID: `filename----buff-intent-buff-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "buff/intent_buff_29.png",`

### intent buff 29

- ID: `res---images-packed-intents-buff-intent-buff-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_29.png`

### intent buff 29.png

- ID: `images-packed-intents-buff-intent-buff-29-png-import0f`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/buff/intent_buff_29.png.import0F`

### intent buff 29.png 5dce341910a4806e8b6dab8c13eb4080

- ID: `godot-imported-intent-buff-29-png-5dce341910a4806e8b6dab8c13eb4080-ctexp8`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff_29.png-5dce341910a4806e8b6dab8c13eb4080.ctexp8`

### intent buff 29.png 5dce341910a4806e8b6dab8c13eb4080

- ID: `path--res----godot-imported-intent-buff-29-png-5dce341910a4806e8b6dab8c13eb4080-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff_29.png-5dce341910a4806e8b6dab8c13eb4080.ctex"`

### intent buff.png

- ID: `images-packed-intents-intent-buff-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_buff.png.import`

### intent buff.png 2a19c1404036338dd3d6b195a1c2910d

- ID: `godot-imported-intent-buff-png-2a19c1404036338dd3d6b195a1c2910d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_buff.png-2a19c1404036338dd3d6b195a1c2910d.ctex`

### intent buff.png 2a19c1404036338dd3d6b195a1c2910d

- ID: `path--res----godot-imported-intent-buff-png-2a19c1404036338dd3d6b195a1c2910d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_buff.png-2a19c1404036338dd3d6b195a1c2910d.ctex"`

### Intent Container

- ID: `intentcontainer`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent death blow

- ID: `res---images-packed-intents-intent-death-blow-png--r`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_death_blow.png]:R`

### intent death blow.png

- ID: `images-packed-intents-intent-death-blow-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_death_blow.png.import`

### intent death blow.png 76c47c6ccc05b131025ebcabecab9dde

- ID: `godot-imported-intent-death-blow-png-76c47c6ccc05b131025ebcabecab9dde-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_death_blow.png-76c47c6ccc05b131025ebcabecab9dde.ctex`

### intent death blow.png 76c47c6ccc05b131025ebcabecab9dde

- ID: `path--res----godot-imported-intent-death-blow-png-76c47c6ccc05b131025ebcabecab9dde-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_death_blow.png-76c47c6ccc05b131025ebcabecab9dde.ctex"`

### intent debuff

- ID: `res---images-packed-intents-intent-debuff-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_debuff.png`

### intent debuff.png

- ID: `images-packed-intents-intent-debuff-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_debuff.png.import`

### intent debuff.png 86dce19d3838077ac807e88a5225db64

- ID: `godot-imported-intent-debuff-png-86dce19d3838077ac807e88a5225db64-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_debuff.png-86dce19d3838077ac807e88a5225db64.ctex`

### intent debuff.png 86dce19d3838077ac807e88a5225db64

- ID: `path--res----godot-imported-intent-debuff-png-86dce19d3838077ac807e88a5225db64-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_debuff.png-86dce19d3838077ac807e88a5225db64.ctex"`

### intent defend

- ID: `res---images-packed-intents-intent-defend-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_defend.png`

### intent defend 00

- ID: `filename----defend-intent-defend-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_00.png",`

### intent defend 00

- ID: `res---images-packed-intents-defend-intent-defend-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_00.png`

### intent defend 00.png

- ID: `images-packed-intents-defend-intent-defend-00-png-importpn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_00.png.importpn`

### intent defend 00.png 74b90ae20ae2c73db857c24d7e1984b9

- ID: `godot-imported-intent-defend-00-png-74b90ae20ae2c73db857c24d7e1984b9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_00.png-74b90ae20ae2c73db857c24d7e1984b9.ctex`

### intent defend 00.png 74b90ae20ae2c73db857c24d7e1984b9

- ID: `path--res----godot-imported-intent-defend-00-png-74b90ae20ae2c73db857c24d7e1984b9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_00.png-74b90ae20ae2c73db857c24d7e1984b9.ctex"`

### intent defend 01

- ID: `filename----defend-intent-defend-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_01.png",`

### intent defend 01

- ID: `res---images-packed-intents-defend-intent-defend-01-pngeu`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_01.pngeU`

### intent defend 01.png

- ID: `images-packed-intents-defend-intent-defend-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_01.png.import`

### intent defend 01.png 5d37bca732edb280447da020124c1356

- ID: `godot-imported-intent-defend-01-png-5d37bca732edb280447da020124c1356-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_01.png-5d37bca732edb280447da020124c1356.ctex`

### intent defend 01.png 5d37bca732edb280447da020124c1356

- ID: `path--res----godot-imported-intent-defend-01-png-5d37bca732edb280447da020124c1356-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_01.png-5d37bca732edb280447da020124c1356.ctex"`

### intent defend 02

- ID: `filename----defend-intent-defend-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_02.png",`

### intent defend 02

- ID: `res---images-packed-intents-defend-intent-defend-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_02.png`

### intent defend 02.png

- ID: `images-packed-intents-defend-intent-defend-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_02.png.import`

### intent defend 02.png 9785c2eed99a6dae46ac050c29a6ef69

- ID: `godot-imported-intent-defend-02-png-9785c2eed99a6dae46ac050c29a6ef69-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_02.png-9785c2eed99a6dae46ac050c29a6ef69.ctex`

### intent defend 02.png 9785c2eed99a6dae46ac050c29a6ef69

- ID: `path--res----godot-imported-intent-defend-02-png-9785c2eed99a6dae46ac050c29a6ef69-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_02.png-9785c2eed99a6dae46ac050c29a6ef69.ctex"`

### intent defend 03

- ID: `filename----defend-intent-defend-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_03.png",`

### intent defend 03.png

- ID: `images-packed-intents-defend-intent-defend-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_03.png.import`

### intent defend 03.png 3f4ddd475c983c930be9180f5adcb4d0

- ID: `godot-imported-intent-defend-03-png-3f4ddd475c983c930be9180f5adcb4d0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_03.png-3f4ddd475c983c930be9180f5adcb4d0.ctex`

### intent defend 03.png 3f4ddd475c983c930be9180f5adcb4d0

- ID: `path--res----godot-imported-intent-defend-03-png-3f4ddd475c983c930be9180f5adcb4d0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_03.png-3f4ddd475c983c930be9180f5adcb4d0.ctex"`

### intent defend 04

- ID: `filename----defend-intent-defend-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_04.png",`

### intent defend 04

- ID: `res---images-packed-intents-defend-intent-defend-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_04.png`

### intent defend 04.png

- ID: `images-packed-intents-defend-intent-defend-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_04.png.import`

### intent defend 04.png 8315fcefb108877349592784ed30d31d

- ID: `godot-imported-intent-defend-04-png-8315fcefb108877349592784ed30d31d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_04.png-8315fcefb108877349592784ed30d31d.ctex`

### intent defend 04.png 8315fcefb108877349592784ed30d31d

- ID: `path--res----godot-imported-intent-defend-04-png-8315fcefb108877349592784ed30d31d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_04.png-8315fcefb108877349592784ed30d31d.ctex"`

### intent defend 05

- ID: `filename----defend-intent-defend-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_05.png",`

### intent defend 05

- ID: `res---images-packed-intents-defend-intent-defend-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_05.png`

### intent defend 05.png

- ID: `images-packed-intents-defend-intent-defend-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_05.png.import`

### intent defend 05.png 904c09ca6724b76b390ebca39063d139

- ID: `godot-imported-intent-defend-05-png-904c09ca6724b76b390ebca39063d139-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_05.png-904c09ca6724b76b390ebca39063d139.ctex`

### intent defend 05.png 904c09ca6724b76b390ebca39063d139

- ID: `path--res----godot-imported-intent-defend-05-png-904c09ca6724b76b390ebca39063d139-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_05.png-904c09ca6724b76b390ebca39063d139.ctex"`

### intent defend 06

- ID: `filename----defend-intent-defend-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_06.png",`

### intent defend 06

- ID: `res---images-packed-intents-defend-intent-defend-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_06.png`

### intent defend 06.png

- ID: `images-packed-intents-defend-intent-defend-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_06.png.import`

### intent defend 06.png 29dc37cffcbad25e3a6a8a897a439469

- ID: `godot-imported-intent-defend-06-png-29dc37cffcbad25e3a6a8a897a439469-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_06.png-29dc37cffcbad25e3a6a8a897a439469.ctex`

### intent defend 06.png 29dc37cffcbad25e3a6a8a897a439469

- ID: `path--res----godot-imported-intent-defend-06-png-29dc37cffcbad25e3a6a8a897a439469-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_06.png-29dc37cffcbad25e3a6a8a897a439469.ctex"`

### intent defend 07

- ID: `filename----defend-intent-defend-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_07.png",`

### intent defend 07

- ID: `res---images-packed-intents-defend-intent-defend-07-pngvq`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_07.pngVq`

### intent defend 07.png

- ID: `images-packed-intents-defend-intent-defend-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_07.png.import`

### intent defend 07.png 7e4eb07d3e848ceb4c1572cad88c70aa

- ID: `godot-imported-intent-defend-07-png-7e4eb07d3e848ceb4c1572cad88c70aa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_07.png-7e4eb07d3e848ceb4c1572cad88c70aa.ctex`

### intent defend 07.png 7e4eb07d3e848ceb4c1572cad88c70aa

- ID: `path--res----godot-imported-intent-defend-07-png-7e4eb07d3e848ceb4c1572cad88c70aa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_07.png-7e4eb07d3e848ceb4c1572cad88c70aa.ctex"`

### intent defend 08

- ID: `filename----defend-intent-defend-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_08.png",`

### intent defend 08

- ID: `res---images-packed-intents-defend-intent-defend-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_08.png`

### intent defend 08.png

- ID: `images-packed-intents-defend-intent-defend-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_08.png.import`

### intent defend 08.png 408fd8a1f57164a2a62bd323c9e376ff

- ID: `godot-imported-intent-defend-08-png-408fd8a1f57164a2a62bd323c9e376ff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_08.png-408fd8a1f57164a2a62bd323c9e376ff.ctex`

### intent defend 08.png 408fd8a1f57164a2a62bd323c9e376ff

- ID: `path--res----godot-imported-intent-defend-08-png-408fd8a1f57164a2a62bd323c9e376ff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_08.png-408fd8a1f57164a2a62bd323c9e376ff.ctex"`

### intent defend 09

- ID: `filename----defend-intent-defend-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_09.png",`

### intent defend 09

- ID: `res---images-packed-intents-defend-intent-defend-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_09.png`

### intent defend 09.png

- ID: `images-packed-intents-defend-intent-defend-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_09.png.import`

### intent defend 09.png 3adbc6e999e5f8bec32c81287a9e623d

- ID: `godot-imported-intent-defend-09-png-3adbc6e999e5f8bec32c81287a9e623d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_09.png-3adbc6e999e5f8bec32c81287a9e623d.ctex`

### intent defend 09.png 3adbc6e999e5f8bec32c81287a9e623d

- ID: `path--res----godot-imported-intent-defend-09-png-3adbc6e999e5f8bec32c81287a9e623d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_09.png-3adbc6e999e5f8bec32c81287a9e623d.ctex"`

### intent defend 10

- ID: `filename----defend-intent-defend-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_10.png",`

### intent defend 10

- ID: `res---images-packed-intents-defend-intent-defend-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_10.png`

### intent defend 10.png

- ID: `images-packed-intents-defend-intent-defend-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_10.png.import`

### intent defend 10.png beec9ce958257d889ffeaed564c4aba8

- ID: `godot-imported-intent-defend-10-png-beec9ce958257d889ffeaed564c4aba8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_10.png-beec9ce958257d889ffeaed564c4aba8.ctex`

### intent defend 10.png beec9ce958257d889ffeaed564c4aba8

- ID: `path--res----godot-imported-intent-defend-10-png-beec9ce958257d889ffeaed564c4aba8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_10.png-beec9ce958257d889ffeaed564c4aba8.ctex"`

### intent defend 11

- ID: `filename----defend-intent-defend-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_11.png",`

### intent defend 11

- ID: `res---images-packed-intents-defend-intent-defend-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_11.png`

### intent defend 11.png

- ID: `images-packed-intents-defend-intent-defend-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_11.png.import`

### intent defend 11.png c4bd3c53e03dad47e4b3fa20e708deb2

- ID: `godot-imported-intent-defend-11-png-c4bd3c53e03dad47e4b3fa20e708deb2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_11.png-c4bd3c53e03dad47e4b3fa20e708deb2.ctex`

### intent defend 11.png c4bd3c53e03dad47e4b3fa20e708deb2

- ID: `path--res----godot-imported-intent-defend-11-png-c4bd3c53e03dad47e4b3fa20e708deb2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_11.png-c4bd3c53e03dad47e4b3fa20e708deb2.ctex"`

### intent defend 12

- ID: `filename----defend-intent-defend-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_12.png",`

### intent defend 12

- ID: `res---images-packed-intents-defend-intent-defend-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_12.png`

### intent defend 12.png

- ID: `images-packed-intents-defend-intent-defend-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_12.png.import`

### intent defend 12.png 8d7c246c4d3f48188784bbffa6d58d2c

- ID: `godot-imported-intent-defend-12-png-8d7c246c4d3f48188784bbffa6d58d2c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_12.png-8d7c246c4d3f48188784bbffa6d58d2c.ctex`

### intent defend 12.png 8d7c246c4d3f48188784bbffa6d58d2c

- ID: `path--res----godot-imported-intent-defend-12-png-8d7c246c4d3f48188784bbffa6d58d2c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_12.png-8d7c246c4d3f48188784bbffa6d58d2c.ctex"`

### intent defend 13

- ID: `filename----defend-intent-defend-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_13.png",`

### intent defend 13

- ID: `res---images-packed-intents-defend-intent-defend-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_13.png|`

### intent defend 13.png

- ID: `images-packed-intents-defend-intent-defend-13-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_13.png.importp`

### intent defend 13.png 4e1228eab0e262455e452439295493f5

- ID: `godot-imported-intent-defend-13-png-4e1228eab0e262455e452439295493f5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_13.png-4e1228eab0e262455e452439295493f5.ctex`

### intent defend 13.png 4e1228eab0e262455e452439295493f5

- ID: `path--res----godot-imported-intent-defend-13-png-4e1228eab0e262455e452439295493f5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_13.png-4e1228eab0e262455e452439295493f5.ctex"`

### intent defend 14

- ID: `filename----defend-intent-defend-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_14.png",`

### intent defend 14.png

- ID: `images-packed-intents-defend-intent-defend-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_14.png.import``

### intent defend 14.png 587357ccf8b72db3db504772e02a75da

- ID: `godot-imported-intent-defend-14-png-587357ccf8b72db3db504772e02a75da-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_14.png-587357ccf8b72db3db504772e02a75da.ctex`

### intent defend 14.png 587357ccf8b72db3db504772e02a75da

- ID: `path--res----godot-imported-intent-defend-14-png-587357ccf8b72db3db504772e02a75da-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_14.png-587357ccf8b72db3db504772e02a75da.ctex"`

### intent defend 15

- ID: `filename----defend-intent-defend-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_15.png",`

### intent defend 15

- ID: `res---images-packed-intents-defend-intent-defend-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_15.png`

### intent defend 15.png

- ID: `images-packed-intents-defend-intent-defend-15-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_15.png.importP`

### intent defend 15.png 146673fa657d0d59076b1d5b691f6bf9

- ID: `godot-imported-intent-defend-15-png-146673fa657d0d59076b1d5b691f6bf9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_15.png-146673fa657d0d59076b1d5b691f6bf9.ctex`

### intent defend 15.png 146673fa657d0d59076b1d5b691f6bf9

- ID: `path--res----godot-imported-intent-defend-15-png-146673fa657d0d59076b1d5b691f6bf9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_15.png-146673fa657d0d59076b1d5b691f6bf9.ctex"`

### intent defend 16

- ID: `filename----defend-intent-defend-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_16.png",`

### intent defend 16

- ID: `res---images-packed-intents-defend-intent-defend-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_16.png`

### intent defend 16.png

- ID: `images-packed-intents-defend-intent-defend-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_16.png.import@<`

### intent defend 16.png 6db12514230c37834fb7b3412380e785

- ID: `godot-imported-intent-defend-16-png-6db12514230c37834fb7b3412380e785-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_16.png-6db12514230c37834fb7b3412380e785.ctex`

### intent defend 16.png 6db12514230c37834fb7b3412380e785

- ID: `path--res----godot-imported-intent-defend-16-png-6db12514230c37834fb7b3412380e785-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_16.png-6db12514230c37834fb7b3412380e785.ctex"`

### intent defend 17

- ID: `filename----defend-intent-defend-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_17.png",`

### intent defend 17

- ID: `res---images-packed-intents-defend-intent-defend-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_17.png`

### intent defend 17.png

- ID: `images-packed-intents-defend-intent-defend-17-png-import0x`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_17.png.import0X`

### intent defend 17.png 1791da4260f4a2ce2b3a640040e183e8

- ID: `godot-imported-intent-defend-17-png-1791da4260f4a2ce2b3a640040e183e8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_17.png-1791da4260f4a2ce2b3a640040e183e8.ctex`

### intent defend 17.png 1791da4260f4a2ce2b3a640040e183e8

- ID: `path--res----godot-imported-intent-defend-17-png-1791da4260f4a2ce2b3a640040e183e8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_17.png-1791da4260f4a2ce2b3a640040e183e8.ctex"`

### intent defend 18

- ID: `filename----defend-intent-defend-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_18.png",`

### intent defend 18

- ID: `res---images-packed-intents-defend-intent-defend-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_18.png`

### intent defend 18.png

- ID: `images-packed-intents-defend-intent-defend-18-png-import-t`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_18.png.import t`

### intent defend 18.png 0703a7b2177c4bafa215a23a39b89a68

- ID: `godot-imported-intent-defend-18-png-0703a7b2177c4bafa215a23a39b89a68-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_18.png-0703a7b2177c4bafa215a23a39b89a68.ctex`

### intent defend 18.png 0703a7b2177c4bafa215a23a39b89a68

- ID: `path--res----godot-imported-intent-defend-18-png-0703a7b2177c4bafa215a23a39b89a68-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_18.png-0703a7b2177c4bafa215a23a39b89a68.ctex"`

### intent defend 19

- ID: `filename----defend-intent-defend-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_19.png",`

### intent defend 19

- ID: `res---images-packed-intents-defend-intent-defend-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_19.png`

### intent defend 19.png

- ID: `images-packed-intents-defend-intent-defend-19-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_19.png.import`

### intent defend 19.png 2324dde1ba18cfab8eb85d6b70f62c28

- ID: `godot-imported-intent-defend-19-png-2324dde1ba18cfab8eb85d6b70f62c28-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_19.png-2324dde1ba18cfab8eb85d6b70f62c28.ctex`

### intent defend 19.png 2324dde1ba18cfab8eb85d6b70f62c28

- ID: `path--res----godot-imported-intent-defend-19-png-2324dde1ba18cfab8eb85d6b70f62c28-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_19.png-2324dde1ba18cfab8eb85d6b70f62c28.ctex"`

### intent defend 20

- ID: `filename----defend-intent-defend-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_20.png",`

### intent defend 20

- ID: `res---images-packed-intents-defend-intent-defend-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_20.png`

### intent defend 20.png

- ID: `images-packed-intents-defend-intent-defend-20-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_20.png.import`

### intent defend 20.png f8e22404cc700983f650e8bdb9403907

- ID: `godot-imported-intent-defend-20-png-f8e22404cc700983f650e8bdb9403907-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_20.png-f8e22404cc700983f650e8bdb9403907.ctex`

### intent defend 20.png f8e22404cc700983f650e8bdb9403907

- ID: `path--res----godot-imported-intent-defend-20-png-f8e22404cc700983f650e8bdb9403907-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_20.png-f8e22404cc700983f650e8bdb9403907.ctex"`

### intent defend 21

- ID: `filename----defend-intent-defend-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_21.png",`

### intent defend 21

- ID: `res---images-packed-intents-defend-intent-defend-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_21.png`

### intent defend 21.png

- ID: `images-packed-intents-defend-intent-defend-21-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_21.png.import`

### intent defend 21.png 59a42d05f37cb4c8515cbed58d7828a9

- ID: `godot-imported-intent-defend-21-png-59a42d05f37cb4c8515cbed58d7828a9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_21.png-59a42d05f37cb4c8515cbed58d7828a9.ctex`

### intent defend 21.png 59a42d05f37cb4c8515cbed58d7828a9

- ID: `path--res----godot-imported-intent-defend-21-png-59a42d05f37cb4c8515cbed58d7828a9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_21.png-59a42d05f37cb4c8515cbed58d7828a9.ctex"`

### intent defend 22

- ID: `filename----defend-intent-defend-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_22.png",`

### intent defend 22

- ID: `res---images-packed-intents-defend-intent-defend-22-pngh`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_22.pngH}"`

### intent defend 22.png

- ID: `images-packed-intents-defend-intent-defend-22-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_22.png.import`

### intent defend 22.png 3fd9427fb3d8f0efb406ce00136794bd

- ID: `godot-imported-intent-defend-22-png-3fd9427fb3d8f0efb406ce00136794bd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_22.png-3fd9427fb3d8f0efb406ce00136794bd.ctex`

### intent defend 22.png 3fd9427fb3d8f0efb406ce00136794bd

- ID: `path--res----godot-imported-intent-defend-22-png-3fd9427fb3d8f0efb406ce00136794bd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_22.png-3fd9427fb3d8f0efb406ce00136794bd.ctex"`

### intent defend 23

- ID: `filename----defend-intent-defend-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_23.png",`

### intent defend 23

- ID: `res---images-packed-intents-defend-intent-defend-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_23.png`

### intent defend 23.png

- ID: `images-packed-intents-defend-intent-defend-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_23.png.import`

### intent defend 23.png 4e9bab52d88e6865b9a97e5661ca7a54

- ID: `godot-imported-intent-defend-23-png-4e9bab52d88e6865b9a97e5661ca7a54-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_23.png-4e9bab52d88e6865b9a97e5661ca7a54.ctex`

### intent defend 23.png 4e9bab52d88e6865b9a97e5661ca7a54

- ID: `path--res----godot-imported-intent-defend-23-png-4e9bab52d88e6865b9a97e5661ca7a54-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_23.png-4e9bab52d88e6865b9a97e5661ca7a54.ctex"`

### intent defend 24

- ID: `filename----defend-intent-defend-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_24.png",`

### intent defend 24

- ID: `res---images-packed-intents-defend-intent-defend-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_24.png`

### intent defend 24.png

- ID: `images-packed-intents-defend-intent-defend-24-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_24.png.import`

### intent defend 24.png c2b00f74ac16cdaddc460054779fb718

- ID: `godot-imported-intent-defend-24-png-c2b00f74ac16cdaddc460054779fb718-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_24.png-c2b00f74ac16cdaddc460054779fb718.ctex`

### intent defend 24.png c2b00f74ac16cdaddc460054779fb718

- ID: `path--res----godot-imported-intent-defend-24-png-c2b00f74ac16cdaddc460054779fb718-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_24.png-c2b00f74ac16cdaddc460054779fb718.ctex"`

### intent defend 25

- ID: `filename----defend-intent-defend-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_25.png",`

### intent defend 25

- ID: `res---images-packed-intents-defend-intent-defend-25-pngme-e`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_25.pngME$e=`

### intent defend 25.png

- ID: `images-packed-intents-defend-intent-defend-25-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_25.png.import`

### intent defend 25.png cbdc3fd77246fa67f47c902ec77d9741

- ID: `godot-imported-intent-defend-25-png-cbdc3fd77246fa67f47c902ec77d9741-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_25.png-cbdc3fd77246fa67f47c902ec77d9741.ctex`

### intent defend 25.png cbdc3fd77246fa67f47c902ec77d9741

- ID: `path--res----godot-imported-intent-defend-25-png-cbdc3fd77246fa67f47c902ec77d9741-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_25.png-cbdc3fd77246fa67f47c902ec77d9741.ctex"`

### intent defend 26

- ID: `filename----defend-intent-defend-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_26.png",`

### intent defend 26

- ID: `res---images-packed-intents-defend-intent-defend-26-pngg`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_26.pngg`

### intent defend 26.png

- ID: `images-packed-intents-defend-intent-defend-26-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_26.png.import`

### intent defend 26.png 16f224a7932bf02d1e89de91baba1fac

- ID: `godot-imported-intent-defend-26-png-16f224a7932bf02d1e89de91baba1fac-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_26.png-16f224a7932bf02d1e89de91baba1fac.ctex`

### intent defend 26.png 16f224a7932bf02d1e89de91baba1fac

- ID: `path--res----godot-imported-intent-defend-26-png-16f224a7932bf02d1e89de91baba1fac-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_26.png-16f224a7932bf02d1e89de91baba1fac.ctex"`

### intent defend 27

- ID: `filename----defend-intent-defend-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_27.png",`

### intent defend 27

- ID: `res---images-packed-intents-defend-intent-defend-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_27.png`

### intent defend 27.png

- ID: `images-packed-intents-defend-intent-defend-27-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_27.png.import`

### intent defend 27.png 6c093ffd6e16c586cc3c94490c35b83a

- ID: `godot-imported-intent-defend-27-png-6c093ffd6e16c586cc3c94490c35b83a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_27.png-6c093ffd6e16c586cc3c94490c35b83a.ctex`

### intent defend 27.png 6c093ffd6e16c586cc3c94490c35b83a

- ID: `path--res----godot-imported-intent-defend-27-png-6c093ffd6e16c586cc3c94490c35b83a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_27.png-6c093ffd6e16c586cc3c94490c35b83a.ctex"`

### intent defend 28

- ID: `filename----defend-intent-defend-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_28.png",`

### intent defend 28

- ID: `res---images-packed-intents-defend-intent-defend-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_28.png@`

### intent defend 28.png

- ID: `images-packed-intents-defend-intent-defend-28-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_28.png.import`

### intent defend 28.png 94e0f066f2ac0f935e16af8bd091909b

- ID: `godot-imported-intent-defend-28-png-94e0f066f2ac0f935e16af8bd091909b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_28.png-94e0f066f2ac0f935e16af8bd091909b.ctex`

### intent defend 28.png 94e0f066f2ac0f935e16af8bd091909b

- ID: `path--res----godot-imported-intent-defend-28-png-94e0f066f2ac0f935e16af8bd091909b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_28.png-94e0f066f2ac0f935e16af8bd091909b.ctex"`

### intent defend 29

- ID: `filename----defend-intent-defend-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_29.png",`

### intent defend 29

- ID: `res---images-packed-intents-defend-intent-defend-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_29.png`

### intent defend 29.png

- ID: `images-packed-intents-defend-intent-defend-29-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_29.png.importp`

### intent defend 29.png ea2f24f2904f7a51596ce6e1420db809

- ID: `godot-imported-intent-defend-29-png-ea2f24f2904f7a51596ce6e1420db809-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_29.png-ea2f24f2904f7a51596ce6e1420db809.ctex`

### intent defend 29.png ea2f24f2904f7a51596ce6e1420db809

- ID: `path--res----godot-imported-intent-defend-29-png-ea2f24f2904f7a51596ce6e1420db809-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_29.png-ea2f24f2904f7a51596ce6e1420db809.ctex"`

### intent defend 30

- ID: `filename----defend-intent-defend-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_30.png",`

### intent defend 30

- ID: `res---images-packed-intents-defend-intent-defend-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_30.png`

### intent defend 30.png

- ID: `images-packed-intents-defend-intent-defend-30-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_30.png.import``

### intent defend 30.png 25cc82a1dfe1badac8574315672b34b6

- ID: `godot-imported-intent-defend-30-png-25cc82a1dfe1badac8574315672b34b6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_30.png-25cc82a1dfe1badac8574315672b34b6.ctex`

### intent defend 30.png 25cc82a1dfe1badac8574315672b34b6

- ID: `path--res----godot-imported-intent-defend-30-png-25cc82a1dfe1badac8574315672b34b6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_30.png-25cc82a1dfe1badac8574315672b34b6.ctex"`

### intent defend 31

- ID: `filename----defend-intent-defend-31-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_31.png",`

### intent defend 31

- ID: `res---images-packed-intents-defend-intent-defend-31-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_31.png`

### intent defend 31.png

- ID: `images-packed-intents-defend-intent-defend-31-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_31.png.importP`

### intent defend 31.png bf364ecb0ef782db2ead9efbf54d5aa8

- ID: `godot-imported-intent-defend-31-png-bf364ecb0ef782db2ead9efbf54d5aa8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_31.png-bf364ecb0ef782db2ead9efbf54d5aa8.ctex`

### intent defend 31.png bf364ecb0ef782db2ead9efbf54d5aa8

- ID: `path--res----godot-imported-intent-defend-31-png-bf364ecb0ef782db2ead9efbf54d5aa8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_31.png-bf364ecb0ef782db2ead9efbf54d5aa8.ctex"`

### intent defend 32

- ID: `filename----defend-intent-defend-32-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_32.png",`

### intent defend 32

- ID: `res---images-packed-intents-defend-intent-defend-32-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_32.png`

### intent defend 32.png

- ID: `images-packed-intents-defend-intent-defend-32-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_32.png.import@`

### intent defend 32.png 0640da8bcdae5dc8f12f7793d2842859

- ID: `godot-imported-intent-defend-32-png-0640da8bcdae5dc8f12f7793d2842859-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_32.png-0640da8bcdae5dc8f12f7793d2842859.ctex`

### intent defend 32.png 0640da8bcdae5dc8f12f7793d2842859

- ID: `path--res----godot-imported-intent-defend-32-png-0640da8bcdae5dc8f12f7793d2842859-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_32.png-0640da8bcdae5dc8f12f7793d2842859.ctex"`

### intent defend 33

- ID: `filename----defend-intent-defend-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_33.png",`

### intent defend 33

- ID: `res---images-packed-intents-defend-intent-defend-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_33.png`

### intent defend 33.png

- ID: `images-packed-intents-defend-intent-defend-33-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_33.png.import0`

### intent defend 33.png 0a06a38debabb8351cb8e15d52408f37

- ID: `godot-imported-intent-defend-33-png-0a06a38debabb8351cb8e15d52408f37-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_33.png-0a06a38debabb8351cb8e15d52408f37.ctex`

### intent defend 33.png 0a06a38debabb8351cb8e15d52408f37

- ID: `path--res----godot-imported-intent-defend-33-png-0a06a38debabb8351cb8e15d52408f37-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_33.png-0a06a38debabb8351cb8e15d52408f37.ctex"`

### intent defend 34

- ID: `filename----defend-intent-defend-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_34.png",`

### intent defend 34

- ID: `res---images-packed-intents-defend-intent-defend-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_34.png`

### intent defend 34.png

- ID: `images-packed-intents-defend-intent-defend-34-png-import-3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_34.png.import 3`

### intent defend 34.png 04349f5472fbdfd42501bd607d779b3e

- ID: `godot-imported-intent-defend-34-png-04349f5472fbdfd42501bd607d779b3e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_34.png-04349f5472fbdfd42501bd607d779b3e.ctex`

### intent defend 34.png 04349f5472fbdfd42501bd607d779b3e

- ID: `path--res----godot-imported-intent-defend-34-png-04349f5472fbdfd42501bd607d779b3e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_34.png-04349f5472fbdfd42501bd607d779b3e.ctex"`

### intent defend 35

- ID: `filename----defend-intent-defend-35-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_35.png",`

### intent defend 35

- ID: `res---images-packed-intents-defend-intent-defend-35-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_35.png`

### intent defend 35.png

- ID: `images-packed-intents-defend-intent-defend-35-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_35.png.import`

### intent defend 35.png 306beb2c2afee33a4c0bd3ec3fc320ce

- ID: `godot-imported-intent-defend-35-png-306beb2c2afee33a4c0bd3ec3fc320ce-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_35.png-306beb2c2afee33a4c0bd3ec3fc320ce.ctex`

### intent defend 35.png 306beb2c2afee33a4c0bd3ec3fc320ce

- ID: `path--res----godot-imported-intent-defend-35-png-306beb2c2afee33a4c0bd3ec3fc320ce-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_35.png-306beb2c2afee33a4c0bd3ec3fc320ce.ctex"`

### intent defend 36

- ID: `filename----defend-intent-defend-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_36.png",`

### intent defend 36

- ID: `res---images-packed-intents-defend-intent-defend-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_36.png`

### intent defend 36.png

- ID: `images-packed-intents-defend-intent-defend-36-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_36.png.import`

### intent defend 36.png 6c2d1287d276a45f5a0136ac5f789ded

- ID: `godot-imported-intent-defend-36-png-6c2d1287d276a45f5a0136ac5f789ded-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_36.png-6c2d1287d276a45f5a0136ac5f789ded.ctex`

### intent defend 36.png 6c2d1287d276a45f5a0136ac5f789ded

- ID: `path--res----godot-imported-intent-defend-36-png-6c2d1287d276a45f5a0136ac5f789ded-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_36.png-6c2d1287d276a45f5a0136ac5f789ded.ctex"`

### intent defend 37

- ID: `filename----defend-intent-defend-37-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_37.png",`

### intent defend 37

- ID: `res---images-packed-intents-defend-intent-defend-37-pngf`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_37.pngF`

### intent defend 37.png

- ID: `images-packed-intents-defend-intent-defend-37-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_37.png.import`

### intent defend 37.png ec136baa90158b2344868954faa1a571

- ID: `godot-imported-intent-defend-37-png-ec136baa90158b2344868954faa1a571-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_37.png-ec136baa90158b2344868954faa1a571.ctex`

### intent defend 37.png ec136baa90158b2344868954faa1a571

- ID: `path--res----godot-imported-intent-defend-37-png-ec136baa90158b2344868954faa1a571-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_37.png-ec136baa90158b2344868954faa1a571.ctex"`

### intent defend 38

- ID: `filename----defend-intent-defend-38-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_38.png",`

### intent defend 38

- ID: `res---images-packed-intents-defend-intent-defend-38-png7`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_38.png7`

### intent defend 38.png

- ID: `images-packed-intents-defend-intent-defend-38-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_38.png.import`

### intent defend 38.png 61205d9a109e36d56c27d082790e1c5d

- ID: `godot-imported-intent-defend-38-png-61205d9a109e36d56c27d082790e1c5d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_38.png-61205d9a109e36d56c27d082790e1c5d.ctex`

### intent defend 38.png 61205d9a109e36d56c27d082790e1c5d

- ID: `path--res----godot-imported-intent-defend-38-png-61205d9a109e36d56c27d082790e1c5d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_38.png-61205d9a109e36d56c27d082790e1c5d.ctex"`

### intent defend 39

- ID: `filename----defend-intent-defend-39-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_39.png",`

### intent defend 39

- ID: `res---images-packed-intents-defend-intent-defend-39-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_39.png`

### intent defend 39.png

- ID: `images-packed-intents-defend-intent-defend-39-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_39.png.import`

### intent defend 39.png 73676f3f504187eebd9e5fb3b172f665

- ID: `godot-imported-intent-defend-39-png-73676f3f504187eebd9e5fb3b172f665-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_39.png-73676f3f504187eebd9e5fb3b172f665.ctex`

### intent defend 39.png 73676f3f504187eebd9e5fb3b172f665

- ID: `path--res----godot-imported-intent-defend-39-png-73676f3f504187eebd9e5fb3b172f665-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_39.png-73676f3f504187eebd9e5fb3b172f665.ctex"`

### intent defend 40

- ID: `filename----defend-intent-defend-40-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_40.png",`

### intent defend 40

- ID: `res---images-packed-intents-defend-intent-defend-40-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_40.png`

### intent defend 40.png

- ID: `images-packed-intents-defend-intent-defend-40-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_40.png.import`

### intent defend 40.png 5c2729a2f082a0e6a7adc438d0c1630d

- ID: `godot-imported-intent-defend-40-png-5c2729a2f082a0e6a7adc438d0c1630d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_40.png-5c2729a2f082a0e6a7adc438d0c1630d.ctex`

### intent defend 40.png 5c2729a2f082a0e6a7adc438d0c1630d

- ID: `path--res----godot-imported-intent-defend-40-png-5c2729a2f082a0e6a7adc438d0c1630d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_40.png-5c2729a2f082a0e6a7adc438d0c1630d.ctex"`

### intent defend 41

- ID: `filename----defend-intent-defend-41-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_41.png",`

### intent defend 41

- ID: `res---images-packed-intents-defend-intent-defend-41-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_41.png`

### intent defend 41.png

- ID: `images-packed-intents-defend-intent-defend-41-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_41.png.import`

### intent defend 41.png ad900e1952678202357d1073e17d4648

- ID: `godot-imported-intent-defend-41-png-ad900e1952678202357d1073e17d4648-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_41.png-ad900e1952678202357d1073e17d4648.ctex`

### intent defend 41.png ad900e1952678202357d1073e17d4648

- ID: `path--res----godot-imported-intent-defend-41-png-ad900e1952678202357d1073e17d4648-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_41.png-ad900e1952678202357d1073e17d4648.ctex"`

### intent defend 42

- ID: `filename----defend-intent-defend-42-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_42.png",`

### intent defend 42

- ID: `res---images-packed-intents-defend-intent-defend-42-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_42.png`

### intent defend 42.png

- ID: `images-packed-intents-defend-intent-defend-42-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_42.png.import`

### intent defend 42.png b5a20ff52f02630a9ad72303c0972393

- ID: `godot-imported-intent-defend-42-png-b5a20ff52f02630a9ad72303c0972393-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_42.png-b5a20ff52f02630a9ad72303c0972393.ctex`

### intent defend 42.png b5a20ff52f02630a9ad72303c0972393

- ID: `path--res----godot-imported-intent-defend-42-png-b5a20ff52f02630a9ad72303c0972393-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_42.png-b5a20ff52f02630a9ad72303c0972393.ctex"`

### intent defend 43

- ID: `filename----defend-intent-defend-43-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_43.png",`

### intent defend 43

- ID: `res---images-packed-intents-defend-intent-defend-43-pngh`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_43.pngh`

### intent defend 43.png

- ID: `images-packed-intents-defend-intent-defend-43-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_43.png.import`

### intent defend 43.png 68284464ca26abacc5b0dec57f5c9413

- ID: `godot-imported-intent-defend-43-png-68284464ca26abacc5b0dec57f5c9413-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_43.png-68284464ca26abacc5b0dec57f5c9413.ctex`

### intent defend 43.png 68284464ca26abacc5b0dec57f5c9413

- ID: `path--res----godot-imported-intent-defend-43-png-68284464ca26abacc5b0dec57f5c9413-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_43.png-68284464ca26abacc5b0dec57f5c9413.ctex"`

### intent defend 44

- ID: `filename----defend-intent-defend-44-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "defend/intent_defend_44.png",`

### intent defend 44

- ID: `res---images-packed-intents-defend-intent-defend-44-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_44.png)!`

### intent defend 44.png

- ID: `images-packed-intents-defend-intent-defend-44-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/defend/intent_defend_44.png.import`

### intent defend 44.png 14fb0548d5bd5cfff5876fece5fdd425

- ID: `godot-imported-intent-defend-44-png-14fb0548d5bd5cfff5876fece5fdd425-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend_44.png-14fb0548d5bd5cfff5876fece5fdd425.ctex`

### intent defend 44.png 14fb0548d5bd5cfff5876fece5fdd425

- ID: `path--res----godot-imported-intent-defend-44-png-14fb0548d5bd5cfff5876fece5fdd425-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend_44.png-14fb0548d5bd5cfff5876fece5fdd425.ctex"`

### intent defend.png

- ID: `images-packed-intents-intent-defend-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_defend.png.import`

### intent defend.png a18605b4d090c3976f048a440bd48294

- ID: `godot-imported-intent-defend-png-a18605b4d090c3976f048a440bd48294-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_defend.png-a18605b4d090c3976f048a440bd48294.ctex`

### intent defend.png a18605b4d090c3976f048a440bd48294

- ID: `path--res----godot-imported-intent-defend-png-a18605b4d090c3976f048a440bd48294-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_defend.png-a18605b4d090c3976f048a440bd48294.ctex"`

### intent escape

- ID: `res---images-packed-intents-intent-escape-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_escape.png`

### intent escape 00

- ID: `filename----escape-intent-escape-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_00.png",`

### intent escape 00

- ID: `res---images-packed-intents-escape-intent-escape-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_00.png[`

### intent escape 00.png

- ID: `images-packed-intents-escape-intent-escape-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_00.png.import`

### intent escape 00.png 470a528c4d2152310cc12a12f1a27679

- ID: `godot-imported-intent-escape-00-png-470a528c4d2152310cc12a12f1a27679-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_00.png-470a528c4d2152310cc12a12f1a27679.ctex`

### intent escape 00.png 470a528c4d2152310cc12a12f1a27679

- ID: `path--res----godot-imported-intent-escape-00-png-470a528c4d2152310cc12a12f1a27679-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_00.png-470a528c4d2152310cc12a12f1a27679.ctex"`

### intent escape 01

- ID: `filename----escape-intent-escape-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_01.png",`

### intent escape 01

- ID: `res---images-packed-intents-escape-intent-escape-01-png-f`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_01.png!f`

### intent escape 01.png

- ID: `images-packed-intents-escape-intent-escape-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_01.png.import`

### intent escape 01.png 767be07a4eec8f6cdf36b81e632a9210

- ID: `godot-imported-intent-escape-01-png-767be07a4eec8f6cdf36b81e632a9210-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_01.png-767be07a4eec8f6cdf36b81e632a9210.ctex`

### intent escape 01.png 767be07a4eec8f6cdf36b81e632a9210

- ID: `path--res----godot-imported-intent-escape-01-png-767be07a4eec8f6cdf36b81e632a9210-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_01.png-767be07a4eec8f6cdf36b81e632a9210.ctex"`

### intent escape 02

- ID: `filename----escape-intent-escape-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_02.png",`

### intent escape 02

- ID: `res---images-packed-intents-escape-intent-escape-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_02.png`

### intent escape 02.png

- ID: `images-packed-intents-escape-intent-escape-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_02.png.import`

### intent escape 02.png 798b3a54a7934f48039954d6519670ce

- ID: `godot-imported-intent-escape-02-png-798b3a54a7934f48039954d6519670ce-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_02.png-798b3a54a7934f48039954d6519670ce.ctex`

### intent escape 02.png 798b3a54a7934f48039954d6519670ce

- ID: `path--res----godot-imported-intent-escape-02-png-798b3a54a7934f48039954d6519670ce-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_02.png-798b3a54a7934f48039954d6519670ce.ctex"`

### intent escape 03

- ID: `filename----escape-intent-escape-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_03.png",`

### intent escape 03

- ID: `res---images-packed-intents-escape-intent-escape-03-pngq`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_03.pngQ`

### intent escape 03.png

- ID: `images-packed-intents-escape-intent-escape-03-png-import-z`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_03.png.import z`

### intent escape 03.png a4b3415d5fca82a50e5762d9f0999f3f

- ID: `godot-imported-intent-escape-03-png-a4b3415d5fca82a50e5762d9f0999f3f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_03.png-a4b3415d5fca82a50e5762d9f0999f3f.ctex`

### intent escape 03.png a4b3415d5fca82a50e5762d9f0999f3f

- ID: `path--res----godot-imported-intent-escape-03-png-a4b3415d5fca82a50e5762d9f0999f3f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_03.png-a4b3415d5fca82a50e5762d9f0999f3f.ctex"`

### intent escape 04

- ID: `filename----escape-intent-escape-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_04.png",`

### intent escape 04

- ID: `res---images-packed-intents-escape-intent-escape-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_04.png`

### intent escape 04.png

- ID: `images-packed-intents-escape-intent-escape-04-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_04.png.import0`

### intent escape 04.png 715dafeb0b12aae5d4a5fa7fbd690755

- ID: `godot-imported-intent-escape-04-png-715dafeb0b12aae5d4a5fa7fbd690755-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_04.png-715dafeb0b12aae5d4a5fa7fbd690755.ctex`

### intent escape 04.png 715dafeb0b12aae5d4a5fa7fbd690755

- ID: `path--res----godot-imported-intent-escape-04-png-715dafeb0b12aae5d4a5fa7fbd690755-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_04.png-715dafeb0b12aae5d4a5fa7fbd690755.ctex"`

### intent escape 05

- ID: `filename----escape-intent-escape-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_05.png",`

### intent escape 05

- ID: `res---images-packed-intents-escape-intent-escape-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_05.png`

### intent escape 05.png

- ID: `images-packed-intents-escape-intent-escape-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_05.png.import`

### intent escape 05.png 38bf8978c18548249446df68ab89d2ee

- ID: `godot-imported-intent-escape-05-png-38bf8978c18548249446df68ab89d2ee-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_05.png-38bf8978c18548249446df68ab89d2ee.ctex`

### intent escape 05.png 38bf8978c18548249446df68ab89d2ee

- ID: `path--res----godot-imported-intent-escape-05-png-38bf8978c18548249446df68ab89d2ee-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_05.png-38bf8978c18548249446df68ab89d2ee.ctex"`

### intent escape 06

- ID: `filename----escape-intent-escape-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_06.png",`

### intent escape 06

- ID: `res---images-packed-intents-escape-intent-escape-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_06.png`

### intent escape 06.png

- ID: `images-packed-intents-escape-intent-escape-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_06.png.import`

### intent escape 06.png bfbb10b0f4d00bcfc9ad7cfb7c56d54b

- ID: `godot-imported-intent-escape-06-png-bfbb10b0f4d00bcfc9ad7cfb7c56d54b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_06.png-bfbb10b0f4d00bcfc9ad7cfb7c56d54b.ctex`

### intent escape 06.png bfbb10b0f4d00bcfc9ad7cfb7c56d54b

- ID: `path--res----godot-imported-intent-escape-06-png-bfbb10b0f4d00bcfc9ad7cfb7c56d54b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_06.png-bfbb10b0f4d00bcfc9ad7cfb7c56d54b.ctex"`

### intent escape 07

- ID: `filename----escape-intent-escape-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_07.png",`

### intent escape 07

- ID: `res---images-packed-intents-escape-intent-escape-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_07.png`

### intent escape 07.png

- ID: `images-packed-intents-escape-intent-escape-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_07.png.import@`

### intent escape 07.png 4dd1ea8d32224433f5c86c55f8a0a3a2

- ID: `godot-imported-intent-escape-07-png-4dd1ea8d32224433f5c86c55f8a0a3a2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_07.png-4dd1ea8d32224433f5c86c55f8a0a3a2.ctex`

### intent escape 07.png 4dd1ea8d32224433f5c86c55f8a0a3a2

- ID: `path--res----godot-imported-intent-escape-07-png-4dd1ea8d32224433f5c86c55f8a0a3a2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_07.png-4dd1ea8d32224433f5c86c55f8a0a3a2.ctex"`

### intent escape 08

- ID: `filename----escape-intent-escape-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_08.png",`

### intent escape 08

- ID: `res---images-packed-intents-escape-intent-escape-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_08.png`

### intent escape 08.png

- ID: `images-packed-intents-escape-intent-escape-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_08.png.import`

### intent escape 08.png bf2acc32594afc236a65f6ec2a073717

- ID: `godot-imported-intent-escape-08-png-bf2acc32594afc236a65f6ec2a073717-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_08.png-bf2acc32594afc236a65f6ec2a073717.ctex`

### intent escape 08.png bf2acc32594afc236a65f6ec2a073717

- ID: `path--res----godot-imported-intent-escape-08-png-bf2acc32594afc236a65f6ec2a073717-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_08.png-bf2acc32594afc236a65f6ec2a073717.ctex"`

### intent escape 09

- ID: `filename----escape-intent-escape-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_09.png",`

### intent escape 09

- ID: `res---images-packed-intents-escape-intent-escape-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_09.png`

### intent escape 09.png

- ID: `images-packed-intents-escape-intent-escape-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_09.png.import`

### intent escape 09.png 3351cb03d9046671bf465f8e59a44548

- ID: `godot-imported-intent-escape-09-png-3351cb03d9046671bf465f8e59a44548-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_09.png-3351cb03d9046671bf465f8e59a44548.ctex`

### intent escape 09.png 3351cb03d9046671bf465f8e59a44548

- ID: `path--res----godot-imported-intent-escape-09-png-3351cb03d9046671bf465f8e59a44548-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_09.png-3351cb03d9046671bf465f8e59a44548.ctex"`

### intent escape 10

- ID: `filename----escape-intent-escape-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_10.png",`

### intent escape 10

- ID: `res---images-packed-intents-escape-intent-escape-10-png-f`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_10.png|F`

### intent escape 10.png

- ID: `images-packed-intents-escape-intent-escape-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_10.png.import`

### intent escape 10.png 630667c3b85c3f48c1d62261837127b4

- ID: `godot-imported-intent-escape-10-png-630667c3b85c3f48c1d62261837127b4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_10.png-630667c3b85c3f48c1d62261837127b4.ctex`

### intent escape 10.png 630667c3b85c3f48c1d62261837127b4

- ID: `path--res----godot-imported-intent-escape-10-png-630667c3b85c3f48c1d62261837127b4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_10.png-630667c3b85c3f48c1d62261837127b4.ctex"`

### intent escape 11

- ID: `filename----escape-intent-escape-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_11.png",`

### intent escape 11

- ID: `res---images-packed-intents-escape-intent-escape-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_11.png`

### intent escape 11.png

- ID: `images-packed-intents-escape-intent-escape-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_11.png.import`

### intent escape 11.png de9222ccfd6bc340e90c75b58a54166b

- ID: `godot-imported-intent-escape-11-png-de9222ccfd6bc340e90c75b58a54166b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_11.png-de9222ccfd6bc340e90c75b58a54166b.ctex`

### intent escape 11.png de9222ccfd6bc340e90c75b58a54166b

- ID: `path--res----godot-imported-intent-escape-11-png-de9222ccfd6bc340e90c75b58a54166b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_11.png-de9222ccfd6bc340e90c75b58a54166b.ctex"`

### intent escape 12

- ID: `filename----escape-intent-escape-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_12.png",`

### intent escape 12

- ID: `res---images-packed-intents-escape-intent-escape-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_12.png`

### intent escape 12.png

- ID: `images-packed-intents-escape-intent-escape-12-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_12.png.importp:`

### intent escape 12.png 6097033d940d6faf00586dfeae01b0f2

- ID: `godot-imported-intent-escape-12-png-6097033d940d6faf00586dfeae01b0f2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_12.png-6097033d940d6faf00586dfeae01b0f2.ctex`

### intent escape 12.png 6097033d940d6faf00586dfeae01b0f2

- ID: `path--res----godot-imported-intent-escape-12-png-6097033d940d6faf00586dfeae01b0f2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_12.png-6097033d940d6faf00586dfeae01b0f2.ctex"`

### intent escape 13

- ID: `filename----escape-intent-escape-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_13.png",`

### intent escape 13

- ID: `res---images-packed-intents-escape-intent-escape-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_13.png`

### intent escape 13.png

- ID: `images-packed-intents-escape-intent-escape-13-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_13.png.import`

### intent escape 13.png 42f55483d26d22ffdd7e5226b7100b18

- ID: `godot-imported-intent-escape-13-png-42f55483d26d22ffdd7e5226b7100b18-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_13.png-42f55483d26d22ffdd7e5226b7100b18.ctex`

### intent escape 13.png 42f55483d26d22ffdd7e5226b7100b18

- ID: `path--res----godot-imported-intent-escape-13-png-42f55483d26d22ffdd7e5226b7100b18-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_13.png-42f55483d26d22ffdd7e5226b7100b18.ctex"`

### intent escape 14

- ID: `filename----escape-intent-escape-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_14.png",`

### intent escape 14

- ID: `res---images-packed-intents-escape-intent-escape-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_14.png`

### intent escape 14.png

- ID: `images-packed-intents-escape-intent-escape-14-png-import0k`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_14.png.import0k`

### intent escape 14.png b329c22fba2775ebff56ee41bc72daf4

- ID: `godot-imported-intent-escape-14-png-b329c22fba2775ebff56ee41bc72daf4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_14.png-b329c22fba2775ebff56ee41bc72daf4.ctex`

### intent escape 14.png b329c22fba2775ebff56ee41bc72daf4

- ID: `path--res----godot-imported-intent-escape-14-png-b329c22fba2775ebff56ee41bc72daf4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_14.png-b329c22fba2775ebff56ee41bc72daf4.ctex"`

### intent escape 15

- ID: `filename----escape-intent-escape-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_15.png",`

### intent escape 15

- ID: `res---images-packed-intents-escape-intent-escape-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_15.png`

### intent escape 15.png

- ID: `images-packed-intents-escape-intent-escape-15-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_15.png.import`

### intent escape 15.png ba8f0812dab8825f6b3b2d39e646b061

- ID: `godot-imported-intent-escape-15-png-ba8f0812dab8825f6b3b2d39e646b061-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_15.png-ba8f0812dab8825f6b3b2d39e646b061.ctex`

### intent escape 15.png ba8f0812dab8825f6b3b2d39e646b061

- ID: `path--res----godot-imported-intent-escape-15-png-ba8f0812dab8825f6b3b2d39e646b061-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_15.png-ba8f0812dab8825f6b3b2d39e646b061.ctex"`

### intent escape 16

- ID: `filename----escape-intent-escape-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_16.png",`

### intent escape 16

- ID: `res---images-packed-intents-escape-intent-escape-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_16.png`

### intent escape 16.png

- ID: `images-packed-intents-escape-intent-escape-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_16.png.import`

### intent escape 16.png 2baf6ad61b0da30781b927980deb500b

- ID: `godot-imported-intent-escape-16-png-2baf6ad61b0da30781b927980deb500b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_16.png-2baf6ad61b0da30781b927980deb500b.ctex`

### intent escape 16.png 2baf6ad61b0da30781b927980deb500b

- ID: `path--res----godot-imported-intent-escape-16-png-2baf6ad61b0da30781b927980deb500b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_16.png-2baf6ad61b0da30781b927980deb500b.ctex"`

### intent escape 17

- ID: `filename----escape-intent-escape-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_17.png",`

### intent escape 17

- ID: `res---images-packed-intents-escape-intent-escape-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_17.png'`

### intent escape 17.png

- ID: `images-packed-intents-escape-intent-escape-17-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_17.png.importP`

### intent escape 17.png 3b9b6b5b670dadc327a3379581633b8d

- ID: `godot-imported-intent-escape-17-png-3b9b6b5b670dadc327a3379581633b8d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_17.png-3b9b6b5b670dadc327a3379581633b8d.ctex`

### intent escape 17.png 3b9b6b5b670dadc327a3379581633b8d

- ID: `path--res----godot-imported-intent-escape-17-png-3b9b6b5b670dadc327a3379581633b8d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_17.png-3b9b6b5b670dadc327a3379581633b8d.ctex"`

### intent escape 18

- ID: `filename----escape-intent-escape-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_18.png",`

### intent escape 18

- ID: `res---images-packed-intents-escape-intent-escape-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_18.png`

### intent escape 18.png

- ID: `images-packed-intents-escape-intent-escape-18-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_18.png.import`

### intent escape 18.png 09368b17c8c36f90b14a44254088bc48

- ID: `godot-imported-intent-escape-18-png-09368b17c8c36f90b14a44254088bc48-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_18.png-09368b17c8c36f90b14a44254088bc48.ctex`

### intent escape 18.png 09368b17c8c36f90b14a44254088bc48

- ID: `path--res----godot-imported-intent-escape-18-png-09368b17c8c36f90b14a44254088bc48-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_18.png-09368b17c8c36f90b14a44254088bc48.ctex"`

### intent escape 19

- ID: `filename----escape-intent-escape-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_19.png",`

### intent escape 19

- ID: `res---images-packed-intents-escape-intent-escape-19-png-wr`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_19.png|wr`

### intent escape 19.png

- ID: `images-packed-intents-escape-intent-escape-19-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_19.png.import`

### intent escape 19.png 64c9fac1d72190986710306f42ee77f0

- ID: `godot-imported-intent-escape-19-png-64c9fac1d72190986710306f42ee77f0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_19.png-64c9fac1d72190986710306f42ee77f0.ctex`

### intent escape 19.png 64c9fac1d72190986710306f42ee77f0

- ID: `path--res----godot-imported-intent-escape-19-png-64c9fac1d72190986710306f42ee77f0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_19.png-64c9fac1d72190986710306f42ee77f0.ctex"`

### intent escape 20

- ID: `filename----escape-intent-escape-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_20.png",`

### intent escape 20

- ID: `res---images-packed-intents-escape-intent-escape-20-pngds`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_20.pngDs`

### intent escape 20.png

- ID: `images-packed-intents-escape-intent-escape-20-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_20.png.importp`

### intent escape 20.png 7e861d34792c2dc7a5908cdfd9601b2b

- ID: `godot-imported-intent-escape-20-png-7e861d34792c2dc7a5908cdfd9601b2b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_20.png-7e861d34792c2dc7a5908cdfd9601b2b.ctex`

### intent escape 20.png 7e861d34792c2dc7a5908cdfd9601b2b

- ID: `path--res----godot-imported-intent-escape-20-png-7e861d34792c2dc7a5908cdfd9601b2b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_20.png-7e861d34792c2dc7a5908cdfd9601b2b.ctex"`

### intent escape 21

- ID: `filename----escape-intent-escape-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_21.png",`

### intent escape 21

- ID: `res---images-packed-intents-escape-intent-escape-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_21.png`

### intent escape 21.png

- ID: `images-packed-intents-escape-intent-escape-21-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_21.png.import`

### intent escape 21.png d4ec2b33a401345afbcefbff3400ebf3

- ID: `godot-imported-intent-escape-21-png-d4ec2b33a401345afbcefbff3400ebf3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_21.png-d4ec2b33a401345afbcefbff3400ebf3.ctex`

### intent escape 21.png d4ec2b33a401345afbcefbff3400ebf3

- ID: `path--res----godot-imported-intent-escape-21-png-d4ec2b33a401345afbcefbff3400ebf3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_21.png-d4ec2b33a401345afbcefbff3400ebf3.ctex"`

### intent escape 22

- ID: `filename----escape-intent-escape-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_22.png",`

### intent escape 22

- ID: `res---images-packed-intents-escape-intent-escape-22-pngw`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_22.pngw`

### intent escape 22.png 0213f579df49afeed69e493b88aa7e2d

- ID: `godot-imported-intent-escape-22-png-0213f579df49afeed69e493b88aa7e2d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_22.png-0213f579df49afeed69e493b88aa7e2d.ctex`

### intent escape 22.png 0213f579df49afeed69e493b88aa7e2d

- ID: `path--res----godot-imported-intent-escape-22-png-0213f579df49afeed69e493b88aa7e2d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_22.png-0213f579df49afeed69e493b88aa7e2d.ctex"`

### intent escape 22.png.import0

- ID: `images-packed-intents-escape-intent-escape-22-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_22.png.import0.`

### intent escape 23

- ID: `filename----escape-intent-escape-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_23.png",`

### intent escape 23

- ID: `res---images-packed-intents-escape-intent-escape-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_23.png`

### intent escape 23.png

- ID: `images-packed-intents-escape-intent-escape-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_23.png.import`

### intent escape 23.png 24dfbe51647d8916cfaa108f8b92844e

- ID: `godot-imported-intent-escape-23-png-24dfbe51647d8916cfaa108f8b92844e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_23.png-24dfbe51647d8916cfaa108f8b92844e.ctex`

### intent escape 23.png 24dfbe51647d8916cfaa108f8b92844e

- ID: `path--res----godot-imported-intent-escape-23-png-24dfbe51647d8916cfaa108f8b92844e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_23.png-24dfbe51647d8916cfaa108f8b92844e.ctex"`

### intent escape 24

- ID: `filename----escape-intent-escape-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_24.png",`

### intent escape 24

- ID: `res---images-packed-intents-escape-intent-escape-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_24.png`

### intent escape 24.png

- ID: `images-packed-intents-escape-intent-escape-24-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_24.png.import`

### intent escape 24.png 2098f032ce6f5281969ba9b673a5b78f

- ID: `godot-imported-intent-escape-24-png-2098f032ce6f5281969ba9b673a5b78f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_24.png-2098f032ce6f5281969ba9b673a5b78f.ctex`

### intent escape 24.png 2098f032ce6f5281969ba9b673a5b78f

- ID: `path--res----godot-imported-intent-escape-24-png-2098f032ce6f5281969ba9b673a5b78f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_24.png-2098f032ce6f5281969ba9b673a5b78f.ctex"`

### intent escape 25

- ID: `filename----escape-intent-escape-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_25.png",`

### intent escape 25

- ID: `res---images-packed-intents-escape-intent-escape-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_25.png`

### intent escape 25.png

- ID: `images-packed-intents-escape-intent-escape-25-png-importpw`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_25.png.importPw`

### intent escape 25.png b39f71bace58ef0c799c2eb2b83d92b2

- ID: `godot-imported-intent-escape-25-png-b39f71bace58ef0c799c2eb2b83d92b2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_25.png-b39f71bace58ef0c799c2eb2b83d92b2.ctex`

### intent escape 25.png b39f71bace58ef0c799c2eb2b83d92b2

- ID: `path--res----godot-imported-intent-escape-25-png-b39f71bace58ef0c799c2eb2b83d92b2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_25.png-b39f71bace58ef0c799c2eb2b83d92b2.ctex"`

### intent escape 26

- ID: `filename----escape-intent-escape-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_26.png",`

### intent escape 26

- ID: `res---images-packed-intents-escape-intent-escape-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_26.png`

### intent escape 26.png

- ID: `images-packed-intents-escape-intent-escape-26-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_26.png.import`

### intent escape 26.png 25ef6532e3e764a24d6b5c48466fb8ec

- ID: `godot-imported-intent-escape-26-png-25ef6532e3e764a24d6b5c48466fb8ec-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_26.png-25ef6532e3e764a24d6b5c48466fb8ec.ctex`

### intent escape 26.png 25ef6532e3e764a24d6b5c48466fb8ec

- ID: `path--res----godot-imported-intent-escape-26-png-25ef6532e3e764a24d6b5c48466fb8ec-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_26.png-25ef6532e3e764a24d6b5c48466fb8ec.ctex"`

### intent escape 27

- ID: `filename----escape-intent-escape-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_27.png",`

### intent escape 27

- ID: `res---images-packed-intents-escape-intent-escape-27-pnga`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_27.pnga`

### intent escape 27.png

- ID: `images-packed-intents-escape-intent-escape-27-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_27.png.import`

### intent escape 27.png 9c91b8fdf2d1386421cafcfae93804ac

- ID: `godot-imported-intent-escape-27-png-9c91b8fdf2d1386421cafcfae93804ac-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_27.png-9c91b8fdf2d1386421cafcfae93804ac.ctex`

### intent escape 27.png 9c91b8fdf2d1386421cafcfae93804ac

- ID: `path--res----godot-imported-intent-escape-27-png-9c91b8fdf2d1386421cafcfae93804ac-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_27.png-9c91b8fdf2d1386421cafcfae93804ac.ctex"`

### intent escape 28

- ID: `filename----escape-intent-escape-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_28.png",`

### intent escape 28

- ID: `res---images-packed-intents-escape-intent-escape-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_28.png)#`

### intent escape 28.png

- ID: `images-packed-intents-escape-intent-escape-28-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_28.png.importp`

### intent escape 28.png 205b6ab3e21175fc3058716a9312bfe4

- ID: `godot-imported-intent-escape-28-png-205b6ab3e21175fc3058716a9312bfe4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_28.png-205b6ab3e21175fc3058716a9312bfe4.ctex`

### intent escape 28.png 205b6ab3e21175fc3058716a9312bfe4

- ID: `path--res----godot-imported-intent-escape-28-png-205b6ab3e21175fc3058716a9312bfe4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_28.png-205b6ab3e21175fc3058716a9312bfe4.ctex"`

### intent escape 29

- ID: `filename----escape-intent-escape-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_29.png",`

### intent escape 29

- ID: `res---images-packed-intents-escape-intent-escape-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_29.png`

### intent escape 29.png

- ID: `images-packed-intents-escape-intent-escape-29-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_29.png.import`

### intent escape 29.png fcbb3d253f0e5571a66d69f6d0025879

- ID: `godot-imported-intent-escape-29-png-fcbb3d253f0e5571a66d69f6d0025879-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_29.png-fcbb3d253f0e5571a66d69f6d0025879.ctex`

### intent escape 29.png fcbb3d253f0e5571a66d69f6d0025879

- ID: `path--res----godot-imported-intent-escape-29-png-fcbb3d253f0e5571a66d69f6d0025879-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_29.png-fcbb3d253f0e5571a66d69f6d0025879.ctex"`

### intent escape 30

- ID: `filename----escape-intent-escape-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_30.png",`

### intent escape 30

- ID: `res---images-packed-intents-escape-intent-escape-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_30.png`

### intent escape 30.png

- ID: `images-packed-intents-escape-intent-escape-30-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_30.png.import`

### intent escape 30.png cb43a2553f62c118414ab7fd0469bcfe

- ID: `godot-imported-intent-escape-30-png-cb43a2553f62c118414ab7fd0469bcfe-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_30.png-cb43a2553f62c118414ab7fd0469bcfe.ctex`

### intent escape 30.png cb43a2553f62c118414ab7fd0469bcfe

- ID: `path--res----godot-imported-intent-escape-30-png-cb43a2553f62c118414ab7fd0469bcfe-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_30.png-cb43a2553f62c118414ab7fd0469bcfe.ctex"`

### intent escape 31

- ID: `filename----escape-intent-escape-31-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_31.png",`

### intent escape 31

- ID: `res---images-packed-intents-escape-intent-escape-31-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_31.png`

### intent escape 31.png

- ID: `images-packed-intents-escape-intent-escape-31-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_31.png.import`

### intent escape 31.png 5ff4741c862b84a7e288a386d5a58e1a

- ID: `godot-imported-intent-escape-31-png-5ff4741c862b84a7e288a386d5a58e1a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_31.png-5ff4741c862b84a7e288a386d5a58e1a.ctex`

### intent escape 31.png 5ff4741c862b84a7e288a386d5a58e1a

- ID: `path--res----godot-imported-intent-escape-31-png-5ff4741c862b84a7e288a386d5a58e1a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_31.png-5ff4741c862b84a7e288a386d5a58e1a.ctex"`

### intent escape 32

- ID: `filename----escape-intent-escape-32-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_32.png",`

### intent escape 32

- ID: `res---images-packed-intents-escape-intent-escape-32-pngd`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_32.pngd`

### intent escape 32.png

- ID: `images-packed-intents-escape-intent-escape-32-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_32.png.importp`

### intent escape 32.png 06c42899d4a9ab039b0aa8aff92bfbe5

- ID: `godot-imported-intent-escape-32-png-06c42899d4a9ab039b0aa8aff92bfbe5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_32.png-06c42899d4a9ab039b0aa8aff92bfbe5.ctex`

### intent escape 32.png 06c42899d4a9ab039b0aa8aff92bfbe5

- ID: `path--res----godot-imported-intent-escape-32-png-06c42899d4a9ab039b0aa8aff92bfbe5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_32.png-06c42899d4a9ab039b0aa8aff92bfbe5.ctex"`

### intent escape 33

- ID: `filename----escape-intent-escape-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_33.png",`

### intent escape 33

- ID: `res---images-packed-intents-escape-intent-escape-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_33.png`

### intent escape 33.png

- ID: `images-packed-intents-escape-intent-escape-33-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_33.png.import@-`

### intent escape 33.png 469a1f38be8ab4bd28ccbc01987f0521

- ID: `godot-imported-intent-escape-33-png-469a1f38be8ab4bd28ccbc01987f0521-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_33.png-469a1f38be8ab4bd28ccbc01987f0521.ctex`

### intent escape 33.png 469a1f38be8ab4bd28ccbc01987f0521

- ID: `path--res----godot-imported-intent-escape-33-png-469a1f38be8ab4bd28ccbc01987f0521-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_33.png-469a1f38be8ab4bd28ccbc01987f0521.ctex"`

### intent escape 34

- ID: `filename----escape-intent-escape-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_34.png",`

### intent escape 34

- ID: `res---images-packed-intents-escape-intent-escape-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_34.png`

### intent escape 34.png

- ID: `images-packed-intents-escape-intent-escape-34-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_34.png.import`

### intent escape 34.png 43ca5b1a3c6674f554e3e074430a76e7

- ID: `godot-imported-intent-escape-34-png-43ca5b1a3c6674f554e3e074430a76e7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_34.png-43ca5b1a3c6674f554e3e074430a76e7.ctex`

### intent escape 34.png 43ca5b1a3c6674f554e3e074430a76e7

- ID: `path--res----godot-imported-intent-escape-34-png-43ca5b1a3c6674f554e3e074430a76e7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_34.png-43ca5b1a3c6674f554e3e074430a76e7.ctex"`

### intent escape 35

- ID: `filename----escape-intent-escape-35-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_35.png",`

### intent escape 35

- ID: `res---images-packed-intents-escape-intent-escape-35-png5`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_35.png5`

### intent escape 35.png

- ID: `images-packed-intents-escape-intent-escape-35-png-import-m`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_35.png.import@M`

### intent escape 35.png 6ba62eb68a856dc9575d28b60435cd11

- ID: `godot-imported-intent-escape-35-png-6ba62eb68a856dc9575d28b60435cd11-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_35.png-6ba62eb68a856dc9575d28b60435cd11.ctex`

### intent escape 35.png 6ba62eb68a856dc9575d28b60435cd11

- ID: `path--res----godot-imported-intent-escape-35-png-6ba62eb68a856dc9575d28b60435cd11-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_35.png-6ba62eb68a856dc9575d28b60435cd11.ctex"`

### intent escape 36

- ID: `filename----escape-intent-escape-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_36.png",`

### intent escape 36

- ID: `res---images-packed-intents-escape-intent-escape-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_36.png`

### intent escape 36.png

- ID: `images-packed-intents-escape-intent-escape-36-png-import-z`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_36.png.import Z`

### intent escape 36.png 9a6c14dce2a53c0bb6eb7cf8e846871a

- ID: `godot-imported-intent-escape-36-png-9a6c14dce2a53c0bb6eb7cf8e846871a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_36.png-9a6c14dce2a53c0bb6eb7cf8e846871a.ctex`

### intent escape 36.png 9a6c14dce2a53c0bb6eb7cf8e846871a

- ID: `path--res----godot-imported-intent-escape-36-png-9a6c14dce2a53c0bb6eb7cf8e846871a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_36.png-9a6c14dce2a53c0bb6eb7cf8e846871a.ctex"`

### intent escape 37

- ID: `filename----escape-intent-escape-37-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_37.png",`

### intent escape 37

- ID: `res---images-packed-intents-escape-intent-escape-37-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_37.png`

### intent escape 37.png

- ID: `images-packed-intents-escape-intent-escape-37-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_37.png.import`

### intent escape 37.png 371f3915709c75c601df793b17a6aa4b

- ID: `godot-imported-intent-escape-37-png-371f3915709c75c601df793b17a6aa4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_37.png-371f3915709c75c601df793b17a6aa4b.ctex`

### intent escape 37.png 371f3915709c75c601df793b17a6aa4b

- ID: `path--res----godot-imported-intent-escape-37-png-371f3915709c75c601df793b17a6aa4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_37.png-371f3915709c75c601df793b17a6aa4b.ctex"`

### intent escape 38

- ID: `filename----escape-intent-escape-38-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_38.png",`

### intent escape 38

- ID: `res---images-packed-intents-escape-intent-escape-38-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_38.png`

### intent escape 38.png

- ID: `images-packed-intents-escape-intent-escape-38-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_38.png.import`

### intent escape 38.png f14e10474a07fc8f25404c677e90212d

- ID: `godot-imported-intent-escape-38-png-f14e10474a07fc8f25404c677e90212d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_38.png-f14e10474a07fc8f25404c677e90212d.ctex`

### intent escape 38.png f14e10474a07fc8f25404c677e90212d

- ID: `path--res----godot-imported-intent-escape-38-png-f14e10474a07fc8f25404c677e90212d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_38.png-f14e10474a07fc8f25404c677e90212d.ctex"`

### intent escape 39

- ID: `filename----escape-intent-escape-39-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "escape/intent_escape_39.png",`

### intent escape 39

- ID: `res---images-packed-intents-escape-intent-escape-39-pngu`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/escape/intent_escape_39.pngU`

### intent escape 39.png

- ID: `images-packed-intents-escape-intent-escape-39-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/escape/intent_escape_39.png.import`

### intent escape 39.png 327619110d0901e0bcbcffa1faf1edb9

- ID: `godot-imported-intent-escape-39-png-327619110d0901e0bcbcffa1faf1edb9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape_39.png-327619110d0901e0bcbcffa1faf1edb9.ctex`

### intent escape 39.png 327619110d0901e0bcbcffa1faf1edb9

- ID: `path--res----godot-imported-intent-escape-39-png-327619110d0901e0bcbcffa1faf1edb9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape_39.png-327619110d0901e0bcbcffa1faf1edb9.ctex"`

### intent escape.png

- ID: `images-packed-intents-intent-escape-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_escape.png.import`

### intent escape.png 82db06a65777761586bc84c13e1f4fd5

- ID: `godot-imported-intent-escape-png-82db06a65777761586bc84c13e1f4fd5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_escape.png-82db06a65777761586bc84c13e1f4fd5.ctex`

### intent escape.png 82db06a65777761586bc84c13e1f4fd5

- ID: `path--res----godot-imported-intent-escape-png-82db06a65777761586bc84c13e1f4fd5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_escape.png-82db06a65777761586bc84c13e1f4fd5.ctex"`

### intent Fade Tween

- ID: `intentfadetween`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent heal

- ID: `res---images-packed-intents-intent-heal-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_heal.png@+`

### intent heal 00

- ID: `filename----heal-intent-heal-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_00.png",`

### intent heal 00

- ID: `res---images-packed-intents-heal-intent-heal-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_00.png`

### intent heal 00.png

- ID: `images-packed-intents-heal-intent-heal-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_00.png.import@`

### intent heal 00.png e4aa55e8fdec85359c1a6aacd225aec6

- ID: `godot-imported-intent-heal-00-png-e4aa55e8fdec85359c1a6aacd225aec6-ctexps`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_00.png-e4aa55e8fdec85359c1a6aacd225aec6.ctexPs`

### intent heal 00.png e4aa55e8fdec85359c1a6aacd225aec6

- ID: `path--res----godot-imported-intent-heal-00-png-e4aa55e8fdec85359c1a6aacd225aec6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_00.png-e4aa55e8fdec85359c1a6aacd225aec6.ctex"`

### intent heal 01

- ID: `filename----heal-intent-heal-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_01.png",`

### intent heal 01

- ID: `res---images-packed-intents-heal-intent-heal-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_01.png`

### intent heal 01.png

- ID: `images-packed-intents-heal-intent-heal-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_01.png.import`

### intent heal 01.png b7701ba90dbf49cccb98a80b358670bc

- ID: `godot-imported-intent-heal-01-png-b7701ba90dbf49cccb98a80b358670bc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_01.png-b7701ba90dbf49cccb98a80b358670bc.ctex`

### intent heal 01.png b7701ba90dbf49cccb98a80b358670bc

- ID: `path--res----godot-imported-intent-heal-01-png-b7701ba90dbf49cccb98a80b358670bc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_01.png-b7701ba90dbf49cccb98a80b358670bc.ctex"`

### intent heal 02

- ID: `filename----heal-intent-heal-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_02.png",`

### intent heal 02

- ID: `res---images-packed-intents-heal-intent-heal-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_02.png`

### intent heal 02.png

- ID: `images-packed-intents-heal-intent-heal-02-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_02.png.import0`

### intent heal 02.png 2d3ca8c36e14af9f54bd8e000e93022d

- ID: `godot-imported-intent-heal-02-png-2d3ca8c36e14af9f54bd8e000e93022d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_02.png-2d3ca8c36e14af9f54bd8e000e93022d.ctex``

### intent heal 02.png 2d3ca8c36e14af9f54bd8e000e93022d

- ID: `path--res----godot-imported-intent-heal-02-png-2d3ca8c36e14af9f54bd8e000e93022d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_02.png-2d3ca8c36e14af9f54bd8e000e93022d.ctex"`

### intent heal 03

- ID: `filename----heal-intent-heal-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_03.png",`

### intent heal 03

- ID: `res---images-packed-intents-heal-intent-heal-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_03.png`

### intent heal 03.png

- ID: `images-packed-intents-heal-intent-heal-03-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_03.png.importP`

### intent heal 03.png 737cca99af4399099d731df213bd7737

- ID: `godot-imported-intent-heal-03-png-737cca99af4399099d731df213bd7737-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_03.png-737cca99af4399099d731df213bd7737.ctex`

### intent heal 03.png 737cca99af4399099d731df213bd7737

- ID: `path--res----godot-imported-intent-heal-03-png-737cca99af4399099d731df213bd7737-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_03.png-737cca99af4399099d731df213bd7737.ctex"`

### intent heal 04

- ID: `filename----heal-intent-heal-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_04.png",`

### intent heal 04

- ID: `res---images-packed-intents-heal-intent-heal-04-png05fj-a--3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_04.png05fj=A??3`

### intent heal 04.png

- ID: `images-packed-intents-heal-intent-heal-04-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_04.png.importP`

### intent heal 04.png e45ac457a1584e4c383232ead520c2ed

- ID: `godot-imported-intent-heal-04-png-e45ac457a1584e4c383232ead520c2ed-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_04.png-e45ac457a1584e4c383232ead520c2ed.ctex`

### intent heal 04.png e45ac457a1584e4c383232ead520c2ed

- ID: `path--res----godot-imported-intent-heal-04-png-e45ac457a1584e4c383232ead520c2ed-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_04.png-e45ac457a1584e4c383232ead520c2ed.ctex"`

### intent heal 05

- ID: `filename----heal-intent-heal-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_05.png",`

### intent heal 05

- ID: `res---images-packed-intents-heal-intent-heal-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_05.png`

### intent heal 05.png

- ID: `images-packed-intents-heal-intent-heal-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_05.png.import`

### intent heal 05.png cc8e515668bff46486e4429e7abd0d2b

- ID: `godot-imported-intent-heal-05-png-cc8e515668bff46486e4429e7abd0d2b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_05.png-cc8e515668bff46486e4429e7abd0d2b.ctex`

### intent heal 05.png cc8e515668bff46486e4429e7abd0d2b

- ID: `path--res----godot-imported-intent-heal-05-png-cc8e515668bff46486e4429e7abd0d2b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_05.png-cc8e515668bff46486e4429e7abd0d2b.ctex"`

### intent heal 06

- ID: `filename----heal-intent-heal-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_06.png",`

### intent heal 06

- ID: `res---images-packed-intents-heal-intent-heal-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_06.png`

### intent heal 06.png

- ID: `images-packed-intents-heal-intent-heal-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_06.png.import@`

### intent heal 06.png a12d8eea0f7e738eb0fa235fe8d3d066

- ID: `godot-imported-intent-heal-06-png-a12d8eea0f7e738eb0fa235fe8d3d066-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_06.png-a12d8eea0f7e738eb0fa235fe8d3d066.ctex`

### intent heal 06.png a12d8eea0f7e738eb0fa235fe8d3d066

- ID: `path--res----godot-imported-intent-heal-06-png-a12d8eea0f7e738eb0fa235fe8d3d066-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_06.png-a12d8eea0f7e738eb0fa235fe8d3d066.ctex"`

### intent heal 07

- ID: `filename----heal-intent-heal-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_07.png",`

### intent heal 07

- ID: `res---images-packed-intents-heal-intent-heal-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_07.png`

### intent heal 07.png

- ID: `images-packed-intents-heal-intent-heal-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_07.png.import`

### intent heal 07.png e37620dcc458547e5a5726a04f90e18d

- ID: `godot-imported-intent-heal-07-png-e37620dcc458547e5a5726a04f90e18d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_07.png-e37620dcc458547e5a5726a04f90e18d.ctex`

### intent heal 07.png e37620dcc458547e5a5726a04f90e18d

- ID: `path--res----godot-imported-intent-heal-07-png-e37620dcc458547e5a5726a04f90e18d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_07.png-e37620dcc458547e5a5726a04f90e18d.ctex"`

### intent heal 08

- ID: `filename----heal-intent-heal-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_08.png",`

### intent heal 08

- ID: `res---images-packed-intents-heal-intent-heal-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_08.png`

### intent heal 08.png

- ID: `images-packed-intents-heal-intent-heal-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_08.png.import``

### intent heal 08.png fb6dfba47f180070b05ca50fd039b44a

- ID: `godot-imported-intent-heal-08-png-fb6dfba47f180070b05ca50fd039b44a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_08.png-fb6dfba47f180070b05ca50fd039b44a.ctex`

### intent heal 08.png fb6dfba47f180070b05ca50fd039b44a

- ID: `path--res----godot-imported-intent-heal-08-png-fb6dfba47f180070b05ca50fd039b44a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_08.png-fb6dfba47f180070b05ca50fd039b44a.ctex"`

### intent heal 09

- ID: `filename----heal-intent-heal-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_09.png",`

### intent heal 09

- ID: `res---images-packed-intents-heal-intent-heal-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_09.png`

### intent heal 09.png

- ID: `images-packed-intents-heal-intent-heal-09-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_09.png.import0#`

### intent heal 09.png ace3a4b9116349fe618a6e24e1ef746b

- ID: `godot-imported-intent-heal-09-png-ace3a4b9116349fe618a6e24e1ef746b-ctex0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_09.png-ace3a4b9116349fe618a6e24e1ef746b.ctex0`

### intent heal 09.png ace3a4b9116349fe618a6e24e1ef746b

- ID: `path--res----godot-imported-intent-heal-09-png-ace3a4b9116349fe618a6e24e1ef746b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_09.png-ace3a4b9116349fe618a6e24e1ef746b.ctex"`

### intent heal 10

- ID: `filename----heal-intent-heal-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_10.png",`

### intent heal 10

- ID: `res---images-packed-intents-heal-intent-heal-10-pnggb`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_10.pngGb`

### intent heal 10.png

- ID: `images-packed-intents-heal-intent-heal-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_10.png.import`

### intent heal 10.png e5e93c485201faa31eef08d68aeeb3fd

- ID: `godot-imported-intent-heal-10-png-e5e93c485201faa31eef08d68aeeb3fd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_10.png-e5e93c485201faa31eef08d68aeeb3fd.ctex`

### intent heal 10.png e5e93c485201faa31eef08d68aeeb3fd

- ID: `path--res----godot-imported-intent-heal-10-png-e5e93c485201faa31eef08d68aeeb3fd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_10.png-e5e93c485201faa31eef08d68aeeb3fd.ctex"`

### intent heal 11

- ID: `filename----heal-intent-heal-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_11.png",`

### intent heal 11

- ID: `res---images-packed-intents-heal-intent-heal-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_11.png`

### intent heal 11.png

- ID: `images-packed-intents-heal-intent-heal-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_11.png.import`

### intent heal 11.png 023aa3ffce2596a0f87e70ac3a84f152

- ID: `godot-imported-intent-heal-11-png-023aa3ffce2596a0f87e70ac3a84f152-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_11.png-023aa3ffce2596a0f87e70ac3a84f152.ctex`

### intent heal 11.png 023aa3ffce2596a0f87e70ac3a84f152

- ID: `path--res----godot-imported-intent-heal-11-png-023aa3ffce2596a0f87e70ac3a84f152-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_11.png-023aa3ffce2596a0f87e70ac3a84f152.ctex"`

### intent heal 12

- ID: `filename----heal-intent-heal-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_12.png",`

### intent heal 12

- ID: `res---images-packed-intents-heal-intent-heal-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_12.png]`

### intent heal 12.png

- ID: `images-packed-intents-heal-intent-heal-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_12.png.import`

### intent heal 12.png 9d6eb62c6254caf5c065800dd00967b7

- ID: `godot-imported-intent-heal-12-png-9d6eb62c6254caf5c065800dd00967b7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_12.png-9d6eb62c6254caf5c065800dd00967b7.ctex`

### intent heal 12.png 9d6eb62c6254caf5c065800dd00967b7

- ID: `path--res----godot-imported-intent-heal-12-png-9d6eb62c6254caf5c065800dd00967b7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_12.png-9d6eb62c6254caf5c065800dd00967b7.ctex"`

### intent heal 13

- ID: `filename----heal-intent-heal-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_13.png",`

### intent heal 13

- ID: `res---images-packed-intents-heal-intent-heal-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_13.png`

### intent heal 13.png

- ID: `images-packed-intents-heal-intent-heal-13-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_13.png.import`

### intent heal 13.png 60b9b92d51357903c4f162861951919c

- ID: `godot-imported-intent-heal-13-png-60b9b92d51357903c4f162861951919c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_13.png-60b9b92d51357903c4f162861951919c.ctex`

### intent heal 13.png 60b9b92d51357903c4f162861951919c

- ID: `path--res----godot-imported-intent-heal-13-png-60b9b92d51357903c4f162861951919c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_13.png-60b9b92d51357903c4f162861951919c.ctex"`

### intent heal 14

- ID: `filename----heal-intent-heal-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_14.png",`

### intent heal 14

- ID: `res---images-packed-intents-heal-intent-heal-14-png1`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_14.png1:`

### intent heal 14.png

- ID: `images-packed-intents-heal-intent-heal-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_14.png.import`

### intent heal 14.png 1c0b248575386799516d4eb09065b6e3

- ID: `godot-imported-intent-heal-14-png-1c0b248575386799516d4eb09065b6e3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_14.png-1c0b248575386799516d4eb09065b6e3.ctex`

### intent heal 14.png 1c0b248575386799516d4eb09065b6e3

- ID: `path--res----godot-imported-intent-heal-14-png-1c0b248575386799516d4eb09065b6e3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_14.png-1c0b248575386799516d4eb09065b6e3.ctex"`

### intent heal 15

- ID: `filename----heal-intent-heal-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_15.png",`

### intent heal 15

- ID: `res---images-packed-intents-heal-intent-heal-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_15.png`

### intent heal 15.png

- ID: `images-packed-intents-heal-intent-heal-15-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_15.png.import`

### intent heal 15.png 0d94eb4db7a37bc619462520bd33da6d

- ID: `godot-imported-intent-heal-15-png-0d94eb4db7a37bc619462520bd33da6d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_15.png-0d94eb4db7a37bc619462520bd33da6d.ctex``

### intent heal 15.png 0d94eb4db7a37bc619462520bd33da6d

- ID: `path--res----godot-imported-intent-heal-15-png-0d94eb4db7a37bc619462520bd33da6d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_15.png-0d94eb4db7a37bc619462520bd33da6d.ctex"`

### intent heal 16

- ID: `filename----heal-intent-heal-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_16.png",`

### intent heal 16

- ID: `res---images-packed-intents-heal-intent-heal-16-pngb`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_16.pngB`

### intent heal 16.png

- ID: `images-packed-intents-heal-intent-heal-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_16.png.import`

### intent heal 16.png 4f34d204d7ee5e97ff9aee7cb4a03fbb

- ID: `godot-imported-intent-heal-16-png-4f34d204d7ee5e97ff9aee7cb4a03fbb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_16.png-4f34d204d7ee5e97ff9aee7cb4a03fbb.ctex`

### intent heal 16.png 4f34d204d7ee5e97ff9aee7cb4a03fbb

- ID: `path--res----godot-imported-intent-heal-16-png-4f34d204d7ee5e97ff9aee7cb4a03fbb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_16.png-4f34d204d7ee5e97ff9aee7cb4a03fbb.ctex"`

### intent heal 17

- ID: `filename----heal-intent-heal-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_17.png",`

### intent heal 17

- ID: `res---images-packed-intents-heal-intent-heal-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_17.png`

### intent heal 17.png

- ID: `images-packed-intents-heal-intent-heal-17-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_17.png.importp`

### intent heal 17.png e2d0206d6d5f9f0045f349464131cd0f

- ID: `godot-imported-intent-heal-17-png-e2d0206d6d5f9f0045f349464131cd0f-ctexp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_17.png-e2d0206d6d5f9f0045f349464131cd0f.ctexp`

### intent heal 17.png e2d0206d6d5f9f0045f349464131cd0f

- ID: `path--res----godot-imported-intent-heal-17-png-e2d0206d6d5f9f0045f349464131cd0f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_17.png-e2d0206d6d5f9f0045f349464131cd0f.ctex"`

### intent heal 18

- ID: `filename----heal-intent-heal-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_18.png",`

### intent heal 18

- ID: `res---images-packed-intents-heal-intent-heal-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_18.png`

### intent heal 18.png

- ID: `images-packed-intents-heal-intent-heal-18-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_18.png.import0`

### intent heal 18.png 95c417aeb73a00777af5d50066b78d08

- ID: `godot-imported-intent-heal-18-png-95c417aeb73a00777af5d50066b78d08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_18.png-95c417aeb73a00777af5d50066b78d08.ctex@`

### intent heal 18.png 95c417aeb73a00777af5d50066b78d08

- ID: `path--res----godot-imported-intent-heal-18-png-95c417aeb73a00777af5d50066b78d08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_18.png-95c417aeb73a00777af5d50066b78d08.ctex"`

### intent heal 19

- ID: `filename----heal-intent-heal-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_19.png",`

### intent heal 19

- ID: `res---images-packed-intents-heal-intent-heal-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_19.png`

### intent heal 19.png

- ID: `images-packed-intents-heal-intent-heal-19-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_19.png.import`

### intent heal 19.png 066567e20fb3a5476390ec5cf191682e

- ID: `godot-imported-intent-heal-19-png-066567e20fb3a5476390ec5cf191682e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_19.png-066567e20fb3a5476390ec5cf191682e.ctex`

### intent heal 19.png 066567e20fb3a5476390ec5cf191682e

- ID: `path--res----godot-imported-intent-heal-19-png-066567e20fb3a5476390ec5cf191682e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_19.png-066567e20fb3a5476390ec5cf191682e.ctex"`

### intent heal 20

- ID: `filename----heal-intent-heal-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_20.png",`

### intent heal 20

- ID: `res---images-packed-intents-heal-intent-heal-20-pngq`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_20.pngQ`

### intent heal 20.png

- ID: `images-packed-intents-heal-intent-heal-20-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_20.png.importP`

### intent heal 20.png e834294d49df296b893899cc7adf5000

- ID: `godot-imported-intent-heal-20-png-e834294d49df296b893899cc7adf5000-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_20.png-e834294d49df296b893899cc7adf5000.ctex`

### intent heal 20.png e834294d49df296b893899cc7adf5000

- ID: `path--res----godot-imported-intent-heal-20-png-e834294d49df296b893899cc7adf5000-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_20.png-e834294d49df296b893899cc7adf5000.ctex"`

### intent heal 21

- ID: `filename----heal-intent-heal-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_21.png",`

### intent heal 21

- ID: `res---images-packed-intents-heal-intent-heal-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_21.png`

### intent heal 21.png

- ID: `images-packed-intents-heal-intent-heal-21-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_21.png.import`

### intent heal 21.png 94eaca179899c3ee763393baeb341872

- ID: `godot-imported-intent-heal-21-png-94eaca179899c3ee763393baeb341872-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_21.png-94eaca179899c3ee763393baeb341872.ctex`

### intent heal 21.png 94eaca179899c3ee763393baeb341872

- ID: `path--res----godot-imported-intent-heal-21-png-94eaca179899c3ee763393baeb341872-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_21.png-94eaca179899c3ee763393baeb341872.ctex"`

### intent heal 22

- ID: `filename----heal-intent-heal-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_22.png",`

### intent heal 22

- ID: `res---images-packed-intents-heal-intent-heal-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_22.png`

### intent heal 22.png

- ID: `images-packed-intents-heal-intent-heal-22-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_22.png.importP+`

### intent heal 22.png 61b3d12a5aa8ffaef4f380a85110b836

- ID: `godot-imported-intent-heal-22-png-61b3d12a5aa8ffaef4f380a85110b836-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_22.png-61b3d12a5aa8ffaef4f380a85110b836.ctex`

### intent heal 22.png 61b3d12a5aa8ffaef4f380a85110b836

- ID: `path--res----godot-imported-intent-heal-22-png-61b3d12a5aa8ffaef4f380a85110b836-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_22.png-61b3d12a5aa8ffaef4f380a85110b836.ctex"`

### intent heal 23

- ID: `filename----heal-intent-heal-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_23.png",`

### intent heal 23

- ID: `res---images-packed-intents-heal-intent-heal-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_23.png`

### intent heal 23.png

- ID: `images-packed-intents-heal-intent-heal-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_23.png.import@<`

### intent heal 23.png 801120fbd366df391cfdf7c8056a9ce4

- ID: `godot-imported-intent-heal-23-png-801120fbd366df391cfdf7c8056a9ce4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_23.png-801120fbd366df391cfdf7c8056a9ce4.ctex ,`

### intent heal 23.png 801120fbd366df391cfdf7c8056a9ce4

- ID: `path--res----godot-imported-intent-heal-23-png-801120fbd366df391cfdf7c8056a9ce4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_23.png-801120fbd366df391cfdf7c8056a9ce4.ctex"`

### intent heal 24

- ID: `filename----heal-intent-heal-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_24.png",`

### intent heal 24

- ID: `res---images-packed-intents-heal-intent-heal-24-pngy`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_24.pngy`

### intent heal 24.png

- ID: `images-packed-intents-heal-intent-heal-24-png-import-l`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_24.png.import@L`

### intent heal 24.png 705ef87f48f4af13151561409fecc474

- ID: `godot-imported-intent-heal-24-png-705ef87f48f4af13151561409fecc474-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_24.png-705ef87f48f4af13151561409fecc474.ctex`

### intent heal 24.png 705ef87f48f4af13151561409fecc474

- ID: `path--res----godot-imported-intent-heal-24-png-705ef87f48f4af13151561409fecc474-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_24.png-705ef87f48f4af13151561409fecc474.ctex"`

### intent heal 25

- ID: `filename----heal-intent-heal-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_25.png",`

### intent heal 25

- ID: `res---images-packed-intents-heal-intent-heal-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_25.png`

### intent heal 25.png

- ID: `images-packed-intents-heal-intent-heal-25-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_25.png.import`

### intent heal 25.png 0f1ac397af0aeadeb4dfc79782781003

- ID: `godot-imported-intent-heal-25-png-0f1ac397af0aeadeb4dfc79782781003-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_25.png-0f1ac397af0aeadeb4dfc79782781003.ctex`

### intent heal 25.png 0f1ac397af0aeadeb4dfc79782781003

- ID: `path--res----godot-imported-intent-heal-25-png-0f1ac397af0aeadeb4dfc79782781003-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_25.png-0f1ac397af0aeadeb4dfc79782781003.ctex"`

### intent heal 26

- ID: `filename----heal-intent-heal-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_26.png",`

### intent heal 26

- ID: `res---images-packed-intents-heal-intent-heal-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_26.png`

### intent heal 26.png

- ID: `images-packed-intents-heal-intent-heal-26-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_26.png.import`

### intent heal 26.png 6db618cef3a8762dae679c7a16835418

- ID: `godot-imported-intent-heal-26-png-6db618cef3a8762dae679c7a16835418-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_26.png-6db618cef3a8762dae679c7a16835418.ctex`

### intent heal 26.png 6db618cef3a8762dae679c7a16835418

- ID: `path--res----godot-imported-intent-heal-26-png-6db618cef3a8762dae679c7a16835418-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_26.png-6db618cef3a8762dae679c7a16835418.ctex"`

### intent heal 27

- ID: `filename----heal-intent-heal-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_27.png",`

### intent heal 27

- ID: `res---images-packed-intents-heal-intent-heal-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_27.png`

### intent heal 27.png

- ID: `images-packed-intents-heal-intent-heal-27-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_27.png.import`

### intent heal 27.png df0ff4171267e24954a6c882c3f6ca25

- ID: `godot-imported-intent-heal-27-png-df0ff4171267e24954a6c882c3f6ca25-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_27.png-df0ff4171267e24954a6c882c3f6ca25.ctex`

### intent heal 27.png df0ff4171267e24954a6c882c3f6ca25

- ID: `path--res----godot-imported-intent-heal-27-png-df0ff4171267e24954a6c882c3f6ca25-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_27.png-df0ff4171267e24954a6c882c3f6ca25.ctex"`

### intent heal 28

- ID: `filename----heal-intent-heal-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_28.png",`

### intent heal 28

- ID: `res---images-packed-intents-heal-intent-heal-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_28.png`

### intent heal 28.png

- ID: `images-packed-intents-heal-intent-heal-28-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_28.png.import`

### intent heal 28.png 91978d9d12b1b2b41cea1bed380d7ffd

- ID: `godot-imported-intent-heal-28-png-91978d9d12b1b2b41cea1bed380d7ffd-ctexpy`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_28.png-91978d9d12b1b2b41cea1bed380d7ffd.ctexPy`

### intent heal 28.png 91978d9d12b1b2b41cea1bed380d7ffd

- ID: `path--res----godot-imported-intent-heal-28-png-91978d9d12b1b2b41cea1bed380d7ffd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_28.png-91978d9d12b1b2b41cea1bed380d7ffd.ctex"`

### intent heal 29

- ID: `filename----heal-intent-heal-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_29.png",`

### intent heal 29

- ID: `res---images-packed-intents-heal-intent-heal-29-pngm`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_29.pngM>`

### intent heal 29.png

- ID: `images-packed-intents-heal-intent-heal-29-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_29.png.import`

### intent heal 29.png 184d2ae12fd2ee67e0f50a53f16fe7cb

- ID: `godot-imported-intent-heal-29-png-184d2ae12fd2ee67e0f50a53f16fe7cb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_29.png-184d2ae12fd2ee67e0f50a53f16fe7cb.ctex`

### intent heal 29.png 184d2ae12fd2ee67e0f50a53f16fe7cb

- ID: `path--res----godot-imported-intent-heal-29-png-184d2ae12fd2ee67e0f50a53f16fe7cb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_29.png-184d2ae12fd2ee67e0f50a53f16fe7cb.ctex"`

### intent heal 30

- ID: `filename----heal-intent-heal-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_30.png",`

### intent heal 30

- ID: `res---images-packed-intents-heal-intent-heal-30-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_30.png`

### intent heal 30.png

- ID: `images-packed-intents-heal-intent-heal-30-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_30.png.import`

### intent heal 30.png 388cf1c3611729c1a83296ecdc2ec45b

- ID: `godot-imported-intent-heal-30-png-388cf1c3611729c1a83296ecdc2ec45b-ctexp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_30.png-388cf1c3611729c1a83296ecdc2ec45b.ctexp`

### intent heal 30.png 388cf1c3611729c1a83296ecdc2ec45b

- ID: `path--res----godot-imported-intent-heal-30-png-388cf1c3611729c1a83296ecdc2ec45b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_30.png-388cf1c3611729c1a83296ecdc2ec45b.ctex"`

### intent heal 31

- ID: `filename----heal-intent-heal-31-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_31.png",`

### intent heal 31

- ID: `res---images-packed-intents-heal-intent-heal-31-pngoj-m`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_31.pngOj(m`

### intent heal 31.png

- ID: `images-packed-intents-heal-intent-heal-31-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_31.png.import`

### intent heal 31.png bb52d7709392efed4499f6d937bc2e8a

- ID: `godot-imported-intent-heal-31-png-bb52d7709392efed4499f6d937bc2e8a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_31.png-bb52d7709392efed4499f6d937bc2e8a.ctex`

### intent heal 31.png bb52d7709392efed4499f6d937bc2e8a

- ID: `path--res----godot-imported-intent-heal-31-png-bb52d7709392efed4499f6d937bc2e8a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_31.png-bb52d7709392efed4499f6d937bc2e8a.ctex"`

### intent heal 32

- ID: `filename----heal-intent-heal-32-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_32.png",`

### intent heal 32

- ID: `res---images-packed-intents-heal-intent-heal-32-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_32.png`

### intent heal 32.png

- ID: `images-packed-intents-heal-intent-heal-32-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_32.png.import`

### intent heal 32.png ea8093362b4ea0b302eb3b1e09e202a5

- ID: `godot-imported-intent-heal-32-png-ea8093362b4ea0b302eb3b1e09e202a5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_32.png-ea8093362b4ea0b302eb3b1e09e202a5.ctex`

### intent heal 32.png ea8093362b4ea0b302eb3b1e09e202a5

- ID: `path--res----godot-imported-intent-heal-32-png-ea8093362b4ea0b302eb3b1e09e202a5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_32.png-ea8093362b4ea0b302eb3b1e09e202a5.ctex"`

### intent heal 33

- ID: `filename----heal-intent-heal-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_33.png",`

### intent heal 33

- ID: `res---images-packed-intents-heal-intent-heal-33-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_33.png`

### intent heal 33.png

- ID: `images-packed-intents-heal-intent-heal-33-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_33.png.import`

### intent heal 33.png 0a58c32b81deff14ec859c5621e927d9

- ID: `godot-imported-intent-heal-33-png-0a58c32b81deff14ec859c5621e927d9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_33.png-0a58c32b81deff14ec859c5621e927d9.ctex`

### intent heal 33.png 0a58c32b81deff14ec859c5621e927d9

- ID: `path--res----godot-imported-intent-heal-33-png-0a58c32b81deff14ec859c5621e927d9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_33.png-0a58c32b81deff14ec859c5621e927d9.ctex"`

### intent heal 34

- ID: `filename----heal-intent-heal-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_34.png",`

### intent heal 34

- ID: `res---images-packed-intents-heal-intent-heal-34-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_34.png`

### intent heal 34.png

- ID: `images-packed-intents-heal-intent-heal-34-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_34.png.import`

### intent heal 34.png b9ce9f3a07066fb970e967112b4eb8b1

- ID: `godot-imported-intent-heal-34-png-b9ce9f3a07066fb970e967112b4eb8b1-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_34.png-b9ce9f3a07066fb970e967112b4eb8b1.ctex`

### intent heal 34.png b9ce9f3a07066fb970e967112b4eb8b1

- ID: `path--res----godot-imported-intent-heal-34-png-b9ce9f3a07066fb970e967112b4eb8b1-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_34.png-b9ce9f3a07066fb970e967112b4eb8b1.ctex"`

### intent heal 35

- ID: `filename----heal-intent-heal-35-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_35.png",`

### intent heal 35

- ID: `res---images-packed-intents-heal-intent-heal-35-pngh`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_35.pngh`

### intent heal 35.png

- ID: `images-packed-intents-heal-intent-heal-35-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_35.png.import`

### intent heal 35.png b05035e254929f2253d4e70bdd65a57b

- ID: `godot-imported-intent-heal-35-png-b05035e254929f2253d4e70bdd65a57b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_35.png-b05035e254929f2253d4e70bdd65a57b.ctex`

### intent heal 35.png b05035e254929f2253d4e70bdd65a57b

- ID: `path--res----godot-imported-intent-heal-35-png-b05035e254929f2253d4e70bdd65a57b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_35.png-b05035e254929f2253d4e70bdd65a57b.ctex"`

### intent heal 36

- ID: `filename----heal-intent-heal-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_36.png",`

### intent heal 36.png

- ID: `images-packed-intents-heal-intent-heal-36-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_36.png.import@`

### intent heal 36.png 4b0334b2d182b98e898379850e3fd480

- ID: `godot-imported-intent-heal-36-png-4b0334b2d182b98e898379850e3fd480-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_36.png-4b0334b2d182b98e898379850e3fd480.ctex`

### intent heal 36.png 4b0334b2d182b98e898379850e3fd480

- ID: `path--res----godot-imported-intent-heal-36-png-4b0334b2d182b98e898379850e3fd480-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_36.png-4b0334b2d182b98e898379850e3fd480.ctex"`

### intent heal 37

- ID: `filename----heal-intent-heal-37-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_37.png",`

### intent heal 37

- ID: `res---images-packed-intents-heal-intent-heal-37-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_37.png`

### intent heal 37.png

- ID: `images-packed-intents-heal-intent-heal-37-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_37.png.import`

### intent heal 37.png f22858d9dc1a62761ff5cb7f4719aa34

- ID: `godot-imported-intent-heal-37-png-f22858d9dc1a62761ff5cb7f4719aa34-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_37.png-f22858d9dc1a62761ff5cb7f4719aa34.ctex`

### intent heal 37.png f22858d9dc1a62761ff5cb7f4719aa34

- ID: `path--res----godot-imported-intent-heal-37-png-f22858d9dc1a62761ff5cb7f4719aa34-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_37.png-f22858d9dc1a62761ff5cb7f4719aa34.ctex"`

### intent heal 38

- ID: `filename----heal-intent-heal-38-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_38.png",`

### intent heal 38

- ID: `res---images-packed-intents-heal-intent-heal-38-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_38.png`

### intent heal 38.png

- ID: `images-packed-intents-heal-intent-heal-38-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_38.png.import`

### intent heal 38.png cc12ed07023886d9330320ea1e1b49d1

- ID: `godot-imported-intent-heal-38-png-cc12ed07023886d9330320ea1e1b49d1-ctexp2`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_38.png-cc12ed07023886d9330320ea1e1b49d1.ctexP2`

### intent heal 38.png cc12ed07023886d9330320ea1e1b49d1

- ID: `path--res----godot-imported-intent-heal-38-png-cc12ed07023886d9330320ea1e1b49d1-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_38.png-cc12ed07023886d9330320ea1e1b49d1.ctex"`

### intent heal 39

- ID: `filename----heal-intent-heal-39-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_39.png",`

### intent heal 39

- ID: `res---images-packed-intents-heal-intent-heal-39-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_39.png`

### intent heal 39.png

- ID: `images-packed-intents-heal-intent-heal-39-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_39.png.import`

### intent heal 39.png f7c30f1bb2fe5e1805c3acf919c712e2

- ID: `godot-imported-intent-heal-39-png-f7c30f1bb2fe5e1805c3acf919c712e2-ctex-e`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_39.png-f7c30f1bb2fe5e1805c3acf919c712e2.ctex`E`

### intent heal 39.png f7c30f1bb2fe5e1805c3acf919c712e2

- ID: `path--res----godot-imported-intent-heal-39-png-f7c30f1bb2fe5e1805c3acf919c712e2-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_39.png-f7c30f1bb2fe5e1805c3acf919c712e2.ctex"`

### intent heal 40

- ID: `filename----heal-intent-heal-40-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_40.png",`

### intent heal 40

- ID: `res---images-packed-intents-heal-intent-heal-40-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_40.png`

### intent heal 40.png

- ID: `images-packed-intents-heal-intent-heal-40-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_40.png.import`

### intent heal 40.png 252aacefb71193feded4f49eecb9bd66

- ID: `godot-imported-intent-heal-40-png-252aacefb71193feded4f49eecb9bd66-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_40.png-252aacefb71193feded4f49eecb9bd66.ctex`

### intent heal 40.png 252aacefb71193feded4f49eecb9bd66

- ID: `path--res----godot-imported-intent-heal-40-png-252aacefb71193feded4f49eecb9bd66-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_40.png-252aacefb71193feded4f49eecb9bd66.ctex"`

### intent heal 41

- ID: `filename----heal-intent-heal-41-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_41.png",`

### intent heal 41

- ID: `res---images-packed-intents-heal-intent-heal-41-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_41.png`

### intent heal 41.png

- ID: `images-packed-intents-heal-intent-heal-41-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_41.png.import`

### intent heal 41.png eb9c8fc221e363c53b694d0b19582fff

- ID: `godot-imported-intent-heal-41-png-eb9c8fc221e363c53b694d0b19582fff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_41.png-eb9c8fc221e363c53b694d0b19582fff.ctex`

### intent heal 41.png eb9c8fc221e363c53b694d0b19582fff

- ID: `path--res----godot-imported-intent-heal-41-png-eb9c8fc221e363c53b694d0b19582fff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_41.png-eb9c8fc221e363c53b694d0b19582fff.ctex"`

### intent heal 42

- ID: `filename----heal-intent-heal-42-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_42.png",`

### intent heal 42

- ID: `res---images-packed-intents-heal-intent-heal-42-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_42.png`

### intent heal 42.png

- ID: `images-packed-intents-heal-intent-heal-42-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_42.png.import``

### intent heal 42.png 1182bb9c3a8257eabe83e3be0bf65d12

- ID: `godot-imported-intent-heal-42-png-1182bb9c3a8257eabe83e3be0bf65d12-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_42.png-1182bb9c3a8257eabe83e3be0bf65d12.ctex`

### intent heal 42.png 1182bb9c3a8257eabe83e3be0bf65d12

- ID: `path--res----godot-imported-intent-heal-42-png-1182bb9c3a8257eabe83e3be0bf65d12-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_42.png-1182bb9c3a8257eabe83e3be0bf65d12.ctex"`

### intent heal 43

- ID: `filename----heal-intent-heal-43-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_43.png",`

### intent heal 43

- ID: `res---images-packed-intents-heal-intent-heal-43-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_43.png`

### intent heal 43.png

- ID: `images-packed-intents-heal-intent-heal-43-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_43.png.import`

### intent heal 43.png 08e6e3acace94b587f8e1f816a6f2919

- ID: `godot-imported-intent-heal-43-png-08e6e3acace94b587f8e1f816a6f2919-ctex0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_43.png-08e6e3acace94b587f8e1f816a6f2919.ctex0`

### intent heal 43.png 08e6e3acace94b587f8e1f816a6f2919

- ID: `path--res----godot-imported-intent-heal-43-png-08e6e3acace94b587f8e1f816a6f2919-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_43.png-08e6e3acace94b587f8e1f816a6f2919.ctex"`

### intent heal 44

- ID: `filename----heal-intent-heal-44-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "heal/intent_heal_44.png",`

### intent heal 44

- ID: `res---images-packed-intents-heal-intent-heal-44-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_44.png`

### intent heal 44.png

- ID: `images-packed-intents-heal-intent-heal-44-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/heal/intent_heal_44.png.import`

### intent heal 44.png bae6ff1f369b169d9641a1b461b93794

- ID: `godot-imported-intent-heal-44-png-bae6ff1f369b169d9641a1b461b93794-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal_44.png-bae6ff1f369b169d9641a1b461b93794.ctex`

### intent heal 44.png bae6ff1f369b169d9641a1b461b93794

- ID: `path--res----godot-imported-intent-heal-44-png-bae6ff1f369b169d9641a1b461b93794-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal_44.png-bae6ff1f369b169d9641a1b461b93794.ctex"`

### intent heal.png

- ID: `images-packed-intents-intent-heal-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_heal.png.import`

### intent heal.png ec05ca102c161b71d91ef8aae48d5647

- ID: `godot-imported-intent-heal-png-ec05ca102c161b71d91ef8aae48d5647-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_heal.png-ec05ca102c161b71d91ef8aae48d5647.ctex`

### intent heal.png ec05ca102c161b71d91ef8aae48d5647

- ID: `path--res----godot-imported-intent-heal-png-ec05ca102c161b71d91ef8aae48d5647-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_heal.png-ec05ca102c161b71d91ef8aae48d5647.ctex"`

### intent hidden

- ID: `res---images-packed-intents-intent-hidden-png6`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_hidden.png6`

### intent hidden.png

- ID: `images-packed-intents-intent-hidden-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_hidden.png.import`

### intent hidden.png 3526fc05f3bc177adc5c8b82cbf3b8fd

- ID: `godot-imported-intent-hidden-png-3526fc05f3bc177adc5c8b82cbf3b8fd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_hidden.png-3526fc05f3bc177adc5c8b82cbf3b8fd.ctex`

### intent hidden.png 3526fc05f3bc177adc5c8b82cbf3b8fd

- ID: `path--res----godot-imported-intent-hidden-png-3526fc05f3bc177adc5c8b82cbf3b8fd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_hidden.png-3526fc05f3bc177adc5c8b82cbf3b8fd.ctex"`

### intent Holder

- ID: `intentholder`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Intent Label Format

- ID: `intentlabelformat`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent megadebuff 00

- ID: `filename----debuff-intent-megadebuff-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_00.png",`

### intent megadebuff 00

- ID: `res---images-packed-intents-debuff-intent-megadebuff-00-pngd`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_00.pngD`

### intent megadebuff 00.png

- ID: `images-packed-intents-debuff-intent-megadebuff-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_00.png.import`

### intent megadebuff 00.png f46b44fa7b700fae85a2825ac9e124f8

- ID: `godot-imported-intent-megadebuff-00-png-f46b44fa7b700fae85a2825ac9e124f8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_00.png-f46b44fa7b700fae85a2825ac9e124f8.ctex`

### intent megadebuff 00.png f46b44fa7b700fae85a2825ac9e124f8

- ID: `path--res----godot-imported-intent-megadebuff-00-png-f46b44fa7b700fae85a2825ac9e124f8-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_00.png-f46b44fa7b700fae85a2825ac9e124f8.ctex"`

### intent megadebuff 01

- ID: `filename----debuff-intent-megadebuff-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_01.png",`

### intent megadebuff 01

- ID: `res---images-packed-intents-debuff-intent-megadebuff-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_01.png``

### intent megadebuff 01.png

- ID: `images-packed-intents-debuff-intent-megadebuff-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_01.png.import`

### intent megadebuff 01.png c544156031edcbffb281599088dd3cbb

- ID: `godot-imported-intent-megadebuff-01-png-c544156031edcbffb281599088dd3cbb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_01.png-c544156031edcbffb281599088dd3cbb.ctex`

### intent megadebuff 01.png c544156031edcbffb281599088dd3cbb

- ID: `path--res----godot-imported-intent-megadebuff-01-png-c544156031edcbffb281599088dd3cbb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_01.png-c544156031edcbffb281599088dd3cbb.ctex"`

### intent megadebuff 02

- ID: `filename----debuff-intent-megadebuff-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_02.png",`

### intent megadebuff 02

- ID: `res---images-packed-intents-debuff-intent-megadebuff-02-pngn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_02.pngN`

### intent megadebuff 02.png

- ID: `images-packed-intents-debuff-intent-megadebuff-02-png-importps`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_02.png.importpS`

### intent megadebuff 02.png 6d3ade3c14b4ad3a3327e460ef84985d

- ID: `godot-imported-intent-megadebuff-02-png-6d3ade3c14b4ad3a3327e460ef84985d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_02.png-6d3ade3c14b4ad3a3327e460ef84985d.ctex`

### intent megadebuff 02.png 6d3ade3c14b4ad3a3327e460ef84985d

- ID: `path--res----godot-imported-intent-megadebuff-02-png-6d3ade3c14b4ad3a3327e460ef84985d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_02.png-6d3ade3c14b4ad3a3327e460ef84985d.ctex"`

### intent megadebuff 03

- ID: `filename----debuff-intent-megadebuff-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_03.png",`

### intent megadebuff 03

- ID: `res---images-packed-intents-debuff-intent-megadebuff-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_03.png`

### intent megadebuff 03.png

- ID: `images-packed-intents-debuff-intent-megadebuff-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_03.png.import`

### intent megadebuff 03.png df35486017bc06bb4e77c4647fe25d4f

- ID: `godot-imported-intent-megadebuff-03-png-df35486017bc06bb4e77c4647fe25d4f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_03.png-df35486017bc06bb4e77c4647fe25d4f.ctex`

### intent megadebuff 03.png df35486017bc06bb4e77c4647fe25d4f

- ID: `path--res----godot-imported-intent-megadebuff-03-png-df35486017bc06bb4e77c4647fe25d4f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_03.png-df35486017bc06bb4e77c4647fe25d4f.ctex"`

### intent megadebuff 04

- ID: `filename----debuff-intent-megadebuff-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_04.png",`

### intent megadebuff 04

- ID: `res---images-packed-intents-debuff-intent-megadebuff-04-pngfcg`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_04.pngfCG`

### intent megadebuff 04.png

- ID: `images-packed-intents-debuff-intent-megadebuff-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_04.png.import@`

### intent megadebuff 04.png a00ef7addffad0023ebbd38d19204da3

- ID: `godot-imported-intent-megadebuff-04-png-a00ef7addffad0023ebbd38d19204da3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_04.png-a00ef7addffad0023ebbd38d19204da3.ctex`

### intent megadebuff 04.png a00ef7addffad0023ebbd38d19204da3

- ID: `path--res----godot-imported-intent-megadebuff-04-png-a00ef7addffad0023ebbd38d19204da3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_04.png-a00ef7addffad0023ebbd38d19204da3.ctex"`

### intent megadebuff 05

- ID: `filename----debuff-intent-megadebuff-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_05.png",`

### intent megadebuff 05

- ID: `res---images-packed-intents-debuff-intent-megadebuff-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_05.png<`

### intent megadebuff 05.png

- ID: `images-packed-intents-debuff-intent-megadebuff-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_05.png.import`

### intent megadebuff 05.png 2decf272123293d8b367dd24da4f9e0b

- ID: `godot-imported-intent-megadebuff-05-png-2decf272123293d8b367dd24da4f9e0b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_05.png-2decf272123293d8b367dd24da4f9e0b.ctex`

### intent megadebuff 05.png 2decf272123293d8b367dd24da4f9e0b

- ID: `path--res----godot-imported-intent-megadebuff-05-png-2decf272123293d8b367dd24da4f9e0b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_05.png-2decf272123293d8b367dd24da4f9e0b.ctex"`

### intent megadebuff 06

- ID: `filename----debuff-intent-megadebuff-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_06.png",`

### intent megadebuff 06

- ID: `res---images-packed-intents-debuff-intent-megadebuff-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_06.png`

### intent megadebuff 06.png

- ID: `images-packed-intents-debuff-intent-megadebuff-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_06.png.import`

### intent megadebuff 06.png 8524efbeda95c2ad417bc62b18d5ad67

- ID: `godot-imported-intent-megadebuff-06-png-8524efbeda95c2ad417bc62b18d5ad67-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_06.png-8524efbeda95c2ad417bc62b18d5ad67.ctex`

### intent megadebuff 06.png 8524efbeda95c2ad417bc62b18d5ad67

- ID: `path--res----godot-imported-intent-megadebuff-06-png-8524efbeda95c2ad417bc62b18d5ad67-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_06.png-8524efbeda95c2ad417bc62b18d5ad67.ctex"`

### intent megadebuff 07

- ID: `filename----debuff-intent-megadebuff-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_07.png",`

### intent megadebuff 07

- ID: `res---images-packed-intents-debuff-intent-megadebuff-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_07.png`

### intent megadebuff 07.png

- ID: `images-packed-intents-debuff-intent-megadebuff-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_07.png.import`

### intent megadebuff 07.png 759b18bf70aa6131d0b7519b5b9a868a

- ID: `godot-imported-intent-megadebuff-07-png-759b18bf70aa6131d0b7519b5b9a868a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_07.png-759b18bf70aa6131d0b7519b5b9a868a.ctex`

### intent megadebuff 07.png 759b18bf70aa6131d0b7519b5b9a868a

- ID: `path--res----godot-imported-intent-megadebuff-07-png-759b18bf70aa6131d0b7519b5b9a868a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_07.png-759b18bf70aa6131d0b7519b5b9a868a.ctex"`

### intent megadebuff 08

- ID: `filename----debuff-intent-megadebuff-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_08.png",`

### intent megadebuff 08

- ID: `res---images-packed-intents-debuff-intent-megadebuff-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_08.png`

### intent megadebuff 08.png

- ID: `images-packed-intents-debuff-intent-megadebuff-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_08.png.import`

### intent megadebuff 08.png f6261563fd2c6f4fa08a86b8f3976062

- ID: `godot-imported-intent-megadebuff-08-png-f6261563fd2c6f4fa08a86b8f3976062-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_08.png-f6261563fd2c6f4fa08a86b8f3976062.ctex`

### intent megadebuff 08.png f6261563fd2c6f4fa08a86b8f3976062

- ID: `path--res----godot-imported-intent-megadebuff-08-png-f6261563fd2c6f4fa08a86b8f3976062-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_08.png-f6261563fd2c6f4fa08a86b8f3976062.ctex"`

### intent megadebuff 09

- ID: `filename----debuff-intent-megadebuff-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_09.png",`

### intent megadebuff 09

- ID: `res---images-packed-intents-debuff-intent-megadebuff-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_09.png`

### intent megadebuff 09.png

- ID: `images-packed-intents-debuff-intent-megadebuff-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_09.png.import`

### intent megadebuff 09.png a2bf387b11d7f3838f94a0ff0f887002

- ID: `godot-imported-intent-megadebuff-09-png-a2bf387b11d7f3838f94a0ff0f887002-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_09.png-a2bf387b11d7f3838f94a0ff0f887002.ctex`

### intent megadebuff 09.png a2bf387b11d7f3838f94a0ff0f887002

- ID: `path--res----godot-imported-intent-megadebuff-09-png-a2bf387b11d7f3838f94a0ff0f887002-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_09.png-a2bf387b11d7f3838f94a0ff0f887002.ctex"`

### intent megadebuff 10

- ID: `filename----debuff-intent-megadebuff-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "debuff/intent_megadebuff_10.png",`

### intent megadebuff 10

- ID: `res---images-packed-intents-debuff-intent-megadebuff-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/debuff/intent_megadebuff_10.png`

### intent megadebuff 10.png

- ID: `images-packed-intents-debuff-intent-megadebuff-10-png-import-r`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/debuff/intent_megadebuff_10.png.import`R`

### intent megadebuff 10.png 438079209ba2f3e7ae3e5e8351b51b47

- ID: `godot-imported-intent-megadebuff-10-png-438079209ba2f3e7ae3e5e8351b51b47-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_megadebuff_10.png-438079209ba2f3e7ae3e5e8351b51b47.ctex`

### intent megadebuff 10.png 438079209ba2f3e7ae3e5e8351b51b47

- ID: `path--res----godot-imported-intent-megadebuff-10-png-438079209ba2f3e7ae3e5e8351b51b47-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_megadebuff_10.png-438079209ba2f3e7ae3e5e8351b51b47.ctex"`

### intent Particle

- ID: `intentparticle`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Intent Position

- ID: `intentposition`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Intent Prefix

- ID: `intentprefix`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent sleep

- ID: `res---images-packed-intents-intent-sleep-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_sleep.png`

### intent sleep 00

- ID: `filename----sleep-intent-sleep-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_00.png",`

### intent sleep 00

- ID: `res---images-packed-intents-sleep-intent-sleep-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_00.png`

### intent sleep 00.png

- ID: `images-packed-intents-sleep-intent-sleep-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_00.png.import`

### intent sleep 00.png 598d227527f4fbd24be6aec197724ff7

- ID: `godot-imported-intent-sleep-00-png-598d227527f4fbd24be6aec197724ff7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_00.png-598d227527f4fbd24be6aec197724ff7.ctex`

### intent sleep 00.png 598d227527f4fbd24be6aec197724ff7

- ID: `path--res----godot-imported-intent-sleep-00-png-598d227527f4fbd24be6aec197724ff7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_00.png-598d227527f4fbd24be6aec197724ff7.ctex"`

### intent sleep 01

- ID: `filename----sleep-intent-sleep-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_01.png",`

### intent sleep 01

- ID: `res---images-packed-intents-sleep-intent-sleep-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_01.png`

### intent sleep 01.png

- ID: `images-packed-intents-sleep-intent-sleep-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_01.png.import`

### intent sleep 01.png cc5e4eaae1ed2a537734cb92f9f5142d

- ID: `godot-imported-intent-sleep-01-png-cc5e4eaae1ed2a537734cb92f9f5142d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_01.png-cc5e4eaae1ed2a537734cb92f9f5142d.ctex`

### intent sleep 01.png cc5e4eaae1ed2a537734cb92f9f5142d

- ID: `path--res----godot-imported-intent-sleep-01-png-cc5e4eaae1ed2a537734cb92f9f5142d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_01.png-cc5e4eaae1ed2a537734cb92f9f5142d.ctex"`

### intent sleep 02

- ID: `filename----sleep-intent-sleep-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_02.png",`

### intent sleep 02

- ID: `res---images-packed-intents-sleep-intent-sleep-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_02.png`

### intent sleep 02.png

- ID: `images-packed-intents-sleep-intent-sleep-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_02.png.import`

### intent sleep 02.png 5bf2fba356d7722654e97edd3be94520

- ID: `godot-imported-intent-sleep-02-png-5bf2fba356d7722654e97edd3be94520-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_02.png-5bf2fba356d7722654e97edd3be94520.ctex`

### intent sleep 02.png 5bf2fba356d7722654e97edd3be94520

- ID: `path--res----godot-imported-intent-sleep-02-png-5bf2fba356d7722654e97edd3be94520-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_02.png-5bf2fba356d7722654e97edd3be94520.ctex"`

### intent sleep 03

- ID: `filename----sleep-intent-sleep-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_03.png",`

### intent sleep 03

- ID: `res---images-packed-intents-sleep-intent-sleep-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_03.png_`

### intent sleep 03.png

- ID: `images-packed-intents-sleep-intent-sleep-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_03.png.import`

### intent sleep 03.png f0083cb0634fee1a0292287a08a43402

- ID: `godot-imported-intent-sleep-03-png-f0083cb0634fee1a0292287a08a43402-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_03.png-f0083cb0634fee1a0292287a08a43402.ctex`

### intent sleep 03.png f0083cb0634fee1a0292287a08a43402

- ID: `path--res----godot-imported-intent-sleep-03-png-f0083cb0634fee1a0292287a08a43402-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_03.png-f0083cb0634fee1a0292287a08a43402.ctex"`

### intent sleep 04

- ID: `filename----sleep-intent-sleep-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_04.png",`

### intent sleep 04

- ID: `res---images-packed-intents-sleep-intent-sleep-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_04.png`

### intent sleep 04.png

- ID: `images-packed-intents-sleep-intent-sleep-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_04.png.import`

### intent sleep 04.png 4321ca2e10950fc0febaa42acb3a1577

- ID: `godot-imported-intent-sleep-04-png-4321ca2e10950fc0febaa42acb3a1577-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_04.png-4321ca2e10950fc0febaa42acb3a1577.ctex`

### intent sleep 04.png 4321ca2e10950fc0febaa42acb3a1577

- ID: `path--res----godot-imported-intent-sleep-04-png-4321ca2e10950fc0febaa42acb3a1577-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_04.png-4321ca2e10950fc0febaa42acb3a1577.ctex"`

### intent sleep 05

- ID: `filename----sleep-intent-sleep-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_05.png",`

### intent sleep 05.png

- ID: `images-packed-intents-sleep-intent-sleep-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_05.png.import`

### intent sleep 05.png 1d839fc101da01b0a1ea259fd25e2bcd

- ID: `godot-imported-intent-sleep-05-png-1d839fc101da01b0a1ea259fd25e2bcd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_05.png-1d839fc101da01b0a1ea259fd25e2bcd.ctex`

### intent sleep 05.png 1d839fc101da01b0a1ea259fd25e2bcd

- ID: `path--res----godot-imported-intent-sleep-05-png-1d839fc101da01b0a1ea259fd25e2bcd-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_05.png-1d839fc101da01b0a1ea259fd25e2bcd.ctex"`

### intent sleep 06

- ID: `filename----sleep-intent-sleep-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_06.png",`

### intent sleep 06

- ID: `res---images-packed-intents-sleep-intent-sleep-06-pngz`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_06.pngz`

### intent sleep 06.png

- ID: `images-packed-intents-sleep-intent-sleep-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_06.png.import`

### intent sleep 06.png 1460d28c8d57f4b82f1b8c25ca7ba99c

- ID: `godot-imported-intent-sleep-06-png-1460d28c8d57f4b82f1b8c25ca7ba99c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_06.png-1460d28c8d57f4b82f1b8c25ca7ba99c.ctex`

### intent sleep 06.png 1460d28c8d57f4b82f1b8c25ca7ba99c

- ID: `path--res----godot-imported-intent-sleep-06-png-1460d28c8d57f4b82f1b8c25ca7ba99c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_06.png-1460d28c8d57f4b82f1b8c25ca7ba99c.ctex"`

### intent sleep 07

- ID: `filename----sleep-intent-sleep-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_07.png",`

### intent sleep 07

- ID: `res---images-packed-intents-sleep-intent-sleep-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_07.png`

### intent sleep 07.png

- ID: `images-packed-intents-sleep-intent-sleep-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_07.png.import`

### intent sleep 07.png e553303bb908c5afc9624c243b4dab48

- ID: `godot-imported-intent-sleep-07-png-e553303bb908c5afc9624c243b4dab48-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_07.png-e553303bb908c5afc9624c243b4dab48.ctex`

### intent sleep 07.png e553303bb908c5afc9624c243b4dab48

- ID: `path--res----godot-imported-intent-sleep-07-png-e553303bb908c5afc9624c243b4dab48-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_07.png-e553303bb908c5afc9624c243b4dab48.ctex"`

### intent sleep 08

- ID: `filename----sleep-intent-sleep-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_08.png",`

### intent sleep 08

- ID: `res---images-packed-intents-sleep-intent-sleep-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_08.png`

### intent sleep 08.png

- ID: `images-packed-intents-sleep-intent-sleep-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_08.png.import`

### intent sleep 08.png 49820d75ab289f79bae4775b82489e58

- ID: `godot-imported-intent-sleep-08-png-49820d75ab289f79bae4775b82489e58-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_08.png-49820d75ab289f79bae4775b82489e58.ctex`

### intent sleep 08.png 49820d75ab289f79bae4775b82489e58

- ID: `path--res----godot-imported-intent-sleep-08-png-49820d75ab289f79bae4775b82489e58-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_08.png-49820d75ab289f79bae4775b82489e58.ctex"`

### intent sleep 09

- ID: `filename----sleep-intent-sleep-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_09.png",`

### intent sleep 09

- ID: `res---images-packed-intents-sleep-intent-sleep-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_09.png`

### intent sleep 09.png

- ID: `images-packed-intents-sleep-intent-sleep-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_09.png.import`

### intent sleep 09.png 8959cf63f9d6f32ed0aa051f0b22f981

- ID: `godot-imported-intent-sleep-09-png-8959cf63f9d6f32ed0aa051f0b22f981-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_09.png-8959cf63f9d6f32ed0aa051f0b22f981.ctex`

### intent sleep 09.png 8959cf63f9d6f32ed0aa051f0b22f981

- ID: `path--res----godot-imported-intent-sleep-09-png-8959cf63f9d6f32ed0aa051f0b22f981-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_09.png-8959cf63f9d6f32ed0aa051f0b22f981.ctex"`

### intent sleep 10

- ID: `filename----sleep-intent-sleep-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_10.png",`

### intent sleep 10

- ID: `res---images-packed-intents-sleep-intent-sleep-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_10.png`

### intent sleep 10.png

- ID: `images-packed-intents-sleep-intent-sleep-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_10.png.import`

### intent sleep 10.png 782e0cc07e8a032f262ce77e5edf8630

- ID: `godot-imported-intent-sleep-10-png-782e0cc07e8a032f262ce77e5edf8630-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_10.png-782e0cc07e8a032f262ce77e5edf8630.ctex`

### intent sleep 10.png 782e0cc07e8a032f262ce77e5edf8630

- ID: `path--res----godot-imported-intent-sleep-10-png-782e0cc07e8a032f262ce77e5edf8630-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_10.png-782e0cc07e8a032f262ce77e5edf8630.ctex"`

### intent sleep 11

- ID: `filename----sleep-intent-sleep-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_11.png",`

### intent sleep 11

- ID: `res---images-packed-intents-sleep-intent-sleep-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_11.png`

### intent sleep 11.png

- ID: `images-packed-intents-sleep-intent-sleep-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_11.png.import`

### intent sleep 11.png f931ce227ee883ac55fc2303d979578c

- ID: `godot-imported-intent-sleep-11-png-f931ce227ee883ac55fc2303d979578c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_11.png-f931ce227ee883ac55fc2303d979578c.ctex`

### intent sleep 11.png f931ce227ee883ac55fc2303d979578c

- ID: `path--res----godot-imported-intent-sleep-11-png-f931ce227ee883ac55fc2303d979578c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_11.png-f931ce227ee883ac55fc2303d979578c.ctex"`

### intent sleep 12

- ID: `filename----sleep-intent-sleep-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_12.png",`

### intent sleep 12

- ID: `res---images-packed-intents-sleep-intent-sleep-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_12.png`

### intent sleep 12.png

- ID: `images-packed-intents-sleep-intent-sleep-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_12.png.import`

### intent sleep 12.png 90b8706f6bb49541f3e804a36144fece

- ID: `godot-imported-intent-sleep-12-png-90b8706f6bb49541f3e804a36144fece-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_12.png-90b8706f6bb49541f3e804a36144fece.ctex`

### intent sleep 12.png 90b8706f6bb49541f3e804a36144fece

- ID: `path--res----godot-imported-intent-sleep-12-png-90b8706f6bb49541f3e804a36144fece-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_12.png-90b8706f6bb49541f3e804a36144fece.ctex"`

### intent sleep 13

- ID: `filename----sleep-intent-sleep-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_13.png",`

### intent sleep 13

- ID: `res---images-packed-intents-sleep-intent-sleep-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_13.png`

### intent sleep 13.png

- ID: `images-packed-intents-sleep-intent-sleep-13-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_13.png.import`

### intent sleep 13.png f7f9d42e110fd4553943e1578daa5c1b

- ID: `godot-imported-intent-sleep-13-png-f7f9d42e110fd4553943e1578daa5c1b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_13.png-f7f9d42e110fd4553943e1578daa5c1b.ctex`

### intent sleep 13.png f7f9d42e110fd4553943e1578daa5c1b

- ID: `path--res----godot-imported-intent-sleep-13-png-f7f9d42e110fd4553943e1578daa5c1b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_13.png-f7f9d42e110fd4553943e1578daa5c1b.ctex"`

### intent sleep 14

- ID: `filename----sleep-intent-sleep-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_14.png",`

### intent sleep 14

- ID: `res---images-packed-intents-sleep-intent-sleep-14-pngl`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_14.pngL%`

### intent sleep 14.png

- ID: `images-packed-intents-sleep-intent-sleep-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_14.png.import`

### intent sleep 14.png 38ac75ed0b2bc5d7b27d3131be87009b

- ID: `godot-imported-intent-sleep-14-png-38ac75ed0b2bc5d7b27d3131be87009b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_14.png-38ac75ed0b2bc5d7b27d3131be87009b.ctex`

### intent sleep 14.png 38ac75ed0b2bc5d7b27d3131be87009b

- ID: `path--res----godot-imported-intent-sleep-14-png-38ac75ed0b2bc5d7b27d3131be87009b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_14.png-38ac75ed0b2bc5d7b27d3131be87009b.ctex"`

### intent sleep 15

- ID: `filename----sleep-intent-sleep-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "sleep/intent_sleep_15.png",`

### intent sleep 15

- ID: `res---images-packed-intents-sleep-intent-sleep-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_15.png`

### intent sleep 15.png

- ID: `images-packed-intents-sleep-intent-sleep-15-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/sleep/intent_sleep_15.png.import`

### intent sleep 15.png 0c26214118d4d2666da7264ba21c3a8c

- ID: `godot-imported-intent-sleep-15-png-0c26214118d4d2666da7264ba21c3a8c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep_15.png-0c26214118d4d2666da7264ba21c3a8c.ctex`

### intent sleep 15.png 0c26214118d4d2666da7264ba21c3a8c

- ID: `path--res----godot-imported-intent-sleep-15-png-0c26214118d4d2666da7264ba21c3a8c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep_15.png-0c26214118d4d2666da7264ba21c3a8c.ctex"`

### intent sleep.png

- ID: `images-packed-intents-intent-sleep-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_sleep.png.import`

### intent sleep.png 5a1839ca2e731f5dd8830ab9e1b012db

- ID: `godot-imported-intent-sleep-png-5a1839ca2e731f5dd8830ab9e1b012db-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_sleep.png-5a1839ca2e731f5dd8830ab9e1b012db.ctex`

### intent sleep.png 5a1839ca2e731f5dd8830ab9e1b012db

- ID: `path--res----godot-imported-intent-sleep-png-5a1839ca2e731f5dd8830ab9e1b012db-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_sleep.png-5a1839ca2e731f5dd8830ab9e1b012db.ctex"`

### intent Sprite

- ID: `intentsprite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intent stun

- ID: `res---images-packed-intents-intent-stun-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_stun.png`

### intent stun.png

- ID: `images-packed-intents-intent-stun-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_stun.png.import`

### intent stun.png 3ae9e499caad29a7bdf4a053db91513a

- ID: `godot-imported-intent-stun-png-3ae9e499caad29a7bdf4a053db91513a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stun.png-3ae9e499caad29a7bdf4a053db91513a.ctex`

### intent stun.png 3ae9e499caad29a7bdf4a053db91513a

- ID: `path--res----godot-imported-intent-stun-png-3ae9e499caad29a7bdf4a053db91513a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stun.png-3ae9e499caad29a7bdf4a053db91513a.ctex"`

### intent stunned 00

- ID: `filename----stun-intent-stunned-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_00.png",`

### intent stunned 00

- ID: `res---images-packed-intents-stun-intent-stunned-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_00.png`

### intent stunned 00.png

- ID: `images-packed-intents-stun-intent-stunned-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_00.png.import`

### intent stunned 00.png c42e47a0c71bf5142e7e8881ba68f847

- ID: `godot-imported-intent-stunned-00-png-c42e47a0c71bf5142e7e8881ba68f847-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_00.png-c42e47a0c71bf5142e7e8881ba68f847.ctex`

### intent stunned 00.png c42e47a0c71bf5142e7e8881ba68f847

- ID: `path--res----godot-imported-intent-stunned-00-png-c42e47a0c71bf5142e7e8881ba68f847-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_00.png-c42e47a0c71bf5142e7e8881ba68f847.ctex"`

### intent stunned 01

- ID: `filename----stun-intent-stunned-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_01.png",`

### intent stunned 01

- ID: `res---images-packed-intents-stun-intent-stunned-01-pngk`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_01.pngk`

### intent stunned 01.png

- ID: `images-packed-intents-stun-intent-stunned-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_01.png.import`

### intent stunned 01.png db2146504f91f0457bc3bf307dae559b

- ID: `godot-imported-intent-stunned-01-png-db2146504f91f0457bc3bf307dae559b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_01.png-db2146504f91f0457bc3bf307dae559b.ctex`

### intent stunned 01.png db2146504f91f0457bc3bf307dae559b

- ID: `path--res----godot-imported-intent-stunned-01-png-db2146504f91f0457bc3bf307dae559b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_01.png-db2146504f91f0457bc3bf307dae559b.ctex"`

### intent stunned 02

- ID: `filename----stun-intent-stunned-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_02.png",`

### intent stunned 02

- ID: `res---images-packed-intents-stun-intent-stunned-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_02.png`

### intent stunned 02.png

- ID: `images-packed-intents-stun-intent-stunned-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_02.png.import`

### intent stunned 02.png 258e749bc41206199779198ceb3f876b

- ID: `godot-imported-intent-stunned-02-png-258e749bc41206199779198ceb3f876b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_02.png-258e749bc41206199779198ceb3f876b.ctex`

### intent stunned 02.png 258e749bc41206199779198ceb3f876b

- ID: `path--res----godot-imported-intent-stunned-02-png-258e749bc41206199779198ceb3f876b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_02.png-258e749bc41206199779198ceb3f876b.ctex"`

### intent stunned 03

- ID: `filename----stun-intent-stunned-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_03.png",`

### intent stunned 03

- ID: `res---images-packed-intents-stun-intent-stunned-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_03.png`

### intent stunned 03.png

- ID: `images-packed-intents-stun-intent-stunned-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_03.png.import`

### intent stunned 03.png ae6d3e53cd2479277c127973ac98cbe0

- ID: `godot-imported-intent-stunned-03-png-ae6d3e53cd2479277c127973ac98cbe0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_03.png-ae6d3e53cd2479277c127973ac98cbe0.ctex`

### intent stunned 03.png ae6d3e53cd2479277c127973ac98cbe0

- ID: `path--res----godot-imported-intent-stunned-03-png-ae6d3e53cd2479277c127973ac98cbe0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_03.png-ae6d3e53cd2479277c127973ac98cbe0.ctex"`

### intent stunned 04

- ID: `filename----stun-intent-stunned-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_04.png",`

### intent stunned 04

- ID: `res---images-packed-intents-stun-intent-stunned-04-pngb`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_04.pngb`

### intent stunned 04.png

- ID: `images-packed-intents-stun-intent-stunned-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_04.png.import`

### intent stunned 04.png 282b638f663c2161022e4450eb280cef

- ID: `godot-imported-intent-stunned-04-png-282b638f663c2161022e4450eb280cef-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_04.png-282b638f663c2161022e4450eb280cef.ctex`

### intent stunned 04.png 282b638f663c2161022e4450eb280cef

- ID: `path--res----godot-imported-intent-stunned-04-png-282b638f663c2161022e4450eb280cef-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_04.png-282b638f663c2161022e4450eb280cef.ctex"`

### intent stunned 05

- ID: `filename----stun-intent-stunned-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_05.png",`

### intent stunned 05

- ID: `res---images-packed-intents-stun-intent-stunned-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_05.png`

### intent stunned 05.png

- ID: `images-packed-intents-stun-intent-stunned-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_05.png.import`

### intent stunned 05.png cee478b45278a71528cd06edc26b8989

- ID: `godot-imported-intent-stunned-05-png-cee478b45278a71528cd06edc26b8989-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_05.png-cee478b45278a71528cd06edc26b8989.ctex`

### intent stunned 05.png cee478b45278a71528cd06edc26b8989

- ID: `path--res----godot-imported-intent-stunned-05-png-cee478b45278a71528cd06edc26b8989-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_05.png-cee478b45278a71528cd06edc26b8989.ctex"`

### intent stunned 06

- ID: `filename----stun-intent-stunned-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_06.png",`

### intent stunned 06

- ID: `res---images-packed-intents-stun-intent-stunned-06-pngx`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_06.pngx=:@`

### intent stunned 06.png

- ID: `images-packed-intents-stun-intent-stunned-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_06.png.import`

### intent stunned 06.png d3c23f0d89f71c3dca0b1dbebbf08147

- ID: `godot-imported-intent-stunned-06-png-d3c23f0d89f71c3dca0b1dbebbf08147-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_06.png-d3c23f0d89f71c3dca0b1dbebbf08147.ctex`

### intent stunned 06.png d3c23f0d89f71c3dca0b1dbebbf08147

- ID: `path--res----godot-imported-intent-stunned-06-png-d3c23f0d89f71c3dca0b1dbebbf08147-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_06.png-d3c23f0d89f71c3dca0b1dbebbf08147.ctex"`

### intent stunned 07

- ID: `filename----stun-intent-stunned-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_07.png",`

### intent stunned 07

- ID: `res---images-packed-intents-stun-intent-stunned-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_07.png`

### intent stunned 07.png

- ID: `images-packed-intents-stun-intent-stunned-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_07.png.import`

### intent stunned 07.png 2e7c0edaf77dd6da599ffefffa21d70f

- ID: `godot-imported-intent-stunned-07-png-2e7c0edaf77dd6da599ffefffa21d70f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_07.png-2e7c0edaf77dd6da599ffefffa21d70f.ctex`

### intent stunned 07.png 2e7c0edaf77dd6da599ffefffa21d70f

- ID: `path--res----godot-imported-intent-stunned-07-png-2e7c0edaf77dd6da599ffefffa21d70f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_07.png-2e7c0edaf77dd6da599ffefffa21d70f.ctex"`

### intent stunned 08

- ID: `filename----stun-intent-stunned-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_08.png",`

### intent stunned 08

- ID: `res---images-packed-intents-stun-intent-stunned-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_08.png~+`

### intent stunned 08.png

- ID: `images-packed-intents-stun-intent-stunned-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_08.png.import`

### intent stunned 08.png 0dea87575c391b9eac4085043177e6b5

- ID: `godot-imported-intent-stunned-08-png-0dea87575c391b9eac4085043177e6b5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_08.png-0dea87575c391b9eac4085043177e6b5.ctex`

### intent stunned 08.png 0dea87575c391b9eac4085043177e6b5

- ID: `path--res----godot-imported-intent-stunned-08-png-0dea87575c391b9eac4085043177e6b5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_08.png-0dea87575c391b9eac4085043177e6b5.ctex"`

### intent stunned 09

- ID: `filename----stun-intent-stunned-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_09.png",`

### intent stunned 09

- ID: `res---images-packed-intents-stun-intent-stunned-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_09.png'`

### intent stunned 09.png

- ID: `images-packed-intents-stun-intent-stunned-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_09.png.import`

### intent stunned 09.png e28916f54ee0dd25d249c13387169c46

- ID: `godot-imported-intent-stunned-09-png-e28916f54ee0dd25d249c13387169c46-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_09.png-e28916f54ee0dd25d249c13387169c46.ctex`

### intent stunned 09.png e28916f54ee0dd25d249c13387169c46

- ID: `path--res----godot-imported-intent-stunned-09-png-e28916f54ee0dd25d249c13387169c46-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_09.png-e28916f54ee0dd25d249c13387169c46.ctex"`

### intent stunned 10

- ID: `filename----stun-intent-stunned-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_10.png",`

### intent stunned 10

- ID: `res---images-packed-intents-stun-intent-stunned-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_10.png`

### intent stunned 10.png

- ID: `images-packed-intents-stun-intent-stunned-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_10.png.import`

### intent stunned 10.png 9cbc865636197d22e3527d1be39a053b

- ID: `godot-imported-intent-stunned-10-png-9cbc865636197d22e3527d1be39a053b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_10.png-9cbc865636197d22e3527d1be39a053b.ctex`

### intent stunned 10.png 9cbc865636197d22e3527d1be39a053b

- ID: `path--res----godot-imported-intent-stunned-10-png-9cbc865636197d22e3527d1be39a053b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_10.png-9cbc865636197d22e3527d1be39a053b.ctex"`

### intent stunned 11

- ID: `filename----stun-intent-stunned-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_11.png",`

### intent stunned 11

- ID: `res---images-packed-intents-stun-intent-stunned-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_11.png`

### intent stunned 11.png

- ID: `images-packed-intents-stun-intent-stunned-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_11.png.import`

### intent stunned 11.png e357d12d5401a6a231fe96e05b92f1c0

- ID: `godot-imported-intent-stunned-11-png-e357d12d5401a6a231fe96e05b92f1c0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_11.png-e357d12d5401a6a231fe96e05b92f1c0.ctex`

### intent stunned 11.png e357d12d5401a6a231fe96e05b92f1c0

- ID: `path--res----godot-imported-intent-stunned-11-png-e357d12d5401a6a231fe96e05b92f1c0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_11.png-e357d12d5401a6a231fe96e05b92f1c0.ctex"`

### intent stunned 12

- ID: `filename----stun-intent-stunned-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_12.png",`

### intent stunned 12

- ID: `res---images-packed-intents-stun-intent-stunned-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_12.png`

### intent stunned 12.png

- ID: `images-packed-intents-stun-intent-stunned-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_12.png.import`

### intent stunned 12.png 4ab46975bd224a628b0c4b96fa2b6acf

- ID: `godot-imported-intent-stunned-12-png-4ab46975bd224a628b0c4b96fa2b6acf-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_12.png-4ab46975bd224a628b0c4b96fa2b6acf.ctex`

### intent stunned 12.png 4ab46975bd224a628b0c4b96fa2b6acf

- ID: `path--res----godot-imported-intent-stunned-12-png-4ab46975bd224a628b0c4b96fa2b6acf-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_12.png-4ab46975bd224a628b0c4b96fa2b6acf.ctex"`

### intent stunned 13

- ID: `filename----stun-intent-stunned-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_13.png",`

### intent stunned 13

- ID: `res---images-packed-intents-stun-intent-stunned-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_13.png`

### intent stunned 13.png

- ID: `images-packed-intents-stun-intent-stunned-13-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_13.png.import`

### intent stunned 13.png 462d1c08a68d8cedb0515241de8f87db

- ID: `godot-imported-intent-stunned-13-png-462d1c08a68d8cedb0515241de8f87db-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_13.png-462d1c08a68d8cedb0515241de8f87db.ctex`

### intent stunned 13.png 462d1c08a68d8cedb0515241de8f87db

- ID: `path--res----godot-imported-intent-stunned-13-png-462d1c08a68d8cedb0515241de8f87db-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_13.png-462d1c08a68d8cedb0515241de8f87db.ctex"`

### intent stunned 14

- ID: `filename----stun-intent-stunned-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_14.png",`

### intent stunned 14

- ID: `res---images-packed-intents-stun-intent-stunned-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_14.png`

### intent stunned 14.png

- ID: `images-packed-intents-stun-intent-stunned-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_14.png.import`

### intent stunned 14.png 5bc77fdaa609e25ece1c635c4b9c1fde

- ID: `godot-imported-intent-stunned-14-png-5bc77fdaa609e25ece1c635c4b9c1fde-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_14.png-5bc77fdaa609e25ece1c635c4b9c1fde.ctex`

### intent stunned 14.png 5bc77fdaa609e25ece1c635c4b9c1fde

- ID: `path--res----godot-imported-intent-stunned-14-png-5bc77fdaa609e25ece1c635c4b9c1fde-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_14.png-5bc77fdaa609e25ece1c635c4b9c1fde.ctex"`

### intent stunned 15

- ID: `filename----stun-intent-stunned-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "stun/intent_stunned_15.png",`

### intent stunned 15

- ID: `res---images-packed-intents-stun-intent-stunned-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/stun/intent_stunned_15.png`

### intent stunned 15.png

- ID: `images-packed-intents-stun-intent-stunned-15-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/stun/intent_stunned_15.png.import`

### intent stunned 15.png 37da2e22614175019d61d5b15a012fa3

- ID: `godot-imported-intent-stunned-15-png-37da2e22614175019d61d5b15a012fa3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_stunned_15.png-37da2e22614175019d61d5b15a012fa3.ctex`

### intent stunned 15.png 37da2e22614175019d61d5b15a012fa3

- ID: `path--res----godot-imported-intent-stunned-15-png-37da2e22614175019d61d5b15a012fa3-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_stunned_15.png-37da2e22614175019d61d5b15a012fa3.ctex"`

### intent summon

- ID: `res---images-packed-intents-intent-summon-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_summon.png`

### intent summon 00

- ID: `filename----summon-intent-summon-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_00.png",`

### intent summon 00

- ID: `res---images-packed-intents-summon-intent-summon-00-png3`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_00.png3`

### intent summon 00.png

- ID: `images-packed-intents-summon-intent-summon-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_00.png.import`

### intent summon 00.png f03815539095b2ff72cd245531523857

- ID: `godot-imported-intent-summon-00-png-f03815539095b2ff72cd245531523857-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_00.png-f03815539095b2ff72cd245531523857.ctex`

### intent summon 00.png f03815539095b2ff72cd245531523857

- ID: `path--res----godot-imported-intent-summon-00-png-f03815539095b2ff72cd245531523857-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_00.png-f03815539095b2ff72cd245531523857.ctex"`

### intent summon 01

- ID: `filename----summon-intent-summon-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_01.png",`

### intent summon 01

- ID: `res---images-packed-intents-summon-intent-summon-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_01.png`

### intent summon 01.png

- ID: `images-packed-intents-summon-intent-summon-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_01.png.import`

### intent summon 01.png f01b75b99b8b935254e649cad006ba9f

- ID: `godot-imported-intent-summon-01-png-f01b75b99b8b935254e649cad006ba9f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_01.png-f01b75b99b8b935254e649cad006ba9f.ctex`

### intent summon 01.png f01b75b99b8b935254e649cad006ba9f

- ID: `path--res----godot-imported-intent-summon-01-png-f01b75b99b8b935254e649cad006ba9f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_01.png-f01b75b99b8b935254e649cad006ba9f.ctex"`

### intent summon 02

- ID: `filename----summon-intent-summon-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_02.png",`

### intent summon 02

- ID: `res---images-packed-intents-summon-intent-summon-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_02.png`

### intent summon 02.png

- ID: `images-packed-intents-summon-intent-summon-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_02.png.import`

### intent summon 02.png c218a0286f1211d5b637af6b20adff37

- ID: `godot-imported-intent-summon-02-png-c218a0286f1211d5b637af6b20adff37-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_02.png-c218a0286f1211d5b637af6b20adff37.ctex`

### intent summon 02.png c218a0286f1211d5b637af6b20adff37

- ID: `path--res----godot-imported-intent-summon-02-png-c218a0286f1211d5b637af6b20adff37-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_02.png-c218a0286f1211d5b637af6b20adff37.ctex"`

### intent summon 03

- ID: `filename----summon-intent-summon-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_03.png",`

### intent summon 03

- ID: `res---images-packed-intents-summon-intent-summon-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_03.png=`

### intent summon 03.png

- ID: `images-packed-intents-summon-intent-summon-03-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_03.png.import0`

### intent summon 03.png e34351c35ca42995a4eb26d26f95de84

- ID: `godot-imported-intent-summon-03-png-e34351c35ca42995a4eb26d26f95de84-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_03.png-e34351c35ca42995a4eb26d26f95de84.ctex`

### intent summon 03.png e34351c35ca42995a4eb26d26f95de84

- ID: `path--res----godot-imported-intent-summon-03-png-e34351c35ca42995a4eb26d26f95de84-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_03.png-e34351c35ca42995a4eb26d26f95de84.ctex"`

### intent summon 04

- ID: `filename----summon-intent-summon-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_04.png",`

### intent summon 04

- ID: `res---images-packed-intents-summon-intent-summon-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_04.png`

### intent summon 04.png

- ID: `images-packed-intents-summon-intent-summon-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_04.png.import`

### intent summon 04.png e78ad6b422dcfb252e53287cd8cf8e36

- ID: `godot-imported-intent-summon-04-png-e78ad6b422dcfb252e53287cd8cf8e36-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_04.png-e78ad6b422dcfb252e53287cd8cf8e36.ctex`

### intent summon 04.png e78ad6b422dcfb252e53287cd8cf8e36

- ID: `path--res----godot-imported-intent-summon-04-png-e78ad6b422dcfb252e53287cd8cf8e36-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_04.png-e78ad6b422dcfb252e53287cd8cf8e36.ctex"`

### intent summon 05

- ID: `filename----summon-intent-summon-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_05.png",`

### intent summon 05

- ID: `res---images-packed-intents-summon-intent-summon-05-pnga`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_05.pnga`

### intent summon 05.png

- ID: `images-packed-intents-summon-intent-summon-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_05.png.import`

### intent summon 05.png 13c22d315666cb94a9cffa6d559cbf18

- ID: `godot-imported-intent-summon-05-png-13c22d315666cb94a9cffa6d559cbf18-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_05.png-13c22d315666cb94a9cffa6d559cbf18.ctex`

### intent summon 05.png 13c22d315666cb94a9cffa6d559cbf18

- ID: `path--res----godot-imported-intent-summon-05-png-13c22d315666cb94a9cffa6d559cbf18-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_05.png-13c22d315666cb94a9cffa6d559cbf18.ctex"`

### intent summon 06

- ID: `filename----summon-intent-summon-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_06.png",`

### intent summon 06

- ID: `res---images-packed-intents-summon-intent-summon-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_06.png`

### intent summon 06.png

- ID: `images-packed-intents-summon-intent-summon-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_06.png.import`

### intent summon 06.png 085dfb27b1b8eea7268b11456b58513d

- ID: `godot-imported-intent-summon-06-png-085dfb27b1b8eea7268b11456b58513d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_06.png-085dfb27b1b8eea7268b11456b58513d.ctex`

### intent summon 06.png 085dfb27b1b8eea7268b11456b58513d

- ID: `path--res----godot-imported-intent-summon-06-png-085dfb27b1b8eea7268b11456b58513d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_06.png-085dfb27b1b8eea7268b11456b58513d.ctex"`

### intent summon 07

- ID: `filename----summon-intent-summon-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_07.png",`

### intent summon 07

- ID: `res---images-packed-intents-summon-intent-summon-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_07.png`

### intent summon 07.png 9975e2c77a3e0be231b5539b167ba4e6

- ID: `godot-imported-intent-summon-07-png-9975e2c77a3e0be231b5539b167ba4e6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_07.png-9975e2c77a3e0be231b5539b167ba4e6.ctex`

### intent summon 07.png 9975e2c77a3e0be231b5539b167ba4e6

- ID: `path--res----godot-imported-intent-summon-07-png-9975e2c77a3e0be231b5539b167ba4e6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_07.png-9975e2c77a3e0be231b5539b167ba4e6.ctex"`

### intent summon 08

- ID: `filename----summon-intent-summon-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_08.png",`

### intent summon 08

- ID: `res---images-packed-intents-summon-intent-summon-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_08.png`

### intent summon 08.png

- ID: `images-packed-intents-summon-intent-summon-08-png-importpq`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_08.png.importPQ`

### intent summon 08.png e41b9a06af799bba8bf7c7c682fd35eb

- ID: `godot-imported-intent-summon-08-png-e41b9a06af799bba8bf7c7c682fd35eb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_08.png-e41b9a06af799bba8bf7c7c682fd35eb.ctex`

### intent summon 08.png e41b9a06af799bba8bf7c7c682fd35eb

- ID: `path--res----godot-imported-intent-summon-08-png-e41b9a06af799bba8bf7c7c682fd35eb-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_08.png-e41b9a06af799bba8bf7c7c682fd35eb.ctex"`

### intent summon 09

- ID: `filename----summon-intent-summon-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_09.png",`

### intent summon 09

- ID: `res---images-packed-intents-summon-intent-summon-09-pnga`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_09.pngA`

### intent summon 09.png

- ID: `images-packed-intents-summon-intent-summon-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_09.png.import`

### intent summon 09.png 2e5d659fb4f54d4320b233890dca450f

- ID: `godot-imported-intent-summon-09-png-2e5d659fb4f54d4320b233890dca450f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_09.png-2e5d659fb4f54d4320b233890dca450f.ctex`

### intent summon 09.png 2e5d659fb4f54d4320b233890dca450f

- ID: `path--res----godot-imported-intent-summon-09-png-2e5d659fb4f54d4320b233890dca450f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_09.png-2e5d659fb4f54d4320b233890dca450f.ctex"`

### intent summon 10

- ID: `filename----summon-intent-summon-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_10.png",`

### intent summon 10

- ID: `res---images-packed-intents-summon-intent-summon-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_10.png`

### intent summon 10.png

- ID: `images-packed-intents-summon-intent-summon-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_10.png.import`

### intent summon 10.png dd79bd6d616ce32ad83660bd8f13998c

- ID: `godot-imported-intent-summon-10-png-dd79bd6d616ce32ad83660bd8f13998c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_10.png-dd79bd6d616ce32ad83660bd8f13998c.ctex`

### intent summon 10.png dd79bd6d616ce32ad83660bd8f13998c

- ID: `path--res----godot-imported-intent-summon-10-png-dd79bd6d616ce32ad83660bd8f13998c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_10.png-dd79bd6d616ce32ad83660bd8f13998c.ctex"`

### intent summon 11

- ID: `filename----summon-intent-summon-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_11.png",`

### intent summon 11

- ID: `res---images-packed-intents-summon-intent-summon-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_11.png`

### intent summon 11.png

- ID: `images-packed-intents-summon-intent-summon-11-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_11.png.importp`

### intent summon 11.png 3645799c17045a32a25d9027d7dbebb0

- ID: `godot-imported-intent-summon-11-png-3645799c17045a32a25d9027d7dbebb0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_11.png-3645799c17045a32a25d9027d7dbebb0.ctex`

### intent summon 11.png 3645799c17045a32a25d9027d7dbebb0

- ID: `path--res----godot-imported-intent-summon-11-png-3645799c17045a32a25d9027d7dbebb0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_11.png-3645799c17045a32a25d9027d7dbebb0.ctex"`

### intent summon 12

- ID: `filename----summon-intent-summon-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_12.png",`

### intent summon 12

- ID: `res---images-packed-intents-summon-intent-summon-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_12.png`

### intent summon 12.png

- ID: `images-packed-intents-summon-intent-summon-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_12.png.import`

### intent summon 12.png be06801b6d78abeb77bf4fe9424c1d6e

- ID: `godot-imported-intent-summon-12-png-be06801b6d78abeb77bf4fe9424c1d6e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_12.png-be06801b6d78abeb77bf4fe9424c1d6e.ctex`

### intent summon 12.png be06801b6d78abeb77bf4fe9424c1d6e

- ID: `path--res----godot-imported-intent-summon-12-png-be06801b6d78abeb77bf4fe9424c1d6e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_12.png-be06801b6d78abeb77bf4fe9424c1d6e.ctex"`

### intent summon 13

- ID: `filename----summon-intent-summon-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_13.png",`

### intent summon 13

- ID: `res---images-packed-intents-summon-intent-summon-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_13.png-`

### intent summon 13.png

- ID: `images-packed-intents-summon-intent-summon-13-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_13.png.import0`

### intent summon 13.png 7329012f7638baa938da47c115dabc40

- ID: `godot-imported-intent-summon-13-png-7329012f7638baa938da47c115dabc40-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_13.png-7329012f7638baa938da47c115dabc40.ctex`

### intent summon 13.png 7329012f7638baa938da47c115dabc40

- ID: `path--res----godot-imported-intent-summon-13-png-7329012f7638baa938da47c115dabc40-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_13.png-7329012f7638baa938da47c115dabc40.ctex"`

### intent summon 14

- ID: `filename----summon-intent-summon-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_14.png",`

### intent summon 14

- ID: `res---images-packed-intents-summon-intent-summon-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_14.png!`

### intent summon 14.png

- ID: `images-packed-intents-summon-intent-summon-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_14.png.import`

### intent summon 14.png 2128d7135f598c2ac364ca7a3700fffa

- ID: `godot-imported-intent-summon-14-png-2128d7135f598c2ac364ca7a3700fffa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_14.png-2128d7135f598c2ac364ca7a3700fffa.ctex`

### intent summon 14.png 2128d7135f598c2ac364ca7a3700fffa

- ID: `path--res----godot-imported-intent-summon-14-png-2128d7135f598c2ac364ca7a3700fffa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_14.png-2128d7135f598c2ac364ca7a3700fffa.ctex"`

### intent summon 15

- ID: `filename----summon-intent-summon-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_15.png",`

### intent summon 15

- ID: `res---images-packed-intents-summon-intent-summon-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_15.png`

### intent summon 15.png

- ID: `images-packed-intents-summon-intent-summon-15-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_15.png.importP)`

### intent summon 15.png 9e63048b299614e3fcb6f397bfb1390e

- ID: `godot-imported-intent-summon-15-png-9e63048b299614e3fcb6f397bfb1390e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_15.png-9e63048b299614e3fcb6f397bfb1390e.ctex`

### intent summon 15.png 9e63048b299614e3fcb6f397bfb1390e

- ID: `path--res----godot-imported-intent-summon-15-png-9e63048b299614e3fcb6f397bfb1390e-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_15.png-9e63048b299614e3fcb6f397bfb1390e.ctex"`

### intent summon 16

- ID: `filename----summon-intent-summon-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_16.png",`

### intent summon 16

- ID: `res---images-packed-intents-summon-intent-summon-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_16.png`

### intent summon 16.png

- ID: `images-packed-intents-summon-intent-summon-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_16.png.import`

### intent summon 16.png 84397b63ca09d51b3bc35680d22c43ba

- ID: `godot-imported-intent-summon-16-png-84397b63ca09d51b3bc35680d22c43ba-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_16.png-84397b63ca09d51b3bc35680d22c43ba.ctex`

### intent summon 16.png 84397b63ca09d51b3bc35680d22c43ba

- ID: `path--res----godot-imported-intent-summon-16-png-84397b63ca09d51b3bc35680d22c43ba-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_16.png-84397b63ca09d51b3bc35680d22c43ba.ctex"`

### intent summon 17

- ID: `filename----summon-intent-summon-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_17.png",`

### intent summon 17

- ID: `res---images-packed-intents-summon-intent-summon-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_17.png`

### intent summon 17.png

- ID: `images-packed-intents-summon-intent-summon-17-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_17.png.import`

### intent summon 17.png f2c2cec42f6ea4f0de7ac1694d5fcf3a

- ID: `godot-imported-intent-summon-17-png-f2c2cec42f6ea4f0de7ac1694d5fcf3a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_17.png-f2c2cec42f6ea4f0de7ac1694d5fcf3a.ctex`

### intent summon 17.png f2c2cec42f6ea4f0de7ac1694d5fcf3a

- ID: `path--res----godot-imported-intent-summon-17-png-f2c2cec42f6ea4f0de7ac1694d5fcf3a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_17.png-f2c2cec42f6ea4f0de7ac1694d5fcf3a.ctex"`

### intent summon 18

- ID: `filename----summon-intent-summon-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_18.png",`

### intent summon 18

- ID: `res---images-packed-intents-summon-intent-summon-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_18.png`

### intent summon 18.png

- ID: `images-packed-intents-summon-intent-summon-18-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_18.png.import`

### intent summon 18.png 2938cbef748c8bb7fef5cf0efc5a8f4b

- ID: `godot-imported-intent-summon-18-png-2938cbef748c8bb7fef5cf0efc5a8f4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_18.png-2938cbef748c8bb7fef5cf0efc5a8f4b.ctex`

### intent summon 18.png 2938cbef748c8bb7fef5cf0efc5a8f4b

- ID: `path--res----godot-imported-intent-summon-18-png-2938cbef748c8bb7fef5cf0efc5a8f4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_18.png-2938cbef748c8bb7fef5cf0efc5a8f4b.ctex"`

### intent summon 19

- ID: `filename----summon-intent-summon-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_19.png",`

### intent summon 19

- ID: `res---images-packed-intents-summon-intent-summon-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_19.png`

### intent summon 19.png

- ID: `images-packed-intents-summon-intent-summon-19-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_19.png.import0`

### intent summon 19.png e54591d5fcbfe4ed95c5e142eadb199b

- ID: `godot-imported-intent-summon-19-png-e54591d5fcbfe4ed95c5e142eadb199b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_19.png-e54591d5fcbfe4ed95c5e142eadb199b.ctex`

### intent summon 19.png e54591d5fcbfe4ed95c5e142eadb199b

- ID: `path--res----godot-imported-intent-summon-19-png-e54591d5fcbfe4ed95c5e142eadb199b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_19.png-e54591d5fcbfe4ed95c5e142eadb199b.ctex"`

### intent summon 20

- ID: `filename----summon-intent-summon-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_20.png",`

### intent summon 20

- ID: `res---images-packed-intents-summon-intent-summon-20-pngv`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_20.pngv`

### intent summon 20.png

- ID: `images-packed-intents-summon-intent-summon-20-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_20.png.importp`

### intent summon 20.png 870b8632ae27d4d398a2eb4dae946c4b

- ID: `godot-imported-intent-summon-20-png-870b8632ae27d4d398a2eb4dae946c4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_20.png-870b8632ae27d4d398a2eb4dae946c4b.ctex`

### intent summon 20.png 870b8632ae27d4d398a2eb4dae946c4b

- ID: `path--res----godot-imported-intent-summon-20-png-870b8632ae27d4d398a2eb4dae946c4b-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_20.png-870b8632ae27d4d398a2eb4dae946c4b.ctex"`

### intent summon 21

- ID: `filename----summon-intent-summon-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_21.png",`

### intent summon 21

- ID: `res---images-packed-intents-summon-intent-summon-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_21.png`

### intent summon 21.png

- ID: `images-packed-intents-summon-intent-summon-21-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_21.png.importp`

### intent summon 21.png 0f967dc357724b0b61a71ee2d0ad26e5

- ID: `godot-imported-intent-summon-21-png-0f967dc357724b0b61a71ee2d0ad26e5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_21.png-0f967dc357724b0b61a71ee2d0ad26e5.ctex`

### intent summon 21.png 0f967dc357724b0b61a71ee2d0ad26e5

- ID: `path--res----godot-imported-intent-summon-21-png-0f967dc357724b0b61a71ee2d0ad26e5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_21.png-0f967dc357724b0b61a71ee2d0ad26e5.ctex"`

### intent summon 22

- ID: `filename----summon-intent-summon-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_22.png",`

### intent summon 22

- ID: `res---images-packed-intents-summon-intent-summon-22-pngko`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_22.pngKo`

### intent summon 22.png

- ID: `images-packed-intents-summon-intent-summon-22-png-importp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_22.png.importP`

### intent summon 22.png 54dc6057f42acd4a243b04f23d08b236

- ID: `godot-imported-intent-summon-22-png-54dc6057f42acd4a243b04f23d08b236-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_22.png-54dc6057f42acd4a243b04f23d08b236.ctex`

### intent summon 22.png 54dc6057f42acd4a243b04f23d08b236

- ID: `path--res----godot-imported-intent-summon-22-png-54dc6057f42acd4a243b04f23d08b236-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_22.png-54dc6057f42acd4a243b04f23d08b236.ctex"`

### intent summon 23

- ID: `filename----summon-intent-summon-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_23.png",`

### intent summon 23

- ID: `res---images-packed-intents-summon-intent-summon-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_23.png`

### intent summon 23.png

- ID: `images-packed-intents-summon-intent-summon-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_23.png.import`

### intent summon 23.png 5e9dc947b39352715829cb2190714bf6

- ID: `godot-imported-intent-summon-23-png-5e9dc947b39352715829cb2190714bf6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_23.png-5e9dc947b39352715829cb2190714bf6.ctex`

### intent summon 23.png 5e9dc947b39352715829cb2190714bf6

- ID: `path--res----godot-imported-intent-summon-23-png-5e9dc947b39352715829cb2190714bf6-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_23.png-5e9dc947b39352715829cb2190714bf6.ctex"`

### intent summon 24

- ID: `filename----summon-intent-summon-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "summon/intent_summon_24.png",`

### intent summon 24

- ID: `res---images-packed-intents-summon-intent-summon-24-pngr`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/summon/intent_summon_24.pngR)`

### intent summon 24.png

- ID: `images-packed-intents-summon-intent-summon-24-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_24.png.import`

### intent summon 24.png 26077a652f4b3bbc15ade094b1a32823

- ID: `godot-imported-intent-summon-24-png-26077a652f4b3bbc15ade094b1a32823-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon_24.png-26077a652f4b3bbc15ade094b1a32823.ctex`

### intent summon 24.png 26077a652f4b3bbc15ade094b1a32823

- ID: `path--res----godot-imported-intent-summon-24-png-26077a652f4b3bbc15ade094b1a32823-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon_24.png-26077a652f4b3bbc15ade094b1a32823.ctex"`

### intent summon.png

- ID: `images-packed-intents-intent-summon-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_summon.png.import`

### intent summon.png 32e3d6cd19755b35c1ec0c14a319124d

- ID: `godot-imported-intent-summon-png-32e3d6cd19755b35c1ec0c14a319124d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_summon.png-32e3d6cd19755b35c1ec0c14a319124d.ctex`

### intent summon.png 32e3d6cd19755b35c1ec0c14a319124d

- ID: `path--res----godot-imported-intent-summon-png-32e3d6cd19755b35c1ec0c14a319124d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_summon.png-32e3d6cd19755b35c1ec0c14a319124d.ctex"`

### Intent Title

- ID: `intenttitle`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Intent Type

- ID: `intenttype`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Intent Type

- ID: `megacrit-sts2-core-monstermoves-intents-intenttype`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.IntentType`

### intent unknown

- ID: `res---images-packed-intents-intent-unknown-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/intent_unknown.png`

### intent unknown 00

- ID: `filename----unknown-intent-unknown-00-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_00.png",`

### intent unknown 00

- ID: `res---images-packed-intents-unknown-intent-unknown-00-png-j`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_00.png-j`

### intent unknown 00.png

- ID: `images-packed-intents-unknown-intent-unknown-00-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_00.png.import`

### intent unknown 00.png b6ee3dac26e78309e2aba2a37cbe0d08

- ID: `godot-imported-intent-unknown-00-png-b6ee3dac26e78309e2aba2a37cbe0d08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_00.png-b6ee3dac26e78309e2aba2a37cbe0d08.ctex`

### intent unknown 00.png b6ee3dac26e78309e2aba2a37cbe0d08

- ID: `path--res----godot-imported-intent-unknown-00-png-b6ee3dac26e78309e2aba2a37cbe0d08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_00.png-b6ee3dac26e78309e2aba2a37cbe0d08.ctex"`

### intent unknown 01

- ID: `filename----unknown-intent-unknown-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_01.png",`

### intent unknown 01

- ID: `res---images-packed-intents-unknown-intent-unknown-01-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_01.png`

### intent unknown 01.png

- ID: `images-packed-intents-unknown-intent-unknown-01-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_01.png.import`

### intent unknown 01.png 6924b57ad284b1634d118220f864a126

- ID: `godot-imported-intent-unknown-01-png-6924b57ad284b1634d118220f864a126-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_01.png-6924b57ad284b1634d118220f864a126.ctex`

### intent unknown 01.png 6924b57ad284b1634d118220f864a126

- ID: `path--res----godot-imported-intent-unknown-01-png-6924b57ad284b1634d118220f864a126-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_01.png-6924b57ad284b1634d118220f864a126.ctex"`

### intent unknown 02

- ID: `filename----unknown-intent-unknown-02-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_02.png",`

### intent unknown 02

- ID: `res---images-packed-intents-unknown-intent-unknown-02-pngy`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_02.pngY`

### intent unknown 02.png

- ID: `images-packed-intents-unknown-intent-unknown-02-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_02.png.import`

### intent unknown 02.png e11935f29396151949388fb150d2a378

- ID: `godot-imported-intent-unknown-02-png-e11935f29396151949388fb150d2a378-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_02.png-e11935f29396151949388fb150d2a378.ctex`

### intent unknown 02.png e11935f29396151949388fb150d2a378

- ID: `path--res----godot-imported-intent-unknown-02-png-e11935f29396151949388fb150d2a378-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_02.png-e11935f29396151949388fb150d2a378.ctex"`

### intent unknown 03

- ID: `filename----unknown-intent-unknown-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_03.png",`

### intent unknown 03

- ID: `res---images-packed-intents-unknown-intent-unknown-03-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_03.png`

### intent unknown 03.png

- ID: `images-packed-intents-unknown-intent-unknown-03-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_03.png.import`

### intent unknown 03.png 178c9c9e7dce16a1453f0b1e6c2cfca9

- ID: `godot-imported-intent-unknown-03-png-178c9c9e7dce16a1453f0b1e6c2cfca9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_03.png-178c9c9e7dce16a1453f0b1e6c2cfca9.ctex`

### intent unknown 03.png 178c9c9e7dce16a1453f0b1e6c2cfca9

- ID: `path--res----godot-imported-intent-unknown-03-png-178c9c9e7dce16a1453f0b1e6c2cfca9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_03.png-178c9c9e7dce16a1453f0b1e6c2cfca9.ctex"`

### intent unknown 04

- ID: `filename----unknown-intent-unknown-04-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_04.png",`

### intent unknown 04

- ID: `res---images-packed-intents-unknown-intent-unknown-04-pngec`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_04.pngeC`

### intent unknown 04.png

- ID: `images-packed-intents-unknown-intent-unknown-04-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_04.png.import`

### intent unknown 04.png 447bb46301d56cacb8259c9b90e6baee

- ID: `godot-imported-intent-unknown-04-png-447bb46301d56cacb8259c9b90e6baee-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_04.png-447bb46301d56cacb8259c9b90e6baee.ctex`

### intent unknown 04.png 447bb46301d56cacb8259c9b90e6baee

- ID: `path--res----godot-imported-intent-unknown-04-png-447bb46301d56cacb8259c9b90e6baee-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_04.png-447bb46301d56cacb8259c9b90e6baee.ctex"`

### intent unknown 05

- ID: `filename----unknown-intent-unknown-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_05.png",`

### intent unknown 05

- ID: `res---images-packed-intents-unknown-intent-unknown-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_05.png`

### intent unknown 05.png

- ID: `images-packed-intents-unknown-intent-unknown-05-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_05.png.import`

### intent unknown 05.png 5eca093775c518a4e7276aefe66a6c08

- ID: `godot-imported-intent-unknown-05-png-5eca093775c518a4e7276aefe66a6c08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_05.png-5eca093775c518a4e7276aefe66a6c08.ctex`

### intent unknown 05.png 5eca093775c518a4e7276aefe66a6c08

- ID: `path--res----godot-imported-intent-unknown-05-png-5eca093775c518a4e7276aefe66a6c08-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_05.png-5eca093775c518a4e7276aefe66a6c08.ctex"`

### intent unknown 06

- ID: `filename----unknown-intent-unknown-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_06.png",`

### intent unknown 06

- ID: `res---images-packed-intents-unknown-intent-unknown-06-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_06.png`

### intent unknown 06.png

- ID: `images-packed-intents-unknown-intent-unknown-06-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_06.png.import`

### intent unknown 06.png 6de4260cfa3512791fe46def1293b900

- ID: `godot-imported-intent-unknown-06-png-6de4260cfa3512791fe46def1293b900-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_06.png-6de4260cfa3512791fe46def1293b900.ctex`

### intent unknown 06.png 6de4260cfa3512791fe46def1293b900

- ID: `path--res----godot-imported-intent-unknown-06-png-6de4260cfa3512791fe46def1293b900-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_06.png-6de4260cfa3512791fe46def1293b900.ctex"`

### intent unknown 07

- ID: `filename----unknown-intent-unknown-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_07.png",`

### intent unknown 07

- ID: `res---images-packed-intents-unknown-intent-unknown-07-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_07.png_`

### intent unknown 07.png

- ID: `images-packed-intents-unknown-intent-unknown-07-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_07.png.import`

### intent unknown 07.png 9501446bf5a392b83bc7e868bbe2c4bc

- ID: `godot-imported-intent-unknown-07-png-9501446bf5a392b83bc7e868bbe2c4bc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_07.png-9501446bf5a392b83bc7e868bbe2c4bc.ctex`

### intent unknown 07.png 9501446bf5a392b83bc7e868bbe2c4bc

- ID: `path--res----godot-imported-intent-unknown-07-png-9501446bf5a392b83bc7e868bbe2c4bc-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_07.png-9501446bf5a392b83bc7e868bbe2c4bc.ctex"`

### intent unknown 08

- ID: `filename----unknown-intent-unknown-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_08.png",`

### intent unknown 08

- ID: `res---images-packed-intents-unknown-intent-unknown-08-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_08.png`

### intent unknown 08.png

- ID: `images-packed-intents-unknown-intent-unknown-08-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_08.png.import`

### intent unknown 08.png b0bfd986c5c570c326d995965d0b9db9

- ID: `godot-imported-intent-unknown-08-png-b0bfd986c5c570c326d995965d0b9db9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_08.png-b0bfd986c5c570c326d995965d0b9db9.ctex`

### intent unknown 08.png b0bfd986c5c570c326d995965d0b9db9

- ID: `path--res----godot-imported-intent-unknown-08-png-b0bfd986c5c570c326d995965d0b9db9-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_08.png-b0bfd986c5c570c326d995965d0b9db9.ctex"`

### intent unknown 09

- ID: `filename----unknown-intent-unknown-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_09.png",`

### intent unknown 09

- ID: `res---images-packed-intents-unknown-intent-unknown-09-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_09.png}`

### intent unknown 09.png

- ID: `images-packed-intents-unknown-intent-unknown-09-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_09.png.import`

### intent unknown 09.png e70e790795d3463f4abe02b228428413

- ID: `godot-imported-intent-unknown-09-png-e70e790795d3463f4abe02b228428413-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_09.png-e70e790795d3463f4abe02b228428413.ctex`

### intent unknown 09.png e70e790795d3463f4abe02b228428413

- ID: `path--res----godot-imported-intent-unknown-09-png-e70e790795d3463f4abe02b228428413-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_09.png-e70e790795d3463f4abe02b228428413.ctex"`

### intent unknown 10

- ID: `filename----unknown-intent-unknown-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_10.png",`

### intent unknown 10

- ID: `res---images-packed-intents-unknown-intent-unknown-10-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_10.png_`

### intent unknown 10.png

- ID: `images-packed-intents-unknown-intent-unknown-10-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_10.png.import`

### intent unknown 10.png c8be90e74813cd1523417fb768c58f25

- ID: `godot-imported-intent-unknown-10-png-c8be90e74813cd1523417fb768c58f25-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_10.png-c8be90e74813cd1523417fb768c58f25.ctex`

### intent unknown 10.png c8be90e74813cd1523417fb768c58f25

- ID: `path--res----godot-imported-intent-unknown-10-png-c8be90e74813cd1523417fb768c58f25-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_10.png-c8be90e74813cd1523417fb768c58f25.ctex"`

### intent unknown 11

- ID: `filename----unknown-intent-unknown-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_11.png",`

### intent unknown 11

- ID: `res---images-packed-intents-unknown-intent-unknown-11-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_11.png`

### intent unknown 11.png

- ID: `images-packed-intents-unknown-intent-unknown-11-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_11.png.import`

### intent unknown 11.png f78f1a22ffe4f9989d51473e4b726ab7

- ID: `godot-imported-intent-unknown-11-png-f78f1a22ffe4f9989d51473e4b726ab7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_11.png-f78f1a22ffe4f9989d51473e4b726ab7.ctex`

### intent unknown 11.png f78f1a22ffe4f9989d51473e4b726ab7

- ID: `path--res----godot-imported-intent-unknown-11-png-f78f1a22ffe4f9989d51473e4b726ab7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_11.png-f78f1a22ffe4f9989d51473e4b726ab7.ctex"`

### intent unknown 12

- ID: `filename----unknown-intent-unknown-12-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_12.png",`

### intent unknown 12

- ID: `res---images-packed-intents-unknown-intent-unknown-12-pngb`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_12.pngB`

### intent unknown 12.png

- ID: `images-packed-intents-unknown-intent-unknown-12-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_12.png.import`

### intent unknown 12.png 9c63f7db7b37a588fd85d510c34380b0

- ID: `godot-imported-intent-unknown-12-png-9c63f7db7b37a588fd85d510c34380b0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_12.png-9c63f7db7b37a588fd85d510c34380b0.ctex`

### intent unknown 12.png 9c63f7db7b37a588fd85d510c34380b0

- ID: `path--res----godot-imported-intent-unknown-12-png-9c63f7db7b37a588fd85d510c34380b0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_12.png-9c63f7db7b37a588fd85d510c34380b0.ctex"`

### intent unknown 13

- ID: `filename----unknown-intent-unknown-13-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_13.png",`

### intent unknown 13

- ID: `res---images-packed-intents-unknown-intent-unknown-13-png0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_13.png0`

### intent unknown 13.png

- ID: `images-packed-intents-unknown-intent-unknown-13-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_13.png.import`

### intent unknown 13.png dbcb696299fc8f4da0572139dbff87c5

- ID: `godot-imported-intent-unknown-13-png-dbcb696299fc8f4da0572139dbff87c5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_13.png-dbcb696299fc8f4da0572139dbff87c5.ctex`

### intent unknown 13.png dbcb696299fc8f4da0572139dbff87c5

- ID: `path--res----godot-imported-intent-unknown-13-png-dbcb696299fc8f4da0572139dbff87c5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_13.png-dbcb696299fc8f4da0572139dbff87c5.ctex"`

### intent unknown 14

- ID: `filename----unknown-intent-unknown-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_14.png",`

### intent unknown 14

- ID: `res---images-packed-intents-unknown-intent-unknown-14-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_14.png`

### intent unknown 14.png

- ID: `images-packed-intents-unknown-intent-unknown-14-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_14.png.import`

### intent unknown 14.png ff16dc9a16d123d907260a39f51a0ae7

- ID: `godot-imported-intent-unknown-14-png-ff16dc9a16d123d907260a39f51a0ae7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_14.png-ff16dc9a16d123d907260a39f51a0ae7.ctex`

### intent unknown 14.png ff16dc9a16d123d907260a39f51a0ae7

- ID: `path--res----godot-imported-intent-unknown-14-png-ff16dc9a16d123d907260a39f51a0ae7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_14.png-ff16dc9a16d123d907260a39f51a0ae7.ctex"`

### intent unknown 15

- ID: `filename----unknown-intent-unknown-15-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_15.png",`

### intent unknown 15

- ID: `res---images-packed-intents-unknown-intent-unknown-15-pngi`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_15.pngI`

### intent unknown 15.png

- ID: `images-packed-intents-unknown-intent-unknown-15-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_15.png.import`

### intent unknown 15.png 674509a9d3e5c60b08489fa947321ffa

- ID: `godot-imported-intent-unknown-15-png-674509a9d3e5c60b08489fa947321ffa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_15.png-674509a9d3e5c60b08489fa947321ffa.ctex`

### intent unknown 15.png 674509a9d3e5c60b08489fa947321ffa

- ID: `path--res----godot-imported-intent-unknown-15-png-674509a9d3e5c60b08489fa947321ffa-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_15.png-674509a9d3e5c60b08489fa947321ffa.ctex"`

### intent unknown 16

- ID: `filename----unknown-intent-unknown-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_16.png",`

### intent unknown 16

- ID: `res---images-packed-intents-unknown-intent-unknown-16-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_16.png`

### intent unknown 16.png

- ID: `images-packed-intents-unknown-intent-unknown-16-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_16.png.import`

### intent unknown 16.png 0a80273149fd3452560caa48c2c5fd7f

- ID: `godot-imported-intent-unknown-16-png-0a80273149fd3452560caa48c2c5fd7f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_16.png-0a80273149fd3452560caa48c2c5fd7f.ctex`

### intent unknown 16.png 0a80273149fd3452560caa48c2c5fd7f

- ID: `path--res----godot-imported-intent-unknown-16-png-0a80273149fd3452560caa48c2c5fd7f-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_16.png-0a80273149fd3452560caa48c2c5fd7f.ctex"`

### intent unknown 17

- ID: `filename----unknown-intent-unknown-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_17.png",`

### intent unknown 17

- ID: `res---images-packed-intents-unknown-intent-unknown-17-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_17.png`

### intent unknown 17.png

- ID: `images-packed-intents-unknown-intent-unknown-17-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_17.png.import`

### intent unknown 17.png 3fc70062fad376dfeeaa12401cb83eca

- ID: `godot-imported-intent-unknown-17-png-3fc70062fad376dfeeaa12401cb83eca-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_17.png-3fc70062fad376dfeeaa12401cb83eca.ctex`

### intent unknown 17.png 3fc70062fad376dfeeaa12401cb83eca

- ID: `path--res----godot-imported-intent-unknown-17-png-3fc70062fad376dfeeaa12401cb83eca-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_17.png-3fc70062fad376dfeeaa12401cb83eca.ctex"`

### intent unknown 18

- ID: `filename----unknown-intent-unknown-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_18.png",`

### intent unknown 18

- ID: `res---images-packed-intents-unknown-intent-unknown-18-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_18.png`

### intent unknown 18.png

- ID: `images-packed-intents-unknown-intent-unknown-18-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_18.png.import`

### intent unknown 18.png 832666d75b43641c93683dc9af45229a

- ID: `godot-imported-intent-unknown-18-png-832666d75b43641c93683dc9af45229a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_18.png-832666d75b43641c93683dc9af45229a.ctex`

### intent unknown 18.png 832666d75b43641c93683dc9af45229a

- ID: `path--res----godot-imported-intent-unknown-18-png-832666d75b43641c93683dc9af45229a-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_18.png-832666d75b43641c93683dc9af45229a.ctex"`

### intent unknown 19

- ID: `filename----unknown-intent-unknown-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_19.png",`

### intent unknown 19

- ID: `res---images-packed-intents-unknown-intent-unknown-19-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_19.png`

### intent unknown 19.png

- ID: `images-packed-intents-unknown-intent-unknown-19-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_19.png.import`

### intent unknown 19.png 9c036798cf9a1f894ac05e1f3e0bdc20

- ID: `godot-imported-intent-unknown-19-png-9c036798cf9a1f894ac05e1f3e0bdc20-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_19.png-9c036798cf9a1f894ac05e1f3e0bdc20.ctex`

### intent unknown 19.png 9c036798cf9a1f894ac05e1f3e0bdc20

- ID: `path--res----godot-imported-intent-unknown-19-png-9c036798cf9a1f894ac05e1f3e0bdc20-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_19.png-9c036798cf9a1f894ac05e1f3e0bdc20.ctex"`

### intent unknown 20

- ID: `filename----unknown-intent-unknown-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_20.png",`

### intent unknown 20

- ID: `res---images-packed-intents-unknown-intent-unknown-20-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_20.png]`

### intent unknown 20.png

- ID: `images-packed-intents-unknown-intent-unknown-20-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_20.png.import`

### intent unknown 20.png f58f20db1707cf265f709fb2827a89ff

- ID: `godot-imported-intent-unknown-20-png-f58f20db1707cf265f709fb2827a89ff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_20.png-f58f20db1707cf265f709fb2827a89ff.ctex`

### intent unknown 20.png f58f20db1707cf265f709fb2827a89ff

- ID: `path--res----godot-imported-intent-unknown-20-png-f58f20db1707cf265f709fb2827a89ff-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_20.png-f58f20db1707cf265f709fb2827a89ff.ctex"`

### intent unknown 21

- ID: `filename----unknown-intent-unknown-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_21.png",`

### intent unknown 21

- ID: `res---images-packed-intents-unknown-intent-unknown-21-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_21.png`

### intent unknown 21.png

- ID: `images-packed-intents-unknown-intent-unknown-21-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_21.png.import`

### intent unknown 21.png 72945d51ef2736e0308e2ec495f0e380

- ID: `godot-imported-intent-unknown-21-png-72945d51ef2736e0308e2ec495f0e380-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_21.png-72945d51ef2736e0308e2ec495f0e380.ctex`

### intent unknown 21.png 72945d51ef2736e0308e2ec495f0e380

- ID: `path--res----godot-imported-intent-unknown-21-png-72945d51ef2736e0308e2ec495f0e380-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_21.png-72945d51ef2736e0308e2ec495f0e380.ctex"`

### intent unknown 22

- ID: `filename----unknown-intent-unknown-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_22.png",`

### intent unknown 22

- ID: `res---images-packed-intents-unknown-intent-unknown-22-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_22.png'=`

### intent unknown 22.png

- ID: `images-packed-intents-unknown-intent-unknown-22-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_22.png.import`

### intent unknown 22.png 8d2cdd4a764536ebe0b0ebf4bc0ee6e4

- ID: `godot-imported-intent-unknown-22-png-8d2cdd4a764536ebe0b0ebf4bc0ee6e4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_22.png-8d2cdd4a764536ebe0b0ebf4bc0ee6e4.ctex`

### intent unknown 22.png 8d2cdd4a764536ebe0b0ebf4bc0ee6e4

- ID: `path--res----godot-imported-intent-unknown-22-png-8d2cdd4a764536ebe0b0ebf4bc0ee6e4-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_22.png-8d2cdd4a764536ebe0b0ebf4bc0ee6e4.ctex"`

### intent unknown 23

- ID: `filename----unknown-intent-unknown-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_23.png",`

### intent unknown 23

- ID: `res---images-packed-intents-unknown-intent-unknown-23-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_23.png`

### intent unknown 23.png

- ID: `images-packed-intents-unknown-intent-unknown-23-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_23.png.import`

### intent unknown 23.png b77007621eb953711bf905f6bad37ea5

- ID: `godot-imported-intent-unknown-23-png-b77007621eb953711bf905f6bad37ea5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_23.png-b77007621eb953711bf905f6bad37ea5.ctex`

### intent unknown 23.png b77007621eb953711bf905f6bad37ea5

- ID: `path--res----godot-imported-intent-unknown-23-png-b77007621eb953711bf905f6bad37ea5-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_23.png-b77007621eb953711bf905f6bad37ea5.ctex"`

### intent unknown 24

- ID: `filename----unknown-intent-unknown-24-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_24.png",`

### intent unknown 24

- ID: `res---images-packed-intents-unknown-intent-unknown-24-pngo`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_24.pngo`

### intent unknown 24.png

- ID: `images-packed-intents-unknown-intent-unknown-24-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_24.png.import`

### intent unknown 24.png cb721cef964a997ba256499c29d33521

- ID: `godot-imported-intent-unknown-24-png-cb721cef964a997ba256499c29d33521-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_24.png-cb721cef964a997ba256499c29d33521.ctex`

### intent unknown 24.png cb721cef964a997ba256499c29d33521

- ID: `path--res----godot-imported-intent-unknown-24-png-cb721cef964a997ba256499c29d33521-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_24.png-cb721cef964a997ba256499c29d33521.ctex"`

### intent unknown 25

- ID: `filename----unknown-intent-unknown-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_25.png",`

### intent unknown 25

- ID: `res---images-packed-intents-unknown-intent-unknown-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_25.png`

### intent unknown 25.png

- ID: `images-packed-intents-unknown-intent-unknown-25-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_25.png.import`

### intent unknown 25.png 2eec790ec48706c25208ce8867b76af7

- ID: `godot-imported-intent-unknown-25-png-2eec790ec48706c25208ce8867b76af7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_25.png-2eec790ec48706c25208ce8867b76af7.ctex`

### intent unknown 25.png 2eec790ec48706c25208ce8867b76af7

- ID: `path--res----godot-imported-intent-unknown-25-png-2eec790ec48706c25208ce8867b76af7-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_25.png-2eec790ec48706c25208ce8867b76af7.ctex"`

### intent unknown 26

- ID: `filename----unknown-intent-unknown-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_26.png",`

### intent unknown 26

- ID: `res---images-packed-intents-unknown-intent-unknown-26-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_26.png`

### intent unknown 26.png

- ID: `images-packed-intents-unknown-intent-unknown-26-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_26.png.import`

### intent unknown 26.png 5e81570be4ec0c75be7ded712085120c

- ID: `godot-imported-intent-unknown-26-png-5e81570be4ec0c75be7ded712085120c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_26.png-5e81570be4ec0c75be7ded712085120c.ctex`

### intent unknown 26.png 5e81570be4ec0c75be7ded712085120c

- ID: `path--res----godot-imported-intent-unknown-26-png-5e81570be4ec0c75be7ded712085120c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_26.png-5e81570be4ec0c75be7ded712085120c.ctex"`

### intent unknown 27

- ID: `filename----unknown-intent-unknown-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_27.png",`

### intent unknown 27

- ID: `res---images-packed-intents-unknown-intent-unknown-27-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_27.png`

### intent unknown 27.png

- ID: `images-packed-intents-unknown-intent-unknown-27-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_27.png.import`

### intent unknown 27.png 656b679a6d35c244d664b6e42d6b03a0

- ID: `godot-imported-intent-unknown-27-png-656b679a6d35c244d664b6e42d6b03a0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_27.png-656b679a6d35c244d664b6e42d6b03a0.ctex`

### intent unknown 27.png 656b679a6d35c244d664b6e42d6b03a0

- ID: `path--res----godot-imported-intent-unknown-27-png-656b679a6d35c244d664b6e42d6b03a0-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_27.png-656b679a6d35c244d664b6e42d6b03a0.ctex"`

### intent unknown 28

- ID: `filename----unknown-intent-unknown-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_28.png",`

### intent unknown 28

- ID: `res---images-packed-intents-unknown-intent-unknown-28-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_28.png`

### intent unknown 28.png

- ID: `images-packed-intents-unknown-intent-unknown-28-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_28.png.import`

### intent unknown 28.png a72ab94cd73f9aa27ad898985408671c

- ID: `godot-imported-intent-unknown-28-png-a72ab94cd73f9aa27ad898985408671c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_28.png-a72ab94cd73f9aa27ad898985408671c.ctex`

### intent unknown 28.png a72ab94cd73f9aa27ad898985408671c

- ID: `path--res----godot-imported-intent-unknown-28-png-a72ab94cd73f9aa27ad898985408671c-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_28.png-a72ab94cd73f9aa27ad898985408671c.ctex"`

### intent unknown 29

- ID: `filename----unknown-intent-unknown-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "unknown/intent_unknown_29.png",`

### intent unknown 29

- ID: `res---images-packed-intents-unknown-intent-unknown-29-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/unknown/intent_unknown_29.png`

### intent unknown 29.png

- ID: `images-packed-intents-unknown-intent-unknown-29-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/unknown/intent_unknown_29.png.import`

### intent unknown 29.png 9877f46a9663d263bc8b12a7a6c527ad

- ID: `godot-imported-intent-unknown-29-png-9877f46a9663d263bc8b12a7a6c527ad-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown_29.png-9877f46a9663d263bc8b12a7a6c527ad.ctex`

### intent unknown 29.png 9877f46a9663d263bc8b12a7a6c527ad

- ID: `path--res----godot-imported-intent-unknown-29-png-9877f46a9663d263bc8b12a7a6c527ad-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown_29.png-9877f46a9663d263bc8b12a7a6c527ad.ctex"`

### intent unknown.png

- ID: `images-packed-intents-intent-unknown-png-import`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/intent_unknown.png.import`

### intent unknown.png d4c8e0126fd20804374d1c9eeddb189d

- ID: `godot-imported-intent-unknown-png-d4c8e0126fd20804374d1c9eeddb189d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/intent_unknown.png-d4c8e0126fd20804374d1c9eeddb189d.ctex`

### intent unknown.png d4c8e0126fd20804374d1c9eeddb189d

- ID: `path--res----godot-imported-intent-unknown-png-d4c8e0126fd20804374d1c9eeddb189d-ctex`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/intent_unknown.png-d4c8e0126fd20804374d1c9eeddb189d.ctex"`

### IntentAnimData

- ID: `res---src-core-entities-intents-intentanimdata-csp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Entities/Intents/IntentAnimData.csP$'`

### Intents

- ID: `intents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### intents

- ID: `localization-deu-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/deu/intents.json`

### intents

- ID: `localization-eng-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/eng/intents.json`

### intents

- ID: `localization-esp-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/esp/intents.json`

### intents

- ID: `localization-fra-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/fra/intents.json`

### intents

- ID: `localization-ita-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/ita/intents.json`

### intents

- ID: `localization-jpn-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/jpn/intents.json`

### intents

- ID: `localization-kor-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/kor/intents.json`

### intents

- ID: `localization-pol-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/pol/intents.json`

### intents

- ID: `localization-ptb-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/ptb/intents.json`

### intents

- ID: `localization-rus-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/rus/intents.json`

### intents

- ID: `localization-spa-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/spa/intents.json`

### intents

- ID: `localization-tha-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/tha/intents.json`

### intents

- ID: `localization-tur-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/tur/intents.json`

### intents

- ID: `localization-zhs-intents-json`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/zhs/intents.json`

### IntentType

- ID: `res---src-core-monstermoves-intents-intenttype-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/IntentType.cs`

### Internal Data

- ID: `megacrit-sts2-core-entities-intents-intentanimdata-internaldata`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Entities.Intents.IntentAnimData+InternalData`

### Is Debug Encounter

- ID: `isdebugencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Is Debug Hide Mp Intents

- ID: `isdebughidempintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Is Debug Hiding Intent

- ID: `isdebughidingintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Kaiser Crab Boss

- ID: `megacrit-sts2-core-models-encounters-kaisercrabboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.KaiserCrabBoss`

### keyword

- ID: `keyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### keyword Hover Tips

- ID: `keywordhovertips`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords

- ID: `keywords`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords

- ID: `megacrit-sts2-gameinfo-objects-keywords`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.GameInfo.Objects.Keywords`

### Keywords Changed

- ID: `keywordschanged`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords Ctor Param Init

- ID: `keywordsctorparaminit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords Prop Init

- ID: `keywordspropinit`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords Serialize Handler

- ID: `keywordsserializehandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Keywords.cs

- ID: `res---src-gameinfo-objects-keywords-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/GameInfo/Objects/Keywords.cs.`

### Killed By Encounter

- ID: `killedbyencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Knights Elite

- ID: `megacrit-sts2-core-models-encounters-knightselite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.KnightsElite`

### Knowledge Demon Boss

- ID: `megacrit-sts2-core-models-encounters-knowledgedemonboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.KnowledgeDemonBoss`

### Lagavulin Matriarch Boss

- ID: `megacrit-sts2-core-models-encounters-lagavulinmatriarchboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.LagavulinMatriarchBoss`

### List Encounter Metric

- ID: `listencountermetric`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### List Encounter Metric Serialize Handler

- ID: `listencountermetricserializehandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### List Encounter Stats

- ID: `listencounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Living Fog Normal

- ID: `megacrit-sts2-core-models-encounters-livingfognormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.LivingFogNormal`

### Louse Progenitor Normal

- ID: `megacrit-sts2-core-models-encounters-louseprogenitornormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.LouseProgenitorNormal`

### Mawler Normal

- ID: `megacrit-sts2-core-models-encounters-mawlernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.MawlerNormal`

### Mecha Knight Elite

- ID: `megacrit-sts2-core-models-encounters-mechaknightelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.MechaKnightElite`

### Method Name

- ID: `megacrit-sts2-core-nodes-combat-nintent-methodname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NIntent+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-multiplayer-nmultiplayerplayerintenthandler-methodname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Multiplayer.NMultiplayerPlayerIntentHandler+MethodName`

### Mock Artifact Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mockartifactencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockArtifactEncounter`

### Mock Boss Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mockbossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockBossEncounter`

### Mock Elite Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mockeliteencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockEliteEncounter`

### Mock Keyword

- ID: `mockkeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Mock Monster Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mockmonsterencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockMonsterEncounter`

### Mock Plating Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mockplatingencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockPlatingEncounter`

### Mock Two Monster Encounter

- ID: `megacrit-sts2-core-models-encounters-mocks-mocktwomonsterencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.Mocks.MockTwoMonsterEncounter`

### Multi Attack Intent

- ID: `megacrit-sts2-core-monstermoves-intents-multiattackintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.MultiAttackIntent`

### MultiAttackIntent

- ID: `res---src-core-monstermoves-intents-multiattackintent-csp`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/MultiAttackIntent.csp`

### multiplayer player intent

- ID: `res---scenes-combat-multiplayer-player-intent-tscn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/combat/multiplayer_player_intent.tscn`

### multiplayer player intent

- ID: `scenes-combat-multiplayer-player-intent-tscn`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/combat/multiplayer_player_intent.tscn`

### mutable Encounter

- ID: `mutableencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Mytes Normal

- ID: `megacrit-sts2-core-models-encounters-mytesnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.MytesNormal`

### Next Boss Encounter

- ID: `nextbossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Next Elite Encounter

- ID: `nexteliteencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Next Normal Encounter

- ID: `nextnormalencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Nibbits Normal

- ID: `megacrit-sts2-core-models-encounters-nibbitsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.NibbitsNormal`

### Nibbits Weak

- ID: `megacrit-sts2-core-models-encounters-nibbitsweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.NibbitsWeak`

### NIntent

- ID: `ext-resource-type--script--uid--uid---cflxul0vhkobf--path--res---src-core-nodes-combat-nintent-cs--id--1-w2ny7`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://cflxul0vhkobf" path="res://src/Core/Nodes/Combat/NIntent.cs" id="1_w2ny7"]`

### NIntent

- ID: `megacrit-sts2-core-nodes-combat-nintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NIntent`

### NIntent

- ID: `res---src-core-nodes-combat-nintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Combat/NIntent.cs`

### NMultiplayer Player Intent Handler

- ID: `megacrit-sts2-core-nodes-multiplayer-nmultiplayerplayerintenthandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Multiplayer.NMultiplayerPlayerIntentHandler`

### NMultiplayerPlayerIntentHandler

- ID: `ext-resource-type--script--uid--uid---j6rd1mtyvpv8--path--res---src-core-nodes-multiplayer-nmultiplayerplayerintenthandler-cs--id--1-m7flx`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://j6rd1mtyvpv8" path="res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerIntentHandler.cs" id="1_m7flx"]`

### NMultiplayerPlayerIntentHandler

- ID: `res---src-core-nodes-multiplayer-nmultiplayerplayerintenthandler-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerIntentHandler.cs`

### Normal Encounter Ids

- ID: `normalencounterids`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### normal Encounters

- ID: `normalencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Normal Encounters Visited

- ID: `normalencountersvisited`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Number Of Weak Encounters

- ID: `numberofweakencounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Overgrowth Crawlers

- ID: `megacrit-sts2-core-models-encounters-overgrowthcrawlers`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.OvergrowthCrawlers`

### Ovicopter Normal

- ID: `megacrit-sts2-core-models-encounters-ovicopternormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.OvicopterNormal`

### Owl Magistrate Normal

- ID: `megacrit-sts2-core-models-encounters-owlmagistratenormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.OwlMagistrateNormal`

### Parse Encounter Stats

- ID: `parseencounterstats`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Perform Intent

- ID: `performintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Phantasmal Gardeners Elite

- ID: `megacrit-sts2-core-models-encounters-phantasmalgardenerselite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.PhantasmalGardenersElite`

### Phrog Parasite Elite

- ID: `megacrit-sts2-core-models-encounters-phrogparasiteelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.PhrogParasiteElite`

### Player Intent Handler

- ID: `playerintenthandler`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### power Intent

- ID: `powerintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Prop Name bot keyword

- ID: `propname-bot-keyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Prop Name encounters

- ID: `propname-encounters`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Prop Name killed By Encounter

- ID: `propname-killedbyencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Property Name

- ID: `megacrit-sts2-core-nodes-combat-nintent-propertyname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NIntent+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-multiplayer-nmultiplayerplayerintenthandler-propertyname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Multiplayer.NMultiplayerPlayerIntentHandler+PropertyName`

### Pull Next Encounter

- ID: `pullnextencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Punch Construct Normal

- ID: `megacrit-sts2-core-models-encounters-punchconstructnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.PunchConstructNormal`

### Queen Boss

- ID: `megacrit-sts2-core-models-encounters-queenboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.QueenBoss`

### Refresh Intents

- ID: `refreshintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Regular Encounter

- ID: `regularencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Remove Keyword

- ID: `removekeyword`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Reveal Intents

- ID: `revealintents`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Ruby Raiders Normal

- ID: `megacrit-sts2-core-models-encounters-rubyraidersnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.RubyRaidersNormal`

### Scrolls Of Biting Normal

- ID: `megacrit-sts2-core-models-encounters-scrollsofbitingnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ScrollsOfBitingNormal`

### Scrolls Of Biting Weak

- ID: `megacrit-sts2-core-models-encounters-scrollsofbitingweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ScrollsOfBitingWeak`

### Seapunk Weak

- ID: `megacrit-sts2-core-models-encounters-seapunkweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SeapunkWeak`

### Second Boss Encounter

- ID: `secondbossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Secondary Enemy Type

- ID: `megacrit-sts2-core-models-encounters-slitheringstranglernormal-secondaryenemytype`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlitheringStranglerNormal+SecondaryEnemyType`

### Set Boss Encounter

- ID: `setbossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Set Custom Description Encounter Source

- ID: `setcustomdescriptionencountersource`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Set Second Boss Encounter

- ID: `setsecondbossencounter`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Sewer Clam Normal

- ID: `megacrit-sts2-core-models-encounters-sewerclamnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SewerClamNormal`

### Shrinker Beetle Weak

- ID: `megacrit-sts2-core-models-encounters-shrinkerbeetleweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ShrinkerBeetleWeak`

### Signal Name

- ID: `megacrit-sts2-core-nodes-combat-nintent-signalname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NIntent+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-multiplayer-nmultiplayerplayerintenthandler-signalname`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Multiplayer.NMultiplayerPlayerIntentHandler+SignalName`

### Single Attack Intent

- ID: `megacrit-sts2-core-monstermoves-intents-singleattackintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.SingleAttackIntent`

### SingleAttackIntent

- ID: `res---src-core-monstermoves-intents-singleattackintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/SingleAttackIntent.cs[`

### Skulking Colony Elite

- ID: `megacrit-sts2-core-models-encounters-skulkingcolonyelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SkulkingColonyElite`

### Sleep Intent

- ID: `megacrit-sts2-core-monstermoves-intents-sleepintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.SleepIntent`

### SleepIntent

- ID: `res---src-core-monstermoves-intents-sleepintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/SleepIntent.cs`

### Slimed Berserker Normal

- ID: `megacrit-sts2-core-models-encounters-slimedberserkernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlimedBerserkerNormal`

### Slimes Normal

- ID: `megacrit-sts2-core-models-encounters-slimesnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlimesNormal`

### Slimes Weak

- ID: `megacrit-sts2-core-models-encounters-slimesweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlimesWeak`

### Slithering Strangler Normal

- ID: `megacrit-sts2-core-models-encounters-slitheringstranglernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlitheringStranglerNormal`

### Sludge Spinner Weak

- ID: `megacrit-sts2-core-models-encounters-sludgespinnerweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SludgeSpinnerWeak`

### Slumbering Beetle Normal

- ID: `megacrit-sts2-core-models-encounters-slumberingbeetlenormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SlumberingBeetleNormal`

### Snapping Jaxfruit Normal

- ID: `megacrit-sts2-core-models-encounters-snappingjaxfruitnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SnappingJaxfruitNormal`

### Soul Fysh Boss

- ID: `megacrit-sts2-core-models-encounters-soulfyshboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SoulFyshBoss`

### Soul Nexus Elite

- ID: `megacrit-sts2-core-models-encounters-soulnexuselite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SoulNexusElite`

### special Searchbar Keywords

- ID: `specialsearchbarkeywords`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Spiny Toad Normal

- ID: `megacrit-sts2-core-models-encounters-spinytoadnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.SpinyToadNormal`

### Status Intent

- ID: `megacrit-sts2-core-monstermoves-intents-statusintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.StatusIntent`

### StatusIntent

- ID: `res---src-core-monstermoves-intents-statusintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/StatusIntent.cs`

### Stun Intent

- ID: `megacrit-sts2-core-monstermoves-intents-stunintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.StunIntent`

### StunIntent

- ID: `res---src-core-monstermoves-intents-stunintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/StunIntent.cs`

### Summon Intent

- ID: `megacrit-sts2-core-monstermoves-intents-summonintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.SummonIntent`

### SummonIntent

- ID: `res---src-core-monstermoves-intents-summonintent-csa`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/SummonIntent.csa`

### Terror Eel Elite

- ID: `megacrit-sts2-core-models-encounters-terroreelelite`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TerrorEelElite`

### Test Subject Boss

- ID: `megacrit-sts2-core-models-encounters-testsubjectboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TestSubjectBoss`

### The Insatiable Boss

- ID: `megacrit-sts2-core-models-encounters-theinsatiableboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TheInsatiableBoss`

### The Kin Boss

- ID: `megacrit-sts2-core-models-encounters-thekinboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TheKinBoss`

### The Lost And Forgotten Normal

- ID: `megacrit-sts2-core-models-encounters-thelostandforgottennormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TheLostAndForgottenNormal`

### The Obscura Normal

- ID: `megacrit-sts2-core-models-encounters-theobscuranormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TheObscuraNormal`

### Thieving Hopper Weak

- ID: `megacrit-sts2-core-models-encounters-thievinghopperweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ThievingHopperWeak`

### Toadpoles Normal

- ID: `megacrit-sts2-core-models-encounters-toadpolesnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ToadpolesNormal`

### Toadpoles Weak

- ID: `megacrit-sts2-core-models-encounters-toadpolesweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.ToadpolesWeak`

### Tunneler Normal

- ID: `megacrit-sts2-core-models-encounters-tunnelernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TunnelerNormal`

### Tunneler Weak

- ID: `megacrit-sts2-core-models-encounters-tunnelerweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TunnelerWeak`

### Turret Operator Weak

- ID: `megacrit-sts2-core-models-encounters-turretoperatorweak`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TurretOperatorWeak`

### Two Tailed Rats Normal

- ID: `megacrit-sts2-core-models-encounters-twotailedratsnormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TwoTailedRatsNormal`

### Unknown

- ID: `images-packed-intents-summon-intent-summon-07-png-import0`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/intents/summon/intent_summon_07.png.import0/`

### Unknown

- ID: `res---images-packed-intents-buff-intent-buff-25-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/buff/intent_buff_25.png/`

### Unknown

- ID: `res---images-packed-intents-defend-intent-defend-03-pngf`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_03.pngf/`

### Unknown

- ID: `res---images-packed-intents-defend-intent-defend-14-pngr`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/defend/intent_defend_14.pngR/`

### Unknown

- ID: `res---images-packed-intents-heal-intent-heal-36-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/heal/intent_heal_36.png/`

### Unknown

- ID: `res---images-packed-intents-sleep-intent-sleep-05-png`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/intents/sleep/intent_sleep_05.png/`

### Unknown Intent

- ID: `megacrit-sts2-core-monstermoves-intents-unknownintent`
- 그룹/풀 추정: 의도/Intent
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.MonsterMoves.Intents.UnknownIntent`

### UnknownIntent

- ID: `res---src-core-monstermoves-intents-unknownintent-cs`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/MonsterMoves/Intents/UnknownIntent.cs--%-`

### Update Intent

- ID: `updateintent`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Vantom Boss

- ID: `megacrit-sts2-core-models-encounters-vantomboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.VantomBoss`

### Vine Shambler Normal

- ID: `megacrit-sts2-core-models-encounters-vineshamblernormal`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.VineShamblerNormal`

### Waterfall Giant Boss

- ID: `megacrit-sts2-core-models-encounters-waterfallgiantboss`
- 그룹/풀 추정: 키워드
- 플레이 중 참조 시점: 카드 문구, 상태 이상, Intent 해석 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.WaterfallGiantBoss`

