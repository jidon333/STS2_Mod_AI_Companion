# STS2 모딩을 처음부터 이해하기

이 문서는 Slay the Spire 2 모딩, 현재 저장소의 모드 구조, 외부 AI 조언 앱까지를 초보자 기준으로 설명합니다.

## 1. 이 저장소가 만들고 싶은 것

최종 목표는 `게임 외부에 떠 있는 AI 조언 앱`입니다.

중요한 점은 아래와 같습니다.

- 게임을 대신 플레이하지 않습니다.
- 입력 자동화나 멀티플레이 개입을 하지 않습니다.
- 게임 상태를 읽고, 선택지와 현재 상태에 대한 조언만 제공합니다.

## 2. 왜 모드가 필요한가

외부 앱이 게임 상태를 알아내려면 먼저 게임 안에서 상태를 읽어 밖으로 내보내는 통로가 필요합니다.

현재 선택한 경로는 아래입니다.

- 게임 안: native STS2 모드
- 게임 밖: live export 파일 감시

즉, 모드는 게임 안에서 상태를 읽고, 외부 앱은 파일만 읽습니다.

## 3. `.pck`와 `.dll`은 무엇인가

STS2 native 모드는 보통 아래 조합으로 배포됩니다.

- `.pck`
- `.dll`
- 필요 시 별도 runtime config JSON

여기서 핵심은 `.pck`와 `.dll`의 basename이 맞아야 한다는 점입니다.

예:

- `sts2-mod-ai-companion.pck`
- `sts2-mod-ai-companion.dll`

이 basename이 맞지 않으면 로더가 `.pck`를 찾고도 대응 `.dll`을 찾지 못할 수 있습니다.

## 4. Harmony는 어디에 쓰는가

모드가 게임 안에서 상태를 관찰하려면 게임 메서드의 특정 시점에 끼어들어야 합니다.

현재 저장소는 `Harmony`를 아래 원칙으로 씁니다.

- 읽기 전용 관찰만 수행
- 게임 로직 값은 바꾸지 않음
- RNG, 입력, 시간 흐름을 조작하지 않음
- 훅 메서드는 가능한 좁게 유지

즉, `게임을 바꾸는 모드`가 아니라 `게임을 관찰하는 모드`에 가깝습니다.

## 5. runtime exporter는 무엇을 하나

게임 안에서 수집한 정보는 즉시 파일로 나갑니다.

출력 파일은 4개입니다.

- `events.ndjson`
- `state.latest.json`
- `state.latest.txt`
- `session.json`

이 파일은 `%AppData%\SlayTheSpire2\steam\<steamAccountId>\modded\profile<index>\ai_companion\live` 아래에 생성됩니다.

## 6. 외부 앱은 무엇을 하나

외부 앱은 직접 게임 메모리를 읽지 않습니다.

대신 아래만 읽습니다.

- live export 파일
- 정적 지식 카탈로그

그리고 그 내용을 기반으로:

- 현재 화면 표시
- 플레이어 상태 표시
- 선택지 표시
- Codex 조언 요청
- advice artifact 저장

을 수행합니다.

## 7. 왜 snapshot / restore가 중요한가

live smoke를 돌리면 게임 `mods` 폴더와 modded profile이 영향을 받을 수 있습니다.

그래서 현재 저장소는 smoke 전에 아래 절차를 권장합니다.

1. `snapshot`
2. `deploy-native-package`
3. `prepare-live-smoke`
4. Steam URI로 게임 실행
5. `inspect-godot-log`, `inspect-live-export`
6. 필요 시 `restore-snapshot-state`

이 절차를 지켜야 실험하다가 상태가 꼬여도 되돌릴 수 있습니다.

## 8. 초보자가 꼭 기억해야 하는 현재 경계

- 외부 조언 앱이 목표입니다.
- 동료 AI, 자동 플레이, 입력 주입은 현재 범위 밖입니다.
- direct exe 실행은 피하고 Steam URI를 사용합니다.
- exporter는 read-only입니다.
- live smoke 전 snapshot은 사실상 필수입니다.
