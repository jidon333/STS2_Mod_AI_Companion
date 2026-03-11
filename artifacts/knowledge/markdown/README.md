# 정적 분석 리포트

이 폴더는 `artifacts/knowledge` 아래의 JSON 산출물을 사람이 읽기 쉬운 Markdown으로 다시 정리한 결과입니다.

## 생성 정보

- 생성 시각: `2026-03-10 23:59:27 +00:00`
- 게임 버전: `v0.98.3`
- 릴리즈 커밋: `cb602cef`
- 지식 루트: `C:\Users\jidon\source\repos\STS2_Mod_AI_Companion\artifacts\knowledge`

## 해석 주의점

- 현재 리포트는 `assembly-scan`, `pck-inventory`, `observed-merge` 결과를 합쳐서 만듭니다.
- 따라서 모든 항목이 실제 플레이로 검증된 것은 아닙니다.
- `관찰 여부: 예` 인 항목이 가장 신뢰도가 높고, 그 외 항목은 정적 후보로 봐야 합니다.
- 카드/유물/이벤트의 효과 텍스트는 아직 일부만 확보됩니다. 효과가 없다고 표시되는 것은 데이터가 없어서이지 기능이 없다는 뜻이 아닙니다.

## 전체 수량

- 카드: 전체 `6635` / 사람이 읽기 좋은 후보 `890` / 실제 관찰 `0`
- 유물: 전체 `2366` / 사람이 읽기 좋은 후보 `540` / 실제 관찰 `0`
- 포션: 전체 `1078` / 사람이 읽기 좋은 후보 `156` / 실제 관찰 `0`
- 이벤트: 전체 `1411` / 사람이 읽기 좋은 후보 `135` / 실제 관찰 `0`
- 상점: 전체 `417` / 사람이 읽기 좋은 후보 `2` / 실제 관찰 `0`
- 보상: 전체 `288` / 사람이 읽기 좋은 후보 `2` / 실제 관찰 `0`
- 키워드: 전체 `1758` / 사람이 읽기 좋은 후보 `89` / 실제 관찰 `0`

## 파이프라인 단계

- `release-scan`: `completed`
- `assembly-scan`: `completed`
- `pck-inventory`: `warning` / 경고 1건
- `observed-merge`: `completed`
- `catalog-build`: `completed`

## 공통 경고

- Configured optional Godot executable: C:\\Tools\\Godot_v4.5.1-stable_win64\\Godot_v4.5.1-stable_win64_console.exe

## 개별 리포트

- [카드](./cards.md)
- [유물](./relics.md)
- [포션](./potions.md)
- [이벤트](./events.md)
- [상점](./shops.md)
- [보상](./rewards.md)
- [키워드](./keywords.md)
