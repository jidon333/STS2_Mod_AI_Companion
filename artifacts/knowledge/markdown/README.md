# 정적 분석 Markdown 리포트

이 폴더는 `artifacts/knowledge` 아래 JSON 산출물을 사람이 읽기 쉬운 마크다운 형태로 재구성한 것입니다.
핵심 목적은 `실제 플레이 중 AI가 무엇을 알 수 있는지`, `어느 항목이 L10N 본문까지 연결되었는지`, `아직 무엇이 교차 검증 대상인지`를 빠르게 파악하는 것입니다.

## 현재 상태 요약

- 카드는 한국어 제목과 설명이 많이 채워져 있습니다.
- 이벤트는 제목, 페이지 본문, 선택지 정보가 부분적으로 연결되어 있습니다.
- 유물, 포션, 키워드, 상점 문구는 coverage가 계속 넓어지는 중이며, 실플레이 교차 검증이 남아 있습니다.
- 어시스턴트 AI가 직접 읽을 수 있는 JSON은 `catalog.assistant.json`과 `assistant/*.json`입니다.

## 산출물 위치

- `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\artifacts\knowledge`
- `catalog.latest.json` / `catalog.latest.txt`
- `catalog.assistant.json` / `catalog.assistant.txt`
- `assistant/cards.json`, `assistant/relics.json`, `assistant/events.json` ...

## 위치별 바로 보기

- [카드](./cards.md): 카드 보상 화면, 상점 구매 후보, 이벤트 특수 선택지, 현재 덩 구성 파악
- [유물](./relics.md): 유물 보상 화면, 상점 구매, 이벤트 보상, 현재 보유 유물 목록
- [포션](./potions.md): 전투 보상 화면, 상점 구매, 현재 포션 슬롯
- [이벤트](./events.md): 이벤트 방 진입, 선택지 표시, 페이지 본문 파악
- [상점](./shops.md): 상인 방 진입, 구매 후보 표시, 카드 제거 서비스
- [보상](./rewards.md): 전투 보상 화면, 이벤트 보상 화면, 카드 선택 UI
- [키워드/의도](./keywords.md): 카드 설명 해석, 상태 이상/버프 파악, 몬스터 Intent 해석

## 전체 수량

- 카드: 595 (설명 562, L10N 575)
- 유물: 296 (설명 285, L10N 285)
- 포션: 68 (설명 63, L10N 63)
- 이벤트: 62 (설명 57, L10N 58)
- 상점: 5 (설명 5, L10N 2)
- 보상: 11 (설명 7, L10N 7)
- 키워드/의도: 264 (설명 262, L10N 262)

## 파이프라인 상태

- `release-scan`: `completed`
- `decompile-scan`: `completed`
- `assembly-scan`: `completed`
- `pck-inventory`: `warning` / 경고 1건
- `strict-domain-parse`: `completed`
- `localization-scan`: `completed`
- `observed-merge`: `completed`
- `catalog-build`: `completed`

## 공통 경고

- Configured optional Godot executable: C:\\Tools\\Godot_v4.5.1-stable_win64\\Godot_v4.5.1-stable_win64_console.exe
