# 프로젝트 현황

> 상태: 현재 사용 중
> 기준 문서: 예
> 갱신 시점: 현재 마일스톤, 현재 blocker, 대표 root, 또는 바로 다음 단계가 바뀔 때

읽기 쉬운 한국어 설명은 [PROJECT_STATUS_READER_KO.md](./PROJECT_STATUS_READER_KO.md)를 함께 본다.

## 날짜
- 2026-03-23

## 현재 마일스톤 위치

- 현재 진행 축: `M6 -> M7 -> M8 평가 -> M9 준비`
- 현재 마일스톤: `M6. Replay/Parity 회귀 게이트 고정`
- 다음 마일스톤: `M7. 비전투 진행 안정화`
- 장기 목표: 사람이 실제 플레이 중 참고할 수 있는 `읽기 전용 advisor` 완성

중요한 구분:

- 제품 진행 순서 관점에서는 `M5`를 여기서 닫는다.
- 다만 `ROADMAP`와 long-run lifecycle 문서가 정의하는 엄격한 `M5 done`은 아직 아니다.
- 남아 있는 엄격한 잔여 gap은 여전히:
  - `terminal`
  - `restart`
  - `next attempt first screen`
  evidence chain이다.

## 현재 우선순위

- 현재 핵심 경로는 더 이상 `startup / trust / first combat clearability / first card reward capture`가 아니다.
- 지금 우선순위는 단일 blocker fix가 아니라:
  - `M6` parity gate 재평가
  - `M7` non-combat mixed-state 잔여 inventory
  - `M8` combat 잔여 inventory
  - `M9` 진입 기준 정의
- 엄격한 `terminal -> restart -> next attempt` 자동화는 잔여 lifecycle 후속 작업으로 분리 추적한다.

## 진행 스냅샷

| Rail | Status | Notes |
|---|---|---|
| Startup / Trust | 기반으로 활용할 만큼 충분히 닫힘 | loader/runtime, bootstrap-first, quartet semantics는 현재 핵심 경로가 아니다 |
| Long-Run Core Continuity | 제품 관점에서 마감 가능 | ancient/map ownership, reward->map, shop->map, repeated combat/reward continuity, natural terminal boundary까지 live evidence 확보 |
| Replay / Evidence | 재가동 준비 완료 | long-run evidence가 충분히 길어졌고, 이제 parity gate를 다시 정식 audit할 시점이다 |
| Non-Combat Stability | 강하지만 정리 필요 | ancient/map은 닫혔고 reward/shop/map continuity도 좋다. generic event/modal/terminal aftermath는 잔여 위험 inventory가 필요하다 |
| Combat Stability | 개선됐지만 미종결 | first combat clearability는 더 이상 top blocker가 아니지만, elite defeat와 noop/confirm 잔여 문제는 아직 평가가 필요하다 |
| Advisor Product | 아직 활성 아님 | M9 진입 전에 representative scene set, acceptance band, honesty/usefulness pack을 먼저 정의해야 한다 |

## 현재 상태

### 현재 바로 활용 가능한 기반

- [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)에서 runtime exporter / harness bridge / fresh snapshot positive가 현재 실행 기준으로 회복됐다.
- [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)에서 bootstrap이 pre-attempt phase로 분리됐고, first authoritative attempt `0001`이 `trustStateAtStart:"valid"`로 시작한다.
- [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)에서 `restart-events` chronology와 projection들 해석 규칙이 정렬됐다.
- [verify-ancient-ownership-normalization-continue-20260323-2157](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-ancient-ownership-normalization-continue-20260323-2157)에서 ancient post-proceed mixed-state가 `foregroundOwner=map`, `choiceExtractorPath=map`, exported node click으로 정규화됐다.
- [verify-speedup-continue-20260323-2230](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-speedup-continue-20260323-2230)에서 sped-up harness가 valid root로 유지되며 session duration을 materially 줄였다.
- [verify-long-run-speedup-continue-20260323-2249](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2249)에서 120-step long run 동안 repeated combat/reward/map/shop continuity가 확보됐다.
- [verify-long-run-speedup-continue-20260323-2301](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301)에서 127-step long run이 natural `player-defeated` terminal boundary까지 이어졌다.

### 지금 실제로 답해야 할 질문

- 현재 핵심 질문은 “새 runtime blindspot 하나를 바로 구현할 것인가”가 아니다.
- 현재 핵심 질문은:
  1. `M6` parity gate를 정식으로 마감할 수 있는가
  2. `M7` non-combat 잔여 위험 중 무엇이 실제 활성 blocker인가
  3. `M8` combat stability를 `closed / partial / open` 중 어디로 판정할 것인가
  4. 그 위에서 `M9`를 어떤 scene set과 acceptance band로 시작할 것인가

### 현재 확정 진단

- `first combat clearability / first card reward capture`는 더 이상 현재 최상위 blocker가 아니다.
- 최신 valid roots는 이미:
  - repeated combat clear
  - repeated reward/card reward capture
  - reward -> map -> exported node
  - shop -> map handoff
  - elite combat 진입
  - natural terminal boundary
  까지 보여 준다.
- 현재 남은 gap은 엄격한 harness lifecycle semantics 쪽의:
  - terminal 이후 restart 자동화
  - restart 이후 next attempt first screen evidence
  다.
- 이 잔여 gap은 중요하지만, 현재 제품 진행 순서를 다시 `M5`에 묶을 이유는 줄었다.

## 먼저 읽어야 할 대표 root

- loader/runtime recovery root:
  - [verify-loader-direct-20260320-000700](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-loader-direct-20260320-000700)
- bootstrap-first sequencing root:
  - [verify-bootstrap-first-attempt-20260320-0044](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-bootstrap-first-attempt-20260320-0044)
- quartet/accounting contract root:
  - [verify-phase1-quartet-20260320-123409](/mnt/c/users/jidon/source/repos/sts2_mod_ai_companion/artifacts/gui-smoke/verify-phase1-quartet-20260320-123409)
- ancient ownership normalization root:
  - [verify-ancient-ownership-normalization-continue-20260323-2157](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-ancient-ownership-normalization-continue-20260323-2157)
- harness speed-up validation root:
  - [verify-speedup-continue-20260323-2230](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-speedup-continue-20260323-2230)
- long-run continuity root:
  - [verify-long-run-speedup-continue-20260323-2249](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2249)
- natural terminal boundary root:
  - [verify-long-run-speedup-continue-20260323-2301](/mnt/c/Users/jidon/source/repos/STS2_Mod_AI_Companion_wu_ancient_release/artifacts/gui-smoke/verify-long-run-speedup-continue-20260323-2301)

핵심 요약:

- startup/trust/accounting rail은 지금 당장 다시 열 필요가 없다.
- ancient/map mixed-state와 reward/map/shop/combat continuity는 실질적으로 안정화됐다.
- 현재 작업 축은 단일 blocker fix가 아니라 `M6~M8` evidence-based evaluation이다.
- 엄격한 `terminal -> restart -> next attempt first screen`은 잔여 lifecycle 후속 작업으로 추적한다.

## 안정된 것

- 배포 검증, 깨끗한 부팅 검증, 시도 번호/요약 집계 규칙은 이제 다음 작업을 얹어도 될 만큼 안정됐다. (`manualCleanBootVerified=true`, valid trust attempt, quartet/accounting semantics)
- 니오우 이후처럼 이벤트 잔상과 맵이 겹치는 장면에서는, 이제 이벤트 잔재를 다시 눌러 보지 않고 맵을 앞 화면으로 보고 맵 노드를 고른다. (`foregroundOwner=map`, `foregroundActionLane=map-node`)
- 하네스는 같은 유효 root 기준에서 예전보다 더 빠르게 돌고, 그 속도 개선이 장기 실행에서도 유지된다.
- 실제 장기 실행 root에서 보상 선택, 맵 복귀, 상점 방문, 다음 전투 진입이 여러 번 연속으로 이어지는 것이 확인됐다.
- 127-step root는 중간에 멈추거나 인공 step 제한에 걸린 것이 아니라, 실제 게임 플레이 도중 캐릭터가 패배하는 자연스러운 종료 지점까지 갔다.

## 부분적으로 안정된 것

- replay-step과 rebuild 결과가 항상 같은 의미를 내는지에 대한 검증 틀은 이미 좋지만, 최신 수정들이 들어간 뒤에도 여전히 같은지 마지막 확인이 더 필요하다.
- 비전투 화면 중 니오우/맵 겹침은 닫혔지만, 일반 이벤트, 모달, 오버레이, 패배/메뉴/재시작 이후 화면처럼 애매한 장면은 아직 한 번 더 목록화해서 점검해야 한다.
- 전투는 이제 “첫 전투도 못 넘긴다” 단계는 지났지만, 엘리트전 패배와 일부 `combat-noop`/`confirm-non-enemy` 흔적을 보면 “전투 품질까지 닫혔다”고 하기는 아직 이르다.
- 장기 실행 문서가 엄격하게 요구하는 `패배나 종료 -> 재시작 -> 다음 시도의 첫 화면` 증거 사슬은 아직 완전히 닫히지 않았다.

## 아직 안정되지 않은 것

- `M6`를 정말 닫아도 되는지에 대한 최종 parity 판정
- `M7`에서 아직 남은 비전투 문제 목록과, 그중 무엇부터 고칠지에 대한 작은 작업 단위
- `M8`에서 전투를 `닫힘 / 부분적으로 닫힘 / 아직 열림` 중 어디로 볼지에 대한 판정
- `M9`를 시작할 때 어떤 장면 묶음과 어떤 평가 기준을 쓸지에 대한 준비 문서
- `종료 -> 재시작 -> 다음 시도 첫 화면` 자동화 증거

## 현재 아키텍처 방향

- `Smoke Harness`는 계속 개발용 검증 도구로 유지한다.
- 하네스의 엄격한 lifecycle 규칙과 제품 마일스톤 진행 순서는 구분해서 읽는다.
- 화면이 겹치거나 모달이 섞여 애매한 장면이 나오면, screenshot 규칙을 더 덧대기 전에 먼저 `artifacts/knowledge/decompiled`와 runtime ownership state에서 게임이 스스로 어떤 상태라고 말하는지부터 찾는다.
- ancient/map ownership normalization과 속도 개선 버전 하네스는 지금 기준선으로 유지하고, regression 증거가 없으면 다시 뜯지 않는다.
- 다음 구현 작업은 큰 리팩터링보다 `M6/M7/M8 평가 -> 그 결과를 바탕으로 작은 다음 작업 선정` 순서로 고른다.

## 바로 다음 단계

1. `M6 Replay/Parity Closeout Audit`
   - 최신 ownership/speed-up 이후에도 replay-step과 rebuild가 같은 뜻을 유지하는지 정식으로 확인한다.
2. `M7 Non-Combat Stability Evaluation`
   - 일반 이벤트, 모달, 오버레이, 종료/메뉴/재시작 이후 화면을 “지금 바로 막는 문제”와 “나중에 묶어서 정리해도 되는 문제”로 나눈다.
3. `M8 Combat Stability Evaluation`
   - 여러 번 전투를 넘긴 root와 엘리트 패배 root를 함께 읽고, 전투 안전성과 전투 품질을 분리해서 판정한다.
4. `M9 Readiness Pack Definition`
   - 조언 품질을 평가할 대표 장면 묶음, 필요한 artifact 묶음, 정직성/유용성 기준을 현재 문서에 고정한다.

## 뒤로 미룬 작업

- 엄격한 `종료 -> 재시작 -> 다음 시도 첫 화면` 자동화 닫기
- 추천 엔진을 넓게 다시 짜는 큰 리팩터링
- startup diagnostic schema 확장
- ancient/map ownership normalization을 다시 일반화하는 큰 작업
