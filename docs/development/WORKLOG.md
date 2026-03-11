# 작업 기록

이 문서는 “무엇을 만들었는가”보다 “어떤 순서로 문제를 좁혀 왔는가”를 짧게 기록합니다.

## 1. 저장소 정리

- 로컬 멀티 에이전트 오케스트레이션 레이어를 제거했습니다.
- 저장소 중심을 다시 `모드 구현 + exporter + packaging + snapshot/restore + live smoke + knowledge extraction`으로 맞췄습니다.
- README, architecture, roadmap, boundaries, development 문서를 이 구조에 맞게 다시 정리했습니다.

## 2. runtime exporter 기초 구축

- live export 4종을 만드는 런타임 경로를 추가했습니다.
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- atomic writer, state tracker, summary formatter, replay/self-test 기반을 붙였습니다.

## 3. Harmony startup failure 정리

- broad generic hook와 void postfix 문제 때문에 `PatchAll` startup failure가 발생하던 구간을 찾아 정리했습니다.
- startup identity와 hook summary를 남기게 해서, 실제로 어떤 DLL과 어떤 hook 세트가 배포됐는지 바로 확인할 수 있게 했습니다.
- main menu까지 live export가 다시 생성되는 상태를 확인했습니다.

## 4. 정적 지식 파이프라인 도입

- observed-only 경로에서 벗어나 `release-scan`, `assembly-scan`, `pck-inventory`, `observed-merge`, `catalog-build` 파이프라인을 붙였습니다.
- `catalog.latest.json`, `catalog.latest.txt`, `source-manifest.json`을 canonical 산출물로 고정했습니다.
- `assembly-scan.json`, `pck-inventory.json`, `observed-merge.json` 같은 중간 산출물도 보존하게 했습니다.

## 5. Host / WPF 뼈대 추가

- `Sts2AiCompanion.Host` 프로젝트를 추가했습니다.
- `Sts2AiCompanion.Wpf` 프로젝트를 추가했습니다.
- live export polling, knowledge slice, prompt pack, Codex CLI 호출, run artifact 저장의 기본 계약을 만들었습니다.

## 6. 카드 중심 Markdown 리포트 정리

- 정적 분석 결과를 사람이 읽기 쉬운 Markdown으로 다시 출력하게 했습니다.
- `artifacts/knowledge/markdown` 아래에 카드/유물/포션/이벤트/상점/보상/키워드 문서를 생성하도록 정리했습니다.
- 플레이 관점 설명이 빠졌던 문서를 다시 작성해, “실제로 게임 중 어디에서 이 정보가 쓰이는가”를 먼저 읽게 바꿨습니다.

## 7. 이번 단계: localization-scan 추가

이번 단계의 핵심은 여기입니다.

- `SlayTheSpire2.pck`에서 localization key/value를 직접 복구하는 `localization-scan` 단계를 추가했습니다.
- 전체 PCK를 통째로 메모리에 올리지 않고, UTF-8 스트리밍 스캔으로 `*.title`, `*.description`, `*.selectionScreenPrompt`를 찾도록 만들었습니다.
- 카드 seed는 assembly/pck inventory에서 가져오고, localization 결과는 canonical card entry에 병합되도록 만들었습니다.
- `catalog.latest.json` 카드 엔트리에 아래 속성이 들어가도록 바꿨습니다.
  - `l10nKey`
  - `preferredLocale`
  - `title`
  - `description`
  - `selectionScreenPrompt`
  - `englishTitle`
  - `englishDescription`
  - `sourceFileHint`
- `cards.md`는 이제 카드 이름뿐 아니라 실제 설명 본문을 보여 줍니다.

## 8. 이번 단계에서 드러난 실제 결과

- `localization-scan.json`이 새로 생성됩니다.
- inspect 기준 카드 localization 항목은 1700개대, 설명 본문 채워진 카드는 1500개대입니다.
- `Acrobatics` 같은 카드는 이제 모델 클래스와 카드 설명 본문이 함께 들어옵니다.
- 영어 fallback은 아직 공격적으로 믿지 않고, 잘못 섞인 locale은 `cjk`, `latin`, `unknown`으로 분리해 과신을 막는 방향으로 조정했습니다.

## 9. 이번 단계에서 추가한 테스트

- synthetic PCK-like 텍스트에서 카드 `title/description/selectionScreenPrompt`를 복구하는 self-test
- localization 결과를 기존 canonical card entry에 병합하는 self-test

## 10. 다음 단계

다음 핵심은 다시 runtime/live 쪽입니다.

1. reward / event / shop / rest / combat smoke
2. current choices 실증
3. WPF + Host + Codex의 실제 gameplay 연결 검증
4. 카드 외 영역으로 localization 확장

## 2026-03-11 작업 로그 메모

- StaticKnowledgeFormatting.cs를 전면 재작성해 generated markdown의 stale placeholder 문구를 제거했습니다.
- LocalizationKnowledgeScanner / catalog build 결과를 기준으로 유물, 포션, 이벤트, 상점, 키워드까지 L10N 연결 범위를 다시 점검했습니다.
- `artifacts/knowledge/assistant/` export와 `assistant/index.json` metadata/provenance를 보강했습니다.
- `inspect-static-knowledge` 기준 localization coverage를 문서에 반영했습니다.
