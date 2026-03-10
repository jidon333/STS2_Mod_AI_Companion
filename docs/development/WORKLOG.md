# 작업 기록

이 문서는 큰 흐름 위주로 정리한 시간순 작업 기록입니다.

## 1. 저장소 정리와 기본 방향 고정

- 로컬 멀티 에이전트 오케스트레이션 레이어를 제거했습니다.
- 저장소 중심을 다시 `모드 구현 + 검증 + live smoke + exporter`로 맞췄습니다.
- snapshot / deploy / inspect / restore 파이프라인은 유지했습니다.

## 2. runtime exporter 기초 추가

- live export 4종 구조를 만들었습니다.
- runtime state tracker, atomic writer, summary formatter를 추가했습니다.
- smoke diagnostics를 넣어 startup failure와 deployed/live mismatch를 더 잘 보이게 했습니다.

## 3. Harmony startup failure 정리

- broad 후보군과 void postfix 경로 때문에 startup이 깨지는 문제를 발견했습니다.
- 훅 후보를 좁히고, startup identity / hook summary 로그를 정리하는 방향으로 바꿨습니다.
- main menu 기준선까지 live export가 다시 생성되는 상태를 확보했습니다.

## 4. static knowledge pipeline 추가

- observed-only 경로를 넘어서 release / assembly / pck 기반 정적 지식 추출을 추가했습니다.
- `extract-static-knowledge`, `inspect-static-knowledge`, `merge-observed-knowledge`를 정리했습니다.
- intermediate artifact도 남기도록 확장했습니다.

## 5. 외부 assistant backend 추가

- `Sts2AiCompanion.Host` 프로젝트를 추가했습니다.
- live export polling, knowledge slice, prompt pack, Codex CLI 실행, artifact 저장을 구현했습니다.
- per-run artifact root를 `artifacts/companion/<run-id>/`로 고정했습니다.

## 6. WPF 조언 앱 추가

- `Sts2AiCompanion.Wpf` 프로젝트를 추가했습니다.
- 현재 상태, 조언, 선택지/이벤트/지식 슬라이스를 한 화면에 보이게 구성했습니다.
- `Analyze Now`, `Pause Auto Advice`, `Retry Last`, `Refresh Knowledge`, `Open Artifacts` 액션을 넣었습니다.

## 7. self-test 확장

- assistant config 로딩
- companion path resolver
- knowledge slice 선택
- prompt builder 계약

까지 self-test로 커버하도록 확장했습니다.

## 8. 2026-03-11 기준 상태

- 솔루션 빌드 통과
- self-test 통과
- static knowledge extract / inspect 통과
- high-value gameplay 화면 smoke는 아직 다음 단계
