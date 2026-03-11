# 작업 기록

이 문서는 저장소가 현재 상태까지 오면서 어떤 순서로 바뀌었는지 압축해 기록합니다. 세부 root cause와 실패 로그는 `DETAILED_INVESTIGATION_LOG.md`가 담당하고, 이 문서는 큰 흐름을 요약합니다.

## 1. 저장소 정리

- 로컬 멀티 에이전트 오케스트레이션 레이어를 걷어냈습니다.
- 저장소 중심을 `모드 구현 + exporter + packaging + snapshot/restore + live smoke + knowledge extraction`으로 되돌렸습니다.

## 2. runtime exporter 기초 구현

- live export 4종을 기록하는 구조를 붙였습니다.
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- writer queue, state tracker, summary formatter, replay/self-test 기반을 만들었습니다.

## 3. Harmony startup failure 정리

- broad hook와 void postfix 문제로 발생하던 `PatchAll` startup failure를 제거했습니다.
- runtime identity / hook summary 로그를 넣고 main menu까지 live export가 다시 생성되는 상태를 확인했습니다.

## 4. 정적 지식 파이프라인 1차

- `release-scan`, `assembly-scan`, `pck-inventory`, `observed-merge`, `catalog-build`를 만들었습니다.
- `catalog.latest.*`와 intermediate 산출물 구조를 고정했습니다.

## 5. Host / WPF 추가

- `Sts2AiCompanion.Host`와 `Sts2AiCompanion.Wpf`를 추가했습니다.
- live export polling, knowledge slice, prompt pack, Codex CLI 연동, advice artifact 저장, WPF 표시 경로를 만들었습니다.

## 6. 사람이 읽는 markdown / AI export 분리

- `artifacts/knowledge/markdown` 아래 사람용 리포트를 만들었습니다.
- `catalog.assistant.json`, `assistant/*.json` 등 AI 전용 산출물을 분리했습니다.

## 7. localization-scan 도입

- `SlayTheSpire2.pck`에서 localization key/value를 직접 복구하는 `localization-scan`을 추가했습니다.
- 카드 seed와 localization을 병합해 title / description / selection prompt를 canonical entry에 붙였습니다.
- 이후 범위를 relic / potion / event / shop / reward / keyword까지 넓혔습니다.

## 8. Spire Codex 방법론 반영

- broad substring match 대신 strict domain parser 방향으로 전환했습니다.
- `spire-codex`를 참고해 모델 클래스 기반 canonical seed 생성, localization merge, 이벤트 `pages + options` 구조 반영 방식을 정했습니다.
- 구현은 그대로 가져오지 않고, 방법론만 C# pipeline으로 옮겼습니다.

## 9. decompile-scan + strict-domain-parse

- `ilspycmd` 기반 `decompile-scan`을 추가했습니다.
- 실제 모델 소스 기준 strict parser를 추가해 cards / relics / potions / events를 canonical로 정리했습니다.
- 이후 shops / rewards / keywords도 strict semantic canonical로 정리했습니다.

## 10. knowledge 품질 정제

- 카드 canonical에서 `OPTION_*`, `*_BUTTON`, `*_POWER` 같은 잡음을 제거했습니다.
- CJK 잘못 붙음 문제를 줄이기 위해 title fallback 우선순위를 보정했습니다.
- shops / rewards / keywords도 broad raw key가 아니라 strict semantic entity만 남도록 다시 정리했습니다.

## 11. gameplay smoke 관련 보강

- reward / event / rest / shop hook가 실제로 observed되는 상태를 확인했습니다.
- `currentChoices` 추출 범위를 넓혀 hand / reward / option / UI child를 더 깊게 읽도록 보강했습니다.
- `LocString`, `MegaLabel`, `MegaRichTextLabel`, `RichTextLabel`, `Text`, `BbcodeText`에서 사람이 읽는 문자열을 뽑는 경로를 추가했습니다.
- overlay 화면이 `RoomType=Combat` 같은 underlying state보다 우선되도록 screen resolution을 수정했습니다.

## 12. Codex / Host 보강

- `CodexCliClient`에 UTF-8 stdin/stdout/stderr 인코딩을 강제했습니다.
- prompt sanitize 경로를 추가해 invalid UTF-8 degraded를 줄이도록 했습니다.
- `analyze-live-once` 경로를 추가했습니다.
- `CompanionHost`는 manual advice 생성 직후 snapshot을 다시 publish하게 했습니다.

## 13. 현재 기준 결론

- manual advice: 성공
- exporter startup: 정상
- high-value hook observed: 확인
- automatic advice after UTF-8 fix: 재검증 필요
- run-scoped Codex session tracking: 미완료

즉 지금 단계의 핵심 병목은 정적 지식이 아니라 `실제 gameplay high-value 화면에서 auto advice와 session tracking을 닫는 것`입니다.
