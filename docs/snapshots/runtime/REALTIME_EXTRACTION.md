# 실시간 추출

> Status: Snapshot
> Source of truth: No
> This is a point-in-time runtime note. Current truth lives in [PROJECT_STATUS.md](../../current/PROJECT_STATUS.md) and [contracts/LIVE_EXPORT_SEMANTICS.md](../../contracts/LIVE_EXPORT_SEMANTICS.md).

## 현재 구현 중심

- 게임 내부의 read-only Harmony exporter
- append-only `events.ndjson`
- atomic `state.latest.json`
- atomic `state.latest.txt`
- atomic `session.json`
- startup identity / hook summary 로그

## live output root

- `%AppData%\SlayTheSpire2\steam\<steamAccountId>\modded\profile<index>\ai_companion\live`

## 현재 훅/추출 전략

- semantic hook는 확인된 후보만 whitelist 방식으로 유지
- broad generic 메서드는 기본 후보군에서 제외
- screen polling fallback을 유지해 early screen / main menu baseline을 확보
- snapshot diff로 카드/유물/포션/리소스 변화를 보강

## 현재 기준선

- main menu까지는 live export 4종 생성이 확인된 상태
- startup failure를 찾기 위한 smoke diagnostics가 강화된 상태
- reward/event/shop/rest/combat은 다음 live smoke에서 계속 실증이 필요한 상태

## observed knowledge와의 관계

live export는 단순 상태 표시뿐 아니라 observed knowledge의 재료이기도 합니다.

예:

- deck 카드
- 현재 보이는 선택지
- 최근 reward/event/shop 정보

이 정보는 `observed-merge` 단계에서 static knowledge catalog와 합쳐집니다.

## 현재 한계

- gameplay high-value 화면 coverage는 아직 부분적입니다.
- exact method names는 런타임 확인을 계속 거쳐야 합니다.
- host/WPF까지 포함한 완전한 end-to-end live 시연은 아직 남아 있습니다.
