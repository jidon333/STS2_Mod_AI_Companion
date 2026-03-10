# 인게임 UI injection 예제

이 scaffold의 기본 template mod는 인게임 UI를 포함하지 않습니다.

대신 `templates\mod-with-ui`에 다음 패턴의 예제를 둡니다.

- `NModInfoContainer.Fill(Mod mod)`에 Harmony `Postfix`
- 현재 선택된 모드가 대상 모드인지 검사
- 우측 상세 패널에 `VBoxContainer`, `HBoxContainer`, `Button`, `Label`을 reflection으로 생성
- 값을 JSON config에 저장
- 런타임은 config write time 변화를 보고 다시 읽음

권장 이유:

- 기본 mod template는 가볍게 유지
- UI가 필요한 모드만 예제를 가져다 쓰면 됨
- 모딩 화면 구조가 바뀌면 UI 예제 쪽만 수정하면 됨

실전 팁:

- 처음에는 `Enabled` 같은 toggle 하나로 시작
- 값 저장 성공 여부는 별도 runtime log로 확인
- layout 문제는 텍스트와 panel position을 먼저 확인
- "저장은 되는데 숫자가 안 바뀌는" 경우는 UI 갱신 경로를 의심
