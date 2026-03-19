# 로드맵

## Phase 1

외부 프로세스 기반 `AI 조언 어시스턴트`를 실제 플레이 중 참고 가능한 수준까지 완성한다.

포함:

- startup / deploy / trust 안정화
- runtime exporter와 high-value 화면 coverage
- 정적 지식 카탈로그
- host / Codex backend
- read-only advisor UI
- replay 기반 acceptance + 실제 live smoke

### Phase 1 10단계 마일스톤

#### M1. 깨끗한 실행 증명

- 목표: stale deploy, partial copy, ambiguous startup 같은 invalid test condition을 먼저 제거한다.
- 사용자 가치: "지금 보고 있는 결과가 현재 코드와 현재 배포물의 결과다"를 믿을 수 있다.
- acceptance: deploy identity, clean-boot evidence, startup artifact truthfulness가 반복 가능하게 유지된다.

#### M2. 모드 로드 진입 증명

- 목표: `sts2-mod-ai-companion`이 game mod loader chain에 실제로 진입했는지 current execution 기준으로 증명한다.
- 사용자 가치: "모드가 아예 안 켜진 판"과 "켜졌는데 다음 단계에서 죽은 판"을 구분할 수 있다.
- acceptance: loader entry 또는 그 이후 edge가 current execution artifact로 직접 관측된다.

#### M3. 런타임 부트스트랩 가동

- 목표: runtime exporter, harness bridge, observer 산출물이 current run에서 실제로 뜨게 만든다.
- 사용자 가치: 게임 상태를 읽을 최소한의 눈이 안정적으로 생긴다.
- acceptance: runtime exporter marker, observer snapshot, bridge 계열 산출물이 반복적으로 살아난다.

#### M4. Trusted Attempt 확보

- 목표: `manualCleanBootVerified=true`와 valid trust attempt를 안정적으로 만든다.
- 사용자 가치: 이후 gameplay 개선을 "믿을 수 있는 실행" 위에서 검증할 수 있다.
- acceptance: valid-trust attempt가 반복 가능하게 생성되고 startup/trust blocker가 gameplay 판정에 섞이지 않는다.

#### M5. 하네스 장기 실행 증거 닫기

- 목표: terminal -> restart -> next attempt first screen까지 long-run evidence chain을 안정화한다.
- 사용자 가치: 한 장면 성공이 아니라 실제 세션 단위 진행 안정성을 증명할 수 있다.
- acceptance: `restart-events.ndjson`, `attempt-index.ndjson`, `supervisor-state.json` 기준 증거 체인이 닫힌다.

#### M6. Replay/Parity 회귀 게이트 고정

- 목표: live와 replay-step/full-request-rebuild가 같은 결론을 내리도록 parity를 고정한다.
- 사용자 가치: 한번 고친 버그가 다시 열리는 것을 빠르게 차단할 수 있다.
- acceptance: parity-sensitive request에서 non-rebuild semantic == rebuild semantic이 반복적으로 유지된다.

#### M7. 비전투 진행 안정화

- 목표: main menu, map, reward, event, shop, rest 같은 고가치 비전투 화면의 흐름을 안정화한다.
- 사용자 가치: 플레이어가 run을 진행할 때 AI가 방의 흐름을 잃지 않는다.
- acceptance: valid-trust long-run artifact에서 mixed-state contamination과 foreground/background drift가 보수적으로 차단된다.

#### M8. 전투 안정화

- 목표: illegal action, stale selection, no-op loop, enemy-turn actuation 같은 전투 안전성 문제를 닫는다.
- 사용자 가치: AI가 전투에서 허공을 누르거나 말도 안 되는 반복을 하지 않는다.
- acceptance: valid-trust combat artifact에서 stale non-enemy loop, combat no-op loop, contract drift가 재발하지 않는다.

#### M9. 실질적 조언 품질 확보

- 목표: 카드/유물/이벤트 지식과 현재 전황을 묶어 사람이 참고할 만한 advice를 만든다.
- 사용자 가치: "그럴듯한 설명"이 아니라 실제 플레이에 도움 되는 추천을 받는다.
- acceptance: 대표 gameplay scene 세트에서 advice artifact가 일관되고 명백한 self-sabotage를 줄인다.

#### M10. Advisor 제품 표면 완성

- 목표: read-only advisor UI에서 live state, rationale, next-action advice를 안정적으로 보여준다.
- 사용자 가치: 사람이 실제 플레이 중 바로 참고 가능한 AI 어시스턴트를 쓸 수 있다.
- acceptance: 실제 플레이 중 end-to-end advice 표시가 안정적으로 동작하고 harness 없이도 advisor mode를 사용할 수 있다.

## Phase 1.5

정확도와 사용성 보강

- knowledge 정규화 품질 향상
- advice trigger / debounce 조정
- 더 넓은 화면 coverage
- run summary 품질 개선

## Phase 2

overlay 또는 더 나은 사용자 표면 검토

- 외부 WPF 앱이 충분히 안정화된 뒤에만 검토
- intrusive patching 없이 가능한 범위를 우선 탐색

## Phase 3

장기 확장 검토

- multiplayer teammate feasibility
- 추가 bridge 설계
- 더 깊은 game-data extraction

현재 저장소의 주력은 계속 `Phase 1`이다.
