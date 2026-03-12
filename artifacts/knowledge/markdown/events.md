# 이벤트

- 전체 항목 수: 77
- 설명 본문이 채워진 항목: 57
- L10N 키 또는 제목이 연결된 항목: 58
- 선택지/옵션 정보가 있는 항목: 67

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
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 13
- 확인된 선택지:
  - 개선: 매 전투 종료 시, 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 도구: 파워 카드를 만듭니다.
  - 무기: 공격 카드를 만듭니다.
  - 보호대: 스킬 카드를 만듭니다.
  - 수락한다: 커스텀 카드를 1장 생성해 [gold]덱[/gold]에 추가합니다.
  - 전문성: [gold]힘[/gold]을 [blue]{ExpertiseStrength}[/blue] 얻습니다. [gold]민첩[/gold]을 [blue]{ExpertiseDexterity}[/blue] 얻습니다.
  - 지혜: 카드를 [blue]{WisdomCards}[/blue]장 뽑습니다.
  - 질식: 이번 턴에 카드를 사용할 때마다, 대상 적이 체력을 [blue]{ChokingDamage}[/blue] 잃습니다.
- 페이지 요약: CHOOSE_CARD_TYPE: “어떤 도구가 [red][jitter]전부 죽여버리는[/jitter][/red] 데에 도움이 될까요?” | CHOOSE_RIDER: “훌륭한 선택이군요! 그럼 이걸로 [sine]뭘[/sine] 해야 할까요?” [orange]과학자[/orange]는 기쁨에 찬 표정으로 두 손을 비벼댑니다. | DONE: “휴! 끝났어요. 이게 당신 거예요! 이제 나가서 마음껏 [red][jitter]학살해보세요[/jitter][/red]!!” 당신은 몰랐겠지만 그 [orange]과학자[/orange]는 이후에 수백 가지의 무기를 만들어냈으며, 그 결과 첨탑 안에서 수천의 목숨이 사라졌습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TINKER_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TinkerTime`
- 리소스 경로: `res://images/events/tinker_time.png`
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
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 3
- 확인된 선택지:
  - 떠난다
  - 매콤 톡톡을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 카드를 1장 [gold]강화[/gold]합니다. 만찬을 이어갑니다!
  - 불량 해초 샐러드를 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. [gold]덱[/gold]에 [gold]광란의 포식[/gold]을 추가합니다. 만찬을 이어갑니다!
  - 빈털터리: 더 먹을 수는 있지만, [gold]골드[/gold]가 없습니다.
  - 수상한 조미료를 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 [gold]포션[/gold]을 1개 생성합니다. 만찬을 이어갑니다!
  - 요리사를 관찰한다: [gold]덱[/gold]의 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 장어 튀김을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. [gold]덱[/gold]에 무작위 무색 카드를 1장 추가합니다. 만찬을 이어갑니다!
  - 젤리 간을 집는다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 카드를 1장 [gold]변화[/gold]시킵니다. 만찬을 이어갑니다!
- 페이지 요약: GRAB_SOMETHING_OFF_THE_BELT: 당신은 [gold]{LastDishTitle}[/gold] 접시를 가져온 뒤, 허겁지겁 먹어치웠습니다. [green]맛있네요![/green] | INITIAL: 당신은 한 오두막집에 들어섭니다. 밝지만 뒤틀린 표지판에는 다음과 같은 내용이 적혀 있습니다: [aqua]“끝없는 만찬 - 배고픈 만큼 계산하세요!”[/aqua] 오두막의 안쪽에는, [orange]가늘고 길쭉한 팔을 여럿 드리우고 있는 요리사[/orange]가 능숙한 솜씨로 [green]한 입 크기의 음식[/green]을 준비해, [sine]구불구불한 키틴질 벨트[/sine] 위에 올려놓고 있습니다. 요리사의 팔 중 하나가 표지판을 가리킵니다: 접시당 [blue]35[/blue] [gold]골드[/gold] | LEAVE: 충분히 배를 채운 당신은, 다시 발걸음을 옮깁니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ENDLESS_CONVEYOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.EndlessConveyor`
- 리소스 경로: `res://images/events/endless_conveyor.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 미끄러운 다리

- ID: `megacrit-sts2-core-models-events-slipperybridge`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 무너져 내릴 듯한 나무 다리를 건너던 도중, [blue][jitter]갑작스럽게 폭우[/jitter][/blue]가 쏟아집니다. [purple][sine]거대한 돌풍[/sine][/purple]이, 마치 당신의 여정을 끝내려는 듯이 위협적으로 휘몰아칩니다.
- 구조 정보:
  - 페이지 키 수: 1
  - 선택지 키 수: 2
- 확인된 선택지:
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
  - 버틴다: 체력을 [red]{HpLoss}[/red] 잃습니다. 상단 선택지의 카드가 무작위로 변경됩니다.
- 페이지 요약: HOLD_ON_0: [jitter]당신은 버티고 있습니다![/jitter] | HOLD_ON_1: 계속... 버티고... [jitter]있습니다아아!!!![/jitter] | HOLD_ON_2: [sine]아아아아아!?!!![/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SLIPPERY_BRIDGE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SlipperyBridge`
- 리소스 경로: `res://images/events/slippery_bridge.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 재판

- ID: `megacrit-sts2-core-models-events-trial`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 거대한 건물로 들어가는 사람들의 줄에 합류합니다.

[gold]황금빛 아치[/gold]를 지나자 나팔 소리가 요란하게 울려 퍼지고 [jitter]색종이가 터져 나오며[/jitter], 천장에서는 [sine]장식용 리본이 흩날립니다[/sine]!

“참가 번호 [blue]{EntrantNumber}[/blue]번, 오늘 [gold]재판[/gold]의 [green]결정자[/green]는 당신입니다.”
- 구조 정보:
  - 페이지 키 수: 10
  - 선택지 키 수: 10
- 확인된 선택지:
  - 거절한다: [red]거절할 수 없습니다.[/red]
  - 수락한다: 오늘의 결정자 역할을 맡습니다.
  - 수락한다: 굴복합니다. 오늘의 결정자 역할을 맡습니다.
  - 완강하게 거절한다: [red]치명적인 결과를 맞이합니다.[/red]
  - 판결: 무죄: [gold]덱[/gold]에 [red]수치[/red]를 추가합니다. 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다.
  - 판결: 무죄: [gold]덱[/gold]에 [red]후회[/red]를 추가합니다. [gold]골드[/gold]를 [blue]300[/blue] 얻습니다.
  - 판결: 무죄: [gold]덱[/gold]에 [red]의심[/red]을 추가합니다. 카드를 [blue]2[/blue]장 [gold]변화[/gold]시킵니다.
  - 판결: 유죄: [gold]덱[/gold]에 [red]후회[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
- 페이지 요약: INITIAL: 당신은 거대한 건물로 들어가는 사람들의 줄에 합류합니다. [gold]황금빛 아치[/gold]를 지나자 나팔 소리가 요란하게 울려 퍼지고 [jitter]색종이가 터져 나오며[/jitter], 천장에서는 [sine]장식용 리본이 흩날립니다[/sine]! “참가 번호 [blue]{EntrantNumber}[/blue]번, 오늘 [gold]재판[/gold]의 [green]결정자[/green]는 당신입니다.” | MERCHANT: [green]부유해 보이는 상인[/green]이 사건의 경위를 진술합니다. 그는 경쟁자 중 한 명에게 [red]살인[/red] 혐의로 기소되었습니다. 증거는 빈약하고, 당신은 그가 결백하다는 듯한 느낌을 받았지만, 방청객들은 피의 심판을 원하고 있는 것으로 보입니다. | MERCHANT_GUILTY: 당신은 그 남자의 소유물을 모두 압수해 재판 소송 비용으로 쓰도록 판결합니다. 방청객들은 환호의 함성을 지릅니다!
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRIAL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Trial`
- 리소스 경로: `res://images/events/trial.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 웡고스에 오신 것을 환영합니다

- ID: `megacrit-sts2-core-models-events-welcometowongos`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: “웡고스에 오신 걸 환영합니다. 원하시는 상품을 웡타스틱한 가격으로 제공합니다.” 당신이 여태 만난 점원 중 가장 무기력한 점원이 말합니다.

“상품들을 둘러보시고 웡고스에서의 시간을 즐겨주세요.” 그는 고개조차 들지 않은 채 말을 이어갑니다.
- 구조 정보:
  - 페이지 키 수: 4
  - 선택지 키 수: 7
- 확인된 선택지:
  - 떠난다: 무작위 카드 1장이 [red]열화[/red]됩니다.
  - 웡고스 랜덤 박스: [gold]골드[/gold]를 [red]{MysteryBoxCost}[/red] 지불합니다. [blue]{MysteryBoxCombatCount}[/blue]번의 전투 후에 무작위 [gold]유물[/gold]을 [blue]{MysteryBoxRelicCount}[/blue]개 얻습니다.
  - 웡고스 추천 상품: [gold]골드[/gold]를 [red]{FeaturedItemCost}[/red] 지불합니다. [gold]{RandomRelic}[/gold]을(를) 얻습니다.
  - 웡고스 할인 코너: [gold]골드[/gold]를 [red]{BargainBinCost}[/red] 지불합니다. 무작위 [gold]일반 유물[/gold]을 [blue]1[/blue]개 얻습니다.
  - 잠김: [gold]골드[/gold]가 [blue]{BargainBinCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{FeaturedItemCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{MysteryBoxCost}[/blue] 필요합니다.
- 페이지 요약: AFTER_BUY: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. [blue]{RemainingWongoPointAmount}[/blue]포인트를 더 모으시면, 저희 점포의 특별한 [gold]웡고스 고객 감사 배지[/gold]를 받으실 수 있습니다. 웡타스틱한 하루 되세요.” 점원이 무슨 얘기를 하고 있는지는 모르겠지만, 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다. | AFTER_BUY_BADGE_COUNTER: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. [blue]{RemainingWongoPointAmount}[/blue]포인트를 더 모으시면, 저희 점포의 특별한 [gold]웡고스 고객 감사 배지[/gold]를 받으실 수 있습니다. 웡타스틱한 하루 되세요.” 당신은 이미 이 쓸모없는 배지를 [blue]{TotalWongoBadgeAmount}[/blue]개나 받았습니다. 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다. | AFTER_BUY_RECEIVE_BADGE: “고객님의 후한 구매에 웡고스럽게 감탄했습니다... 존경하는 고객님, 고객님이 현재 지니고 계신 [gold]웡고스 포인트[/gold]는 [blue]{WongoPointAmount}[/blue]포인트입니다. 포인트 누적을 통해 [gold]웡고스 고객 감사 배지[/gold]가 수여되었습니다.” 점원은 조악하게 만들어진 배지 하나를 당신에게 건냅니다. “웡타스틱한 하루 되세요.” 현재까지 받으신 배지는 총 [blue]{TotalWongoBadgeAmount}[/blue]개입니다. 당신은 다시는 웡고스에 발을 들이지 않겠다고 다짐합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WELCOME_TO_WONGOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos`
- 리소스 경로: `res://images/events/welcome_to_wongos.png`
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
- 구조 정보:
  - 페이지 키 수: 4
  - 선택지 키 수: 7
- 확인된 선택지:
  - 나아간다: 인챈트할 수 있는 카드가 없습니다.
  - 뒷면을 읽는다: 공격 카드를 1장 선택해 [purple]{Enchantment1}[/purple]를 [blue]{Enchantment1Amount}[/blue] [gold]인챈트[/gold]합니다.
  - 무작위 문단을 읽는다: 스킬 카드를 1장 선택해 [purple]{Enchantment2}[/purple]을 [blue]{Enchantment2Amount}[/blue] [gold]인챈트[/gold]합니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 파워 카드가 없습니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 스킬 카드가 없습니다.
  - 잠김: [gold]인챈트[/gold]할 수 있는 공격 카드가 없습니다.
  - 책을 모두 읽는다: 파워 카드를 1장 선택해 [purple]{Enchantment3}[/purple]을 [blue]{Enchantment3Amount}[/blue] [gold]인챈트[/gold]합니다.
- 페이지 요약: INITIAL: 바닥에는 책 한 권이 놓여있습니다. “첨탑을 정복하기 위한 3가지 방법” 책의 표면은 [red]피로 뒤덮여[/red] 있었기 때문에, 굉장히 수상쩍은 느낌이 듭니다. 읽어보시겠습니까? | NO_OPTIONS: 그냥 책도 딱히 좋아하지도 않는데, 피로 뒤덮인 책이라면 말할 것도 없습니다. | READ_ENTIRE_BOOK: 정말 흥미진진한 읽을거리입니다! 인상적인 문구 몇 개가 여전히 머릿속에 선명하게 남아 있습니다: “거대한 몬스터는 피하세요.” “모든 카드를 집으세요.” “방어도는 겁쟁이나 쌓는 겁니다.” 당신의 두 손이 [red]피투성이[/red]가 됩니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SELF_HELP_BOOK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SelfHelpBook`
- 리소스 경로: `res://images/events/self_help_book.pnga`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 진리의 석판

- ID: `megacrit-sts2-core-models-events-tabletoftruth`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 두 명의 [blue]혈족 수호자[/blue]를 쓰러뜨리고, 그들이 지키고 있던 보관실 안으로 들어갑니다.

방 안에는 [purple][sine]익숙한 문양이 새겨진 석판[/sine][/purple]이 놓여 있습니다. 직감적으로, 이 언어를 그리 어렵지 않게 해독할 수 있을 것 같은 느낌이 듭니다... 하지만, 한 번 해독을 시작하면 멈출 수 없을 것입니다.

석판에서는 차분한 기운이 흘러나오고 있으며, 해독하는 대신 부숴 석판의 [green]치유의 기운[/green]을 얻을 수도 있습니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 계속 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 계속 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 모든 것을 잃는다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 모든 카드를 [gold]강화[/gold]합니다.
  - 부순다: 체력을 [green]{SmashHPGain}[/green] 회복합니다.
  - 포기한다: 글을 읽는 것을 멈추고 떠납니다.
  - 해독을 이어간다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 해독한다: 최대 체력을 [red]{DecipherMaxHpLoss}[/red] 잃습니다. 무작위 카드를 1장 [gold]강화[/gold]합니다.
- 페이지 요약: DECIPHER_1: 당신이 석판을 해독하기 시작하자, [blue]혈족의 만트라[/blue]가 당신에게 말을 걸어옵니다. “[gold]진리[/gold]는 모든 것이다...” | DECIPHER_2: 석판을 읽다 보니 어지러워지는 듯한 느낌이 듭니다! [purple][sine]영웅은 이렇게 오랫동안 글을 읽도록 만들어진 존재가 아닐 텐데...[/sine][/purple] | DECIPHER_3: [sine][red]당신은 간신히 버티고 있습니다...[/red][/sine] 석판의 해독이 거의 끝나갑니다. [blue]만트라[/blue]는 진리를 품고 있습니다… [jitter]진리!! 대체 진리가 뭐죠!?[/jitter]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TABLET_OF_TRUTH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TabletOfTruth`
- 리소스 경로: `res://images/events/tablet_of_truth.pngx`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거대한 꽃

- ID: `megacrit-sts2-core-models-events-colossalflower`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [red][jitter]산처럼 쌓인 뼈무더기[/jitter][/red] 꼭대기에, [green]거대한 꽃[/green] 하나가 자라나 있습니다.

[sine][rainbow freq=0.3 sat=0.8 val=1]색을 바꾸는 꽃잎들[/rainbow][/sine]의 맥동 사이 중심부에 [aqua]막대한 힘을 지닌 꽃가루 덩어리[/aqua]가 존재한다는 것이 느껴지지만, 요동치는 꽃잎들은 [red]아주 날카롭고[/red], 그 움직임을 도저히 예측할 수 없습니다.

[gold]황금빛 꿀[/gold]만 조금 집어갈 수도 있겠지만, 중심부에 있는 값진 보상에 손을 뻗고 싶다는 마음이 가라앉지 않습니다...
- 구조 정보:
  - 페이지 키 수: 4
  - 선택지 키 수: 2
- 확인된 선택지:
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize1}[/blue] 얻습니다.
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize2}[/blue] 얻습니다.
  - 꿀을 추출한다: [gold]골드[/gold]를 [blue]{Prize3}[/blue] 얻습니다.
  - 더 깊게 다가간다: 더 깊게 들어갑니다. 체력을 [red]5[/red] 잃습니다.
  - 더 깊게 다가간다: 훨씬 더 깊게 들어갑니다. 체력을 [red]6[/red] 잃습니다.
  - 중심부로 들어간다: 체력을 [red]7[/red] 잃습니다. [gold]꽃가루 핵[/gold]을 얻습니다.
- 페이지 요약: EXTRACT_CURRENT_PRIZE: 당신은 꽃의 표피에서 약간의 [gold]꿀[/gold]을 조심스럽게 채취합니다. 꽃잎들이 분노한 듯이 반짝거립니다. | EXTRACT_INSTEAD: 당신은 온몸의 감각을 잃고 [purple]독소[/purple]와 [red]가시[/red], [green]잎사귀[/green]들로 인해 엉망이 되었습니다. 이 정도면 충분한 것 같네요. 당신은 [gold]품질 좋은 꿀[/gold]을 챙겨 넣은 뒤, 서둘러 물러납니다. | INITIAL: [red][jitter]산처럼 쌓인 뼈무더기[/jitter][/red] 꼭대기에, [green]거대한 꽃[/green] 하나가 자라나 있습니다. [sine][rainbow freq=0.3 sat=0.8 val=1]색을 바꾸는 꽃잎들[/rainbow][/sine]의 맥동 사이 중심부에 [aqua]막대한 힘을 지닌 꽃가루 덩어리[/aqua]가 존재한다는 것이 느껴지지만, 요동치는 꽃잎들은 [red]아주 날카롭고[/red], 그 움직임을 도저히 예측할 수 없습니다. [gold]황금빛 꿀[/gold]만 조금 집어갈 수도 있겠지만, 중심부에 있는 값진 보상에 손을 뻗고 싶다는 마음이 가라앉지 않습니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLOSSAL_FLOWER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColossalFlower`
- 리소스 경로: `res://images/events/colossal_flower.png`
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
- 구조 정보:
  - 페이지 키 수: 1
- 확인된 선택지:
  - 분홍색: 네크로바인더 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 빨간색: 아이언클래드 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 주황색: 리젠트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 초록색: 사일런트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 파란색: 디펙트 카드를 [blue]{Cards}[/blue]장 얻습니다.
  - 평등: [gold]프리즘 조각[/gold]을 얻습니다.
- 페이지 요약: DONE: 당신의 의견은 받아들여지지 않는 것으로 보이며, 동상들은 끝없는 논쟁을 이어 나갑니다. | INITIAL: 당신의 눈앞에 꽤나 멋진 광경이 펼쳐집니다. 당신은 우뚝 서있는 세 가지 색의 동상들이 단상에 서서, 색채의 철학적 의미에 관해 [jitter][red]열띤 토론[/red][/jitter]을 벌이고 있는 모습을 목격합니다. 논쟁을 듣고 있자니, 현재 그들에게 있어 가장 중요한 의문은 진실로 [gold]가장 위대한[/gold] 색은 어떤 색인가에 관한 것으로 보입니다. 당신도 스스로 생각하는 바를 그들에게 말합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `COLORFUL_PHILOSOPHERS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ColorfulPhilosophers`
- 리소스 경로: `res://src/Core/Models/Events/ColorfulPhilosophers.css`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 물에 잠긴 기록실

- ID: `megacrit-sts2-core-models-events-waterloggedscriptorium`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 어두컴컴한 통로를 헤쳐 나아가던 중, 작은 가게에서 일하고 있는 [aqua]수척한 인물[/aqua]을 우연히 발견합니다. 무수히 많은 선반에는 눅눅한 두루마리와 양피지가 [sine]빽빽하게[/sine] 쌓여 있습니다.

손님이 들어왔다는 것을 알아차린 [aqua]필경사 상인[/aqua]은 잽싸게 자세를 고쳐 앉고, 책상 위에 놓인 몇몇 [gold]도구들[/gold]을 가리킵니다.
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 5
- 확인된 선택지:
  - 꺼끌꺼끌한 스펀지: [gold]골드[/gold]를 [red]{PricklySpongeGold}[/red] 지불합니다. 카드 [blue]{Cards}[/blue]장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{PricklySpongeGold}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{Gold}[/blue] 필요합니다.
  - 촉수 펜: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 카드 1장에 [purple]안정[/purple]을 [gold]인챈트[/gold]합니다.
  - 핏빛 잉크: 최대 체력을 [green]6[/green] 얻습니다.
- 페이지 요약: BLOODY_INK: [aqua]필경사[/aqua]는 [red]밝은 붉은빛의 잉크[/red]가 담긴 병을 조심스레 집어 들고서 한 번 흔든 뒤, 자신의 손을 병 안에 담그는 듯한 동작을 취합니다. 당신이 손가락을 잉크에 담그자, [green][jitter]강인한 생명력[/jitter][/green]이 당신의 온몸을 꿰뚫고 지나갑니다! 당신에 반응에 만족한 [aqua]필경사[/aqua]는, 정중하게 고개를 숙인 뒤 손짓하며 당신을 배웅합니다. | INITIAL: 당신은 어두컴컴한 통로를 헤쳐 나아가던 중, 작은 가게에서 일하고 있는 [aqua]수척한 인물[/aqua]을 우연히 발견합니다. 무수히 많은 선반에는 눅눅한 두루마리와 양피지가 [sine]빽빽하게[/sine] 쌓여 있습니다. 손님이 들어왔다는 것을 알아차린 [aqua]필경사 상인[/aqua]은 잽싸게 자세를 고쳐 앉고, 책상 위에 놓인 몇몇 [gold]도구들[/gold]을 가리킵니다. | PRICKLY_SPONGE: [aqua]필경사[/aqua]는 [gold]꺼끌꺼끌한 스펀지[/gold]를 움켜쥐고 (삐걱대는 소리가 납니다) 당신의 두루마리를 가볍게 두드립니다. [jitter][blue]*삐걱 삐걱 삐걱*[/blue][/jitter] 스펀지의 꺼끌꺼끌한 감촉이 [purple][sine]반짝이는 자국[/sine][/purple]을 남기며, 잉크를 안정시키고 있는 것으로 보입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WATERLOGGED_SCRIPTORIUM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WaterloggedScriptorium`
- 리소스 경로: `res://images/events/waterlogged_scriptorium.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 장로 랜위드

- ID: `megacrit-sts2-core-models-events-ranwidtheelder`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신이 [purple]지금까지 본 사람 중 가장 나이가 많은 사람[/purple]이 당신에게 다가옵니다.
[sine]“또 만났네요... 저잖아요, 랜위드!”[/sine]

당신은 이 사람을 모릅니다.
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 5
- 확인된 선택지:
  - {Potion}을(를) 준다: 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - {Relic}을(를) 준다: 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
  - 골드를 {Gold} 준다: 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 잠김: 줄 수 있는 포션이 없습니다.
  - 잠김: 줄 수 있는 유물이 없습니다.
- 페이지 요약: GOLD: [sine]“엄청.. 나네요...”[/sine] 랜위드는 [gold]골드[/gold]를 씹으며 중얼거립니다. | INITIAL: 당신이 [purple]지금까지 본 사람 중 가장 나이가 많은 사람[/purple]이 당신에게 다가옵니다. [sine]“또 만났네요... 저잖아요, 랜위드!”[/sine] 당신은 이 사람을 모릅니다. | POTION: [sine]“절묘하네요...”[/sine] [sine][blue]꿀꺽 꿀꺽 꿀꺽[/blue][/sine] 그는 [gold]{Potion}[/gold] 한 병을 한 입에 모두 마셨습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RANWID_THE_ELDER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RanwidTheElder`
- 리소스 경로: `res://images/events/ranwid_the_elder.png:`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 차의 명인

- ID: `megacrit-sts2-core-models-events-teamaster`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 불빛이 희미하게 비치는 오두막에 우연히 들어간 당신은, 기이할 정도로 다양한 차들로 가득 찬 너무나도 평온한 공간에 빠져듭니다.

오두막 안에 있던 남자는 아무 말 없이 하나 같이 [gold]고가[/gold]의 가격표가 붙어있는 다양한 [green]차[/green]들을 가리킵니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 5
- 확인된 선택지:
  - 무례함의 차
  - 뼈다귀 차: [gold]골드[/gold]를 [red]{BoneTeaCost}[/red] 지불합니다. {BoneTeaDescription}
  - 잉걸불 차: [gold]골드[/gold]를 [red]{EmberTeaCost}[/red] 지불합니다. {EmberTeaDescription}
  - 잠김: [gold]골드[/gold]가 [blue]{BoneTeaCost}[/blue] 필요합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{EmberTeaCost}[/blue] 필요합니다.
- 페이지 요약: DONE: [blue]차의 명인[/blue]은 당신의 요청을 받아들이고, 곧바로 준비를 시작합니다. [gold]1.[/gold] 숯이 채워진 물병에 든 물을 주전자에 붓습니다. [gold]2.[/gold] 적절한 [purple]보랏빛 불꽃[/purple]으로 주전자를 가열합니다. [gold]3.[/gold] 물이 끓기 전에, 찻잎 한 스푼을 주전자에 넣습니다. [gold]4.[/gold] 불을 끄고, 리드미컬한 박수로 시간을 잽니다. [gold]5.[/gold] 주전자를 높게 들어올린 뒤, 자그마한 컵에 차를 정확히 부어 넣습니다. [green][sine]“차 나왔습니다.”[/sine][/green] | INITIAL: 불빛이 희미하게 비치는 오두막에 우연히 들어간 당신은, 기이할 정도로 다양한 차들로 가득 찬 너무나도 평온한 공간에 빠져듭니다. 오두막 안에 있던 남자는 아무 말 없이 하나 같이 [gold]고가[/gold]의 가격표가 붙어있는 다양한 [green]차[/green]들을 가리킵니다. | TEA_OF_DISCOURTESY: 당신은 정중하게 차를 거절했고— [jitter][red]*쾅*[/red][/jitter] 뒤에 있던 문이 쾅하고 닫힙니다. [blue]차의 명인[/blue]은 잠시 동안 당신을 쳐다보다, 흉측하게 생긴 컵 하나를 당신에게 선물합니다. 오, 이 차는 무례한 사람들에게 내어지는 차입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TEA_MASTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TeaMaster`
- 리소스 경로: `res://images/events/tea_master.png4g`
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
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 4
- 확인된 선택지:
  - 감정의 인식: [gold]골드[/gold]를 [red]{EmotionalAwarenessCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다.
  - 거미류 침술: [gold]골드[/gold]를 [red]{ArachnidAcupunctureCost}[/red] 지불합니다. [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다.
  - 잠김: [gold]골드[/gold]가 부족합니다.
  - 호흡법: [gold]골드[/gold]를 [red]{BreathingTechniquesCost}[/red] 지불합니다. [gold]덱[/gold]에 [gold]계몽[/gold]을 [blue]2[/blue]장 추가합니다.
- 페이지 요약: ARACHNID_ACUPUNCTURE: [sine]“움직이지... 마세요...”[/sine] [orange]조그마한 거미[/orange]가 중얼거리자 거미의 앞다리가 [aqua][jitter]빛나기[/jitter][/aqua] 시작합니다. 그리고— [jitter]날카롭게 몰아치는 찌르기[/jitter] 동작으로, 당신의 몸을 찔러댑니다! 당신의 몸이 깃털처럼 가벼워집니다. 돈을 낼 만한 가치가 있었네요. | BREATHING_TECHNIQUES: [sine]“기도가 계속 열려 있는 모습을 상상해 보세요... 모든 호흡에는 의미가 있습니다.”[/sine] 당신은 스스로가 이해할 수 있는 방식으로 이 기술을 연마합니다. | EMOTIONAL_AWARENESS: [orange]콩알만 한 거미[/orange]가 [sine][purple]최면을 거는 듯한 춤을 추며[/purple][/sine] 반복적으로 외치기 시작합니다. [sine]“생각은 현실에 영향을 주지 않는다. 공포는 생각을 죽인다. 계획을 따르라.”[/sine] 당신은 거미의 말을 이해합니다. [orange]조그만 거미[/orange] 한 마리가 던진 묵직한 한마디였네요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ZEN_WEAVER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ZenWeaver`
- 리소스 경로: `res://images/events/zen_weaver.pngP`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 나무 조각

- ID: `megacrit-sts2-core-models-events-woodcarvings`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 먼지가 쌓인 상자를 열고, 정교한 형태의 나무 조각 3개를 발견했습니다:

새, 뱀, 그리고... 고리?

근처에 있던 받침대에는 조각의 바닥 부분과 일치하는 움푹 들어간 흔적이 있습니다. 받침대 위에 어떤 조각을 올려놓을까요?
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 4
- 확인된 선택지:
  - 고리: 시작 카드를 [blue]1[/blue]장 선택해 [gold]{ToricCard}[/gold]으로 [gold]변화[/gold]시킵니다.
  - 뱀: 카드 [blue]1[/blue]장에 [purple]{SnakeEnchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 새: 시작 카드를 [blue]1[/blue]장 선택해 [gold]{BirdCard}[/gold]로 [gold]변화[/gold]시킵니다.
  - 잠김: [purple]미끈거림[/purple]을 인챈트할 수 있는 카드가 없습니다.
- 페이지 요약: BIRD: 새 조각을 받침대 위에 올려놓자 조각상의 눈이 밝게 빛나고, 당신의 머릿속에서 희미하게 어떤 소리가 울려 퍼집니다. [sine]...까악.... 까악 까아아아아악...[/sine] 받침대가 열리고, 이상한 액체가 쏟아져 나오기 시작합니다. 당연히, 당신은 그것을 모두 마셨습니다. | INITIAL: 당신은 먼지가 쌓인 상자를 열고, 정교한 형태의 나무 조각 3개를 발견했습니다: 새, 뱀, 그리고... 고리? 근처에 있던 받침대에는 조각의 바닥 부분과 일치하는 움푹 들어간 흔적이 있습니다. 받침대 위에 어떤 조각을 올려놓을까요? | SNAKE: 뱀 조각을 받침대 위에 올려놓자 조각상의 비늘이 밝게 빛나기 시작합니다! 당신의 약간의 온기와 편안함과 함께, 몸이 휘청거리는 느낌을 받았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WOOD_CARVINGS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WoodCarvings`
- 리소스 경로: `res://images/events/wood_carvings.png v`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 랜턴 열쇠

- ID: `megacrit-sts2-core-models-events-thelanternkey`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 희미하게 빛나는 열쇠를 발견하고, 열쇠를 집어 들기 위해 다가갑니다.

“그 열쇠를 찾으러 사방팔방 뒤지고 있었다네! 혹시 실례가 아니라면...”

조금 귀찮아 보이는 사람이긴 하지만, 함부로 단정 짓는 건 좋지 않을 것 같습니다.
- 구조 정보:
  - 페이지 키 수: 1
  - 선택지 키 수: 3
- 확인된 선택지:
  - RETURN THE KEY: 낯선 사람은 당신의 협조에 감사를 표하며, 후하게 사례합니다.
  - 싸운다
  - 열쇠를 돌려준다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
  - 열쇠를 지킨다: 열쇠를 얻기 위해 싸웁니다.
- 페이지 요약: INITIAL: 당신은 희미하게 빛나는 열쇠를 발견하고, 열쇠를 집어 들기 위해 다가갑니다. “그 열쇠를 찾으러 사방팔방 뒤지고 있었다네! 혹시 실례가 아니라면...” 조금 귀찮아 보이는 사람이긴 하지만, 함부로 단정 짓는 건 좋지 않을 것 같습니다. | KEEP_THE_KEY: 낯선 사람은 살기 어린 시선으로 당신을 바라봅니다. 열쇠를 가지고 이곳을 떠날 수 있는 건 오직 한 명뿐인 것 같습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_LANTERN_KEY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLanternKey`
- 리소스 경로: `res://src/Core/Models/Events/TheLanternKey.cs`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 심연의 욕탕

- ID: `megacrit-sts2-core-models-events-abyssalbaths`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 후미진 곳에 있는 방을 발견했습니다.

최면을 거는 듯한 리듬으로 색이 변하는 뜨거운 액체 웅덩이에서는 증기가 피어오르고 있습니다. 천장에는 따개비처럼 매달린 증식물들로부터, 바닥에 닿는 순간 치익 소리를 내며 부글거리는 끈적한 액체가 떨어지고 있습니다. 주변의 공기는 [blue]소금기[/blue]와 함께 [green]명백하게 유기적인[/green] 무언가로 가득 찬 채로, 무겁게 가라앉아 있습니다.

가장 큰 웅덩이의 가장자리로 다가가니, 액체에 파동이 일기 시작합니다. 당신이 들어올 것을 기대하기라도 한 듯 더 맹렬하게 거품이 일어납니다.
- 구조 정보:
  - 페이지 키 수: 4
  - 선택지 키 수: 4
- 확인된 선택지:
  - 몸을 담근다: 최대 체력을 [green]{MaxHp}[/green] 얻습니다. 피해를 [red]{Damage}[/red] 받습니다.
  - 자제한다: 체력을 [green]{Heal}[/green] 회복합니다.
  - 좀 더 머문다: 최대 체력을 [green]{MaxHp}[/green] 얻습니다. 피해를 [red]{Damage}[/red] 받습니다.
  - 탕에서 나간다
- 페이지 요약: ABSTAIN: 당신은 물의 유혹을 뿌리치고서, 가장자리를 따라 결정화되어 있는 소금을 그러모았습니다. 소금 결정을 피부에 바르니, 웅덩이에서 파도가 일며 거품이 올라오기 시작합니다. 주변 기온이 눈에 띄게 떨어지고, 방 안의 모든 반사면에 공허한 눈을 지닌 왜소해 보이는 또 다른 당신의 모습이 잠시 비춰집니다. 완전히 새로운 자신이 될 수 있는 기회를 놓쳤다는 느낌이 머릿속을 떠나지 않습니다. | DEATH_WARNING: [sine][red]이 이상 목욕 시 죽게 될 것입니다.[/red][/sine] | EXIT_BATHS: 당신은 더 이상 열기를 버티지 못하고, 욕탕 밖으로 뛰쳐나왔습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ABYSSAL_BATHS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AbyssalBaths`
- 리소스 경로: `res://images/events/abyssal_baths.png`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 4
- 확인된 선택지:
  - 목을 축이고 들어올린다: [red]{DrinkRandomPotion}[/red]을(를) 잃습니다. 최대 체력을 [blue]{DrinkMaxHpGain}[/blue] 얻습니다.
  - 민다: 체력을 [red]{PushHpLoss}[/red] 잃습니다. 공격 카드 1장에 [purple]활력[/purple]을 [blue]{PushVigorousAmount}[/blue] [gold]인챈트[/gold]합니다.
  - 잠김: 포션이 필요합니다.
  - 잠김: 공격 카드가 필요합니다.
- 페이지 요약: INITIAL: 버려진 안뜰 한복판에 거대한 바위 하나가 놓여 있습니다. 명판에는 이렇게 적혀 있습니다: [gold][b]영원의 돌[/b][/gold]. 바위의 존재감과 형태, 그리고 장식들은, 이 안뜰이 돌의 주변을 따라 지어졌으며, [blue]돌[/blue]은 이곳에서 단 한 번도 움직인 적이 없음을 나타냅니다. | LIFT: 당신은 [gold]{DrinkRandomPotion}[/gold] 병을 내려놓은 뒤, [jitter]온 힘을 다해 바위를 들어올렸습니다[/jitter]!! 아무 일도 일어나지 않았습니다. | PUSH: 당신은 가지고 있는 모든 힘을 쥐어짜내 바위를 [jitter]힘차게 밀었습니다[/jitter]!! 아무 일도 일어나지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `STONE_OF_ALL_TIME`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.StoneOfAllTime`
- 리소스 경로: `res://images/events/stone_of_all_time.png`
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
- 구조 정보:
  - 페이지 키 수: 5
  - 선택지 키 수: 3
- 확인된 선택지:
  - TAKE: [gold]{RelicName}[/gold]을(를) 받습니다.
  - 깊게 고민한 뒤 최고의 선택을 한다: 체력을 [red]{ExamineHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]3[/blue]개 중 [blue]1[/blue]개를 선택합니다.
  - 아무거나 고른다: 무작위 [gold]인형 유물[/gold]을 1개 얻습니다.
  - 잠시 고민한다: 체력을 [red]{TakeTimeHpLoss}[/red] 잃습니다. [gold]인형 유물[/gold] [blue]2[/blue]개 중 [blue]1[/blue]개를 선택합니다.
- 페이지 요약: DAUGHTER_OF_WIND: [blue]“내가 널 골랐다는 걸 기뻐해야 해!”[/blue] 인형은 의기양양하게 선언합니다. | EXAMINE: 당신은 선택을 내리기 전에 모든 인형을 빠짐없이 살펴봅니다. 인형들의 비명 소리는 점점 커져 [red][jitter]귀청을 찢는 듯한 소리[/jitter][/red]가 됐지만, 당신은 완전히 미쳐버리기 전에 어떻게든 선택을 내렸습니다! | FABLE: 상냥한 목소리가 만족스럽게 당신의 머릿속을 채웁니다. [orange][sine]“우리 함께 가장 신나는 이야기들을 파헤쳐 보자!”[/sine][/orange]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOLL_ROOM`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DollRoom`
- 리소스 경로: `res://images/events/doll_room.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 공생체

- ID: `megacrit-sts2-core-models-events-symbiote`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 길을 나아가던 도중 우연히, 형태가 일정치 않은 한 검은색 덩어리를 발견합니다. 당신은 그 덩어리가 굉장히 오래되었고, 극도로 사악한 존재라는 것을 느낄 수 있었습니다.

[sine]가까이 다가갑니다...[/sine]
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 다가간다: 공격 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 불로 태워 죽인다: 카드를 1장 선택해 [gold]변화[/gold]시킵니다.
  - 잠김: 인챈트할 수 있는 공격 카드가 없습니다.
- 페이지 요약: APPROACH: 덩어리가 갑자기 당신에게 달려듭니다. 당신은 팔에 들러붙은 덩어리들로 인해 타들어가는 듯한 통증을 느꼈지만, 덩어리는 순식간에 사라져버렸습니다. ...? 어디로 사라진 걸까요? | INITIAL: 당신은 길을 나아가던 도중 우연히, 형태가 일정치 않은 한 검은색 덩어리를 발견합니다. 당신은 그 덩어리가 굉장히 오래되었고, 극도로 사악한 존재라는 것을 느낄 수 있었습니다. [sine]가까이 다가갑니다...[/sine] | KILL_WITH_FIRE: 당신은 검은색 덩어리에 횃불을 갖다 댑니다. 덩어리가 날카롭게 비명을 지르며 재로 변하는 동안 당신의 머리는 고통스럽게 울렸지만, 간신히 정신은 잃지 않았습니다. 대체 그건 뭐였을까요?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SYMBIOTE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Symbiote`
- 리소스 경로: `res://images/events/symbiote.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 길 잃은 위습

- ID: `megacrit-sts2-core-models-events-lostwisp`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]파워[/gold] 카드를 사용할 때마다, 모든 적에게 피해를 [blue]{Damage}[/blue] 줍니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 위습을 붙잡는다: [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]{Relic}[/gold]을 얻습니다.
  - 잠김: 최대 체력이 너무 낮습니다.
  - 주변을 탐색한다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
- 페이지 요약: CLAIM: 당신은 조심스럽게 [orange]위습[/orange]에게 다가갑니다... 가까이 다가갈수록, 위습은 당신의 존재에 점점 더 격하게 반응합니다. 당신은 손이 닿을 거리에서 [orange]위습[/orange]을 붙잡으려고 했지만, 위습은 [red][jitter]뜨거운 증기와 불꽃을 뿜어내 당신에게 화상을 입힙니다![/jitter][/red] 당신은 고통 속에서도 [jitter]허우적거리며 더듬어댄[/jitter] 끝에, 결국 그 생물을 굴복시킵니다. [orange]위습[/orange]은 결국 패배를 인정하고, 당신을 새로운 주인으로 받아들입니다. | INITIAL: 저 멀리 기이한 광경이 보입니다. [sine][red]죽은 곤충들의 사체[/red][/sine]가 산더미처럼 쌓여 있고, 그 한가운데에는 [orange]작은 빛나는 먼지[/orange]처럼 보이는 무언가가 있습니다. 그 먼지는 온갖 벌레를 효과적으로 유인하고 있는 것처럼 보였으나, 돌연 불꽃을 내뿜어대고 있습니다! 당신은 조사를 위해 더 가까이 다가갑니다. | SEARCH: 당신은 그 꼬마 친구를 내버려두기로 했습니다. 건드리지 않는 게 좋겠네요! 주변에 딱히 눈에 띄는 건 없지만, 죽은 벌레들 사이에 약간의 [gold]골드[/gold]가 흩어져 있는 것을 발견했습니다. 왜인지는 궁금해하지 않기로 합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LOST_WISP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LostWisp`
- 리소스 경로: `res://images/events/lost_wisp.png`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 관찰한다: [gold]타격[/gold]이나 [gold]수비[/gold] 1장에 [purple]소용돌이[/purple]를 [gold]인챈트[/gold]합니다.
  - 마신다: 체력을 [green]{Heal}[/green] 회복합니다.
  - 손을 뻗는다: 무작위 [gold]유물[/gold]을 1개 얻습니다. [gold]덱[/gold]에 [red]몸부림[/red]을 추가합니다.
- 페이지 요약: DRINK: 당신은 두 손을 모아 [sine][aqua]나선형 소용돌이[/aqua][/sine] 속으로 손을 담급니다. 물은 천천히 당신의 손을 채웠고, 당신은 눈을 감고 조금씩 음미하듯 물을 마십니다. 상쾌한 물이 [sine]소용돌이치듯[/sine] 당신의 [gold]중심[/gold]을 지나 흐르며, 마음속의 해로운 생각과 육체의 고통을 씻어냅니다... | INITIAL: 당신은 우연히 [aqua][sine]거대한 나선형 소용돌이[/sine][/aqua]를 발견했습니다. 물은 아름답게 소용돌이치고, 나선형 무늬가 주변 벽을 수놓고 있습니다. [purple][sine]빙글빙글 돌고...돌고... 또 돌고...정말 멋진 광경입니다......[/sine][/purple] 무엇을 할까요? | OBSERVE: [sine][purple]...아아, 정말 아름답네요..... 소용돌이는 돌고... 계속 돌고 있습니다. 그래요... 오! 방금..... 물이 튄 거였나요...? ...당연하죠. 당신도 함께 놀 자격이 있답니다,[/purple] [aqua]소용돌이 씨[/aqua][purple]...하하... ... ....... ...[/purple][/sine] 이제 가야 할 시간입니다. 당신은 자리를 떠나며 무언가 중얼거립니다. [gold]“고마워요, 소용돌이.”[/gold]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPIRALING_WHIRLPOOL`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiralingWhirlpool`
- 리소스 경로: `res://images/events/spiraling_whirlpool.pngh`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 난타전

- ID: `megacrit-sts2-core-models-events-punchoff`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]권투형 구조체[/gold] 두 마리가 서로 격렬하게 치고받고 있는 가운데, 둘 사이에 보물이 놓여 있는 모습이 보입니다...

슬쩍 낚아채 볼까요?
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 낚아챈다: [gold]덱[/gold]에 [red]상처[/red]를 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 저 정도는 가뿐하지: 싸우고 [gold]더 좋은 보상[/gold]을 얻습니다.
  - 전투
- 페이지 요약: INITIAL: [gold]권투형 구조체[/gold] 두 마리가 서로 격렬하게 치고받고 있는 가운데, 둘 사이에 보물이 놓여 있는 모습이 보입니다... 슬쩍 낚아채 볼까요? | I_CAN_TAKE_THEM: [gold]구조체들[/gold]이 위협적으로 당신을 향해 돌아섭니다! 당신은 싸울 준비를 합니다. | NAB: 당신은 성공적으로 유물을 낚아챕니다! ...적어도 그렇게 생각했습니다. 오른손 훅이 [jitter][red]당신의 안면을 강타합니다[/red][/jitter].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `PUNCH_OFF`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.PunchOff`
- 리소스 경로: `res://images/events/punch_off.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 빛나는 합창단

- ID: `megacrit-sts2-core-models-events-luminouschoir`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 우연히 부자연스러운 푸른 빛에 휩싸인 공터를 발견합니다. 높게 치솟은 버섯들은 마치 살아있는 듯이 빛나고 맥동하며, 볼록해진 갓은 반짝거리고 있습니다. 가까이 다가가자 버섯들은 [sine][purple]섬뜩하면서도 감미로운 선율[/purple][/sine]을 노래하기 시작하고, 그 울림은 당신의 가슴 속에 깊게 울려 퍼집니다.

당신은 가장 거대한 버섯의 살 속에 박혀 반짝이는 무언가를 발견했고, 그것은 버섯의 노랫소리에 박자를 맞추듯이 [sine]맥동하고[/sine] 있습니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 공물을 바친다: [gold]골드[/gold]를 [red]{Gold}[/red] 지불합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 버섯의 살에 다가간다: [gold]덱[/gold]에서 카드를 [blue]2[/blue]장 제거합니다. [gold]덱[/gold]에 [red]포자 잠식[/red]을 추가합니다.
  - 잠김: [gold]골드[/gold]가 [blue]{Gold}[/blue] 필요합니다.
- 페이지 요약: INITIAL: 당신은 우연히 부자연스러운 푸른 빛에 휩싸인 공터를 발견합니다. 높게 치솟은 버섯들은 마치 살아있는 듯이 빛나고 맥동하며, 볼록해진 갓은 반짝거리고 있습니다. 가까이 다가가자 버섯들은 [sine][purple]섬뜩하면서도 감미로운 선율[/purple][/sine]을 노래하기 시작하고, 그 울림은 당신의 가슴 속에 깊게 울려 퍼집니다. 당신은 가장 거대한 버섯의 살 속에 박혀 반짝이는 무언가를 발견했고, 그것은 버섯의 노랫소리에 박자를 맞추듯이 [sine]맥동하고[/sine] 있습니다. | OFFER_TRIBUTE: 물컹한 땅 위에 금화를 올려놓습니다. 유물을 깔끔하게 회수할 수 있도록 가느다란 촉수들이 유물을 지면으로 밀어올리는 동시에, 동전들이 서서히 가라앉습니다. | REACH_INTO_THE_FLESH: 당신은 가장 큰 버섯의 물컹한 조직 속으로 손을 밀어넣어, 안에 박혀 있는 유물을 꺼내려 합니다. 균근망은 당신의 침입으로 인해 몸서리치고, 그 반응에 당신의 정신도 함께 [sine]흔들립니다[/sine].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `LUMINOUS_CHOIR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.LuminousChoir`
- 리소스 경로: `res://images/events/luminous_choir.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 울창한 초목

- ID: `megacrit-sts2-core-models-events-densevegetation`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 한참 동안 길을 잘못 든 채로 헤매다 보니, 당신은 [green]양치류[/green]와 [green]관목[/green], [green]덩굴[/green]이 뒤엉킨 울창한 정글에 들어왔다는 것을 깨닫습니다. 특히 [green]덩굴[/green]이 많네요. 피로가 몰려오고, 불길한 생각 하나가 머리를 스칩니다:

[sine][purple]“너는 길을 잃었고, 무방비하며, 피할 수 없는 죽음이 가까워지고 있다.”[/purple][/sine]

무엇을 할까요?
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 묵묵히 나아간다: [gold]덱[/gold]에서 카드를 1장 제거합니다. 체력을 [red]{HpLoss}[/red] 잃습니다.
  - 싸운다!
  - 휴식한다: 체력을 [green]{Heal}[/green] 회복합니다. [red]적[/red]들과 싸웁니다.
- 페이지 요약: INITIAL: 한참 동안 길을 잘못 든 채로 헤매다 보니, 당신은 [green]양치류[/green]와 [green]관목[/green], [green]덩굴[/green]이 뒤엉킨 울창한 정글에 들어왔다는 것을 깨닫습니다. 특히 [green]덩굴[/green]이 많네요. 피로가 몰려오고, 불길한 생각 하나가 머리를 스칩니다: [sine][purple]“너는 길을 잃었고, 무방비하며, 피할 수 없는 죽음이 가까워지고 있다.”[/purple][/sine] 무엇을 할까요? | REST: 당신은 몰려오는 피로를 이기지 못하고, 잠시 잠에 듭니다... ...하지만 이내 몸 위에서 [gold][jitter]무언가가 꿈틀거리는 감각[/jitter][/gold]에 의해 잠에서 깨어납니다! | TRUDGE_ON: 당신은 밀림을 가르고 헤치며 나아갔지만, 숲은 끝없이 이어집니다... 당신은 [green]정체를 알 수 없는 과일[/green]과 [orange]바삭거리는 벌레[/orange]들로 허기를 달래고, [aqua]빛나는 식물[/aqua]의 꿀을 마시며 버텼지만, 점점 [jitter][red]편집적인 망상[/red][/jitter]이 떠오르기 시작합니다... 하지만 이윽고, 당신은 공터와 오솔길, 땅바닥에 그려진 지도 같은 그림들을 발견합니다. 문제가 해결됐네요.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DENSE_VEGETATION`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DenseVegetation`
- 리소스 경로: `res://images/events/dense_vegetation.png`
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
- 구조 정보:
  - 페이지 키 수: 3
  - 선택지 키 수: 3
- 확인된 선택지:
  - 계속
  - 싸움을 건다: 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
  - 차를 즐긴다: [red]{Relic}[/red]을 얻습니다. [green]체력을 모두 회복합니다.[/green]
- 페이지 요약: CONTINUE_FIGHT: 파티의 참석자들은 놀란 눈으로 익숙한 일원의 시신을 바라봅니다... 그리고 [jitter]갑자기 박수를 치기 시작합니다!?[/jitter] “[red]와이고어 경[/red]을 어떻게 처리해야 할지 고민하고 있었는데, 정말 놀랍군요!” 그들은 당신의 [green]독이 든 차[/green]를 던져버렸고, 당신은 그들과 함께 밤새도록 [orange]전술[/orange]과 [purple]비밀[/purple] 이야기를 나눴습니다. 정말 멋진 다과회였네요. | ENJOY_TEA: 당신은 찻잔을 집어들고서 한 번에 들이켭니다. 홍차는 [jitter][red]미친듯이 뜨거웠지만[/red][/jitter], 당신은 표정 하나 바꾸지 않습니다. 파티의 참석자들은 당신의 자기 파괴적 행위에 놀라움을 금치 못합니다. 악의는 없었습니다, [blue]갤러롯 경[/blue]. 진심으로 사과드립니다.” 당신은 [green]독[/green]을 섭취했다는 것도 모른 채로, 홍차와 크럼핏을 즐겼습니다. | INITIAL: 당신은 [blue]“갤러롯 경”[/blue]에게 온 다과회의 [orange]초대장[/orange]을 발견했고, 그를 대신해 파티에 참석하기로 했습니다. 비교적 소박해 보이는 원형 홀에 들어서자, 당신은 [gold]지휘관[/gold]과 [aqua]장군[/aqua], [red]군벌[/red], [blue]용병[/blue]들이 함께... 홍차를 마시고 있는 곳 한복판에 있다는 것을 알게 됐습니다. [gold]황금 왕관[/gold]을 쓴 거대한 기사 한 명이 입을 엽니다. [jitter]“[b]차[/b] 모임에 조금 늦으셨군요?!”[/jitter]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROUND_TEA_PARTY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoundTeaParty`
- 리소스 경로: `res://images/events/round_tea_party.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 유물 상인

- ID: `megacrit-sts2-core-models-events-relictrader`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신이 모퉁이를 도는 순간, 바로 앞에 수상한 인물이 서 있습니다. 그는 당신을 향해 돌아서며 말합니다.

“어서 오세요! 뭔가 거래하실 건가요?”
그는 질문과 동시에 망토를 펼치며, 당신에게 대량의 [sine][purple]수상한 물건들[/purple][/sine]을 선보입니다.
- 구조 정보:
  - 페이지 키 수: 1
  - 선택지 키 수: 3
- 확인된 선택지:
  - 아래의 것을 가져간다: [gold]{BottomRelicOwned}[/gold]을(를) [gold]{BottomRelicNew}[/gold](와)과 거래합니다.
  - 위의 것을 가져간다: [gold]{TopRelicOwned}[/gold]을(를) [gold]{TopRelicNew}[/gold](와)과 거래합니다.
  - 중간 것을 가져간다: [gold]{MiddleRelicOwned}[/gold]을(를) [gold]{MiddleRelicNew}[/gold](와)과 거래합니다.
- 페이지 요약: DONE: “헤헤헤 흐... 감사합니다!” | INITIAL: 당신이 모퉁이를 도는 순간, 바로 앞에 수상한 인물이 서 있습니다. 그는 당신을 향해 돌아서며 말합니다. “어서 오세요! 뭔가 거래하실 건가요?” 그는 질문과 동시에 망토를 펼치며, 당신에게 대량의 [sine][purple]수상한 물건들[/purple][/sine]을 선보입니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `RELIC_TRADER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RelicTrader`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 잊힌 자의 무덤

- ID: `megacrit-sts2-core-models-events-graveoftheforgotten`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter][aqua]푸른 불꽃이 거세게 일렁이는[/aqua][/jitter] 무덤 하나가 있습니다...

긍지 높은 한 전사는 전투 중 사망했지만, 영혼은 아직 안식을 얻지 못했습니다. 그 영혼은 계속 싸우고 싶어하고, 당신과 함께 여행을 떠나고 싶다 [sine]애원하는[/sine] 듯합니다.

하지만 어쩌면, 이제는 그가 [red]진실[/red]을 마주해야 할 때일지도 모릅니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - {Relic}을 받아들인다: [gold]{Relic}[/gold]을 얻습니다.
  - 사실을 알린다: [gold]덱[/gold]에 [red]{Curse}[/red]를 추가합니다. [gold]소멸[/gold] 카드에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 잠김: [purple]인챈트[/purple]할 수 있는 [gold]소멸[/gold] 카드를 보유하고 있지 않습니다.
- 페이지 요약: ACCEPT: 당신은 무덤에 손을 얹고 속삭입니다... [sine]“나와 함께 올라가자…”[/sine] 이에 응답하듯, 무덤으로부터 불꽃이 피어올라 한데 모여 하나의 [aqua]불타는 영혼[/aqua]이 되고, 당신의 곁을 충성스럽게 맴돕니다. 그 영혼은 당신이 무엇인지 알고 있으며, 마지막 목적지까지 당신을 인도하고 싶어 합니다. | CONFRONT: 당신은 무덤에 손을 얹고 속삭입니다... [sine]“당신의 싸움은 끝났다…”[/sine] 이에 응답하듯, 그 영혼은 당신의 영혼과 충돌하며 [jitter][aqua]영혼의 결투[/aqua][/jitter]가 벌어집니다! 당신이 겪어온 셀 수 없이 많은 전투에 경악한 그 영혼은, 패배를 인정합니다. | INITIAL: [jitter][aqua]푸른 불꽃이 거세게 일렁이는[/aqua][/jitter] 무덤 하나가 있습니다... 긍지 높은 한 전사는 전투 중 사망했지만, 영혼은 아직 안식을 얻지 못했습니다. 그 영혼은 계속 싸우고 싶어하고, 당신과 함께 여행을 떠나고 싶다 [sine]애원하는[/sine] 듯합니다. 하지만 어쩌면, 이제는 그가 [red]진실[/red]을 마주해야 할 때일지도 모릅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `GRAVE_OF_THE_FORGOTTEN`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.GraveOfTheForgotten`
- 리소스 경로: `res://images/events/grave_of_the_forgotten.png<`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 3
- 확인된 선택지:
  - 1로 설정: 체력이 [blue]{Setting1Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
  - 2로 설정: 체력이 [blue]{Setting2Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 카드를 [blue]2[/blue]장 [gold]강화[/gold]합니다.
  - 3으로 설정: 체력이 [blue]{Setting3Hp}[/blue]인 훈련 인형과 싸웁니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: DEFEAT: “[jitter][red]너는 약하다!![/red][/jitter] 굴욕을 집행했다!” 인형은 잠시 동안 침묵한 뒤 말합니다— “다음에는 더 낮게 설정하시길 바랍니다. 좋은 하루 되세요.” | INITIAL: 당신이 가까이 다가가자, 인형은 덜거덕거리며 지직대는 소리와 함께 강렬한 빛을 뿜기 시작합니다! “[jitter]찌리릿![/jitter] 훈련 시간!!! 나를 쓰러뜨리기 위한 [blue]3번의 턴[/blue]이 주어진다! 설정을 조절하지 않으면 [red]치명적인 굴욕[/red]을 마주하게 된다. 선택지는 다음과 같습니다:” 섬뜩한 메시지가 끝난 뒤, 훈련 인형은 상세한 설명 사항을 차분하게 읽어 내려갑니다. 어떤 것을 선택할까요? | VICTORY: “[jitter]너는 훈련을 통과했다![/jitter] 넌 이제 이 [gold]공장[/gold]을 침입자들로부터 방어할 모든 준비를 마쳤다!! 훈련 세션 후에는 반드시 수분을 섭취하도록!”
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BATTLEWORN_DUMMY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BattlewornDummy`
- 리소스 경로: `res://images/events/battleworn_dummy.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 가라앉는 등대

- ID: `megacrit-sts2-core-models-events-drowningbeacon`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 바위와 잔잔한 물로 이루어진 초현실적인 풍경을 지나자, 당신은 [purple]빛과 반대되는 무언가[/purple]를 뿜어대고 있는 [sine]가라앉은 등대[/sine]를 마주합니다.

당신은 이곳에서 [aqua]기분 나쁘게 빛나고 있는[/aqua] 물을 병에 담거나, 낡고 허름한 구조물을 타고 올라가 등대의 [gold]렌즈[/gold]를 수거할 수 있습니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 병에 담는다: [aqua]{Potion}[/aqua]을 생성합니다.
  - 올라간다: [gold]{Relic}[/gold]를 획득합니다. 최대 체력을 [red]{HpLoss}[/red] 잃습니다.
- 페이지 요약: BOTTLE: 당신은 등대가 조용히 진흙탕 속으로 가라앉는 동안, 근처에 있는 병을 사용해 [aqua]반짝거리는 액체[/aqua]를 퍼 담았습니다. 등대 안에 있지 않았던 게 천만다행이네요. | CLIMB: 당신은 등대가 계속해서 가라앉는 것을 불안하게 여기며, 등대의 꼭대기로 올라갑니다. 당신은 [gold]조명실[/gold]에 도착한 뒤, [purple]빛과 반대되는 무언가[/purple]가 [jitter][red]당신의 생기를 빨아들이는[/red][/jitter] 동안 렌즈를 떼어내기 시작했습니다. 마침내 렌즈를 분리하자, 등대는 가라앉기를 멈춥니다. | INITIAL: 바위와 잔잔한 물로 이루어진 초현실적인 풍경을 지나자, 당신은 [purple]빛과 반대되는 무언가[/purple]를 뿜어대고 있는 [sine]가라앉은 등대[/sine]를 마주합니다. 당신은 이곳에서 [aqua]기분 나쁘게 빛나고 있는[/aqua] 물을 병에 담거나, 낡고 허름한 구조물을 타고 올라가 등대의 [gold]렌즈[/gold]를 수거할 수 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DROWNING_BEACON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DrowningBeacon`
- 리소스 경로: `res://images/events/drowning_beacon.png9N`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 두 번째 상자: [gold]골드[/gold]를 [blue]{LargeChestGold}[/blue] 얻습니다. [red]탐욕[/red]을 얻습니다.
  - 첫 번째 상자: [gold]골드[/gold]를 [blue]{SmallChestGold}[/blue] 얻습니다.
- 페이지 요약: FIRST_CHEST: 네, [gold]골드[/gold]가 들어 있네요. | INITIAL: 길을 따라가던 당신은, 일부가 물에 잠긴 저장고를 발견합니다. [gold]상자[/gold]는 [blue]2[/blue]개가 있지만, 금방이라도 [jitter][purple]망가져버릴 듯한 열쇠[/purple][/jitter]는 [blue]1[/blue]개 뿐입니다. [gold]첫 번째 상자:[/gold] 흔들어보니 딸랑거리는 소리가 납니다. 약간의 골드가 들어 있습니다. [gold]두 번째 상자:[/gold] 거대하고 화려한 상자로, 명백히 [red]저주받았습니다[/red]. 많은 골드가 있습니다! | SECOND_CHEST: 이렇게 많은 [gold]골드[/gold]가 모여있는 건 생전 처음 보는 광경입니다! [sine][/sine][blue]상인[/blue][sine]이 파는 물건들을 전부 사버릴 수도 있을 것 같습니다...[/sine] {Monologue} [sine]위험한 불량배들은 돈으로 물러나게 할 수 있습니다...[/sine] 하지만 [red][sine]병적인 탐욕[/sine][/red]은 영원히 당신의 마음속에 남을 것입니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUNKEN_TREASURY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenTreasury`
- 리소스 경로: `res://images/events/sunken_treasury.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 감염된 자동인형

- ID: `megacrit-sts2-core-models-events-infestedautomaton`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [red]작동을 멈춘 로봇들[/red]로 가득한 방이 있습니다.

고독한 자동인형 하나가 [sine][aqua]희미하게 빛나는 핵[/aqua][/sine]을 유지하고 있지만, 기괴한 유기적 증식체에 휩싸여 아무런 반응도 보이지 않습니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 살펴본다: 무작위 [gold]파워 카드[/gold]를 1장 얻습니다.
  - 핵을 만진다: 무작위 [gold]비용이 0인 카드[/gold]를 1장 얻습니다.
- 페이지 요약: INITIAL: [red]작동을 멈춘 로봇들[/red]로 가득한 방이 있습니다. 고독한 자동인형 하나가 [sine][aqua]희미하게 빛나는 핵[/aqua][/sine]을 유지하고 있지만, 기괴한 유기적 증식체에 휩싸여 아무런 반응도 보이지 않습니다. | STUDY: [aqua]빛나는 자동인형[/aqua]과 인형의 내부 구조가 어떤 방식으로 배치되어 있는지를 살펴본 당신은, 특정 금속과 그 금속의 배열 상태가 여러 가지 이상 상태에 저항할 수 있다는 사실을 깨닫습니다! 당신은 이 [blue]기술[/blue]을 자신에게 적용합니다. | TOUCH_CORE: 손이 [aqua]핵[/aqua]에 살짝 닿는 순간, [jitter][gold]전기[/gold] 충격이 당신을 강타합니다[/jitter]. 당신은 그 경험으로 인해 무언가 [sine][purple]달라진[/purple][/sine] 기분을 느끼며, 황급히 달아납니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `INFESTED_AUTOMATON`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.InfestedAutomaton`
- 리소스 경로: `res://images/events/infested_automaton.pngl`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 거울에 비치다 다치비 에울거

- ID: `megacrit-sts2-core-models-events-reflections`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 이건 뭘까요? 이곳은 뭔가 이상한 느낌이 듭니다...
[sine]...다니듭 이낌느 한상이 가뭔 은곳이 ?요까뭘 건이[/sine]
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 거울을 만진다: 무작위 카드를 [blue]2[/blue]장 [red]열화[/red]시킵니다. 무작위 카드를 [blue]4[/blue]장 [green]강화[/green]합니다.
  - 부순다: [gold]덱[/gold]을 복제합니다. [purple]불운[/purple]을 얻습니다.
- 페이지 요약: INITIAL: 이건 뭘까요? 이곳은 뭔가 이상한 느낌이 듭니다... [sine]...다니듭 이낌느 한상이 가뭔 은곳이 ?요까뭘 건이[/sine] | SHATTER: 당신은 정신을 가다듬고, 눈앞에 있는 것이 이음새 하나 없는 거대한 거울이라는 사실을 깨닫습니다. 당신은 거대한 거울을 빠르게 걷어차 [jitter]산산조각[/jitter] 냈습니다! 거울이 깨지는 순간, 당신의 거울상은 자신을 자책하듯이 눈부시게 빛나는 수천 개의 파편으로 흩어집니다. 어떻게 이런 공간이 존재할 수 있는 걸까요? | TOUCH_A_MIRROR: 당신은 손을 뻗어, 눈앞에 있는 거울상 속으로 녹아듭니다. [sine]어떤 게 나지?[/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `REFLECTIONS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Reflections`
- 리소스 경로: `res://images/events/reflections.png6`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 거머리를 떼어낸다: 체력을 [red]{RipHpLoss}[/red] 잃습니다. 무색 카드 보상을 1번 얻습니다.
  - 지식을 공유한다: 무작위 카드 [blue]{FromCardChoiceCount}[/blue]장 중 [blue]{CardChoiceCount}[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.
- 페이지 요약: INITIAL: [jitter]*푹*[/jitter] 당신의 머리 위쪽에 날카로운 통증이 스치고, 한 생각이 당신의 마음을 파고듭니다. [purple][sine]“지식을 공유하겠나???”[/sine][/purple] 당신은 어떻게 해야 할지 확신이 서지 않습니다... | RIP: 당신은 [jitter]머리에서 거머리를 격렬하게 뜯어낸 뒤[/jitter] 저 멀리 던져버렸습니다! 머릿속에서는 목소리가 들려옵니다: [purple][sine]“안돼애애애애애애애애애애애애애애....” | SHARE_KNOWLEDGE: [purple][sine]“이 얼마나 사치스럽게 학구적인, 미로와도 같은 사고인가! 두뇌의 양식이 될 만한 참된 지적 풍요의 향연이로다!”[/sine][/purple]. 만족한 듯한 그 생명체는, 머리에서 튀어나와 기어가 버립니다. 당신은 [blue]어안이 벙벙해졌습니다[/blue].
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BRAIN_LEECH`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.BrainLeech`
- 리소스 경로: `res://images/events/brain_leech.png`
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
- 구조 정보:
  - 페이지 키 수: 2
- 확인된 선택지:
  - 커다란 버섯
  - 향기로운 버섯
- 페이지 요약: BIG_MUSHROOM: 당신은 [orange]커다란 버섯[/orange]을 뜯어 먹습니다. 버섯의 속살은 탄탄하고 전분기가 느껴져, 먹고 나니 든든합니다. 버섯을 먹으면 먹을수록, 마치 버섯이 당신의 허기를 먹어치우고 있듯이, 오히려 배가 더 고파집니다. [sine]정말 맛있네요... 당신은 [red]식곤증[/red]에 빠져듭니다.[/sine] 이 폭식은 대가를 치를 것입니다. | FRAGRANT_MUSHROOM: 당신은 특히 [green]향기로운[/green], 은은한 조개향을 풍기는 작은 목질의 버섯을 한 입 맛봅니다... 에너지가 폭발하듯이 솟구쳐, 달리고 뛰며 [gold]격렬하게 몸을 단련[/gold]하고 싶어집니다! 그러다 이내, 짧은 삶에서 최후의 순간을 맞이하는 살아있는 곰팡이가 몸부림치며, [red][jitter]날카로운 통증[/jitter][/red]이 당신에게 몰려옵니다. | INITIAL: [sine]마지막으로 뭔가 먹었던 게 언제였을까요? ...잠깐, 이 환상적인 냄새는 뭐죠?[/sine] 향기를 따라가다 보니, 당신은 온갖 종류의 [green]맛있는 버섯들[/green]이 익어가고 있는 아늑한 야영지에 도착했습니다! 당신은 배가 너무 고픈 나머지, 이 버섯들을 먹어도 안전할지는 고려하지도 않습니다. (배가 너무 고파서 모험가의 시체가 있다는 건 깨닫지도 못했습니다)
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `HUNGRY_FOR_MUSHROOMS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.HungryForMushrooms`
- 리소스 경로: `res://images/events/hungry_for_mushrooms.png<v`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 박멸 기술 배우기: [gold]덱[/gold]에 [gold]{Card1}[/gold]을 추가합니다.
  - 짓누르기 기술 배우기: [gold]덱[/gold]에 [gold]{Card2}[/gold]를 추가합니다.
- 페이지 요약: DONE: 그는 아무 말없이 당신에게 고개를 끄덕인 뒤, 자리를 떠납니다. | EXTERMINATION: “좋아요... [gold]박멸 기술[/gold]을 배우고 싶으신 거군요. 알겠습니다...” 그는 잠시 생각에 잠겨, 이 기술을 당신에게 가장 수월하게 전할 수 있는 방법을 모색합니다... [jitter]“이게 바로 그 기술입니다! 보시죠!!”[/jitter] 그는 [green]살충제[/green] 캔을 들고 자신의 실력을 뽐냅니다. | INITIAL: 당신은 [jitter][red]난폭한 곤충 무리[/red][/jitter]를 정신없이 상대하던 와중, 당신의 옆에서 내내 함께 싸우고 있었던 [gold]강인하고 거친 한 전사 사내[/gold]를 뒤늦게 발견했습니다! 곤충들은 패배했다는 것을 깨닫고 흩어집니다. 당신은 곁에서 함께 싸웠던 전우를 돌아봅니다. “이 해충들을 박멸하기 위한 요령 하나 알려드릴까요?” 정말 정중하군요. 당신은 고개를 끄덕이고 그의 제안을 받아들였습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BUGSLAYER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Bugslayer`
- 리소스 경로: `res://images/events/bugslayer.png%db`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 무리: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 카드를 [blue]2[/blue]장 [gold]변화[/gold]시킵니다.
  - 외톨이: 최대 체력을 [green]{MaxHp}[/green] 얻습니다.
- 페이지 요약: GROUP: 당신이 재잘거리던 [aqua]변성체들[/aqua]에게 인사를 건내자, 그들은 결정화된 팔로 당신을 끌어안았습니다. [aqua]변성체들[/aqua]은 당신에게 자신들의 결합된 지식을 공유하고, [sine]변성 마법[/sine]을 주입해 당신의 본질을 변화시킵니다... 정말 사랑스러운 생명체들인 것 같네요. [red]변성체들 중 하나[/red]가 당신의 [gold]골드[/gold]를 [red]훔쳐갔습니다.[/red] | INITIAL: 당신은 [green]결정화된 나무들[/green]로 가득한 숲에 들어갔고, 나무들이 [jitter]격렬하게 떨기[/jitter] 시작합니다! 나무 사이에서 한 무리의 [aqua]변성체들[/aqua]이 [jitter]튀어나오며[/jitter], 당신에게 [sine]인사와 환영[/sine]의 말을 쏟아냅니다. [aqua]변성체들[/aqua] 중 한 마리는 구석에서 불안해하고 있는 것으로 보아, 다른 개체만큼 사교적이지는 않은 것 같습니다. 무리와 외톨이 중 누구에게 다가갈까요? | LONER: 당신은 외톨이 [aqua]변성체[/aqua]의 본질에 공감하며, 그에게 손을 뻗었습니다. 그는 당신에게 작은 크리스탈 과일을 선물했고, 당신은 관습에 따라 과일을 먹었습니다. 과일을 먹자 당신의 정신과 결의가 선명해집니다. 당신은 감사를 표한 뒤 길을 나아갔습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `MORPHIC_GROVE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.MorphicGrove`
- 리소스 경로: `res://images/events/morphic_grove.png`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 그래도 휴식한다: [green]체력을 모두 회복합니다.[/green] [red]수면 부족[/red]을 얻습니다.
  - 나무들을 베어낸다: 최대 체력을 [red]{MaxHpLoss}[/red] 잃습니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: INITIAL: 한적한 [gold]휴식 장소[/gold]를 발견한 당신은 [orange]불[/orange]을 피우고 잠시 휴식을 취합니다. 적어도, 그렇게 믿고 있었습니다. 불을 피우자 불길은 위로 치솟는 대신 [sine]옆으로[/sine] 퍼져가며, [purple]기름이 스며 나오는 나무들[/purple]이 모인 숲을 향해 번져 갑니다. 그럼에도 불구하고 휴식을 취할까요? | KILL: 나무들은 [purple]명백히 위협적[/purple]이었기 때문에, 당신은 나무들을 처리하기로 합니다. 며칠 후, 당신은 [red]재[/red]와 [purple]기름 찌꺼기[/purple]투성이가 됐습니다. 베어 넘긴 나무들에서 작은 섀의 영혼들이 떼를 지어 날아오르더니, 당신의 발치에 [gold]작은 상자[/gold] 하나를 떨어뜨립니다. [green]“나무들로부터 저희를 해방해 주셔서 감사드립니다... [sine]짹짹[/sine]!”[/green] 잠을 좀 더 자야할 것 같네요. | REST: 잇따른 모험으로 지친 당신은, 기묘한 상황들을 뒤로 한 채 잠을 청하기로 합니다. [orange]불길[/orange]의 포효와 타닥거림 속에서, 당신은 밤새도록 뒤척이며 잠을 설칩니다. [purple]역겨운 나무들[/purple]에서 흘러내리는 새까만 수액은 끔찍한 악취를 내뿜고 있습니다. 당신은 잠에서 깨어났고, 체력은 회복됐지만 기운은 돌아오지 않았습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `UNREST_SITE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.UnrestSite`
- 리소스 경로: `res://images/events/unrest_site.png`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 빛의 문: 무작위 카드를 [blue]{Cards}[/blue]장 [gold]강화[/gold]합니다.
  - 어둠의 문: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다.
- 페이지 요약: DARK: [sine][purple]어둠 속으로...[/purple][/sine] 아무것도 보이지 않습니다. 하지만 보이지 않는 덕에... [red]쓸데없는 생각들[/red]이 떨어져 나가는 것 같습니다. 당신은 다시 [blue]지하 선착장[/blue]으로 돌아왔습니다. 어떻게 여기로? 어라...? | INITIAL: 방금 전까지만 해도 존재하지 않았던 출입구가 어느새 생겨나 있습니다... 안으로 들어서자, 희미하게 빛나는 두 개의 문과 [gold]잘 차려입은 문지기[/gold]가 보입니다. “드디어, 방문객이시군요! 제발 부디! 문을 선택해주세요, 아무거나요!” [blue]여긴 아직 첨탑 안인 걸까요? 어떻게 이렇게 깨끗할 수가 있죠?[/blue] | LIGHT: [sine][gold]빛 속으로...[/gold][/sine] [jitter]눈이 부셔옵니다!![/jitter] 하지만 그 빛이 당신을 [gold]강하게[/gold] 만듭니다. 당신은 다시 [blue]지하 선착장[/blue]으로 돌아왔습니다. 어떻게 여기로? 어라...?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `DOORS_OF_LIGHT_AND_DARK`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.DoorsOfLightAndDark`
- 리소스 경로: `res://images/events/doors_of_light_and_dark.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사람 크기 구멍의 들판

- ID: `megacrit-sts2-core-models-events-fieldofmansizedholes`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 거대한 들판이 갑작스럽게 눈앞에 펼쳐지고, 당신은 [jitter]사람들의 윤곽이 선명하게 새겨진 흔적들[/jitter]을 발견하고 걸음을 멈춥니다.

새겨진 윤곽 중 하나는 당신의 것과 일치합니다.

[sine][orange]완벽하네요...[/orange][/sine]
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 내 구멍에 들어간다: 카드 1장에 [purple]{Enchantment}[/purple]을 [gold]인챈트[/gold]합니다.
  - 저항한다: [gold]덱[/gold]에서 카드를 [blue]{Cards}[/blue]장 제거합니다. [gold]덱[/gold]에 [red]{ResistCurse}[/red]를 추가합니다.
- 페이지 요약: ENTER_YOUR_HOLE: 조금 비좁긴 하지만, 당신은 몸을 구겨 구멍 속으로 들어갑니다... 당신이 더 깊게 나아갈수록, 구멍 속은 [sine][purple]점점 어두워집니다[/purple][/sine]. [gold]희미하던 빛[/gold]은 끝내 점점 강해지고, 건너편의 모습이 드러납니다. 당신은 아무것도 변하지 않은 채로 구멍을 빠져나왔지만, [green]생명의 숨겨진 비밀[/green]을 깨달았습니다. | INITIAL: 거대한 들판이 갑작스럽게 눈앞에 펼쳐지고, 당신은 [jitter]사람들의 윤곽이 선명하게 새겨진 흔적들[/jitter]을 발견하고 걸음을 멈춥니다. 새겨진 윤곽 중 하나는 당신의 것과 일치합니다. [sine][orange]완벽하네요...[/orange][/sine] | RESIST: 당신은 모든 구멍을, 심지어 [orange]당신의 형상이 완벽하게 새겨진[/orange] 구멍조차도 지나쳐 갑니다... 왜 안으로 들어가지 않았을까요? 들어갔으면 무슨 일이 생겼을까요? 저 구멍에 들어간 적이 있었을까요? 누가 이 구멍들을 만든 거죠? [sine][red]이건 뭐... 누구... 어디?[/red][/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `FIELD_OF_MAN_SIZED_HOLES`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.FieldOfManSizedHoles`
- 리소스 경로: `res://images/events/field_of_man_sized_holes.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 사파이어 씨앗

- ID: `megacrit-sts2-core-models-events-sapphireseed`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 빛나는 날카로운 물체를 밟으려던 것을 가까스로 피합니다 ...씨앗? 특유의 색깔로 짐작해봤을 때, 분명 [aqua]사파이어 씨앗[/aqua]입니다! 이 엄청난 씨앗들은 분명 멸종했던 거 아니었나요!?

전설에 따르면 씨앗을 먹을 시 [gold]지구력이 크게 상승[/gold]한다고 합니다만, 씨앗을 먹는 대신 심고서 정성껏 기른다면 어떤 일이 벌어질까요?
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 먹는다: 체력을 [green]{Heal}[/green] 회복합니다. [gold]덱[/gold]에 있는 무작위 카드를 1장 [gold]강화[/gold]합니다.
  - 심고 영양을 공급한다: 카드 1장에 [purple]{Enchantment}[/purple]를 [gold]인챈트[/gold]합니다.
- 페이지 요약: EAT: [aqua]사파이어 씨앗[/aqua]을 삼키자, [blue]팔다리가 가벼워지고[/blue] [gold]세상의 색이 선명해지며[/gold], [purple][sine]주변에 있는 적들의 영혼이 느껴집니다[/sine][/purple]. 씨앗을 밟지 않길 잘했네요. | INITIAL: 당신은 빛나는 날카로운 물체를 밟으려던 것을 가까스로 피합니다 ...씨앗? 특유의 색깔로 짐작해봤을 때, 분명 [aqua]사파이어 씨앗[/aqua]입니다! 이 엄청난 씨앗들은 분명 멸종했던 거 아니었나요!? 전설에 따르면 씨앗을 먹을 시 [gold]지구력이 크게 상승[/gold]한다고 합니다만, 씨앗을 먹는 대신 심고서 정성껏 기른다면 어떤 일이 벌어질까요? | PLANT: 당신은 [green]과성장[/green] 지대를 샅샅이 뒤져, [blue]통기성이 좋은[/blue] 화분을 찾아냅니다. 그 후, 당신은 [orange]갈라진 벽 틈으로 빛이 스며드는[/orange] 아직 훼손되지 않은 장소를 찾아 주위를 돌아다닙니다. 당신은 조심스럽게 화분을 나무껍질과 덩굴로 보강해 [green]위장하고[/green], 비옥하고 영양분이 풍부한 흙을 채운 뒤, [aqua]씨앗[/aqua]을 지면 아래에 살며시 자리잡게 합니다. 마지막으로, 근처 식물에서 이슬을 모아 흙 위에 붓습니다. [sine]쑥쑥 자라라, 작은 씨앗아...[/sine] 당신도 모르는 사이에, [aqua]사파이어 씨앗[/aqua]이 당신의 영혼에 깃듭니다. 당신의 노고에 감사하고 있습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SAPPHIRE_SEED`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SapphireSeed`
- 리소스 경로: `res://images/events/sapphire_seed.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 샘터

- ID: `megacrit-sts2-core-models-events-wellspring`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [jitter]낮게 울리는 소리[/jitter]의 근원을 따라가던 당신은 고요한 샘 앞에 이르렀고, 그 샘의 모습에 매료됩니다. [sine][aqua]에메랄드빛 녹색[/aqua][/sine]의 물에는 [sine][gold]빛나는 알갱이[/gold][/sine]들이 모여 있습니다.

샘이 당신을 유혹하고 있는 듯합니다. 설마 무슨 문제라도 있겠어요?
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 몸을 담근다: [gold]덱[/gold]에서 카드를 [blue]1[/blue]장 제거합니다. [gold]덱[/gold]에 [red]죄책감[/red]을 [blue]{BatheCurses}[/blue]장 추가합니다.
  - 병에 담는다: 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 페이지 요약: BATHE: 당신은 미묘한 [red][sine]불안감을 느끼며[/sine][/red] 샘을 빠져 나왔습니다. 하지만 예상외로 상쾌한 기분이 듭니다. | BOTTLE: 이토록 [sine][aqua]아름다운 물[/aqua][/sine]에 몸을 담근다는 사실이 왠지 꺼림칙합니다… 당신은 그 대신 약간의 물을 병에 담아가기로 했습니다. 분명 쓸모가 있겠죠. | INITIAL: [jitter]낮게 울리는 소리[/jitter]의 근원을 따라가던 당신은 고요한 샘 앞에 이르렀고, 그 샘의 모습에 매료됩니다. [sine][aqua]에메랄드빛 녹색[/aqua][/sine]의 물에는 [sine][gold]빛나는 알갱이[/gold][/sine]들이 모여 있습니다. 샘이 당신을 유혹하고 있는 듯합니다. 설마 무슨 문제라도 있겠어요?
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WELLSPRING`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Wellspring`
- 리소스 경로: `res://images/events/wellspring.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 섀도니스 둥지

- ID: `megacrit-sts2-core-models-events-byrdonisnest`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 거대한 [jitter][red]비틀거리는 야수[/red][/jitter]가 부상을 입은 [green][sine]초록 섀[/sine][/green]를 쫓아내는 모습을 목격합니다.

섀가 도망친 오목한 공간 안에는, [gold]무방비한 상태의 알 하나[/gold]가 버려져 있습니다.

[sine]당신의 배가 꼬르륵거립니다…[/sine]
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 알을 가져간다: [gold]덱[/gold]에 [gold]{Card}[/gold]을 추가합니다.
  - 알을 먹는다: 최대 체력을 [blue]{MaxHp}[/blue] 얻습니다.
- 페이지 요약: EAT: 당신은 알을 깨고서 개걸스럽게 먹어치웠습니다. 좋은 아침거리였네요. | INITIAL: 당신은 거대한 [jitter][red]비틀거리는 야수[/red][/jitter]가 부상을 입은 [green][sine]초록 섀[/sine][/green]를 쫓아내는 모습을 목격합니다. 섀가 도망친 오목한 공간 안에는, [gold]무방비한 상태의 알 하나[/gold]가 버려져 있습니다. [sine]당신의 배가 꼬르륵거립니다…[/sine] | TAKE: 당신은 나중에 쓸모있는 무언가가 [green]부화[/green]할지도 모른다고 생각하며, 조심스럽게 알을 들어 올려 챙겼습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `BYRDONIS_NEST`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ByrdonisNest`
- 리소스 경로: `res://images/events/byrdonis_nest.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 속삭이는 골짜기

- ID: `megacrit-sts2-core-models-events-whisperinghollow`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [red]죽은 나무들로 이뤄진 골짜기[/red]를 지나던 중, 우연히 뼈처럼 새하얀 색의 나무 한 그루를 발견합니다. 무언가를 보호하는 갈비뼈처럼 안쪽으로 휘어진 가지에는, 점토 장식이 매달려 있습니다.

정말 [purple]소름끼치는 나무[/purple]입니다. 나무는 속삭입니다.

[sine][blue]...거래하라.....[/blue][/sine]
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 골드를 거래한다: [gold]골드[/gold]를 [red]{Gold}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다.
  - 나무를 끌어안는다: 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]변화[/gold]시킬 카드를 1장 선택합니다.
- 페이지 요약: GOLD: 당신은 나무 밑동 근처에 [gold]골드[/gold]를 몇 개 흩뿌렸고, 점토로 이뤄진 통 두 개가 아무런 소리 없이 바닥에 떨어집니다. 통은 부서진 뒤 열렸고, 안에는 [green]굉장히 온전한 상태의 포션들[/green]이 들어 있었습니다. 당신은 포션을 낚아챈 뒤 도망쳤습니다. | HUG: 나무는 자신에게 [gold]골드[/gold]를 넘기라고 [blue][sine]속삭였지만[/sine][/blue], 당신은 그 대신 나무를 강하게 끌어안았습니다! 나무는 [red][jitter]몸부림치며[/jitter][/red] 나뭇가지로 당신을 긁어댔지만, 이윽고 얌전해집니다. [green]나무를 끌어안는 경험[/green]은 당신을 [gold]변화[/gold]시켰습니다. [purple][sine]...포옹한다.....?[/sine][/purple] | INITIAL: 당신은 [red]죽은 나무들로 이뤄진 골짜기[/red]를 지나던 중, 우연히 뼈처럼 새하얀 색의 나무 한 그루를 발견합니다. 무언가를 보호하는 갈비뼈처럼 안쪽으로 휘어진 가지에는, 점토 장식이 매달려 있습니다. 정말 [purple]소름끼치는 나무[/purple]입니다. 나무는 속삭입니다. [sine][blue]...거래하라.....[/blue][/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WHISPERING_HOLLOW`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WhisperingHollow`
- 리소스 경로: `res://images/events/whispering_hollow.pngX`
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
- 구조 정보:
  - 페이지 키 수: 1
  - 선택지 키 수: 2
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
- 리소스 경로: `res://images/events/crystal_sphere.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 쓰레기 더미

- ID: `megacrit-sts2-core-models-events-trashheap`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 [red][jitter]고철이 된 무기[/jitter][/red]와 [orange]버려진 장신구[/orange], 그리고 [purple][sine]다른 기묘한 물건들[/sine][/purple]이 산처럼 쌓여있는 거대한 더미를 발견합니다. 거대한 더미는 안쪽에서부터 커지고 있는 듯이 움직이며 진동합니다...

쓰레기 더미의 표면을 뒤져보면 쓸 만한 물건 몇 개는 건질 수도 있습니다. 하지만 저 더미 안으로 들어간다면, [gold]신기한 보물[/gold]을 찾아낼 수 있을지도 모릅니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 뛰어든다: 체력을 [red]{HpLoss}[/red] 잃습니다. 과거의 무작위 잊힌 [gold]유물[/gold]을 1개 얻습니다.
  - 아무 고물이나 잡는다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 과거의 무작위 잊힌 카드를 1장 얻습니다.
- 페이지 요약: DIVE_IN: 당신은 꽤 오랜 시간 동안 [red][sine]위험천만한 쓰레기들[/sine][/red] 사이를 헤치며 나아간 끝에, [gold]오래된 유물[/gold] 하나를 찾아냅니다! 왜 이게 버려져 있던 걸까요? | GRAB: 당신은 안전한 거리를 유지한 채 더미 주위를 돌며, 여기저기 흩어진 [gold]잊힌 귀중품[/gold]과 [purple][sine]기묘한 물건들[/sine][/purple]을 주워 담습니다. 애초에 이것들은 왜 만들어진 걸까요? | INITIAL: 당신은 [red][jitter]고철이 된 무기[/jitter][/red]와 [orange]버려진 장신구[/orange], 그리고 [purple][sine]다른 기묘한 물건들[/sine][/purple]이 산처럼 쌓여있는 거대한 더미를 발견합니다. 거대한 더미는 안쪽에서부터 커지고 있는 듯이 움직이며 진동합니다... 쓰레기 더미의 표면을 뒤져보면 쓸 만한 물건 몇 개는 건질 수도 있습니다. 하지만 저 더미 안으로 들어간다면, [gold]신기한 보물[/gold]을 찾아낼 수 있을지도 모릅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `TRASH_HEAP`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TrashHeap`
- 리소스 경로: `res://images/events/trash_heap.png`
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
- 구조 정보:
  - 페이지 키 수: 2
- 확인된 선택지:
  - 수비를 합친다: [gold]수비[/gold]를 [blue]2[/blue]장 제거합니다. [gold]{Card2}[/gold]를 [gold]덱[/gold]에 추가합니다.
  - 타격을 합친다: [gold]타격[/gold]을 [blue]2[/blue]장 제거합니다. [gold]{Card1}[/gold]을 [gold]덱[/gold]에 추가합니다.
- 페이지 요약: COMBINE_DEFENDS: [b][jitter]깡! 깡!!![/jitter][/b] [orange]융합자[/orange]가 자신의 뼈 모루를 내려칩니다. 한 번의 망치질마다, 전투와 방어, 수비적인 전략에 관한 기억들이 점점 더 선명해집니다. 당신이 지닌 기술의 극치가 당신에게 돌아옵니다. | COMBINE_STRIKES: [b][jitter]깡! 깡!!![/jitter][/b] [orange]융합자[/orange]가 자신의 뼈 모루를 내려칩니다. 한 번의 망치질마다, 전투와 공격, 공격적인 전략에 관한 기억들이 점점 더 선명해집니다. 당신이 지닌 기술의 극치가 당신에게 돌아옵니다. | INITIAL: [b][jitter]깡! 깡!!![/jitter][/b] 벽 건너편에서 금속과 금속이 부딪히는 소리가 울려 퍼집니다... 당신이 벽에 머리를 기대고 귀를 기울이려는 찰나—벽이 갈라지듯 열렸고, [orange]여섯 개의 팔을 지닌 거구의 인물[/orange]이 작업에 몰두하고 있는 광경이 드러납니다. 그의 “얼굴”이라 불릴 만한 곳에는, [gold][sine]빛나는 문양들이 뒤엉켜 소용돌이[/sine][/gold]치고 있습니다. “[aqua]승천하는 영혼[/aqua]을 지닌 자가 내 공방에 찾아온 건가? 좋아, [jitter]결합이다[/jitter]!”
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AMALGAMATOR`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.Amalgamator`
- 리소스 경로: `res://images/events/amalgamator.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 이거 아님 저거?

- ID: `megacrit-sts2-core-models-events-thisorthat`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 근처의 구멍에서 갑자기, [gold]보물이 담긴 수상한 자루[/gold]와 명백히 [purple]저주받은 유물[/purple]을 움켜진 손이 튀어나옵니다.

[jitter][blue]"이거... 아님 저거?"[/blue][/jitter]
날카롭게 긁어대는 목소리가 아래쪽에서 속삭입니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 이거: 체력을 [red]{HpLoss}[/red] 잃습니다. [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다.
  - 저거: [red]{Curse}[/red]을 [gold]덱[/gold]에 추가합니다. 무작위 [gold]유물[/gold]을 1개 얻습니다.
- 페이지 요약: INITIAL: 근처의 구멍에서 갑자기, [gold]보물이 담긴 수상한 자루[/gold]와 명백히 [purple]저주받은 유물[/purple]을 움켜진 손이 튀어나옵니다. [jitter][blue]"이거... 아님 저거?"[/blue][/jitter] 날카롭게 긁어대는 목소리가 아래쪽에서 속삭입니다. | ORNATE: 당신은 [purple]저주받은 유물[/purple]을 움켜쥐었습니다. 유물을 움켜쥐는 순간, 당신의 손에 수갑이 채워집니다. 마치 [blue][sine]유령이 내는 듯한 웃음소리[/sine][/blue]와 함께 눈부신 [orange]불꽃과 연기[/orange]가 당신의 시야를 가득 채웁니다! 구멍과 팔은 사라져버렸습니다. | PLAIN: 당신이 보물 자루를 움켜쥐는 순간, [blue][jitter]세 번째 손[/jitter][/blue]이 나타나 당신을 붙잡습니다!! [red][jitter]한바탕 몸싸움[/jitter][/red]을 벌이고 나서야 세 번째 손은 당신을 놓았고—팔과 구멍은 갑자기 사라져버렸습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THIS_OR_THAT`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.ThisOrThat`
- 리소스 경로: `res://images/events/this_or_that.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전설은 사실이었어

- ID: `megacrit-sts2-core-models-events-thelegendsweretrue`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 칠흑 같이 어두운 방 안으로 들어서자, 위에서 한 줄기 빛이 갑자기 어둠을 가르며 내려오는 동시에, 당신의 뒤쪽에서 문이 쾅하고 닫힙니다.

빛 속에서, 지도가 놓여있는 연단이 당신의 눈앞에 모습을 드러냅니다. 왜인지 강제로 가져가야 할 듯한 느낌이 듭니다.

수상하네요.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 지도를 가져간다: [gold]보물 지도[/gold]를 얻습니다.
  - 천천히 출구를 찾는다: 체력을 [red]{Damage}[/red] 잃습니다. 무작위 [gold]포션[/gold]을 [blue]1[/blue]개 생성합니다.
- 페이지 요약: INITIAL: 칠흑 같이 어두운 방 안으로 들어서자, 위에서 한 줄기 빛이 갑자기 어둠을 가르며 내려오는 동시에, 당신의 뒤쪽에서 문이 쾅하고 닫힙니다. 빛 속에서, 지도가 놓여있는 연단이 당신의 눈앞에 모습을 드러냅니다. 왜인지 강제로 가져가야 할 듯한 느낌이 듭니다. 수상하네요. | NAB_THE_MAP: 뭐, 가져간다고 해서 별 일 있겠어요? 당신이 지도를 손에 들자, 방 건너편에 새로운 출구가 모습을 드러냈습니다. | SLOWLY_FIND_AN_EXIT: 당신이 어둠 속을 조심스럽게 나아가며 탈출구를 찾던 도중, 방이 격렬하게 흔들리기 시작했습니다. 갑작스러운 굉음과 함께, 당신을 둘러싼 방 전체가 무너져 내립니다. 간신히 잔해 속에서 빠져나온 당신은, 방 안에 숨겨져 있었던 것으로 추정되는, 유용한 포션 병 하나를 발견했습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_LEGENDS_WERE_TRUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheLegendsWereTrue`
- 리소스 경로: `res://images/events/the_legends_were_true.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 전쟁사학자 레피

- ID: `megacrit-sts2-core-models-events-warhistorianrepy`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: [gold]보물 상자[/gold] 옆에 있는 [purple]매달린 철창[/purple] 안에, 한 학자가 갇혀 있습니다.

그 학자는 미친듯이 무언가를 써 내려가며, [sine][orange]“일회용 열쇠”[/orange][/sine]라는 말을 끊임없이 중얼대고 있습니다.

[purple]철창[/purple]이나 [gold]상자[/gold] 중 하나를 열 수 있을 것 같습니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 상자를 연다: [gold]랜턴 열쇠[/gold]를 잃습니다. 무작위 [gold]포션[/gold]을 [blue]2[/blue]개 생성합니다. 무작위 [gold]유물[/gold]을 [blue]2[/blue]개 얻습니다.
  - 철창을 연다: [gold]랜턴 열쇠[/gold]를 잃습니다. [gold]역사 강의서[/gold]를 얻습니다.
- 페이지 요약: INITIAL: [gold]보물 상자[/gold] 옆에 있는 [purple]매달린 철창[/purple] 안에, 한 학자가 갇혀 있습니다. 그 학자는 미친듯이 무언가를 써 내려가며, [sine][orange]“일회용 열쇠”[/orange][/sine]라는 말을 끊임없이 중얼대고 있습니다. [purple]철창[/purple]이나 [gold]상자[/gold] 중 하나를 열 수 있을 것 같습니다. | UNLOCK_CAGE: 철창을 열자 [green]레피[/green]는 당신에게 감사 인사를 전합니다. 그는 [gold]두꺼운 가죽 표지의 책[/gold] 한 권을 당신에게 내밉니다. “역사를 모르는 자는 그것을 되풀이할 수밖에 없는 운명입니다. 하지만 당신도 반복에 꽤나 익숙해 보이시는 것 같군요. 아닌가요?” | UNLOCK_CHEST: “이 학자, [green]레피[/green]를 풀어주지 않으실 건가요? 고전 철학에 따라, 당신은 [red]도덕적인 실패자[/red]로 여겨질 겁니다. 좋아요, 다음번에는 여기에 [purple][jitter]살아있는 사람[/jitter][/purple]이 갇혀있다고 생각해보시죠! 지금 여기 갇혀 있는 사회적으로 높은 가치를 지니지 않는 구성원이 전혀 [purple][jitter]곤란한 상황[/jitter][/purple]에 처해 있지 않다는 것처럼 말입니다!!” “자, 봐요, 이 철창이 얼마나 좁아터졌는지 알겠어요!? 내가 왜 불평하는지도? 이미 피상적이기 짝이 없는 결정을 내리셨군요... 그래, 상자 안에는 뭐가 있었습니까? [gold]포션[/gold]이랑 [gold]유물[/gold] 몇 개? 정말 대단한 것들이네요. [sine]멋지고 말고요. 큰 도움이 되겠어요...[/sine]” 당신이 떠나는 모습을 [green]레피[/green]가 경멸스럽게 지켜봅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `WAR_HISTORIAN_REPY`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.WarHistorianRepy`
- 리소스 경로: `res://images/events/war_historian_repy.png7`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정글 미로 탐험

- ID: `megacrit-sts2-core-models-events-junglemazeadventure`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 당신은 공터에서 [purple]거대한 미로[/purple]를 내려다보며 손짓하고 있는 [green]오합지졸 모험가 무리[/green]를 만났습니다.

모험가들은 당신에게, 이 미로의 전리품을 차지하기 위한 [b]장대한 모험[/b]에 함께 협력할 것을 제안합니다. 경험 많은 모험가인 당신은, 이 미로가 [red][jitter]치명적인 함정[/jitter][/red]과 [orange][sine]수호자[/sine][/orange]들로 가득 차 있음을 곧바로 깨닫습니다.

함께 나아가면 더 수월하겠지만, 얻는 전리품은 나눠야 할 것입니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 협력한다: [gold]골드[/gold]를 [blue]{JoinForcesGold}[/blue] 얻습니다.
  - 홀로 탐색한다: [gold]골드[/gold]를 [blue]{SoloGold}[/blue] 얻습니다. 체력을 [red]{SoloHp}[/red] 잃습니다.
- 페이지 요약: INITIAL: 당신은 공터에서 [purple]거대한 미로[/purple]를 내려다보며 손짓하고 있는 [green]오합지졸 모험가 무리[/green]를 만났습니다. 모험가들은 당신에게, 이 미로의 전리품을 차지하기 위한 [b]장대한 모험[/b]에 함께 협력할 것을 제안합니다. 경험 많은 모험가인 당신은, 이 미로가 [red][jitter]치명적인 함정[/jitter][/red]과 [orange][sine]수호자[/sine][/orange]들로 가득 차 있음을 곧바로 깨닫습니다. 함께 나아가면 더 수월하겠지만, 얻는 전리품은 나눠야 할 것입니다. | JOIN_FORCES: {IsMultiplayer:당신과 친구들을|당신을} 포함한 모든 사람들의 얼굴이 즉시 밝아집니다! 모험가들 중 한 명이 음유시인이라는 사실을 알게 됐습니다! 당신과 [red]그라토리안 전사[/red]가 [orange]가시 곤봉을 든 경비병[/orange]들을 무난하게 제거하는 동안, 함정을 손쉽게 무력화하는 [aqua]오징어 마법사 아퀴드[/aqua]와 함께 당신은 즐겁게 미로 속으로 걸어 들어갔습니다. 이번 모험은 당신의 인생에서 가장 즐거운 모험이었고, 정말로 [gold][sine]멋진 추억[/sine][/gold]을 만들었습니다. | SOLO_QUEST: 사람을 피하고 싶어진 {IsMultiplayer:당신과 친구들은|당신은}, 안 좋은 표정의 모험가들을 말없이 지나쳐 갑니다. 그들 중 한 명은 심지어 당신에게 [jitter][purple]주먹을 휘두르기도 합니다[/purple][/jitter]. 미로는 방문객을 거부합니다. 당신은 미로를 나아가는 동안 가시 함정에 빠지고, 거대한 가시 통나무에 맞고, [orange]가시 곤봉을 든 난폭한 생물[/orange]들과 전투를 펼쳤습니다. 가시라면 지긋지긋해진 당신은, 다음 모험에서는 팀워크를 고려해 보기로 합니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `JUNGLE_MAZE_ADVENTURE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.JungleMazeAdventure`
- 리소스 경로: `res://images/events/jungle_maze_adventure.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 정령 접합자

- ID: `megacrit-sts2-core-models-events-spiritgrafter`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 위쪽에서 고치가 [jitter]흔들리며 꿈틀거리고[/jitter], 금방이라도 터져 나올 것 같습니다!

...이윽고 꿈틀거림이 멈춥니다. [orange][sine]고치가 타오르기 시작합니다[/sine][/orange]! [jitter]무슨 일이 일어나고 있는 거죠?[/jitter]
번쩍이는 [gold]빛[/gold]과 [red]불꽃[/red] 속에서, [orange]정령 접합자[/orange]가 당신에게 뛰어들어 당신과 하나가 되어 완전해지려고 합니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 거부한다: 체력을 [red]{RejectionHpLoss}[/red] 잃습니다. [gold]덱[/gold]에서 카드를 1장 제거합니다.
  - 받아들인다: 체력을 [green]{LetItInHealAmount}[/green] 회복합니다. [gold]덱[/gold]에 [gold]탈바꿈[/gold]을 추가합니다.
- 페이지 요약: INITIAL: 위쪽에서 고치가 [jitter]흔들리며 꿈틀거리고[/jitter], 금방이라도 터져 나올 것 같습니다! ...이윽고 꿈틀거림이 멈춥니다. [orange][sine]고치가 타오르기 시작합니다[/sine][/orange]! [jitter]무슨 일이 일어나고 있는 거죠?[/jitter] 번쩍이는 [gold]빛[/gold]과 [red]불꽃[/red] 속에서, [orange]정령 접합자[/orange]가 당신에게 뛰어들어 당신과 하나가 되어 완전해지려고 합니다. | LET_IT_IN: [orange]정령 접합자[/orange]가 당신과 융합합니다. 생명력 넘치는 동족의 영혼이 당신을 빠르게 [green]치유합니다[/green]. 당신은 [blue]새롭게 태어났습니다[/blue]. | REJECTION: [orange]정령 접합자[/orange]는 당신의 굳은 의지에 의해 손쉽게 저지됩니다. 하지만 그것은 숙주를 갈망한 나머지, 계속해서 당신에게로 날아듭니다. 마침내 힘이 다한 그 정령은, [purple][sine]휙[/sine][/purple] 소리와 함께 사라집니다. [sine]덧없는 정령이네요.[/sine]
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SPIRIT_GRAFTER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SpiritGrafter`
- 리소스 경로: `res://images/events/spirit_grafter.pngui`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 잔뜩 먹는다: 무작위 [gold]일반[/gold] 카드 [blue]8[/blue]장 중 [blue]2[/blue]장을 선택해 [gold]덱[/gold]에 추가합니다.
  - 탐색한다: 체력을 [red]{Damage}[/red] 잃습니다. [gold]엄선된 치즈[/gold]를 얻습니다.
- 페이지 요약: GORGE: [b]치즈[/b]를 한 입 베어물자, 더 많은 [b]치즈[/b]를 먹고 싶은 욕구가 되려 솟아납니다. [sine]시간이 흘러갑니다...[/sine] 당신은 감탄이 절로 나올 양의 [b]치즈[/b]를 먹어치웠습니다. 오늘밤 꿈에는 [b]치즈[/b]가 나올 것 같네요. | INITIAL: 이 방은 [b][jitter]치즈[/jitter][/b]로 가득 차 있습니다!!! 함정이 없는지 주변을 살펴보다 위를 올려다보니...더 많은 [b]치즈[/b]가 있습니다. 이 방의 양초는 [b]치즈[/b]로 만들어져 있는 것으로 보입니다. 방의 가구도 [b]치즈[/b]로 만들어져 있습니다. 갑자기, 당신의 뒤에 있던 문이 쾅 하고 닫힙니다. 문도 [b]치즈[/b]로 만들어져 있습니다. | SEARCH: 당신은 [sine]몇 시간이고[/sine] 방 안을 뒤적이며 헤맵니다… [b]치즈[/b]들이 풍기는 압도적인 존재감 속에서 당신의 정신은 기묘한 곳을 떠돌며, 시간은 흘러가고 당신의 존재는 혼미해집니다... 깜짝 놀라 정신을 차리고 보니, 당신은 무언가를 쥐고 있다는 것을 깨닫게 됩니다. [b]엄선된 치즈[/b]입니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `ROOM_FULL_OF_CHEESE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.RoomFullOfCheese`
- 리소스 경로: `res://images/events/room_full_of_cheese.png`
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
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 검을 집는다: [gold]{Relic}[/gold]을 얻습니다.
  - 물로 뛰어든다: [gold]골드[/gold]를 [blue]{Gold}[/blue] 얻습니다. 체력을 [red]{HpLoss}[/red] 잃습니다.
- 페이지 요약: DIVE_INTO_WATER: 당신은 물속으로 뛰어들었습니다! [gold]봉헌물[/gold]을 챙기던 와중에 [sine][blue]둔탁한 소리[/blue][/sine]가 울려 퍼지고, 돌들이 날아와 당신을 때려대기 시작합니다! 수면 위로 올라오자, 당신의 간섭으로 인해 이 오래된 석상이 [red]물리적[/red]으로도 [purple]영적[/purple]으로도 붕괴되어버린 모습이 보입니다. | GRAB_SWORD: 조각상의 손에서 [blue]검[/blue]을 [jitter]빼내니[/jitter], 석상 전체가 한번에 무너져 내립니다! 연못 바닥에서 [gold][jitter]반짝거리던 것들[/jitter][/gold]을 지금 집으러 가기에는 너무 위험해 보입니다... [blue]검[/blue]을 자세히 살펴보니, 검 안에 [green]잠들어있는 놀라운 힘[/green]이 느껴집니다. | INITIAL: 당신은 연못에 반쯤 잠겨 있는 오래된 석상을 발견했습니다. 석상의 두 손은 [blue]돌 검[/blue] 위에 조심스레 얹혀 있습니다. 연못 바닥에는 무언가가 [gold][sine]반짝거리고[/sine][/gold] 있습니다. 누군가에게 바치기 위한 것들일까요? 저 돈들은 꽤나 도움이 될 것 같습니다...
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `SUNKEN_STATUE`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.SunkenStatue`
- 리소스 경로: `res://images/events/sunken_statue.png`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포션 배달원

- ID: `megacrit-sts2-core-models-events-potioncourier`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 공기 중에 퍼진 [sine]시큼한[/sine] 냄새를 깨달은 당신은, 이내 땅에 쓰러진 [gold]포션 배달원[/gold]을 발견했습니다.
미동도 하지 않고, 생기도 없습니다. [jitter]죽은 걸까요!?[/jitter]
그의 소지품은 이미 전부 빼앗긴 상태였지만, [green][sine]역겨운 악취를 풍기는 포션[/sine][/green] 한 묶음과 함께 쪽지 하나가 남아 있습니다: "수령인: 상인".
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 뒤져본다: 무작위 [gold]고급 포션[/gold]을 [blue]1[/blue]개 생성합니다.
  - 포션을 집는다: [gold]역겨운 포션[/gold]을 [blue]{FoulPotions}[/blue]개 생성합니다.
- 페이지 요약: GRAB_POTIONS: [blue]상인[/blue]은 이 포션들로 대체 뭘 하려는 걸까요? | INITIAL: 공기 중에 퍼진 [sine]시큼한[/sine] 냄새를 깨달은 당신은, 이내 땅에 쓰러진 [gold]포션 배달원[/gold]을 발견했습니다. 미동도 하지 않고, 생기도 없습니다. [jitter]죽은 걸까요!?[/jitter] 그의 소지품은 이미 전부 빼앗긴 상태였지만, [green][sine]역겨운 악취를 풍기는 포션[/sine][/green] 한 묶음과 함께 쪽지 하나가 남아 있습니다: "수령인: 상인". | RANSACK: 온전한 물약은 한 개뿐이었습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `POTION_COURIER`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.PotionCourier`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 혼돈의 향기

- ID: `megacrit-sts2-core-models-events-aromaofchaos`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 울창한 덤불을 헤치고 나와 공터에 다다른 당신은, 정체를 알 수 없는 그리움에 사로잡힙니다.

[purple]꽃 향기[/purple]와 [green]썩은 냄새[/green], 그리고 [orange]전혀 다른 어떤 향기[/orange]가 한데 뒤섞여 풍겨옵니다. 걸음을 옮길 때마다 향기는 점점 더 진해지고, 주변 세상이 [sine]일그러지고 뒤틀리는[/sine] 듯한 느낌이 듭니다.

[sine][rainbow freq=0.3 sat=0.8 val=1]변화하는 혼돈[/rainbow][/sine]의 감각에 압도당한 채, 당신은 점점 자신을 잃어가는 듯한 느낌에 사로잡힙니다.
- 구조 정보:
  - 페이지 키 수: 2
  - 선택지 키 수: 2
- 확인된 선택지:
  - 정신을 붙잡는다: [gold]덱[/gold]에 있는 카드를 1장 [gold]강화[/gold]합니다.
  - 향기에 몸을 맡긴다: [gold]덱[/gold]에 있는 카드를 1장 [gold]변화[/gold]시킵니다.
- 페이지 요약: INITIAL: 울창한 덤불을 헤치고 나와 공터에 다다른 당신은, 정체를 알 수 없는 그리움에 사로잡힙니다. [purple]꽃 향기[/purple]와 [green]썩은 냄새[/green], 그리고 [orange]전혀 다른 어떤 향기[/orange]가 한데 뒤섞여 풍겨옵니다. 걸음을 옮길 때마다 향기는 점점 더 진해지고, 주변 세상이 [sine]일그러지고 뒤틀리는[/sine] 듯한 느낌이 듭니다. [sine][rainbow freq=0.3 sat=0.8 val=1]변화하는 혼돈[/rainbow][/sine]의 감각에 압도당한 채, 당신은 점점 자신을 잃어가는 듯한 느낌에 사로잡힙니다. | LET_GO: 당신은 굴복했고, 정신이 [sine][purple]흐릿해지기[/purple][/sine] 시작합니다… [blue]친구들과 어울리며[/blue] [green]팀을 이뤄 싸우고[/green], [gold]엄청난 적들과 싸우는[/gold] 당신의 모습이 눈앞에 아른거립니다. 이건 꿈일까요? 현실일까요? 아니면 둘 다일까요? 수없이 많은 눈들이 당신과 당신의 꿈을 인지합니다. 그들은 누구일까요...? 얼마인지 알 수 없는 시간이 흐른 뒤, 당신은 깨어납니다. 향기는 점차 희미해지고, 의식이 선명해짐에 따라 그 효과도 사라집니다. 안정을 취하고 나니, 그 어느 때보다도 머리가 맑아진 느낌이 듭니다. [sine][red]그 눈들은 뭐였을까요?[/red][/sine] | MAINTAIN_CONTROL: 당신은 굴복하고 싶다는 유혹을 억누르며 향기가 닿지 않는 나무 위로 올라갔지만, 풍겨오는 향기의 힘은 여전히 당신의 마음을 파고듭니다. 제정신을 붙잡고 있기 힘들지만, 당신은 자신의 생각을 스스로의 [blue]핵심 원칙[/blue]에 단단히 묶어둡니다. {AromaPrinciple} {AromaPrinciple} {AromaPrinciple} 향기가 가라앉습니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `AROMA_OF_CHAOS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.AromaOfChaos`
- 리소스 경로: `res://images/events/aroma_of_chaos.png*`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### 포션의 미래?

- ID: `megacrit-sts2-core-models-events-thefutureofpotions`
- 그룹/풀 추정: 선택지 확인됨, 페이지 본문 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 한국어 또는 영어 설명 본문까지 연결된 상태입니다.
- 핵심 설명: 모퉁이를 도는 순간 희미한 진동이 느껴지고, 거대한 회전 장치가 당신의 눈앞에 모습을 드러냅니다!
여러 개의 투입구가 있어, 액체를 고도로 압축해 섭취할 수 있는 알약으로 바꾸는 구조의 장치인 것으로 보입니다.

이 건강 음료들을 작은 알약 한 알로 바꾼다는 발상은 영 꺼림칙하지만, 가끔은 새로운 시도를 해보는 것도 나쁘지 않을지도 모릅니다.
- 구조 정보:
  - 페이지 키 수: 1
  - 선택지 키 수: 1
- 확인된 선택지:
  - {Rarity} 포션을 삽입한다: [gold]{Potion}[/gold]을(를) 잃습니다. [gold]강화된[/gold] [gold]{Rarity} {Type}[/gold] 카드를 얻습니다.
- 페이지 요약: DONE: 알약은 역겨운 맛이 났습니다. 미래에는 희망이 없네요. | INITIAL: 모퉁이를 도는 순간 희미한 진동이 느껴지고, 거대한 회전 장치가 당신의 눈앞에 모습을 드러냅니다! 여러 개의 투입구가 있어, 액체를 고도로 압축해 섭취할 수 있는 알약으로 바꾸는 구조의 장치인 것으로 보입니다. 이 건강 음료들을 작은 알약 한 알로 바꾼다는 발상은 영 꺼림칙하지만, 가끔은 새로운 시도를 해보는 것도 나쁘지 않을지도 모릅니다.
- 관찰 로그 반영: 아니오
- 주요 소스: `localization-scan`
- 선호 locale: `kor`
- L10N key: `THE_FUTURE_OF_POTIONS`
- 모델 클래스: `MegaCrit.Sts2.Core.Models.Events.TheFutureOfPotions`
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
- 리소스 경로: `res://scenes/events/custom/fake_merchant.tscn!`
- 추출 파일 힌트: `localization/cjk/ | localization/kor/ | localization/latin/ | localization/unknown/`

### Observed Event

- ID: `event-00c8f2a30a9e`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 몸통 박치기+, 부메랑 칼날+, 불타는 혈액, 사혈+
- 확인된 선택지:
  - 몸통 박치기+
  - 부메랑 칼날+
  - 불타는 혈액
  - 사혈+
  - 은 도가니
  - 이중 타격+
  - 잿불+
  - 진정한 끈기+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-021a374e4202`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. CardGrid, NSimpleCardSelectScreen, 녹아내리는 주먹+, 분노+
- 확인된 선택지:
  - CardGrid
  - NSimpleCardSelectScreen
  - 녹아내리는 주먹+
  - 분노+
  - 불타는 혈액
  - 완벽한 타격+
  - 은 도가니
  - 전투장비+
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-20d7f613c413`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@524, @Control@525, Back, Deck
- 확인된 선택지:
  - @Control@524
  - @Control@525
  - Back
  - Deck
  - Dismisser
  - Exclaim
  - Map
  - PotionHolder
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-22965ba8955b`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. @Control@524, Back, Deck, Dismisser
- 확인된 선택지:
  - @Control@524
  - Back
  - Deck
  - Dismisser
  - Exclaim
  - Highlight
  - Map
  - Question
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-4c3a3623500a`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. Back, CardGrid, NSimpleCardSelectScreen, 녹아내리는 주먹+
- 확인된 선택지:
  - Back
  - CardGrid
  - NSimpleCardSelectScreen
  - 녹아내리는 주먹+
  - 분노+
  - 불타는 혈액
  - 완벽한 타격+
  - 은 도가니
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-156b67178f47`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 니오우의 비탄, 은 도가니, 정밀한 가위
- 확인된 선택지:
  - 니오우의 비탄
  - 은 도가니
  - 정밀한 가위
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-921123c82e3e`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 저주받은 진주, 정밀한 가위, 포맨더
- 확인된 선택지:
  - 저주받은 진주
  - 정밀한 가위
  - 포맨더
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-4210b7289309`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 거머리를 떼어낸다, 지식을 공유한다
- 확인된 선택지:
  - 거머리를 떼어낸다
  - 지식을 공유한다
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-f9e6455241fd`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 잔뜩 먹는다, 탐색한다
- 확인된 선택지:
  - 잔뜩 먹는다
  - 탐색한다
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-23a9f0224cc4`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 계속
- 확인된 선택지:
  - 계속
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-52e94db6f68c`
- 그룹/풀 추정: 선택지 확인됨
- 플레이 중 참조 시점: 이벤트 본문과 선택지 비교 시 참조
- 설명 상태: 선택지 또는 페이지 단위 정보까지는 연결되었지만, 전체 설명 완성도는 더 다듬어야 합니다.
- 핵심 설명: 선택지 정보는 확인되었지만 상세 설명 본문은 아직 비어 있습니다. 진행
- 확인된 선택지:
  - 진행
- 관찰 로그 반영: 예
- 주요 소스: `live-event:choice-list-presented`

### Observed Event

- ID: `event-038d71dc0b75`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-16cb90063d12`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-265c6e8061f7`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-26e6a7ca7618`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-40b5da66bab4`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-8be3ce008286`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-adabf1d956de`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

### Observed Event

- ID: `event-bdd840a24101`
- 그룹/풀 추정: 미분류
- 플레이 중 참조 시점: 이벤트 방 제목/본문 파악 시 참조
- 설명 상태: 플레이 해석에 필요한 정보가 아직 충분하지 않습니다.
- 핵심 설명: 현재 항목은 정적 후보로만 식별되었고, 플레이 의미를 해석할 근거가 부족합니다.
- 관찰 로그 반영: 예
- 주요 소스: `live-event:event-screen-opened`

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

