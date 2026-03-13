# 작업 기록

이 문서는 저장소가 현재 상태까지 어떤 순서로 바뀌었는지 간단히 요약한 기록입니다. 상세 root cause와 시행착오는 `DETAILED_INVESTIGATION_LOG.md`가 담당하고, 이 문서는 큰 흐름만 적습니다.

## 1. 저장소 정리

- 로컬 멀티 에이전트 오케스트레이션 레이어를 제거했습니다.
- 저장소 중심을 `모드 구현 + exporter + packaging + snapshot/restore + live smoke + knowledge extraction`으로 되돌렸습니다.

## 2. runtime exporter 기초 구현

- live export 4종 구조를 고정했습니다.
  - `events.ndjson`
  - `state.latest.json`
  - `state.latest.txt`
  - `session.json`
- writer queue, state tracker, summary formatter, replay/self-test 기반을 만들었습니다.

## 3. Harmony startup failure 정리

- broad hook와 void postfix 문제를 줄여 startup failure를 제거했습니다.
- runtime identity / hook summary 로그를 넣고 main menu까지 live export가 생성되는 상태를 확인했습니다.

## 4. 정적 지식 파이프라인 1차

- `release-scan`, `assembly-scan`, `pck-inventory`, `observed-merge`, `catalog-build`를 만들었습니다.
- `catalog.latest.*`와 intermediate 산출물 구조를 고정했습니다.

## 5. Host / WPF 추가

- `Sts2AiCompanion.Host`와 `Sts2AiCompanion.Wpf`를 추가했습니다.
- live export polling, knowledge slice, prompt pack, Codex CLI 연동, advice artifact 저장, WPF 표시 경로를 만들었습니다.

## 6. 사람용 markdown / AI용 export 분리

- `artifacts/knowledge/markdown` 아래 사람용 리포트를 만들었습니다.
- `catalog.assistant.json`, `assistant/*.json` 등 AI 전용 산출물을 분리했습니다.

## 7. localization-scan 도입

- `SlayTheSpire2.pck`에서 localization key/value를 추출하는 `localization-scan`을 추가했습니다.
- 카드 seed와 localization을 병합해 title / description / selection prompt를 canonical entry에 붙였습니다.
- 이후 범위를 relic / potion / event / shop / reward / keyword까지 확장했습니다.

## 8. Spire Codex 방법론 반영

- broad substring match 대신 strict domain parser 방향으로 전환했습니다.
- `spire-codex`의 모델 클래스 기반 분류, localization merge, 이벤트 pages/options 구조 설계를 참고해 C# knowledge pipeline에 맞게 옮겼습니다.

## 9. decompile-scan + strict-domain-parse

- `ilspycmd` 기반 `decompile-scan`을 추가했습니다.
- 실제 모델 소스 기준 strict parser를 넣어 cards / relics / potions / events canonical을 정리했습니다.
- shops / rewards / keywords도 semantic canonical 수준으로 정리했습니다.

## 10. knowledge 정제

- 카드 canonical에서 `OPTION_*`, `*_BUTTON`, `*_POWER` 같은 잡음을 제거했습니다.
- CJK title fallback 문제를 줄이도록 title 우선순위를 조정했습니다.
- shops / rewards / keywords도 broad raw key 나열이 아니라 strict semantic entity 위주로 다시 만들었습니다.

## 11. gameplay extractor 보강

- overlay 화면이 `RoomType=Combat`에 즉시 덮이지 않도록 screen resolution과 state merge를 조정했습니다.
- reward / event / shop / rest의 choice 후보 수집을 강화했습니다.
- `LocString`, `MegaLabel`, `RichTextLabel` 등에서 실제 표시 문자열을 더 우선적으로 읽도록 보강했습니다.
- placeholder/UI 내부 이름 필터를 강화했습니다.

## 12. Codex / Host 보강

- `CodexCliClient`의 UTF-8 stdin/stdout/stderr 경로를 강제했습니다.
- prompt sanitize 경로를 추가해 invalid UTF-8 degraded를 줄였습니다.
- `analyze-live-once`를 추가했습니다.
- manual advice 직후 snapshot을 다시 publish하게 했습니다.
- JSON event stream에서 `thread.started`를 읽어 `sessionId`를 캡처하도록 했습니다.
- manual advice 기준 `current-run.json`과 `codex-session.json`에 `sessionId`가 반영되는 것까지 확인했습니다.

## 13. collector mode 도입

- 수집 런 전용 collector mode를 추가했습니다.
- collector mode에서는 다음을 남깁니다.
  - `raw-observations.ndjson`
  - `screen-transitions.ndjson`
  - `choice-candidates.ndjson`
  - `choice-decisions.ndjson`
  - `semantic-snapshots/*`
  - `collector-summary.json`
- collector summary에는 request latency, duplicate trigger, screen overwrite, state regression, knowledge usage, runtime fatal errors, apphang suspicion을 넣었습니다.

## 14. advice loop와 UI 의미 정리

- automatic advice trigger를 high-priority 화면 중심으로 제한하고 coalesce 정책을 넣었습니다.
- `Analyze Now`와 `Retry Last`를 분리했습니다.
- WPF에 모델 선택, 추론 선택, 분석중 상태, 경과 시간, collector 진단 패널을 추가했습니다.
- 출력 텍스트를 복사 가능한 읽기 전용 텍스트 박스로 유지했습니다.

## 15. 현재 기준 결론

- manual advice: 성공
- session capture / run-scoped session reuse 경로: 코드상 구현 완료
- collector diagnostics: 코드상 구현 완료
- runtime stability: 강화 중
- gameplay auto advice: 미실증
- gameplay session reuse: 최신 배포본 기준 실증 필요

즉 현재 단계의 병목은 더 많은 정적 지식이 아니라, `실제 gameplay high-value 화면에서 auto advice와 session reuse를 끝까지 연결하는 것`입니다.

## 16. dual-mode 구조 재정렬

- 저장소를 `shared foundation + advisor mode + harness mode`로 재해석하는 상위 구조 문서를 추가했습니다.
- `Sts2AiCompanion.Foundation`, `Sts2AiCompanion.Advisor`, `Sts2AiCompanion.Harness`, `Sts2ModAiCompanion.HarnessBridge` 프로젝트 골격을 추가했습니다.
- WPF는 legacy host를 직접 보는 대신 advisor façade를 통해 붙도록 구조를 정리하기 시작했습니다.
- harness는 아직 스켈레톤 단계이며, 다음 단계에서 test-only action executor와 첫 unattended scenario를 닫는 것이 목표입니다.

## 17. harness contamination 경계 재정의와 external control 파일 계약 추가

- 최근 검은 화면/자동 진행 관측은 정상 런 기준이 아니라 stale harness queue와 auto-enabled test path가 섞인 contamination 상태일 가능성이 높다는 판단을 고정했습니다.
- `Manual Clean Boot`를 first reward보다 앞선 최우선 게이트로 재설정했습니다.
- bridge를 `dormant by default`로 보고, arm/session token 없이는 queue를 소비하지 않는 방향으로 제어 경계를 다시 잡았습니다.
- `arm.json`과 `inventory.latest.json`을 포함한 harness control 파일 계약을 추가하고, external command 기본 단위를 semantic label에서 `nodeId`로 옮기기 시작했습니다.
- CLI에 `arm-harness-session`, `disarm-harness-session`, `inspect-harness-control`, `dispatch-harness-node` 경로를 추가해 외부 지휘면을 tool 쪽으로 분리했습니다.
- 이번 단계에서 중요한 판단은 “DLL은 손발과 관측만, 판단은 외부 세션”이며, UI-bound dispatch만 허용한다는 점입니다.
- 아직 최신 경계 구현에 대한 Manual Clean Boot 재검증은 남아 있습니다.

## 18. 2026-03-13 - Manual Clean Boot gate passed
- 최소 clean-boot bridge(dormant/status/trace/action-ignore)를 수동 컴파일 후 배포했다.
- 초기 재부팅에서는 Sts2ModKit.Core.dll ABI mismatch로 HarnessArmSession 타입 로드 실패를 확인했다.
- Core/Foundation/HarnessBridge DLL을 함께 맞춘 뒤 다시 Steam URI 부팅했고, stale __start__, __ironclad__, __confirm__ 액션이 모두 ction-ignored로만 남는 것을 확인했다.
- status.json은 mode=dormant, message=bridge-dormant-no-arm으로 기록됐고 live state는 main-menu에 머물렀다.

## 19. 2026-03-13 - Observer typing improved, stabilization still partial
- foundation과 harness bridge가 같은 scene normalizer를 쓰도록 공용 helper를 도입했다.
- harness inventory는 main menu에서 `profile-slot`, `continue-run`, `menu-action` 같은 semantic node kind를 내보내기 시작했다.
- self-test와 Steam URI clean boot에서 dormant 유지, stale action ignore 유지, semantic inventory publish를 다시 확인했다.
- 다만 transient suppress 규칙은 아직 충분히 강하지 않다. `bootstrap`, `feedback-overlay`, `startup` 같은 초기 scene이 두 번 연속 관측되면 여전히 publish될 수 있었다.
- 따라서 이번 작업은 observer fidelity 개선까지는 성공했지만, `dispatch_node` 재개 전 acceptance 수준의 stabilization은 아직 미완이다.


## 20. 2026-03-13 - Decompiled source-first observer strategy documented
- observer 개선 논의를 정리하면서 polling 자체를 문제로 보는 것은 정확하지 않다는 점을 명시했다.
- 현재 observer는 `event + polling mixed observer`로 보고, polling은 continuous state / reconciliation / watchdog 역할을 계속 맡는다고 문서화했다.
- 대신 scene transition, screen ready, lifecycle boundary 같은 권위 있는 경계 판단은 먼저 decompiled source에서 후보 메서드를 찾고, 그 다음 runtime hook/event로 검증해야 한다는 원칙을 고정했다.
- 이 판단을 `AI_HANDOFF_PROMPT_KO`, `AGENTS.md`, `HARNESS_MODE`, `PENDING_HOOKS_AND_RISKS`, `PROJECT_STATUS`, `DOCUMENT_MAP`, `STS2_HARNESS_REVIEW`에 동시에 반영해 다음 AI가 같은 기준으로 출발하도록 정리했다.
## 21. 2026-03-13 - Main menu to combat observer authority chain implemented
- Added decompiled-backed runtime hook vocabulary for `main-menu`, `singleplayer-button-pressed`, `singleplayer-submenu`, `open-character-select`, `character-select`, `character-selected`, `map`, and `map-point-selected`.
- Replaced `map-node-entered` as the canonical map transition with `map`, while keeping the legacy alias for consumer compatibility.
- Updated live export consumers and self-test coverage so menu-to-combat authority is preserved even when `runtime-poll` observations interleave with hook-backed events.
- Left actuation closed. Runtime smoke acceptance for existing-run and zero-run menu branches still remains as the next gate.
