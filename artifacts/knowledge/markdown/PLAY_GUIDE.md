# 플레이 기준 읽기 가이드

이 문서는 `실제 게임 플레이 중` 어떤 화면에서 이 정적 지식 산출물을 참조하면 되는지를 설명합니다.

## 기본 원칙

- 카드, 유물, 포션, 이벤트, 상점 문구는 오프라인 지식으로 먼저 읽고, 실시간 state/latest 파일과 함께 보아야 합니다.
- 설명이 채워진 항목은 AI가 직접 그 본문을 참조할 수 있고, 설명이 비어 있는 항목은 구조 정보와 관찰 로그가 주요 근거가 됩니다.
- 실플레이 교차 검증은 reward/event/shop/rest/combat 화면을 지나며 이루어져야 합니다.

## 화면별 참조 순서

1. 메인 메뉴 / 런 시작: state.latest.txt, session.json, catalog.assistant.json 연결 상태 확인
2. 카드 보상: cards.md, assistant/cards.json, currentChoices 교차 확인
3. 이벤트: events.md, assistant/events.json, page/options 정보 확인
4. 상점 / 휴식: shops.md, rewards.md, currentChoices 와 설명 문구 확인
5. 전투 / 턴 시작: cards.md, relics.md, keywords.md 와 실시간 덱/유물 상태 함께 해석

## 현재 바로 사용할 수 있는 것

- 카드 설명 채움 항목: 562
- 유물 설명 채움 항목: 285
- 포션 설명 채움 항목: 63
- 이벤트 설명 또는 선택지 확인 항목: 76

## 아직 남은 것

- 모든 항목이 플레이 결과까지 100% 검증된 것은 아닙니다.
- 버프, 키워드, 상점 규칙, 특수 이벤트 분기는 실플레이 교차 검증으로 계속 다듬어야 합니다.
- 어시스턴트 AI는 항상 이 정적 카탈로그를 state.latest.* 와 함께 읽어야 합니다.
