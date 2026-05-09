# WaterGrow Prototype

물 키우기 MVP 개발용 Unity 2D 모바일 캐주얼 머지 디펜스 프로젝트입니다.

## 현재 구현 범위

- 모바일 세로 화면 기준 `MainScene`
- 상단 HUD: Stage, Enemies, Base HP, Gold
- 중앙 전투 필드: 물 정령 placeholder, 불꽃 병사 placeholder, base/spawn marker
- 하단 6x4 머지 보드
- Lv.1 물 유닛 소환
- 같은 레벨 유닛 드래그 머지
- Lv.5 제한
- 최고 레벨, 먼저 생성된 순서 기준 대표 출전 유닛 계산
- 대표 유닛 기반 자동 단일 타겟 공격
- Stage 1-1 테스트 전투
  - 불꽃 병사 10마리
  - 1.5초 간격 스폰
  - baseHp 5
- 적 이동, 피격, HP 감소, 사망, base 도달 처리
- 처치 시 Gold 지급
- 스테이지 클리어 보상 지급
- Gold 기반 공격력 강화
- 공격력 강화 수치 저장 및 전투 피해량 반영
- PlayerPrefs 기반 기본 저장 구조
- JSON 기반 Unit/Enemy 데이터 로딩

## Unity에서 실행

1. Unity Hub에서 `C:\Users\k2417\watergrow` 폴더를 프로젝트로 엽니다.
2. Unity 버전은 `2022.3.62f3` 기준입니다.
3. `Assets/01_Scenes/MainScene.unity`를 엽니다.
4. Play 버튼을 누릅니다.
5. `SUMMON` 버튼으로 Lv.1 물 유닛을 생성합니다.
6. 같은 레벨 유닛 하나를 다른 같은 레벨 유닛 위로 드래그해서 머지합니다.
7. Gold가 모이면 `UPGRADE` 버튼으로 공격력을 강화합니다.

## 메인씬이 비어 보일 때

상단 메뉴에서 아래 항목을 실행하세요.

`WaterGrow > Rebuild Prototype Preview Scene`

이 메뉴는 다음 오브젝트를 다시 생성하고 `MainScene`에 저장합니다.

- `Main Camera`
- `EventSystem`
- `PrototypePreviewCanvas`
- `Systems`
- `BattleField`
- `MergeBoardPanel`
- `SummonButton`
- `RestartButton`
- `ResetButton`

## 테스트 방법

1. Play Mode를 시작합니다.
2. `SUMMON`을 눌러 Lv.1 유닛을 생성합니다.
3. 같은 레벨 유닛을 다른 같은 레벨 유닛 위로 드래그합니다.
4. 드롭한 칸에 다음 레벨 유닛이 생기는지 확인합니다.
5. 대표 출전 유닛 레벨이 갱신되는지 확인합니다.
6. 불꽃 병사가 오른쪽에서 왼쪽으로 이동하는지 확인합니다.
7. 대표 유닛이 있으면 자동 공격으로 적 HP가 줄어드는지 확인합니다.
8. 적을 모두 처치하면 클리어 메시지가 표시되는지 확인합니다.
9. 클리어 보상 Gold/Crystal이 지급되는지 확인합니다.
10. `UPGRADE`로 공격력 강화 후 적 처치 속도가 빨라지는지 확인합니다.
11. 적이 base에 도달하면 Base HP가 감소하는지 확인합니다.

## 현재 한계

- 아트, 사운드, 이펙트는 최종 리소스가 아니라 UI placeholder입니다.
- 드래그는 입력 제스처만 처리하며, 유닛 아이콘을 손가락 아래로 따라오게 하는 ghost visual은 아직 없습니다.
- 강화는 현재 공격력 강화 1종만 있습니다.
- Crystal은 저장되지만 아직 별도 사용처는 없습니다.
- 자동 공격은 단일 타겟만 지원합니다.
- Prototype 0.2는 일반 적 1종 테스트 스테이지만 포함합니다.
- Android APK 빌드 설정은 아직 MVP 단계에서 정리해야 합니다.
