# WPF 사용자 흐름

이 문서는 사용자가 실제로 보게 될 화면과 사용 흐름을 설명합니다.

## 1. 앱을 먼저 실행

사용자는 `STS2 AI Companion` WPF 앱을 먼저 실행할 수 있습니다.

이때 보이는 기본 상태:

- 상단: 연결 상태, run id, current screen, 마지막 갱신 시각
- 좌측: 플레이어 상태, 덱 요약, 유물/포션
- 중앙: AI 조언
- 우측: 현재 선택지, 최근 이벤트, 관련 지식

초기 상태 메시지는 `live export 대기`에 가깝습니다.

## 2. 게임 실행

게임을 Steam URI로 실행하고 모드가 정상 로드되면, 앱이 live export를 감지합니다.

이후 앱은 자동으로:

- 현재 run 상태 표시
- 현재 screen 표시
- 상태 갱신 시각 표시

를 시작합니다.

## 3. main menu

main menu가 잡히면 앱은 연결이 살아 있음을 보여 줍니다.

이 단계에서 사용자는:

- exporter가 살아 있는지
- 현재 profile/run이 무엇인지

정도를 확인할 수 있습니다.

## 4. 고가치 화면

reward, event, shop, rest, combat start 같은 화면이 잡히면 앱의 중앙 advice 영역이 자동으로 갱신되는 것이 목표입니다.

보여 줄 핵심 정보:

- 추천 선택 또는 추천 행동
- 짧은 요약
- 이유 2~5개
- 리스크 / 불확실성
- 관련 knowledge reference

## 5. 수동 분석

사용자는 `Analyze Now` 버튼을 눌러 현재 상태에 대한 즉시 분석을 다시 요청할 수 있습니다.

이 기능은 아래 상황에 유용합니다.

- 자동 advice가 아직 안 떴을 때
- 상태가 변했는데 다시 묻고 싶을 때
- Codex 오류 이후 재시도하고 싶을 때

현재 `Retry Last` 버튼도 있으며, 현재 구현에서는 `Analyze Now`와 같은 수동 advice 경로를 다시 호출합니다.

## 6. 자동 advice 일시정지

`Pause Auto Advice`는 자동 트리거를 끄고, 사용자가 수동으로만 분석을 요청하고 싶을 때 씁니다.

이 경우에도 live state 표시는 계속 유지됩니다.

## 7. artifact 열기

`Open Artifacts`를 누르면 현재 run 기준 artifact 폴더를 열 수 있습니다.

여기서 확인할 수 있는 것:

- prompt pack
- latest advice
- advice 로그
- live mirror
- host 상태

## 8. knowledge 새로고침

`Refresh Knowledge`는 live export와 현재 Host snapshot 기준 표시를 새로 읽어 화면에 다시 반영할 때 씁니다.

중요:

- 현재 구현에서 이 버튼은 새로운 Codex 호출을 강제하지 않습니다.
- 즉 `수동 재분석`은 `Analyze Now` 또는 `Retry Last`가 담당하고, `Refresh Knowledge`는 표시 갱신에 가깝습니다.

## 9. 현재 사용자 흐름의 현실적인 상태

2026-03-11 기준으로 이 UI는 빌드 가능하고 host 계약도 연결되어 있습니다.

아직 남아 있는 것은 아래입니다.

- 실제 gameplay high-value 화면과의 자동 advice 실증
- manual analyze와 auto advice의 end-to-end 검증
- run 종료 후 summary 표시 흐름
