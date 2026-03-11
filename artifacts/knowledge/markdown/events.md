# 이벤트

- 전체 항목 수: 1411
- 설명 본문이 채워진 항목: 91
- L10N 키 또는 제목이 연결된 항목: 107
- 선택지/옵션 정보가 있는 항목: 58

## 이 섹션이 도와주는 플레이 장면

- 이벤트 방 진입
- 선택지 표시
- 페이지 본문 파악

## 현재 이 섹션에서 확인된 것

- 이벤트 제목
- 페이지 본문
- 선택지 제목/설명
- 대사 프리뷰

## 아직 남은 점

- 분기 결과 보강
- 실플레이 선택지 교차 검증

## 주요 L10N/리소스 힌트

- `localization/kor/events.json`
- `localization/eng/events.json`

## 항목 목록

### 땜질 시간

- ID: `megacrit-sts2-core-models-events-tinkertime`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [red][sine]끝도 없이 넘쳐나는 시체들[/sine][/red]을 헤쳐나가던 도중, 각양각색의 잔해를 뒤적거리고 있는 [orange]괴짜 과학자[/orange]를 발견했습니다.

“어. 네, 안녕하세요! 꽤 실력이 있는 전사이신 것 같네요... 제 다음 [green]포악 장치[/green]의 테스터가 필요했거든요! 한 번 어때요?”
- 확인된 선택지:
  - 개선: 매 전투 종료 시, 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 도구: 파워 카드를 만듭니다.
  - 무기: 공격 카드를 만듭니다.
  - 보호대: 스킬 카드를 만듭니다.
  - 수락한다: 커스텀 카드를 1장 생성해 [gold]덱[/gold]에 추가합니다.
  - 전문성: [gold]힘[/gold]을 [blue]{ExpertiseStrength}[/blue] 얻습니다. [gold]민첩[/gold]을 [blue]{ExpertiseDexterity}[/blue] 얻습니다.
  - 지혜: 카드를 [blue]{WisdomCards}[/blue]장 뽑습니다.
  - 질식: 이번 턴에 카드를 사용할 때마다, 대상 적이 체력을 [blue]{ChokingDamage}[/blue] 잃습니다.
- 부가 메모: 공격 카드를 만듭니다. | 파워 카드를 만듭니다. | 스킬 카드를 만듭니다. | 무작위 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드의 비용이 [blue]0[/blue] {energyPrefix:energyIcons(1)}이 됩니다.
- 페이지 요약: CHOOSE_CARD_TYPE: “어떤 도구가 [red][jitter]전부 죽여버리는[/jitter][/red] 데에 도움이 될까요?” | CHOOSE_RIDER: “훌륭한 선택이군요! 그럼 이걸로 [sine]뭘[/sine] 해야 할까요?” [orange]과학자[/orange]는 기쁨에 찬 표정으로 두 손을 비벼댑니다. | DONE: “휴! 끝났어요. 이게 당신 거예요! 이제 나가서 마음껏 [red][jitter]학살해보세요[/jitter][/red]!!” 당신은 몰랐겠지만 그 [orange]과학자[/orange]는 이후에 수백 가지의 무기를 만들어냈으며, 그 결과 첨탑 안에서 수천의 목숨이 사라졌습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TINKER_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TinkerTime`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 무한의 컨베이어

- ID: `megacrit-sts2-core-models-events-endlessconveyor`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 한 오두막집에 들어섭니다. 밝지만 뒤틀린 표지판에는 다음과 같은 내용이 적혀 있습니다:
[aqua]“끝없는 만찬 - 배고픈 만큼 계산하세요!”[/aqua]

오두막의 안쪽에는, [orange]가늘고 길쭉한 팔을 여럿 드리우고 있는 요리사[/orange]가 능숙한 솜씨로 [green]한 입 크기의 음식[/green]을 준비해, [sine]구불구불한 키틴질 벨트[/sine] 위에 올려놓고 있습니다.

요리사의 팔 중 하나가 표지판을 가리킵니다:
접시당 [blue]35[/blue] [gold]골드[/gold]
- 확인된 선택지:
  - 떠난다
  - 매콤 톡톡을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 카드를 1장 [gold]강화[/gold]합니다. 만찬을 이어갑니다!
  - 불량 해초 샐러드를 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. [gold]덱[/gold]에 [gold]광란의 포식[/gold]을 추가합니다. 만찬을 이어갑니다!
  - 빈털터리: 더 먹을 수는 있지만, [gold]골드[/gold]가 없습니다.
  - 수상한 조미료를 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 [gold]포션[/gold]을 1개 생성합니다. 만찬을 이어갑니다!
  - 요리사를 관찰한다: [gold]덱[/gold]의 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 장어 튀김을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. [gold]덱[/gold]에 무작위 무색 카드를 1장 추가합니다. 만찬을 이어갑니다!
  - 젤리 간을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 카드를 1장 [gold]변화[/gold]시킵니다. 만찬을 이어갑니다!
- 부가 메모: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 최대 체력을 [green]{CaviarMaxHp}[/green] 얻습니다. 만찬을 이어갑니다! | [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 체력을 [green]{ClamRollHeal}[/green] 회복합니다. 만찬을 이어갑니다! | [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. [gold]덱[/gold]에 무작위 무색 카드를 1장 추가합니다. 만찬을 이어갑니다! | [green]운이 좋으시군요![/green] [gold]골드[/gold]를 [blue]{GoldenFyshGold}[/blue] 얻습니다.
- 페이지 요약: GRAB_SOMETHING_OFF_THE_BELT: 당신은 [gold]{LastDishTitle}[/gold] 접시를 가져온 뒤, 허겁지겁 먹어치웠습니다. [green]맛있네요![/green] | INITIAL: 당신은 한 오두막집에 들어섭니다. 밝지만 뒤틀린 표지판에는 다음과 같은 내용이 적혀 있습니다: [aqua]“끝없는 만찬 - 배고픈 만큼 계산하세요!”[/aqua] 오두막의 안쪽에는, [orange]가늘고 길쭉한 팔을 여럿 드리우고 있는 요리사[/orange]가 능숙한 솜씨로 [green]한 입 크기의 음식[/green]을 준비해, [sine]구불구불한 키틴질 벨트[/sine] 위에 올려놓고 있습니다. 요리사의 팔 중 하나가 표지판을 가리킵니다: 접시당 [blue]35[/blue] [gold]골드[/gold] | LEAVE: 충분히 배를 채운 당신은, 다시 발걸음을 옮깁니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENDLESS_CONVEYOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 미끄러운 다리

- ID: `megacrit-sts2-core-models-events-slipperybridge`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무너져 내릴 듯한 나무 다리를 건너던 도중, [blue][jitter]갑작스럽게 폭우[/jitter][/blue]가 쏟아집니다. [purple][sine]거대한 돌풍[/sine][/purple]이, 마치 당신의 여정을 끝내려는 듯이 위협적으로 휘몰아칩니다.
- 확인된 선택지:
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
- 부가 메모: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다. | [red]{RandomCard}[/red](이)가 [gold]덱[/gold]에서 제거됩니다.
- 페이지 요약: HOLD_ON_0: [jitter]당신은 버티고 있습니다![/jitter] | HOLD_ON_1: 계속... 버티고... [jitter]있습니다아아!!!![/jitter] | HOLD_ON_2: [sine]아아아아아!?!!![/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLIPPERY_BRIDGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SlipperyBridge`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재판

- ID: `megacrit-sts2-core-models-events-trial`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 거대한 건물로 들어가는 사람들의 줄에 합류합니다.

[gold]황금빛 아치[/gold]를 지나자 나팔 소리가 요란하게 울려 퍼지고 [jitter]색종이가 터져 나오며[/jitter], 천장에서는 [sine]장식용 리본이 흩날립니다[/sine]!

“참가 번호 [blue]{EntrantNumber}[/blue]번, 오늘 [gold]재판[/gold]의 [green]결정자[/green]는 당신입니다.”
- 확인된 선택지:
  - 거절한다: [red]거절할 수 없습니다.[/red]
  - 수락한다: 오늘의 결정자 역할을 맡습니다.
  - 수락한다: 굴복합니다. 오늘의 결정자 역할을 맡습니다.
  - 완강하게 거절한다: [red]치명적인 결과를 맞이합니다.[/red]
  - 판결: 무죄: [gold]덱[/gold]에 [red]수치[/red]를 추가합니다. 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다.
  - 판결: 무죄: [gold]덱[/gold]에 [red]후회[/red]를 추가합니다. [gold]골드[/gold]를 [blue]300[/blue] 얻습니다.
  - 판결: 무죄: [gold]덱[/gold]에 [red]의심[/red]을 추가합니다. 카드를 [blue]2[/blue]장 [gold]변화[/gold]시킵니다.
  - 판결: 유죄: [gold]덱[/gold]에 [red]후회[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
- 부가 메모: 오늘의 결정자 역할을 맡습니다. | [red]거절할 수 없습니다.[/red] | [gold]덱[/gold]에 [red]후회[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다. | [gold]덱[/gold]에 [red]수치[/red]를 추가합니다. 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다.
- 페이지 요약: INITIAL: 당신은 거대한 건물로 들어가는 사람들의 줄에 합류합니다. [gold]황금빛 아치[/gold]를 지나자 나팔 소리가 요란하게 울려 퍼지고 [jitter]색종이가 터져 나오며[/jitter], 천장에서는 [sine]장식용 리본이 흩날립니다[/sine]! “참가 번호 [blue]{EntrantNumber}[/blue]번, 오늘 [gold]재판[/gold]의 [green]결정자[/green]는 당신입니다.” | MERCHANT: [green]부유해 보이는 상인[/green]이 사건의 경위를 진술합니다. 그는 경쟁자 중 한 명에게 [red]살인[/red] 혐의로 기소되었습니다. 증거는 빈약하고, 당신은 그가 결백하다는 듯한 느낌을 받았지만, 방청객들은 피의 심판을 원하고 있는 것으로 보입니다. | MERCHANT_GUILTY: 당신은 그 남자의 소유물을 모두 압수해 재판 소송 비용으로 쓰도록 판결합니다. 방청객들은 환호의 함성을 지릅니다!
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRIAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 웡고스에 오신 것을 환영합니다

- ID: `megacrit-sts2-core-models-events-welcometowongos`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: “웡고스에 오신 걸 환영합니다. 원하시는 상품을 웡타스틱한 가격으로 제공합니다.” 당신이 여태 만난 점원 중 가장 무기력한 점원이 말합니다.

“상품들을 둘러보시고 웡고스에서의 시간을 즐겨주세요.” 그는 고개조차 들지 않은 채 말을 이어갑니다.
- 확인된 선택지:
  - 떠난다: 무작위 카드 1장이 [red]열화[/red]됩니다.
  - 웡고스 랜덤 박스: [gold]골드[/gold]를 [red]{MysteryBoxCost}[/red] 지불합니다. [blue]{MysteryBoxCombatCount}[/blue]번의 전투 후에 무작위 [gold]유물[/gold]을 [blue]{MysteryBoxRelicCount}[/blue]개 얻습니다.
  - 웡고스 추천 상품: [gold]골드[/gold]를 [red]{FeaturedItemCost}[/red] 지불합니다. [gold]{RandomRelic}[/gold]을(를) 얻습니다.
  - 웡고스 할인 코너: [gold]골드[/gold]를 [red]{BargainBinCost}[/red] 지불합니다. 무작위 [gold]일반 유물[/gold]을 [blue]1[/blue]개 얻습니다.
  - 잠김: [gold]골드[/gold]가 [blue]{BargainBinCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{FeaturedItemCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{MysteryBoxCost}[/blue] 필요합니다.
- 부가 메모: [gold]골드[/gold]를 [red]{BargainBinCost}[/red] 지불합니다. 무작위 [gold]일반 유물[/gold]을 [blue]1[/blue]개 얻습니다. | [gold]골드[/gold]가 [blue]{BargainBinCost}[/blue] 필요합니다. | [gold]골드[/gold]를 [red]{FeaturedItemCost}[/red] 지불합니다. [gold]{RandomRelic}[/gold]을(를) 얻습니다. | [gold]골드[/gold]가 [blue]{FeaturedItemCost}[/blue] 필요합니다.
- 페이지 요약: AFTER_BUY: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. [blue]{RemainingWongoPointAmount}[/blue]포인트를 더 모으시면, 저희 점포의 특별한 [gold]웡고스 고객 감사 배지[/gold]를 받으실 수 있습니다. 웡타스틱한 하루 되세요.” 점원이 무슨 얘기를 하고 있는지는 모르겠지만, 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다. | AFTER_BUY_BADGE_COUNTER: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. [blue]{RemainingWongoPointAmount}[/blue]포인트를 더 모으시면, 저희 점포의 특별한 [gold]웡고스 고객 감사 배지[/gold]를 받으실 수 있습니다. 웡타스틱한 하루 되세요.” 당신은 이미 이 쓸모없는 배지를 [blue]{TotalWongoBadgeAmount}[/blue]개나 받았습니다. 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다. | AFTER_BUY_RECEIVE_BADGE: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. 포인트 누적을 통해 [gold]웡고스 고객 감사 배지[/gold]가 수여되었습니다.” 점원은 조악하게 만들어진 배지 하나를 당신에게 건냅니다. “웡타스틱한 하루 되세요.” 현재까지 받으신 배지는 총 [blue]{TotalWongoBadgeAmount}[/blue]개입니다. 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WELCOME_TO_WONGOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 자기계발서

- ID: `megacrit-sts2-core-models-events-selfhelpbook`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 바닥에는 책 한 권이 놓여있습니다.
“첨탑을 정복하기 위한 3가지 방법”

책의 표면은 [red]피로 뒤덮여[/red] 있었기 때문에, 굉장히 수상쩍은 느낌이 듭니다.

읽어보시겠습니까?
- 확인된 선택지:
  - 나아간다: 인챈트할 수 있는 카드가 없습니다.
  - 뒷면을 읽는다: 공격 카드를 1장 선택해 [purple]{Enchantment1}[/purple]를 [blue]{Enchantment1Amount}[/blue] [gold]인챈트[/gold]합니다.
  - 무작위 문단을 읽는다: 스킬 카드를 1장 선택해 [purple]{Enchantment2}[/purple]을 [blue]{Enchantment2Amount}[/blue] [gold]인챈트[/gold]합니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 파워 카드가 없습니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 스킬 카드가 없습니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 공격 카드가 없습니다.
  - 책을 모두 읽는다: 파워 카드를 1장 선택해 [purple]{Enchantment3}[/purple]을 [blue]{Enchantment3Amount}[/blue] [gold]인챈트[/gold]합니다.
- 부가 메모: 인챈트할 수 있는 카드가 없습니다. | 파워 카드를 1장 선택해 [purple]{Enchantment3}[/purple]을 [blue]{Enchantment3Amount}[/blue] [gold]인챈트[/gold]합니다. | [gold]인챈트[/gold]할 수 있는 파워 카드가 없습니다. | 스킬 카드를 1장 선택해 [purple]{Enchantment2}[/purple]을 [blue]{Enchantment2Amount}[/blue] [gold]인챈트[/gold]합니다.
- 페이지 요약: INITIAL: 바닥에는 책 한 권이 놓여있습니다. “첨탑을 정복하기 위한 3가지 방법” 책의 표면은 [red]피로 뒤덮여[/red] 있었기 때문에, 굉장히 수상쩍은 느낌이 듭니다. 읽어보시겠습니까? | NO_OPTIONS: 그냥 책도 딱히 좋아하지도 않는데, 피로 뒤덮인 책이라면 말할 것도 없습니다. | READ_ENTIRE_BOOK: 정말 흥미진진한 읽을거리입니다! 인상적인 문구 몇 개가 여전히 머릿속에 선명하게 남아 있습니다: “거대한 몬스터는 피하세요.” “모든 카드를 집으세요.” “방어도는 겁쟁이나 쌓는 겁니다.” 당신의 두 손이 [red]피투성이[/red]가 됩니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SELF_HELP_BOOK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 진리의 석판

- ID: `megacrit-sts2-core-models-events-tabletoftruth`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 두 명의 [blue]혈족 수호자[/blue]를 쓰러뜨리고, 그들이 지키고 있던 보관실 안으로 들어갑니다.

방 안에는 [purple][sine]익숙한 문양이 새겨진 석판[/sine][/purple]이 놓여 있습니다. 직감적으로, 이 언어를 그리 어렵지 않게 해독할 수 있을 것 같은 느낌이 듭니다... 하지만, 한 번 해독을 시작하면 멈출 수 없을 것입니다.

석판에서는 차분한 기운이 흘러나오고 있으며, 해독하는 대신 부숴 석판의 [green]치유의 기운[/green]을 얻을 수도 있습니다.
- 확인된 선택지:
  - 계속 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 계속 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 모든 것을 잃는다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 모든 카드를 [gold]강화[/gold]합니다.
  - 부순다: 체력을 [green]{SmashHPGain}[/green] 회복합니다.
  - 포기한다: 글을 읽는 것을 멈추고 떠납니다.
  - 해독을 이어간다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
- 부가 메모: 글을 읽는 것을 멈추고 떠납니다. | 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다. | 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 모든 카드를 [gold]강화[/gold]합니다. | 체력을 [green]{SmashHPGain}[/green] 회복합니다.
- 페이지 요약: DECIPHER_1: 당신이 석판을 해독하기 시작하자, [blue]혈족의 만트라[/blue]가 당신에게 말을 걸어옵니다. “[gold]진리[/gold]는 모든 것이다...” | DECIPHER_2: 석판을 읽다 보니 어지러워지는 듯한 느낌이 듭니다! [purple][sine]영웅은 이렇게 오랫동안 글을 읽도록 만들어진 존재가 아닐 텐데...[/sine][/purple] | DECIPHER_3: [sine][red]당신은 간신히 버티고 있습니다...[/red][/sine] 석판의 해독이 거의 끝나갑니다. [blue]만트라[/blue]는 진리를 품고 있습니다… [jitter]진리!! 대체 진리가 뭐죠!?[/jitter]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TABLET_OF_TRUTH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거대한 꽃

- ID: `megacrit-sts2-core-models-events-colossalflower`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [red][jitter]산처럼 쌓인 뼈무더기[/jitter][/red] 꼭대기에, [green]거대한 꽃[/green] 하나가 자라나 있습니다.

[sine][rainbow freq=0.3 sat=0.8 val=1]색을 바꾸는 꽃잎들[/rainbow][/sine]의 맥동 사이 중심부에 [aqua]막대한 힘을 지닌 꽃가루 덩어리[/aqua]가 존재한다는 것이 느껴지지만, 요동치는 꽃잎들은 [red]아주 날카롭고[/red], 그 움직임을 도저히 예측할 수 없습니다.

[gold]황금빛 꿀[/gold]만 조금 집어갈 수도 있겠지만, 중심부에 있는 값진 보상에 손을 뻗고 싶다는 마음이 가라앉지 않습니다...
- 확인된 선택지:
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize1}[/blue] 얻습니다.
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize2}[/blue] 얻습니다.
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize3}[/blue] 얻습니다.
  - 더 깊게 다가간다: 더 깊게 들어갑니다. 체력을 [red]5[/red] 잃습니다.
  - 더 깊게 다가간다: 훨씬 더 깊게 들어갑니다. 체력을 [red]6[/red] 잃습니다.
  - 중심부로 들어간다: 체력을 [red]7[/red] 잃습니다. [gold]꽃가루 핵[/gold]을 얻습니다.
- 부가 메모: [gold]골드[/gold]를 [blue]{Prize1}[/blue] 얻습니다. | 더 깊게 들어갑니다. 체력을 [red]5[/red] 잃습니다. | [gold]골드[/gold]를 [blue]{Prize2}[/blue] 얻습니다. | 훨씬 더 깊게 들어갑니다. 체력을 [red]6[/red] 잃습니다.
- 페이지 요약: EXTRACT_CURRENT_PRIZE: 당신은 꽃의 표피에서 약간의 [gold]꿀[/gold]을 조심스럽게 채취합니다. 꽃잎들이 분노한 듯이 반짝거립니다. | EXTRACT_INSTEAD: 당신은 온몸의 감각을 잃고 [purple]독소[/purple]와 [red]가시[/red], [green]잎사귀[/green]들로 인해 엉망이 되었습니다. 이 정도면 충분한 것 같네요. 당신은 [gold]품질 좋은 꿀[/gold]을 챙겨 넣은 뒤, 서둘러 물러납니다. | INITIAL: [red][jitter]산처럼 쌓인 뼈무더기[/jitter][/red] 꼭대기에, [green]거대한 꽃[/green] 하나가 자라나 있습니다. [sine][rainbow freq=0.3 sat=0.8 val=1]색을 바꾸는 꽃잎들[/rainbow][/sine]의 맥동 사이 중심부에 [aqua]막대한 힘을 지닌 꽃가루 덩어리[/aqua]가 존재한다는 것이 느껴지지만, 요동치는 꽃잎들은 [red]아주 날카롭고[/red], 그 움직임을 도저히 예측할 수 없습니다. [gold]황금빛 꿀[/gold]만 조금 집어갈 수도 있겠지만, 중심부에 있는 값진 보상에 손을 뻗고 싶다는 마음이 가라앉지 않습니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLOSSAL_FLOWER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 다채로운 철학자들

- ID: `megacrit-sts2-core-models-events-colorfulphilosophers`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신의 눈앞에 꽤나 멋진 광경이 펼쳐집니다.
당신은 우뚝 서있는 세 가지 색의 동상들이 단상에 서서, 색채의 철학적 의미에 관해 [jitter][red]열띤 토론[/red][/jitter]을 벌이고 있는 모습을 목격합니다.

논쟁을 듣고 있자니, 현재 그들에게 있어 가장 중요한 의문은 진실로 [gold]가장 위대한[/gold] 색은 어떤 색인가에 관한 것으로 보입니다.

당신도 스스로 생각하는 바를 그들에게 말합니다.
- 확인된 선택지:
  - 분홍색: 네크로바인더 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 빨간색: 아이언클래드 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 주황색: 리젠트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 초록색: 사일런트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 파란색: 디펙트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 평등: [gold]프리즘 조각[/gold]을 얻습니다.
- 부가 메모: 디펙트 카드를 [blue]{Cards}[/blue]장 얻습니다. | [gold]프리즘 조각[/gold]을 얻습니다. | 아이언클래드 카드를 [blue]{Cards}[/blue]장 얻습니다. | 네크로바인더 카드를 [blue]{Cards}[/blue]장 얻습니다.
- 페이지 요약: DONE: 당신의 의견은 받아들여지지 않는 것으로 보이며, 동상들은 끝없는 논쟁을 이어 나갑니다. | INITIAL: 당신의 눈앞에 꽤나 멋진 광경이 펼쳐집니다. 당신은 우뚝 서있는 세 가지 색의 동상들이 단상에 서서, 색채의 철학적 의미에 관해 [jitter][red]열띤 토론[/red][/jitter]을 벌이고 있는 모습을 목격합니다. 논쟁을 듣고 있자니, 현재 그들에게 있어 가장 중요한 의문은 진실로 [gold]가장 위대한[/gold] 색은 어떤 색인가에 관한 것으로 보입니다. 당신도 스스로 생각하는 바를 그들에게 말합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLORFUL_PHILOSOPHERS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColorfulPhilosophers`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포션의 미래?

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalspherecell`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모퉁이를 도는 순간 희미한 진동이 느껴지고, 거대한 회전 장치가 당신의 눈앞에 모습을 드러냅니다!
여러 개의 투입구가 있어, 액체를 고도로 압축해 섭취할 수 있는 알약으로 바꾸는 구조의 장치인 것으로 보입니다.

이 건강 음료들을 작은 알약 한 알로 바꾼다는 발상은 영 꺼림칙하지만, 가끔은 새로운 시도를 해보는 것도 나쁘지 않을지도 모릅니다.
- 확인된 선택지:
  - {Rarity} 포션을 삽입한다: [gold]{Potion}[/gold]을(를) 잃습니다. [gold]강화된[/gold] [gold]{Rarity} {Type}[/gold] 카드를 얻습니다.
  - 뒤져본다: 무작위 [gold]고급 포션[/gold]을 [blue]1[/blue]개 생성합니다.
  - 아래의 것을 가져간다: [gold]{BottomRelicOwned}[/gold]을(를) [gold]{BottomRelicNew}[/gold](와)과 거래합니다.
  - 위의 것을 가져간다: [gold]{TopRelicOwned}[/gold]을(를) [gold]{TopRelicNew}[/gold](와)과 거래합니다.
  - 중간 것을 가져간다: [gold]{MiddleRelicOwned}[/gold]을(를) [gold]{MiddleRelicNew}[/gold](와)과 거래합니다.
  - 포션을 집는다: [gold]역겨운 포션[/gold]을 [blue]{FoulPotions}[/blue]개 생성합니다.
- 부가 메모: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요... | PLACEHOLDER: 시체랑은 거래하지 않아요.
- 페이지 요약: DONE: 알약은 역겨운 맛이 났습니다. 미래에는 희망이 없네요. | INITIAL: 모퉁이를 도는 순간 희미한 진동이 느껴지고, 거대한 회전 장치가 당신의 눈앞에 모습을 드러냅니다! 여러 개의 투입구가 있어, 액체를 고도로 압축해 섭취할 수 있는 알약으로 바꾸는 구조의 장치인 것으로 보입니다. 이 건강 음료들을 작은 알약 한 알로 바꾼다는 발상은 영 꺼림칙하지만, 가끔은 새로운 시도를 해보는 것도 나쁘지 않을지도 모릅니다.
- 대사 프리뷰: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_FUTURE_OF_POTIONS`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereCell`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 물에 잠긴 기록실

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 어두컴컴한 통로를 헤쳐 나아가던 중, 작은 가게에서 일하고 있는 [aqua]수척한 인물[/aqua]을 우연히 발견합니다. 무수히 많은 선반에는 눅눅한 두루마리와 양피지가 [sine]빽빽하게[/sine] 쌓여 있습니다.

손님이 들어왔다는 것을 알아차린 [aqua]필경사 상인[/aqua]은 잽싸게 자세를 고쳐 앉고, 책상 위에 놓인 몇몇 [gold]도구들[/gold]을 가리킵니다.
- 확인된 선택지:
  - 꺼끌꺼끌한 스펀지: [gold]골드[/gold]를 [red]{PricklySpongeGold}[/red] 지불합니다. 카드 [blue]{Cards}[/blue]장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{PricklySpongeGold}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{Gold}[/blue] 필요합니다.
  - 촉수 펜: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 카드 1장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다.
  - 핏빛 잉크: 최대 체력을 [green]6[/green] 얻습니다.
- 부가 메모: 최대 체력을 [green]6[/green] 얻습니다. | [gold]골드[/gold]를 [red]{PricklySpongeGold}[/red] 지불합니다. 카드 [blue]{Cards}[/blue]장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다. | [gold]골드[/gold]가 [blue]{PricklySpongeGold}[/blue] 필요합니다. | [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 카드 1장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다.
- 페이지 요약: BLOODY_INK: [aqua]필경사[/aqua]는 [red]밝은 붉은빛의 잉크[/red]가 담긴 병을 조심스레 집어 들고서 한 번 흔든 뒤, 자신의 손을 병 안에 담그는 듯한 동작을 취합니다. 당신이 손가락을 잉크에 담그자, [green][jitter]강인한 생명력[/jitter][/green]이 당신의 온몸을 꿰뚫고 지나갑니다! 당신에 반응에 만족한 [aqua]필경사[/aqua]는, 정중하게 고개를 숙인 뒤 손짓하며 당신을 배웅합니다. | INITIAL: 당신은 어두컴컴한 통로를 헤쳐 나아가던 중, 작은 가게에서 일하고 있는 [aqua]수척한 인물[/aqua]을 우연히 발견합니다. 무수히 많은 선반에는 눅눅한 두루마리와 양피지가 [sine]빽빽하게[/sine] 쌓여 있습니다. 손님이 들어왔다는 것을 알아차린 [aqua]필경사 상인[/aqua]은 잽싸게 자세를 고쳐 앉고, 책상 위에 놓인 몇몇 [gold]도구들[/gold]을 가리킵니다. | PRICKLY_SPONGE: [aqua]필경사[/aqua]는 [gold]꺼끌꺼끌한 스펀지[/gold]를 움켜쥐고 (삐걱대는 소리가 납니다) 당신의 두루마리를 가볍게 두드립니다. [jitter][blue]*삐걱 삐걱 삐걱*[/blue][/jitter] 스펀지의 꺼끌꺼끌한 감촉이 [purple][sine]반짝이는 자국[/sine][/purple]을 남기며, 잉크를 안정시키고 있는 것으로 보입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WATERLOGGED_SCRIPTORIUM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 장로 랜위드

- ID: `megacrit-sts2-core-models-events-ranwidtheelder`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신이 [purple]지금까지 본 사람 중 가장 나이가 많은 사람[/purple]이 당신에게 다가옵니다.
[sine]“또 만났네요... 저잖아요, 랜위드!”[/sine]

당신은 이 사람을 모릅니다.
- 확인된 선택지:
  - {Potion}을(를) 준다: 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - {Relic}을(를) 준다: 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
  - 골드를 {Gold} 준다: 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 잠김: 줄 수 있는 포션이 없습니다.
  - 잠김: 줄 수 있는 유물이 없습니다.
- 부가 메모: 무작위 [gold]유물[/gold]을 1개 얻습니다. | 줄 수 있는 포션이 없습니다. | 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다. | 줄 수 있는 유물이 없습니다.
- 페이지 요약: GOLD: [sine]“엄청.. 나네요...”[/sine] 랜위드는 [gold]골드[/gold]를 씹으며 중얼거립니다. | INITIAL: 당신이 [purple]지금까지 본 사람 중 가장 나이가 많은 사람[/purple]이 당신에게 다가옵니다. [sine]“또 만났네요... 저잖아요, 랜위드!”[/sine] 당신은 이 사람을 모릅니다. | POTION: [sine]“절묘하네요...”[/sine] [sine][blue]꿀꺽 꿀꺽 꿀꺽[/blue][/sine] 그는 [gold]{Potion}[/gold] 한 병을 한 입에 모두 마셨습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RANWID_THE_ELDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 차의 명인

- ID: `megacrit-sts2-core-models-events-teamaster`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 불빛이 희미하게 비치는 오두막에 우연히 들어간 당신은, 기이할 정도로 다양한 차들로 가득 찬 너무나도 평온한 공간에 빠져듭니다.

오두막 안에 있던 남자는 아무 말 없이 하나 같이 [gold]고가[/gold]의 가격표가 붙어있는 다양한 [green]차[/green]들을 가리킵니다.
- 확인된 선택지:
  - 무례함의 차
  - 뼈다귀 차: [gold]골드[/gold]를 [red]{BoneTeaCost}[/red] 지불합니다. {BoneTeaDescription}
  - 잉걸불 차: [gold]골드[/gold]를 [red]{EmberTeaCost}[/red] 지불합니다. {EmberTeaDescription}
  - 잠김: [gold]골드[/gold]가 [blue]{BoneTeaCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{EmberTeaCost}[/blue] 필요합니다.
- 부가 메모: [gold]골드[/gold]를 [red]{BoneTeaCost}[/red] 지불합니다. {BoneTeaDescription} | [gold]골드[/gold]가 [blue]{BoneTeaCost}[/blue] 필요합니다. | [gold]골드[/gold]를 [red]{EmberTeaCost}[/red] 지불합니다. {EmberTeaDescription} | [gold]골드[/gold]가 [blue]{EmberTeaCost}[/blue] 필요합니다.
- 페이지 요약: DONE: [blue]차의 명인[/blue]은 당신의 요청을 받아들이고, 곧바로 준비를 시작합니다. [gold]1.[/gold] 숯이 채워진 물병에 든 물을 주전자에 붓습니다. [gold]2.[/gold] 적절한 [purple]보랏빛 불꽃[/purple]으로 주전자를 가열합니다. [gold]3.[/gold] 물이 끓기 전에, 찻잎 한 스푼을 주전자에 넣습니다. [gold]4.[/gold] 불을 끄고, 리드미컬한 박수로 시간을 잽니다. [gold]5.[/gold] 주전자를 높게 들어올린 뒤, 자그마한 컵에 차를 정확히 부어 넣습니다. [green][sine]“차 나왔습니다.”[/sine][/green] | INITIAL: 불빛이 희미하게 비치는 오두막에 우연히 들어간 당신은, 기이할 정도로 다양한 차들로 가득 찬 너무나도 평온한 공간에 빠져듭니다. 오두막 안에 있던 남자는 아무 말 없이 하나 같이 [gold]고가[/gold]의 가격표가 붙어있는 다양한 [green]차[/green]들을 가리킵니다. | TEA_OF_DISCOURTESY: 당신은 정중하게 차를 거절했고— [jitter][red]*쾅*[/red][/jitter] 뒤에 있던 문이 쾅하고 닫힙니다. [blue]차의 명인[/blue]은 잠시 동안 당신을 쳐다보다, 흉측하게 생긴 컵 하나를 당신에게 선물합니다. 오, 이 차는 무례한 사람들에게 내어지는 차입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TEA_MASTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포션 배달원

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspherecurse`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공기 중에 퍼진 [sine]시큼한[/sine] 냄새를 깨달은 당신은, 이내 땅에 쓰러진 [gold]포션 배달원[/gold]을 발견했습니다.
미동도 하지 않고, 생기도 없습니다. [jitter]죽은 걸까요!?[/jitter]
그의 소지품은 이미 전부 빼앗긴 상태였지만, [green][sine]역겨운 악취를 풍기는 포션[/sine][/green] 한 묶음과 함께 쪽지 하나가 남아 있습니다: "수령인: 상인".
- 확인된 선택지:
  - 뒤져본다: 무작위 [gold]고급 포션[/gold]을 [blue]1[/blue]개 생성합니다.
  - 아래의 것을 가져간다: [gold]{BottomRelicOwned}[/gold]을(를) [gold]{BottomRelicNew}[/gold](와)과 거래합니다.
  - 위의 것을 가져간다: [gold]{TopRelicOwned}[/gold]을(를) [gold]{TopRelicNew}[/gold](와)과 거래합니다.
  - 중간 것을 가져간다: [gold]{MiddleRelicOwned}[/gold]을(를) [gold]{MiddleRelicNew}[/gold](와)과 거래합니다.
  - 포션을 집는다: [gold]역겨운 포션[/gold]을 [blue]{FoulPotions}[/blue]개 생성합니다.
- 부가 메모: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요... | PLACEHOLDER: 시체랑은 거래하지 않아요.
- 페이지 요약: GRAB_POTIONS: [blue]상인[/blue]은 이 포션들로 대체 뭘 하려는 걸까요? | INITIAL: 공기 중에 퍼진 [sine]시큼한[/sine] 냄새를 깨달은 당신은, 이내 땅에 쓰러진 [gold]포션 배달원[/gold]을 발견했습니다. 미동도 하지 않고, 생기도 없습니다. [jitter]죽은 걸까요!?[/jitter] 그의 소지품은 이미 전부 빼앗긴 상태였지만, [green][sine]역겨운 악취를 풍기는 포션[/sine][/green] 한 묶음과 함께 쪽지 하나가 남아 있습니다: "수령인: 상인". | RANSACK: 온전한 물약은 한 개뿐이었습니다.
- 대사 프리뷰: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_COURIER`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems.CrystalSphereCurse`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고요를 짜는 거미

- ID: `megacrit-sts2-core-models-events-zenweaver`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 거미줄에 걸려, [jitter][red]공황[/red][/jitter] 상태에 빠졌습니다!
하지만 곧, [sine][blue]평온의 파도[/blue][/sine]가 당신을 덮쳐옵니다...

[sine]“진정하세요. 제 거미줄을 어지럽히지 말아주시죠.”[/sine]

어떻게 이렇게 빨리 진정할 수 있었던 걸까요?
[orange]아주 작은[/orange] 거미가 당신을 보기 위해 아래로 내려옵니다. [gold]골드[/gold]를 준다면 몇 가지 기술을 기꺼이 전수해 줄 것 같습니다.
- 확인된 선택지:
  - 감정의 인식: [gold]골드[/gold]를 [red]{EmotionalAwarenessCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다.
  - 거미류 침술: [gold]골드[/gold]를 [red]{ArachnidAcupunctureCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다.
  - 잠김: [gold]골드[/gold]가 부족합니다.
  - 호흡법: [gold]골드[/gold]를 [red]{BreathingTechniquesCost}[/red] 지불합니다. [gold]덱[/gold]에 [gold]계몽[/gold]을 [blue]2[/blue]장 추가합니다.
- 부가 메모: [gold]골드[/gold]를 [red]{ArachnidAcupunctureCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다. | [gold]골드[/gold]를 [red]{BreathingTechniquesCost}[/red] 지불합니다. [gold]덱[/gold]에 [gold]계몽[/gold]을 [blue]2[/blue]장 추가합니다. | [gold]골드[/gold]를 [red]{EmotionalAwarenessCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다. | [gold]골드[/gold]가 부족합니다.
- 페이지 요약: ARACHNID_ACUPUNCTURE: [sine]“움직이지... 마세요...”[/sine] [orange]조그마한 거미[/orange]가 중얼거리자 거미의 앞다리가 [aqua][jitter]빛나기[/jitter][/aqua] 시작합니다. 그리고— [jitter]날카롭게 몰아치는 찌르기[/jitter] 동작으로, 당신의 몸을 찔러댑니다! 당신의 몸이 깃털처럼 가벼워집니다. 돈을 낼 만한 가치가 있었네요. | BREATHING_TECHNIQUES: [sine]“기도가 계속 열려 있는 모습을 상상해 보세요... 모든 호흡에는 의미가 있습니다.”[/sine] 당신은 스스로가 이해할 수 있는 방식으로 이 기술을 연마합니다. | EMOTIONAL_AWARENESS: [orange]콩알만 한 거미[/orange]가 [sine][purple]최면을 거는 듯한 춤을 추며[/purple][/sine] 반복적으로 외치기 시작합니다. [sine]“생각은 현실에 영향을 주지 않는다. 공포는 생각을 죽인다. 계획을 따르라.”[/sine] 당신은 거미의 말을 이해합니다. [orange]조그만 거미[/orange] 한 마리가 던진 묵직한 한마디였네요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ZEN_WEAVER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ZenWeaver`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나무 조각

- ID: `megacrit-sts2-core-models-events-woodcarvings`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 먼지가 쌓인 상자를 열고, 정교한 형태의 나무 조각 3개를 발견했습니다:

새, 뱀, 그리고... 고리?

근처에 있던 받침대에는 조각의 바닥 부분과 일치하는 움푹 들어간 흔적이 있습니다. 받침대 위에 어떤 조각을 올려놓을까요?
- 확인된 선택지:
  - 고리: 시작 카드를 [blue]1[/blue]장 선택해 [gold]{ToricCard}[/gold]으로 [gold]변화[/gold]시킵니다.
  - 뱀: 카드 [blue]1[/blue]장에 [purple]{SnakeEnchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 새: 시작 카드를 [blue]1[/blue]장 선택해 [gold]{BirdCard}[/gold]로 [gold]변화[/gold]시킵니다.
  - 잠김: [purple]미끈거림[/purple]을 인챈트할 수 있는 카드가 없습니다.
- 부가 메모: 시작 카드를 [blue]1[/blue]장 선택해 [gold]{BirdCard}[/gold]로 [gold]변화[/gold]시킵니다. | 카드 [blue]1[/blue]장에 [purple]{SnakeEnchantment}[/purple]을 [gold]인챈트[/gold]합니다. | [purple]미끈거림[/purple]을 인챈트할 수 있는 카드가 없습니다. | 시작 카드를 [blue]1[/blue]장 선택해 [gold]{ToricCard}[/gold]으로 [gold]변화[/gold]시킵니다.
- 페이지 요약: BIRD: 새 조각을 받침대 위에 올려놓자 조각상의 눈이 밝게 빛나고, 당신의 머릿속에서 희미하게 어떤 소리가 울려 퍼집니다. [sine]...까악.... 까악 까아아아아악...[/sine] 받침대가 열리고, 이상한 액체가 쏟아져 나오기 시작합니다. 당연히, 당신은 그것을 모두 마셨습니다. | INITIAL: 당신은 먼지가 쌓인 상자를 열고, 정교한 형태의 나무 조각 3개를 발견했습니다: 새, 뱀, 그리고... 고리? 근처에 있던 받침대에는 조각의 바닥 부분과 일치하는 움푹 들어간 흔적이 있습니다. 받침대 위에 어떤 조각을 올려놓을까요? | SNAKE: 뱀 조각을 받침대 위에 올려놓자 조각상의 비늘이 밝게 빛나기 시작합니다! 당신의 약간의 온기와 편안함과 함께, 몸이 휘청거리는 느낌을 받았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WOOD_CARVINGS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 랜턴 열쇠

- ID: `megacrit-sts2-core-models-events-thelanternkey`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 희미하게 빛나는 열쇠를 발견하고, 열쇠를 집어 들기 위해 다가갑니다.

“그 열쇠를 찾으러 사방팔방 뒤지고 있었다네! 혹시 실례가 아니라면...”

조금 귀찮아 보이는 사람이긴 하지만, 함부로 단정 짓는 건 좋지 않을 것 같습니다.
- 확인된 선택지:
  - RETURN THE KEY: 낯선 사람은 당신의 협조에 감사를 표하며, 후하게 사례합니다.
  - 싸운다
  - 열쇠를 돌려준다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
  - 열쇠를 지킨다: 열쇠를 얻기 위해 싸웁니다.
- 부가 메모: 낯선 사람은 당신의 협조에 감사를 표하며, 후하게 사례합니다. | 열쇠를 얻기 위해 싸웁니다. | [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
- 페이지 요약: INITIAL: 당신은 희미하게 빛나는 열쇠를 발견하고, 열쇠를 집어 들기 위해 다가갑니다. “그 열쇠를 찾으러 사방팔방 뒤지고 있었다네! 혹시 실례가 아니라면...” 조금 귀찮아 보이는 사람이긴 하지만, 함부로 단정 짓는 건 좋지 않을 것 같습니다. | KEEP_THE_KEY: 낯선 사람은 살기 어린 시선으로 당신을 바라봅니다. 열쇠를 가지고 이곳을 떠날 수 있는 건 오직 한 명뿐인 것 같습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_LANTERN_KEY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLanternKey`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 심연의 욕탕

- ID: `megacrit-sts2-core-models-events-abyssalbaths`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 후미진 곳에 있는 방을 발견했습니다.

최면을 거는 듯한 리듬으로 색이 변하는 뜨거운 액체 웅덩이에서는 증기가 피어오르고 있습니다. 천장에는 따개비처럼 매달린 증식물들로부터, 바닥에 닿는 순간 치익 소리를 내며 부글거리는 끈적한 액체가 떨어지고 있습니다. 주변의 공기는 [blue]소금기[/blue]와 함께 [green]명백하게 유기적인[/green] 무언가로 가득 찬 채로, 무겁게 가라앉아 있습니다.

가장 큰 웅덩이의 가장자리로 다가가니, 액체에 파동이 일기 시작합니다. 당신이 들어올 것을 기대하기라도 한 듯 더 맹렬하게 거품이 일어납니다.
- 확인된 선택지:
  - 몸을 담근다: 최대 체력을 [green]{MaxHp}[/green] 얻습니다. 피해를 [red]{Damage}[/red] 받습니다.
  - 자제한다: 체력을 [green]{Heal}[/green] 회복합니다.
  - 좀 더 머문다: 최대 체력을 [green]{MaxHp}[/green] 얻습니다. 피해를 [red]{Damage}[/red] 받습니다.
  - 탕에서 나간다
- 부가 메모: 최대 체력을 [green]{MaxHp}[/green] 얻습니다. 피해를 [red]{Damage}[/red] 받습니다. | 체력을 [green]{Heal}[/green] 회복합니다.
- 페이지 요약: ABSTAIN: 당신은 물의 유혹을 뿌리치고서, 가장자리를 따라 결정화되어 있는 소금을 그러모았습니다. 소금 결정을 피부에 바르니, 웅덩이에서 파도가 일며 거품이 올라오기 시작합니다. 주변 기온이 눈에 띄게 떨어지고, 방 안의 모든 반사면에 공허한 눈을 지닌 왜소해 보이는 또 다른 당신의 모습이 잠시 비춰집니다. 완전히 새로운 자신이 될 수 있는 기회를 놓쳤다는 느낌이 머릿속을 떠나지 않습니다. | DEATH_WARNING: [sine][red]이 이상 목욕 시 죽게 될 것입니다.[/red][/sine] | EXIT_BATHS: 당신은 더 이상 열기를 버티지 못하고, 욕탕 밖으로 뛰쳐나왔습니다.
- 대사 프리뷰: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ABYSSAL_BATHS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영원의 돌

- ID: `megacrit-sts2-core-models-events-stoneofalltime`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 버려진 안뜰 한복판에 거대한 바위 하나가 놓여 있습니다.

명판에는 이렇게 적혀 있습니다:
[gold][b]영원의 돌[/b][/gold].

바위의 존재감과 형태, 그리고 장식들은, 이 안뜰이 돌의 주변을 따라 지어졌으며, [blue]돌[/blue]은 이곳에서 단 한 번도 움직인 적이 없음을 나타냅니다.
- 확인된 선택지:
  - 목을 축이고 들어올린다: [red]{DrinkRandomPotion}[/red]을(를) 잃습니다. 최대 체력을 [blue]{DrinkMaxHpGain}[/blue] 얻습니다.
  - 민다: 체력을 [red]{PushHpLoss}[/red] 잃습니다. 공격 카드 1장에 [purple]활력[/purple]을 [blue]{PushVigorousAmount}[/blue] [gold]인챈트[/gold]합니다.
  - 잠김: 포션이 필요합니다.
  - 잠김: 공격 카드가 필요합니다.
- 부가 메모: [red]{DrinkRandomPotion}[/red]을(를) 잃습니다. 최대 체력을 [blue]{DrinkMaxHpGain}[/blue] 얻습니다. | 포션이 필요합니다. | 체력을 [red]{PushHpLoss}[/red] 잃습니다. 공격 카드 1장에 [purple]활력[/purple]을 [blue]{PushVigorousAmount}[/blue] [gold]인챈트[/gold]합니다. | 공격 카드가 필요합니다.
- 페이지 요약: INITIAL: 버려진 안뜰 한복판에 거대한 바위 하나가 놓여 있습니다. 명판에는 이렇게 적혀 있습니다: [gold][b]영원의 돌[/b][/gold]. 바위의 존재감과 형태, 그리고 장식들은, 이 안뜰이 돌의 주변을 따라 지어졌으며, [blue]돌[/blue]은 이곳에서 단 한 번도 움직인 적이 없음을 나타냅니다. | LIFT: 당신은 [gold]{DrinkRandomPotion}[/gold] 병을 내려놓은 뒤, [jitter]온 힘을 다해 바위를 들어올렸습니다[/jitter]!! 아무 일도 일어나지 않았습니다. | PUSH: 당신은 가지고 있는 모든 힘을 쥐어짜내 바위를 [jitter]힘차게 밀었습니다[/jitter]!! 아무 일도 일어나지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STONE_OF_ALL_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.StoneOfAllTime`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 인형의 방

- ID: `megacrit-sts2-core-models-events-dollroom`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 숨겨진 방에 들어섭니다...

방 안은 다양한 인형들로 가득합니다. 모든 인형들은 저마다 [green]기쁨[/green]부터 [purple]슬픔[/purple]까지 다양한 표정을 띄고 있으며, 수세기의 시간과 당신이 알지 못하는 영역까지 아우르는 복장을 입고 있습니다.

인형들이 [sine][blue]입을 모아 속삭이는 소리[/blue][/sine]는 점점 커지더니, 이윽고 다른 인형보다 [jitter][red]더 크게 소리치려 하며[/red][/jitter] 당신을 부르기 시작합니다...

인형 중 하나를 선택하고 즉시 자리를 떠야 합니다.
- 확인된 선택지:
  - TAKE: [gold]{RelicName}[/gold]을(를) 받습니다.
  - 깊게 고민한 뒤 최고의 선택을 한다: 체력을 [red]{ExamineHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]3[/blue]개 중 [blue]1[/blue]개를 선택합니다.
  - 아무거나 고른다: 무작위 [gold]인형 유물[/gold]을 1개 얻습니다.
  - 잠시 고민한다: 체력을 [red]{TakeTimeHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]2[/blue]개 중 [blue]1[/blue]개를 선택합니다.
- 부가 메모: 체력을 [red]{ExamineHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]3[/blue]개 중 [blue]1[/blue]개를 선택합니다. | 무작위 [gold]인형 유물[/gold]을 1개 얻습니다. | 체력을 [red]{TakeTimeHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]2[/blue]개 중 [blue]1[/blue]개를 선택합니다. | [gold]{RelicName}[/gold]을(를) 받습니다.
- 페이지 요약: DAUGHTER_OF_WIND: [blue]“내가 널 골랐다는 걸 기뻐해야 해!”[/blue] 인형은 의기양양하게 선언합니다. | EXAMINE: 당신은 선택을 내리기 전에 모든 인형을 빠짐없이 살펴봅니다. 인형들의 비명 소리는 점점 커져 [red][jitter]귀청을 찢는 듯한 소리[/jitter][/red]가 됐지만, 당신은 완전히 미쳐버리기 전에 어떻게든 선택을 내렸습니다! | FABLE: 상냥한 목소리가 만족스럽게 당신의 머릿속을 채웁니다. [orange][sine]“우리 함께 가장 신나는 이야기들을 파헤쳐 보자!”[/sine][/orange]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOLL_ROOM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공생체

- ID: `megacrit-sts2-core-models-events-symbiote`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 길을 나아가던 도중 우연히, 형태가 일정치 않은 한 검은색 덩어리를 발견합니다. 당신은 그 덩어리가 굉장히 오래되었고, 극도로 사악한 존재라는 것을 느낄 수 있었습니다.

[sine]가까이 다가갑니다...[/sine]
- 확인된 선택지:
  - 다가간다: 공격 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 불로 태워 죽인다: 카드를 1장 선택해 [gold]변화[/gold]시킵니다.
  - 잠김: 인챈트할 수 있는 공격 카드가 없습니다.
- 부가 메모: 공격 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다. | 인챈트할 수 있는 공격 카드가 없습니다. | 카드를 1장 선택해 [gold]변화[/gold]시킵니다.
- 페이지 요약: APPROACH: 덩어리가 갑자기 당신에게 달려듭니다. 당신은 팔에 들러붙은 덩어리들로 인해 타들어가는 듯한 통증을 느꼈지만, 덩어리는 순식간에 사라져버렸습니다. ...? 어디로 사라진 걸까요? | INITIAL: 당신은 길을 나아가던 도중 우연히, 형태가 일정치 않은 한 검은색 덩어리를 발견합니다. 당신은 그 덩어리가 굉장히 오래되었고, 극도로 사악한 존재라는 것을 느낄 수 있었습니다. [sine]가까이 다가갑니다...[/sine] | KILL_WITH_FIRE: 당신은 검은색 덩어리에 횃불을 갖다 댑니다. 덩어리가 날카롭게 비명을 지르며 재로 변하는 동안 당신의 머리는 고통스럽게 울렸지만, 간신히 정신은 잃지 않았습니다. 대체 그건 뭐였을까요?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SYMBIOTE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Symbiote`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 길 잃은 위습

- ID: `megacrit-sts2-core-models-events-lostwisp`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]파워[/gold] 카드를 사용할 때마다, 모든 적에게 피해를 [blue]{Damage}[/blue] 줍니다.
- 확인된 선택지:
  - 위습을 붙잡는다: [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]{Relic}[/gold]을 얻습니다.
  - 잠김: 최대 체력이 너무 낮습니다.
  - 주변을 탐색한다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
- 부가 메모: [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]{Relic}[/gold]을 얻습니다. | 최대 체력이 너무 낮습니다. | [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
- 페이지 요약: CLAIM: 당신은 조심스럽게 [orange]위습[/orange]에게 다가갑니다... 가까이 다가갈수록, 위습은 당신의 존재에 점점 더 격하게 반응합니다. 당신은 손이 닿을 거리에서 [orange]위습[/orange]을 붙잡으려고 했지만, 위습은 [red][jitter]뜨거운 증기와 불꽃을 뿜어내 당신에게 화상을 입힙니다![/jitter][/red] 당신은 고통 속에서도 [jitter]허우적거리며 더듬어댄[/jitter] 끝에, 결국 그 생물을 굴복시킵니다. [orange]위습[/orange]은 결국 패배를 인정하고, 당신을 새로운 주인으로 받아들입니다. | INITIAL: 저 멀리 기이한 광경이 보입니다. [sine][red]죽은 곤충들의 사체[/red][/sine]가 산더미처럼 쌓여 있고, 그 한가운데에는 [orange]작은 빛나는 먼지[/orange]처럼 보이는 무언가가 있습니다. 그 먼지는 온갖 벌레를 효과적으로 유인하고 있는 것처럼 보였으나, 돌연 불꽃을 내뿜어대고 있습니다! 당신은 조사를 위해 더 가까이 다가갑니다. | SEARCH: 당신은 그 꼬마 친구를 내버려두기로 했습니다. 건드리지 않는 게 좋겠네요! 주변에 딱히 눈에 띄는 건 없지만, 죽은 벌레들 사이에 약간의 [gold]골드[/gold]가 흩어져 있는 것을 발견했습니다. 왜인지는 궁금해하지 않기로 합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LOST_WISP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LostWisp`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나선형 소용돌이

- ID: `megacrit-sts2-core-models-events-spiralingwhirlpool`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 우연히 [aqua][sine]거대한 나선형 소용돌이[/sine][/aqua]를 발견했습니다. 물은 아름답게 소용돌이치고, 나선형 무늬가 주변 벽을 수놓고 있습니다.

[purple][sine]빙글빙글 돌고...돌고...
또 돌고...정말 멋진 광경입니다......[/sine][/purple]

무엇을 할까요?
- 확인된 선택지:
  - 관찰한다: [gold]타격[/gold]이나 [gold]수비[/gold] 1장에 [purple]소용돌이[/purple]를 [gold]인챈트[/gold]합니다.
  - 마신다: 체력을 [green]{Heal}[/green] 회복합니다.
  - 손을 뻗는다: 무작위 [gold]유물[/gold]을 1개 얻습니다. [gold]덱[/gold]에 [red]몸부림[/red]을 추가합니다.
- 부가 메모: 체력을 [green]{Heal}[/green] 회복합니다. | [gold]타격[/gold]이나 [gold]수비[/gold] 1장에 [purple]소용돌이[/purple]를 [gold]인챈트[/gold]합니다. | 무작위 [gold]유물[/gold]을 1개 얻습니다. [gold]덱[/gold]에 [red]몸부림[/red]을 추가합니다.
- 페이지 요약: DRINK: 당신은 두 손을 모아 [sine][aqua]나선형 소용돌이[/aqua][/sine] 속으로 손을 담급니다. 물은 천천히 당신의 손을 채웠고, 당신은 눈을 감고 조금씩 음미하듯 물을 마십니다. 상쾌한 물이 [sine]소용돌이치듯[/sine] 당신의 [gold]중심[/gold]을 지나 흐르며, 마음속의 해로운 생각과 육체의 고통을 씻어냅니다... | INITIAL: 당신은 우연히 [aqua][sine]거대한 나선형 소용돌이[/sine][/aqua]를 발견했습니다. 물은 아름답게 소용돌이치고, 나선형 무늬가 주변 벽을 수놓고 있습니다. [purple][sine]빙글빙글 돌고...돌고... 또 돌고...정말 멋진 광경입니다......[/sine][/purple] 무엇을 할까요? | OBSERVE: [sine][purple]...아아, 정말 아름답네요..... 소용돌이는 돌고... 계속 돌고 있습니다. 그래요... 오! 방금..... 물이 튄 거였나요...? ...당연하죠. 당신도 함께 놀 자격이 있답니다,[/purple] [aqua]소용돌이 씨[/aqua][purple]...하하... ... ....... ...[/purple][/sine] 이제 가야 할 시간입니다. 당신은 자리를 떠나며 무언가 중얼거립니다. [gold]“고마워요, 소용돌이.”[/gold]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPIRALING_WHIRLPOOL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiralingWhirlpool`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난타전

- ID: `megacrit-sts2-core-models-events-punchoff`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]권투형 구조체[/gold] 두 마리가 서로 격렬하게 치고받고 있는 가운데, 둘 사이에 보물이 놓여 있는 모습이 보입니다...

슬쩍 낚아채 볼까요?
- 확인된 선택지:
  - 낚아챈다: [gold]덱[/gold]에 [red]상처[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 저 정도는 가뿐하지: 싸우고 [gold]더 좋은 보상[/gold]을 얻습니다.
  - 전투
- 부가 메모: 싸우고 [gold]더 좋은 보상[/gold]을 얻습니다. | [gold]덱[/gold]에 [red]상처[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: INITIAL: [gold]권투형 구조체[/gold] 두 마리가 서로 격렬하게 치고받고 있는 가운데, 둘 사이에 보물이 놓여 있는 모습이 보입니다... 슬쩍 낚아채 볼까요? | I_CAN_TAKE_THEM: [gold]구조체들[/gold]이 위협적으로 당신을 향해 돌아섭니다! 당신은 싸울 준비를 합니다. | NAB: 당신은 성공적으로 유물을 낚아챕니다! ...적어도 그렇게 생각했습니다. 오른손 훅이 [jitter][red]당신의 안면을 강타합니다[/red][/jitter].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PUNCH_OFF`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.PunchOff`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛나는 합창단

- ID: `megacrit-sts2-core-models-events-luminouschoir`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 우연히 부자연스러운 푸른 빛에 휩싸인 공터를 발견합니다. 높게 치솟은 버섯들은 마치 살아있는 듯이 빛나고 맥동하며, 볼록해진 갓은 반짝거리고 있습니다. 가까이 다가가자 버섯들은 [sine][purple]섬뜩하면서도 감미로운 선율[/purple][/sine]을 노래하기 시작하고, 그 울림은 당신의 가슴 속에 깊게 울려 퍼집니다.

당신은 가장 거대한 버섯의 살 속에 박혀 반짝이는 무언가를 발견했고, 그것은 버섯의 노랫소리에 박자를 맞추듯이 [sine]맥동하고[/sine] 있습니다.
- 확인된 선택지:
  - 공물을 바친다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 버섯의 살에 다가간다: [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다. [gold]덱[/gold]에 [red]포자 잠식[/red]을 추가합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{Gold}[/blue] 필요합니다.
- 부가 메모: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다. | [gold]골드[/gold]가 [blue]{Gold}[/blue] 필요합니다. | [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다. [gold]덱[/gold]에 [red]포자 잠식[/red]을 추가합니다.
- 페이지 요약: INITIAL: 당신은 우연히 부자연스러운 푸른 빛에 휩싸인 공터를 발견합니다. 높게 치솟은 버섯들은 마치 살아있는 듯이 빛나고 맥동하며, 볼록해진 갓은 반짝거리고 있습니다. 가까이 다가가자 버섯들은 [sine][purple]섬뜩하면서도 감미로운 선율[/purple][/sine]을 노래하기 시작하고, 그 울림은 당신의 가슴 속에 깊게 울려 퍼집니다. 당신은 가장 거대한 버섯의 살 속에 박혀 반짝이는 무언가를 발견했고, 그것은 버섯의 노랫소리에 박자를 맞추듯이 [sine]맥동하고[/sine] 있습니다. | OFFER_TRIBUTE: 물컹한 땅 위에 금화를 올려놓습니다. 유물을 깔끔하게 회수할 수 있도록 가느다란 촉수들이 유물을 지면으로 밀어올리는 동시에, 동전들이 서서히 가라앉습니다. | REACH_INTO_THE_FLESH: 당신은 가장 큰 버섯의 물컹한 조직 속으로 손을 밀어넣어, 안에 박혀 있는 유물을 꺼내려 합니다. 균근망은 당신의 침입으로 인해 몸서리치고, 그 반응에 당신의 정신도 함께 [sine]흔들립니다[/sine].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LUMINOUS_CHOIR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LuminousChoir`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 울창한 초목

- ID: `megacrit-sts2-core-models-events-densevegetation`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한참 동안 길을 잘못 든 채로 헤매다 보니, 당신은 [green]양치류[/green]와 [green]관목[/green], [green]덩굴[/green]이 뒤엉킨 울창한 정글에 들어왔다는 것을 깨닫습니다. 특히 [green]덩굴[/green]이 많네요. 피로가 몰려오고, 불길한 생각 하나가 머리를 스칩니다:

[sine][purple]“너는 길을 잃었고, 무방비하며, 피할 수 없는 죽음이 가까워지고 있다.”[/purple][/sine]

무엇을 할까요?
- 확인된 선택지:
  - 묵묵히 나아간다: [gold]덱[/gold]에서 카드를 1장 제거합니다. 체력을 [red]{HpLoss}[/red] 잃습니다.
  - 싸운다!
  - 휴식한다: 체력을 [green]{Heal}[/green] 회복합니다. [red]적[/red]들과 싸웁니다.
- 부가 메모: 체력을 [green]{Heal}[/green] 회복합니다. [red]적[/red]들과 싸웁니다. | [gold]덱[/gold]에서 카드를 1장 제거합니다. 체력을 [red]{HpLoss}[/red] 잃습니다.
- 페이지 요약: INITIAL: 한참 동안 길을 잘못 든 채로 헤매다 보니, 당신은 [green]양치류[/green]와 [green]관목[/green], [green]덩굴[/green]이 뒤엉킨 울창한 정글에 들어왔다는 것을 깨닫습니다. 특히 [green]덩굴[/green]이 많네요. 피로가 몰려오고, 불길한 생각 하나가 머리를 스칩니다: [sine][purple]“너는 길을 잃었고, 무방비하며, 피할 수 없는 죽음이 가까워지고 있다.”[/purple][/sine] 무엇을 할까요? | REST: 당신은 몰려오는 피로를 이기지 못하고, 잠시 잠에 듭니다... ...하지만 이내 몸 위에서 [gold][jitter]무언가가 꿈틀거리는 감각[/jitter][/gold]에 의해 잠에서 깨어납니다! | TRUDGE_ON: 당신은 밀림을 가르고 헤치며 나아갔지만, 숲은 끝없이 이어집니다... 당신은 [green]정체를 알 수 없는 과일[/green]과 [orange]바삭거리는 벌레[/orange]들로 허기를 달래고, [aqua]빛나는 식물[/aqua]의 꿀을 마시며 버텼지만, 점점 [jitter][red]편집적인 망상[/red][/jitter]이 떠오르기 시작합니다... 하지만 이윽고, 당신은 공터와 오솔길, 땅바닥에 그려진 지도 같은 그림들을 발견합니다. 문제가 해결됐네요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DENSE_VEGETATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DenseVegetation`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 원탁의 다과회

- ID: `megacrit-sts2-core-models-events-roundteaparty`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [blue]“갤러롯 경”[/blue]에게 온 다과회의 [orange]초대장[/orange]을 발견했고, 그를 대신해 파티에 참석하기로 했습니다.

비교적 소박해 보이는 원형 홀에 들어서자, 당신은 [gold]지휘관[/gold]과 [aqua]장군[/aqua], [red]군벌[/red], [blue]용병[/blue]들이 함께... 홍차를 마시고 있는 곳 한복판에 있다는 것을 알게 됐습니다.
[gold]황금 왕관[/gold]을 쓴 거대한 기사 한 명이 입을 엽니다.

[jitter]“[b]차[/b] 모임에 조금 늦으셨군요?!”[/jitter]
- 확인된 선택지:
  - 계속
  - 싸움을 건다: 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 차를 즐긴다: [red]{Relic}[/red]을 얻습니다. [green]체력을 모두 회복합니다.[/green]
- 부가 메모: [red]{Relic}[/red]을 얻습니다. [green]체력을 모두 회복합니다.[/green] | 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: CONTINUE_FIGHT: 파티의 참석자들은 놀란 눈으로 익숙한 일원의 시신을 바라봅니다... 그리고 [jitter]갑자기 박수를 치기 시작합니다!?[/jitter] “[red]와이고어 경[/red]을 어떻게 처리해야 할지 고민하고 있었는데, 정말 놀랍군요!” 그들은 당신의 [green]독이 든 차[/green]를 던져버렸고, 당신은 그들과 함께 밤새도록 [orange]전술[/orange]과 [purple]비밀[/purple] 이야기를 나눴습니다. 정말 멋진 다과회였네요. | ENJOY_TEA: 당신은 찻잔을 집어들고서 한 번에 들이켭니다. 홍차는 [jitter][red]미친듯이 뜨거웠지만[/red][/jitter], 당신은 표정 하나 바꾸지 않습니다. 파티의 참석자들은 당신의 자기 파괴적 행위에 놀라움을 금치 못합니다. 악의는 없었습니다, [blue]갤러롯 경[/blue]. 진심으로 사과드립니다.” 당신은 [green]독[/green]을 섭취했다는 것도 모른 채로, 홍차와 크럼핏을 즐겼습니다. | INITIAL: 당신은 [blue]“갤러롯 경”[/blue]에게 온 다과회의 [orange]초대장[/orange]을 발견했고, 그를 대신해 파티에 참석하기로 했습니다. 비교적 소박해 보이는 원형 홀에 들어서자, 당신은 [gold]지휘관[/gold]과 [aqua]장군[/aqua], [red]군벌[/red], [blue]용병[/blue]들이 함께... 홍차를 마시고 있는 곳 한복판에 있다는 것을 알게 됐습니다. [gold]황금 왕관[/gold]을 쓴 거대한 기사 한 명이 입을 엽니다. [jitter]“[b]차[/b] 모임에 조금 늦으셨군요?!”[/jitter]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROUND_TEA_PARTY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoundTeaParty`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잊힌 자의 무덤

- ID: `megacrit-sts2-core-models-events-graveoftheforgotten`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter][aqua]푸른 불꽃이 거세게 일렁이는[/aqua][/jitter] 무덤 하나가 있습니다...

긍지 높은 한 전사는 전투 중 사망했지만, 영혼은 아직 안식을 얻지 못했습니다. 그 영혼은 계속 싸우고 싶어하고, 당신과 함께 여행을 떠나고 싶다 [sine]애원하는[/sine] 듯합니다.

하지만 어쩌면, 이제는 그가 [red]진실[/red]을 마주해야 할 때일지도 모릅니다.
- 확인된 선택지:
  - {Relic}을 받아들인다: [gold]{Relic}[/gold]을 얻습니다.
  - 사실을 알린다: [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]소멸[/gold] 카드에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 잠김: [purple]인챈트[/purple]할 수 있는 [gold]소멸[/gold] 카드를 보유하고 있지 않습니다.
- 부가 메모: [gold]{Relic}[/gold]을 얻습니다. | [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]소멸[/gold] 카드에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다. | [purple]인챈트[/purple]할 수 있는 [gold]소멸[/gold] 카드를 보유하고 있지 않습니다.
- 페이지 요약: ACCEPT: 당신은 무덤에 손을 얹고 속삭입니다... [sine]“나와 함께 올라가자…”[/sine] 이에 응답하듯, 무덤으로부터 불꽃이 피어올라 한데 모여 하나의 [aqua]불타는 영혼[/aqua]이 되고, 당신의 곁을 충성스럽게 맴돕니다. 그 영혼은 당신이 무엇인지 알고 있으며, 마지막 목적지까지 당신을 인도하고 싶어 합니다. | CONFRONT: 당신은 무덤에 손을 얹고 속삭입니다... [sine]“당신의 싸움은 끝났다…”[/sine] 이에 응답하듯, 그 영혼은 당신의 영혼과 충돌하며 [jitter][aqua]영혼의 결투[/aqua][/jitter]가 벌어집니다! 당신이 겪어온 셀 수 없이 많은 전투에 경악한 그 영혼은, 패배를 인정합니다. | INITIAL: [jitter][aqua]푸른 불꽃이 거세게 일렁이는[/aqua][/jitter] 무덤 하나가 있습니다... 긍지 높은 한 전사는 전투 중 사망했지만, 영혼은 아직 안식을 얻지 못했습니다. 그 영혼은 계속 싸우고 싶어하고, 당신과 함께 여행을 떠나고 싶다 [sine]애원하는[/sine] 듯합니다. 하지만 어쩌면, 이제는 그가 [red]진실[/red]을 마주해야 할 때일지도 모릅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAVE_OF_THE_FORGOTTEN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.GraveOfTheForgotten`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투로 손상된 훈련 인형

- ID: `megacrit-sts2-core-models-events-battleworndummy`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신이 가까이 다가가자, 인형은 덜거덕거리며 지직대는 소리와 함께 강렬한 빛을 뿜기 시작합니다!

“[jitter]찌리릿![/jitter] 훈련 시간!!! 나를 쓰러뜨리기 위한 [blue]3번의 턴[/blue]이 주어진다!
설정을 조절하지 않으면 [red]치명적인 굴욕[/red]을 마주하게 된다.
선택지는 다음과 같습니다:”

섬뜩한 메시지가 끝난 뒤, 훈련 인형은 상세한 설명 사항을 차분하게 읽어 내려갑니다.

어떤 것을 선택할까요?
- 확인된 선택지:
  - 1로 설정: 체력이 [blue]{Setting1Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
  - 2로 설정: 체력이 [blue]{Setting2Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다.
  - 3으로 설정: 체력이 [blue]{Setting3Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 부가 메모: 체력이 [blue]{Setting1Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다. | 체력이 [blue]{Setting2Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다. | 체력이 [blue]{Setting3Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: DEFEAT: “[jitter][red]너는 약하다!![/red][/jitter] 굴욕을 집행했다!” 인형은 잠시 동안 침묵한 뒤 말합니다— “다음에는 더 낮게 설정하시길 바랍니다. 좋은 하루 되세요.” | INITIAL: 당신이 가까이 다가가자, 인형은 덜거덕거리며 지직대는 소리와 함께 강렬한 빛을 뿜기 시작합니다! “[jitter]찌리릿![/jitter] 훈련 시간!!! 나를 쓰러뜨리기 위한 [blue]3번의 턴[/blue]이 주어진다! 설정을 조절하지 않으면 [red]치명적인 굴욕[/red]을 마주하게 된다. 선택지는 다음과 같습니다:” 섬뜩한 메시지가 끝난 뒤, 훈련 인형은 상세한 설명 사항을 차분하게 읽어 내려갑니다. 어떤 것을 선택할까요? | VICTORY: “[jitter]너는 훈련을 통과했다![/jitter] 넌 이제 이 [gold]공장[/gold]을 침입자들로부터 방어할 모든 준비를 마쳤다!! 훈련 세션 후에는 반드시 수분을 섭취하도록!”
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BATTLEWORN_DUMMY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BattlewornDummy`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탐험가

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspheregold`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 이벤트를 만나보세요.
- 확인된 선택지:
  - 아래의 것을 가져간다: [gold]{BottomRelicOwned}[/gold]을(를) [gold]{BottomRelicNew}[/gold](와)과 거래합니다.
  - 위의 것을 가져간다: [gold]{TopRelicOwned}[/gold]을(를) [gold]{TopRelicNew}[/gold](와)과 거래합니다.
  - 중간 것을 가져간다: [gold]{MiddleRelicOwned}[/gold]을(를) [gold]{MiddleRelicNew}[/gold](와)과 거래합니다.
- 부가 메모: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요... | PLACEHOLDER: 시체랑은 거래하지 않아요.
- 페이지 요약: DONE: “헤헤헤 흐... 감사합니다!” | INITIAL: 당신이 모퉁이를 도는 순간, 바로 앞에 수상한 인물이 서 있습니다. 그는 당신을 향해 돌아서며 말합니다. “어서 오세요! 뭔가 거래하실 건가요?” 그는 질문과 동시에 망토를 펼치며, 당신에게 대량의 [sine][purple]수상한 물건들[/purple][/sine]을 선보입니다.
- 대사 프리뷰: 그래애애, 바로 그거에요! | 계속 주세요! | 아아, 정말 상쾌한 기분이네요...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISCOVER_ALL_EVENTS`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems.CrystalSphereGold`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가라앉는 등대

- ID: `megacrit-sts2-core-models-events-drowningbeacon`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 바위와 잔잔한 물로 이루어진 초현실적인 풍경을 지나자, 당신은 [purple]빛과 반대되는 무언가[/purple]를 뿜어대고 있는 [sine]가라앉은 등대[/sine]를 마주합니다.

당신은 이곳에서 [aqua]기분 나쁘게 빛나고 있는[/aqua] 물을 병에 담거나, 낡고 허름한 구조물을 타고 올라가 등대의 [gold]렌즈[/gold]를 수거할 수 있습니다.
- 확인된 선택지:
  - 병에 담는다: [aqua]{Potion}[/aqua]을 생성합니다.
  - 올라간다: [gold]{Relic}[/gold]를 획득합니다. 최대 체력을 [red]{HpLoss}[/red] 잃습니다.
- 부가 메모: [aqua]{Potion}[/aqua]을 생성합니다. | [gold]{Relic}[/gold]를 획득합니다. 최대 체력을 [red]{HpLoss}[/red] 잃습니다.
- 페이지 요약: BOTTLE: 당신은 등대가 조용히 진흙탕 속으로 가라앉는 동안, 근처에 있는 병을 사용해 [aqua]반짝거리는 액체[/aqua]를 퍼 담았습니다. 등대 안에 있지 않았던 게 천만다행이네요. | CLIMB: 당신은 등대가 계속해서 가라앉는 것을 불안하게 여기며, 등대의 꼭대기로 올라갑니다. 당신은 [gold]조명실[/gold]에 도착한 뒤, [purple]빛과 반대되는 무언가[/purple]가 [jitter][red]당신의 생기를 빨아들이는[/red][/jitter] 동안 렌즈를 떼어내기 시작했습니다. 마침내 렌즈를 분리하자, 등대는 가라앉기를 멈춥니다. | INITIAL: 바위와 잔잔한 물로 이루어진 초현실적인 풍경을 지나자, 당신은 [purple]빛과 반대되는 무언가[/purple]를 뿜어대고 있는 [sine]가라앉은 등대[/sine]를 마주합니다. 당신은 이곳에서 [aqua]기분 나쁘게 빛나고 있는[/aqua] 물을 병에 담거나, 낡고 허름한 구조물을 타고 올라가 등대의 [gold]렌즈[/gold]를 수거할 수 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DROWNING_BEACON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DrowningBeacon`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가라앉은 보물

- ID: `megacrit-sts2-core-models-events-sunkentreasury`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 길을 따라가던 당신은, 일부가 물에 잠긴 저장고를 발견합니다.
[gold]상자[/gold]는 [blue]2[/blue]개가 있지만, 금방이라도 [jitter][purple]망가져버릴 듯한 열쇠[/purple][/jitter]는 [blue]1[/blue]개 뿐입니다.

[gold]첫 번째 상자:[/gold] 흔들어보니 딸랑거리는 소리가 납니다. 약간의 골드가 들어 있습니다.
[gold]두 번째 상자:[/gold] 거대하고 화려한 상자로, 명백히 [red]저주받았습니다[/red]. 많은 골드가 있습니다!
- 확인된 선택지:
  - 두 번째 상자: [gold]골드[/gold]를 [blue]{LargeChestGold}[/blue] 얻습니다. [red]탐욕[/red]을 얻습니다.
  - 첫 번째 상자: [gold]골드[/gold]를 [blue]{SmallChestGold}[/blue] 얻습니다.
- 부가 메모: [gold]골드[/gold]를 [blue]{SmallChestGold}[/blue] 얻습니다. | [gold]골드[/gold]를 [blue]{LargeChestGold}[/blue] 얻습니다. [red]탐욕[/red]을 얻습니다.
- 페이지 요약: FIRST_CHEST: 네, [gold]골드[/gold]가 들어 있네요. | INITIAL: 길을 따라가던 당신은, 일부가 물에 잠긴 저장고를 발견합니다. [gold]상자[/gold]는 [blue]2[/blue]개가 있지만, 금방이라도 [jitter][purple]망가져버릴 듯한 열쇠[/purple][/jitter]는 [blue]1[/blue]개 뿐입니다. [gold]첫 번째 상자:[/gold] 흔들어보니 딸랑거리는 소리가 납니다. 약간의 골드가 들어 있습니다. [gold]두 번째 상자:[/gold] 거대하고 화려한 상자로, 명백히 [red]저주받았습니다[/red]. 많은 골드가 있습니다! | SECOND_CHEST: 이렇게 많은 [gold]골드[/gold]가 모여있는 건 생전 처음 보는 광경입니다! [sine][/sine][blue]상인[/blue][sine]이 파는 물건들을 전부 사버릴 수도 있을 것 같습니다...[/sine] {Monologue} [sine]위험한 불량배들은 돈으로 물러나게 할 수 있습니다...[/sine] 하지만 [red][sine]병적인 탐욕[/sine][/red]은 영원히 당신의 마음속에 남을 것입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUNKEN_TREASURY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenTreasury`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 감염된 자동인형

- ID: `megacrit-sts2-core-models-events-infestedautomaton`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [red]작동을 멈춘 로봇들[/red]로 가득한 방이 있습니다.

고독한 자동인형 하나가 [sine][aqua]희미하게 빛나는 핵[/aqua][/sine]을 유지하고 있지만, 기괴한 유기적 증식체에 휩싸여 아무런 반응도 보이지 않습니다.
- 확인된 선택지:
  - 살펴본다: 무작위 [gold]파워 카드[/gold]를 1장 얻습니다.
  - 핵을 만진다: 무작위 [gold]비용이 0인 카드[/gold]를 1장 얻습니다.
- 부가 메모: 무작위 [gold]파워 카드[/gold]를 1장 얻습니다. | 무작위 [gold]비용이 0인 카드[/gold]를 1장 얻습니다.
- 페이지 요약: INITIAL: [red]작동을 멈춘 로봇들[/red]로 가득한 방이 있습니다. 고독한 자동인형 하나가 [sine][aqua]희미하게 빛나는 핵[/aqua][/sine]을 유지하고 있지만, 기괴한 유기적 증식체에 휩싸여 아무런 반응도 보이지 않습니다. | STUDY: [aqua]빛나는 자동인형[/aqua]과 인형의 내부 구조가 어떤 방식으로 배치되어 있는지를 살펴본 당신은, 특정 금속과 그 금속의 배열 상태가 여러 가지 이상 상태에 저항할 수 있다는 사실을 깨닫습니다! 당신은 이 [blue]기술[/blue]을 자신에게 적용합니다. | TOUCH_CORE: 손이 [aqua]핵[/aqua]에 살짝 닿는 순간, [jitter][gold]전기[/gold] 충격이 당신을 강타합니다[/jitter]. 당신은 그 경험으로 인해 무언가 [sine][purple]달라진[/purple][/sine] 기분을 느끼며, 황급히 달아납니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFESTED_AUTOMATON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.InfestedAutomaton`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거울에 비치다 다치비 에울거

- ID: `megacrit-sts2-core-models-events-reflections`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이건 뭘까요? 이곳은 뭔가 이상한 느낌이 듭니다...
[sine]...다니듭 이낌느 한상이 가뭔 은곳이 ?요까뭘 건이[/sine]
- 확인된 선택지:
  - 거울을 만진다: 무작위 카드를 [blue]2[/blue]장 [red]열화[/red]시킵니다. 무작위 카드를 [blue]4[/blue]장 [green]강화[/green]합니다.
  - 부순다: [gold]덱[/gold]을 복제합니다. [purple]불운[/purple]을 얻습니다.
- 부가 메모: [gold]덱[/gold]을 복제합니다. [purple]불운[/purple]을 얻습니다. | 무작위 카드를 [blue]2[/blue]장 [red]열화[/red]시킵니다. 무작위 카드를 [blue]4[/blue]장 [green]강화[/green]합니다.
- 페이지 요약: INITIAL: 이건 뭘까요? 이곳은 뭔가 이상한 느낌이 듭니다... [sine]...다니듭 이낌느 한상이 가뭔 은곳이 ?요까뭘 건이[/sine] | SHATTER: 당신은 정신을 가다듬고, 눈앞에 있는 것이 이음새 하나 없는 거대한 거울이라는 사실을 깨닫습니다. 당신은 거대한 거울을 빠르게 걷어차 [jitter]산산조각[/jitter] 냈습니다! 거울이 깨지는 순간, 당신의 거울상은 자신을 자책하듯이 눈부시게 빛나는 수천 개의 파편으로 흩어집니다. 어떻게 이런 공간이 존재할 수 있는 걸까요? | TOUCH_A_MIRROR: 당신은 손을 뻗어, 눈앞에 있는 거울상 속으로 녹아듭니다. [sine]어떤 게 나지?[/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFLECTIONS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Reflections`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 뇌 거머리

- ID: `megacrit-sts2-core-models-events-brainleech`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter]*푹*[/jitter]

당신의 머리 위쪽에 날카로운 통증이 스치고, 한 생각이 당신의 마음을 파고듭니다.

[purple][sine]“지식을 공유하겠나???”[/sine][/purple]

당신은 어떻게 해야 할지 확신이 서지 않습니다...
- 확인된 선택지:
  - 거머리를 떼어낸다: 체력을 [red]{RipHpLoss}[/red] 잃습니다. 무색 카드 보상을 1번 얻습니다.
  - 지식을 공유한다: 무작위 카드 [blue]{FromCardChoiceCount}[/blue]장 중 [blue]{CardChoiceCount}[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.
- 부가 메모: 체력을 [red]{RipHpLoss}[/red] 잃습니다. 무색 카드 보상을 1번 얻습니다. | 무작위 카드 [blue]{FromCardChoiceCount}[/blue]장 중 [blue]{CardChoiceCount}[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.
- 페이지 요약: INITIAL: [jitter]*푹*[/jitter] 당신의 머리 위쪽에 날카로운 통증이 스치고, 한 생각이 당신의 마음을 파고듭니다. [purple][sine]“지식을 공유하겠나???”[/sine][/purple] 당신은 어떻게 해야 할지 확신이 서지 않습니다... | RIP: 당신은 [jitter]머리에서 거머리를 격렬하게 뜯어낸 뒤[/jitter] 저 멀리 던져버렸습니다! 머릿속에서는 목소리가 들려옵니다: [purple][sine]“안돼애애애애애애애애애애애애애애....” | SHARE_KNOWLEDGE: [purple][sine]“이 얼마나 사치스럽게 학구적인, 미로와도 같은 사고인가! 두뇌의 양식이 될 만한 참된 지적 풍요의 향연이로다!”[/sine][/purple]. 만족한 듯한 그 생명체는, 머리에서 튀어나와 기어가 버립니다. 당신은 [blue]어안이 벙벙해졌습니다[/blue].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BRAIN_LEECH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BrainLeech`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 버섯이 먹고 싶어

- ID: `megacrit-sts2-core-models-events-hungryformushrooms`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [sine]마지막으로 뭔가 먹었던 게 언제였을까요?
...잠깐, 이 환상적인 냄새는 뭐죠?[/sine]

향기를 따라가다 보니, 당신은 온갖 종류의 [green]맛있는 버섯들[/green]이 익어가고 있는 아늑한 야영지에 도착했습니다! 당신은 배가 너무 고픈 나머지, 이 버섯들을 먹어도 안전할지는 고려하지도 않습니다.
(배가 너무 고파서 모험가의 시체가 있다는 건 깨닫지도 못했습니다)
- 확인된 선택지:
  - 커다란 버섯
  - 향기로운 버섯
- 페이지 요약: BIG_MUSHROOM: 당신은 [orange]커다란 버섯[/orange]을 뜯어 먹습니다. 버섯의 속살은 탄탄하고 전분기가 느껴져, 먹고 나니 든든합니다. 버섯을 먹으면 먹을수록, 마치 버섯이 당신의 허기를 먹어치우고 있듯이, 오히려 배가 더 고파집니다. [sine]정말 맛있네요... 당신은 [red]식곤증[/red]에 빠져듭니다.[/sine] 이 폭식은 대가를 치를 것입니다. | FRAGRANT_MUSHROOM: 당신은 특히 [green]향기로운[/green], 은은한 조개향을 풍기는 작은 목질의 버섯을 한 입 맛봅니다... 에너지가 폭발하듯이 솟구쳐, 달리고 뛰며 [gold]격렬하게 몸을 단련[/gold]하고 싶어집니다! 그러다 이내, 짧은 삶에서 최후의 순간을 맞이하는 살아있는 곰팡이가 몸부림치며, [red][jitter]날카로운 통증[/jitter][/red]이 당신에게 몰려옵니다. | INITIAL: [sine]마지막으로 뭔가 먹었던 게 언제였을까요? ...잠깐, 이 환상적인 냄새는 뭐죠?[/sine] 향기를 따라가다 보니, 당신은 온갖 종류의 [green]맛있는 버섯들[/green]이 익어가고 있는 아늑한 야영지에 도착했습니다! 당신은 배가 너무 고픈 나머지, 이 버섯들을 먹어도 안전할지는 고려하지도 않습니다. (배가 너무 고파서 모험가의 시체가 있다는 건 깨닫지도 못했습니다)
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HUNGRY_FOR_MUSHROOMS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.HungryForMushrooms`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 벌레 학살자

- ID: `megacrit-sts2-core-models-events-bugslayer`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [jitter][red]난폭한 곤충 무리[/red][/jitter]를 정신없이 상대하던 와중, 당신의 옆에서 내내 함께 싸우고 있었던 [gold]강인하고 거친 한 전사 사내[/gold]를 뒤늦게 발견했습니다!

곤충들은 패배했다는 것을 깨닫고 흩어집니다. 당신은 곁에서 함께 싸웠던 전우를 돌아봅니다.
“이 해충들을 박멸하기 위한 요령 하나 알려드릴까요?”

정말 정중하군요. 당신은 고개를 끄덕이고 그의 제안을 받아들였습니다.
- 확인된 선택지:
  - 박멸 기술 배우기: [gold]덱[/gold]에 [gold]{Card1}[/gold]을 추가합니다.
  - 짓누르기 기술 배우기: [gold]덱[/gold]에 [gold]{Card2}[/gold]를 추가합니다.
- 부가 메모: [gold]덱[/gold]에 [gold]{Card1}[/gold]을 추가합니다. | [gold]덱[/gold]에 [gold]{Card2}[/gold]를 추가합니다.
- 페이지 요약: DONE: 그는 아무 말없이 당신에게 고개를 끄덕인 뒤, 자리를 떠납니다. | EXTERMINATION: “좋아요... [gold]박멸 기술[/gold]을 배우고 싶으신 거군요. 알겠습니다...” 그는 잠시 생각에 잠겨, 이 기술을 당신에게 가장 수월하게 전할 수 있는 방법을 모색합니다... [jitter]“이게 바로 그 기술입니다! 보시죠!!”[/jitter] 그는 [green]살충제[/green] 캔을 들고 자신의 실력을 뽐냅니다. | INITIAL: 당신은 [jitter][red]난폭한 곤충 무리[/red][/jitter]를 정신없이 상대하던 와중, 당신의 옆에서 내내 함께 싸우고 있었던 [gold]강인하고 거친 한 전사 사내[/gold]를 뒤늦게 발견했습니다! 곤충들은 패배했다는 것을 깨닫고 흩어집니다. 당신은 곁에서 함께 싸웠던 전우를 돌아봅니다. “이 해충들을 박멸하기 위한 요령 하나 알려드릴까요?” 정말 정중하군요. 당신은 고개를 끄덕이고 그의 제안을 받아들였습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BUGSLAYER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Bugslayer`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 변성체의 숲

- ID: `megacrit-sts2-core-models-events-morphicgrove`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [green]결정화된 나무들[/green]로 가득한 숲에 들어갔고, 나무들이 [jitter]격렬하게 떨기[/jitter] 시작합니다!
나무 사이에서 한 무리의 [aqua]변성체들[/aqua]이 [jitter]튀어나오며[/jitter], 당신에게 [sine]인사와 환영[/sine]의 말을 쏟아냅니다.

[aqua]변성체들[/aqua] 중 한 마리는 구석에서 불안해하고 있는 것으로 보아, 다른 개체만큼 사교적이지는 않은 것 같습니다.

무리와 외톨이 중 누구에게 다가갈까요?
- 확인된 선택지:
  - 무리: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 카드를 [blue]2[/blue]장 [gold]변화[/gold]시킵니다.
  - 외톨이: 최대 체력을 [green]{MaxHp}[/green] 얻습니다.
- 부가 메모: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 카드를 [blue]2[/blue]장 [gold]변화[/gold]시킵니다. | 최대 체력을 [green]{MaxHp}[/green] 얻습니다.
- 페이지 요약: GROUP: 당신이 재잘거리던 [aqua]변성체들[/aqua]에게 인사를 건내자, 그들은 결정화된 팔로 당신을 끌어안았습니다. [aqua]변성체들[/aqua]은 당신에게 자신들의 결합된 지식을 공유하고, [sine]변성 마법[/sine]을 주입해 당신의 본질을 변화시킵니다... 정말 사랑스러운 생명체들인 것 같네요. [red]변성체들 중 하나[/red]가 당신의 [gold]골드[/gold]를 [red]훔쳐갔습니다.[/red] | INITIAL: 당신은 [green]결정화된 나무들[/green]로 가득한 숲에 들어갔고, 나무들이 [jitter]격렬하게 떨기[/jitter] 시작합니다! 나무 사이에서 한 무리의 [aqua]변성체들[/aqua]이 [jitter]튀어나오며[/jitter], 당신에게 [sine]인사와 환영[/sine]의 말을 쏟아냅니다. [aqua]변성체들[/aqua] 중 한 마리는 구석에서 불안해하고 있는 것으로 보아, 다른 개체만큼 사교적이지는 않은 것 같습니다. 무리와 외톨이 중 누구에게 다가갈까요? | LONER: 당신은 외톨이 [aqua]변성체[/aqua]의 본질에 공감하며, 그에게 손을 뻗었습니다. 그는 당신에게 작은 크리스탈 과일을 선물했고, 당신은 관습에 따라 과일을 먹었습니다. 과일을 먹자 당신의 정신과 결의가 선명해집니다. 당신은 감사를 표한 뒤 길을 나아갔습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MORPHIC_GROVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.MorphicGrove`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 불안한 휴식 장소

- ID: `megacrit-sts2-core-models-events-unrestsite`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한적한 [gold]휴식 장소[/gold]를 발견한 당신은 [orange]불[/orange]을 피우고 잠시 휴식을 취합니다.
적어도, 그렇게 믿고 있었습니다.

불을 피우자 불길은 위로 치솟는 대신 [sine]옆으로[/sine] 퍼져가며, [purple]기름이 스며 나오는 나무들[/purple]이 모인 숲을 향해 번져 갑니다.

그럼에도 불구하고 휴식을 취할까요?
- 확인된 선택지:
  - 그래도 휴식한다: [green]체력을 모두 회복합니다.[/green] [red]수면 부족[/red]을 얻습니다.
  - 나무들을 베어낸다: 최대 체력을 [red]{MaxHpLoss}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 부가 메모: 최대 체력을 [red]{MaxHpLoss}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다. | [green]체력을 모두 회복합니다.[/green] [red]수면 부족[/red]을 얻습니다.
- 페이지 요약: INITIAL: 한적한 [gold]휴식 장소[/gold]를 발견한 당신은 [orange]불[/orange]을 피우고 잠시 휴식을 취합니다. 적어도, 그렇게 믿고 있었습니다. 불을 피우자 불길은 위로 치솟는 대신 [sine]옆으로[/sine] 퍼져가며, [purple]기름이 스며 나오는 나무들[/purple]이 모인 숲을 향해 번져 갑니다. 그럼에도 불구하고 휴식을 취할까요? | KILL: 나무들은 [purple]명백히 위협적[/purple]이었기 때문에, 당신은 나무들을 처리하기로 합니다. 며칠 후, 당신은 [red]재[/red]와 [purple]기름 찌꺼기[/purple]투성이가 됐습니다. 베어 넘긴 나무들에서 작은 섀의 영혼들이 떼를 지어 날아오르더니, 당신의 발치에 [gold]작은 상자[/gold] 하나를 떨어뜨립니다. [green]“나무들로부터 저희를 해방해 주셔서 감사드립니다... [sine]짹짹[/sine]!”[/green] 잠을 좀 더 자야할 것 같네요. | REST: 잇따른 모험으로 지친 당신은, 기묘한 상황들을 뒤로 한 채 잠을 청하기로 합니다. [orange]불길[/orange]의 포효와 타닥거림 속에서, 당신은 밤새도록 뒤척이며 잠을 설칩니다. [purple]역겨운 나무들[/purple]에서 흘러내리는 새까만 수액은 끔찍한 악취를 내뿜고 있습니다. 당신은 잠에서 깨어났고, 체력은 회복됐지만 기운은 돌아오지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNREST_SITE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.UnrestSite`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛과 어둠의 문

- ID: `megacrit-sts2-core-models-events-doorsoflightanddark`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 방금 전까지만 해도 존재하지 않았던 출입구가 어느새 생겨나 있습니다...

안으로 들어서자, 희미하게 빛나는 두 개의 문과 [gold]잘 차려입은 문지기[/gold]가 보입니다.
“드디어, 방문객이시군요! 제발 부디! 문을 선택해주세요, 아무거나요!”

[blue]여긴 아직 첨탑 안인 걸까요? 어떻게 이렇게 깨끗할 수가 있죠?[/blue]
- 확인된 선택지:
  - 빛의 문: 무작위 카드를 [blue]{Cards}[/blue]장 [gold]강화[/gold]합니다.
  - 어둠의 문: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다.
- 부가 메모: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다. | 무작위 카드를 [blue]{Cards}[/blue]장 [gold]강화[/gold]합니다.
- 페이지 요약: DARK: [sine][purple]어둠 속으로...[/purple][/sine] 아무것도 보이지 않습니다. 하지만 보이지 않는 덕에... [red]쓸데없는 생각들[/red]이 떨어져 나가는 것 같습니다. 당신은 다시 [blue]지하 선착장[/blue]으로 돌아왔습니다. 어떻게 여기로? 어라...? | INITIAL: 방금 전까지만 해도 존재하지 않았던 출입구가 어느새 생겨나 있습니다... 안으로 들어서자, 희미하게 빛나는 두 개의 문과 [gold]잘 차려입은 문지기[/gold]가 보입니다. “드디어, 방문객이시군요! 제발 부디! 문을 선택해주세요, 아무거나요!” [blue]여긴 아직 첨탑 안인 걸까요? 어떻게 이렇게 깨끗할 수가 있죠?[/blue] | LIGHT: [sine][gold]빛 속으로...[/gold][/sine] [jitter]눈이 부셔옵니다!![/jitter] 하지만 그 빛이 당신을 [gold]강하게[/gold] 만듭니다. 당신은 다시 [blue]지하 선착장[/blue]으로 돌아왔습니다. 어떻게 여기로? 어라...?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOORS_OF_LIGHT_AND_DARK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DoorsOfLightAndDark`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사람 크기 구멍의 들판

- ID: `megacrit-sts2-core-models-events-fieldofmansizedholes`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 거대한 들판이 갑작스럽게 눈앞에 펼쳐지고, 당신은 [jitter]사람들의 윤곽이 선명하게 새겨진 흔적들[/jitter]을 발견하고 걸음을 멈춥니다.

새겨진 윤곽 중 하나는 당신의 것과 일치합니다.

[sine][orange]완벽하네요...[/orange][/sine]
- 확인된 선택지:
  - 내 구멍에 들어간다: 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 저항한다: [gold]덱[/gold]에서 카드를 [blue]{Cards}[/blue]장 제거합니다. [gold]덱[/gold]에 [red]{ResistCurse}[/red]를 추가합니다.
- 부가 메모: 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다. | [gold]덱[/gold]에서 카드를 [blue]{Cards}[/blue]장 제거합니다. [gold]덱[/gold]에 [red]{ResistCurse}[/red]를 추가합니다.
- 페이지 요약: ENTER_YOUR_HOLE: 조금 비좁긴 하지만, 당신은 몸을 구겨 구멍 속으로 들어갑니다... 당신이 더 깊게 나아갈수록, 구멍 속은 [sine][purple]점점 어두워집니다[/purple][/sine]. [gold]희미하던 빛[/gold]은 끝내 점점 강해지고, 건너편의 모습이 드러납니다. 당신은 아무것도 변하지 않은 채로 구멍을 빠져나왔지만, [green]생명의 숨겨진 비밀[/green]을 깨달았습니다. | INITIAL: 거대한 들판이 갑작스럽게 눈앞에 펼쳐지고, 당신은 [jitter]사람들의 윤곽이 선명하게 새겨진 흔적들[/jitter]을 발견하고 걸음을 멈춥니다. 새겨진 윤곽 중 하나는 당신의 것과 일치합니다. [sine][orange]완벽하네요...[/orange][/sine] | RESIST: 당신은 모든 구멍을, 심지어 [orange]당신의 형상이 완벽하게 새겨진[/orange] 구멍조차도 지나쳐 갑니다... 왜 안으로 들어가지 않았을까요? 들어갔으면 무슨 일이 생겼을까요? 저 구멍에 들어간 적이 있었을까요? 누가 이 구멍들을 만든 거죠? [sine][red]이건 뭐... 누구... 어디?[/red][/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIELD_OF_MAN_SIZED_HOLES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FieldOfManSizedHoles`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사파이어 씨앗

- ID: `megacrit-sts2-core-models-events-sapphireseed`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 빛나는 날카로운 물체를 밟으려던 것을 가까스로 피합니다 ...씨앗? 특유의 색깔로 짐작해봤을 때, 분명 [aqua]사파이어 씨앗[/aqua]입니다! 이 엄청난 씨앗들은 분명 멸종했던 거 아니었나요!?

전설에 따르면 씨앗을 먹을 시 [gold]지구력이 크게 상승[/gold]한다고 합니다만, 씨앗을 먹는 대신 심고서 정성껏 기른다면 어떤 일이 벌어질까요?
- 확인된 선택지:
  - 먹는다: 체력을 [green]{Heal}[/green] 회복합니다. [gold]덱[/gold]에 있는 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 심고 영양을 공급한다: 카드 1장에 [purple]{Enchantment}[/purple]를 [gold]인챈트[/gold]합니다.
- 부가 메모: 체력을 [green]{Heal}[/green] 회복합니다. [gold]덱[/gold]에 있는 무작위 카드를 1장 [gold]강화[/gold]합니다. | 카드 1장에 [purple]{Enchantment}[/purple]를 [gold]인챈트[/gold]합니다.
- 페이지 요약: EAT: [aqua]사파이어 씨앗[/aqua]을 삼키자, [blue]팔다리가 가벼워지고[/blue] [gold]세상의 색이 선명해지며[/gold], [purple][sine]주변에 있는 적들의 영혼이 느껴집니다[/sine][/purple]. 씨앗을 밟지 않길 잘했네요. | INITIAL: 당신은 빛나는 날카로운 물체를 밟으려던 것을 가까스로 피합니다 ...씨앗? 특유의 색깔로 짐작해봤을 때, 분명 [aqua]사파이어 씨앗[/aqua]입니다! 이 엄청난 씨앗들은 분명 멸종했던 거 아니었나요!? 전설에 따르면 씨앗을 먹을 시 [gold]지구력이 크게 상승[/gold]한다고 합니다만, 씨앗을 먹는 대신 심고서 정성껏 기른다면 어떤 일이 벌어질까요? | PLANT: 당신은 [green]과성장[/green] 지대를 샅샅이 뒤져, [blue]통기성이 좋은[/blue] 화분을 찾아냅니다. 그 후, 당신은 [orange]갈라진 벽 틈으로 빛이 스며드는[/orange] 아직 훼손되지 않은 장소를 찾아 주위를 돌아다닙니다. 당신은 조심스럽게 화분을 나무껍질과 덩굴로 보강해 [green]위장하고[/green], 비옥하고 영양분이 풍부한 흙을 채운 뒤, [aqua]씨앗[/aqua]을 지면 아래에 살며시 자리잡게 합니다. 마지막으로, 근처 식물에서 이슬을 모아 흙 위에 붓습니다. [sine]쑥쑥 자라라, 작은 씨앗아...[/sine] 당신도 모르는 사이에, [aqua]사파이어 씨앗[/aqua]이 당신의 영혼에 깃듭니다. 당신의 노고에 감사하고 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SAPPHIRE_SEED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SapphireSeed`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 샘터

- ID: `megacrit-sts2-core-models-events-wellspring`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter]낮게 울리는 소리[/jitter]의 근원을 따라가던 당신은 고요한 샘 앞에 이르렀고, 그 샘의 모습에 매료됩니다. [sine][aqua]에메랄드빛 녹색[/aqua][/sine]의 물에는 [sine][gold]빛나는 알갱이[/gold][/sine]들이 모여 있습니다.

샘이 당신을 유혹하고 있는 듯합니다. 설마 무슨 문제라도 있겠어요?
- 확인된 선택지:
  - 몸을 담근다: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다. [gold]덱[/gold]에 [red]죄책감[/red]을 [blue]{BatheCurses}[/blue]장 추가합니다.
  - 병에 담는다: 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 부가 메모: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다. [gold]덱[/gold]에 [red]죄책감[/red]을 [blue]{BatheCurses}[/blue]장 추가합니다. | 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 페이지 요약: BATHE: 당신은 미묘한 [red][sine]불안감을 느끼며[/sine][/red] 샘을 빠져 나왔습니다. 하지만 예상외로 상쾌한 기분이 듭니다. | BOTTLE: 이토록 [sine][aqua]아름다운 물[/aqua][/sine]에 몸을 담근다는 사실이 왠지 꺼림칙합니다… 당신은 그 대신 약간의 물을 병에 담아가기로 했습니다. 분명 쓸모가 있겠죠. | INITIAL: [jitter]낮게 울리는 소리[/jitter]의 근원을 따라가던 당신은 고요한 샘 앞에 이르렀고, 그 샘의 모습에 매료됩니다. [sine][aqua]에메랄드빛 녹색[/aqua][/sine]의 물에는 [sine][gold]빛나는 알갱이[/gold][/sine]들이 모여 있습니다. 샘이 당신을 유혹하고 있는 듯합니다. 설마 무슨 문제라도 있겠어요?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WELLSPRING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Wellspring`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 섀도니스 둥지

- ID: `megacrit-sts2-core-models-events-byrdonisnest`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 거대한 [jitter][red]비틀거리는 야수[/red][/jitter]가 부상을 입은 [green][sine]초록 섀[/sine][/green]를 쫓아내는 모습을 목격합니다.

섀가 도망친 오목한 공간 안에는, [gold]무방비한 상태의 알 하나[/gold]가 버려져 있습니다.

[sine]당신의 배가 꼬르륵거립니다…[/sine]
- 확인된 선택지:
  - 알을 가져간다: [gold]덱[/gold]에 [gold]{Card}[/gold]을 추가합니다.
  - 알을 먹는다: 최대 체력을 [blue]{MaxHp}[/blue] 얻습니다.
- 부가 메모: 최대 체력을 [blue]{MaxHp}[/blue] 얻습니다. | [gold]덱[/gold]에 [gold]{Card}[/gold]을 추가합니다.
- 페이지 요약: EAT: 당신은 알을 깨고서 개걸스럽게 먹어치웠습니다. 좋은 아침거리였네요. | INITIAL: 당신은 거대한 [jitter][red]비틀거리는 야수[/red][/jitter]가 부상을 입은 [green][sine]초록 섀[/sine][/green]를 쫓아내는 모습을 목격합니다. 섀가 도망친 오목한 공간 안에는, [gold]무방비한 상태의 알 하나[/gold]가 버려져 있습니다. [sine]당신의 배가 꼬르륵거립니다…[/sine] | TAKE: 당신은 나중에 쓸모있는 무언가가 [green]부화[/green]할지도 모른다고 생각하며, 조심스럽게 알을 들어 올려 챙겼습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BYRDONIS_NEST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ByrdonisNest`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 속삭이는 골짜기

- ID: `megacrit-sts2-core-models-events-whisperinghollow`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [red]죽은 나무들로 이뤄진 골짜기[/red]를 지나던 중, 우연히 뼈처럼 새하얀 색의 나무 한 그루를 발견합니다. 무언가를 보호하는 갈비뼈처럼 안쪽으로 휘어진 가지에는, 점토 장식이 매달려 있습니다.

정말 [purple]소름끼치는 나무[/purple]입니다. 나무는 속삭입니다.

[sine][blue]...거래하라.....[/blue][/sine]
- 확인된 선택지:
  - 골드를 거래한다: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다.
  - 나무를 끌어안는다: 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]변화[/gold]시킬 카드를 1장 선택합니다.
- 부가 메모: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다. | 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]변화[/gold]시킬 카드를 1장 선택합니다.
- 페이지 요약: GOLD: 당신은 나무 밑동 근처에 [gold]골드[/gold]를 몇 개 흩뿌렸고, 점토로 이뤄진 통 두 개가 아무런 소리 없이 바닥에 떨어집니다. 통은 부서진 뒤 열렸고, 안에는 [green]굉장히 온전한 상태의 포션들[/green]이 들어 있었습니다. 당신은 포션을 낚아챈 뒤 도망쳤습니다. | HUG: 나무는 자신에게 [gold]골드[/gold]를 넘기라고 [blue][sine]속삭였지만[/sine][/blue], 당신은 그 대신 나무를 강하게 끌어안았습니다! 나무는 [red][jitter]몸부림치며[/jitter][/red] 나뭇가지로 당신을 긁어댔지만, 이윽고 얌전해집니다. [green]나무를 끌어안는 경험[/green]은 당신을 [gold]변화[/gold]시켰습니다. [purple][sine]...포옹한다.....?[/sine][/purple] | INITIAL: 당신은 [red]죽은 나무들로 이뤄진 골짜기[/red]를 지나던 중, 우연히 뼈처럼 새하얀 색의 나무 한 그루를 발견합니다. 무언가를 보호하는 갈비뼈처럼 안쪽으로 휘어진 가지에는, 점토 장식이 매달려 있습니다. 정말 [purple]소름끼치는 나무[/purple]입니다. 나무는 속삭입니다. [sine][blue]...거래하라.....[/blue][/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHISPERING_HOLLOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WhisperingHollow`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수정구

- ID: `megacrit-sts2-core-models-events-crystalsphere`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 타일을 열어 당신의 미래를 드러내 보세요!

[gold]소형 점술[/gold]이나 [gold]대형 점술[/gold]을 선택한 뒤, 수정구 안의 타일들을 열 수 있습니다.

공개된 아이템들은 세션이 끝난 뒤에 수령할 수 있게 됩니다...

하지만 조심하세요, 몇몇 아이템들은 [red]위험[/red]할 수도 있습니다!
- 확인된 선택지:
  - 미래를 알아본다: [gold]골드[/gold]를 [red]{UncoverFutureCost}[/red] 지불합니다. [blue]{UncoverFutureProphesizeCount}[/blue]번 [purple]점을 칩니다[/purple].
  - 분할 납부: [red]{CurseTitle}[/red]을 얻습니다. [blue]{PaymentPlanCount}[/blue]번 [purple]점을 칩니다[/purple].
- 부가 메모: 타일을 열어 당신의 미래를 드러내 보세요! [gold]소형 점술[/gold]이나 [gold]대형 점술[/gold]을 선택한 뒤, 수정구 안의 타일들을 열 수 있습니다. 공개된 아이템들은 세션이 끝난 뒤에 수령할 수 있게 됩니다... 하지만 조심하세요, 몇몇 아이템들은 [red]위험[/red]할 수도 있습니다!
- 페이지 요약: FINISH: 사색을 마친다 | INITIAL: [sine][blue]“당신이 올 것을 알고 있었습니다...!”[/blue][/sine] [purple]신비로운 오두막[/purple]에 들어서자 [jitter]탁한[/jitter] 목소리의 존재가 당신을 부릅니다. [sine][blue]“당신의 운명이 당신을 이곳으로 이끌었습니다. 당신의 미래와 운명을 밝혀내야만 우리 모두가 구원받을 수 있습니다!!”[/blue][/sine] “좋아요, 여기가 [gold]수정구 점술[/gold]에서 고를 수 있는 항목들입니다. 여기 면책 서류에도 서명해주시고.” 점술가는 펜과 양피지를 꺼내며 말합니다. | PAYMENT_PLAN: 분할 납부
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CRYSTAL_SPHERE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.CrystalSphere`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쓰레기 더미

- ID: `megacrit-sts2-core-models-events-trashheap`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [red][jitter]고철이 된 무기[/jitter][/red]와 [orange]버려진 장신구[/orange], 그리고 [purple][sine]다른 기묘한 물건들[/sine][/purple]이 산처럼 쌓여있는 거대한 더미를 발견합니다. 거대한 더미는 안쪽에서부터 커지고 있는 듯이 움직이며 진동합니다...

쓰레기 더미의 표면을 뒤져보면 쓸 만한 물건 몇 개는 건질 수도 있습니다. 하지만 저 더미 안으로 들어간다면, [gold]신기한 보물[/gold]을 찾아낼 수 있을지도 모릅니다.
- 확인된 선택지:
  - 뛰어든다: 체력을 [red]{HpLoss}[/red] 잃습니다. 과거의 무작위 잊힌 [gold]유물[/gold]을 1개 얻습니다.
  - 아무 고물이나 잡는다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 과거의 무작위 잊힌 카드를 1장 얻습니다.
- 부가 메모: 체력을 [red]{HpLoss}[/red] 잃습니다. 과거의 무작위 잊힌 [gold]유물[/gold]을 1개 얻습니다. | [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 과거의 무작위 잊힌 카드를 1장 얻습니다.
- 페이지 요약: DIVE_IN: 당신은 꽤 오랜 시간 동안 [red][sine]위험천만한 쓰레기들[/sine][/red] 사이를 헤치며 나아간 끝에, [gold]오래된 유물[/gold] 하나를 찾아냅니다! 왜 이게 버려져 있던 걸까요? | GRAB: 당신은 안전한 거리를 유지한 채 더미 주위를 돌며, 여기저기 흩어진 [gold]잊힌 귀중품[/gold]과 [purple][sine]기묘한 물건들[/sine][/purple]을 주워 담습니다. 애초에 이것들은 왜 만들어진 걸까요? | INITIAL: 당신은 [red][jitter]고철이 된 무기[/jitter][/red]와 [orange]버려진 장신구[/orange], 그리고 [purple][sine]다른 기묘한 물건들[/sine][/purple]이 산처럼 쌓여있는 거대한 더미를 발견합니다. 거대한 더미는 안쪽에서부터 커지고 있는 듯이 움직이며 진동합니다... 쓰레기 더미의 표면을 뒤져보면 쓸 만한 물건 몇 개는 건질 수도 있습니다. 하지만 저 더미 안으로 들어간다면, [gold]신기한 보물[/gold]을 찾아낼 수 있을지도 모릅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRASH_HEAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TrashHeap`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 융합자

- ID: `megacrit-sts2-core-models-events-amalgamator`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [b][jitter]깡! 깡!!![/jitter][/b]

벽 건너편에서 금속과 금속이 부딪히는 소리가 울려 퍼집니다...

당신이 벽에 머리를 기대고 귀를 기울이려는 찰나—벽이 갈라지듯 열렸고, [orange]여섯 개의 팔을 지닌 거구의 인물[/orange]이 작업에 몰두하고 있는 광경이 드러납니다.
그의 “얼굴”이라 불릴 만한 곳에는, [gold][sine]빛나는 문양들이 뒤엉켜 소용돌이[/sine][/gold]치고 있습니다.

“[aqua]승천하는 영혼[/aqua]을 지닌 자가 내 공방에 찾아온 건가? 좋아, [jitter]결합이다[/jitter]!”
- 확인된 선택지:
  - 수비를 합친다: [gold]수비[/gold]를 [blue]2[/blue]장 제거합니다. [gold]{Card2}[/gold]를 [gold]덱[/gold]에 추가합니다.
  - 타격을 합친다: [gold]타격[/gold]을 [blue]2[/blue]장 제거합니다. [gold]{Card1}[/gold]을 [gold]덱[/gold]에 추가합니다.
- 부가 메모: [gold]수비[/gold]를 [blue]2[/blue]장 제거합니다. [gold]{Card2}[/gold]를 [gold]덱[/gold]에 추가합니다. | [gold]타격[/gold]을 [blue]2[/blue]장 제거합니다. [gold]{Card1}[/gold]을 [gold]덱[/gold]에 추가합니다.
- 페이지 요약: COMBINE_DEFENDS: [b][jitter]깡! 깡!!![/jitter][/b] [orange]융합자[/orange]가 자신의 뼈 모루를 내려칩니다. 한 번의 망치질마다, 전투와 방어, 수비적인 전략에 관한 기억들이 점점 더 선명해집니다. 당신이 지닌 기술의 극치가 당신에게 돌아옵니다. | COMBINE_STRIKES: [b][jitter]깡! 깡!!![/jitter][/b] [orange]융합자[/orange]가 자신의 뼈 모루를 내려칩니다. 한 번의 망치질마다, 전투와 공격, 공격적인 전략에 관한 기억들이 점점 더 선명해집니다. 당신이 지닌 기술의 극치가 당신에게 돌아옵니다. | INITIAL: [b][jitter]깡! 깡!!![/jitter][/b] 벽 건너편에서 금속과 금속이 부딪히는 소리가 울려 퍼집니다... 당신이 벽에 머리를 기대고 귀를 기울이려는 찰나—벽이 갈라지듯 열렸고, [orange]여섯 개의 팔을 지닌 거구의 인물[/orange]이 작업에 몰두하고 있는 광경이 드러납니다. 그의 “얼굴”이라 불릴 만한 곳에는, [gold][sine]빛나는 문양들이 뒤엉켜 소용돌이[/sine][/gold]치고 있습니다. “[aqua]승천하는 영혼[/aqua]을 지닌 자가 내 공방에 찾아온 건가? 좋아, [jitter]결합이다[/jitter]!”
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AMALGAMATOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Amalgamator`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이거 아님 저거?

- ID: `megacrit-sts2-core-models-events-thisorthat`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 근처의 구멍에서 갑자기, [gold]보물이 담긴 수상한 자루[/gold]와 명백히 [purple]저주받은 유물[/purple]을 움켜진 손이 튀어나옵니다.

[jitter][blue]"이거... 아님 저거?"[/blue][/jitter]
날카롭게 긁어대는 목소리가 아래쪽에서 속삭입니다.
- 확인된 선택지:
  - 이거: 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
  - 저거: [red]{Curse}[/red]을 [gold]덱[/gold]에 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 부가 메모: [red]{Curse}[/red]을 [gold]덱[/gold]에 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다. | 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
- 페이지 요약: INITIAL: 근처의 구멍에서 갑자기, [gold]보물이 담긴 수상한 자루[/gold]와 명백히 [purple]저주받은 유물[/purple]을 움켜진 손이 튀어나옵니다. [jitter][blue]"이거... 아님 저거?"[/blue][/jitter] 날카롭게 긁어대는 목소리가 아래쪽에서 속삭입니다. | ORNATE: 당신은 [purple]저주받은 유물[/purple]을 움켜쥐었습니다. 유물을 움켜쥐는 순간, 당신의 손에 수갑이 채워집니다. 마치 [blue][sine]유령이 내는 듯한 웃음소리[/sine][/blue]와 함께 눈부신 [orange]불꽃과 연기[/orange]가 당신의 시야를 가득 채웁니다! 구멍과 팔은 사라져버렸습니다. | PLAIN: 당신이 보물 자루를 움켜쥐는 순간, [blue][jitter]세 번째 손[/jitter][/blue]이 나타나 당신을 붙잡습니다!! [red][jitter]한바탕 몸싸움[/jitter][/red]을 벌이고 나서야 세 번째 손은 당신을 놓았고—팔과 구멍은 갑자기 사라져버렸습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THIS_OR_THAT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ThisOrThat`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전설은 사실이었어

- ID: `megacrit-sts2-core-models-events-thelegendsweretrue`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 칠흑 같이 어두운 방 안으로 들어서자, 위에서 한 줄기 빛이 갑자기 어둠을 가르며 내려오는 동시에, 당신의 뒤쪽에서 문이 쾅하고 닫힙니다.

빛 속에서, 지도가 놓여있는 연단이 당신의 눈앞에 모습을 드러냅니다. 왜인지 강제로 가져가야 할 듯한 느낌이 듭니다.

수상하네요.
- 확인된 선택지:
  - 지도를 가져간다: [gold]보물 지도[/gold]를 얻습니다.
  - 천천히 출구를 찾는다: 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 부가 메모: [gold]보물 지도[/gold]를 얻습니다. | 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 페이지 요약: INITIAL: 칠흑 같이 어두운 방 안으로 들어서자, 위에서 한 줄기 빛이 갑자기 어둠을 가르며 내려오는 동시에, 당신의 뒤쪽에서 문이 쾅하고 닫힙니다. 빛 속에서, 지도가 놓여있는 연단이 당신의 눈앞에 모습을 드러냅니다. 왜인지 강제로 가져가야 할 듯한 느낌이 듭니다. 수상하네요. | NAB_THE_MAP: 뭐, 가져간다고 해서 별 일 있겠어요? 당신이 지도를 손에 들자, 방 건너편에 새로운 출구가 모습을 드러냈습니다. | SLOWLY_FIND_AN_EXIT: 당신이 어둠 속을 조심스럽게 나아가며 탈출구를 찾던 도중, 방이 격렬하게 흔들리기 시작했습니다. 갑작스러운 굉음과 함께, 당신을 둘러싼 방 전체가 무너져 내립니다. 간신히 잔해 속에서 빠져나온 당신은, 방 안에 숨겨져 있었던 것으로 추정되는, 유용한 포션 병 하나를 발견했습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_LEGENDS_WERE_TRUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLegendsWereTrue`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전쟁사학자 레피

- ID: `megacrit-sts2-core-models-events-warhistorianrepy`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]보물 상자[/gold] 옆에 있는 [purple]매달린 철창[/purple] 안에, 한 학자가 갇혀 있습니다.

그 학자는 미친듯이 무언가를 써 내려가며, [sine][orange]“일회용 열쇠”[/orange][/sine]라는 말을 끊임없이 중얼대고 있습니다.

[purple]철창[/purple]이나 [gold]상자[/gold] 중 하나를 열 수 있을 것 같습니다.
- 확인된 선택지:
  - 상자를 연다: [gold]랜턴 열쇠[/gold]를 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
  - 철창을 연다: [gold]랜턴 열쇠[/gold]를 잃습니다. [gold]역사 강의서[/gold]를 얻습니다.
- 부가 메모: [gold]랜턴 열쇠[/gold]를 잃습니다. [gold]역사 강의서[/gold]를 얻습니다. | [gold]랜턴 열쇠[/gold]를 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
- 페이지 요약: INITIAL: [gold]보물 상자[/gold] 옆에 있는 [purple]매달린 철창[/purple] 안에, 한 학자가 갇혀 있습니다. 그 학자는 미친듯이 무언가를 써 내려가며, [sine][orange]“일회용 열쇠”[/orange][/sine]라는 말을 끊임없이 중얼대고 있습니다. [purple]철창[/purple]이나 [gold]상자[/gold] 중 하나를 열 수 있을 것 같습니다. | UNLOCK_CAGE: 철창을 열자 [green]레피[/green]는 당신에게 감사 인사를 전합니다. 그는 [gold]두꺼운 가죽 표지의 책[/gold] 한 권을 당신에게 내밉니다. “역사를 모르는 자는 그것을 되풀이할 수밖에 없는 운명입니다. 하지만 당신도 반복에 꽤나 익숙해 보이시는 것 같군요. 아닌가요?” | UNLOCK_CHEST: “이 학자, [green]레피[/green]를 풀어주지 않으실 건가요? 고전 철학에 따라, 당신은 [red]도덕적인 실패자[/red]로 여겨질 겁니다. 좋아요, 다음번에는 여기에 [purple][jitter]살아있는 사람[/jitter][/purple]이 갇혀있다고 생각해보시죠! 지금 여기 갇혀 있는 사회적으로 높은 가치를 지니지 않는 구성원이 전혀 [purple][jitter]곤란한 상황[/jitter][/purple]에 처해 있지 않다는 것처럼 말입니다!!” “자, 봐요, 이 철창이 얼마나 좁아터졌는지 알겠어요!? 내가 왜 불평하는지도? 이미 피상적이기 짝이 없는 결정을 내리셨군요... 그래, 상자 안에는 뭐가 있었습니까? [gold]포션[/gold]이랑 [gold]유물[/gold] 몇 개? 정말 대단한 것들이네요. [sine]멋지고 말고요. 큰 도움이 되겠어요...[/sine]” 당신이 떠나는 모습을 [green]레피[/green]가 경멸스럽게 지켜봅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WAR_HISTORIAN_REPY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정글 미로 탐험

- ID: `megacrit-sts2-core-models-events-junglemazeadventure`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 공터에서 [purple]거대한 미로[/purple]를 내려다보며 손짓하고 있는 [green]오합지졸 모험가 무리[/green]를 만났습니다.

모험가들은 당신에게, 이 미로의 전리품을 차지하기 위한 [b]장대한 모험[/b]에 함께 협력할 것을 제안합니다. 경험 많은 모험가인 당신은, 이 미로가 [red][jitter]치명적인 함정[/jitter][/red]과 [orange][sine]수호자[/sine][/orange]들로 가득 차 있음을 곧바로 깨닫습니다.

함께 나아가면 더 수월하겠지만, 얻는 전리품은 나눠야 할 것입니다.
- 확인된 선택지:
  - 협력한다: [gold]골드[/gold]를 [blue]{JoinForcesGold}[/blue] 얻습니다.
  - 홀로 탐색한다: [gold]골드[/gold]를 [blue]{SoloGold}[/blue] 얻습니다. 체력을 [red]{SoloHp}[/red] 잃습니다.
- 부가 메모: [gold]골드[/gold]를 [blue]{JoinForcesGold}[/blue] 얻습니다. | [gold]골드[/gold]를 [blue]{SoloGold}[/blue] 얻습니다. 체력을 [red]{SoloHp}[/red] 잃습니다.
- 페이지 요약: INITIAL: 당신은 공터에서 [purple]거대한 미로[/purple]를 내려다보며 손짓하고 있는 [green]오합지졸 모험가 무리[/green]를 만났습니다. 모험가들은 당신에게, 이 미로의 전리품을 차지하기 위한 [b]장대한 모험[/b]에 함께 협력할 것을 제안합니다. 경험 많은 모험가인 당신은, 이 미로가 [red][jitter]치명적인 함정[/jitter][/red]과 [orange][sine]수호자[/sine][/orange]들로 가득 차 있음을 곧바로 깨닫습니다. 함께 나아가면 더 수월하겠지만, 얻는 전리품은 나눠야 할 것입니다. | JOIN_FORCES: {IsMultiplayer:당신과 친구들을|당신을} 포함한 모든 사람들의 얼굴이 즉시 밝아집니다! 모험가들 중 한 명이 음유시인이라는 사실을 알게 됐습니다! 당신과 [red]그라토리안 전사[/red]가 [orange]가시 곤봉을 든 경비병[/orange]들을 무난하게 제거하는 동안, 함정을 손쉽게 무력화하는 [aqua]오징어 마법사 아퀴드[/aqua]와 함께 당신은 즐겁게 미로 속으로 걸어 들어갔습니다. 이번 모험은 당신의 인생에서 가장 즐거운 모험이었고, 정말로 [gold][sine]멋진 추억[/sine][/gold]을 만들었습니다. | SOLO_QUEST: 사람을 피하고 싶어진 {IsMultiplayer:당신과 친구들은|당신은}, 안 좋은 표정의 모험가들을 말없이 지나쳐 갑니다. 그들 중 한 명은 심지어 당신에게 [jitter][purple]주먹을 휘두르기도 합니다[/purple][/jitter]. 미로는 방문객을 거부합니다. 당신은 미로를 나아가는 동안 가시 함정에 빠지고, 거대한 가시 통나무에 맞고, [orange]가시 곤봉을 든 난폭한 생물[/orange]들과 전투를 펼쳤습니다. 가시라면 지긋지긋해진 당신은, 다음 모험에서는 팀워크를 고려해 보기로 합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JUNGLE_MAZE_ADVENTURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.JungleMazeAdventure`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정령 접합자

- ID: `megacrit-sts2-core-models-events-spiritgrafter`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 위쪽에서 고치가 [jitter]흔들리며 꿈틀거리고[/jitter], 금방이라도 터져 나올 것 같습니다!

...이윽고 꿈틀거림이 멈춥니다. [orange][sine]고치가 타오르기 시작합니다[/sine][/orange]! [jitter]무슨 일이 일어나고 있는 거죠?[/jitter]
번쩍이는 [gold]빛[/gold]과 [red]불꽃[/red] 속에서, [orange]정령 접합자[/orange]가 당신에게 뛰어들어 당신과 하나가 되어 완전해지려고 합니다.
- 확인된 선택지:
  - 거부한다: 체력을 [red]{RejectionHpLoss}[/red] 잃습니다. [gold]덱[/gold]에서 카드를 1장 제거합니다.
  - 받아들인다: 체력을 [green]{LetItInHealAmount}[/green] 회복합니다. [gold]덱[/gold]에 [gold]탈바꿈[/gold]을 추가합니다.
- 부가 메모: 체력을 [green]{LetItInHealAmount}[/green] 회복합니다. [gold]덱[/gold]에 [gold]탈바꿈[/gold]을 추가합니다. | 체력을 [red]{RejectionHpLoss}[/red] 잃습니다. [gold]덱[/gold]에서 카드를 1장 제거합니다.
- 페이지 요약: INITIAL: 위쪽에서 고치가 [jitter]흔들리며 꿈틀거리고[/jitter], 금방이라도 터져 나올 것 같습니다! ...이윽고 꿈틀거림이 멈춥니다. [orange][sine]고치가 타오르기 시작합니다[/sine][/orange]! [jitter]무슨 일이 일어나고 있는 거죠?[/jitter] 번쩍이는 [gold]빛[/gold]과 [red]불꽃[/red] 속에서, [orange]정령 접합자[/orange]가 당신에게 뛰어들어 당신과 하나가 되어 완전해지려고 합니다. | LET_IT_IN: [orange]정령 접합자[/orange]가 당신과 융합합니다. 생명력 넘치는 동족의 영혼이 당신을 빠르게 [green]치유합니다[/green]. 당신은 [blue]새롭게 태어났습니다[/blue]. | REJECTION: [orange]정령 접합자[/orange]는 당신의 굳은 의지에 의해 손쉽게 저지됩니다. 하지만 그것은 숙주를 갈망한 나머지, 계속해서 당신에게로 날아듭니다. 마침내 힘이 다한 그 정령은, [purple][sine]휙[/sine][/purple] 소리와 함께 사라집니다. [sine]덧없는 정령이네요.[/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPIRIT_GRAFTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiritGrafter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 치즈로 가득한 방

- ID: `megacrit-sts2-core-models-events-roomfullofcheese`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 방은 [b][jitter]치즈[/jitter][/b]로 가득 차 있습니다!!!

함정이 없는지 주변을 살펴보다 위를 올려다보니...더 많은 [b]치즈[/b]가 있습니다. 이 방의 양초는 [b]치즈[/b]로 만들어져 있는 것으로 보입니다. 방의 가구도 [b]치즈[/b]로 만들어져 있습니다.
갑자기, 당신의 뒤에 있던 문이 쾅 하고 닫힙니다.

문도 [b]치즈[/b]로 만들어져 있습니다.
- 확인된 선택지:
  - 잔뜩 먹는다: 무작위 [gold]일반[/gold] 카드 [blue]8[/blue]장 중 [blue]2[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.
  - 탐색한다: 체력을 [red]{Damage}[/red] 잃습니다. [gold]엄선된 치즈[/gold]를 얻습니다.
- 부가 메모: 무작위 [gold]일반[/gold] 카드 [blue]8[/blue]장 중 [blue]2[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다. | 체력을 [red]{Damage}[/red] 잃습니다. [gold]엄선된 치즈[/gold]를 얻습니다.
- 페이지 요약: GORGE: [b]치즈[/b]를 한 입 베어물자, 더 많은 [b]치즈[/b]를 먹고 싶은 욕구가 되려 솟아납니다. [sine]시간이 흘러갑니다...[/sine] 당신은 감탄이 절로 나올 양의 [b]치즈[/b]를 먹어치웠습니다. 오늘밤 꿈에는 [b]치즈[/b]가 나올 것 같네요. | INITIAL: 이 방은 [b][jitter]치즈[/jitter][/b]로 가득 차 있습니다!!! 함정이 없는지 주변을 살펴보다 위를 올려다보니...더 많은 [b]치즈[/b]가 있습니다. 이 방의 양초는 [b]치즈[/b]로 만들어져 있는 것으로 보입니다. 방의 가구도 [b]치즈[/b]로 만들어져 있습니다. 갑자기, 당신의 뒤에 있던 문이 쾅 하고 닫힙니다. 문도 [b]치즈[/b]로 만들어져 있습니다. | SEARCH: 당신은 [sine]몇 시간이고[/sine] 방 안을 뒤적이며 헤맵니다… [b]치즈[/b]들이 풍기는 압도적인 존재감 속에서 당신의 정신은 기묘한 곳을 떠돌며, 시간은 흘러가고 당신의 존재는 혼미해집니다... 깜짝 놀라 정신을 차리고 보니, 당신은 무언가를 쥐고 있다는 것을 깨닫게 됩니다. [b]엄선된 치즈[/b]입니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROOM_FULL_OF_CHEESE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 침몰한 조각상

- ID: `megacrit-sts2-core-models-events-sunkenstatue`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 연못에 반쯤 잠겨 있는 오래된 석상을 발견했습니다. 석상의 두 손은 [blue]돌 검[/blue] 위에 조심스레 얹혀 있습니다.

연못 바닥에는 무언가가 [gold][sine]반짝거리고[/sine][/gold] 있습니다.
누군가에게 바치기 위한 것들일까요?

저 돈들은 꽤나 도움이 될 것 같습니다...
- 확인된 선택지:
  - 검을 집는다: [gold]{Relic}[/gold]을 얻습니다.
  - 물로 뛰어든다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 체력을 [red]{HpLoss}[/red] 잃습니다.
- 부가 메모: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 체력을 [red]{HpLoss}[/red] 잃습니다. | [gold]{Relic}[/gold]을 얻습니다.
- 페이지 요약: DIVE_INTO_WATER: 당신은 물속으로 뛰어들었습니다! [gold]봉헌물[/gold]을 챙기던 와중에 [sine][blue]둔탁한 소리[/blue][/sine]가 울려 퍼지고, 돌들이 날아와 당신을 때려대기 시작합니다! 수면 위로 올라오자, 당신의 간섭으로 인해 이 오래된 석상이 [red]물리적[/red]으로도 [purple]영적[/purple]으로도 붕괴되어버린 모습이 보입니다. | GRAB_SWORD: 조각상의 손에서 [blue]검[/blue]을 [jitter]빼내니[/jitter], 석상 전체가 한번에 무너져 내립니다! 연못 바닥에서 [gold][jitter]반짝거리던 것들[/jitter][/gold]을 지금 집으러 가기에는 너무 위험해 보입니다... [blue]검[/blue]을 자세히 살펴보니, 검 안에 [green]잠들어있는 놀라운 힘[/green]이 느껴집니다. | INITIAL: 당신은 연못에 반쯤 잠겨 있는 오래된 석상을 발견했습니다. 석상의 두 손은 [blue]돌 검[/blue] 위에 조심스레 얹혀 있습니다. 연못 바닥에는 무언가가 [gold][sine]반짝거리고[/sine][/gold] 있습니다. 누군가에게 바치기 위한 것들일까요? 저 돈들은 꽤나 도움이 될 것 같습니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUNKEN_STATUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenStatue`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혼돈의 향기

- ID: `megacrit-sts2-core-models-events-aromaofchaos`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 울창한 덤불을 헤치고 나와 공터에 다다른 당신은, 정체를 알 수 없는 그리움에 사로잡힙니다.

[purple]꽃 향기[/purple]와 [green]썩은 냄새[/green], 그리고 [orange]전혀 다른 어떤 향기[/orange]가 한데 뒤섞여 풍겨옵니다. 걸음을 옮길 때마다 향기는 점점 더 진해지고, 주변 세상이 [sine]일그러지고 뒤틀리는[/sine] 듯한 느낌이 듭니다.

[sine][rainbow freq=0.3 sat=0.8 val=1]변화하는 혼돈[/rainbow][/sine]의 감각에 압도당한 채, 당신은 점점 자신을 잃어가는 듯한 느낌에 사로잡힙니다.
- 확인된 선택지:
  - 정신을 붙잡는다: [gold]덱[/gold]에 있는 카드를 1장 [gold]강화[/gold]합니다.
  - 향기에 몸을 맡긴다: [gold]덱[/gold]에 있는 카드를 1장 [gold]변화[/gold]시킵니다.
- 부가 메모: [gold]덱[/gold]에 있는 카드를 1장 [gold]변화[/gold]시킵니다. | [gold]덱[/gold]에 있는 카드를 1장 [gold]강화[/gold]합니다.
- 페이지 요약: INITIAL: 울창한 덤불을 헤치고 나와 공터에 다다른 당신은, 정체를 알 수 없는 그리움에 사로잡힙니다. [purple]꽃 향기[/purple]와 [green]썩은 냄새[/green], 그리고 [orange]전혀 다른 어떤 향기[/orange]가 한데 뒤섞여 풍겨옵니다. 걸음을 옮길 때마다 향기는 점점 더 진해지고, 주변 세상이 [sine]일그러지고 뒤틀리는[/sine] 듯한 느낌이 듭니다. [sine][rainbow freq=0.3 sat=0.8 val=1]변화하는 혼돈[/rainbow][/sine]의 감각에 압도당한 채, 당신은 점점 자신을 잃어가는 듯한 느낌에 사로잡힙니다. | LET_GO: 당신은 굴복했고, 정신이 [sine][purple]흐릿해지기[/purple][/sine] 시작합니다… [blue]친구들과 어울리며[/blue] [green]팀을 이뤄 싸우고[/green], [gold]엄청난 적들과 싸우는[/gold] 당신의 모습이 눈앞에 아른거립니다. 이건 꿈일까요? 현실일까요? 아니면 둘 다일까요? 수없이 많은 눈들이 당신과 당신의 꿈을 인지합니다. 그들은 누구일까요...? 얼마인지 알 수 없는 시간이 흐른 뒤, 당신은 깨어납니다. 향기는 점차 희미해지고, 의식이 선명해짐에 따라 그 효과도 사라집니다. 안정을 취하고 나니, 그 어느 때보다도 머리가 맑아진 느낌이 듭니다. [sine][red]그 눈들은 뭐였을까요?[/red][/sine] | MAINTAIN_CONTROL: 당신은 굴복하고 싶다는 유혹을 억누르며 향기가 닿지 않는 나무 위로 올라갔지만, 풍겨오는 향기의 힘은 여전히 당신의 마음을 파고듭니다. 제정신을 붙잡고 있기 힘들지만, 당신은 자신의 생각을 스스로의 [blue]핵심 원칙[/blue]에 단단히 묶어둡니다. {AromaPrinciple} {AromaPrinciple} {AromaPrinciple} 향기가 가라앉습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AROMA_OF_CHAOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AromaOfChaos`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가짜 이벤트

- ID: `megacrit-sts2-core-models-events-mocks-mockeventmodel`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 가짜 이벤트입니다.
- 확인된 선택지:
  - 가짜 선택지: 가짜 선택지입니다.
- 부가 메모: 가짜 선택지입니다.
- 페이지 요약: INITIAL: 가짜 이벤트입니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MOCK_EVENT_MODEL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Mocks.MockEventModel`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 오로바스

- ID: `megacrit-sts2-core-models-events-orobas`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 시작 유물이나 카드가 없습니다.
- 확인된 선택지:
  - 잠김: 시작 유물이나 카드가 없습니다.
- 부가 메모: 시작 유물이나 카드가 없습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OROBAS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Orobas`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Orobas

### 你好世界

- ID: `filename----event-beta-hello-world-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 내 턴 시작 시, 무작위 일반 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HELLO_WORLD`
- 리소스 경로: `"filename": "event/beta/hello_world.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 격돌

- ID: `filename----event-beta-clash-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]손[/gold]에 있는 모든 카드가 공격 카드일 때만 사용할 수 있습니다.
피해를 {Damage:diff()} 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CLASH`
- 리소스 경로: `"filename": "event/beta/clash.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 계몽

- ID: `filename----event-enlightenment-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 {IfUpgraded:show:전투|턴} 동안, [gold]손[/gold]에 있는 모든 카드의 비용이 1로 감소합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENLIGHTENMENT`
- 리소스 경로: `"filename": "event/enlightenment.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 고리형 강인함

- ID: `filename----event-beta-toric-toughness-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 {Block:diff()} 얻습니다.
다음 {Turns:diff()}턴 동안, 턴 시작 시 [gold]방어도[/gold]를 {Block:diff()} 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TORIC_TOUGHNESS`
- 리소스 경로: `"filename": "event/beta/toric_toughness.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광란의 포식

- ID: `filename----event-beta-feeding-frenzy-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이번 턴 동안 [gold]힘[/gold]을 {StrengthPower:diff()} 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FEEDING_FRENZY`
- 리소스 경로: `"filename": "event/beta/feeding_frenzy.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 광신자들

- ID: `megacrit-sts2-core-timeline-epochs-event1epoch`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter]“까악... 까악 까악!!”[/jitter]
정말 굴욕적인 패배였습니다... 그들의 예배 장소와 제물은 감당할 수 없는 침입자들에 의해 몇 번이고 훼손되었습니다.

[blue]마잘레스[/blue]가 우리를 버린 걸까요?

벌레들의 개체 수가 급증하면서, [gold]첨탑[/gold] 내부의 식량은 점점 부족해졌습니다. [blue]무리[/blue]는 [gold]첨탑[/gold]의 더 아래 층으로 도망쳐, [green][sine]해초[/sine][/green]와 [purple][jitter]하수구의 생물들[/jitter][/purple]로 근근히 살아가고 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EVENT1_EPOCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Timeline.Epochs.Event1Epoch`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 구식 이벤트

- ID: `megacrit-sts2-core-models-events-deprecatedevent`
- 그룹/풀 추정: 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 이벤트는 게임에서 삭제되었습니다.
- 페이지 요약: INITIAL: 이 이벤트는 게임에서 삭제되었습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEPRECATED_EVENT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DeprecatedEvent`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 날뛰기

- ID: `filename----event-rip-and-tear-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 적에게 피해를 {Damage:diff()}만큼 2번 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RIP_AND_TEAR`
- 리소스 경로: `"filename": "event/rip_and_tear.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 니오우의 격분

- ID: `filename----event-neows-fury-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
[gold]버린 카드 더미[/gold]에서 무작위 카드를 {Cards:choose(1):1장|{:diff()}장} [gold]손[/gold]으로 가져옵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEOWS_FURY`
- 리소스 경로: `"filename": "event/neows_fury.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 돌아오다

- ID: `megacrit-sts2-core-timeline-epochs-event2epoch`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 고대의 존재들은 자신들의 은신처를 벗어나, [gold]첨탑[/gold]을 향해 나아갔습니다...

그들이 그 축복을 받은 지도 너무 오랜 시간이 흘렀습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EVENT2_EPOCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Timeline.Epochs.Event2Epoch`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 되돌리기

- ID: `filename----event-beta-rebound-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
이번 턴에 다음으로 사용하는 카드를 [gold]뽑을 카드 더미[/gold] 맨 위에 놓습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REBOUND`
- 리소스 경로: `"filename": "event/beta/rebound.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 마름쇠

- ID: `filename----event-beta-caltrops-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격을 받을 때마다, 공격한 적에게 피해를 {ThornsPower:diff()} 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `CALTROPS`
- 리소스 경로: `"filename": "event/beta/caltrops.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 미지

- ID: `res---scenes-vfx-events-dense-vegetation-slice-vfx-tscnh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이 적의 의도는 [gold]알 수 없습니다[/gold].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNKNOWN`
- 리소스 경로: `res://scenes/vfx/events/dense_vegetation_slice_vfx.tscnH/`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 박멸

- ID: `filename----event-exterminate-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 적에게 피해를 {Damage:diff()}만큼 {Repeat:diff()}번 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EXTERMINATE`
- 리소스 경로: `"filename": "event/exterminate.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 방해

- ID: `filename----event-beta-distraction-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무작위 스킬 카드를 1장 [gold]손[/gold]으로 가져옵니다. 이번 턴 동안 그 카드를 비용 없이 사용할 수 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DISTRACTION`
- 리소스 경로: `"filename": "event/beta/distraction.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 상?인

- ID: `megacrit-sts2-core-models-events-fakemerchant`
- 그룹/풀 추정: 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 뭐야--?!
- 부가 메모: 뭐야--?! | 그래, 그렇게 나오시겠다?! | 건드릴 상인 잘못 골랐어, 꼬마 친구. | 이게 정말...
- 페이지 요약: INITIAL: 플레이스홀더
- 대사 프리뷰: 뭐야--?! | 그래, 그렇게 나오시겠다?! | 건드릴 상인 잘못 골랐어, 꼬마 친구.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FAKE_MERCHANT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FakeMerchant`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 섀 급습

- ID: `filename----event-byrd-swoop-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BYRD_SWOOP`
- 리소스 경로: `"filename": "event/byrd_swoop.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 스택

- ID: `filename----event-beta-stack-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]버린 카드 더미[/gold]에 있는 카드의 수{IfUpgraded:show: +{CalculationBase}|}만큼 [gold]방어도[/gold]를 얻습니다.{InCombat:
([gold]방어도[/gold]를 {CalculatedBlock:diff()} 얻습니다.)|}
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STACK`
- 리소스 경로: `"filename": "event/beta/stack.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 신성

- ID: `filename----event-apotheosis-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모든 카드를 [gold]강화[/gold]합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `APOTHEOSIS`
- 리소스 경로: `"filename": "event/apotheosis.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 안식

- ID: `filename----event-beta-relax-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]를 {Block:diff()} 얻습니다.
다음 턴에, 카드를 {Cards:diff()}장 뽑고 {Energy:energyIcons()}를 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RELAX`
- 리소스 경로: `"filename": "event/beta/relax.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 압도

- ID: `filename----event-beta-outmaneuver-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 다음 턴에, {Energy:energyIcons()}를 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `OUTMANEUVER`
- 리소스 경로: `"filename": "event/beta/outmaneuver.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 염원

- ID: `filename----event-wish-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에서 카드를 1장 [gold]손[/gold]으로 가져옵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WISH`
- 리소스 경로: `"filename": "event/wish.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 영체화

- ID: `filename----event-beta-apparition-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]불가침[/gold]을 {IntangiblePower:diff()} 얻습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `APPARITION`
- 리소스 경로: `"filename": "event/beta/apparition.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 위험천만한 이벤트

- ID: `megacrit-sts2-core-models-modifiers-deadlyevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 미지 방에 이제 엘리트가 등장할 수 있게 되지만, 보물 방이 등장할 확률도 증가합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DEADLY_EVENTS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Modifiers.DeadlyEvents`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이도류

- ID: `filename----event-beta-dual-wield-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공격이나 파워 카드를 1장 선택합니다. 그 카드의 복사본을 {IfUpgraded:show:{Cards}장|1장} [gold]손[/gold]으로 가져옵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DUAL_WIELD`
- 리소스 경로: `"filename": "event/beta/dual_wield.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 짓누르기

- ID: `filename----event-squash-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
[gold]취약[/gold]을 {VulnerablePower:diff()} 부여합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SQUASH`
- 리소스 경로: `"filename": "event/squash.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쪼기

- ID: `filename----event-peck-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()}만큼 {Repeat:diff()}번 줍니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PECK`
- 리소스 경로: `"filename": "event/peck.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 찬란한 불꽃

- ID: `filename----event-beta-brightest-flame-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: {Energy:energyIcons()}를 얻습니다.
카드를 {Cards:diff()}장 뽑습니다.
최대 체력을 {MaxHp:diff()} 잃습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BRIGHTEST_FLAME`
- 리소스 경로: `"filename": "event/beta/brightest_flame.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 참호

- ID: `filename----event-beta-entrench-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]방어도[/gold]가 2배가 됩니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENTRENCH`
- 리소스 경로: `"filename": "event/beta/entrench.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 첨탑

- ID: `megacrit-sts2-core-timeline-epochs-event3epoch`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 그 뿌리들은 더 이상 [orange]프리온[/orange]으로부터 양분을 끌어올 수 없었고, [gold]고대의 나무들[/gold]은 시들어 죽어가기 시작했습니다… 한때는 아름다웠던 숲도 결국 영원의 무게 앞에 무너져 내리고 말았습니다.

니오우의 나무조차도 가지와 푸른 잎을 잃었고, 마지막으로 남은 나무는 더 이상 나무라 칭할 수 없었습니다.
그들은 그것에 이름을 붙였습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EVENT3_EPOCH`
- 모델 클래스: `MegaCrit.Sts2.Core.Timeline.Epochs.Event3Epoch`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탈바꿈

- ID: `filename----event-metamorphosis-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]뽑을 카드 더미[/gold]에 무작위 공격 카드를 {Cards:diff()}장 추가합니다. 이번 전투 동안 그 카드들을 비용 없이 사용할 수 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `METAMORPHOSIS`
- 리소스 경로: `"filename": "event/metamorphosis.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 할퀴기

- ID: `filename----event-beta-maul-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()}만큼 2번 줍니다.
이번 전투 동안 모든 할퀴기 카드의 피해량이 {Increase:diff()} 증가합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MAUL`
- 리소스 경로: `"filename": "event/beta/maul.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 호각 불기

- ID: `filename----event-beta-whistle-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 피해를 {Damage:diff()} 줍니다.
적을 [gold]기절[/gold]시킵니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHISTLE`
- 리소스 경로: `"filename": "event/beta/whistle.png",`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### "filename": "event1 epoch

- ID: `filename----event1-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event1_epoch.png",`

### "filename": "event2 epoch

- ID: `filename----event2-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event2_epoch.png",`

### "filename": "event3 epoch

- ID: `filename----event3-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event3_epoch.png",`

### <0>  Event Or Deprecated

- ID: `0---eventordeprecated`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <6>  Event Info Ctor Param Init

- ID: `6---eventinfoctorparaminit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <<Generate Initial Options>b  0>d

- ID: `megacrit-sts2-core-models-events-ranwidtheelder---c--displayclass18-0---generateinitialoptions-b--0-d`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder+<>c__DisplayClass18_0+<<GenerateInitialOptions>b__0>d`

### <<Generate Initial Options>b  2>d

- ID: `megacrit-sts2-core-models-events-ranwidtheelder---c--displayclass18-0---generateinitialoptions-b--2-d`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder+<>c__DisplayClass18_0+<<GenerateInitialOptions>b__2>d`

### <>c

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<>c`

### <>c

- ID: `megacrit-sts2-core-devconsole-consolecommands-eventconsolecmd---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.EventConsoleCmd+<>c`

### <>c

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-amalgamator---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Amalgamator+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-battleworndummy---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BattlewornDummy+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-byrdonisnest---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ByrdonisNest+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-colorfulphilosophers---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColorfulPhilosophers+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-colossalflower---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-crystalsphere---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.CrystalSphere+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-darv---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Darv+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-doorsoflightanddark---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DoorsOfLightAndDark+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-endlessconveyor---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-fakemerchant---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FakeMerchant+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-fieldofmansizedholes---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FieldOfManSizedHoles+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-infestedautomaton---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.InfestedAutomaton+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-neow---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Neow+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-orobas---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Orobas+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-pael---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Pael+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-ranwidtheelder---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-reflections---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Reflections+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-roomfullofcheese---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-slipperybridge---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SlipperyBridge+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-spiralingwhirlpool---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiralingWhirlpool+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-stoneofalltime---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.StoneOfAllTime+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-tabletoftruth---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-tanx---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Tanx+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-teamaster---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-thearchitect---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-thelegendsweretrue---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLegendsWereTrue+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-trashheap---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TrashHeap+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-unrestsite---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.UnrestSite+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-warhistorianrepy---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-welcometowongos---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<>c`

### <>c

- ID: `megacrit-sts2-core-models-events-woodcarvings---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings+<>c`

### <>c

- ID: `megacrit-sts2-core-multiplayer-game-eventsynchronizer---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Game.EventSynchronizer+<>c`

### <>c

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+<>c`

### <>c

- ID: `megacrit-sts2-core-nodes-events-eventsplitvoteanimation---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.EventSplitVoteAnimation+<>c`

### <>c

- ID: `megacrit-sts2-core-rooms-combateventvisuals---c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Rooms.CombatEventVisuals+<>c`

### <>c  Display Class0 0

- ID: `megacrit-sts2-core-models-events-luminouschoir---c--displayclass0-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LuminousChoir+<>c__DisplayClass0_0`

### <>c  Display Class10 0

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler---c--displayclass10-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<>c__DisplayClass10_0`

### <>c  Display Class11 0

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler---c--displayclass11-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<>c__DisplayClass11_0`

### <>c  Display Class12 0`1

- ID: `megacrit-sts2-core-models-events-selfhelpbook---c--displayclass12-0-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<>c__DisplayClass12_0`1`

### <>c  Display Class13 0

- ID: `megacrit-sts2-core-models-events-tinkertime---c--displayclass13-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TinkerTime+<>c__DisplayClass13_0`

### <>c  Display Class13 0`1

- ID: `megacrit-sts2-core-models-events-selfhelpbook---c--displayclass13-0-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<>c__DisplayClass13_0`1`

### <>c  Display Class16 0

- ID: `megacrit-sts2-core-models-events-dollroom---c--displayclass16-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+<>c__DisplayClass16_0`

### <>c  Display Class18 0

- ID: `megacrit-sts2-core-models-events-orobas---c--displayclass18-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Orobas+<>c__DisplayClass18_0`

### <>c  Display Class18 0

- ID: `megacrit-sts2-core-models-events-ranwidtheelder---c--displayclass18-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder+<>c__DisplayClass18_0`

### <>c  Display Class18 0

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen---c--displayclass18-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+<>c__DisplayClass18_0`

### <>c  Display Class24 0

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen---c--displayclass24-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+<>c__DisplayClass24_0`

### <>c  Display Class3 0

- ID: `megacrit-sts2-core-models-events-graveoftheforgotten---c--displayclass3-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.GraveOfTheForgotten+<>c__DisplayClass3_0`

### <>c  Display Class30 0

- ID: `megacrit-sts2-core-nodes-events-neventlayout---c--displayclass30-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout+<>c__DisplayClass30_0`

### <>c  Display Class37 0

- ID: `megacrit-sts2-core-models-events-neow---c--displayclass37-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Neow+<>c__DisplayClass37_0`

### <>c  Display Class4 0

- ID: `megacrit-sts2-core-models-events-colorfulphilosophers---c--displayclass4-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColorfulPhilosophers+<>c__DisplayClass4_0`

### <>c  Display Class5 0

- ID: `megacrit-sts2-core-models-events-sapphireseed---c--displayclass5-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SapphireSeed+<>c__DisplayClass5_0`

### <>c  Display Class6 0

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler---c--displayclass6-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<>c__DisplayClass6_0`

### <>c  Display Class65 0

- ID: `megacrit-sts2-core-events-eventoption---c--displayclass65-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.EventOption+<>c__DisplayClass65_0`

### <>c  Display Class69 0

- ID: `megacrit-sts2-core-models-ancienteventmodel---c--displayclass69-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.AncientEventModel+<>c__DisplayClass69_0`

### <>c  Display Class8 0

- ID: `megacrit-sts2-core-devconsole-consolecommands-eventconsolecmd---c--displayclass8-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.EventConsoleCmd+<>c__DisplayClass8_0`

### <>c  Display Class9 0

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler---c--displayclass9-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<>c__DisplayClass9_0`

### <>O

- ID: `megacrit-sts2-core-models-ancienteventmodel---o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.AncientEventModel+<>O`

### <>O

- ID: `megacrit-sts2-core-models-events-symbiote---o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Symbiote+<>O`

### <>O

- ID: `megacrit-sts2-core-nodes-rooms-neventroom---o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+<>O`

### <Abstain>d  10

- ID: `megacrit-sts2-core-models-events-abyssalbaths--abstain-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths+<Abstain>d__10`

### <Accept>d  7

- ID: `megacrit-sts2-core-models-events-graveoftheforgotten--accept-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.GraveOfTheForgotten+<Accept>d__7`

### <Add And Preview>d  5`1

- ID: `megacrit-sts2-core-models-events-bugslayer--addandpreview-d--5-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Bugslayer+<AddAndPreview>d__5`1`

### <Add Guilty>d  6

- ID: `megacrit-sts2-core-models-events-wellspring--addguilty-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Wellspring+<AddGuilty>d__6`

### <Advance Dialogue>d  46

- ID: `megacrit-sts2-core-models-events-thearchitect--advancedialogue-d--46`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<AdvanceDialogue>d__46`

### <After Preventing Block Clear>d  60

- ID: `megacrit-sts2-core-hooks-hook--afterpreventingblockclear-d--60`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Hooks.Hook+<AfterPreventingBlockClear>d__60`

### <After Preventing Death>d  13

- ID: `megacrit-sts2-core-models-powers-mocks-mockrevivepower--afterpreventingdeath-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Powers.Mocks.MockRevivePower+<AfterPreventingDeath>d__13`

### <After Preventing Death>d  5

- ID: `megacrit-sts2-core-models-powers-mocks-mockpreventdeathpower--afterpreventingdeath-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Powers.Mocks.MockPreventDeathPower+<AfterPreventingDeath>d__5`

### <After Preventing Death>d  61

- ID: `megacrit-sts2-core-hooks-hook--afterpreventingdeath-d--61`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Hooks.Hook+<AfterPreventingDeath>d__61`

### <After Preventing Draw>d  62

- ID: `megacrit-sts2-core-hooks-hook--afterpreventingdraw-d--62`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Hooks.Hook+<AfterPreventingDraw>d__62`

### <Anim Architect Attack If Necessary>d  50

- ID: `megacrit-sts2-core-models-events-thearchitect--animarchitectattackifnecessary-d--50`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<AnimArchitectAttackIfNecessary>d__50`

### <Anim Player Attack If Necessary>d  48

- ID: `megacrit-sts2-core-models-events-thearchitect--animplayerattackifnecessary-d--48`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<AnimPlayerAttackIfNecessary>d__48`

### <Approach>d  5

- ID: `megacrit-sts2-core-models-events-symbiote--approach-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Symbiote+<Approach>d__5`

### <Arachnid Acupuncture>d  9

- ID: `megacrit-sts2-core-models-events-zenweaver--arachnidacupuncture-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ZenWeaver+<ArachnidAcupuncture>d__9`

### <Bathe>d  5

- ID: `megacrit-sts2-core-models-events-wellspring--bathe-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Wellspring+<Bathe>d__5`

### <Before Event Started>d  62

- ID: `megacrit-sts2-core-models-ancienteventmodel--beforeeventstarted-d--62`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.AncientEventModel+<BeforeEventStarted>d__62`

### <Before Option Chosen>d  29

- ID: `megacrit-sts2-core-nodes-rooms-neventroom--beforeoptionchosen-d--29`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+<BeforeOptionChosen>d__29`

### <Before Shared Option Chosen>d  31

- ID: `megacrit-sts2-core-nodes-events-neventlayout--beforesharedoptionchosen-d--31`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout+<BeforeSharedOptionChosen>d__31`

### <Begin Event>d  30

- ID: `megacrit-sts2-core-models-eventmodel--beginevent-d--30`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.EventModel+<BeginEvent>d__30`

### <Big Mushroom>d  1

- ID: `megacrit-sts2-core-models-events-hungryformushrooms--bigmushroom-d--1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.HungryForMushrooms+<BigMushroom>d__1`

### <Bird>d  7

- ID: `megacrit-sts2-core-models-events-woodcarvings--bird-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings+<Bird>d__7`

### <Bloody Ink>d  8

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium--bloodyink-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium+<BloodyInk>d__8`

### <Bone Tea>d  8

- ID: `megacrit-sts2-core-models-events-teamaster--bonetea-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster+<BoneTea>d__8`

### <Bottle Option>d  5

- ID: `megacrit-sts2-core-models-events-drowningbeacon--bottleoption-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DrowningBeacon+<BottleOption>d__5`

### <Bottle>d  4

- ID: `megacrit-sts2-core-models-events-wellspring--bottle-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Wellspring+<Bottle>d__4`

### <Breathing Techniques>d  7

- ID: `megacrit-sts2-core-models-events-zenweaver--breathingtechniques-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ZenWeaver+<BreathingTechniques>d__7`

### <Buy Bargain Bin>d  19

- ID: `megacrit-sts2-core-models-events-welcometowongos--buybargainbin-d--19`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<BuyBargainBin>d__19`

### <Buy Featured Item>d  21

- ID: `megacrit-sts2-core-models-events-welcometowongos--buyfeatureditem-d--21`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<BuyFeaturedItem>d__21`

### <Buy Mystery Box>d  20

- ID: `megacrit-sts2-core-models-events-welcometowongos--buymysterybox-d--20`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<BuyMysteryBox>d__20`

### <Canonical Event>k  Backing Field

- ID: `canonicalevent-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Caviar>d  23

- ID: `megacrit-sts2-core-models-events-endlessconveyor--caviar-d--23`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<Caviar>d__23`

### <Cell Clicked>d  51

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame--cellclicked-d--51`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+<CellClicked>d__51`

### <Check Obtain Wongo Badge>d  18

- ID: `megacrit-sts2-core-models-events-welcometowongos--checkobtainwongobadge-d--18`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<CheckObtainWongoBadge>d__18`

### <Choose Doll And Show Description>d  17

- ID: `megacrit-sts2-core-models-events-dollroom--choosedollandshowdescription-d--17`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+<ChooseDollAndShowDescription>d__17`

### <Choose Random>d  13

- ID: `megacrit-sts2-core-models-events-dollroom--chooserandom-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+<ChooseRandom>d__13`

### <Chosen>d  63

- ID: `megacrit-sts2-core-events-eventoption--chosen-d--63`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.EventOption+<Chosen>d__63`

### <Claim>d  8

- ID: `megacrit-sts2-core-models-events-lostwisp--claim-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LostWisp+<Claim>d__8`

### <Clam Roll>d  22

- ID: `megacrit-sts2-core-models-events-endlessconveyor--clamroll-d--22`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<ClamRoll>d__22`

### <Clear Cell>d  52

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame--clearcell-d--52`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+<ClearCell>d__52`

### <Click Event Proceed If Needed>b  0

- ID: `clickeventproceedifneeded-b--0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Click Event Proceed If Needed>b  26 1

- ID: `clickeventproceedifneeded-b--26-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Click Event Proceed If Needed>d  26

- ID: `megacrit-sts2-core-autoslay-autoslayer--clickeventproceedifneeded-d--26`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.AutoSlayer+<ClickEventProceedIfNeeded>d__26`

### <Climb Option>d  6

- ID: `megacrit-sts2-core-models-events-drowningbeacon--climboption-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DrowningBeacon+<ClimbOption>d__6`

### <Combine Defends>d  5

- ID: `megacrit-sts2-core-models-events-amalgamator--combinedefends-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Amalgamator+<CombineDefends>d__5`

### <Combine Strikes>d  4

- ID: `megacrit-sts2-core-models-events-amalgamator--combinestrikes-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Amalgamator+<CombineStrikes>d__4`

### <Complete Minigame>d  55

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame--completeminigame-d--55`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+<CompleteMinigame>d__55`

### <Confront>d  6

- ID: `megacrit-sts2-core-models-events-graveoftheforgotten--confront-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.GraveOfTheForgotten+<Confront>d__6`

### <Continue Fight>d  5

- ID: `megacrit-sts2-core-models-events-roundteaparty--continuefight-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoundTeaParty+<ContinueFight>d__5`

### <Create Event Choice Metric>b  1

- ID: `create-eventchoicemetric-b--1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Choice Metric>b  55 0

- ID: `create-eventchoicemetric-b--55-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Choice Metric>b  55 2

- ID: `create-eventchoicemetric-b--55-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Info>b  1

- ID: `create-eventinfo-b--1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Info>b  49 0

- ID: `create-eventinfo-b--49-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Info>b  49 2

- ID: `create-eventinfo-b--49-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Option History Entry>b  1

- ID: `create-eventoptionhistoryentry-b--1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Option History Entry>b  153 0

- ID: `create-eventoptionhistoryentry-b--153-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create Event Option History Entry>b  153 2

- ID: `create-eventoptionhistoryentry-b--153-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create List Event Choice Metric>b  124 0

- ID: `create-listeventchoicemetric-b--124-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Create List Event Option History Entry>b  530 0

- ID: `create-listeventoptionhistoryentry-b--530-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Dark>d  4

- ID: `megacrit-sts2-core-models-events-doorsoflightanddark--dark-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DoorsOfLightAndDark+<Dark>d__4`

### <Deal Reach Deeper Damage>d  15

- ID: `megacrit-sts2-core-models-events-colossalflower--dealreachdeeperdamage-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<DealReachDeeperDamage>d__15`

### <Decipher>d  10

- ID: `megacrit-sts2-core-models-events-tabletoftruth--decipher-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth+<Decipher>d__10`

### <Discovered Events>k  Backing Field

- ID: `discoveredevents-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Dive In>d  8

- ID: `megacrit-sts2-core-models-events-trashheap--divein-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TrashHeap+<DiveIn>d__8`

### <Dive Into Water>d  8

- ID: `megacrit-sts2-core-models-events-sunkenstatue--diveintowater-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenStatue+<DiveIntoWater>d__8`

### <Dont Need Help>d  10

- ID: `megacrit-sts2-core-models-events-junglemazeadventure--dontneedhelp-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.JungleMazeAdventure+<DontNeedHelp>d__10`

### <Drink>d  6

- ID: `megacrit-sts2-core-models-events-spiralingwhirlpool--drink-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiralingWhirlpool+<Drink>d__6`

### <Eat>d  4

- ID: `megacrit-sts2-core-models-events-sapphireseed--eat-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SapphireSeed+<Eat>d__4`

### <Eat>d  5

- ID: `megacrit-sts2-core-models-events-byrdonisnest--eat-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ByrdonisNest+<Eat>d__5`

### <Ember Tea>d  9

- ID: `megacrit-sts2-core-models-events-teamaster--embertea-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster+<EmberTea>d__9`

### <Emotional Awareness>d  8

- ID: `megacrit-sts2-core-models-events-zenweaver--emotionalawareness-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ZenWeaver+<EmotionalAwareness>d__8`

### <Enjoy Tea>d  3

- ID: `megacrit-sts2-core-models-events-roundteaparty--enjoytea-d--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoundTeaParty+<EnjoyTea>d__3`

### <Enter Your Hole>d  5

- ID: `megacrit-sts2-core-models-events-fieldofmansizedholes--enteryourhole-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FieldOfManSizedHoles+<EnterYourHole>d__5`

### <Enter>d  18

- ID: `megacrit-sts2-core-rooms-eventroom--enter-d--18`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Rooms.EventRoom+<Enter>d__18`

### <Event Button Color>k  Backing Field

- ID: `eventbuttoncolor-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Choice Metric Prop Init>b  56 0

- ID: `eventchoicemetricpropinit-b--56-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Choice Metric Prop Init>b  56 1

- ID: `eventchoicemetricpropinit-b--56-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Choice Metric Prop Init>b  56 2

- ID: `eventchoicemetricpropinit-b--56-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Choice Metric Prop Init>b  56 3

- ID: `eventchoicemetricpropinit-b--56-3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Choices>k  Backing Field

- ID: `eventchoices-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Id>k  Backing Field

- ID: `eventid-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Ids>k  Backing Field

- ID: `eventids-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 0

- ID: `eventinfopropinit-b--50-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 1

- ID: `eventinfopropinit-b--50-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 10

- ID: `eventinfopropinit-b--50-10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 11

- ID: `eventinfopropinit-b--50-11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 12

- ID: `eventinfopropinit-b--50-12`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 13

- ID: `eventinfopropinit-b--50-13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 14

- ID: `eventinfopropinit-b--50-14`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 15

- ID: `eventinfopropinit-b--50-15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 16

- ID: `eventinfopropinit-b--50-16`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 17

- ID: `eventinfopropinit-b--50-17`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 2

- ID: `eventinfopropinit-b--50-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 3

- ID: `eventinfopropinit-b--50-3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 4

- ID: `eventinfopropinit-b--50-4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 5

- ID: `eventinfopropinit-b--50-5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 6

- ID: `eventinfopropinit-b--50-6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 7

- ID: `eventinfopropinit-b--50-7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 8

- ID: `eventinfopropinit-b--50-8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Info Prop Init>b  50 9

- ID: `eventinfopropinit-b--50-9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 0

- ID: `eventoptionhistoryentrypropinit-b--154-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 1

- ID: `eventoptionhistoryentrypropinit-b--154-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 2

- ID: `eventoptionhistoryentrypropinit-b--154-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 3

- ID: `eventoptionhistoryentrypropinit-b--154-3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 4

- ID: `eventoptionhistoryentrypropinit-b--154-4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Option History Entry Prop Init>b  154 5

- ID: `eventoptionhistoryentrypropinit-b--154-5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event Synchronizer>k  Backing Field

- ID: `eventsynchronizer-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Event>k  Backing Field

- ID: `event-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Events Seen>k  Backing Field

- ID: `eventsseen-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Events Visited>k  Backing Field

- ID: `eventsvisited-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Examine>d  15

- ID: `megacrit-sts2-core-models-events-dollroom--examine-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+<Examine>d__15`

### <Expire Death Prevention Vfx>d  51

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton--expiredeathpreventionvfx-d--51`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+<ExpireDeathPreventionVfx>d__51`

### <Extermination>d  3

- ID: `megacrit-sts2-core-models-events-bugslayer--extermination-d--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Bugslayer+<Extermination>d__3`

### <Extract Current Prize>d  12

- ID: `megacrit-sts2-core-models-events-colossalflower--extractcurrentprize-d--12`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<ExtractCurrentPrize>d__12`

### <Extract Instead>d  13

- ID: `megacrit-sts2-core-models-events-colossalflower--extractinstead-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<ExtractInstead>d__13`

### <First Chest>d  6

- ID: `megacrit-sts2-core-models-events-sunkentreasury--firstchest-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenTreasury+<FirstChest>d__6`

### <Flash Confirmation>d  55

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton--flashconfirmation-d--55`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+<FlashConfirmation>d__55`

### <Fragrant Mushroom>d  2

- ID: `megacrit-sts2-core-models-events-hungryformushrooms--fragrantmushroom-d--2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.HungryForMushrooms+<FragrantMushroom>d__2`

### <Fried Eel>d  27

- ID: `megacrit-sts2-core-models-events-endlessconveyor--friedeel-d--27`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<FriedEel>d__27`

### <get All Events>b  52 0

- ID: `get-allevents-b--52-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <get Event Odds>b  19 0

- ID: `get-eventodds-b--19-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Give Gold>d  20

- ID: `megacrit-sts2-core-models-events-ranwidtheelder--givegold-d--20`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder+<GiveGold>d__20`

### <Gold>d  4

- ID: `megacrit-sts2-core-models-events-whisperinghollow--gold-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WhisperingHollow+<Gold>d__4`

### <Golden Fysh>d  28

- ID: `megacrit-sts2-core-models-events-endlessconveyor--goldenfysh-d--28`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<GoldenFysh>d__28`

### <Gorge>d  4

- ID: `megacrit-sts2-core-models-events-roomfullofcheese--gorge-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese+<Gorge>d__4`

### <Grab Something Off The Belt>d  20

- ID: `megacrit-sts2-core-models-events-endlessconveyor--grabsomethingoffthebelt-d--20`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<GrabSomethingOffTheBelt>d__20`

### <Grab Sword>d  7

- ID: `megacrit-sts2-core-models-events-sunkenstatue--grabsword-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenStatue+<GrabSword>d__7`

### <Grab>d  9

- ID: `megacrit-sts2-core-models-events-trashheap--grab-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TrashHeap+<Grab>d__9`

### <Group>d  5

- ID: `megacrit-sts2-core-models-events-morphicgrove--group-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.MorphicGrove+<Group>d__5`

### <Handle Ancient Event Dialogue>b  0

- ID: `handleancienteventdialogue-b--0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Ancient Event Dialogue>b  11 1

- ID: `handleancienteventdialogue-b--11-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Ancient Event Dialogue>b  11 2

- ID: `handleancienteventdialogue-b--11-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Ancient Event Dialogue>d  11

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--handleancienteventdialogue-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<HandleAncientEventDialogue>d__11`

### <Handle Async>d  6

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--handleasync-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<HandleAsync>d__6`

### <Handle Event Combat>b  7 0

- ID: `handleeventcombat-b--7-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Event Combat>b  7 1

- ID: `handleeventcombat-b--7-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Event Combat>b  7 2

- ID: `handleeventcombat-b--7-2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Event Combat>b  7 3

- ID: `handleeventcombat-b--7-3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Event Combat>d  7

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--handleeventcombat-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<HandleEventCombat>d__7`

### <Handle Fake Merchant Event>b  0

- ID: `handlefakemerchantevent-b--0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Handle Fake Merchant Event>d  10

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--handlefakemerchantevent-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<HandleFakeMerchantEvent>d__10`

### <Has Event Pet>b  81 0

- ID: `haseventpet-b--81-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Has Event Pet>b  81 1

- ID: `haseventpet-b--81-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Hold On>d  21

- ID: `megacrit-sts2-core-models-events-slipperybridge--holdon-d--21`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SlipperyBridge+<HoldOn>d__21`

### <Hug>d  5

- ID: `megacrit-sts2-core-models-events-whisperinghollow--hug-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WhisperingHollow+<Hug>d__5`

### <Immerse>d  9

- ID: `megacrit-sts2-core-models-events-abyssalbaths--immerse-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths+<Immerse>d__9`

### <Jelly Liver>d  25

- ID: `megacrit-sts2-core-models-events-endlessconveyor--jellyliver-d--25`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<JellyLiver>d__25`

### <Kill With Fire>d  6

- ID: `megacrit-sts2-core-models-events-symbiote--killwithfire-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Symbiote+<KillWithFire>d__6`

### <Kill>d  7

- ID: `megacrit-sts2-core-models-events-unrestsite--kill-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.UnrestSite+<Kill>d__7`

### <Killed By Event>k  Backing Field

- ID: `killedbyevent-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Leave>d  22

- ID: `megacrit-sts2-core-models-events-welcometowongos--leave-d--22`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos+<Leave>d__22`

### <Let Go>d  3

- ID: `megacrit-sts2-core-models-events-aromaofchaos--letgo-d--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AromaOfChaos+<LetGo>d__3`

### <Lift>d  14

- ID: `megacrit-sts2-core-models-events-stoneofalltime--lift-d--14`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.StoneOfAllTime+<Lift>d__14`

### <Linger>d  11

- ID: `megacrit-sts2-core-models-events-abyssalbaths--linger-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths+<Linger>d__11`

### <Load Room Event Assets>d  13

- ID: `megacrit-sts2-core-assets-preloadmanager--loadroomeventassets-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Assets.PreloadManager+<LoadRoomEventAssets>d__13`

### <local Event>5  2

- ID: `localevent-5--2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Loner>d  4

- ID: `megacrit-sts2-core-models-events-morphicgrove--loner-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.MorphicGrove+<Loner>d__4`

### <Lose Max Hp And Upgrade>d  13

- ID: `megacrit-sts2-core-models-events-tabletoftruth--losemaxhpandupgrade-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth+<LoseMaxHpAndUpgrade>d__13`

### <Maintain Control>d  4

- ID: `megacrit-sts2-core-models-events-aromaofchaos--maintaincontrol-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AromaOfChaos+<MaintainControl>d__4`

### <Merchant Guilty>d  16

- ID: `megacrit-sts2-core-models-events-trial--merchantguilty-d--16`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<MerchantGuilty>d__16`

### <Merchant Innocent>d  17

- ID: `megacrit-sts2-core-models-events-trial--merchantinnocent-d--17`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<MerchantInnocent>d__17`

### <Nab The Map>d  4

- ID: `megacrit-sts2-core-models-events-thelegendsweretrue--nabthemap-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLegendsWereTrue+<NabTheMap>d__4`

### <Nab>d  15

- ID: `megacrit-sts2-core-models-events-punchoff--nab-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.PunchOff+<Nab>d__15`

### <Noble Guilty>d  18

- ID: `megacrit-sts2-core-models-events-trial--nobleguilty-d--18`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<NobleGuilty>d__18`

### <Noble Innocent>d  19

- ID: `megacrit-sts2-core-models-events-trial--nobleinnocent-d--19`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<NobleInnocent>d__19`

### <Nondescript Guilty>d  20

- ID: `megacrit-sts2-core-models-events-trial--nondescriptguilty-d--20`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<NondescriptGuilty>d__20`

### <Nondescript Innocent>d  21

- ID: `megacrit-sts2-core-models-events-trial--nondescriptinnocent-d--21`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial+<NondescriptInnocent>d__21`

### <Observe The Spiral>d  5

- ID: `megacrit-sts2-core-models-events-spiralingwhirlpool--observethespiral-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiralingWhirlpool+<ObserveTheSpiral>d__5`

### <Obtain Pollinous Core>d  14

- ID: `megacrit-sts2-core-models-events-colossalflower--obtainpollinouscore-d--14`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<ObtainPollinousCore>d__14`

### <Offer Rewards>d  5

- ID: `megacrit-sts2-core-models-events-colorfulphilosophers--offerrewards-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColorfulPhilosophers+<OfferRewards>d__5`

### <Offer Tribute>d  6

- ID: `megacrit-sts2-core-models-events-luminouschoir--offertribute-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LuminousChoir+<OfferTribute>d__6`

### <On Cell Clicked>d  24

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen--oncellclicked-d--24`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+<OnCellClicked>d__24`

### <On Immerse>d  14

- ID: `megacrit-sts2-core-models-events-abyssalbaths--onimmerse-d--14`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths+<OnImmerse>d__14`

### <On Modifier Option Selected>d  38

- ID: `megacrit-sts2-core-models-events-neow--onmodifieroptionselected-d--38`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Neow+<OnModifierOptionSelected>d__38`

### <Ornate>d  5

- ID: `megacrit-sts2-core-models-events-thisorthat--ornate-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ThisOrThat+<Ornate>d__5`

### <Overcome>d  20

- ID: `megacrit-sts2-core-models-events-slipperybridge--overcome-d--20`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SlipperyBridge+<Overcome>d__20`

### <Parent Event Id>k  Backing Field

- ID: `parenteventid-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Payment Plan>d  16

- ID: `megacrit-sts2-core-models-events-crystalsphere--paymentplan-d--16`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.CrystalSphere+<PaymentPlan>d__16`

### <Plain>d  4

- ID: `megacrit-sts2-core-models-events-thisorthat--plain-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ThisOrThat+<Plain>d__4`

### <Plant>d  5

- ID: `megacrit-sts2-core-models-events-sapphireseed--plant-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SapphireSeed+<Plant>d__5`

### <Play Current Line>d  51

- ID: `megacrit-sts2-core-models-events-thearchitect--playcurrentline-d--51`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<PlayCurrentLine>d__51`

### <Play Heal Vfx After Fade In>d  27

- ID: `megacrit-sts2-core-nodes-events-nancienteventlayout--playhealvfxafterfadein-d--27`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout+<PlayHealVfxAfterFadeIn>d__27`

### <Play Minigame>d  46

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame--playminigame-d--46`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+<PlayMinigame>d__46`

### <preventer>5  3

- ID: `preventer-5--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <preventer>5  4

- ID: `preventer-5--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Prickly Sponge>d  6

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium--pricklysponge-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium+<PricklySponge>d__6`

### <Process Mouse Drawing Event>b  104 0

- ID: `processmousedrawingevent-b--104-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Punch Each Other>d  12

- ID: `megacrit-sts2-core-models-events-punchoff--puncheachother-d--12`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.PunchOff+<PunchEachOther>d__12`

### <Push>d  15

- ID: `megacrit-sts2-core-models-events-stoneofalltime--push-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.StoneOfAllTime+<Push>d__15`

### <Reach Deeper>d  11

- ID: `megacrit-sts2-core-models-events-colossalflower--reachdeeper-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower+<ReachDeeper>d__11`

### <Reach Into The Flesh>d  5

- ID: `megacrit-sts2-core-models-events-luminouschoir--reachintotheflesh-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LuminousChoir+<ReachIntoTheFlesh>d__5`

### <Read Entire Book>d  11

- ID: `megacrit-sts2-core-models-events-selfhelpbook--readentirebook-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<ReadEntireBook>d__11`

### <Read Passage>d  10

- ID: `megacrit-sts2-core-models-events-selfhelpbook--readpassage-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<ReadPassage>d__10`

### <Read The Back>d  9

- ID: `megacrit-sts2-core-models-events-selfhelpbook--readtheback-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<ReadTheBack>d__9`

### <Remove Lantern Key>d  6

- ID: `megacrit-sts2-core-models-events-warhistorianrepy--removelanternkey-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy+<RemoveLanternKey>d__6`

### <replay Event>5  4

- ID: `replayevent-5--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Resist>d  4

- ID: `megacrit-sts2-core-models-events-fieldofmansizedholes--resist-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FieldOfManSizedHoles+<Resist>d__4`

### <Rest>d  6

- ID: `megacrit-sts2-core-models-events-unrestsite--rest-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.UnrestSite+<Rest>d__6`

### <Rest>d  7

- ID: `megacrit-sts2-core-models-events-densevegetation--rest-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DenseVegetation+<Rest>d__7`

### <Resume>d  11

- ID: `megacrit-sts2-core-models-events-battleworndummy--resume-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BattlewornDummy+<Resume>d__11`

### <Return The Key>d  9

- ID: `megacrit-sts2-core-models-events-thelanternkey--returnthekey-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLanternKey+<ReturnTheKey>d__9`

### <Reveal Item>d  13

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspheregold--revealitem-d--13`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems.CrystalSphereGold+<RevealItem>d__13`

### <Reveal Item>d  4

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspherecurse--revealitem-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItems.CrystalSphereCurse+<RevealItem>d__4`

### <Rider Chosen>d  15

- ID: `megacrit-sts2-core-models-events-tinkertime--riderchosen-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TinkerTime+<RiderChosen>d__15`

### <Rip>d  8

- ID: `megacrit-sts2-core-models-events-brainleech--rip-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BrainLeech+<Rip>d__8`

### <Rumble Death Vfx>d  50

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton--rumbledeathvfx-d--50`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+<RumbleDeathVfx>d__50`

### <Safety In Numbers>d  11

- ID: `megacrit-sts2-core-models-events-junglemazeadventure--safetyinnumbers-d--11`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.JungleMazeAdventure+<SafetyInNumbers>d__11`

### <Seapunk Salad>d  26

- ID: `megacrit-sts2-core-models-events-endlessconveyor--seapunksalad-d--26`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<SeapunkSalad>d__26`

### <Search>d  5

- ID: `megacrit-sts2-core-models-events-roomfullofcheese--search-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese+<Search>d__5`

### <Search>d  9

- ID: `megacrit-sts2-core-models-events-lostwisp--search-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LostWisp+<Search>d__9`

### <Second Chest>d  7

- ID: `megacrit-sts2-core-models-events-sunkentreasury--secondchest-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenTreasury+<SecondChest>d__7`

### <Select And Enchant>d  13`1

- ID: `megacrit-sts2-core-models-events-selfhelpbook--selectandenchant-d--13-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook+<SelectAndEnchant>d__13`1`

### <Setup Layout>d  23

- ID: `megacrit-sts2-core-nodes-rooms-neventroom--setuplayout-d--23`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+<SetupLayout>d__23`

### <Share Knowledge>d  9

- ID: `megacrit-sts2-core-models-events-brainleech--shareknowledge-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BrainLeech+<ShareKnowledge>d__9`

### <Shatter>d  3

- ID: `megacrit-sts2-core-models-events-reflections--shatter-d--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Reflections+<Shatter>d__3`

### <Should Resume Parent Event After Combat>k  Backing Field

- ID: `shouldresumeparenteventaftercombat-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Should Resume Parent Event>k  Backing Field

- ID: `shouldresumeparentevent-k--backingfield`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Show Welcome Dialogue>d  21

- ID: `megacrit-sts2-core-nodes-events-custom-nfakemerchant--showwelcomedialogue-d--21`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.NFakeMerchant+<ShowWelcomeDialogue>d__21`

### <Slowly Find An Exit>d  5

- ID: `megacrit-sts2-core-models-events-thelegendsweretrue--slowlyfindanexit-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLegendsWereTrue+<SlowlyFindAnExit>d__5`

### <Smash>d  9

- ID: `megacrit-sts2-core-models-events-tabletoftruth--smash-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth+<Smash>d__9`

### <Snake>d  8

- ID: `megacrit-sts2-core-models-events-woodcarvings--snake-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings+<Snake>d__8`

### <Squash>d  4

- ID: `megacrit-sts2-core-models-events-bugslayer--squash-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Bugslayer+<Squash>d__4`

### <Step Inside>d  5

- ID: `megacrit-sts2-core-models-events-spiritgrafter--stepinside-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiritGrafter+<StepInside>d__5`

### <Stick Arm In>d  6

- ID: `megacrit-sts2-core-models-events-spiritgrafter--stickarmin-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiritGrafter+<StickArmIn>d__6`

### <Study>d  3

- ID: `megacrit-sts2-core-models-events-infestedautomaton--study-d--3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.InfestedAutomaton+<Study>d__3`

### <Suspicious Condiment>d  24

- ID: `megacrit-sts2-core-models-events-endlessconveyor--suspiciouscondiment-d--24`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+<SuspiciousCondiment>d__24`

### <Take Some Time>d  14

- ID: `megacrit-sts2-core-models-events-dollroom--takesometime-d--14`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+<TakeSomeTime>d__14`

### <Take>d  6

- ID: `megacrit-sts2-core-models-events-byrdonisnest--take-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ByrdonisNest+<Take>d__6`

### <Tea Of Discourtesy>d  10

- ID: `megacrit-sts2-core-models-events-teamaster--teaofdiscourtesy-d--10`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster+<TeaOfDiscourtesy>d__10`

### <Tentacle Quill>d  7

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium--tentaclequill-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium+<TentacleQuill>d__7`

### <Torus>d  9

- ID: `megacrit-sts2-core-models-events-woodcarvings--torus-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings+<Torus>d__9`

### <Touch AMirror>d  2

- ID: `megacrit-sts2-core-models-events-reflections--touchamirror-d--2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Reflections+<TouchAMirror>d__2`

### <Touch Core>d  4

- ID: `megacrit-sts2-core-models-events-infestedautomaton--touchcore-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.InfestedAutomaton+<TouchCore>d__4`

### <Trudge On>d  6

- ID: `megacrit-sts2-core-models-events-densevegetation--trudgeon-d--6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DenseVegetation+<TrudgeOn>d__6`

### <Try Play>d  7

- ID: `megacrit-sts2-core-nodes-events-eventsplitvoteanimation--tryplay-d--7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.EventSplitVoteAnimation+<TryPlay>d__7`

### <Uncover Future>d  15

- ID: `megacrit-sts2-core-models-events-crystalsphere--uncoverfuture-d--15`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.CrystalSphere+<UncoverFuture>d__15`

### <Unlock Cage>d  4

- ID: `megacrit-sts2-core-models-events-warhistorianrepy--unlockcage-d--4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy+<UnlockCage>d__4`

### <Unlock Chest>d  5

- ID: `megacrit-sts2-core-models-events-warhistorianrepy--unlockchest-d--5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy+<UnlockChest>d__5`

### <Unlock Events>b  17 0

- ID: `unlockevents-b--17-0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Unlock Events>b  17 1

- ID: `unlockevents-b--17-1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Wait For Event Options>b  0

- ID: `waitforeventoptions-b--0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### <Wait For Event Options>d  9

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--waitforeventoptions-d--9`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<WaitForEventOptions>d__9`

### <Wait For Event Room>d  8

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler--waitforeventroom-d--8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler+<WaitForEventRoom>d__8`

### <Win Run>d  47

- ID: `megacrit-sts2-core-models-events-thearchitect--winrun-d--47`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect+<WinRun>d__47`

### abyssal baths

- ID: `res---images-events-abyssal-baths-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/abyssal_baths.png`

### abyssal baths vfx

- ID: `res---scenes-vfx-events-abyssal-baths-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/abyssal_baths_vfx.tscn`

### abyssal baths vfx

- ID: `scenes-vfx-events-abyssal-baths-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/abyssal_baths_vfx.tscn`

### abyssal baths.png

- ID: `images-events-abyssal-baths-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/abyssal_baths.png.import`

### AbyssalBaths

- ID: `res---src-core-models-events-abyssalbaths-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/AbyssalBaths.cs`

### Add Visited Event

- ID: `addvisitedevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### After Add To Deck Prevented

- ID: `afteraddtodeckprevented`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### After Event Started

- ID: `aftereventstarted`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### After Preventing Block Clear

- ID: `afterpreventingblockclear`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### After Preventing Death

- ID: `afterpreventingdeath`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### After Preventing Draw

- ID: `afterpreventingdraw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### All Events

- ID: `allevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### all Shared Events

- ID: `allsharedevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### amalgamator

- ID: `res---images-events-amalgamator-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/amalgamator.png`

### Amalgamator

- ID: `res---src-core-models-events-amalgamator-csh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Amalgamator.csh`

### amalgamator.png

- ID: `images-events-amalgamator-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/amalgamator.png.import`

### ancient dialogue line

- ID: `res---scenes-events-ancient-dialogue-line-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/ancient_dialogue_line.tscn`

### ancient dialogue line

- ID: `scenes-events-ancient-dialogue-line-tscnp4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/ancient_dialogue_line.tscnp4`

### ancient Event

- ID: `ancientevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### ancient event layout

- ID: `res---scenes-events-ancient-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/ancient_event_layout.tscn`

### ancient event layout

- ID: `scenes-events-ancient-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/ancient_event_layout.tscn`

### Ancient Event Model

- ID: `megacrit-sts2-core-models-ancienteventmodel`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.AncientEventModel`

### ancient event option button

- ID: `ext-resource-type--texture2d--uid--uid---bi648q3cavtx7--path--res---images-packed-common-ui-ancient-event-option-button-png--id--5-pnf1f`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bi648q3cavtx7" path="res://images/packed/common_ui/ancient_event_option_button.png" id="5_pnf1f"]`

### ancient event option button

- ID: `res---images-packed-common-ui-ancient-event-option-button-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/common_ui/ancient_event_option_button.png`

### ancient event option button

- ID: `res---scenes-events-ancient-event-option-button-tscnn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/ancient_event_option_button.tscnn`

### ancient event option button

- ID: `scenes-events-ancient-event-option-button-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/ancient_event_option_button.tscn`

### ancient event option button outline

- ID: `ext-resource-type--texture2d--uid--uid---ckdr7u7ds42xr--path--res---images-packed-common-ui-ancient-event-option-button-outline-png--id--3-pnf1f`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ckdr7u7ds42xr" path="res://images/packed/common_ui/ancient_event_option_button_outline.png" id="3_pnf1f"]`

### ancient event option button outline

- ID: `res---images-packed-common-ui-ancient-event-option-button-outline-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/common_ui/ancient_event_option_button_outline.png`

### ancient event option button outline.png

- ID: `images-packed-common-ui-ancient-event-option-button-outline-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/common_ui/ancient_event_option_button_outline.png.import`

### ancient event option button outline.png 2d4a5fa59cd124b3bb51d611c7f65f83

- ID: `godot-imported-ancient-event-option-button-outline-png-2d4a5fa59cd124b3bb51d611c7f65f83-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/ancient_event_option_button_outline.png-2d4a5fa59cd124b3bb51d611c7f65f83.ctex`

### ancient event option button outline.png 2d4a5fa59cd124b3bb51d611c7f65f83

- ID: `path--res----godot-imported-ancient-event-option-button-outline-png-2d4a5fa59cd124b3bb51d611c7f65f83-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/ancient_event_option_button_outline.png-2d4a5fa59cd124b3bb51d611c7f65f83.ctex"`

### ancient event option button.png

- ID: `images-packed-common-ui-ancient-event-option-button-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/common_ui/ancient_event_option_button.png.import`

### ancient event option button.png d78443e40cde521f5e0f59074b7c47c6

- ID: `godot-imported-ancient-event-option-button-png-d78443e40cde521f5e0f59074b7c47c6-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/ancient_event_option_button.png-d78443e40cde521f5e0f59074b7c47c6.ctex`

### ancient event option button.png d78443e40cde521f5e0f59074b7c47c6

- ID: `path--res----godot-imported-ancient-event-option-button-png-d78443e40cde521f5e0f59074b7c47c6-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/ancient_event_option_button.png-d78443e40cde521f5e0f59074b7c47c6.ctex"`

### Ancient Event Or Deprecated

- ID: `ancienteventordeprecated`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### AncientEventModel

- ID: `res---src-core-models-ancienteventmodel-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/AncientEventModel.cs`

### architect bg 00 a

- ID: `ext-resource-type--packedscene--uid--uid---boe5m0pjpg2eo--path--res---scenes-backgrounds-the-architect-event-encounter-layers-architect-bg-00-a-tscn--id--3-l4j40`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://boe5m0pjpg2eo" path="res://scenes/backgrounds/the_architect_event_encounter/layers/architect_bg_00_a.tscn" id="3_l4j40"]`

### architect bg 00 a

- ID: `res---scenes-backgrounds-the-architect-event-encounter-layers-architect-bg-00-a-tscn-w`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/layers/architect_bg_00_a.tscn(W`

### architect bg 00 a

- ID: `scenes-backgrounds-the-architect-event-encounter-layers-architect-bg-00-a-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/the_architect_event_encounter/layers/architect_bg_00_a.tscn`

### architect bg animate

- ID: `atlas-data---architect-bg-animate-png-nsize-5321-1522-nfilter-linear-linear-nscale-0-66666-nhue-saturation-luminosity-12-copy-nbounds-3632-297-442-425-nlayer-109-nbounds-4816-918-601-502-nrotate-90-nlayer-129-nbounds-1930-728-2880-791-nlayer-148-copy-nbounds-3-665-854-1921-nrotate-90-nlayer-154-nbounds-4816-16-379-896-ndesk-lamp-nbounds-5201-115-51-78-noffsets-2-2-55-80-nfloating-rock-nbounds-462-305-354-368-nrotate-90-nfloating-scroll-nbounds-3632-40-251-307-nrotate-90-nfloaty-1-nbounds-5201-31-30-47-noffsets-2-0-35-47-nrotate-90-nfloaty-2-nbounds-5258-147-43-46-noffsets-2-2-47-48-nfloaty-3-nbounds-5201-67-42-64-noffsets-2-0-45-66-nrotate-90-nheart-nbounds-4080-113-609-284-noffsets-0-3-609-287-nrotate-90-nheart-piece-1-nbounds-452-117-155-277-nrotate-90-nheart-piece-2-nbounds-4658-4-92-243-nmetal-tube-nbounds-4511-26-141-696-nrobot-1-nbounds-1317-88-149-264-noffsets-0-1-149-265-nrobot-2-nbounds-3-278-381-453-nrotate-90-nrobot-3-nbounds-1472-217-135-135-nspine-top-copy-nbounds-836-33-475-319-nspine-1-nbounds-2859-31-691-767-nrotate-90-nspine-bottom-nbounds-1662-116-543-243-nrotate-90-ntop-robots-nbounds-1930-3-923-719-ntube-1-nbounds-5201-199-115-713-ntube-2-nbounds-4370-13-135-709-ntube-3-nbounds-836-358-301-820-nrotate-90-ntube-4-nbounds-3-43-229-443-nrotate-90-ntube-5-nbounds-4658-253-142-469-n---normal-texture-prefix---n---source-path---res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-atlas---specular-texture-prefix---s`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `{"atlas_data":"architect_bg_animate.png/nsize:5321,1522/nfilter:Linear,Linear/nscale:0.66666/nHue/Saturation/Luminosity 12 Copy/nbounds:3632,297,442,425/nLayer 109/nbounds:4816,918,601,502/nrotate:90/nLayer 129/nbounds:1930,728,2880,791/nLayer 148 Copy/nbounds:3,665,854,1921/nrotate:90/nLayer 154/nbounds:4816,16,379,896/ndesk_lamp/nbounds:5201,115,51,78/noffsets:2,2,55,80/nfloating rock/nbounds:462,305,354,368/nrotate:90/nfloating scroll/nbounds:3632,40,251,307/nrotate:90/nfloaty_1/nbounds:5201,31,30,47/noffsets:2,0,35,47/nrotate:90/nfloaty_2/nbounds:5258,147,43,46/noffsets:2,2,47,48/nfloaty_3/nbounds:5201,67,42,64/noffsets:2,0,45,66/nrotate:90/nheart/nbounds:4080,113,609,284/noffsets:0,3,609,287/nrotate:90/nheart_piece_1/nbounds:452,117,155,277/nrotate:90/nheart_piece_2/nbounds:4658,4,92,243/nmetal_tube/nbounds:4511,26,141,696/nrobot_1/nbounds:1317,88,149,264/noffsets:0,1,149,265/nrobot_2/nbounds:3,278,381,453/nrotate:90/nrobot_3/nbounds:1472,217,135,135/nspine top Copy/nbounds:836,33,475,319/nspine_1/nbounds:2859,31,691,767/nrotate:90/nspine_bottom/nbounds:1662,116,543,243/nrotate:90/ntop_robots/nbounds:1930,3,923,719/ntube_1/nbounds:5201,199,115,713/ntube_2/nbounds:4370,13,135,709/ntube_3/nbounds:836,358,301,820/nrotate:90/ntube_4/nbounds:3,43,229,443/nrotate:90/ntube_5/nbounds:4658,253,142,469/n","normal_texture_prefix":"n","source_path":"res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.atlas","specular_texture_prefix":"s"}`

### architect bg animate

- ID: `ext-resource-type--spineatlasresource--uid--uid---c75y682nt0ji3--path--res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-atlas--id--1-w1a2j`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="SpineAtlasResource" uid="uid://c75y682nt0ji3" path="res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.atlas" id="1_w1a2j"]`

### architect bg animate

- ID: `ext-resource-type--spineskeletondataresource--uid--uid---2hrpjgw6lpfm--path--res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-tres--id--2-hn5xq`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="SpineSkeletonDataResource" uid="uid://2hrpjgw6lpfm" path="res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.tres" id="2_hn5xq"]`

### architect bg animate

- ID: `ext-resource-type--spineskeletonfileresource--uid--uid---cu62fn2eeqv4q--path--res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-skel--id--2-fkluo`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="SpineSkeletonFileResource" uid="uid://cu62fn2eeqv4q" path="res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.skel" id="2_fkluo"]`

### architect bg animate

- ID: `res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-atlas`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.atlas`

### architect bg animate

- ID: `res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.png>`

### architect bg animate

- ID: `res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-skel`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.skel`

### architect bg animate

- ID: `res---scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-tres`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.tres`

### architect bg animate

- ID: `scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-tres`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.tres`

### architect bg animate.png

- ID: `scenes-backgrounds-the-architect-event-encounter-architect-bg-animate-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/the_architect_event_encounter/architect_bg_animate.png.import``

### aroma of chaos

- ID: `res---images-events-aroma-of-chaos-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/aroma_of_chaos.png*`

### aroma of chaos.png

- ID: `images-events-aroma-of-chaos-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/aroma_of_chaos.png.import`

### AromaOfChaos

- ID: `res---src-core-models-events-aromaofchaos-cso`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/AromaOfChaos.csO`

### Ascension Level Changed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-characterselect-nascensionpanel-ascensionlevelchangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NAscensionPanel+AscensionLevelChangedEventHandler`

### battleworn dummy

- ID: `res---images-events-battleworn-dummy-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/battleworn_dummy.png`

### battleworn dummy.png

- ID: `images-events-battleworn-dummy-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/battleworn_dummy.png.import`

### BattlewornDummy

- ID: `res---src-core-models-events-battleworndummy-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/BattlewornDummy.cs"`

### BattlewornDummyEventEncounter

- ID: `res---src-core-models-encounters-battleworndummyeventencounter-css`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/BattlewornDummyEventEncounter.css`

### Beautiful Bracelet Event Option

- ID: `beautifulbraceleteventoption`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### bedlam beacon foreground

- ID: `ext-resource-type--texture2d--uid--uid---dd3dioqq2odlc--path--res---images-packed-vfx-event-bedlam-beacon-foreground-png--id--5-pdg26`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dd3dioqq2odlc" path="res://images/packed/vfx/event/bedlam_beacon_foreground.png" id="5_pdg26"]`

### bedlam beacon foreground

- ID: `res---images-packed-vfx-event-bedlam-beacon-foreground-png3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/bedlam_beacon_foreground.png3`

### bedlam beacon foreground.png

- ID: `images-packed-vfx-event-bedlam-beacon-foreground-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/bedlam_beacon_foreground.png.import`

### Before Event Started

- ID: `beforeeventstarted`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Begin Event

- ID: `beginevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### big divination icon

- ID: `ext-resource-type--texture2d--uid--uid---bnaqrmholpuvc--path--res---images-events-crystal-sphere-big-divination-icon-png--id--9-nj12v`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bnaqrmholpuvc" path="res://images/events/crystal_sphere/big_divination_icon.png" id="9_nj12v"]`

### big divination icon.png

- ID: `images-events-crystal-sphere-big-divination-icon-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/big_divination_icon.png.import`

### brain leech

- ID: `res---images-events-brain-leech-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/brain_leech.png`

### brain leech.png

- ID: `images-events-brain-leech-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/brain_leech.png.import`

### BrainLeech

- ID: `res---src-core-models-events-brainleech-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/BrainLeech.cs`

### brightest flame

- ID: `filename----event-brightest-flame-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/brightest_flame.png",`

### bugslayer

- ID: `res---images-events-bugslayer-png-db`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/bugslayer.png%db`

### Bugslayer

- ID: `res---src-core-models-events-bugslayer-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Bugslayer.cs`

### bugslayer vfx

- ID: `res---scenes-vfx-events-bugslayer-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/bugslayer_vfx.tscn`

### bugslayer vfx

- ID: `scenes-vfx-events-bugslayer-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/bugslayer_vfx.tscn`

### bugslayer.png

- ID: `images-events-bugslayer-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/bugslayer.png.import`

### byrdonis feathers

- ID: `ext-resource-type--texture2d--uid--uid---pv6ualtu8yg6--path--res---images-packed-vfx-event-byrdonis-feathers-png--id--1-1apxl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://pv6ualtu8yg6" path="res://images/packed/vfx/event/byrdonis_feathers.png" id="1_1apxl"]`

### byrdonis feathers

- ID: `ext-resource-type--texture2d--uid--uid---pv6ualtu8yg6--path--res---images-packed-vfx-event-byrdonis-feathers-png--id--4-k1pgm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://pv6ualtu8yg6" path="res://images/packed/vfx/event/byrdonis_feathers.png" id="4_k1pgm"]`

### byrdonis feathers

- ID: `res---images-packed-vfx-event-byrdonis-feathers-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/byrdonis_feathers.png`

### byrdonis feathers.png

- ID: `images-packed-vfx-event-byrdonis-feathers-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/byrdonis_feathers.png.import`

### byrdonis nest

- ID: `res---images-events-byrdonis-nest-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/byrdonis_nest.png`

### byrdonis nest shine

- ID: `ext-resource-type--texture2d--uid--uid---w2x7msrwjw21--path--res---images-packed-vfx-event-byrdonis-nest-shine-png--id--12-qu8ae`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://w2x7msrwjw21" path="res://images/packed/vfx/event/byrdonis_nest_shine.png" id="12_qu8ae"]`

### byrdonis nest shine

- ID: `ext-resource-type--texture2d--uid--uid---w2x7msrwjw21--path--res---images-packed-vfx-event-byrdonis-nest-shine-png--id--2-bssu4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://w2x7msrwjw21" path="res://images/packed/vfx/event/byrdonis_nest_shine.png" id="2_bssu4"]`

### byrdonis nest shine

- ID: `ext-resource-type--texture2d--uid--uid---w2x7msrwjw21--path--res---images-packed-vfx-event-byrdonis-nest-shine-png--id--2-o4vk5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://w2x7msrwjw21" path="res://images/packed/vfx/event/byrdonis_nest_shine.png" id="2_o4vk5"]`

### byrdonis nest shine

- ID: `res---images-packed-vfx-event-byrdonis-nest-shine-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/byrdonis_nest_shine.png`

### byrdonis nest shine.png

- ID: `images-packed-vfx-event-byrdonis-nest-shine-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/byrdonis_nest_shine.png.import`

### byrdonis nest vfx

- ID: `res---scenes-vfx-events-byrdonis-nest-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/byrdonis_nest_vfx.tscn`

### byrdonis nest vfx

- ID: `scenes-vfx-events-byrdonis-nest-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/byrdonis_nest_vfx.tscn`

### byrdonis nest.png

- ID: `images-events-byrdonis-nest-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/byrdonis_nest.png.import`

### ByrdonisNest

- ID: `res---src-core-models-events-byrdonisnest-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/ByrdonisNest.cs`

### caltrops

- ID: `filename----event-caltrops-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/caltrops.png",`

### Canonical Event

- ID: `canonicalevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Capstone Closed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-capstones-ncapstonecontainer-capstoneclosedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Capstones.NCapstoneContainer+CapstoneClosedEventHandler`

### Changed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-capstones-ncapstonecontainer-changedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Capstones.NCapstoneContainer+ChangedEventHandler`

### Changed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-overlays-noverlaystack-changedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Overlays.NOverlayStack+ChangedEventHandler`

### Choose Option For Event

- ID: `chooseoptionforevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Choose Option For Shared Event

- ID: `chooseoptionforsharedevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Choose Shared Event Option

- ID: `choosesharedeventoption`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### CiTestEventListener

- ID: `res---ridertestrunner-citesteventlistener-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://RiderTestRunner/CiTestEventListener.cs`

### Click Event Proceed If Needed

- ID: `clickeventproceedifneeded`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Clicked Event Handler

- ID: `megacrit-sts2-core-nodes-screens-runhistoryscreen-ndeckhistoryentry-clickedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen.NDeckHistoryEntry+ClickedEventHandler`

### Closed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-map-nmapscreen-closedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen+ClosedEventHandler`

### colorful philosophers vfx

- ID: `res---scenes-vfx-events-colorful-philosophers-vfx-tscnx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/colorful_philosophers_vfx.tscnx`

### colorful philosophers vfx

- ID: `scenes-vfx-events-colorful-philosophers-vfx-tscn0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/colorful_philosophers_vfx.tscn0`

### colorful philosophers.png

- ID: `images-events-colorful-philosophers-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/colorful_philosophers.png.import`

### colorful philosophers.png8

- ID: `res---images-events-colorful-philosophers-png8-6uk`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/colorful_philosophers.png8.6UK`

### ColorfulPhilosophers

- ID: `res---src-core-models-events-colorfulphilosophers-css`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/ColorfulPhilosophers.css`

### colossal flower

- ID: `res---images-events-colossal-flower-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/colossal_flower.png`

### colossal flower vfx

- ID: `res---scenes-vfx-events-colossal-flower-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/colossal_flower_vfx.tscn`

### colossal flower vfx

- ID: `scenes-vfx-events-colossal-flower-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/colossal_flower_vfx.tscn`

### colossal flower.png

- ID: `images-events-colossal-flower-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/colossal_flower.png.import`

### ColossalFlower

- ID: `res---src-core-models-events-colossalflower-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/ColossalFlower.cs`

### combat event layout

- ID: `res---scenes-events-combat-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/combat_event_layout.tscn`

### combat event layout

- ID: `scenes-events-combat-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/combat_event_layout.tscn`

### Combat Event Visuals

- ID: `megacrit-sts2-core-rooms-combateventvisuals`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Rooms.CombatEventVisuals`

### Combat Replay Event

- ID: `megacrit-sts2-core-multiplayer-replay-combatreplayevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Replay.CombatReplayEvent`

### Combat Replay Event Type

- ID: `megacrit-sts2-core-multiplayer-replay-combatreplayeventtype`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Replay.CombatReplayEventType`

### CombatEventVisuals

- ID: `res---src-core-rooms-combateventvisuals-cs2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Rooms/CombatEventVisuals.cs2`

### CombatReplayEvent

- ID: `res---src-core-multiplayer-replay-combatreplayevent-cso`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Replay/CombatReplayEvent.csO`

### CombatReplayEventType

- ID: `res---src-core-multiplayer-replay-combatreplayeventtype-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Replay/CombatReplayEventType.cs`

### Completed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-nrewardsscreen-completedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.NRewardsScreen+CompletedEventHandler`

### Connect Animation Event

- ID: `connectanimationevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Connect Player Events

- ID: `connectplayerevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Controller Detected Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ncontrollermanager-controllerdetectedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager+ControllerDetectedEventHandler`

### Controller Type Changed Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ncontrollermanager-controllertypechangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager+ControllerTypeChangedEventHandler`

### Create Event Choice Metric

- ID: `create-eventchoicemetric`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create Event Info

- ID: `create-eventinfo`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create Event Option History Entry

- ID: `create-eventoptionhistoryentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create List Event Choice Metric

- ID: `create-listeventchoicemetric`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Create List Event Option History Entry

- ID: `create-listeventoptionhistoryentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Creature Hovered Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-creaturehoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+CreatureHoveredEventHandler`

### Creature Unhovered Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-creatureunhoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+CreatureUnhoveredEventHandler`

### crystal ball five square ui

- ID: `res---images-events-crystal-sphere-crystal-ball-five-square-ui-png-w`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_ball_five_square_ui.png,W`

### crystal ball five square ui.png

- ID: `images-events-crystal-sphere-crystal-ball-five-square-ui-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_ball_five_square_ui.png.import`

### crystal ball single square ui

- ID: `ext-resource-type--texture2d--uid--uid---g0txvye4uolo--path--res---images-events-crystal-sphere-crystal-ball-single-square-ui-png--id--2-i3xup`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://g0txvye4uolo" path="res://images/events/crystal_sphere/crystal_ball_single_square_ui.png" id="2_i3xup"]`

### crystal ball single square ui

- ID: `res---images-events-crystal-sphere-crystal-ball-single-square-ui-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_ball_single_square_ui.png`

### crystal ball single square ui.png

- ID: `images-events-crystal-sphere-crystal-ball-single-square-ui-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_ball_single_square_ui.png.import`

### crystal sphere

- ID: `res---images-events-crystal-sphere-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere.png`

### crystal sphere big gold

- ID: `res---images-events-crystal-sphere-crystal-sphere-big-gold-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_sphere_big_gold.png`

### crystal sphere big gold.png

- ID: `images-events-crystal-sphere-crystal-sphere-big-gold-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_sphere_big_gold.png.import`

### crystal sphere cell

- ID: `res---scenes-events-custom-crystal-sphere-crystal-sphere-cell-tscnq`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/crystal_sphere/crystal_sphere_cell.tscnQ`

### crystal sphere cell

- ID: `scenes-events-custom-crystal-sphere-crystal-sphere-cell-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/crystal_sphere/crystal_sphere_cell.tscn@|`

### crystal sphere curse

- ID: `res---images-events-crystal-sphere-crystal-sphere-curse-png-x`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_sphere_curse.png:x#`

### crystal sphere curse.png

- ID: `images-events-crystal-sphere-crystal-sphere-curse-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_sphere_curse.png.import`

### crystal sphere dialogue

- ID: `ext-resource-type--packedscene--uid--uid---2xn7t08h01j6--path--res---scenes-events-custom-crystal-sphere-crystal-sphere-dialogue-tscn--id--8-2svsg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://2xn7t08h01j6" path="res://scenes/events/custom/crystal_sphere/crystal_sphere_dialogue.tscn" id="8_2svsg"]`

### crystal sphere dialogue

- ID: `res---scenes-events-custom-crystal-sphere-crystal-sphere-dialogue-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/crystal_sphere/crystal_sphere_dialogue.tscn`

### crystal sphere dialogue

- ID: `scenes-events-custom-crystal-sphere-crystal-sphere-dialogue-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/crystal_sphere/crystal_sphere_dialogue.tscn`

### crystal sphere gold

- ID: `res---images-events-crystal-sphere-crystal-sphere-gold-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_sphere_gold.png`

### crystal sphere gold.png

- ID: `images-events-crystal-sphere-crystal-sphere-gold-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_sphere_gold.png.import`

### Crystal Sphere Item

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereitem`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereItem`

### crystal sphere item

- ID: `res---scenes-events-custom-crystal-sphere-crystal-sphere-item-tscnu`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/crystal_sphere/crystal_sphere_item.tscnu`

### crystal sphere item

- ID: `scenes-events-custom-crystal-sphere-crystal-sphere-item-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/crystal_sphere/crystal_sphere_item.tscn`

### Crystal Sphere Minigame

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame`

### crystal sphere minigame bg

- ID: `ext-resource-type--texture2d--uid--uid---deroqsearxm2d--path--res---images-events-crystal-sphere-crystal-sphere-minigame-bg-png--id--2-eb8pr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://deroqsearxm2d" path="res://images/events/crystal_sphere/crystal_sphere_minigame_bg.png" id="2_eb8pr"]`

### crystal sphere minigame bg

- ID: `res---images-events-crystal-sphere-crystal-sphere-minigame-bg-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/crystal_sphere_minigame_bg.png`

### crystal sphere minigame bg.png

- ID: `images-events-crystal-sphere-crystal-sphere-minigame-bg-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/crystal_sphere_minigame_bg.png.import`

### crystal sphere screen

- ID: `res---scenes-events-custom-crystal-sphere-crystal-sphere-screen-tscn-ck`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/crystal_sphere/crystal_sphere_screen.tscn$ck`

### crystal sphere screen

- ID: `scenes-events-custom-crystal-sphere-crystal-sphere-screen-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/crystal_sphere/crystal_sphere_screen.tscn`

### Crystal Sphere Tool Type

- ID: `megacrit-sts2-core-events-custom-crystalsphereevent-crystalsphereminigame-crystalspheretooltype`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent.CrystalSphereMinigame+CrystalSphereToolType`

### crystal sphere vfx

- ID: `res---scenes-vfx-events-crystal-sphere-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/crystal_sphere_vfx.tscn`

### crystal sphere vfx

- ID: `scenes-vfx-events-crystal-sphere-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/crystal_sphere_vfx.tscn`

### crystal sphere.png

- ID: `images-events-crystal-sphere-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere.png.import`

### CrystalSphere

- ID: `res---src-core-models-events-crystalsphere-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/CrystalSphere.cs`

### CrystalSphereCell

- ID: `res---src-core-events-custom-crystalsphereevent-crystalspherecell-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/Custom/CrystalSphereEvent/CrystalSphereCell.cs`

### CrystalSphereCurse

- ID: `res---src-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspherecurse-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/Custom/CrystalSphereEvent/CrystalSphereItems/CrystalSphereCurse.cs`

### CrystalSphereGold

- ID: `res---src-core-events-custom-crystalsphereevent-crystalsphereitems-crystalspheregold-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/Custom/CrystalSphereEvent/CrystalSphereItems/CrystalSphereGold.cs&$`

### CrystalSphereItem

- ID: `res---src-core-events-custom-crystalsphereevent-crystalsphereitem-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/Custom/CrystalSphereEvent/CrystalSphereItem.cs`

### Custom Event Node

- ID: `customeventnode`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--10-7qoo0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="10_7qoo0"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--12-87cja`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="12_87cja"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--17-njsfc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="17_njsfc"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--18-tcns1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="18_tcns1"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--2-0mt28`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="2_0mt28"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--2-0rimb`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="2_0rimb"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--2-r4hcm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="2_r4hcm"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--2-siowr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="2_siowr"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--2-viiu4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="2_viiu4"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--3-26g5c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="3_26g5c"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--3-2dkxc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="3_2dkxc"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--3-guj2f`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="3_guj2f"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--3-j5c2l`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="3_j5c2l"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--4-0vj2m`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="4_0vj2m"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--4-b0iw7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="4_b0iw7"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--4-ldmxq`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="4_ldmxq"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--5-8sei4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="5_8sei4"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--5-g60ek`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="5_g60ek"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--5-gar08`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="5_gar08"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--5-r76b2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="5_r76b2"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--5-snohx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="5_snohx"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-kge8u`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_kge8u"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-nnngp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_nnngp"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-qu0nw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_qu0nw"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-t3nmd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_t3nmd"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-t5lao`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_t5lao"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--6-tuls4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="6_tuls4"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--7-d5mpy`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="7_d5mpy"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--7-j3v0v`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="7_j3v0v"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--7-lhshe`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="7_lhshe"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--7-xcojn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="7_xcojn"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--8-0vlxl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="8_0vlxl"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--8-omnku`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="8_omnku"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--8-qmc50`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="8_qmc50"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--8-yh7td`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="8_yh7td"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--9-134tf`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="9_134tf"]`

### dark overlay events

- ID: `ext-resource-type--texture2d--uid--uid---ksu7glbppu1c--path--res---images-packed-vfx-generic-dark-overlay-events-png--id--9-hv6wi`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ksu7glbppu1c" path="res://images/packed/vfx/generic/dark_overlay_events.png" id="9_hv6wi"]`

### dark overlay events

- ID: `res---images-packed-vfx-generic-dark-overlay-events-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/generic/dark_overlay_events.png`

### dark overlay events.png

- ID: `images-packed-vfx-generic-dark-overlay-events-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/generic/dark_overlay_events.png.import`

### dark overlay events.png 25f3e7f28c449be45dcfb7b6444574e9.s3tc

- ID: `godot-imported-dark-overlay-events-png-25f3e7f28c449be45dcfb7b6444574e9-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/dark_overlay_events.png-25f3e7f28c449be45dcfb7b6444574e9.s3tc.ctex`

### dark overlay events.png 25f3e7f28c449be45dcfb7b6444574e9.s3tc

- ID: `path-s3tc--res----godot-imported-dark-overlay-events-png-25f3e7f28c449be45dcfb7b6444574e9-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.s3tc="res://.godot/imported/dark_overlay_events.png-25f3e7f28c449be45dcfb7b6444574e9.s3tc.ctex"`

### darv

- ID: `res---scenes-events-background-scenes-darv-tscn-s`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/darv.tscn!S`

### Darv

- ID: `res---src-core-models-events-darv-csx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Darv.csX`

### darv

- ID: `scenes-events-background-scenes-darv-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/darv.tscn`

### deadly events

- ID: `res---images-packed-modifiers-deadly-events-pngp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/modifiers/deadly_events.pngp`

### deadly events.png

- ID: `images-packed-modifiers-deadly-events-png-importpw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/modifiers/deadly_events.png.importPW`

### deadly events.png 00ff01eb2aea2f48cae7c6bde99ad5a8.bptc

- ID: `godot-imported-deadly-events-png-00ff01eb2aea2f48cae7c6bde99ad5a8-bptc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/deadly_events.png-00ff01eb2aea2f48cae7c6bde99ad5a8.bptc.ctex`

### deadly events.png 00ff01eb2aea2f48cae7c6bde99ad5a8.bptc

- ID: `path-bptc--res----godot-imported-deadly-events-png-00ff01eb2aea2f48cae7c6bde99ad5a8-bptc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.bptc="res://.godot/imported/deadly_events.png-00ff01eb2aea2f48cae7c6bde99ad5a8.bptc.ctex"`

### DeadlyEvents

- ID: `res---src-core-models-modifiers-deadlyevents-cst`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Modifiers/DeadlyEvents.cst`

### death Prevention Cancellation

- ID: `deathpreventioncancellation`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### death Prevention Vfx

- ID: `deathpreventionvfx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### death Prevention Vfx Position

- ID: `deathpreventionvfxposition`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### default event layout

- ID: `res---scenes-events-default-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/default_event_layout.tscn`

### default event layout

- ID: `scenes-events-default-event-layout-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/default_event_layout.tscn`

### dense vegetation

- ID: `ext-resource-type--texture2d--uid--uid---ccxjea8y404qu--path--res---images-events-dense-vegetation-png--id--2-764r3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ccxjea8y404qu" path="res://images/events/dense_vegetation.png" id="2_764r3"]`

### dense vegetation

- ID: `res---images-events-dense-vegetation-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/dense_vegetation.png`

### dense vegetation event encounter

- ID: `res---scenes-encounters-dense-vegetation-event-encounter-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/encounters/dense_vegetation_event_encounter.tscn`

### dense vegetation event encounter

- ID: `scenes-encounters-dense-vegetation-event-encounter-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/encounters/dense_vegetation_event_encounter.tscn`

### dense vegetation foreground

- ID: `ext-resource-type--texture2d--uid--uid---dfirg1d7v58m6--path--res---images-events-dense-vegetation-foreground-png--id--3-l2tc0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dfirg1d7v58m6" path="res://images/events/dense_vegetation_foreground.png" id="3_l2tc0"]`

### dense vegetation foreground

- ID: `res---images-events-dense-vegetation-foreground-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/dense_vegetation_foreground.png`

### dense vegetation foreground.png

- ID: `images-events-dense-vegetation-foreground-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/dense_vegetation_foreground.png.import`

### dense vegetation slice vfx

- ID: `scenes-vfx-events-dense-vegetation-slice-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/dense_vegetation_slice_vfx.tscn`

### dense vegetation vfx

- ID: `res---scenes-vfx-events-dense-vegetation-vfx-tscnl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/dense_vegetation_vfx.tscnl`

### dense vegetation vfx

- ID: `scenes-vfx-events-dense-vegetation-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/dense_vegetation_vfx.tscn`

### dense vegetation.png

- ID: `images-events-dense-vegetation-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/dense_vegetation.png.import`

### DenseVegetation

- ID: `res---src-core-models-events-densevegetation-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DenseVegetation.cs`

### DenseVegetationEventEncounter

- ID: `res---src-core-models-encounters-densevegetationeventencounter-csf`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/DenseVegetationEventEncounter.csF`

### Deprecated Ancient Event

- ID: `megacrit-sts2-core-models-events-deprecatedancientevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DeprecatedAncientEvent`

### DeprecatedAncientEvent

- ID: `res---src-core-models-events-deprecatedancientevent-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DeprecatedAncientEvent.cs`

### DeprecatedEvent

- ID: `res---src-core-models-events-deprecatedevent-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DeprecatedEvent.cs`

### Disable Event Options

- ID: `disableeventoptions`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Disconnect Animation Event

- ID: `disconnectanimationevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Disconnect Player Events

- ID: `disconnectplayerevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### discovered Events

- ID: `discoveredevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Dish

- ID: `megacrit-sts2-core-models-events-endlessconveyor-dish`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor+Dish`

### divination button

- ID: `ext-resource-type--packedscene--uid--uid---canjgquyeoak7--path--res---scenes-events-custom-crystal-sphere-divination-button-tscn--id--10-2dh5v`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://canjgquyeoak7" path="res://scenes/events/custom/crystal_sphere/divination_button.tscn" id="10_2dh5v"]`

### divination button

- ID: `res---scenes-events-custom-crystal-sphere-divination-button-tscn-d`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/crystal_sphere/divination_button.tscn:d>`

### divination button

- ID: `scenes-events-custom-crystal-sphere-divination-button-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/crystal_sphere/divination_button.tscn`

### divine button

- ID: `ext-resource-type--texture2d--uid--uid---dal5psd35k7yq--path--res---images-events-crystal-sphere-divine-button-png--id--2-t7brm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dal5psd35k7yq" path="res://images/events/crystal_sphere/divine_button.png" id="2_t7brm"]`

### divine button

- ID: `res---images-events-crystal-sphere-divine-button-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/divine_button.png`

### divine button outline

- ID: `ext-resource-type--texture2d--uid--uid---cmcog1fhacbvv--path--res---images-events-crystal-sphere-divine-button-outline-png--id--3-2upy3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cmcog1fhacbvv" path="res://images/events/crystal_sphere/divine_button_outline.png" id="3_2upy3"]`

### divine button outline

- ID: `res---images-events-crystal-sphere-divine-button-outline-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/divine_button_outline.png`

### divine button outline.png

- ID: `images-events-crystal-sphere-divine-button-outline-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/divine_button_outline.png.import`

### divine button.png

- ID: `images-events-crystal-sphere-divine-button-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/divine_button.png.import`

### Doll Choice

- ID: `megacrit-sts2-core-models-events-dollroom-dollchoice`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom+DollChoice`

### doll room

- ID: `res---images-events-doll-room-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/doll_room.png`

### doll room foreground

- ID: `ext-resource-type--texture2d--uid--uid---clk87uphfehi2--path--res---images-packed-vfx-event-doll-room-foreground-png--id--9-8148x`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://clk87uphfehi2" path="res://images/packed/vfx/event/doll_room_foreground.png" id="9_8148x"]`

### doll room foreground

- ID: `res---images-packed-vfx-event-doll-room-foreground-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/doll_room_foreground.png`

### doll room foreground.png

- ID: `images-packed-vfx-event-doll-room-foreground-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/doll_room_foreground.png.import`

### doll room vfx

- ID: `res---scenes-vfx-events-doll-room-vfx-tscnv`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/doll_room_vfx.tscnV`

### doll room vfx

- ID: `scenes-vfx-events-doll-room-vfx-tscn0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/doll_room_vfx.tscn0`

### doll room whisper bubble l

- ID: `ext-resource-type--texture2d--uid--uid---jlx7qxdm2jd6--path--res---images-packed-vfx-event-doll-room-whisper-bubble-l-png--id--4-ipi7n`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://jlx7qxdm2jd6" path="res://images/packed/vfx/event/doll_room_whisper_bubble_l.png" id="4_ipi7n"]`

### doll room whisper bubble l

- ID: `res---images-packed-vfx-event-doll-room-whisper-bubble-l-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/doll_room_whisper_bubble_l.png`

### doll room whisper bubble l.png

- ID: `images-packed-vfx-event-doll-room-whisper-bubble-l-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/doll_room_whisper_bubble_l.png.import`

### doll room whisper bubble r

- ID: `ext-resource-type--texture2d--uid--uid---cam6ey8apiako--path--res---images-packed-vfx-event-doll-room-whisper-bubble-r-png--id--5-hd0pl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cam6ey8apiako" path="res://images/packed/vfx/event/doll_room_whisper_bubble_r.png" id="5_hd0pl"]`

### doll room whisper bubble r

- ID: `res---images-packed-vfx-event-doll-room-whisper-bubble-r-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/doll_room_whisper_bubble_r.png`

### doll room whisper bubble r.png

- ID: `images-packed-vfx-event-doll-room-whisper-bubble-r-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/doll_room_whisper_bubble_r.png.import`

### doll room whispers

- ID: `ext-resource-type--texture2d--uid--uid---cxx5t8i8ryckh--path--res---images-packed-vfx-event-doll-room-whispers-png--id--6-2q7w0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cxx5t8i8ryckh" path="res://images/packed/vfx/event/doll_room_whispers.png" id="6_2q7w0"]`

### doll room whispers

- ID: `res---images-packed-vfx-event-doll-room-whispers-pnga`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/doll_room_whispers.pnga`

### doll room whispers.png

- ID: `images-packed-vfx-event-doll-room-whispers-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/doll_room_whispers.png.import`

### doll room.png

- ID: `images-events-doll-room-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/doll_room.png.import`

### DollRoom

- ID: `res---src-core-models-events-dollroom-csz`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DollRoom.csZ`

### doors of light and dark

- ID: `res---images-events-doors-of-light-and-dark-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/doors_of_light_and_dark.png`

### doors of light and dark vfx

- ID: `res---scenes-vfx-events-doors-of-light-and-dark-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/doors_of_light_and_dark_vfx.tscn`

### doors of light and dark vfx

- ID: `scenes-vfx-events-doors-of-light-and-dark-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/doors_of_light_and_dark_vfx.tscn`

### doors of light and dark.png

- ID: `images-events-doors-of-light-and-dark-png-importp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/doors_of_light_and_dark.png.importp`

### DoorsOfLightAndDark

- ID: `res---src-core-models-events-doorsoflightanddark-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DoorsOfLightAndDark.cs_`

### drowning beacon

- ID: `res---images-events-drowning-beacon-png9n`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/drowning_beacon.png9N`

### drowning beacon vfx

- ID: `res---scenes-vfx-events-drowning-beacon-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/drowning_beacon_vfx.tscn:`

### drowning beacon vfx

- ID: `scenes-vfx-events-drowning-beacon-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/drowning_beacon_vfx.tscn`

### drowning beacon.png

- ID: `images-events-drowning-beacon-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/drowning_beacon.png.import`

### DrowningBeacon

- ID: `res---src-core-models-events-drowningbeacon-csog`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/DrowningBeacon.csOg`

### Dummy Setting

- ID: `megacrit-sts2-core-models-encounters-battleworndummyeventencounter-dummysetting`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BattlewornDummyEventEncounter+DummySetting`

### Dynamic Event Description

- ID: `dynamiceventdescription`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### e

- ID: `res---images-events-crystal-sphere-big-divination-icon-png-e`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/big_divination_icon.png/e`

### endless conveyor

- ID: `res---images-events-endless-conveyor-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/endless_conveyor.png`

### endless conveyor vfx

- ID: `res---scenes-vfx-events-endless-conveyor-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/endless_conveyor_vfx.tscn_`

### endless conveyor vfx

- ID: `scenes-vfx-events-endless-conveyor-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/endless_conveyor_vfx.tscn`

### endless conveyor.png

- ID: `images-events-endless-conveyor-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/endless_conveyor.png.import`

### EndlessConveyor

- ID: `res---src-core-models-events-endlessconveyor-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/EndlessConveyor.cs?`

### Ensure Next Event Is Valid

- ID: `ensurenexteventisvalid`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Enter Combat Without Exiting Event

- ID: `entercombatwithoutexitingevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Entering Event Combat

- ID: `enteringeventcombat`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event

- ID: `event`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event button

- ID: `ext-resource-type--texture2d--uid--uid---co4nflor6pb3b--path--res---images-packed-common-ui-event-button-png--id--2-7u5py`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://co4nflor6pb3b" path="res://images/packed/common_ui/event_button.png" id="2_7u5py"]`

### event button

- ID: `ext-resource-type--texture2d--uid--uid---co4nflor6pb3b--path--res---images-packed-common-ui-event-button-png--id--3-11fl7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://co4nflor6pb3b" path="res://images/packed/common_ui/event_button.png" id="3_11fl7"]`

### event button

- ID: `ext-resource-type--texture2d--uid--uid---co4nflor6pb3b--path--res---images-packed-common-ui-event-button-png--id--3-gxyrd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://co4nflor6pb3b" path="res://images/packed/common_ui/event_button.png" id="3_gxyrd"]`

### event button

- ID: `ext-resource-type--texture2d--uid--uid---co4nflor6pb3b--path--res---images-packed-common-ui-event-button-png--id--7-ojb1d`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://co4nflor6pb3b" path="res://images/packed/common_ui/event_button.png" id="7_ojb1d"]`

### event button

- ID: `res---images-packed-common-ui-event-button-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/common_ui/event_button.png`

### Event Button Color

- ID: `eventbuttoncolor`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event button outline

- ID: `ext-resource-type--texture2d--uid--uid---6687vb6wm670--path--res---images-packed-common-ui-event-button-outline-png--id--3-3nbsy`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://6687vb6wm670" path="res://images/packed/common_ui/event_button_outline.png" id="3_3nbsy"]`

### event button outline

- ID: `res---images-packed-common-ui-event-button-outline-png-c`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/common_ui/event_button_outline.png%c`

### event button outline.png

- ID: `images-packed-common-ui-event-button-outline-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/common_ui/event_button_outline.png.import`

### event button outline.png 1cc7063a6e5fa2a366cfd2973b77daa5.s3tc

- ID: `godot-imported-event-button-outline-png-1cc7063a6e5fa2a366cfd2973b77daa5-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event_button_outline.png-1cc7063a6e5fa2a366cfd2973b77daa5.s3tc.ctex`

### event button outline.png 1cc7063a6e5fa2a366cfd2973b77daa5.s3tc

- ID: `path-s3tc--res----godot-imported-event-button-outline-png-1cc7063a6e5fa2a366cfd2973b77daa5-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.s3tc="res://.godot/imported/event_button_outline.png-1cc7063a6e5fa2a366cfd2973b77daa5.s3tc.ctex"`

### event button sdf

- ID: `ext-resource-type--texture2d--uid--uid---dtb0v8fepvdyr--path--res---images-packed-common-ui-event-button-sdf-png--id--2-b1sfw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dtb0v8fepvdyr" path="res://images/packed/common_ui/event_button_sdf.png" id="2_b1sfw"]`

### event button sdf

- ID: `ext-resource-type--texture2d--uid--uid---dtb0v8fepvdyr--path--res---images-packed-common-ui-event-button-sdf-png--id--2-ijq5o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dtb0v8fepvdyr" path="res://images/packed/common_ui/event_button_sdf.png" id="2_ijq5o"]`

### event button sdf

- ID: `res---images-packed-common-ui-event-button-sdf-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/common_ui/event_button_sdf.png`

### event button sdf.png

- ID: `images-packed-common-ui-event-button-sdf-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/common_ui/event_button_sdf.png.import`

### event button sdf.png a7534bb77c12c9ee2c51c16a29069751

- ID: `godot-imported-event-button-sdf-png-a7534bb77c12c9ee2c51c16a29069751-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event_button_sdf.png-a7534bb77c12c9ee2c51c16a29069751.ctex`

### event button sdf.png a7534bb77c12c9ee2c51c16a29069751

- ID: `path--res----godot-imported-event-button-sdf-png-a7534bb77c12c9ee2c51c16a29069751-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event_button_sdf.png-a7534bb77c12c9ee2c51c16a29069751.ctex"`

### event button.png

- ID: `images-packed-common-ui-event-button-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/common_ui/event_button.png.import`

### event button.png bb9036a25b7da11be807935a4e32ff47

- ID: `godot-imported-event-button-png-bb9036a25b7da11be807935a4e32ff47-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event_button.png-bb9036a25b7da11be807935a4e32ff47.ctex`

### event button.png bb9036a25b7da11be807935a4e32ff47

- ID: `path--res----godot-imported-event-button-png-bb9036a25b7da11be807935a4e32ff47-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event_button.png-bb9036a25b7da11be807935a4e32ff47.ctex"`

### Event Choice Metric

- ID: `eventchoicemetric`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Choice Metric

- ID: `megacrit-sts2-core-runs-metrics-eventchoicemetric`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Runs.Metrics.EventChoiceMetric`

### Event Choice Metric Prop Init

- ID: `eventchoicemetricpropinit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Choice Metric Serialize Handler

- ID: `eventchoicemetricserializehandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Choices

- ID: `eventchoices`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Console Cmd

- ID: `megacrit-sts2-core-devconsole-consolecommands-eventconsolecmd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.EventConsoleCmd`

### event Container

- ID: `eventcontainer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Death

- ID: `eventdeath`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Death Prevention Line

- ID: `eventdeathpreventionline`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Description

- ID: `eventdescription`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event icon

- ID: `ext-resource-type--texture2d--uid--uid---cmpgmbn3y4svl--path--res---addons-fmod-icons-event-icon-svg--id--1-kuu6i`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cmpgmbn3y4svl" path="res://addons/fmod/icons/event_icon.svg" id="1_kuu6i"]`

### event icon

- ID: `res---addons-fmod-icons-event-icon-svg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/icons/event_icon.svg`

### event icon.svg 4e6e2103d076f95b7bef82f079e433e6

- ID: `path--res----godot-imported-event-icon-svg-4e6e2103d076f95b7bef82f079e433e6-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event_icon.svg-4e6e2103d076f95b7bef82f079e433e6.ctex"`

### Event Id

- ID: `eventid`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Ids

- ID: `eventids`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Info

- ID: `eventinfo`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Info

- ID: `megacrit-sts2-gameinfo-objects-eventinfo`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.GameInfo.Objects.EventInfo`

### Event Info Ctor Param Init

- ID: `eventinfoctorparaminit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Info Prop Init

- ID: `eventinfopropinit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Info Serialize Handler

- ID: `eventinfoserializehandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event Layout

- ID: `eventlayout`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Layout Type

- ID: `megacrit-sts2-core-events-eventlayouttype`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.EventLayoutType`

### event Model

- ID: `eventmodel`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Model

- ID: `megacrit-sts2-core-models-eventmodel`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.EventModel`

### event Name

- ID: `eventname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Odds

- ID: `eventodds`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Option

- ID: `megacrit-sts2-core-events-eventoption`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Events.EventOption`

### event option button

- ID: `res---scenes-events-event-option-button-tscn0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/event_option_button.tscn0|`

### event option button

- ID: `scenes-events-event-option-button-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/event_option_button.tscn`

### Event Option History Entry

- ID: `eventoptionhistoryentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Option History Entry

- ID: `megacrit-sts2-core-runs-history-eventoptionhistoryentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Runs.History.EventOptionHistoryEntry`

### Event Option History Entry Prop Init

- ID: `eventoptionhistoryentrypropinit`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Or Deprecated

- ID: `eventordeprecated`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event outline

- ID: `res---images-ui-run-history-event-outline-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/ui/run_history/event_outline.png`

### event outline.png

- ID: `images-ui-run-history-event-outline-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/ui/run_history/event_outline.png.import`

### event outline.png 9c645aca66b2b32a8398e537a54d13b5.s3tc

- ID: `godot-imported-event-outline-png-9c645aca66b2b32a8398e537a54d13b5-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event_outline.png-9c645aca66b2b32a8398e537a54d13b5.s3tc.ctex`

### event outline.png 9c645aca66b2b32a8398e537a54d13b5.s3tc

- ID: `path-s3tc--res----godot-imported-event-outline-png-9c645aca66b2b32a8398e537a54d13b5-s3tc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.s3tc="res://.godot/imported/event_outline.png-9c645aca66b2b32a8398e537a54d13b5.s3tc.ctex"`

### Event Room

- ID: `eventroom`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Room

- ID: `megacrit-sts2-core-rooms-eventroom`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Rooms.EventRoom`

### event room

- ID: `res---scenes-rooms-event-room-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/rooms/event_room.tscn`

### event room

- ID: `scenes-rooms-event-room-tscn-e`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/rooms/event_room.tscn E`

### Event Room Handler

- ID: `megacrit-sts2-core-autoslay-handlers-rooms-eventroomhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.AutoSlay.Handlers.Rooms.EventRoomHandler`

### Event Split Vote Animation

- ID: `megacrit-sts2-core-nodes-events-eventsplitvoteanimation`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.EventSplitVoteAnimation`

### Event Synchronizer

- ID: `eventsynchronizer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Event Synchronizer

- ID: `megacrit-sts2-core-multiplayer-game-eventsynchronizer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Game.EventSynchronizer`

### event Type

- ID: `eventtype`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### event.png

- ID: `images-ui-run-history-event-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/ui/run_history/event.png.import`

### event.png 35bc3b33f31ca669f5b01b2d1a895a56.bptc

- ID: `godot-imported-event-png-35bc3b33f31ca669f5b01b2d1a895a56-bptc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event.png-35bc3b33f31ca669f5b01b2d1a895a56.bptc.ctex`

### event.png 35bc3b33f31ca669f5b01b2d1a895a56.bptc

- ID: `path-bptc--res----godot-imported-event-png-35bc3b33f31ca669f5b01b2d1a895a56-bptc-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path.bptc="res://.godot/imported/event.png-35bc3b33f31ca669f5b01b2d1a895a56.bptc.ctex"`

### event1 epoch

- ID: `res---images-timeline-epoch-portraits-event1-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/timeline/epoch_portraits/event1_epoch.png`

### event1 epoch.png

- ID: `images-timeline-epoch-portraits-event1-epoch-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/timeline/epoch_portraits/event1_epoch.png.import`

### event1 epoch.png c6630ce765d13f478583657218921db2

- ID: `godot-imported-event1-epoch-png-c6630ce765d13f478583657218921db2-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event1_epoch.png-c6630ce765d13f478583657218921db2.ctex`

### event1 epoch.png c6630ce765d13f478583657218921db2

- ID: `path--res----godot-imported-event1-epoch-png-c6630ce765d13f478583657218921db2-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event1_epoch.png-c6630ce765d13f478583657218921db2.ctex"`

### Event1Epoch

- ID: `res---src-core-timeline-epochs-event1epoch-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Timeline/Epochs/Event1Epoch.cs`

### event2 epoch

- ID: `res---images-timeline-epoch-portraits-event2-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/timeline/epoch_portraits/event2_epoch.png`

### event2 epoch.png

- ID: `images-timeline-epoch-portraits-event2-epoch-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/timeline/epoch_portraits/event2_epoch.png.import`

### event2 epoch.png 4cfe6dc7ab73fb76146025a36d6168a7

- ID: `godot-imported-event2-epoch-png-4cfe6dc7ab73fb76146025a36d6168a7-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event2_epoch.png-4cfe6dc7ab73fb76146025a36d6168a7.ctex`

### event2 epoch.png 4cfe6dc7ab73fb76146025a36d6168a7

- ID: `path--res----godot-imported-event2-epoch-png-4cfe6dc7ab73fb76146025a36d6168a7-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event2_epoch.png-4cfe6dc7ab73fb76146025a36d6168a7.ctex"`

### Event2Epoch

- ID: `res---src-core-timeline-epochs-event2epoch-cs0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Timeline/Epochs/Event2Epoch.cs0`

### event3 epoch

- ID: `res---images-timeline-epoch-portraits-event3-epoch-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/timeline/epoch_portraits/event3_epoch.png`

### event3 epoch.png

- ID: `images-timeline-epoch-portraits-event3-epoch-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/timeline/epoch_portraits/event3_epoch.png.import`

### event3 epoch.png 3c5d2adc853e7648f3f4930ceb316f72

- ID: `godot-imported-event3-epoch-png-3c5d2adc853e7648f3f4930ceb316f72-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `.godot/imported/event3_epoch.png-3c5d2adc853e7648f3f4930ceb316f72.ctex`

### event3 epoch.png 3c5d2adc853e7648f3f4930ceb316f72

- ID: `path--res----godot-imported-event3-epoch-png-3c5d2adc853e7648f3f4930ceb316f72-ctex`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://.godot/imported/event3_epoch.png-3c5d2adc853e7648f3f4930ceb316f72.ctex"`

### Event3Epoch

- ID: `res---src-core-timeline-epochs-event3epoch-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Timeline/Epochs/Event3Epoch.cs`

### EventChoiceMetric

- ID: `res---src-core-runs-metrics-eventchoicemetric-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Runs/Metrics/EventChoiceMetric.cs`

### EventConsoleCmd

- ID: `res---src-core-devconsole-consolecommands-eventconsolecmd-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/DevConsole/ConsoleCommands/EventConsoleCmd.cs`

### EventInfo

- ID: `res---src-gameinfo-objects-eventinfo-csp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/GameInfo/Objects/EventInfo.csp`

### EventLayoutType

- ID: `res---src-core-events-eventlayouttype-csl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/EventLayoutType.csl`

### EventModel

- ID: `res---src-core-models-eventmodel-cs-jn-xh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/EventModel.cs^JN#XH`

### EventOption

- ID: `res---src-core-events-eventoption-csl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/EventOption.csL`

### EventOptionHistoryEntry

- ID: `res---src-core-runs-history-eventoptionhistoryentry-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Runs/History/EventOptionHistoryEntry.cs`

### EventParametersDisplay

- ID: `addons-fmod-tool-ui-eventparametersdisplay-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `addons/fmod/tool/ui/EventParametersDisplay.tscn`

### EventParametersDisplay

- ID: `ext-resource-type--packedscene--uid--uid---cppeyr1ke5wre--path--res---addons-fmod-tool-ui-eventparametersdisplay-tscn--id--1-clkxg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://cppeyr1ke5wre" path="res://addons/fmod/tool/ui/EventParametersDisplay.tscn" id="1_clkxg"]`

### EventParametersDisplay

- ID: `ext-resource-type--packedscene--uid--uid---cppeyr1ke5wre--path--res---addons-fmod-tool-ui-eventparametersdisplay-tscn--id--2-uoyg8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://cppeyr1ke5wre" path="res://addons/fmod/tool/ui/EventParametersDisplay.tscn" id="2_uoyg8"]`

### EventParametersDisplay

- ID: `ext-resource-type--script--uid--uid---7relkis52fsu--path--res---addons-fmod-tool-ui-eventparametersdisplay-gd--id--1-2l58q`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://7relkis52fsu" path="res://addons/fmod/tool/ui/EventParametersDisplay.gd" id="1_2l58q"]`

### EventParametersDisplay

- ID: `path----res---addons-fmod-tool-ui-eventparametersdisplay-gd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"path": "res://addons/fmod/tool/ui/EventParametersDisplay.gd"`

### EventParametersDisplay

- ID: `path--res---addons-fmod-tool-ui-eventparametersdisplay-gdc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://addons/fmod/tool/ui/EventParametersDisplay.gdc"`

### EventParametersDisplay

- ID: `res---addons-fmod-tool-ui-eventparametersdisplay-gd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/ui/EventParametersDisplay.gd`

### EventParametersDisplay

- ID: `res---addons-fmod-tool-ui-eventparametersdisplay-tscnli`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/ui/EventParametersDisplay.tscnLI`

### EventParametersWindow

- ID: `addons-fmod-tool-ui-eventparameterswindow-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `addons/fmod/tool/ui/EventParametersWindow.tscn`

### EventParametersWindow

- ID: `res---addons-fmod-tool-ui-eventparameterswindow-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/ui/EventParametersWindow.tscn`

### EventPlayControls

- ID: `ext-resource-type--script--uid--uid---vgmq7hfrbddw--path--res---addons-fmod-tool-ui-eventplaycontrols-gd--id--2-mleop`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://vgmq7hfrbddw" path="res://addons/fmod/tool/ui/EventPlayControls.gd" id="2_mleop"]`

### EventPlayControls

- ID: `path--res---addons-fmod-tool-ui-eventplaycontrols-gdc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://addons/fmod/tool/ui/EventPlayControls.gdc"`

### EventPlayControls

- ID: `res---addons-fmod-tool-ui-eventplaycontrols-gd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/ui/EventPlayControls.gd`

### EventRoom

- ID: `res---src-core-rooms-eventroom-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Rooms/EventRoom.cs`

### EventRoomHandler

- ID: `res---src-core-autoslay-handlers-rooms-eventroomhandler-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/AutoSlay/Handlers/Rooms/EventRoomHandler.cs`

### Events

- ID: `events`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### events

- ID: `localization-deu-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/deu/events.json`

### events

- ID: `localization-eng-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/eng/events.json`

### events

- ID: `localization-esp-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/esp/events.json``

### events

- ID: `localization-fra-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/fra/events.json`

### events

- ID: `localization-ita-events-json-f`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/ita/events.json f`

### events

- ID: `localization-jpn-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/jpn/events.json`

### events

- ID: `localization-kor-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/kor/events.json`

### events

- ID: `localization-pol-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/pol/events.json`

### events

- ID: `localization-ptb-events-json`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/ptb/events.json`

### events

- ID: `localization-rus-events-json-3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/rus/events.json 3`

### events

- ID: `localization-spa-events-json0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/spa/events.json0`

### events

- ID: `localization-tha-events-json0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/tha/events.json0`

### events

- ID: `localization-tur-events-json-g1a`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/tur/events.json G1a`

### events

- ID: `localization-zhs-events-json0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `localization/zhs/events.json0`

### events Entry

- ID: `eventsentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### events Icon Path

- ID: `eventsiconpath`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Events Seen

- ID: `eventsseen`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Events Visited

- ID: `eventsvisited`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### EventSplitVoteAnimation

- ID: `res---src-core-nodes-events-eventsplitvoteanimation-csh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/EventSplitVoteAnimation.csH`

### EventSynchronizer

- ID: `res---src-core-multiplayer-game-eventsynchronizer-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Game/EventSynchronizer.cs`

### Expire Death Prevention Vfx

- ID: `expiredeathpreventionvfx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### eye 01

- ID: `ext-resource-type--texture2d--uid--uid---d3wg5u1mbkhw8--path--res---images-packed-vfx-event-symbiote-eye-01-png--id--5-7ohl6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://d3wg5u1mbkhw8" path="res://images/packed/vfx/event/symbiote/eye_01.png" id="5_7ohl6"]`

### eye 01

- ID: `res---images-packed-vfx-event-symbiote-eye-01-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/eye_01.png`

### eye 01.png

- ID: `images-packed-vfx-event-symbiote-eye-01-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/eye_01.png.import`

### eye 02

- ID: `ext-resource-type--texture2d--uid--uid---lwjdi644kgyi--path--res---images-packed-vfx-event-symbiote-eye-02-png--id--9-l5ne0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://lwjdi644kgyi" path="res://images/packed/vfx/event/symbiote/eye_02.png" id="9_l5ne0"]`

### eye 02

- ID: `res---images-packed-vfx-event-symbiote-eye-02-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/eye_02.png`

### eye 02.png

- ID: `images-packed-vfx-event-symbiote-eye-02-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/eye_02.png.import`

### eye 03

- ID: `ext-resource-type--texture2d--uid--uid---bh6n6j831bd53--path--res---images-packed-vfx-event-symbiote-eye-03-png--id--7-7h546`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bh6n6j831bd53" path="res://images/packed/vfx/event/symbiote/eye_03.png" id="7_7h546"]`

### eye 03

- ID: `res---images-packed-vfx-event-symbiote-eye-03-pngrs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/eye_03.pngrS`

### eye 03.png

- ID: `images-packed-vfx-event-symbiote-eye-03-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/eye_03.png.import`

### fake merchant

- ID: `res---scenes-events-custom-fake-merchant-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/fake_merchant.tscn!`

### fake merchant

- ID: `scenes-events-custom-fake-merchant-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/fake_merchant.tscn`

### fake merchant bg 00 a

- ID: `ext-resource-type--packedscene--uid--uid---62s52whrkexj--path--res---scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-bg-00-a-tscn--id--3-3g08h`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://62s52whrkexj" path="res://scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_bg_00_a.tscn" id="3_3g08h"]`

### fake merchant bg 00 a

- ID: `res---scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-bg-00-a-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_bg_00_a.tscn`

### fake merchant bg 00 a

- ID: `scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-bg-00-a-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_bg_00_a.tscn`

### fake merchant event encounter

- ID: `res---scenes-encounters-fake-merchant-event-encounter-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/encounters/fake_merchant_event_encounter.tscn`

### fake merchant event encounter

- ID: `scenes-encounters-fake-merchant-event-encounter-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/encounters/fake_merchant_event_encounter.tscn`

### fake merchant event encounter background

- ID: `res---scenes-backgrounds-fake-merchant-event-encounter-fake-merchant-event-encounter-background-tscni`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscnI`

### fake merchant event encounter background

- ID: `scenes-backgrounds-fake-merchant-event-encounter-fake-merchant-event-encounter-background-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/fake_merchant_event_encounter/fake_merchant_event_encounter_background.tscn`

### fake merchant fg a

- ID: `ext-resource-type--packedscene--uid--uid---dcnjj3cew7ybt--path--res---scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-fg-a-tscn--id--4-si7lg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://dcnjj3cew7ybt" path="res://scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_fg_a.tscn" id="4_si7lg"]`

### fake merchant fg a

- ID: `res---scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-fg-a-tscneh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_fg_a.tscnEh`

### fake merchant fg a

- ID: `scenes-backgrounds-fake-merchant-event-encounter-layers-fake-merchant-fg-a-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_fg_a.tscn`

### fake merchant inventory

- ID: `ext-resource-type--packedscene--uid--uid---b1ay07letu06p--path--res---scenes-events-custom-fake-merchant-inventory-tscn--id--25-ag3xl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="PackedScene" uid="uid://b1ay07letu06p" path="res://scenes/events/custom/fake_merchant_inventory.tscn" id="25_ag3xl"]`

### fake merchant inventory

- ID: `res---scenes-events-custom-fake-merchant-inventory-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/custom/fake_merchant_inventory.tscn`

### fake merchant inventory

- ID: `scenes-events-custom-fake-merchant-inventory-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/custom/fake_merchant_inventory.tscn`

### fake merchant rug

- ID: `ext-resource-type--texture2d--uid--uid---yagkc6fnfvt6--path--res---images-events-custom-fake-merchant-rug-png--id--2-2cdx4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://yagkc6fnfvt6" path="res://images/events/custom/fake_merchant_rug.png" id="2_2cdx4"]`

### fake merchant rug

- ID: `res---images-events-custom-fake-merchant-rug-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/custom/fake_merchant_rug.png`

### fake merchant rug.png

- ID: `images-events-custom-fake-merchant-rug-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/custom/fake_merchant_rug.png.import`

### FakeMerchant

- ID: `res---src-core-models-events-fakemerchant-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/FakeMerchant.cs`

### FakeMerchantEventEncounter

- ID: `res---src-core-models-encounters-fakemerchanteventencounter-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/FakeMerchantEventEncounter.cs`

### field of man sized holes

- ID: `res---images-events-field-of-man-sized-holes-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/field_of_man_sized_holes.png`

### field of man sized holes.png

- ID: `images-events-field-of-man-sized-holes-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/field_of_man_sized_holes.png.import`

### FieldOfManSizedHoles

- ID: `res---src-core-models-events-fieldofmansizedholes-csl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/FieldOfManSizedHoles.csl`

### Finished Event Handler

- ID: `megacrit-sts2-core-nodes-screens-map-nmapdrawinginput-finishedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapDrawingInput+FinishedEventHandler`

### fmod icon

- ID: `fmodeventemitter2d----res---addons-fmod-icons-fmod-icon-svg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `FmodEventEmitter2D = "res://addons/fmod/icons/fmod_icon.svg"`

### fmod icon

- ID: `fmodeventemitter3d----res---addons-fmod-icons-fmod-icon-svg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `FmodEventEmitter3D = "res://addons/fmod/icons/fmod_icon.svg"`

### FmodEventEditorProperty

- ID: `addons-fmod-tool-property-editors-fmodeventeditorproperty-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `addons/fmod/tool/property_editors/FmodEventEditorProperty.tscn`

### FmodEventEditorProperty

- ID: `ext-resource-type--script--uid--uid---b32x60k0th8td--path--res---addons-fmod-tool-property-editors-fmodeventeditorproperty-gd--id--2-nkhkm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://b32x60k0th8td" path="res://addons/fmod/tool/property_editors/FmodEventEditorProperty.gd" id="2_nkhkm"]`

### FmodEventEditorProperty

- ID: `path----res---addons-fmod-tool-property-editors-fmodeventeditorproperty-gd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"path": "res://addons/fmod/tool/property_editors/FmodEventEditorProperty.gd"`

### FmodEventEditorProperty

- ID: `path--res---addons-fmod-tool-property-editors-fmodeventeditorproperty-gdc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `path="res://addons/fmod/tool/property_editors/FmodEventEditorProperty.gdc"`

### FmodEventEditorProperty

- ID: `res---addons-fmod-tool-property-editors-fmodeventeditorproperty-gd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/property_editors/FmodEventEditorProperty.gd`

### FmodEventEditorProperty.tscns1

- ID: `res---addons-fmod-tool-property-editors-fmodeventeditorproperty-tscns1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://addons/fmod/tool/property_editors/FmodEventEditorProperty.tscns1."`

### Focused Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nclickablecontrol-focusedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl+FocusedEventHandler`

### Get Drag For Scroll Event

- ID: `getdragforscrollevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Event For Player

- ID: `geteventforplayer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Event Name

- ID: `geteventname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Get Local Event

- ID: `getlocalevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### grave of the forgotten

- ID: `res---images-events-grave-of-the-forgotten-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/grave_of_the_forgotten.png<`

### grave of the forgotten vfx

- ID: `res---scenes-vfx-events-grave-of-the-forgotten-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/grave_of_the_forgotten_vfx.tscn`

### grave of the forgotten vfx

- ID: `scenes-vfx-events-grave-of-the-forgotten-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/grave_of_the_forgotten_vfx.tscn`

### grave of the forgotten.png

- ID: `images-events-grave-of-the-forgotten-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/grave_of_the_forgotten.png.import`

### GraveOfTheForgotten

- ID: `res---src-core-models-events-graveoftheforgotten-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/GraveOfTheForgotten.cs`

### Handle Ancient Event Dialogue

- ID: `handleancienteventdialogue`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Handle Event Combat

- ID: `handleeventcombat`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Handle Event Option Chosen Message

- ID: `handleeventoptionchosenmessage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Handle Fake Merchant Event

- ID: `handlefakemerchantevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Handle Shared Event Option Chosen Message

- ID: `handlesharedeventoptionchosenmessage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Handle Voted For Shared Event Option Message

- ID: `handlevotedforsharedeventoptionmessage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Has Event Pet

- ID: `haseventpet`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### hello world

- ID: `filename----event-hello-world-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/hello_world.png",`

### hide Event Ui

- ID: `hideeventui`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Hide Event Visuals

- ID: `hideeventvisuals`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Hit Creature Event Handler

- ID: `megacrit-sts2-core-nodes-vfx-nrollingbouldervfx-hitcreatureeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Vfx.NRollingBoulderVfx+HitCreatureEventHandler`

### Hovered Event Handler

- ID: `megacrit-sts2-core-nodes-screens-runhistoryscreen-ndeckhistory-hoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen.NDeckHistory+HoveredEventHandler`

### Hovered Event Handler

- ID: `megacrit-sts2-core-nodes-screens-shops-nmerchantslot-hoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantSlot+HoveredEventHandler`

### hungry for mushrooms

- ID: `res---images-events-hungry-for-mushrooms-png-v`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/hungry_for_mushrooms.png<v`

### hungry for mushrooms vfx

- ID: `res---scenes-vfx-events-hungry-for-mushrooms-vfx-tscnkf`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/hungry_for_mushrooms_vfx.tscnkf`

### hungry for mushrooms vfx

- ID: `scenes-vfx-events-hungry-for-mushrooms-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/hungry_for_mushrooms_vfx.tscn`

### hungry for mushrooms.png

- ID: `images-events-hungry-for-mushrooms-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/hungry_for_mushrooms.png.import`

### HungryForMushrooms

- ID: `res---src-core-models-events-hungryformushrooms-cs4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/HungryForMushrooms.cs4`

### ICustom Event Node

- ID: `megacrit-sts2-core-nodes-events-icustomeventnode`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.ICustomEventNode`

### ICustomEventNode

- ID: `res---src-core-nodes-events-icustomeventnode-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/ICustomEventNode.cs`

### infested automaton

- ID: `res---images-events-infested-automaton-pngl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/infested_automaton.pngl`

### infested automaton flies

- ID: `ext-resource-type--texture2d--uid--uid---cjn0wwfjlw6xv--path--res---images-packed-vfx-event-infested-automaton-flies-png--id--3-fqjrq`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cjn0wwfjlw6xv" path="res://images/packed/vfx/event/infested_automaton_flies.png" id="3_fqjrq"]`

### infested automaton flies

- ID: `ext-resource-type--texture2d--uid--uid---cjn0wwfjlw6xv--path--res---images-packed-vfx-event-infested-automaton-flies-png--id--4-yaa5o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cjn0wwfjlw6xv" path="res://images/packed/vfx/event/infested_automaton_flies.png" id="4_yaa5o"]`

### infested automaton flies

- ID: `ext-resource-type--texture2d--uid--uid---cjn0wwfjlw6xv--path--res---images-packed-vfx-event-infested-automaton-flies-png--id--5-786oa`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cjn0wwfjlw6xv" path="res://images/packed/vfx/event/infested_automaton_flies.png" id="5_786oa"]`

### infested automaton flies

- ID: `ext-resource-type--texture2d--uid--uid---cjn0wwfjlw6xv--path--res---images-packed-vfx-event-infested-automaton-flies-png--id--5-7y7qb`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cjn0wwfjlw6xv" path="res://images/packed/vfx/event/infested_automaton_flies.png" id="5_7y7qb"]`

### infested automaton flies

- ID: `ext-resource-type--texture2d--uid--uid---cjn0wwfjlw6xv--path--res---images-packed-vfx-event-infested-automaton-flies-png--id--5-difsb`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cjn0wwfjlw6xv" path="res://images/packed/vfx/event/infested_automaton_flies.png" id="5_difsb"]`

### infested automaton flies

- ID: `res---images-packed-vfx-event-infested-automaton-flies-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/infested_automaton_flies.png`

### infested automaton flies.png

- ID: `images-packed-vfx-event-infested-automaton-flies-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/infested_automaton_flies.png.import`

### infested automaton vfx

- ID: `res---scenes-vfx-events-infested-automaton-vfx-tscnz`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/infested_automaton_vfx.tscnZ`

### infested automaton vfx

- ID: `scenes-vfx-events-infested-automaton-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/infested_automaton_vfx.tscn`

### infested automaton.png

- ID: `images-events-infested-automaton-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/infested_automaton.png.import`

### InfestedAutomaton

- ID: `res---src-core-models-events-infestedautomaton-csdi`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/InfestedAutomaton.csdI`

### input Event

- ID: `inputevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### input Events

- ID: `inputevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Input Rebound Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ninputmanager-inputreboundeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NInputManager+InputReboundEventHandler`

### Inventory Closed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-shops-nmerchantinventory-inventoryclosedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantInventory+InventoryClosedEventHandler`

### Invoke Died Event

- ID: `invokediedevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### jungle maze adventure

- ID: `res---images-events-jungle-maze-adventure-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/jungle_maze_adventure.png`

### jungle maze adventure.png

- ID: `images-events-jungle-maze-adventure-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/jungle_maze_adventure.png.import`

### JungleMazeAdventure

- ID: `res---src-core-models-events-junglemazeadventure-csy`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/JungleMazeAdventure.csy`

### Killed By Event

- ID: `killedbyevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### List Event Choice Metric

- ID: `listeventchoicemetric`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### List Event Choice Metric Serialize Handler

- ID: `listeventchoicemetricserializehandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### List Event Option History Entry

- ID: `listeventoptionhistoryentry`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Load Room Event Assets

- ID: `loadroomeventassets`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Local Mutable Event

- ID: `localmutableevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### lost wisp

- ID: `res---images-events-lost-wisp-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/lost_wisp.png`

### lost wisp vfx

- ID: `res---scenes-vfx-events-lost-wisp-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/lost_wisp_vfx.tscn`

### lost wisp vfx

- ID: `scenes-vfx-events-lost-wisp-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/lost_wisp_vfx.tscn`

### lost wisp.png

- ID: `images-events-lost-wisp-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/lost_wisp.png.import`

### LostWisp

- ID: `res---src-core-models-events-lostwisp-csmqj`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/LostWisp.csMqj`

### luminous choir

- ID: `res---images-events-luminous-choir-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/luminous_choir.png`

### luminous choir.png

- ID: `images-events-luminous-choir-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/luminous_choir.png.import`

### LuminousChoir

- ID: `res---src-core-models-events-luminouschoir-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/LuminousChoir.cs`

### mad science attack

- ID: `filename----event-mad-science-attack-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/mad_science_attack.png",`

### mad science power

- ID: `filename----event-mad-science-power-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/mad_science_power.png",`

### mad science skill

- ID: `filename----event-mad-science-skill-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/mad_science_skill.png",`

### Map Drawing Event Type

- ID: `megacrit-sts2-core-multiplayer-game-peerinput-mapdrawingeventtype`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput.MapDrawingEventType`

### MapDrawingEventType

- ID: `res---src-core-multiplayer-game-peerinput-mapdrawingeventtype-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Game/PeerInput/MapDrawingEventType.cs`

### Mark Event As Seen

- ID: `markeventasseen`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### maul

- ID: `filename----event-maul-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/maul.png",`

### max Event Count

- ID: `maxeventcount`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Mega Event

- ID: `megacrit-sts2-core-bindings-megaspine-megaevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaEvent`

### Mega Event Data

- ID: `megacrit-sts2-core-bindings-megaspine-megaeventdata`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaEventData`

### MegaEvent

- ID: `res---src-core-bindings-megaspine-megaevent-cs-fbj2`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Bindings/MegaSpine/MegaEvent.cs$fbj2"`

### MegaEventData

- ID: `res---src-core-bindings-megaspine-megaeventdata-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Bindings/MegaSpine/MegaEventData.cs`

### Merchant Opened Event Handler

- ID: `megacrit-sts2-core-nodes-rooms-nmerchantbutton-merchantopenedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantButton+MerchantOpenedEventHandler`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherecell-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereCell+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereDialogue+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalsphereitem-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereItem+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheremask-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereMask+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-custom-nfakemerchant-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.NFakeMerchant+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialoguehitbox-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueHitbox+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialogueline-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueLine+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-nancienteventlayout-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-ncombateventlayout-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NCombatEventLayout+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-ndivinationbutton-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NDivinationButton+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-neventlayout-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-rooms-neventroom-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+MethodName`

### Method Name

- ID: `megacrit-sts2-core-nodes-vfx-events-nsymbioteeyemovementvfx-methodname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Vfx.Events.NSymbioteEyeMovementVfx+MethodName`

### mirror mask3

- ID: `ext-resource-type--texture2d--uid--uid---c6djtq0qwc5cw--path--res---images-events-mirror-mask3-png--id--2-imu0m`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c6djtq0qwc5cw" path="res://images/events/mirror_mask3.png" id="2_imu0m"]`

### mirror mask3

- ID: `res---images-events-mirror-mask3-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/mirror_mask3.png`

### mirror mask3.png

- ID: `images-events-mirror-mask3-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/mirror_mask3.png.import`

### MockEventModel

- ID: `res---src-core-models-events-mocks-mockeventmodel-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Mocks/MockEventModel.cs`

### MockPreventDeathPower

- ID: `res---src-core-models-powers-mocks-mockpreventdeathpower-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Powers/Mocks/MockPreventDeathPower.cs`

### Mode Changed Event Handler

- ID: `megacrit-sts2-core-nodes-combat-nplayerhand-modechangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NPlayerHand+ModeChangedEventHandler`

### Modifiers Changed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-mainmenu-ncustomrunmodifierslist-modifierschangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NCustomRunModifiersList+ModifiersChangedEventHandler`

### Modify Next Event

- ID: `modifynextevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### morphic grove

- ID: `res---images-events-morphic-grove-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/morphic_grove.png`

### morphic grove foreground

- ID: `ext-resource-type--texture2d--uid--uid---c56fo54ukxcy8--path--res---images-packed-vfx-event-morphic-grove-morphic-grove-foreground-png--id--4-j6hrm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c56fo54ukxcy8" path="res://images/packed/vfx/event/morphic_grove/morphic_grove_foreground.png" id="4_j6hrm"]`

### morphic grove foreground

- ID: `res---images-packed-vfx-event-morphic-grove-morphic-grove-foreground-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/morphic_grove/morphic_grove_foreground.png`

### morphic grove foreground.png

- ID: `images-packed-vfx-event-morphic-grove-morphic-grove-foreground-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/morphic_grove/morphic_grove_foreground.png.import`

### morphic grove vfx

- ID: `res---scenes-vfx-events-morphic-grove-vfx-tscnx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/morphic_grove_vfx.tscnx`

### morphic grove vfx

- ID: `scenes-vfx-events-morphic-grove-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/morphic_grove_vfx.tscn`

### morphic grove.png

- ID: `images-events-morphic-grove-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/morphic_grove.png.import`

### MorphicGrove

- ID: `res---src-core-models-events-morphicgrove-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/MorphicGrove.cs`

### Mouse Detected Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ncontrollermanager-mousedetectedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NControllerManager+MouseDetectedEventHandler`

### Mouse Pressed Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nclickablecontrol-mousepressedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl+MousePressedEventHandler`

### Mouse Pressed Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nscrollbar-mousepressedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NScrollbar+MousePressedEventHandler`

### Mouse Pressed Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nslider-mousepressedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NSlider+MousePressedEventHandler`

### Mouse Released Event Handler

- ID: `megacrit-sts2-core-nodes-debug-nfpsvisualizer-mousereleasedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Debug.NFpsVisualizer+MouseReleasedEventHandler`

### Mouse Released Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nclickablecontrol-mousereleasedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl+MouseReleasedEventHandler`

### Mouse Released Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nscrollbar-mousereleasedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NScrollbar+MouseReleasedEventHandler`

### Mouse Released Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nslider-mousereleasedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NSlider+MouseReleasedEventHandler`

### MysteriousKnightEventEncounter

- ID: `res---src-core-models-encounters-mysteriousknighteventencounter-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/MysteriousKnightEventEncounter.cs`

### NAncient Dialogue Hitbox

- ID: `megacrit-sts2-core-nodes-events-nancientdialoguehitbox`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueHitbox`

### NAncient Dialogue Line

- ID: `megacrit-sts2-core-nodes-events-nancientdialogueline`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueLine`

### NAncient Event Layout

- ID: `megacrit-sts2-core-nodes-events-nancienteventlayout`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout`

### NAncientDialogueHitbox

- ID: `ext-resource-type--script--uid--uid---dfls1sumgwb36--path--res---src-core-nodes-events-nancientdialoguehitbox-cs--id--5-spk35`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dfls1sumgwb36" path="res://src/Core/Nodes/Events/NAncientDialogueHitbox.cs" id="5_spk35"]`

### NAncientDialogueHitbox

- ID: `res---src-core-nodes-events-nancientdialoguehitbox-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NAncientDialogueHitbox.cs`

### NAncientDialogueLine

- ID: `ext-resource-type--script--uid--uid---cqo0ws30xidun--path--res---src-core-nodes-events-nancientdialogueline-cs--id--1-8k2e5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://cqo0ws30xidun" path="res://src/Core/Nodes/Events/NAncientDialogueLine.cs" id="1_8k2e5"]`

### NAncientDialogueLine

- ID: `res---src-core-nodes-events-nancientdialogueline-cs-q`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NAncientDialogueLine.cs{q`

### NAncientEventLayout

- ID: `ext-resource-type--script--uid--uid---jey7qn0rnfl0--path--res---src-core-nodes-events-nancienteventlayout-cs--id--1-nb4me`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://jey7qn0rnfl0" path="res://src/Core/Nodes/Events/NAncientEventLayout.cs" id="1_nb4me"]`

### NAncientEventLayout

- ID: `res---src-core-nodes-events-nancienteventlayout-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NAncientEventLayout.cs`

### NCombat Event Layout

- ID: `megacrit-sts2-core-nodes-events-ncombateventlayout`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NCombatEventLayout`

### NCombatEventLayout

- ID: `ext-resource-type--script--uid--uid---cbd4afyvlsa28--path--res---src-core-nodes-events-ncombateventlayout-cs--id--1-po4aa`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://cbd4afyvlsa28" path="res://src/Core/Nodes/Events/NCombatEventLayout.cs" id="1_po4aa"]`

### NCombatEventLayout

- ID: `res---src-core-nodes-events-ncombateventlayout-cstl`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NCombatEventLayout.csTl,`

### NCrystal Sphere Cell

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherecell`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereCell`

### NCrystal Sphere Dialogue

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereDialogue`

### NCrystal Sphere Item

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalsphereitem`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereItem`

### NCrystal Sphere Mask

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheremask`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereMask`

### NCrystal Sphere Screen

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen`

### NCrystalSphereCell

- ID: `ext-resource-type--script--uid--uid---deyocmnoxh2h--path--res---src-core-nodes-events-custom-crystalsphere-ncrystalspherecell-cs--id--1-pstm0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://deyocmnoxh2h" path="res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereCell.cs" id="1_pstm0"]`

### NCrystalSphereCell

- ID: `res---src-core-nodes-events-custom-crystalsphere-ncrystalspherecell-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereCell.cs`

### NCrystalSphereDialogue

- ID: `ext-resource-type--script--uid--uid---dlgtqhfjtf73--path--res---src-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue-cs--id--1-5gm1a`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dlgtqhfjtf73" path="res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereDialogue.cs" id="1_5gm1a"]`

### NCrystalSphereDialogue

- ID: `res---src-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereDialogue.cs`

### NCrystalSphereItem

- ID: `ext-resource-type--script--uid--uid---dhqnaxdf385ds--path--res---src-core-nodes-events-custom-crystalsphere-ncrystalsphereitem-cs--id--1-e8pif`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dhqnaxdf385ds" path="res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereItem.cs" id="1_e8pif"]`

### NCrystalSphereItem

- ID: `res---src-core-nodes-events-custom-crystalsphere-ncrystalsphereitem-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereItem.cs`

### NCrystalSphereMask

- ID: `ext-resource-type--script--uid--uid---dh7b1y25it22y--path--res---src-core-nodes-events-custom-crystalsphere-ncrystalspheremask-cs--id--5-tsr0m`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dh7b1y25it22y" path="res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereMask.cs" id="5_tsr0m"]`

### NCrystalSphereMask

- ID: `res---src-core-nodes-events-custom-crystalsphere-ncrystalspheremask-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereMask.cs`

### NCrystalSphereScreen

- ID: `ext-resource-type--script--uid--uid---nup24q26505e--path--res---src-core-nodes-events-custom-crystalsphere-ncrystalspherescreen-cs--id--1-12402`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://nup24q26505e" path="res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereScreen.cs" id="1_12402"]`

### NCrystalSphereScreen

- ID: `res---src-core-nodes-events-custom-crystalsphere-ncrystalspherescreen-csg0h`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereScreen.csG0H`

### NDivination Button

- ID: `megacrit-sts2-core-nodes-events-ndivinationbutton`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NDivinationButton`

### NDivinationButton

- ID: `ext-resource-type--script--uid--uid---b1ftew3lxqiv1--path--res---src-core-nodes-events-ndivinationbutton-cs--id--1-t7brm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://b1ftew3lxqiv1" path="res://src/Core/Nodes/Events/NDivinationButton.cs" id="1_t7brm"]`

### NDivinationButton

- ID: `res---src-core-nodes-events-ndivinationbutton-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NDivinationButton.cs`

### neow

- ID: `res---scenes-events-background-scenes-neow-tscnr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/neow.tscnR`

### Neow

- ID: `res---src-core-models-events-neow-csr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Neow.csr`

### neow

- ID: `scenes-events-background-scenes-neow-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/neow.tscn`

### neow water reflection

- ID: `res---scenes-events-background-scenes-neow-water-reflection-gdshader`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/neow_water_reflection.gdshader?`

### Net Map Drawing Event

- ID: `megacrit-sts2-core-multiplayer-game-peerinput-netmapdrawingevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput.NetMapDrawingEvent`

### NetMapDrawingEvent

- ID: `res---src-core-multiplayer-game-peerinput-netmapdrawingevent-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Game/PeerInput/NetMapDrawingEvent.cs`

### NEvent Layout

- ID: `megacrit-sts2-core-nodes-events-neventlayout`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout`

### NEvent Option Button

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton`

### NEvent Room

- ID: `megacrit-sts2-core-nodes-rooms-neventroom`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom`

### NEventLayout

- ID: `ext-resource-type--script--uid--uid---be5tcq5cnqlvj--path--res---src-core-nodes-events-neventlayout-cs--id--1-76lfr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://be5tcq5cnqlvj" path="res://src/Core/Nodes/Events/NEventLayout.cs" id="1_76lfr"]`

### NEventLayout

- ID: `res---src-core-nodes-events-neventlayout-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NEventLayout.cs`

### NEventOptionButton

- ID: `ext-resource-type--script--uid--uid---ciaipjg10g6in--path--res---src-core-nodes-events-neventoptionbutton-cs--id--1-kqlw5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://ciaipjg10g6in" path="res://src/Core/Nodes/Events/NEventOptionButton.cs" id="1_kqlw5"]`

### NEventOptionButton

- ID: `ext-resource-type--script--uid--uid---ciaipjg10g6in--path--res---src-core-nodes-events-neventoptionbutton-cs--id--1-pnf1f`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://ciaipjg10g6in" path="res://src/Core/Nodes/Events/NEventOptionButton.cs" id="1_pnf1f"]`

### NEventOptionButton

- ID: `res---src-core-nodes-events-neventoptionbutton-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/NEventOptionButton.cs`

### NEventRoom

- ID: `ext-resource-type--script--uid--uid---b7s18yn22bknu--path--res---src-core-nodes-rooms-neventroom-cs--id--1-sknbe`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://b7s18yn22bknu" path="res://src/Core/Nodes/Rooms/NEventRoom.cs" id="1_sknbe"]`

### NEventRoom

- ID: `res---src-core-nodes-rooms-neventroom-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Rooms/NEventRoom.cs`

### Next Event

- ID: `nextevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### NFake Merchant

- ID: `megacrit-sts2-core-nodes-events-custom-nfakemerchant`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.NFakeMerchant`

### NFakeMerchant

- ID: `ext-resource-type--script--uid--uid---dsn8m6d5vwtlf--path--res---src-core-nodes-events-custom-nfakemerchant-cs--id--1-w55kg`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dsn8m6d5vwtlf" path="res://src/Core/Nodes/Events/Custom/NFakeMerchant.cs" id="1_w55kg"]`

### NFakeMerchant

- ID: `res---src-core-nodes-events-custom-nfakemerchant-cst`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Events/Custom/NFakeMerchant.csT|`

### Node Hovered Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-nodehoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+NodeHoveredEventHandler`

### Node Unhovered Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-nodeunhoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+NodeUnhoveredEventHandler`

### non Event Odds

- ID: `noneventodds`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### nonupeipe

- ID: `res---scenes-events-background-scenes-nonupeipe-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/nonupeipe.tscn`

### Nonupeipe

- ID: `res---src-core-models-events-nonupeipe-csf`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Nonupeipe.csF`

### nonupeipe

- ID: `scenes-events-background-scenes-nonupeipe-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/nonupeipe.tscn`

### NSymbiote Eye Movement Vfx

- ID: `megacrit-sts2-core-nodes-vfx-events-nsymbioteeyemovementvfx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Vfx.Events.NSymbioteEyeMovementVfx`

### NSymbioteEyeMovementVfx

- ID: `ext-resource-type--script--uid--uid---dvh4ft75j6tsk--path--res---src-core-nodes-vfx-events-nsymbioteeyemovementvfx-cs--id--2-j8d4e`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dvh4ft75j6tsk" path="res://src/Core/Nodes/Vfx/Events/NSymbioteEyeMovementVfx.cs" id="2_j8d4e"]`

### NSymbioteEyeMovementVfx

- ID: `ext-resource-type--script--uid--uid---dvh4ft75j6tsk--path--res---src-core-nodes-vfx-events-nsymbioteeyemovementvfx-cs--id--4-vjmaa`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Script" uid="uid://dvh4ft75j6tsk" path="res://src/Core/Nodes/Vfx/Events/NSymbioteEyeMovementVfx.cs" id="4_vjmaa"]`

### NSymbioteEyeMovementVfx

- ID: `res---src-core-nodes-vfx-events-nsymbioteeyemovementvfx-csh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Nodes/Vfx/Events/NSymbioteEyeMovementVfx.csH`

### On Animation Event

- ID: `onanimationevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### On Animation Finished Event Handler

- ID: `nepochchains-onanimationfinishedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `NEpochChains+OnAnimationFinishedEventHandler`

### On Entering Event Combat

- ID: `onenteringeventcombat`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### On Event Finished

- ID: `oneventfinished`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### On Event State Changed

- ID: `oneventstatechanged`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### On Front Event

- ID: `onfrontevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Opened Event Handler

- ID: `megacrit-sts2-core-nodes-screens-map-nmapscreen-openedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Map.NMapScreen+OpenedEventHandler`

### orobas

- ID: `res---scenes-events-background-scenes-orobas-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/orobas.tscn`

### Orobas

- ID: `res---src-core-models-events-orobas-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Orobas.cs`

### orobas

- ID: `scenes-events-background-scenes-orobas-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/orobas.tscn`

### pael

- ID: `res---scenes-events-background-scenes-pael-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/pael.tscn`

### Pael

- ID: `res---src-core-models-events-pael-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Pael.cs`

### pael

- ID: `scenes-events-background-scenes-pael-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/pael.tscn`

### Parent Event Id

- ID: `parenteventid`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### preventer

- ID: `preventer`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Process Controller Event

- ID: `processcontrollerevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Process Mouse Drawing Event

- ID: `processmousedrawingevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Process Mouse Event

- ID: `processmouseevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Process Pan Event

- ID: `processpanevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Process Scroll Event

- ID: `processscrollevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Prop Name event Choices

- ID: `propname-eventchoices`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherecell-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereCell+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereDialogue+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalsphereitem-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereItem+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheremask-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereMask+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-custom-nfakemerchant-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.NFakeMerchant+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialoguehitbox-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueHitbox+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialogueline-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueLine+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-nancienteventlayout-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-ncombateventlayout-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NCombatEventLayout+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-ndivinationbutton-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NDivinationButton+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-neventlayout-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-rooms-neventroom-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+PropertyName`

### Property Name

- ID: `megacrit-sts2-core-nodes-vfx-events-nsymbioteeyemovementvfx-propertyname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Vfx.Events.NSymbioteEyeMovementVfx+PropertyName`

### Pull Next Event

- ID: `pullnextevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### punch off

- ID: `res---images-events-punch-off-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/punch_off.png`

### punch off vfx

- ID: `res---scenes-vfx-events-punch-off-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/punch_off_vfx.tscn`

### punch off vfx

- ID: `scenes-vfx-events-punch-off-vfx-tscn-x`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/punch_off_vfx.tscn`X`

### punch off.png

- ID: `images-events-punch-off-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/punch_off.png.import`

### PunchOff

- ID: `res---src-core-models-events-punchoff-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/PunchOff.cs`

### PunchOffEventEncounter

- ID: `res---src-core-models-encounters-punchoffeventencounter-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/PunchOffEventEncounter.cs`

### Query Changed Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-nsearchbar-querychangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NSearchBar+QueryChangedEventHandler`

### Query Submitted Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-nsearchbar-querysubmittedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NSearchBar+QuerySubmittedEventHandler`

### Queue Or Send Event

- ID: `queueorsendevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### ranwid the elder

- ID: `res---images-events-ranwid-the-elder-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/ranwid_the_elder.png:`

### ranwid the elder vfx

- ID: `res---scenes-vfx-events-ranwid-the-elder-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/ranwid_the_elder_vfx.tscn`

### ranwid the elder vfx

- ID: `scenes-vfx-events-ranwid-the-elder-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/ranwid_the_elder_vfx.tscn`

### ranwid the elder.png

- ID: `images-events-ranwid-the-elder-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/ranwid_the_elder.png.import`

### RanwidTheElder

- ID: `res---src-core-models-events-ranwidtheelder-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/RanwidTheElder.cs`

### reflections

- ID: `ext-resource-type--texture2d--uid--uid---cahpcdd33b2re--path--res---images-events-reflections-png--id--1-s2l28`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cahpcdd33b2re" path="res://images/events/reflections.png" id="1_s2l28"]`

### reflections

- ID: `res---images-events-reflections-png6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/reflections.png6`

### Reflections

- ID: `res---src-core-models-events-reflections-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Reflections.cs,`

### reflections.png

- ID: `images-events-reflections-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/reflections.png.import`

### Refresh Event State

- ID: `refresheventstate`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### relax

- ID: `filename----event-relax-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/relax.png",`

### Released Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nclickablecontrol-releasedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl+ReleasedEventHandler`

### Remove Event From Set

- ID: `removeeventfromset`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Resume Events

- ID: `resumeevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### reusable Event

- ID: `reusableevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Reward Claimed Event Handler

- ID: `megacrit-sts2-core-nodes-rewards-nlinkedrewardset-rewardclaimedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rewards.NLinkedRewardSet+RewardClaimedEventHandler`

### Reward Claimed Event Handler

- ID: `megacrit-sts2-core-nodes-rewards-nrewardbutton-rewardclaimedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rewards.NRewardButton+RewardClaimedEventHandler`

### Reward Skipped Event Handler

- ID: `megacrit-sts2-core-nodes-rewards-nrewardbutton-rewardskippedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rewards.NRewardButton+RewardSkippedEventHandler`

### Rider Effect

- ID: `megacrit-sts2-core-models-events-tinkertime-ridereffect`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TinkerTime+RiderEffect`

### room full of cheese

- ID: `res---images-events-room-full-of-cheese-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/room_full_of_cheese.png`

### room full of cheese.png

- ID: `images-events-room-full-of-cheese-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/room_full_of_cheese.png.import`

### RoomFullOfCheese

- ID: `res---src-core-models-events-roomfullofcheese-cs-k`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/RoomFullOfCheese.cs{k`

### round tea party

- ID: `res---images-events-round-tea-party-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/round_tea_party.png`

### round tea party.png

- ID: `images-events-round-tea-party-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/round_tea_party.png.import`

### RoundTeaParty

- ID: `res---src-core-models-events-roundteaparty-csth`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/RoundTeaParty.csTH`

### sapphire seed

- ID: `res---images-events-sapphire-seed-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/sapphire_seed.png`

### sapphire seed.png

- ID: `images-events-sapphire-seed-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/sapphire_seed.png.import`

### SapphireSeed

- ID: `res---src-core-models-events-sapphireseed-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SapphireSeed.cs`

### Save Event Option To History

- ID: `saveeventoptiontohistory`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Selected Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ndropdownitem-selectedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NDropdownItem+SelectedEventHandler`

### self help book

- ID: `res---images-events-self-help-book-pnga`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/self_help_book.pnga`

### self help book shine

- ID: `ext-resource-type--texture2d--uid--uid---bxj366erre8q1--path--res---images-packed-vfx-event-self-help-book-shine-png--id--1-7fgdd`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bxj366erre8q1" path="res://images/packed/vfx/event/self_help_book_shine.png" id="1_7fgdd"]`

### self help book shine

- ID: `res---images-packed-vfx-event-self-help-book-shine-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/self_help_book_shine.png`

### self help book shine.png

- ID: `images-packed-vfx-event-self-help-book-shine-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/self_help_book_shine.png.import`

### self help book vfx

- ID: `res---scenes-vfx-events-self-help-book-vfx-tscna`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/self_help_book_vfx.tscnA`

### self help book vfx

- ID: `scenes-vfx-events-self-help-book-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/self_help_book_vfx.tscn`

### self help book.png

- ID: `images-events-self-help-book-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/self_help_book.png.import`

### SelfHelpBook

- ID: `res---src-core-models-events-selfhelpbook-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SelfHelpBook.cs`

### Set Event

- ID: `setevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Set Event Finished

- ID: `seteventfinished`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Set Event State

- ID: `seteventstate`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Set Initial Event State

- ID: `setinitialeventstate`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Settings Closed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-settings-nsettingsscreen-settingsclosedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Settings.NSettingsScreen+SettingsClosedEventHandler`

### Settings Opened Event Handler

- ID: `megacrit-sts2-core-nodes-screens-settings-nsettingsscreen-settingsopenedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Settings.NSettingsScreen+SettingsOpenedEventHandler`

### shared Event Label

- ID: `sharedeventlabel`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### shared Event Loc

- ID: `sharedeventloc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Shared Event Option Chosen Message

- ID: `megacrit-sts2-core-multiplayer-messages-game-sync-sharedeventoptionchosenmessage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync.SharedEventOptionChosenMessage`

### SharedEventOptionChosenMessage

- ID: `res---src-core-multiplayer-messages-game-sync-sharedeventoptionchosenmessage-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Messages/Game/Sync/SharedEventOptionChosenMessage.cs`

### Should Report Sentry Events

- ID: `shouldreportsentryevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Should Resume Parent Event

- ID: `shouldresumeparentevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Should Resume Parent Event After Combat

- ID: `shouldresumeparenteventaftercombat`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherecell-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereCell+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheredialogue-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereDialogue+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalsphereitem-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereItem+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspheremask-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereMask+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-crystalsphere-ncrystalspherescreen-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere.NCrystalSphereScreen+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-custom-nfakemerchant-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.Custom.NFakeMerchant+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialoguehitbox-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueHitbox+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-nancientdialogueline-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientDialogueLine+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-nancienteventlayout-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-ncombateventlayout-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NCombatEventLayout+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-ndivinationbutton-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NDivinationButton+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-neventlayout-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventLayout+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-events-neventoptionbutton-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Events.NEventOptionButton+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-rooms-neventroom-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom+SignalName`

### Signal Name

- ID: `megacrit-sts2-core-nodes-vfx-events-nsymbioteeyemovementvfx-signalname`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Vfx.Events.NSymbioteEyeMovementVfx+SignalName`

### slippery bridge

- ID: `res---images-events-slippery-bridge-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/slippery_bridge.png`

### slippery bridge.png

- ID: `images-events-slippery-bridge-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/slippery_bridge.png.import`

### SlipperyBridge

- ID: `res---src-core-models-events-slipperybridge-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SlipperyBridge.cs`

### small divination icon

- ID: `ext-resource-type--texture2d--uid--uid---b63aqrp7sgreb--path--res---images-events-crystal-sphere-small-divination-icon-png--id--6-81bdj`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://b63aqrp7sgreb" path="res://images/events/crystal_sphere/small_divination_icon.png" id="6_81bdj"]`

### small divination icon

- ID: `res---images-events-crystal-sphere-small-divination-icon-pngq`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/crystal_sphere/small_divination_icon.pngQ`

### small divination icon.png

- ID: `images-events-crystal-sphere-small-divination-icon-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/crystal_sphere/small_divination_icon.png.import`

### spiraling whirlpool

- ID: `res---images-events-spiraling-whirlpool-pngh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/spiraling_whirlpool.pngh`

### spiraling whirlpool vfx

- ID: `res---scenes-vfx-events-spiraling-whirlpool-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/spiraling_whirlpool_vfx.tscn`

### spiraling whirlpool vfx

- ID: `scenes-vfx-events-spiraling-whirlpool-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/spiraling_whirlpool_vfx.tscn`

### spiraling whirlpool.png

- ID: `images-events-spiraling-whirlpool-png-import0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/spiraling_whirlpool.png.import0`

### SpiralingWhirlpool

- ID: `res---src-core-models-events-spiralingwhirlpool-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SpiralingWhirlpool.cs`

### spirit grafter

- ID: `res---images-events-spirit-grafter-pngui`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/spirit_grafter.pngui`

### spirit grafter.png

- ID: `images-events-spirit-grafter-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/spirit_grafter.png.import`

### SpiritGrafter

- ID: `res---src-core-models-events-spiritgrafter-cso`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SpiritGrafter.cso`

### Stack Modified Event Handler

- ID: `megacrit-sts2-core-nodes-screens-mainmenu-nsubmenustack-stackmodifiedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NSubmenuStack+StackModifiedEventHandler`

### stone of all time

- ID: `res---images-events-stone-of-all-time-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/stone_of_all_time.png`

### stone of all time.png

- ID: `images-events-stone-of-all-time-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/stone_of_all_time.png.import`

### StoneOfAllTime

- ID: `res---src-core-models-events-stoneofalltime-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/StoneOfAllTime.cs`

### Subscribe To Combat Events

- ID: `subscribetocombatevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Subscribe To Creature Events

- ID: `subscribetocreatureevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Subscribe To Events

- ID: `subscribetoevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Subscribe To Model Events

- ID: `subscribetomodelevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### sunken statue

- ID: `res---images-events-sunken-statue-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/sunken_statue.png`

### sunken statue.png

- ID: `images-events-sunken-statue-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/sunken_statue.png.import`

### sunken treasury

- ID: `res---images-events-sunken-treasury-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/sunken_treasury.png`

### sunken treasury vfx

- ID: `res---scenes-vfx-events-sunken-treasury-vfx-tscn-9-d`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/sunken_treasury_vfx.tscn(9{d`

### sunken treasury vfx

- ID: `scenes-vfx-events-sunken-treasury-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/sunken_treasury_vfx.tscn`

### sunken treasury.png

- ID: `images-events-sunken-treasury-png-importp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/sunken_treasury.png.importp`

### SunkenStatue

- ID: `res---src-core-models-events-sunkenstatue-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SunkenStatue.cs`

### SunkenTreasury

- ID: `res---src-core-models-events-sunkentreasury-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/SunkenTreasury.cs`

### symbiote

- ID: `ext-resource-type--texture2d--uid--uid---cuqowf008plle--path--res---images-events-symbiote-png--id--2-1x4hx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cuqowf008plle" path="res://images/events/symbiote.png" id="2_1x4hx"]`

### symbiote

- ID: `res---images-events-symbiote-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/symbiote.png`

### Symbiote

- ID: `res---src-core-models-events-symbiote-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Symbiote.cs`

### symbiote eye

- ID: `ext-resource-type--texture2d--uid--uid---kfetc2wnbbxi--path--res---images-packed-vfx-event-symbiote-symbiote-eye-png--id--3-ngsx0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://kfetc2wnbbxi" path="res://images/packed/vfx/event/symbiote/symbiote_eye.png" id="3_ngsx0"]`

### symbiote eye

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-pnga`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye.pngA`

### symbiote eye blink 01

- ID: `ext-resource-type--texture2d--uid--uid---d3puchln3s5er--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-01-png--id--6-b01w1`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://d3puchln3s5er" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_01.png" id="6_b01w1"]`

### symbiote eye blink 01

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-01-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_01.png`

### symbiote eye blink 01.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-01-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_01.png.import`

### symbiote eye blink 02

- ID: `ext-resource-type--texture2d--uid--uid---ccf8cukcermmu--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-02-png--id--8-pmeeu`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://ccf8cukcermmu" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_02.png" id="8_pmeeu"]`

### symbiote eye blink 02

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-02-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_02.png`

### symbiote eye blink 02.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-02-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_02.png.import`

### symbiote eye blink 03

- ID: `ext-resource-type--texture2d--uid--uid---b5nmy0pgfbl0h--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-03-png--id--10-nfvt8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://b5nmy0pgfbl0h" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_03.png" id="10_nfvt8"]`

### symbiote eye blink 03

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-03-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_03.png`

### symbiote eye blink 03.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-03-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_03.png.import`

### symbiote eye blink 06

- ID: `ext-resource-type--texture2d--uid--uid---dpr34tpgj0wr1--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-06-png--id--11-mx7kh`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dpr34tpgj0wr1" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_06.png" id="11_mx7kh"]`

### symbiote eye blink 06

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-06-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_06.png`

### symbiote eye blink 06.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-06-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_06.png.import`

### symbiote eye blink 09

- ID: `ext-resource-type--texture2d--uid--uid---cfcewwtaxmy4t--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-09-png--id--12-lsaca`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cfcewwtaxmy4t" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_09.png" id="12_lsaca"]`

### symbiote eye blink 09

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-09-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_09.png`

### symbiote eye blink 09.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-09-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_09.png.import`

### symbiote eye blink 13

- ID: `ext-resource-type--texture2d--uid--uid---d2wkbm1iqtmfp--path--res---images-packed-vfx-event-symbiote-symbiote-eye-blink-13-png--id--13-3h7b0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://d2wkbm1iqtmfp" path="res://images/packed/vfx/event/symbiote/symbiote_eye_blink_13.png" id="13_3h7b0"]`

### symbiote eye blink 13

- ID: `res---images-packed-vfx-event-symbiote-symbiote-eye-blink-13-png-z`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_eye_blink_13.png>z`

### symbiote eye blink 13.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-blink-13-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye_blink_13.png.import`

### symbiote eye.png

- ID: `images-packed-vfx-event-symbiote-symbiote-eye-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_eye.png.import`

### symbiote highlight

- ID: `ext-resource-type--texture2d--uid--uid---b3w0n7woyan3s--path--res---images-packed-vfx-event-symbiote-symbiote-highlight-png--id--4-78cly`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://b3w0n7woyan3s" path="res://images/packed/vfx/event/symbiote/symbiote_highlight.png" id="4_78cly"]`

### symbiote highlight

- ID: `res---images-packed-vfx-event-symbiote-symbiote-highlight-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/symbiote/symbiote_highlight.png`

### symbiote highlight.png

- ID: `images-packed-vfx-event-symbiote-symbiote-highlight-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/symbiote/symbiote_highlight.png.import`

### symbiote vfx

- ID: `res---scenes-vfx-events-symbiote-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/symbiote_vfx.tscn`

### symbiote vfx

- ID: `scenes-vfx-events-symbiote-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/symbiote_vfx.tscn`

### symbiote.png

- ID: `images-events-symbiote-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/symbiote.png.import`

### Tab Changed Event Handler

- ID: `megacrit-sts2-core-nodes-screens-settings-nsettingstabmanager-tabchangedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Settings.NSettingsTabManager+TabChangedEventHandler`

### tablet of truth

- ID: `res---images-events-tablet-of-truth-pngx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/tablet_of_truth.pngx`

### tablet of truth rock

- ID: `ext-resource-type--texture2d--uid--uid---cag8gcf06cqct--path--res---images-packed-vfx-event-tablet-of-truth-rock-png--id--3-qhcde`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cag8gcf06cqct" path="res://images/packed/vfx/event/tablet_of_truth_rock.png" id="3_qhcde"]`

### tablet of truth rock

- ID: `res---images-packed-vfx-event-tablet-of-truth-rock-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tablet_of_truth_rock.png`

### tablet of truth rock.png

- ID: `images-packed-vfx-event-tablet-of-truth-rock-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tablet_of_truth_rock.png.import`

### tablet of truth vfx

- ID: `res---scenes-vfx-events-tablet-of-truth-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/tablet_of_truth_vfx.tscn`

### tablet of truth vfx

- ID: `scenes-vfx-events-tablet-of-truth-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/tablet_of_truth_vfx.tscn`

### tablet of truth.png

- ID: `images-events-tablet-of-truth-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/tablet_of_truth.png.import`

### TabletOfTruth

- ID: `res---src-core-models-events-tabletoftruth-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TabletOfTruth.cs`

### tanx

- ID: `res---scenes-events-background-scenes-tanx-tscnu`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/tanx.tscnu`

### Tanx

- ID: `res---src-core-models-events-tanx-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Tanx.cs`

### tanx

- ID: `scenes-events-background-scenes-tanx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/tanx.tscn`

### tanx smoke

- ID: `ext-resource-type--texture2d--uid--uid---d3jsr4iu3se0x--path--res---images-packed-vfx-event-tanx-smoke-png--id--9-c2shc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://d3jsr4iu3se0x" path="res://images/packed/vfx/event/tanx_smoke.png" id="9_c2shc"]`

### tanx smoke

- ID: `res---images-packed-vfx-event-tanx-smoke-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tanx_smoke.png`

### tanx smoke.png

- ID: `images-packed-vfx-event-tanx-smoke-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tanx_smoke.png.import`

### Targeting Began Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-targetingbeganeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+TargetingBeganEventHandler`

### Targeting Ended Event Handler

- ID: `megacrit-sts2-core-nodes-combat-ntargetmanager-targetingendedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NTargetManager+TargetingEndedEventHandler`

### tea master

- ID: `res---images-events-tea-master-png4g`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/tea_master.png4g`

### tea master 0

- ID: `ext-resource-type--texture2d--uid--uid---b5apakovln1wp--path--res---images-packed-vfx-event-tea-master-tea-master-0-png--id--2-n22il`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://b5apakovln1wp" path="res://images/packed/vfx/event/tea_master/tea_master_0.png" id="2_n22il"]`

### tea master 0

- ID: `res---images-packed-vfx-event-tea-master-tea-master-0-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_0.png`

### tea master 0.png

- ID: `images-packed-vfx-event-tea-master-tea-master-0-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_0.png.import`

### tea master 01

- ID: `ext-resource-type--texture2d--uid--uid---raaso5gmqc1o--path--res---images-packed-vfx-event-tea-master-tea-master-01-png--id--3-erxwo`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://raaso5gmqc1o" path="res://images/packed/vfx/event/tea_master/tea_master_01.png" id="3_erxwo"]`

### tea master 01

- ID: `res---images-packed-vfx-event-tea-master-tea-master-01-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_01.png`

### tea master 01.png

- ID: `images-packed-vfx-event-tea-master-tea-master-01-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_01.png.import`

### tea master 02

- ID: `ext-resource-type--texture2d--uid--uid---qm5sutn5kd8s--path--res---images-packed-vfx-event-tea-master-tea-master-02-png--id--4-y6anw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://qm5sutn5kd8s" path="res://images/packed/vfx/event/tea_master/tea_master_02.png" id="4_y6anw"]`

### tea master 02

- ID: `res---images-packed-vfx-event-tea-master-tea-master-02-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_02.png`

### tea master 02.png

- ID: `images-packed-vfx-event-tea-master-tea-master-02-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_02.png.import`

### tea master 03

- ID: `ext-resource-type--texture2d--uid--uid---c8fkwnfm4pf5v--path--res---images-packed-vfx-event-tea-master-tea-master-03-png--id--5-dokk8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8fkwnfm4pf5v" path="res://images/packed/vfx/event/tea_master/tea_master_03.png" id="5_dokk8"]`

### tea master 03

- ID: `res---images-packed-vfx-event-tea-master-tea-master-03-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_03.png`

### tea master 03.png

- ID: `images-packed-vfx-event-tea-master-tea-master-03-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_03.png.import`

### tea master 04

- ID: `ext-resource-type--texture2d--uid--uid---wgqxqgmmueia--path--res---images-packed-vfx-event-tea-master-tea-master-04-png--id--6-1kbde`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://wgqxqgmmueia" path="res://images/packed/vfx/event/tea_master/tea_master_04.png" id="6_1kbde"]`

### tea master 04

- ID: `res---images-packed-vfx-event-tea-master-tea-master-04-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_04.png`

### tea master 04.png

- ID: `images-packed-vfx-event-tea-master-tea-master-04-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_04.png.import`

### tea master 05

- ID: `ext-resource-type--texture2d--uid--uid---bfwyhfdw6afik--path--res---images-packed-vfx-event-tea-master-tea-master-05-png--id--7-0gwjw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bfwyhfdw6afik" path="res://images/packed/vfx/event/tea_master/tea_master_05.png" id="7_0gwjw"]`

### tea master 05

- ID: `res---images-packed-vfx-event-tea-master-tea-master-05-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_05.png`

### tea master 05.png

- ID: `images-packed-vfx-event-tea-master-tea-master-05-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_05.png.import`

### tea master 06

- ID: `ext-resource-type--texture2d--uid--uid---2a3ir3lvjjw0--path--res---images-packed-vfx-event-tea-master-tea-master-06-png--id--8-4ob2e`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://2a3ir3lvjjw0" path="res://images/packed/vfx/event/tea_master/tea_master_06.png" id="8_4ob2e"]`

### tea master 06

- ID: `res---images-packed-vfx-event-tea-master-tea-master-06-pngjr`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_06.pngJr}`

### tea master 06.png

- ID: `images-packed-vfx-event-tea-master-tea-master-06-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_06.png.import`

### tea master 07

- ID: `ext-resource-type--texture2d--uid--uid---cgimoiae1kpxq--path--res---images-packed-vfx-event-tea-master-tea-master-07-png--id--9-6elx3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cgimoiae1kpxq" path="res://images/packed/vfx/event/tea_master/tea_master_07.png" id="9_6elx3"]`

### tea master 07

- ID: `res---images-packed-vfx-event-tea-master-tea-master-07-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_07.png`

### tea master 07.png

- ID: `images-packed-vfx-event-tea-master-tea-master-07-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_07.png.import`

### tea master 08

- ID: `ext-resource-type--texture2d--uid--uid---btbotnf3xvrkd--path--res---images-packed-vfx-event-tea-master-tea-master-08-png--id--10-t8tu5`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://btbotnf3xvrkd" path="res://images/packed/vfx/event/tea_master/tea_master_08.png" id="10_t8tu5"]`

### tea master 08

- ID: `res---images-packed-vfx-event-tea-master-tea-master-08-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_08.png`

### tea master 08.png

- ID: `images-packed-vfx-event-tea-master-tea-master-08-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_08.png.import`

### tea master 09

- ID: `ext-resource-type--texture2d--uid--uid---f31ftkrcdrj7--path--res---images-packed-vfx-event-tea-master-tea-master-09-png--id--11-sbk68`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://f31ftkrcdrj7" path="res://images/packed/vfx/event/tea_master/tea_master_09.png" id="11_sbk68"]`

### tea master 09

- ID: `res---images-packed-vfx-event-tea-master-tea-master-09-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/tea_master/tea_master_09.png$`

### tea master 09.png

- ID: `images-packed-vfx-event-tea-master-tea-master-09-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/tea_master/tea_master_09.png.import`

### tea master vfx

- ID: `res---scenes-vfx-events-tea-master-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/tea_master_vfx.tscn`

### tea master vfx

- ID: `scenes-vfx-events-tea-master-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/tea_master_vfx.tscn`

### tea master.png

- ID: `images-events-tea-master-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/tea_master.png.import`

### TeaMaster

- ID: `res---src-core-models-events-teamaster-csc2h`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TeaMaster.csc2h`

### tezcatara

- ID: `res---scenes-events-background-scenes-tezcatara-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/tezcatara.tscn`

### Tezcatara

- ID: `res---src-core-models-events-tezcatara-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Tezcatara.cs`

### tezcatara

- ID: `scenes-events-background-scenes-tezcatara-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/tezcatara.tscn`

### the architect event encounter background

- ID: `res---scenes-backgrounds-the-architect-event-encounter-the-architect-event-encounter-background-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/backgrounds/the_architect_event_encounter/the_architect_event_encounter_background.tscn<`

### the architect event encounter background

- ID: `scenes-backgrounds-the-architect-event-encounter-the-architect-event-encounter-background-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/backgrounds/the_architect_event_encounter/the_architect_event_encounter_background.tscn`

### the legends were true

- ID: `res---images-events-the-legends-were-true-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/the_legends_were_true.png`

### the legends were true vfx

- ID: `res---scenes-vfx-events-the-legends-were-true-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/the_legends_were_true_vfx.tscn~`

### the legends were true vfx

- ID: `scenes-vfx-events-the-legends-were-true-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/the_legends_were_true_vfx.tscn`

### the legends were true.png

- ID: `images-events-the-legends-were-true-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/the_legends_were_true.png.import`

### TheArchitect.cs

- ID: `res---src-core-models-events-thearchitect-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TheArchitect.cs.`

### TheArchitectEventEncounter

- ID: `res---src-core-models-encounters-thearchitecteventencounter-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Encounters/TheArchitectEventEncounter.cs`

### TheLanternKey

- ID: `res---src-core-models-events-thelanternkey-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TheLanternKey.cs`

### TheLegendsWereTrue

- ID: `res---src-core-models-events-thelegendsweretrue-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TheLegendsWereTrue.cs`

### this or that

- ID: `res---images-events-this-or-that-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/this_or_that.png`

### this or that vfx

- ID: `res---scenes-vfx-events-this-or-that-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/this_or_that_vfx.tscn`

### this or that vfx

- ID: `scenes-vfx-events-this-or-that-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/this_or_that_vfx.tscn`

### this or that.png

- ID: `images-events-this-or-that-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/this_or_that.png.import`

### ThisOrThat

- ID: `res---src-core-models-events-thisorthat-csv`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/ThisOrThat.csV`

### tinker time

- ID: `res---images-events-tinker-time-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/tinker_time.png`

### tinker time.png

- ID: `images-events-tinker-time-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/tinker_time.png.import`

### TinkerTime

- ID: `res---src-core-models-events-tinkertime-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TinkerTime.cs`

### Toggled Event Handler

- ID: `megacrit-sts2-core-nodes-combat-npeekbutton-toggledeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Combat.NPeekButton+ToggledEventHandler`

### Toggled Event Handler

- ID: `megacrit-sts2-core-nodes-commonui-ntickbox-toggledeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.CommonUi.NTickbox+ToggledEventHandler`

### toric toughness

- ID: `filename----event-toric-toughness-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/toric_toughness.png",`

### trash heap

- ID: `res---images-events-trash-heap-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/trash_heap.png`

### trash heap.png

- ID: `images-events-trash-heap-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/trash_heap.png.import`

### TrashHeap

- ID: `res---src-core-models-events-trashheap-cs-z-t`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/TrashHeap.cs:Z"T`

### trial

- ID: `res---images-events-trial-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/trial.png`

### Trial

- ID: `res---src-core-models-events-trial-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Trial.cs`

### trial light beam

- ID: `ext-resource-type--texture2d--uid--uid---bhkcgels5hnh0--path--res---images-packed-vfx-event-trial-light-beam-png--id--2-1lmx6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bhkcgels5hnh0" path="res://images/packed/vfx/event/trial_light_beam.png" id="2_1lmx6"]`

### trial light beam

- ID: `res---images-packed-vfx-event-trial-light-beam-pngyc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_light_beam.pngyC`

### trial light beam.png

- ID: `images-packed-vfx-event-trial-light-beam-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_light_beam.png.import`

### trial merchant

- ID: `ext-resource-type--texture2d--uid--uid---bs1b0ky7i7y2d--path--res---images-packed-vfx-event-trial-merchant-png--id--2-qdgtc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bs1b0ky7i7y2d" path="res://images/packed/vfx/event/trial_merchant.png" id="2_qdgtc"]`

### trial merchant

- ID: `res---images-packed-vfx-event-trial-merchant-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_merchant.png`

### trial merchant vfx

- ID: `res---scenes-vfx-events-trial-merchant-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/trial_merchant_vfx.tscn`

### trial merchant vfx

- ID: `scenes-vfx-events-trial-merchant-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/trial_merchant_vfx.tscn`

### trial merchant.png

- ID: `images-packed-vfx-event-trial-merchant-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_merchant.png.import`

### trial noble

- ID: `ext-resource-type--texture2d--uid--uid---31vjdpwoeahl--path--res---images-packed-vfx-event-trial-noble-png--id--2-opa4o`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://31vjdpwoeahl" path="res://images/packed/vfx/event/trial_noble.png" id="2_opa4o"]`

### trial noble

- ID: `res---images-packed-vfx-event-trial-noble-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_noble.png`

### trial noble vfx

- ID: `res---scenes-vfx-events-trial-noble-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/trial_noble_vfx.tscn`

### trial noble vfx

- ID: `scenes-vfx-events-trial-noble-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/trial_noble_vfx.tscn`

### trial noble.png

- ID: `images-packed-vfx-event-trial-noble-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_noble.png.import`

### trial nondescript

- ID: `ext-resource-type--texture2d--uid--uid---bvry4s3ytgxee--path--res---images-packed-vfx-event-trial-nondescript-png--id--2-2rdmw`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://bvry4s3ytgxee" path="res://images/packed/vfx/event/trial_nondescript.png" id="2_2rdmw"]`

### trial nondescript

- ID: `res---images-packed-vfx-event-trial-nondescript-png4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_nondescript.png4`

### trial nondescript vfx

- ID: `res---scenes-vfx-events-trial-nondescript-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/trial_nondescript_vfx.tscn`

### trial nondescript vfx

- ID: `scenes-vfx-events-trial-nondescript-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/trial_nondescript_vfx.tscn`

### trial nondescript.png

- ID: `images-packed-vfx-event-trial-nondescript-png-importp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_nondescript.png.importp`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--2-1kjdf`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="2_1kjdf"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--2-nwdma`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="2_nwdma"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--4-ixsua`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="4_ixsua"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--4-uljtm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="4_uljtm"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--4-xqskc`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="4_xqskc"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--40-3se23`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="40_3se23"]`

### trial stand light

- ID: `ext-resource-type--texture2d--uid--uid---c8cducyphvfqi--path--res---images-packed-vfx-event-trial-stand-light-png--id--5-4oeou`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://c8cducyphvfqi" path="res://images/packed/vfx/event/trial_stand_light.png" id="5_4oeou"]`

### trial stand light

- ID: `res---images-packed-vfx-event-trial-stand-light-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_stand_light.png`

### trial stand light.png

- ID: `images-packed-vfx-event-trial-stand-light-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_stand_light.png.import`

### trial started

- ID: `res---images-events-trial-started-png-z9k`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/trial_started.png;Z9k`

### trial started.png

- ID: `images-events-trial-started-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/trial_started.png.import`

### trial top layer

- ID: `ext-resource-type--texture2d--uid--uid---d2yh4xnlundqp--path--res---images-packed-vfx-event-trial-top-layer-png--id--3-fp2ga`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://d2yh4xnlundqp" path="res://images/packed/vfx/event/trial_top_layer.png" id="3_fp2ga"]`

### trial top layer

- ID: `res---images-packed-vfx-event-trial-top-layer-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/trial_top_layer.png&`

### trial top layer.png

- ID: `images-packed-vfx-event-trial-top-layer-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/trial_top_layer.png.import`

### trial vfx

- ID: `res---scenes-vfx-events-trial-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/trial_vfx.tscn`

### trial vfx

- ID: `scenes-vfx-events-trial-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/trial_vfx.tscn`

### trial.png

- ID: `images-events-trial-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/trial.png.import`

### Try Add Event

- ID: `tryaddevent`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Unfocused Event Handler

- ID: `megacrit-sts2-core-nodes-godotextensions-nclickablecontrol-unfocusedeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl+UnfocusedEventHandler`

### Unhovered Event Handler

- ID: `megacrit-sts2-core-nodes-screens-runhistoryscreen-ndeckhistory-unhoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen.NDeckHistory+UnhoveredEventHandler`

### Unhovered Event Handler

- ID: `megacrit-sts2-core-nodes-screens-shops-nmerchantslot-unhoveredeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantSlot+UnhoveredEventHandler`

### Unlock Events

- ID: `unlockevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### unrest site

- ID: `res---images-events-unrest-site-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/unrest_site.png`

### unrest site vfx

- ID: `res---scenes-vfx-events-unrest-site-vfx-tscn-m`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/unrest_site_vfx.tscn;m`

### unrest site vfx

- ID: `scenes-vfx-events-unrest-site-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/unrest_site_vfx.tscn`

### unrest site.png

- ID: `images-events-unrest-site-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/unrest_site.png.import`

### UnrestSite

- ID: `res---src-core-models-events-unrestsite-cs-g`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/UnrestSite.cs`g`

### Unsubscribe From Events

- ID: `unsubscribefromevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Unsubscribe From Model Events

- ID: `unsubscribefrommodelevents`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Vakuu

- ID: `res---src-core-models-events-vakuu-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Vakuu.cs`

### vakuu

- ID: `scenes-events-background-scenes-vakuu-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/events/background_scenes/vakuu.tscn`

### vakuu.tscn

- ID: `res---scenes-events-background-scenes-vakuu-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/events/background_scenes/vakuu.tscn.`

### vfx dense vegetation bug 00

- ID: `ext-resource-type--texture2d--uid--uid---dtbj54b6o2n7e--path--res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-00-png--id--5-fwmva`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://dtbj54b6o2n7e" path="res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_00.png" id="5_fwmva"]`

### vfx dense vegetation bug 00

- ID: `res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-00-png53`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_00.png53`

### vfx dense vegetation bug 00.png

- ID: `images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-00-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_00.png.import`

### vfx dense vegetation bug 02

- ID: `ext-resource-type--texture2d--uid--uid---cha7ksnd15fdl--path--res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-02-png--id--6-15l1s`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cha7ksnd15fdl" path="res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_02.png" id="6_15l1s"]`

### vfx dense vegetation bug 02

- ID: `res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-02-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_02.png`

### vfx dense vegetation bug 02.png

- ID: `images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-02-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_02.png.import`

### vfx dense vegetation bug 03

- ID: `ext-resource-type--texture2d--uid--uid---x08op3pocvek--path--res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-03-png--id--7-21i2k`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://x08op3pocvek" path="res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_03.png" id="7_21i2k"]`

### vfx dense vegetation bug 03

- ID: `res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-03-png8`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_03.png8`

### vfx dense vegetation bug 03.png

- ID: `images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-03-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_03.png.import`

### vfx dense vegetation bug 04

- ID: `ext-resource-type--texture2d--uid--uid---cv1uaf4ha613a--path--res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-04-png--id--8-4cqao`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://cv1uaf4ha613a" path="res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_04.png" id="8_4cqao"]`

### vfx dense vegetation bug 04

- ID: `res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-04-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_04.png>`

### vfx dense vegetation bug 04.png

- ID: `images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-04-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_04.png.import`

### vfx dense vegetation bug 05

- ID: `ext-resource-type--texture2d--uid--uid---r1uqktrahlhm--path--res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-05-png--id--9-37tn6`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `[ext_resource type="Texture2D" uid="uid://r1uqktrahlhm" path="res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_05.png" id="9_37tn6"]`

### vfx dense vegetation bug 05

- ID: `res---images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-05-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_05.png`

### vfx dense vegetation bug 05.png

- ID: `images-packed-vfx-event-dense-vegetation-vfx-dense-vegetation-bug-05-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/packed/vfx/event/dense_vegetation/vfx_dense_vegetation_bug_05.png.import`

### visited Event Ids

- ID: `visitedeventids`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Voted For Shared Event Option Message

- ID: `megacrit-sts2-core-multiplayer-messages-game-sync-votedforsharedeventoptionmessage`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Sync.VotedForSharedEventOptionMessage`

### VotedForSharedEventOptionMessage

- ID: `res---src-core-multiplayer-messages-game-sync-votedforsharedeventoptionmessage-cso`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Multiplayer/Messages/Game/Sync/VotedForSharedEventOptionMessage.cso`

### Wait For Event Options

- ID: `waitforeventoptions`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### Wait For Event Room

- ID: `waitforeventroom`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### war historian repy

- ID: `res---images-events-war-historian-repy-png7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/war_historian_repy.png7`

### war historian repy vfx

- ID: `res---scenes-vfx-events-war-historian-repy-vfx-tscnv`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/war_historian_repy_vfx.tscnV`

### war historian repy vfx

- ID: `scenes-vfx-events-war-historian-repy-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/war_historian_repy_vfx.tscn`

### war historian repy.png

- ID: `images-events-war-historian-repy-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/war_historian_repy.png.import`

### WarHistorianRepy

- ID: `res---src-core-models-events-warhistorianrepy-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/WarHistorianRepy.cs`

### was Removal Prevented

- ID: `wasremovalprevented`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`

### waterlogged scriptorium

- ID: `res---images-events-waterlogged-scriptorium-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/waterlogged_scriptorium.png`

### waterlogged scriptorium vfx

- ID: `res---scenes-vfx-events-waterlogged-scriptorium-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/waterlogged_scriptorium_vfx.tscn`

### waterlogged scriptorium vfx

- ID: `scenes-vfx-events-waterlogged-scriptorium-vfx-tscn`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/waterlogged_scriptorium_vfx.tscn`

### waterlogged scriptorium.png

- ID: `images-events-waterlogged-scriptorium-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/waterlogged_scriptorium.png.import@`

### WaterloggedScriptorium

- ID: `res---src-core-models-events-waterloggedscriptorium-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/WaterloggedScriptorium.cs`

### welcome to wongos

- ID: `res---images-events-welcome-to-wongos-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/welcome_to_wongos.png`

### welcome to wongos.png

- ID: `images-events-welcome-to-wongos-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/welcome_to_wongos.png.import`

### WelcomeToWongos

- ID: `res---src-core-models-events-welcometowongos-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/WelcomeToWongos.cs`

### wellspring

- ID: `res---images-events-wellspring-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/wellspring.png`

### Wellspring

- ID: `res---src-core-models-events-wellspring-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/Wellspring.cs`

### wellspring.png

- ID: `images-events-wellspring-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/wellspring.png.import`

### whispering hollow

- ID: `res---images-events-whispering-hollow-pngx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/whispering_hollow.pngX`

### whispering hollow vfx

- ID: `res---scenes-vfx-events-whispering-hollow-vfx-tscn3`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://scenes/vfx/events/whispering_hollow_vfx.tscn3`

### whispering hollow vfx

- ID: `scenes-vfx-events-whispering-hollow-vfx-tscn0`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `scenes/vfx/events/whispering_hollow_vfx.tscn0`

### whispering hollow.png

- ID: `images-events-whispering-hollow-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/whispering_hollow.png.import`

### WhisperingHollow

- ID: `res---src-core-models-events-whisperinghollow-csa`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/WhisperingHollow.csa`

### whistle

- ID: `filename----event-whistle-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `"filename": "event/whistle.png",`

### Window Change Event Handler

- ID: `megacrit-sts2-core-nodes-ngame-windowchangeeventhandler`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `assembly-scan`
- 모델 클래스: `MegaCrit.Sts2.Core.Nodes.NGame+WindowChangeEventHandler`

### wood carvings

- ID: `res---images-events-wood-carvings-png-v`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/wood_carvings.png v`

### wood carvings.png

- ID: `images-events-wood-carvings-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/wood_carvings.png.import`

### WoodCarvings

- ID: `res---src-core-models-events-woodcarvings-cs-zm`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/WoodCarvings.cs%ZM`

### Y

- ID: `res---src-core-events-custom-crystalsphereevent-crystalsphereminigame-csk-y`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Events/Custom/CrystalSphereEvent/CrystalSphereMinigame.csk/Y`

### zen weaver

- ID: `res---images-events-zen-weaver-pngp`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://images/events/zen_weaver.pngP`

### zen weaver.png

- ID: `images-events-zen-weaver-png-import`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `images/events/zen_weaver.png.import`

### ZenWeaver

- ID: `res---src-core-models-events-zenweaver-cs`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 모델 클래스, 리소스 경로, 관찰 힌트 수준의 구조 정보만 연결된 상태입니다.
- 핵심 설명: 구조 정보는 확인되었지만, 플레이 효과를 설명할 본문은 아직 확보되지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `pck-inventory`
- 리소스 경로: `res://src/Core/Models/Events/ZenWeaver.cs`

### 가짜 죽음 방지

- ID: `megacrit-sts2-core-models-powers-mocks-mockpreventdeathpower`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MOCK_PREVENT_DEATH_POWER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Powers.Mocks.MockPreventDeathPower`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 권투형 구조체들

- ID: `megacrit-sts2-core-models-encounters-punchoffeventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PUNCH_OFF_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.PunchOffEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 꿈틀벌레들

- ID: `megacrit-sts2-core-models-encounters-densevegetationeventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DENSE_VEGETATION_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.DenseVegetationEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 노누파이페

- ID: `megacrit-sts2-core-models-events-nonupeipe`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NONUPEIPE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Nonupeipe`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Nonupeipe

### 니오우

- ID: `megacrit-sts2-core-models-events-neow`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `NEOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Neow`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Neow

### 다브

- ID: `megacrit-sts2-core-models-events-darv`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DARV`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Darv`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Darv

### 바쿠

- ID: `megacrit-sts2-core-models-events-vakuu`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `VAKUU`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Vakuu`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Vakuu

### 상?인

- ID: `megacrit-sts2-core-models-encounters-fakemerchanteventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FAKE_MERCHANT_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.FakeMerchantEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 수수께끼의 기사

- ID: `megacrit-sts2-core-models-encounters-mysteriousknighteventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MYSTERIOUS_KNIGHT_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.MysteriousKnightEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 아키텍트

- ID: `megacrit-sts2-core-models-encounters-thearchitecteventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_ARCHITECT_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.TheArchitectEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 아키텍트

- ID: `megacrit-sts2-core-models-events-thearchitect`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_ARCHITECT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheArchitect`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이벤트

- ID: `res---images-ui-run-history-event-png`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `EVENT`
- 리소스 경로: `res://images/ui/run_history/event.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전투로 손상된 훈련 인형

- ID: `megacrit-sts2-core-models-encounters-battleworndummyeventencounter`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BATTLEWORN_DUMMY_EVENT_ENCOUNTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Encounters.BattlewornDummyEventEncounter`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 탄스

- ID: `megacrit-sts2-core-models-events-tanx`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TANX`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Tanx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Tanx

### 테즈카타라

- ID: `megacrit-sts2-core-models-events-tezcatara`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TEZCATARA`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Tezcatara`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Tezcatara

### 파엘

- ID: `megacrit-sts2-core-models-events-pael`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 이름은 L10N과 연결되었지만, 설명 본문 확보가 더 필요합니다.
- 핵심 설명: 이름과 L10N 키는 연결되었지만, 효과 설명 본문은 아직 비어 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PAEL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Pael`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`
- 영어 fallback: Pael

